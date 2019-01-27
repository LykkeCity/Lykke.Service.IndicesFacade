namespace Lykke.Service.IndicesFacade.Contract
{
    /// <summary>
    /// Update of the asset market cap
    /// </summary>
    public class AssetMarketCapUpdate
    {
        /// <summary>
        /// Asset id
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// Market capitalization of the asset in USD
        /// </summary>
        public decimal MarketCap { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{AssetId}, MarketCap: {MarketCap}";
        }
    }
}
