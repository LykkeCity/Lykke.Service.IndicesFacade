using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.IndicesFacade.Settings
{
    public class RabbitMqSettings
    {
        [AmqpCheck]
        public string ConnectionString { get; set; }

        public string SubscribeCryptoIndicesTickPricesExchange { get; set; }

        public string PublishIndicesUpdatesExchange { get; set; }

        public string PublishIndicesHistoryUpdatesExchange { get; set; }

        public string PublishIndicesPriceUpdatesExchange { get; set; }
    }
}
