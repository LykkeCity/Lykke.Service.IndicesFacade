using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.IndicesFacade.Client;
using Lykke.Service.IndicesFacade.Contract;
using Lykke.Service.IndicesFacade.Services;
using Microsoft.AspNetCore.Mvc;
using ValidationApiException = Lykke.Common.ApiLibrary.Exceptions.ValidationApiException;

namespace Lykke.Service.IndicesFacade.Controllers
{
    [Route("/api/[controller]")]
    public class IndicesController : Controller, IIndicesFacadeApi
    {
        private readonly IndicesFacadeService _indicesFacadeService;

        public IndicesController(IndicesFacadeService indicesFacadeService)
        {
            _indicesFacadeService = indicesFacadeService;
        }

        [HttpGet("")]
        [ProducesResponseType(typeof(IList<Index>), (int)HttpStatusCode.OK)]
        public async Task<IList<Index>> GetAllAsync()
        {
            return await _indicesFacadeService.GetAllAsync();
        }

        [HttpGet("{assetId}")]
        [ProducesResponseType(typeof(Index), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<Index> GetAsync(string assetId)
        {
            if (string.IsNullOrWhiteSpace(assetId))
                throw new ValidationApiException(HttpStatusCode.BadRequest, "Please fill 'assetId' parameter.");

            if (!_indicesFacadeService.IsPresent(assetId))
                throw new ValidationApiException(HttpStatusCode.NotFound, $"Index is not found by 'assetId' : {assetId}.");

            return await _indicesFacadeService.GetAsync(assetId);
        }

        [HttpGet("{assetId}/history/{timeInterval}")]
        [ProducesResponseType(typeof(IList<HistoryElement>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IList<HistoryElement>> GetHistoryAsync(string assetId, TimeInterval timeInterval)
        {
            if (string.IsNullOrWhiteSpace(assetId))
                throw new ValidationApiException(HttpStatusCode.BadRequest, "Please fill 'assetId' parameter.");

            if (!_indicesFacadeService.IsPresent(assetId))
                throw new ValidationApiException(HttpStatusCode.NotFound, $"Index is not found by 'assetId' : {assetId}.");

            if (timeInterval == TimeInterval.Unspecified)
                throw new ValidationApiException(HttpStatusCode.BadRequest, "Please fill 'timeInterval' parameter.");

            return await _indicesFacadeService.GetHistoryAsync(assetId, timeInterval);
        }

        [HttpGet("{assetId}/assetsInfo")]
        [ProducesResponseType(typeof(IList<AssetInfo>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IList<AssetInfo>> GetAssetInfosAsync(string assetId)
        {
            if (string.IsNullOrWhiteSpace(assetId))
                throw new ValidationApiException(HttpStatusCode.BadRequest, "Please fill 'assetId' parameter.");

            if (!_indicesFacadeService.IsPresent(assetId))
                throw new ValidationApiException(HttpStatusCode.NotFound, $"Index is not found by 'assetId' : {assetId}.");

            return await _indicesFacadeService.GetAssetInfosAsync(assetId);
        }
    }
}
