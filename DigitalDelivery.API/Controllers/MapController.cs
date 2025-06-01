using DigitalDelivery.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalDelivery.API.Controllers
{
    [Authorize, ApiController, Route("api/map")]
    public class MapController : Controller
    {
        private readonly IMapParser _mapParser;
        private readonly IMapService _mapService;

        public MapController(IMapParser mapParser, IMapService mapService)
        {
            _mapParser = mapParser;
            _mapService = mapService;
        }

        [HttpPost("parse")]
        public IActionResult ParseMapFromOsmFile()
        {
            _mapParser.ParseAndSave("export.osm");
            return Ok();
        }

        [HttpGet("graph")]
        public async Task<IActionResult> GetGraph()
        {
            var nodes = await _mapService.GetNodesAsync();
            var edges = await _mapService.GetEdgesAsync();

            return Ok(new
            {
                Nodes = nodes,
                Edges = edges
            });
        }
    }
}
