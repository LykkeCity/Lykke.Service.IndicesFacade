using System.Collections.Generic;

namespace Lykke.Service.IndicesFacade.Contract
{
    /// <summary>
    /// Represents index asset price updates
    /// </summary>
    public class AssetPricesUpdate
    {
        /// <summary>
        /// Index asset id
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// Source price updates
        /// </summary>
        public IList<AssetPrices> PriceUpdates { get; set; } = new List<AssetPrices>();

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{AssetId}, Prices: [{PriceUpdates.Count}]";
        }
    }
}
