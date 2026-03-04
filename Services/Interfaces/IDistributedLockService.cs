namespace GestionProduccion.Services.Interfaces;

public interface IDistributedLockService
{
    /// <summary>
    /// Acquires a named lock.
    /// </summary>
    /// <param name="lockName">Unique name for the lock.</param>
    /// <param name="timeoutSeconds">Max time to wait for the lock.</param>
    /// <returns>A disposable lock handle if successful, otherwise null.</returns>
    Task<IDisposable?> AcquireLockAsync(string lockName, int timeoutSeconds = 10);
}
