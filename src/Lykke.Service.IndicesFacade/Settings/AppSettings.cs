using JetBrains.Annotations;
using Lykke.Sdk.Settings;
using Lykke.Service.Assets.Client;

namespace Lykke.Service.IndicesFacade.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public IndicesFacadeSettings IndicesFacadeService { get; set; }

        public CryptoIndexServiceClientInstancesSettings CryptoIndexServiceClient { get; set; }

        public AssetServiceSettings AssetsServiceClient { get; set; }
    }
}
