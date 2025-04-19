using DigitalDelivery.Application.Interfaces;
using DigitalDelivery.Domain.Entities;

namespace DigitalDelivery.Application.Services.RobotSelectionStrategies
{
    public class RandomRobotSelectionStrategy : IRobotSelectionStrategy
    {
        private const double minBateryLvl = 20;

        public Robot SelectBestRobot(IQueryable<Robot> robots, Order order)
        {
            if (robots == null || order == null)
            {
                return null;
            }

            var availableRobots = robots.Where(r => r.Telemetry.BatteryLevel >= minBateryLvl).ToList();

            Random random = new Random();
            int randomIndex = random.Next(availableRobots.Count);

            return availableRobots[randomIndex];
        }
    }
}