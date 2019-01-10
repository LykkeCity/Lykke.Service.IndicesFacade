using Lykke.HttpClientGenerator;

namespace Lykke.Service.IndicesFacade.Client
{
    /// <summary>
    /// IndicesFacade API aggregating interface.
    /// </summary>
    public class IndicesFacadeClient : IIndicesFacadeClient
    {
        // Note: Add similar Api properties for each new service controller

        /// <summary>Inerface to IndicesFacade Api.</summary>
        public IIndicesFacadeApi Api { get; private set; }

        /// <summary>C-tor</summary>
        public IndicesFacadeClient(IHttpClientGenerator httpClientGenerator)
        {
            Api = httpClientGenerator.Generate<IIndicesFacadeApi>();
        }
    }
}
