using System;
using System.Collections.Generic;

namespace Lykke.Service.IndicesFacade.Contract
{
    /// <summary>
    /// Index with details.
    /// </summary>
    public class Index
    {
        /// <summary>
        /// Asset id of index
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// Index name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Index value
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Constituents with weights
        /// </summary>
        public IList<Constituent> Composition { get; set; }

        /// <summary>
        /// Timestamp of current index value
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Return for the last 24 hours
        /// </summary>
        public decimal Return24H { get; set; }

        /// <summary>
        /// Return for the last 5 days
        /// </summary>
        public decimal Return5D { get; set; }

        /// <summary>
        /// Return for the last 30 days
        /// </summary>
        public decimal Return30D { get; set; }

        /// <summary>
        /// Maximum value for the last 24 hours
        /// </summary>
        public decimal Max24H { get; set; }

        /// <summary>
        /// Minimum value for the last 24 hours
        /// </summary>
        public decimal Min24H { get; set; }

        /// <summary>
        /// Volatility for the last 24 hours
        /// </summary>
        public decimal Volatility24H { get; set; }

        /// <summary>
        /// Volatility for the last 30 days
        /// </summary>
        public decimal Volatility30D { get; set; }
    }
}
