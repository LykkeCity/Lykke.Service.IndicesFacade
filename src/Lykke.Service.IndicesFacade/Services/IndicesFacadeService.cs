using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.HttpClientGenerator.Infrastructure;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.Assets.Client.ReadModels;
using Lykke.Service.CryptoIndex.Client.Api;
using Lykke.Service.CryptoIndex.Client.Models;
using Lykke.Service.CryptoIndex.Contract;
using Lykke.Service.IndicesFacade.Client;
using Lykke.Service.IndicesFacade.Contract;
using Lykke.Service.IndicesFacade.RabbitMq.Publishers;
using Lykke.Service.IndicesFacade.Settings;
using Microsoft.Rest;
using TimeInterval = Lykke.Service.CryptoIndex.Client.Models.TimeInterval;

namespace Lykke.Service.IndicesFacade.Services
{
    public class IndicesFacadeService : IIndicesFacadeApi
    {
        private readonly CryptoIndexServiceClientInstancesSettings _cryptoIndexServiceClientInstancesSettings;
        private readonly ConcurrentDictionary<string, IPublicApi> _assetIdsClients = new ConcurrentDictionary<string, IPublicApi>();
        private readonly ConcurrentDictionary<string, Index> _assetIdsIndicesCache = new ConcurrentDictionary<string, Index>();
        private readonly ConcurrentDictionary<string, IList<HistoryElement>> _assetIdIntervalsHistoriesCache = new ConcurrentDictionary<string, IList<HistoryElement>>();
        private readonly IndicesUpdatesPublisher _indicesUpdatesPublisher;
        private readonly IndicesHistoryUpdatesPublisher _indicesHistoryUpdatesPublisher;
        private readonly IAssetsReadModelRepository _assetsReadModelRepository;

        private readonly ConcurrentDictionary<string, string> _assetIdsDisplayIds
            = new ConcurrentDictionary<string, string>();

        private readonly ILog _log;

        public IndicesFacadeService(CryptoIndexServiceClientInstancesSettings cryptoIndexServiceClientInstancesSettings,
            IndicesUpdatesPublisher indicesUpdatesPublisher,
            IndicesHistoryUpdatesPublisher indicesHistoryUpdatesPublisher,
            IAssetsReadModelRepository assetsReadModelRepository,
            ILogFactory logFactory)
        {
            _cryptoIndexServiceClientInstancesSettings = cryptoIndexServiceClientInstancesSettings;

            foreach (var instance in _cryptoIndexServiceClientInstancesSettings.Instances)
            {
                var client = CreateApiClient(instance.ServiceUrl);
                _assetIdsClients[instance.AssetId] = client;
            }

            _indicesUpdatesPublisher = indicesUpdatesPublisher;
            _indicesHistoryUpdatesPublisher = indicesHistoryUpdatesPublisher;

            _assetsReadModelRepository = assetsReadModelRepository;
            InitializeAssetsDisplayIds();

            _log = logFactory.CreateLog(this);
        }

        public async Task<IList<Index>> GetAllAsync()
        {
            var result = _assetIdsIndicesCache.Values.OrderBy(x => x.AssetId).ToList();

            return result;
        }

        public async Task<Index> GetAsync(string assetId)
        {
            var result = _assetIdsIndicesCache[assetId];

            return result;
        }

        public async Task<IList<HistoryElement>> GetHistoryAsync(string assetId, Contract.TimeInterval timeInterval)
        {
            var key = GetHistoryCacheKey(assetId, timeInterval);

            var result = _assetIdIntervalsHistoriesCache[key];

            return result;
        }

        public bool IsPresent(string assetId)
        {
            return _assetIdsClients.ContainsKey(assetId);
        }

        public async Task Handle(IndexTickPrice indexTickPrice)
        {
            var assetId = MapIndexNameToAssetId(indexTickPrice.AssetPair);

            try
            {
                await UpdateIndexCache(assetId);
            }
            catch (Exception ex)
            {
                _log.Warning("", ex);
            }
        }

        private void InitializeAssetsDisplayIds()
        {
            foreach (var assetId in _assetIdsClients.Keys.ToList())
            {
                var asset = _assetsReadModelRepository.TryGet(assetId);
                if (asset != null)
                    _assetIdsDisplayIds[assetId] = asset.DisplayId;
                else
                {
                    _assetIdsDisplayIds[assetId] = _cryptoIndexServiceClientInstancesSettings.Instances
                        .Single(x => x.AssetId == assetId).IndexName;
                    _log.Warning($"Did not find assetId '{assetId}' in the Assets Service. Assigned 'IndexName' from the settings.");
                }
            }
        }

        private async Task UpdateIndexCache(string assetId)
        {
            var publicApi = _assetIdsClients[assetId];

            PublicIndexHistory indexHistory;
            try
            {
                indexHistory = await publicApi.GetLastAsync();
            }
            catch (Exception ex)
            {
                _log.Warning($"Something went wrong while {nameof(publicApi.GetLastAsync)} element from '{MapAssetIdToIndexName(assetId)}': ", ex);

                return;
            }

            KeyNumbers keyNumbers;
            try
            {
                keyNumbers = await publicApi.GetKeyNumbers();
            }
            catch (Exception ex)
            {
                _log.Warning($"Something went wrong while requesting {nameof(publicApi.GetKeyNumbers)} from '{MapAssetIdToIndexName(assetId)}': ", ex);

                return;
            }

            IDictionary<DateTime, decimal> updatedHistory24H;
            IDictionary<DateTime, decimal> updatedHistory5D;
            IDictionary<DateTime, decimal> updatedHistory30D;
            try
            {
                updatedHistory24H = await publicApi.GetIndexHistory(TimeInterval.Hour24);
                updatedHistory5D = await publicApi.GetIndexHistory(TimeInterval.Day5);
                updatedHistory30D = await publicApi.GetIndexHistory(TimeInterval.Day30);
            }
            catch (Exception ex)
            {
                _log.Warning($"Something went wrong while requesting history from '{MapAssetIdToIndexName(assetId)}': ", ex);

                return;
            }

            var index = Map(assetId, indexHistory, keyNumbers);

            _assetIdsIndicesCache[assetId] = index;

            // Publish new index
            _indicesUpdatesPublisher.Publish(index);

            // Update history for every interval and publish if needed
            UpdateHistoryCacheAndPublish(assetId, Contract.TimeInterval.Hour24, updatedHistory24H);
            UpdateHistoryCacheAndPublish(assetId, Contract.TimeInterval.Day5, updatedHistory5D);
            UpdateHistoryCacheAndPublish(assetId, Contract.TimeInterval.Day30, updatedHistory30D);
        }

        private void UpdateHistoryCacheAndPublish(string assetId, Contract.TimeInterval timeInterval, IDictionary<DateTime, decimal> updatedHistory)
        {
            var cacheKey = GetHistoryCacheKey(assetId, timeInterval);

            if (!_assetIdIntervalsHistoriesCache.ContainsKey(cacheKey))
                _assetIdIntervalsHistoriesCache[cacheKey] = Map(updatedHistory);

            var previousHistory = _assetIdIntervalsHistoriesCache[cacheKey];
            var lastTimestampInUpdatedHistory = updatedHistory.Keys.Last();
            var updateExistsInPreviousHistory = previousHistory.Any(x => x.Timestamp == lastTimestampInUpdatedHistory);
            if (!updateExistsInPreviousHistory)
            {
                _assetIdIntervalsHistoriesCache[cacheKey] = Map(updatedHistory);

                // Publish new index history element
                var newHistoryElement = Create(assetId, timeInterval, updatedHistory.Keys.Last(), updatedHistory.Values.Last());
                _indicesHistoryUpdatesPublisher.Publish(newHistoryElement);
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
            return new Index
            {
                AssetId = assetId,
                Name = _assetIdsDisplayIds[assetId],
                Composition = Map(indexHistory.Weights, indexHistory.MiddlePrices),
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

        private IList<Constituent> Map(IDictionary<string, decimal> weights, IDictionary<string, decimal> prices)
        {
            if (weights.Count != prices.Count)
                throw new ValidationException($"Count of weights ({weights.Count}) and count of prices ({prices.Count}) are not equals.");

            var result = new List<Constituent>();

            foreach (var assetWeight in weights)
            {
                var asset = assetWeight.Key;
                var weight = assetWeight.Value;
                var price = prices[asset];

                var constituent = new Constituent
                {
                    AssetId = asset,
                    Weight = weight,
                    Price = price
                };

                result.Add(constituent);
            }

            return result;
        }

        private IList<HistoryElement> Map(IDictionary<DateTime, decimal> history)
        {
            return history.Select(x => new HistoryElement {Timestamp = x.Key, Value = x.Value}).ToList();
        }

        private string MapIndexNameToAssetId(string indexName)
        {
            var indexSettings = _cryptoIndexServiceClientInstancesSettings
                .Instances
                .SingleOrDefault(x => x.IndexName == indexName);

            if (indexSettings == null)
                throw new InvalidOperationException($"Can't find index assetId by index name '{indexName}'. " +
                                                    $"Compare KeyValue 'CryptoIndexService-Instances' in settings to 'Source' fields in CryptoIndex services settings.");

            return indexSettings.AssetId;
        }

        private string MapAssetIdToIndexName(string assetId)
        {
            var indexSettings = _cryptoIndexServiceClientInstancesSettings
                .Instances
                .SingleOrDefault(x => x.AssetId == assetId);

            if (indexSettings == null)
                throw new InvalidOperationException($"Can't find index index name by assetId '{assetId}'. " +
                                                    $"Compare KeyValue 'CryptoIndexService-Instances' in settings to 'Source' fields in CryptoIndex services settings.");

            return indexSettings.IndexName;
        }

        private HistoryElementUpdate Create(string assetId, Contract.TimeInterval timeInterval, DateTime timestamp, decimal value)
        {
            var result = new HistoryElementUpdate
            {
                AssetId = assetId,
                HistoryElement = new HistoryElement { Timestamp = timestamp, Value = value },
                TimeInterval = timeInterval
            };

            return result;
        }
    }
}
