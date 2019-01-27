namespace Lykke.Service.IndicesFacade.Contract
{
    /// <summary>
    /// Source update of the price
    /// </summary>
    public class SourcePriceUpdate
    {
        /// <summary>
        /// Name of the source
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Asset id
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// Price on the source
        /// </summary>
        public decimal Price { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Source}, {AssetId}, {Price}";
        }
    }
}
