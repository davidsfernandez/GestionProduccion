using GestionProduccion.Domain.Entities;

namespace GestionProduccion.Domain.Interfaces.Repositories;

public interface IPasswordResetTokenRepository
{
    Task AddAsync(PasswordResetToken token);
    Task<PasswordResetToken?> GetByHashAsync(string hash);
    Task UpdateAsync(PasswordResetToken token);
}
