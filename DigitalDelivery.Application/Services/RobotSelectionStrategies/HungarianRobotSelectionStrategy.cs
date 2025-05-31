using System;
using System.Collections.Generic;
using System.Linq;
using DigitalDelivery.Application.Interfaces;
using DigitalDelivery.Application.Models.Map;
using DigitalDelivery.Domain.Entities;

public class HungarianRobotSelectionStrategy : IRobotSelectionStrategy
{
    private readonly IDistanceCalculationService _distanceCalculationService;

    public HungarianRobotSelectionStrategy(IDistanceCalculationService distanceCalculationService)
    {
        _distanceCalculationService = distanceCalculationService;
    }

    public Robot SelectBestRobot(IQueryable<Robot> robots, Order order)
    {
        throw new NotImplementedException("This method is not used in this strategy.");
    }

    public Dictionary<Robot, Order> AssignSingleOrderToRobots(List<Robot> robots, List<Order> orders)
    {
        int numRobots = robots.Count;
        int numOrders = orders.Count;

        if (numRobots == 0 || numOrders == 0)
        {
            return new Dictionary<Robot, Order>();
        }

        double[,] costMatrix = new double[numRobots, numOrders];

        for (int i = 0; i < numRobots; i++)
        {
            for (int j = 0; j < numOrders; j++)
            {
                costMatrix[i, j] = CalculateCost(robots[i], orders[j]);
            }
        }

        int[] assignments = HungarianAlgorithm.Solve(costMatrix);

        var robotOrderAssignments = new Dictionary<Robot, Order>();
        for (int i = 0; i < assignments.Length; i++)
        {
            if (assignments[i] != -1)
            {
                robotOrderAssignments[robots[i]] = orders[assignments[i]];
            }
        }

        return robotOrderAssignments;
    }

    private double CalculateCost(Robot robot, Order order)
    {
        (double minDistance, double minDistanceWithCharging) = _distanceCalculationService.SimpleCalculationDistance(
            new GeoCoordinate(robot.Telemetry.Latitude, robot.Telemetry.Longitude),
            new GeoCoordinate(order.PickupAddress.Latitude, order.PickupAddress.Longitude),
            new GeoCoordinate(order.DeliveryAddress.Latitude, order.DeliveryAddress.Longitude));

        double time = minDistance / robot.Specification.MaxSpeedKph;

        if (!HasEnoughBattery(robot, minDistanceWithCharging))
        {
            return double.MaxValue;
        }

        time += (DateTime.UtcNow - order.CreatedAt).TotalMinutes / 10.0;

        return time;
    }

    private bool HasEnoughBattery(Robot robot, double distance)
    {
        double neededBattery = distance * robot.Specification.EnergyConsumptionPerM;
        double currentBattery = robot.Specification.BatteryCapacityAh * robot.Telemetry.BatteryLevel / 100.0;

        return currentBattery >= neededBattery;
    }
}

public static class HungarianAlgorithm
{
    public static int[] Solve(double[,] costMatrix)
    {
        int n = costMatrix.GetLength(0);
        int m = costMatrix.GetLength(1);

        if (n != m)
        {
            throw new ArgumentException("Cost matrix must be square.");
        }

        int[] assignments = new int[n];
        int[] rowCover = new int[n];
        int[] colCover = new int[n];
        int zeroRowCount = 0;

        for (int i = 0; i < n; i++)
        {
            double minVal = double.MaxValue;
            for (int j = 0; j < n; j++)
            {
                if (costMatrix[i, j] < minVal)
                {
                    minVal = costMatrix[i, j];
                }
            }
            for (int j = 0; j < n; j++)
            {
                costMatrix[i, j] -= minVal;
            }
        }

        for (int j = 0; j < n; j++)
        {
            double minVal = double.MaxValue;
            for (int i = 0; i < n; i++)
            {
                if (costMatrix[i, j] < minVal)
                {
                    minVal = costMatrix[i, j];
                }
            }
            for (int i = 0; i < n; i++)
            {
                costMatrix[i, j] -= minVal;
            }
        }

        while (true)
        {
            zeroRowCount = CoverZeros(costMatrix, rowCover, colCover);
            if (zeroRowCount == n)
            {
                break;
            }
            AdjustMatrix(costMatrix, rowCover, colCover);
        }
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (costMatrix[i, j] == 0 && rowCover[i] == 0 && colCover[j] == 0)
                {
                    assignments[i] = j;
                    rowCover[i] = 1;
                    colCover[j] = 1;
                    break;
                }
            }
        }

        return assignments;
    }

    private static int CoverZeros(double[,] costMatrix, int[] rowCover, int[] colCover)
    {
        int n = costMatrix.GetLength(0);
        int zeroRowCount = 0;

        Array.Clear(rowCover, 0, n);
        Array.Clear(colCover, 0, n);

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (costMatrix[i, j] == 0)
                {
                    if (rowCover[i] == 0 && colCover[j] == 0)
                    {
                        rowCover[i] = 1;
                        colCover[j] = 1;
                        zeroRowCount++;
                    }
                }
            }
        }

        return zeroRowCount;
    }

    private static void AdjustMatrix(double[,] costMatrix, int[] rowCover, int[] colCover)
    {
        int n = costMatrix.GetLength(0);
        double minVal = double.MaxValue;

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (rowCover[i] == 0 && colCover[j] == 0 && costMatrix[i, j] < minVal)
                {
                    minVal = costMatrix[i, j];
                }
            }
        }

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (rowCover[i] == 1)
                {
                    costMatrix[i, j] += minVal;
                }
                if (colCover[j] == 0)
                {
                    costMatrix[i, j] -= minVal;
                }
            }
        }
    }
}