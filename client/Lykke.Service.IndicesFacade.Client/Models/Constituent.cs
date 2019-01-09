namespace Lykke.Service.IndicesFacade.Client.Models
{
    /// <summary>
    /// Constituent of the index
    /// </summary>
    public class Constituent
    {
        /// <summary>
        /// Asset id
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// Weight of the current constituent
        /// </summary>
        public decimal Weight { get; set; }
    }
}
