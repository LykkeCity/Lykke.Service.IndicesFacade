namespace Lykke.Service.IndicesFacade.Contract
{
    /// <summary>
    /// Update of the used price of the asset
    /// </summary>
    public class AssetPriceUpdate
    {
        /// <summary>
        /// Asset id
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// Using price of the asset
        /// </summary>
        public decimal Price { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{AssetId}, {Price}";
        }
    }
}
