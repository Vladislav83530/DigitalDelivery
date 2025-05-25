using DigitalDelivery.Domain.Entities;

namespace DigitalDelivery.Application.Interfaces
{
    public interface IMapService
    {
        public Task<List<Node>> GetNodesAsync();
        public Task<List<Edge>> GetEdgesAsync();
    }
}
