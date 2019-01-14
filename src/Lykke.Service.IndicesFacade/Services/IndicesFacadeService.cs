using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.HttpClientGenerator.Infrastructure;
using Lykke.Service.CryptoIndex.Client.Api;
using Lykke.Service.CryptoIndex.Client.Models;
using Lykke.Service.CryptoIndex.Contract;
using Lykke.Service.IndicesFacade.Client;
using Lykke.Service.IndicesFacade.Contract;
using Lykke.Service.IndicesFacade.RabbitMq.Publishers;
using Lykke.Service.IndicesFacade.Settings;
using TimeInterval = Lykke.Service.CryptoIndex.Client.Models.TimeInterval;

namespace Lykke.Service.IndicesFacade.Services
{
    internal class IndicesFacadeService : IIndicesFacadeApi
    {
        private readonly CryptoIndexServiceClientInstancesSettings _cryptoIndexServiceClientInstancesSettings;
        private readonly ConcurrentDictionary<string, IPublicApi> _clients = new ConcurrentDictionary<string, IPublicApi>();
        private readonly ConcurrentDictionary<string, Index> _indicesCache = new ConcurrentDictionary<string, Index>();
        private readonly ConcurrentDictionary<string, IList<HistoryElement>> _historyCache = new ConcurrentDictionary<string, IList<HistoryElement>>();
        private readonly IndicesUpdatesPublisher _indicesUpdatesPublisher;
        private readonly IndicesHistoryUpdatesPublisher _indicesHistoryUpdatesPublisher;

        private readonly ILog _log;

        public IndicesFacadeService(CryptoIndexServiceClientInstancesSettings cryptoIndexServiceClientInstancesSettings,
            IndicesUpdatesPublisher indicesUpdatesPublisher,
            IndicesHistoryUpdatesPublisher indicesHistoryUpdatesPublisher,
            ILogFactory logFactory)
        {
            _cryptoIndexServiceClientInstancesSettings = cryptoIndexServiceClientInstancesSettings;

            foreach (var instance in _cryptoIndexServiceClientInstancesSettings.Instances)
            {
                var client = CreateApiClient(instance.ServiceUrl);
                _clients[instance.AssetId] = client;
            }

            _indicesUpdatesPublisher = indicesUpdatesPublisher;
            _indicesHistoryUpdatesPublisher = indicesHistoryUpdatesPublisher;

            _log = logFactory.CreateLog(this);
        }

        public async Task<IList<Index>> GetAllAsync()
        {
            var result = _indicesCache.Values.OrderBy(x => x.AssetId).ToList();

            return result;
        }

        public async Task<Index> GetAsync(string assetId)
        {
            var result = _indicesCache[assetId];

            return result;
        }

        public async Task<IList<HistoryElement>> GetHistoryAsync(string assetId, Contract.TimeInterval timeInterval)
        {
            var key = GetHistoryCacheKey(assetId, timeInterval);

            var result = _historyCache[key];

            return result;
        }

        public bool IsPresent(string assetId)
        {
            return _clients.ContainsKey(assetId);
        }

        public async Task Handle(IndexTickPrice indexTickPrice)
        {
            var assetId = MapIndexNameToAssetId(indexTickPrice.AssetPair);

            await UpdateIndexCache(assetId);
        }

        private async Task UpdateIndexCache(string assetId)
        {
            var publicApi = _clients[assetId];

            var indexHistory = await publicApi.GetLastAsync();
            var keyNumbers = await publicApi.GetKeyNumbers();

            var updatedHistory24H = await publicApi.GetIndexHistory(TimeInterval.Hour24);
            var updatedHistory5D = await publicApi.GetIndexHistory(TimeInterval.Day5);
            var updatedHistory30D = await publicApi.GetIndexHistory(TimeInterval.Day30);

            var index = Map(assetId, indexHistory, keyNumbers);

            _indicesCache[assetId] = index;

            // Publish new index
            _indicesUpdatesPublisher.Publish(index);

            // 24 hours
            var key24H = GetHistoryCacheKey(assetId, Contract.TimeInterval.Hour24);
            var previousHistory24H = _historyCache.ContainsKey(key24H) ? _historyCache[key24H] : new List<HistoryElement>();
            var lastTimestampInUpdatedHistory24H = updatedHistory24H.Keys.Last();
            var updateExistsInPreviousHistory24H = previousHistory24H.Any(x => x.Timestamp == lastTimestampInUpdatedHistory24H);
            if (!updateExistsInPreviousHistory24H)
            {
                _historyCache[key24H] = Map(updatedHistory24H);

                // Publish new 24H index history element
                var newHistory24H = Create(assetId, Contract.TimeInterval.Hour24, updatedHistory24H.Keys.Last(), updatedHistory24H.Values.Last());
                _indicesHistoryUpdatesPublisher.Publish(newHistory24H);
            }

            // 5 days
            var key5D = GetHistoryCacheKey(assetId, Contract.TimeInterval.Day5);
            var previousHistory5D = _historyCache.ContainsKey(key5D) ? _historyCache[key5D] : new List<HistoryElement>();
            var lastTimestampInUpdatedHistory5D = updatedHistory5D.Keys.Last();
            var updateExistsInPreviousHistory5D = previousHistory5D.Any(x => x.Timestamp == lastTimestampInUpdatedHistory5D);
            if (!updateExistsInPreviousHistory5D)
            {
                _historyCache[key5D] = Map(updatedHistory5D);

                // Publish new 5D index history element
                var newHistory5D = Create(assetId, Contract.TimeInterval.Day5, updatedHistory5D.Keys.Last(), updatedHistory5D.Values.Last());
                _indicesHistoryUpdatesPublisher.Publish(newHistory5D);
            }

            // 30 days
            var key30D = GetHistoryCacheKey(assetId, Contract.TimeInterval.Day30);
            var previousHistory30D = _historyCache.ContainsKey(key30D) ? _historyCache[key30D] : new List<HistoryElement>();
            var lastTimestampInUpdatedHistory30D = updatedHistory30D.Keys.Last();
            var updateExistsInPreviousHistory30D = previousHistory30D.Any(x => x.Timestamp == lastTimestampInUpdatedHistory30D);
            if (!updateExistsInPreviousHistory30D)
            {
                _historyCache[key30D] = Map(updatedHistory30D);

                // Publish new 30D index history element
                var newHistory30D = Create(assetId, Contract.TimeInterval.Day30, updatedHistory30D.Keys.Last(), updatedHistory30D.Values.Last());
                _indicesHistoryUpdatesPublisher.Publish(newHistory30D);
            }
        }

        private string GetHistoryCacheKey(string assetId, Contract.TimeInterval interval)
        {
            return $"{assetId}-{interval}";
        }

        private IPublicApi CreateApiClient(string url)
        {
            var generator = HttpClientGenerator.HttpClientGenerator.BuildForUrl(url)
                .WithAdditionalCallsWrapper(new ExceptionHandlerCallsWrapper())
                .WithoutRetries()
                .WithoutCaching()
                .Create();

            var client = generator.Generate<IPublicApi>();

            return client;
        }

        private Index Map(string assetId, PublicIndexHistory indexHistory, KeyNumbers keyNumbers)
        {
            var name = _cryptoIndexServiceClientInstancesSettings.Instances.Single(x => x.AssetId == assetId).DisplayName;

            return new Index
            {
                AssetId = assetId,
                Name = name,
                Composition = Map(indexHistory.Weights),
                Value = indexHistory.Value,
                Timestamp = indexHistory.Time,
                // key numbers
                Return24H = keyNumbers.Return24h,
                Return5D = keyNumbers.Return5d,
                Return30D = keyNumbers.Return30d,
                Max24H = keyNumbers.Max24h,
                Min24H = keyNumbers.Min24h,
                Volatility24H = keyNumbers.Volatility24h,
                Volatility30D = keyNumbers.Volatility30d
            };
        }

        private IList<Constituent> Map(IDictionary<string, decimal> weights)
        {
            return weights.Select(x => new Constituent
                {
                    AssetId = x.Key,
                    Weight = x.Value
                })
                .ToList();
        }

        private IList<HistoryElement> Map(IDictionary<DateTime, decimal> history)
        {
            return history.Select(x => new HistoryElement {Timestamp = x.Key, Value = x.Value}).ToList();
        }

        private string MapIndexNameToAssetId(string indexName)
        {
            var indexSettings = _cryptoIndexServiceClientInstancesSettings
                .Instances
                .SingleOrDefault(x => x.DisplayName == indexName);

            if (indexSettings == null)
                throw new InvalidOperationException($"Can't find index assetId by index name '{indexName}'. " +
                                                    $"Compare KeyValue 'CryptoIndexService-Instances' in settings to 'Source' fields in CryptoIndex services settings.");

            return indexSettings.AssetId;
        }

        private HistoryElementUpdate Create(string assetId, Contract.TimeInterval timeInterval, DateTime timestamp, decimal value)
        {
            var result = new HistoryElementUpdate
            {
                AssetId = assetId,
                HistoryElement = new HistoryElement { Timestamp = timestamp, Value = value },
                TimeInterval = Contract.TimeInterval.Day5
            };

            return result;
        }
    }
}
