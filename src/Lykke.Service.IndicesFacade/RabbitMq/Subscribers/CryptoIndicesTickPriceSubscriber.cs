using System;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.CryptoIndex.Contract;
using Lykke.Service.IndicesFacade.Services;
using Lykke.Service.IndicesFacade.Settings;

namespace Lykke.Service.IndicesFacade.RabbitMq.Subscribers
{
    internal sealed class CryptoIndicesTickPriceSubscriber : IStartable, IStopable
    {
        private readonly RabbitMqSettings _settings;
        private RabbitMqSubscriber<IndexTickPrice> _subscriber;

        private readonly IndicesFacadeService _indicesFacadeService;
        private readonly ILogFactory _logFactory;
        private readonly ILog _log;

        public CryptoIndicesTickPriceSubscriber(
            RabbitMqSettings settings,
            IndicesFacadeService indicesFacadeService,
            ILogFactory logFactory)
        {
            _settings = settings;

            _indicesFacadeService = indicesFacadeService;
            _logFactory = logFactory;
            _log = logFactory.CreateLog(this);
        }

        public void Start()
        {
            var settings = new RabbitMqSubscriptionSettings
            {
                ConnectionString = _settings.ConnectionString,
                ExchangeName = _settings.SubscribeCryptoIndicesTickPricesExchange,
                QueueName = _settings.SubscribeCryptoIndicesTickPricesExchange + "IndicesFacade",
                IsDurable = false
            };

            _subscriber = new RabbitMqSubscriber<IndexTickPrice>(_logFactory, settings,
                    new ResilientErrorHandlingStrategy(_logFactory, settings, TimeSpan.FromSeconds(10)))
                .SetMessageDeserializer(new JsonMessageDeserializer<IndexTickPrice>())
                .Subscribe(ProcessMessageAsync)
                .CreateDefaultBinding()
                .Start();
        }

        public void Stop()
        {
            _subscriber?.Stop();
        }

        public void Dispose()
        {
            _subscriber?.Dispose();
        }

        private async Task ProcessMessageAsync(IndexTickPrice tickPrice)
        {
            _log.Info($"Received '{tickPrice.Source}' tick price.");

            await _indicesFacadeService.Handle(tickPrice);
        }
    }
}
