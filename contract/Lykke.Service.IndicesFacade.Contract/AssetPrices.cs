using System.Collections.Generic;

namespace Lykke.Service.IndicesFacade.Contract
{
    /// <summary>
    /// Represents asset prices
    /// </summary>
    public class AssetPrices
    {
        /// <summary>
        /// Asset id
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// Asset prices
        /// </summary>
        public IList<SourcePrice> Prices { get; set; } = new List<SourcePrice>();

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{AssetId}, Prices: [{Prices.Count}]";
        }
    }
}
