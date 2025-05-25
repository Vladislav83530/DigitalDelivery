using DigitalDelivery.Domain.Entities;

namespace DigitalDelivery.Application.Interfaces
{
    public interface IAStarPathFinder
    {
        List<Node> FindPath(long startNodeId, long goalNodeId, Dictionary<long, Node> nodes, Dictionary<long, List<Edge>> graph);
    }
}