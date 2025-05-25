using DigitalDelivery.Application.Interfaces;
using DigitalDelivery.Domain.Entities;
using DigitalDelivery.Infrastructure.EF;
using System.Globalization;
using System.Xml.Linq;

namespace DigitalDelivery.Application.Services
{
    public class OsmMapParser : IMapParser
    {
        private readonly AppDbContext _context;
        private long _newNodeId = -1;

        public OsmMapParser(AppDbContext context)
        {
            _context = context;
        }

        public void ParseAndSave(string filePath)
        {
            var xml = XDocument.Load(filePath);

            var nodeDict = ParseNodes(xml);
            var (roadWays, buildingWays) = ParseWays(xml);

            var edges = BuildRoadGraph(roadWays, nodeDict);

            ProcessBuildings(buildingWays, nodeDict, edges);

            SaveToDatabase(nodeDict.Values.ToList(), edges);
        }

        private Dictionary<long, Node> ParseNodes(XDocument xml)
        {
            var nodeDict = new Dictionary<long, Node>();
            foreach (var nodeElement in xml.Descendants("node"))
            {
                var id = long.Parse(nodeElement.Attribute("id")!.Value);
                var node = new Node
                {
                    Id = id,
                    Latitude = double.Parse(nodeElement.Attribute("lat")!.Value, CultureInfo.InvariantCulture),
                    Longitude = double.Parse(nodeElement.Attribute("lon")!.Value, CultureInfo.InvariantCulture)
                };

                nodeDict[id] = node;
            }

            Console.WriteLine($"Loaded {nodeDict.Count} nodes");
            return nodeDict;
        }

        private (List<List<long>> roadWays, List<List<long>> buildingWays) ParseWays(XDocument xml)
        {
            var roadWays = new List<List<long>>();
            var buildingWays = new List<List<long>>();

            foreach (var wayElement in xml.Descendants("way"))
            {
                var tags = wayElement.Elements("tag").ToDictionary(
                    t => t.Attribute("k")?.Value,
                    t => t.Attribute("v")?.Value
                );

                var ndRefs = wayElement.Elements("nd")
                    .Select(nd => long.Parse(nd.Attribute("ref")!.Value))
                    .ToList();

                if (tags.ContainsKey("highway"))
                {
                    roadWays.Add(ndRefs);
                }

                if (tags.ContainsKey("building"))
                {
                    buildingWays.Add(ndRefs);
                }
            }

            Console.WriteLine($"Loaded {roadWays.Count} roads and {buildingWays.Count} buildings");
            return (roadWays, buildingWays);

        }

        private List<Edge> BuildRoadGraph(List<List<long>> roadWays, Dictionary<long, Node> nodeDict)
        {
            var edges = new List<Edge>();

            foreach (var way in roadWays)
            {
                for (int i = 0; i < way.Count - 1; i++)
                {
                    if (nodeDict.TryGetValue(way[i], out var fromNode) && nodeDict.TryGetValue(way[i + 1], out var toNode))
                    {
                        if (fromNode.IsBuildingCenter && toNode.IsBuildingCenter)
                        {
                            continue;
                        }

                        double cost = GetDistance(fromNode.Latitude, fromNode.Longitude, toNode.Latitude, toNode.Longitude);

                        edges.Add(new Edge
                        {
                            FromNodeId = fromNode.Id,
                            ToNodeId = toNode.Id,
                            Cost = cost
                        });

                        edges.Add(new Edge
                        {
                            FromNodeId = toNode.Id,
                            ToNodeId = fromNode.Id,
                            Cost = cost
                        });
                    }
                }
            }

            Console.WriteLine($"Generated {edges.Count} edges");
            return edges;
        }

        private void ProcessBuildings(List<List<long>> buildingWays, Dictionary<long, Node> nodeDict, List<Edge> edges)
        {
            foreach (var building in buildingWays)
            {
                var buildingNodes = building
                    .Where(nodeDict.ContainsKey)
                    .Select(id => nodeDict[id])
                    .ToList();

                if (buildingNodes.Count == 0)
                {
                    continue;
                }

                var buildingCenter = new Node
                {
                    Id = _newNodeId--,
                    Latitude = buildingNodes.Average(n => n.Latitude),
                    Longitude = buildingNodes.Average(n => n.Longitude),
                    IsBuildingCenter = true
                };

                nodeDict[buildingCenter.Id] = buildingCenter;

                FindClosestRoadSegment(buildingCenter, nodeDict, edges, out var startNode, out var endNode);

                if (startNode == null || endNode == null)
                {
                    continue;
                }

                var roadPoint = GetClosestPointOnSegment(startNode, endNode, buildingCenter);
                var roadNode = new Node
                {
                    Id = _newNodeId--,
                    Latitude = roadPoint.Lat,
                    Longitude = roadPoint.Lon
                };

                nodeDict[roadNode.Id] = roadNode;

                edges.RemoveAll(e => (e.FromNodeId == startNode.Id && e.ToNodeId == endNode.Id) ||
                                     (e.FromNodeId == endNode.Id && e.ToNodeId == startNode.Id));

                double cost1 = GetDistance(startNode.Latitude, startNode.Longitude, roadNode.Latitude, roadNode.Longitude);
                double cost2 = GetDistance(endNode.Latitude, endNode.Longitude, roadNode.Latitude, roadNode.Longitude);
                edges.Add(new Edge { FromNodeId = startNode.Id, ToNodeId = roadNode.Id, Cost = cost1 });
                edges.Add(new Edge { FromNodeId = roadNode.Id, ToNodeId = startNode.Id, Cost = cost1 });
                edges.Add(new Edge { FromNodeId = endNode.Id, ToNodeId = roadNode.Id, Cost = cost2 });
                edges.Add(new Edge { FromNodeId = roadNode.Id, ToNodeId = endNode.Id, Cost = cost2 });

                double costToBuilding = GetDistance(roadNode.Latitude, roadNode.Longitude, buildingCenter.Latitude, buildingCenter.Longitude);
                edges.Add(new Edge { FromNodeId = roadNode.Id, ToNodeId = buildingCenter.Id, Cost = costToBuilding });
                edges.Add(new Edge { FromNodeId = buildingCenter.Id, ToNodeId = roadNode.Id, Cost = costToBuilding });
            }
        }

        private void FindClosestRoadSegment(Node buildingCenter, Dictionary<long, Node> nodeDict, List<Edge> edges, out Node startNode, out Node endNode)
        {
            double minDistance = double.MaxValue;
            startNode = null;
            endNode = null;

            foreach (var edge in edges)
            {
                if (!nodeDict.TryGetValue(edge.FromNodeId, out var fromNode) ||
                    !nodeDict.TryGetValue(edge.ToNodeId, out var toNode))
                {
                    continue;
                }

                if (fromNode.IsBuildingCenter || toNode.IsBuildingCenter) 
                {
                    continue;
                }

                var point = GetClosestPointOnSegment(fromNode, toNode, buildingCenter);
                var distance = GetDistance(buildingCenter.Latitude, buildingCenter.Longitude, point.Lat, point.Lon);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    startNode = fromNode;
                    endNode = toNode;
                }
            }
        }

        private (double Lat, double Lon) GetClosestPointOnSegment(Node a, Node b, Node p)
        {
            var ax = a.Latitude;
            var ay = a.Longitude;
            var bx = b.Latitude;
            var by = b.Longitude;
            var px = p.Latitude;
            var py = p.Longitude;

            var dx = bx - ax;
            var dy = by - ay;

            if (dx == 0 && dy == 0)
            {
                return (ax, ay);
            }

            double t = ((px - ax) * dx + (py - ay) * dy) / (dx * dx + dy * dy);
            t = Math.Max(0, Math.Min(1, t));

            return (ax + t * dx, ay + t * dy);
        }

        private double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371e3; // meters
            var phi1 = lat1 * Math.PI / 180;
            var phi2 = lat2 * Math.PI / 180;
            var deltaPhi = (lat2 - lat1) * Math.PI / 180;
            var deltaLambda = (lon2 - lon1) * Math.PI / 180;

            var a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2) +
                    Math.Cos(phi1) * Math.Cos(phi2) *
                    Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        private void SaveToDatabase(List<Node> nodes, List<Edge> edges)
        {
            var nodeDict = nodes.ToDictionary(n => n.Id);

            var filteredEdges = edges.Where(e =>
            {
                var fromIsBuilding = nodeDict.TryGetValue(e.FromNodeId, out var fromNode) && fromNode.IsBuildingCenter;
                var toIsBuilding = nodeDict.TryGetValue(e.ToNodeId, out var toNode) && toNode.IsBuildingCenter;

                return !(fromIsBuilding && toIsBuilding);
            }).ToList();

            var usedNodeIds = new HashSet<long>(
                filteredEdges.Select(e => e.FromNodeId)
                     .Concat(filteredEdges.Select(e => e.ToNodeId))
            );

            var filteredNodes = nodes.Where(n => usedNodeIds.Contains(n.Id)).ToList();

            _context.Nodes.AddRange(filteredNodes);
            _context.Edges.AddRange(filteredEdges);
            _context.SaveChanges();
            Console.WriteLine($"Saved {nodes.Count} nodes and {edges.Count} edges to database.");
        }
    }
}
