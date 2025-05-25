using DigitalDelivery.Application.Models;
using DigitalDelivery.Application.Models.Map;
using DigitalDelivery.Domain.Entities;
using DigitalDelivery.Domain.Enums;

namespace DigitalDelivery.Application.Interfaces
{
    public interface IRouteService
    {
        List<Node> FindShortestPath(GeoCoordinate startCoordinate, GeoCoordinate finishCoordinate);
        Task<Result<bool>> SaveShortestPathAsync(List<Node> nodes, RouteType type, int orderId, double totalDistance);
        Task<(List<Node>, List<Edge>)> GetShortestPathAsync(int id);
        Task<double> CalculateTotalDistanceAsync(List<Node> nodes);
    }
}
