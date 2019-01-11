namespace Lykke.Service.IndicesFacade.Contract
{
    public class HistoryElementUpdate
    {
        public string AssetId { get; set; }

        public TimeInterval TimeInterval { get; set; }

        public HistoryElement HistoryElement { get; set; }
    }
}
