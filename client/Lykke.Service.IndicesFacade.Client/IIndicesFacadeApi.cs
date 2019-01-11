using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.IndicesFacade.Contract;
using Refit;

namespace Lykke.Service.IndicesFacade.Client
{
    /// <summary>
    /// IndicesFacade client API interface.
    /// </summary>
    [PublicAPI]
    public interface IIndicesFacadeApi
    {
        /// <summary>
        /// Returns all indices with details
        /// </summary>
        [Get("/api/indices")]
        Task<IList<Index>> GetAllAsync();

        /// <summary>
        /// Returns index details by asset id
        /// </summary>
        [Get("/api/indices/{assetId}")]
        Task<Index> GetAsync(string assetId);

        /// <summary>
        /// Returns chart data by asset id and time interval
        /// </summary>
        [Get("/api/indices/{assetId}/history/{timeInterval}")]
        Task<IList<HistoryElement>> GetHistoryAsync(string assetId, TimeInterval timeInterval);
    }
}
