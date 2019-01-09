using Autofac;
using Common;
using Lykke.Service.IndicesFacade.Services;
using Lykke.Service.IndicesFacade.Settings;
using Lykke.SettingsReader;

namespace Lykke.Service.IndicesFacade.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;

        public ServiceModule(IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var cryptoIndexInstances = _appSettings.CurrentValue.CryptoIndexServiceClient;
            builder.RegisterInstance(cryptoIndexInstances).SingleInstance();

            builder.RegisterType<IndicesFacadeService>()
                   .AsSelf()
                   .As<IStartable>()
                   .As<IStopable>()
                   .SingleInstance();
        }
    }
}
