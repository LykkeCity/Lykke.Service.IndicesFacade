using System.Collections.Generic;

namespace Lykke.Service.IndicesFacade.Contract
{
    /// <summary>
    /// Represents an index assets info update
    /// </summary>
    public class AssetsInfoUpdate
    {
        /// <summary>
        /// Asset id of the index
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// Source price updates
        /// </summary>
        public IList<SourcePriceUpdate> PriceUpdates { get; set; } = new List<SourcePriceUpdate>();

        /// <summary>
        /// Assets market capitalization updates
        /// </summary>
        public IList<AssetMarketCapUpdate> MarketCapUpdates { get; set; } = new List<AssetMarketCapUpdate>();

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{AssetId}";
        }
    }
}
