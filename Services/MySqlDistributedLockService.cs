/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited. 
 * 
 * Proprietary and Confidential.
 */

using GestionProduccion.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using GestionProduccion.Data;
using MySqlConnector;

namespace GestionProduccion.Services
{
    public class MySqlDistributedLockService : IDistributedLockService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MySqlDistributedLockService> _logger;

        public MySqlDistributedLockService(AppDbContext context, ILogger<MySqlDistributedLockService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> AcquireLockAsync(string resourceKey, TimeSpan timeout, CancellationToken ct = default)
        {
            try
            {
                var conn = _context.Database.GetDbConnection();
                if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync(ct);

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT GET_LOCK(@key, @timeout)";
                
                var keyParam = new MySqlParameter("@key", resourceKey);
                var timeoutParam = new MySqlParameter("@timeout", (int)timeout.TotalSeconds);
                
                cmd.Parameters.Add(keyParam);
                cmd.Parameters.Add(timeoutParam);

                var result = await cmd.ExecuteScalarAsync(ct);
                return result != null && (long)result == 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error acquiring MySQL lock for {Key}", resourceKey);
                return false;
            }
        }

        public async Task ReleaseLockAsync(string resourceKey)
        {
            try
            {
                var conn = _context.Database.GetDbConnection();
                if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT RELEASE_LOCK(@key)";
                
                var keyParam = new MySqlParameter("@key", resourceKey);
                cmd.Parameters.Add(keyParam);

                await cmd.ExecuteScalarAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing MySQL lock for {Key}", resourceKey);
            }
        }
    }
}
