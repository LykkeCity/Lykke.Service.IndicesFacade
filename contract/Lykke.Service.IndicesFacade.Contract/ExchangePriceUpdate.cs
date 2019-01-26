namespace Lykke.Service.IndicesFacade.Contract
{
    /// <summary>
    /// Update of the exchange price
    /// </summary>
    public class ExchangePriceUpdate
    {
        /// <summary>
        /// Asset id
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// Name of the exchange
        /// </summary>
        public string ExchangeName { get; set; }

        /// <summary>
        /// Price on the exchange
        /// </summary>
        public decimal Price { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{AssetId}, {ExchangeName}, {Price}";
        }
    }
}
