using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.IndicesFacade.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class IndicesFacadeSettings
    {
        public DbSettings Db { get; set; }
    }
}
