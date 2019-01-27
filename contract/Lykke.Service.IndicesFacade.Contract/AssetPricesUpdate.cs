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
        public IList<SourcePriceUpdate> PriceUpdates { get; set; } = new List<SourcePriceUpdate>();

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{AssetId}, Prices: [{PriceUpdates.Count}]";
        }
    }
}
