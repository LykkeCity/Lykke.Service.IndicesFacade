using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.HttpClientGenerator.Infrastructure;
using Lykke.Service.CryptoIndex.Client.Api;
using Lykke.Service.CryptoIndex.Client.Models;
using Lykke.Service.IndicesFacade.Client;
using Lykke.Service.IndicesFacade.Client.Models;
using Lykke.Service.IndicesFacade.Settings;

namespace Lykke.Service.IndicesFacade.Services
{
    public class IndicesFacadeService : IIndicesFacadeApi, IStartable, IStopable
    {
        private readonly CryptoIndexServiceClientInstancesSettings _cryptoIndexServiceClientInstancesSettings;
        private readonly ConcurrentDictionary<string, IPublicApi> _clients = new ConcurrentDictionary<string, IPublicApi>();
        private readonly ConcurrentDictionary<string, Index> _indicesCache = new ConcurrentDictionary<string, Index>();
        private readonly ConcurrentDictionary<string, IList<HistoryElement>> _historyCache = new ConcurrentDictionary<string, IList<HistoryElement>>();

        private readonly TimerTrigger _trigger;

        private readonly ILog _log;

        public IndicesFacadeService(CryptoIndexServiceClientInstancesSettings cryptoIndexServiceClientInstancesSettings, ILogFactory logFactory)
        {
            _cryptoIndexServiceClientInstancesSettings = cryptoIndexServiceClientInstancesSettings;

            foreach (var instance in _cryptoIndexServiceClientInstancesSettings.Instances)
            {
                var client = CreateApiClient(instance.ServiceUrl);
                _clients[instance.AssetId] = client;
            }

            _trigger = new TimerTrigger(nameof(IndicesFacadeService), TimeSpan.FromSeconds(15), logFactory, Execute);

            _log = logFactory.CreateLog(this);
        }

        public async Task Execute(ITimerTrigger timer, TimerTriggeredHandlerArgs args, CancellationToken cancellationToken)
        {
            try
            {
                await Execute();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        private async Task Execute()
        {
            foreach (var assetId in _clients.Keys.ToList())
            {
                var publicApi = _clients[assetId];

                var indexHistory = await publicApi.GetLastAsync();
                var keyNumbers = await publicApi.GetKeyNumbers();

                var history24H = await publicApi.GetIndexHistory(TimeInterval.Hour24);
                var history5D = await publicApi.GetIndexHistory(TimeInterval.Day5);
                var history30D = await publicApi.GetIndexHistory(TimeInterval.Day30);

                var index = Map(assetId, indexHistory, keyNumbers);

                _indicesCache[assetId] = index;

                var key24H = GetHistoryCacheKey(assetId, TimeInterval.Hour24);
                _historyCache[key24H] = Map(history24H);

                var key5D = GetHistoryCacheKey(assetId, TimeInterval.Day5);
                _historyCache[key5D] = Map(history5D);

                var key30D = GetHistoryCacheKey(assetId, TimeInterval.Day30);
                _historyCache[key30D] = Map(history30D);
            }
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

        public async Task<IList<HistoryElement>> GetHistoryAsync(string assetId, TimeInterval timeInterval)
        {
            var key = GetHistoryCacheKey(assetId, timeInterval);

            var result = _historyCache[key];

            return result;
        }

        public bool IsPresent(string assetId)
        {
            return _clients.ContainsKey(assetId);
        }

        private string GetHistoryCacheKey(string assetId, TimeInterval interval)
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

        #region IStartable, IStopable

        public void Start()
        {
            _trigger.Start();
        }

        public void Stop()
        {
            _trigger.Stop();
        }

        public void Dispose()
        {
            _trigger?.Dispose();
        }

        #endregion
    }
}
