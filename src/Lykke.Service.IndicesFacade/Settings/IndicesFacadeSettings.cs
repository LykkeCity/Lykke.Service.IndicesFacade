using JetBrains.Annotations;

namespace Lykke.Service.IndicesFacade.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class IndicesFacadeSettings
    {
        public DbSettings Db { get; set; }

        public RabbitMqSettings RabbitMq { get; set; }
    }
}
