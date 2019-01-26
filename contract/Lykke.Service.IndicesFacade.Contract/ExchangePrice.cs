namespace Lykke.Service.IndicesFacade.Contract
{
    /// <summary>
    /// Exchange price of the asset
    /// </summary>
    public class ExchangePrice
    {
        /// <summary>
        /// Exchange name
        /// </summary>
        public string ExchangeName { get; set; }

        /// <summary>
        /// Price on the exchange
        /// </summary>
        public decimal Price { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{ExchangeName}, {Price}";
        }
    }
}
