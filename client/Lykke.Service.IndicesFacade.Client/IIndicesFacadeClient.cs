using JetBrains.Annotations;

namespace Lykke.Service.IndicesFacade.Client
{
    /// <summary>
    /// IndicesFacade client interface.
    /// </summary>
    [PublicAPI]
    public interface IIndicesFacadeClient
    {
        // Make your app's controller interfaces visible by adding corresponding properties here.
        // NO actual methods should be placed here (these go to controller interfaces, for example - IIndicesFacadeApi).
        // ONLY properties for accessing controller interfaces are allowed.

        /// <summary>Application Api interface</summary>
        IIndicesFacadeApi Api { get; }
    }
}
