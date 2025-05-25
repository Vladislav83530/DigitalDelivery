using DigitalDelivery.Application.Interfaces;
using DigitalDelivery.Domain.Entities;
using DigitalDelivery.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;

namespace DigitalDelivery.Application.Services
{
    public class MapService : IMapService
    {
        private readonly AppDbContext _context;

        public MapService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Node>> GetNodesAsync()
        {
            return await _context.Nodes.ToListAsync();
        }

        public async Task<List<Edge>> GetEdgesAsync()
        {
            return await _context.Edges.ToListAsync();
        }
    }
}
