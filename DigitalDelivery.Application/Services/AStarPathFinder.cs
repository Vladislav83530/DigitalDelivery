using DigitalDelivery.Application.Interfaces;
using DigitalDelivery.Domain.Entities;

namespace DigitalDelivery.Application.Services
{
    public class AStarPathFinder : IAStarPathFinder
    {
        public List<Node> FindPath(
            long startNodeId,
            long goalNodeId,
            Dictionary<long, Node> nodes,
            Dictionary<long, List<Edge>> graph)
        {
            var openSet = new List<long> { startNodeId };
            var closedSet = new HashSet<long>();

            var cameFrom = new Dictionary<long, long>();
            var gScore = nodes.Keys.ToDictionary(id => id, id => double.PositiveInfinity);
            var fScore = nodes.Keys.ToDictionary(id => id, id => double.PositiveInfinity);

            gScore[startNodeId] = 0;
            fScore[startNodeId] = Heuristic(nodes[startNodeId], nodes[goalNodeId]);

            while (openSet.Count > 0)
            {
                long currentId = openSet.OrderBy(id => fScore[id]).First();
                if (currentId == goalNodeId)
                {
                    return ReconstructPath(cameFrom, nodes, currentId);
                }

                openSet.Remove(currentId);
                closedSet.Add(currentId);

                foreach (var edge in graph[currentId])
                {
                    var neighborId = edge.ToNodeId;
                    if (closedSet.Contains(neighborId))
                    {
                        continue;
                    }

                    double tentativeG = gScore[currentId] + edge.Cost;

                    if (!openSet.Contains(neighborId))
                        openSet.Add(neighborId);

                    if (tentativeG >= gScore[neighborId])
                    {
                        continue;
                    }

                    cameFrom[neighborId] = currentId;
                    gScore[neighborId] = tentativeG;
                    fScore[neighborId] = tentativeG + Heuristic(nodes[neighborId], nodes[goalNodeId]);
                }
            }

            return new List<Node>();
        }

        private List<Node> ReconstructPath(
            Dictionary<long, long> cameFrom,
            Dictionary<long, Node> nodes,
            long current)
        {
            var path = new List<Node> { nodes[current] };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Insert(0, nodes[current]);
            }
            return path;
        }

        private double Heuristic(Node a, Node b)
        {
            double dx = a.Longitude - b.Longitude;
            double dy = a.Latitude - b.Latitude;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
