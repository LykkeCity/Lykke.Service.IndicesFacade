using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.IndicesFacade.Client 
{
    /// <summary>
    /// IndicesFacade client settings.
    /// </summary>
    public class IndicesFacadeServiceClientSettings 
    {
        /// <summary>Service url.</summary>
        [HttpCheck("api/isalive")]
        public string ServiceUrl {get; set;}
    }
}
