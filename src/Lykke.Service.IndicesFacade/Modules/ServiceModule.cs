using Autofac;
using Common;
using Lykke.Service.Assets.Client;
using Lykke.Service.IndicesFacade.RabbitMq.Publishers;
using Lykke.Service.IndicesFacade.RabbitMq.Subscribers;
using Lykke.Service.IndicesFacade.Services;
using Lykke.Service.IndicesFacade.Settings;
using Lykke.SettingsReader;

namespace Lykke.Service.IndicesFacade.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;
        private readonly IndicesFacadeSettings _settings;

        public ServiceModule(IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings;
            _settings = _appSettings.CurrentValue.IndicesFacadeService;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var cryptoIndexInstances = _appSettings.CurrentValue.CryptoIndexServiceClient;
            builder.RegisterInstance(cryptoIndexInstances).SingleInstance();

            builder.RegisterAssetsClient(new AssetServiceSettings {
                ServiceUrl = _appSettings.CurrentValue.AssetsServiceClient.ServiceUrl });

            builder.RegisterType<IndicesFacadeService>()
                   .AsSelf()
                   .SingleInstance();

            builder.RegisterType<CryptoIndicesTickPriceSubscriber>()
                .AsSelf()
                .As<IStartable>()
                .As<IStopable>()
                .WithParameter(TypedParameter.From(_settings.RabbitMq))
                .SingleInstance();

            builder.RegisterType<IndicesHistoryUpdatesPublisher>()
                .AsSelf()
                .As<IStartable>()
                .As<IStopable>()
                .WithParameter(TypedParameter.From(_settings.RabbitMq))
                .SingleInstance();

            builder.RegisterType<IndicesUpdatesPublisher>()
                .AsSelf()
                .As<IStartable>()
                .As<IStopable>()
                .WithParameter(TypedParameter.From(_settings.RabbitMq))
                .SingleInstance();

            builder.RegisterType<IndicesPriceUpdatesPublisher>()
                .AsSelf()
                .As<IStartable>()
                .As<IStopable>()
                .WithParameter(TypedParameter.From(_settings.RabbitMq))
                .SingleInstance();
        }
    }
}
