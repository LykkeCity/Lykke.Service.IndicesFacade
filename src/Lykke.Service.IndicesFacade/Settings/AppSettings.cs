using JetBrains.Annotations;
using Lykke.Sdk.Settings;

namespace Lykke.Service.IndicesFacade.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public IndicesFacadeSettings IndicesFacadeService { get; set; }

        public CryptoIndexServiceClientInstancesSettings CryptoIndexServiceClient { get; set; }

        public AssetsServiceClientSettings AssetsServiceClient { get; set; }
    }
}
