using GestionProduccion.Services.Interfaces;
using System.Collections.Concurrent;

namespace GestionProduccion.Services;

/// <summary>
/// Simple in-memory implementation of IDistributedLockService for testing environments.
/// </summary>
public class InMemoryDistributedLockService : IDistributedLockService
{
    private static readonly ConcurrentDictionary<string, byte> _locks = new();

    public async Task<IDisposable?> AcquireLockAsync(string lockName, int timeoutSeconds = 10)
    {
        var endTime = DateTime.UtcNow.AddSeconds(timeoutSeconds);
        
        while (DateTime.UtcNow < endTime)
        {
            if (_locks.TryAdd(lockName, 0))
            {
                return new LockHandle(lockName);
            }
            await Task.Delay(50);
        }

        return null;
    }

    private class LockHandle : IDisposable
    {
        private readonly string _lockName;
        public LockHandle(string lockName) => _lockName = lockName;
        public void Dispose() => _locks.TryRemove(_lockName, out _);
    }
}
