namespace ParcialGonzalezJose.Models;

public sealed class DroneRouteResult
{
    public DroneRouteResult(
        bool success,
        int[,] board,
        bool[,] reachable,
        int reachableCount,
        IReadOnlyList<DroneStep> steps)
    {
        Success = success;
        Board = board;
        Reachable = reachable;
        ReachableCount = reachableCount;
        Steps = steps;
    }

    public bool Success { get; }

    public int[,] Board { get; }

    public bool[,] Reachable { get; }

    public int ReachableCount { get; }

    public IReadOnlyList<DroneStep> Steps { get; }
}
