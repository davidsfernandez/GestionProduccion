using GestionProduccion.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Data;

namespace GestionProduccion.Services;

public class MySqlDistributedLockService : IDistributedLockService
{
    private readonly string _connectionString;
    private readonly ILogger<MySqlDistributedLockService> _logger;

    public MySqlDistributedLockService(IConfiguration configuration, ILogger<MySqlDistributedLockService> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        _logger = logger;
    }

    public async Task<IDisposable?> AcquireLockAsync(string lockName, int timeoutSeconds = 10)
    {
        var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT GET_LOCK(@lockName, @timeout)";
        command.Parameters.AddWithValue("@lockName", lockName);
        command.Parameters.AddWithValue("@timeout", timeoutSeconds);

        var result = await command.ExecuteScalarAsync();

        if (result != null && result != DBNull.Value && Convert.ToInt32(result) == 1)
        {
            _logger.LogInformation("Acquired MySQL distributed lock: {LockName}", lockName);
            return new MySqlLockHandle(connection, lockName, _logger);
        }

        await connection.CloseAsync();
        _logger.LogWarning("Failed to acquire MySQL distributed lock: {LockName}", lockName);
        return null;
    }

    private class MySqlLockHandle : IDisposable
    {
        private readonly MySqlConnection _connection;
        private readonly string _lockName;
        private readonly ILogger _logger;
        private bool _disposed;

        public MySqlLockHandle(MySqlConnection connection, string lockName, ILogger logger)
        {
            _connection = connection;
            _lockName = lockName;
            _logger = logger;
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = "SELECT RELEASE_LOCK(@lockName)";
                command.Parameters.AddWithValue("@lockName", _lockName);
                command.ExecuteNonQuery();
                
                _connection.Close();
                _connection.Dispose();
                _logger.LogInformation("Released MySQL distributed lock: {LockName}", _lockName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing MySQL distributed lock: {LockName}", _lockName);
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}
