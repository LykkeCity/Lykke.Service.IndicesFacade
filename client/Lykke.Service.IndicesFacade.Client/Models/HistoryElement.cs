using System;
using Newtonsoft.Json;

namespace Lykke.Service.IndicesFacade.Client.Models
{
    /// <summary>
    /// Index history element.
    /// </summary>
    public class HistoryElement
    {
        /// <summary>
        /// Value of the index
        /// </summary>
        [JsonProperty(PropertyName = "v")]
        public decimal Value { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonProperty(PropertyName = "dt")]
        public DateTime Timestamp { get; set; }
    }
}
