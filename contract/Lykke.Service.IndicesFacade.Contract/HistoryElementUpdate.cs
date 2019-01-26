namespace Lykke.Service.IndicesFacade.Contract
{
    /// <summary>
    /// Update of the history
    /// </summary>
    public class HistoryElementUpdate
    {
        /// <summary>
        /// Asset id
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// Time interval
        /// </summary>
        public TimeInterval TimeInterval { get; set; }

        /// <summary>
        /// History element
        /// </summary>
        public HistoryElement HistoryElement { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{AssetId}, {TimeInterval}, {HistoryElement}";
        }
    }
}
