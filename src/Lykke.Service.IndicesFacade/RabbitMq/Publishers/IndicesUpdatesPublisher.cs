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
    internal sealed class IndicesUpdatesPublisher : IStartable, IStopable
    {
        private readonly ILogFactory _logFactory;
        private readonly RabbitMqSettings _settings;
        private RabbitMqPublisher<Index> _publisher;
        private readonly ILog _log;

        public IndicesUpdatesPublisher(RabbitMqSettings settings, ILogFactory logFactory)
        {
            _logFactory = logFactory;
            _settings = settings;
            _log = logFactory.CreateLog(this);
        }

        public void Dispose()
        {
            _publisher?.Dispose();
        }

        public void Publish(Index index)
        {
            _publisher.ProduceAsync(index);

            _log.Info($"Published '{index.Name}': {index.ToJson()}.");
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings
                .ForPublisher(_settings.ConnectionString, _settings.PublishIndicesUpdatesExchange);

            _publisher = new RabbitMqPublisher<Index>(_logFactory, settings)
                .SetSerializer(new JsonMessageSerializer<Index>())
                .DisableInMemoryQueuePersistence()
                .Start();
        }

        public void Stop()
        {
            _publisher?.Stop();
        }
    }
}
