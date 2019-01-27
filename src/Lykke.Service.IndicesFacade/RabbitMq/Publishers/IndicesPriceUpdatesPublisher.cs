using Autofac;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.IndicesFacade.Contract;
using Lykke.Service.IndicesFacade.Settings;

namespace Lykke.Service.IndicesFacade.RabbitMq.Publishers
{
    public sealed class IndicesPriceUpdatesPublisher : IStartable, IStopable
    {
        private readonly ILogFactory _logFactory;
        private readonly RabbitMqSettings _settings;
        private RabbitMqPublisher<AssetsInfoUpdate> _publisher;
        private readonly ILog _log;

        public IndicesPriceUpdatesPublisher(RabbitMqSettings settings, ILogFactory logFactory)
        {
            _logFactory = logFactory;
            _settings = settings;
            _log = logFactory.CreateLog(this);
        }

        public void Dispose()
        {
            _publisher?.Dispose();
        }

        public void Publish(AssetsInfoUpdate index, string indexName)
        {
            _publisher.ProduceAsync(index);

            _log.Info($"Published '{indexName}': {index.ToJson()}.");
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings
                .ForPublisher(_settings.ConnectionString, _settings.PublishIndicesPriceUpdatesExchange);

            _publisher = new RabbitMqPublisher<AssetsInfoUpdate>(_logFactory, settings)
                .SetSerializer(new JsonMessageSerializer<AssetsInfoUpdate>())
                .DisableInMemoryQueuePersistence()
                .Start();
        }

        public void Stop()
        {
            _publisher?.Stop();
        }
    }
}
