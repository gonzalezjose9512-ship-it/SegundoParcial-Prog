using ParcialGonzalezJose.Models;

namespace ParcialGonzalezJose.Services;

public sealed class DroneRouteSolver
{
    private const int SmallBoardGreedyAttempts = 20000;
    private const int LargeBoardGreedyAttempts = 512;
    private const int SmallBoardBacktrackingAttempts = 512;
    private const int LargeBoardBacktrackingAttempts = 64;
    private const int SmallBoardNodeLimit = 50000;
    private const int LargeBoardNodeLimit = 10000;

    private static readonly Coordinate[] Movements =
    [
        new(2, 1),
        new(2, -1),
        new(-2, 1),
        new(-2, -1),
        new(1, 2),
        new(1, -2),
        new(-1, 2),
        new(-1, -2)
    ];

    public DroneRouteResult Resolve(int size, Coordinate start)
    {
        if (size < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(size), "El tamanio del terreno debe ser mayor o igual a 1.");
        }

        if (!IsInside(start.X, start.Y, size))
        {
            throw new ArgumentOutOfRangeException(nameof(start), "La coordenada inicial debe estar dentro del terreno.");
        }

        bool[,] reachable = CalculateReachableCells(size, start);
        int reachableCount = CountReachableCells(reachable, size);
        int[,] board = CreateBoard(size);

        if (!HasValidColorBalance(reachable, size, start))
        {
            return new DroneRouteResult(false, board, reachable, reachableCount, Array.Empty<DroneStep>());
        }

        bool success = TryFindRoute(
            size,
            reachable,
            reachableCount,
            start,
            board,
            out IReadOnlyList<DroneStep> finalSteps);

        return new DroneRouteResult(success, board, reachable, reachableCount, finalSteps);
    }

    private static bool TryFindRoute(
        int size,
        bool[,] reachable,
        int reachableCount,
        Coordinate start,
        int[,] board,
        out IReadOnlyList<DroneStep> steps)
    {
        int attempts = reachableCount <= 100
            ? SmallBoardGreedyAttempts
            : LargeBoardGreedyAttempts;

        var currentSteps = new List<DroneStep>(reachableCount);

        int attempt = 0;
        while (attempt < attempts)
        {
            ResetBoard(board, size);
            currentSteps.Clear();
            board[start.X, start.Y] = 0;
            currentSteps.Add(new DroneStep(0, start.X, start.Y));

            var random = new Random(GetSeed(size, start, attempt));

            if (TryGreedyRoute(size, board, reachable, reachableCount, start, currentSteps, random))
            {
                steps = currentSteps.ToList();
                return true;
            }

            attempt++;
        }

        attempts = reachableCount <= 100
            ? SmallBoardBacktrackingAttempts
            : LargeBoardBacktrackingAttempts;

        int nodeLimit = reachableCount <= 100
            ? SmallBoardNodeLimit
            : LargeBoardNodeLimit;

        attempt = 0;
        while (attempt < attempts)
        {
            ResetBoard(board, size);
            currentSteps.Clear();
            board[start.X, start.Y] = 0;
            currentSteps.Add(new DroneStep(0, start.X, start.Y));

            var random = new Random(GetSeed(size, start, attempt + SmallBoardGreedyAttempts));
            int nodesRemaining = nodeLimit;

            if (SearchBounded(
                    size,
                    board,
                    reachable,
                    start.X,
                    start.Y,
                    nextStepNumber: 1,
                    targetCount: reachableCount,
                    currentSteps,
                    random,
                    ref nodesRemaining))
            {
                steps = currentSteps.ToList();
                return true;
            }

            attempt++;
        }

        steps = Array.Empty<DroneStep>();
        return false;
    }

    private static bool TryGreedyRoute(
        int size,
        int[,] board,
        bool[,] reachable,
        int reachableCount,
        Coordinate start,
        List<DroneStep> steps,
        Random random)
    {
        int currentX = start.X;
        int currentY = start.Y;
        int nextStepNumber = 1;

        while (nextStepNumber < reachableCount)
        {
            List<Candidate> candidates = GetOrderedCandidates(size, board, reachable, currentX, currentY, random);

            if (candidates.Count == 0)
            {
                return false;
            }

            bool isFinalMove = nextStepNumber + 1 == reachableCount;
            Candidate? selected = SelectCandidate(candidates, isFinalMove);

            if (selected is null)
            {
                return false;
            }

            Candidate selectedValue = selected.Value;
            board[selectedValue.X, selectedValue.Y] = nextStepNumber;
            steps.Add(new DroneStep(nextStepNumber, selectedValue.X, selectedValue.Y));
            currentX = selectedValue.X;
            currentY = selectedValue.Y;
            nextStepNumber++;
        }

        return true;
    }

    private static bool SearchBounded(
        int size,
        int[,] board,
        bool[,] reachable,
        int currentX,
        int currentY,
        int nextStepNumber,
        int targetCount,
        List<DroneStep> steps,
        Random random,
        ref int nodesRemaining)
    {
        if (nextStepNumber == targetCount)
        {
            return true;
        }

        if (nodesRemaining <= 0)
        {
            return false;
        }

        nodesRemaining--;

        List<Candidate> candidates = GetOrderedCandidates(size, board, reachable, currentX, currentY, random);
        int i = 0;

        while (i < candidates.Count)
        {
            Candidate candidate = candidates[i];

            if (candidate.Degree == 0 && nextStepNumber + 1 < targetCount)
            {
                i++;
                continue;
            }

            board[candidate.X, candidate.Y] = nextStepNumber;
            steps.Add(new DroneStep(nextStepNumber, candidate.X, candidate.Y));

            if (!HasDisconnectedRemainder(size, board, reachable, candidate.X, candidate.Y, nextStepNumber + 1, targetCount)
                && SearchBounded(
                    size,
                    board,
                    reachable,
                    candidate.X,
                    candidate.Y,
                    nextStepNumber + 1,
                    targetCount,
                    steps,
                    random,
                    ref nodesRemaining))
            {
                return true;
            }

            steps.RemoveAt(steps.Count - 1);
            board[candidate.X, candidate.Y] = -1;
            i++;
        }

        return false;
    }

    private static Candidate? SelectCandidate(List<Candidate> candidates, bool isFinalMove)
    {
        int i = 0;

        while (i < candidates.Count)
        {
            Candidate candidate = candidates[i];

            if (isFinalMove || candidate.Degree > 0)
            {
                return candidate;
            }

            i++;
        }

        return null;
    }

    private static List<Candidate> GetOrderedCandidates(
        int size,
        int[,] board,
        bool[,] reachable,
        int currentX,
        int currentY,
        Random? random)
    {
        var candidates = new List<Candidate>();
        int i = 0;

        while (i < Movements.Length)
        {
            int nextX = currentX + Movements[i].X;
            int nextY = currentY + Movements[i].Y;

            if (IsAvailable(nextX, nextY, size, board, reachable))
            {
                int degree = CountFreeExits(size, board, reachable, nextX, nextY);
                int tieBreaker = random?.Next() ?? 0;
                candidates.Add(new Candidate(nextX, nextY, degree, tieBreaker));
            }

            i++;
        }

        candidates.Sort(static (left, right) =>
        {
            int comparison = left.Degree.CompareTo(right.Degree);

            if (comparison != 0)
            {
                return comparison;
            }

            comparison = left.TieBreaker.CompareTo(right.TieBreaker);

            if (comparison != 0)
            {
                return comparison;
            }

            comparison = left.X.CompareTo(right.X);

            if (comparison != 0)
            {
                return comparison;
            }

            return left.Y.CompareTo(right.Y);
        });

        return candidates;
    }

    private static int CountFreeExits(int size, int[,] board, bool[,] reachable, int fromX, int fromY)
    {
        int exits = 0;
        int i = 0;

        while (i < Movements.Length)
        {
            int nextX = fromX + Movements[i].X;
            int nextY = fromY + Movements[i].Y;

            if (IsAvailable(nextX, nextY, size, board, reachable))
            {
                exits++;
            }

            i++;
        }

        return exits;
    }

    private static bool HasDisconnectedRemainder(
        int size,
        int[,] board,
        bool[,] reachable,
        int currentX,
        int currentY,
        int visitedCount,
        int targetCount)
    {
        int remaining = targetCount - visitedCount;

        if (remaining == 0)
        {
            return false;
        }

        var seen = new bool[size, size];
        var pending = new Queue<Coordinate>();
        int connected = 0;

        pending.Enqueue(new Coordinate(currentX, currentY));

        while (pending.Count > 0)
        {
            Coordinate current = pending.Dequeue();
            int i = 0;

            while (i < Movements.Length)
            {
                int nextX = current.X + Movements[i].X;
                int nextY = current.Y + Movements[i].Y;

                if (IsInside(nextX, nextY, size)
                    && reachable[nextX, nextY]
                    && board[nextX, nextY] == -1
                    && !seen[nextX, nextY])
                {
                    seen[nextX, nextY] = true;
                    connected++;
                    pending.Enqueue(new Coordinate(nextX, nextY));
                }

                i++;
            }
        }

        if (connected != remaining)
        {
            return true;
        }

        int x = 0;
        while (x < size)
        {
            int y = 0;

            while (y < size)
            {
                if (reachable[x, y] && board[x, y] == -1)
                {
                    int exits = CountFreeExits(size, board, reachable, x, y);

                    if (exits == 0 && (remaining > 1 || !CanMove(currentX, currentY, x, y)))
                    {
                        return true;
                    }
                }

                y++;
            }

            x++;
        }

        return false;
    }

    private static bool[,] CalculateReachableCells(int size, Coordinate start)
    {
        var reachable = new bool[size, size];
        var pending = new Queue<Coordinate>();

        reachable[start.X, start.Y] = true;
        pending.Enqueue(start);

        while (pending.Count > 0)
        {
            Coordinate current = pending.Dequeue();
            int i = 0;

            while (i < Movements.Length)
            {
                int nextX = current.X + Movements[i].X;
                int nextY = current.Y + Movements[i].Y;

                if (IsInside(nextX, nextY, size) && !reachable[nextX, nextY])
                {
                    reachable[nextX, nextY] = true;
                    pending.Enqueue(new Coordinate(nextX, nextY));
                }

                i++;
            }
        }

        return reachable;
    }

    private static int CountReachableCells(bool[,] reachable, int size)
    {
        int count = 0;
        int x = 0;

        while (x < size)
        {
            int y = 0;

            while (y < size)
            {
                if (reachable[x, y])
                {
                    count++;
                }

                y++;
            }

            x++;
        }

        return count;
    }

    private static bool HasValidColorBalance(bool[,] reachable, int size, Coordinate start)
    {
        int evenCount = 0;
        int oddCount = 0;
        int x = 0;

        while (x < size)
        {
            int y = 0;

            while (y < size)
            {
                if (reachable[x, y])
                {
                    if (((x + y) & 1) == 0)
                    {
                        evenCount++;
                    }
                    else
                    {
                        oddCount++;
                    }
                }

                y++;
            }

            x++;
        }

        int difference = Math.Abs(evenCount - oddCount);

        if (difference > 1)
        {
            return false;
        }

        if (difference == 0)
        {
            return true;
        }

        int startColor = (start.X + start.Y) & 1;
        int majorityColor = evenCount > oddCount ? 0 : 1;
        return startColor == majorityColor;
    }

    private static int[,] CreateBoard(int size)
    {
        var board = new int[size, size];
        ResetBoard(board, size);
        return board;
    }

    private static void ResetBoard(int[,] board, int size)
    {
        int x = 0;

        while (x < size)
        {
            int y = 0;

            while (y < size)
            {
                board[x, y] = -1;
                y++;
            }

            x++;
        }
    }

    private static bool IsAvailable(int x, int y, int size, int[,] board, bool[,] reachable)
    {
        return IsInside(x, y, size) && reachable[x, y] && board[x, y] == -1;
    }

    private static bool IsInside(int x, int y, int size)
    {
        return x >= 0 && x < size && y >= 0 && y < size;
    }

    private static bool CanMove(int fromX, int fromY, int toX, int toY)
    {
        int deltaX = Math.Abs(fromX - toX);
        int deltaY = Math.Abs(fromY - toY);
        return (deltaX == 2 && deltaY == 1) || (deltaX == 1 && deltaY == 2);
    }

    private static int GetSeed(int size, Coordinate start, int attempt)
    {
        unchecked
        {
            int seed = 17;
            seed = (seed * 31) + size;
            seed = (seed * 31) + start.X;
            seed = (seed * 31) + start.Y;
            seed = (seed * 31) + attempt;
            return seed;
        }
    }

    private readonly record struct Candidate(int X, int Y, int Degree, int TieBreaker);
}
