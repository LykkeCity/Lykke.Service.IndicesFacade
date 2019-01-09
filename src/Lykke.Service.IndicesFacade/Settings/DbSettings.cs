using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.IndicesFacade.Settings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}
