using DigitalDelivery.Domain.Entities;

namespace DigitalDelivery.Infrastructure.Queues
{
    public interface IOrderQueue
    {
        Task EnqueueAsync(Order order);
        Task<Order?> DequeueAsync();
        Task<int> GetQueueLengthAsync();
    }

    public class InMemoryOrderQueue : IOrderQueue
    {
        private readonly Queue<Order> _queue = new Queue<Order>();

        public async Task EnqueueAsync(Order order)
        {
            _queue.Enqueue(order);
            await Task.CompletedTask;
        }

        public async Task<Order?> DequeueAsync()
        {
            return await Task.FromResult(_queue.Any() ? _queue.Dequeue() : null);
        }

        public async Task<int> GetQueueLengthAsync()
        {
            return await Task.FromResult(_queue.Count);
        }
    }
}
