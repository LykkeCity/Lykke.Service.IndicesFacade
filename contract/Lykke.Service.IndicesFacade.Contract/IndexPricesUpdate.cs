using System.Collections.Generic;

namespace Lykke.Service.IndicesFacade.Contract
{
    public class IndexPricesUpdate
    {
        /// <summary>
        /// Asset id of the index
        /// </summary>
        public string IndexAssetId { get; set; }

        /// <summary>
        /// Exchange price updates
        /// </summary>
        public IList<ExchangePriceUpdate> ExchangePriceUpdates { get; set; } = new List<ExchangePriceUpdate>();

        /// <summary>
        /// Asset price updates
        /// </summary>
        public IList<AssetPriceUpdate> AssetPriceUpdates { get; set; } = new List<AssetPriceUpdate>();

        /// <summary>
        /// Asset market capitalization updates
        /// </summary>
        public IList<AssetMarketCapUpdate> AssetMarketCapUpdates { get; set; } = new List<AssetMarketCapUpdate>();

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{IndexAssetId}";
        }
    }
}
