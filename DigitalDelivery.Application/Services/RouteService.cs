using DigitalDelivery.Application.Interfaces;
using DigitalDelivery.Application.Models;
using DigitalDelivery.Application.Models.Map;
using DigitalDelivery.Domain.Entities;
using DigitalDelivery.Domain.Enums;
using DigitalDelivery.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DigitalDelivery.Application.Services
{
    public class RouteService : IRouteService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _memoryCache;
        private readonly IAStarPathFinder _aStarPathFinder;
        private readonly ISimulatedClock _clock;

        private const string NodeCacheKey = "nodes_cache_key";
        private const string GraphCacheKey = "graph_cache_key";
        private readonly OrderStatusEnum[] _inProgressStatuses = { OrderStatusEnum.MoveToDeliveryPoint, OrderStatusEnum.MoveToPickupPoint };

        public RouteService(
            AppDbContext context,
            IMemoryCache memoryCache,
            IAStarPathFinder aStarPathFinder,
            ISimulatedClock clock)
        {
            _context = context;
            _memoryCache = memoryCache;
            _aStarPathFinder = aStarPathFinder;
            _clock = clock;
        }

        public List<Node> FindShortestPath(GeoCoordinate startCoordinate, GeoCoordinate finishCoordinate)
        {
            Dictionary<long, Node> nodes = GetNodes();
            Dictionary<long, List<Edge>> graph = GetGraph();

            long startNodeId = FindNearestNodeId(startCoordinate.Latitude, startCoordinate.Longitude, nodes);
            long goalNodeId = FindNearestNodeId(finishCoordinate.Latitude, finishCoordinate.Longitude, nodes);

            List<Node> route = _aStarPathFinder.FindPath(startNodeId, goalNodeId, nodes, graph);

            return route;
        }

        public async Task<(List<Node>, List<Edge>)> GetShortestPathAsync(int orderId)
        {
            var status = await _context.OrderStatuses
                .Where(os => os.OrderId == orderId)
                .OrderByDescending(os => os.DateIn)
                .FirstOrDefaultAsync();

            if (!_inProgressStatuses.Contains(status.Status))
            {
                return (null, null);
            }

            var routeType = status.Status == OrderStatusEnum.MoveToPickupPoint
                ? RouteType.ToClient
                : RouteType.Delivery;

            var routeNodes = _context.Routes
                .Where(rn => rn.OrderId == orderId && rn.RouteType == routeType)
                .Select(x => x.RouteNodes)
                .FirstOrDefault();

            var routeNodeIds = routeNodes.Select(rn => rn.NodeId).ToList();

            var edges = await _context.Edges
                .Where(e => routeNodeIds.Contains(e.FromNodeId) && routeNodeIds.Contains(e.ToNodeId))
                .ToListAsync();

            var nodes = await _context.Nodes
                .Where(n => routeNodeIds.Contains(n.Id))
                .ToListAsync();

            return (nodes, edges);
        }

        public async Task<Result<bool>> SaveShortestPathAsync(List<Node> nodes, RouteType type, int orderId, double totalDistance)
        {
            var route = new Route
            {
                StartNodeId = nodes.First().Id,
                EndNodeId = nodes.Last().Id,
                TotalDistance = totalDistance,
                CreatedAt = _clock.Now,
                RouteType = type,
                RouteNodes = nodes.Select(node => new RouteNode
                {
                    NodeId = node.Id,

                }).ToList(),
                OrderId = orderId
            };

            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

            return new Result<bool>(true);
        }

        public async Task<double> CalculateTotalDistanceAsync(List<Node> nodes)
        {
            if (nodes == null || nodes.Count < 2)
            {
                return 0;
            }

            double totalDistance = 0;

            for (int i = 0; i < nodes.Count - 1; i++)
            {
                long fromNodeId = nodes[i].Id;
                long toNodeId = nodes[i + 1].Id;

                var edge = await _context.Set<Edge>()
                    .FirstOrDefaultAsync(e => e.FromNodeId == fromNodeId && e.ToNodeId == toNodeId);

                if (edge == null)
                {
                    throw new InvalidOperationException($"Edge not found between Node {fromNodeId} and {toNodeId}");
                }

                totalDistance += edge.Cost;
            }

            return totalDistance;
        }

        private Dictionary<long, Node> GetNodes()
        {
            Dictionary<long, Node> nodes = null;

            if (_memoryCache.TryGetValue(NodeCacheKey, out nodes))
            {
                return nodes;
            }

            return _context.Nodes.ToDictionary(n => n.Id);
        }

        private Dictionary<long, List<Edge>> GetGraph()
        {
            Dictionary<long, List<Edge>> graph = null;

            if (_memoryCache.TryGetValue(GraphCacheKey, out graph))
            {
                return graph;
            }

            return _context.Nodes
                .Include(n => n.OutgoingEdges)
                .ToDictionary(n => n.Id, n => n.OutgoingEdges?.ToList() ?? new List<Edge>());
        }

        private long FindNearestNodeId(double latitude, double longitude, Dictionary<long, Node> nodes)
        {
            Node nearest = null;
            double minDist = double.MaxValue;

            foreach (var node in nodes)
            {
                double dx = node.Value.Longitude - longitude;
                double dy = node.Value.Latitude - latitude;
                double dist = dx * dx + dy * dy;

                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = node.Value;
                }
            }

            return nearest?.Id ?? throw new InvalidOperationException("No nodes found.");
        }
    }
}
