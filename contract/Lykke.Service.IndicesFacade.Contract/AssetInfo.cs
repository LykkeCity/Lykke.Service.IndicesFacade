using System.Collections.Generic;

namespace Lykke.Service.IndicesFacade.Contract
{
    /// <summary>
    /// Represents information about the asset.
    /// </summary>
    public class AssetInfo
    {
        /// <summary>
        /// Asset name
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// Market capitalization of the asset
        /// </summary>
        public decimal MarketCap { get; set; }

        /// <summary>
        /// Exchange prices of the asset
        /// </summary>
        public IList<SourcePrice> Prices { get; set; } = new List<SourcePrice>();

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{AssetId}, MarketCap: {MarketCap}, Prices: [{Prices.Count}]";
        }
    }
}
