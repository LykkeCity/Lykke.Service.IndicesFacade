namespace Lykke.Service.IndicesFacade.Contract
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

        /// <summary>
        /// Price of the current constituent
        /// </summary>
        public decimal Price { get; set; }
    }
}
