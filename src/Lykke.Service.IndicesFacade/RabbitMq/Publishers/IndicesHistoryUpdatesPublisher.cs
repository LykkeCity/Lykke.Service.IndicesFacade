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
    internal sealed class IndicesHistoryUpdatesPublisher : IStartable, IStopable
    {
        private readonly ILogFactory _logFactory;
        private readonly RabbitMqSettings _settings;
        private RabbitMqPublisher<HistoryElementUpdate> _publisher;
        private readonly ILog _log;

        public IndicesHistoryUpdatesPublisher(RabbitMqSettings settings, ILogFactory logFactory)
        {
            _logFactory = logFactory;
            _settings = settings;
            _log = logFactory.CreateLog(this);
        }

        public void Dispose()
        {
            _publisher?.Dispose();
        }

        public void Publish(HistoryElementUpdate historyElementUpdate)
        {
            _publisher.ProduceAsync(historyElementUpdate);

            _log.Info($"Published '{historyElementUpdate.AssetId}' {historyElementUpdate.TimeInterval} update: {historyElementUpdate.ToJson()}.");
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings
                .ForPublisher(_settings.ConnectionString, _settings.PublishIndicesHistoryUpdatesExchange);

            _publisher = new RabbitMqPublisher<HistoryElementUpdate>(_logFactory, settings)
                .SetSerializer(new JsonMessageSerializer<HistoryElementUpdate>())
                .DisableInMemoryQueuePersistence()
                .Start();
        }

        public void Stop()
        {
            _publisher?.Stop();
        }
    }
}
