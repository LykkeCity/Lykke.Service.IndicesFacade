namespace Lykke.Service.IndicesFacade.Contract
{
    /// <summary>
    /// Source price of the asset
    /// </summary>
    public class SourcePrice
    {
        /// <summary>
        /// Source name
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Price on the source
        /// </summary>
        public decimal Price { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Source}, {Price}";
        }
    }
}
