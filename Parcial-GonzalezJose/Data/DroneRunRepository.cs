using Npgsql;
using ParcialGonzalezJose.Models;

namespace ParcialGonzalezJose.Data;

public sealed class DroneRunRepository
{
    private readonly string _connectionString;

    public DroneRunRepository(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("La cadena de conexion no puede estar vacia.", nameof(connectionString));
        }

        _connectionString = connectionString;
    }

    public int SaveSuccessfulRun(int size, int startX, int startY, IReadOnlyList<DroneStep> steps)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        using NpgsqlTransaction transaction = connection.BeginTransaction();

        try
        {
            int masterId = InsertMaster(connection, transaction, size, startX, startY);
            InsertDetails(connection, transaction, masterId, steps);
            transaction.Commit();
            return masterId;
        }
        catch
        {
            TryRollback(transaction);
            throw;
        }
    }

    public IReadOnlyList<PersistedStepReport> GetLastFiveReconstructedSteps(int masterId)
    {
        var report = new List<PersistedStepReport>();

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        using var command = new NpgsqlCommand(
            """
            SELECT id, paso_actual, posicion_x, posicion_y
            FROM tb_det_log
            WHERE id_master_control = @id_master_control
            ORDER BY id DESC
            LIMIT 5;
            """,
            connection);

        command.Parameters.AddWithValue("id_master_control", masterId);

        using NpgsqlDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            int logId = reader.GetInt32(0);
            int storedStep = reader.GetInt32(1);
            int realStep = ReconstructStep(storedStep);
            int x = reader.GetInt32(2);
            int y = reader.GetInt32(3);

            report.Add(new PersistedStepReport(logId, storedStep, realStep, x, y));
        }

        return report;
    }

    private static int InsertMaster(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        int size,
        int startX,
        int startY)
    {
        using var command = new NpgsqlCommand(
            """
            INSERT INTO tb_master_control (fecha_sistema, tamanio_terreno, despegue_x, despegue_y)
            VALUES (CURRENT_TIMESTAMP, @tamanio_terreno, @despegue_x, @despegue_y)
            RETURNING id;
            """,
            connection,
            transaction);

        command.Parameters.AddWithValue("tamanio_terreno", size);
        command.Parameters.AddWithValue("despegue_x", startX);
        command.Parameters.AddWithValue("despegue_y", startY);

        object? result = command.ExecuteScalar();

        if (result is null)
        {
            throw new InvalidOperationException("PostgreSQL no devolvio el ID de tb_master_control.");
        }

        return Convert.ToInt32(result);
    }

    private static void InsertDetails(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        int masterId,
        IReadOnlyList<DroneStep> steps)
    {
        int i = 0;

        while (i < steps.Count)
        {
            DroneStep step = steps[i];

            using var command = new NpgsqlCommand(
                """
                INSERT INTO tb_det_log (id_master_control, paso_actual, posicion_x, posicion_y)
                VALUES (@id_master_control, @paso_actual, @posicion_x, @posicion_y);
                """,
                connection,
                transaction);

            command.Parameters.AddWithValue("id_master_control", masterId);
            command.Parameters.AddWithValue("paso_actual", ObfuscateStep(step.StepNumber));
            command.Parameters.AddWithValue("posicion_x", step.X);
            command.Parameters.AddWithValue("posicion_y", step.Y);

            command.ExecuteNonQuery();
            i++;
        }
    }

    private static int ObfuscateStep(int stepNumber)
    {
        if (stepNumber % 2 == 0)
        {
            return stepNumber * 2;
        }

        return -stepNumber;
    }

    private static int ReconstructStep(int storedStep)
    {
        if (storedStep < 0)
        {
            return -storedStep;
        }

        return storedStep / 2;
    }

    private static void TryRollback(NpgsqlTransaction transaction)
    {
        try
        {
            transaction.Rollback();
        }
        catch
        {
            // If the connection is already broken, the original exception is the useful one.
        }
    }
}
