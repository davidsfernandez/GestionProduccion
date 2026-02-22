using GestionProduccion.Domain.Entities;

namespace GestionProduccion.Domain.Interfaces.Repositories;

public interface IUserRefreshTokenRepository
{
    Task AddAsync(UserRefreshToken token);
    Task<UserRefreshToken?> GetByTokenAsync(string token);
    Task RevokeAllUserTokensAsync(int userId);
    Task UpdateAsync(UserRefreshToken token);
}
