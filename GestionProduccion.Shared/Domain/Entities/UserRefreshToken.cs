using System.ComponentModel.DataAnnotations;

namespace GestionProduccion.Domain.Entities;

public class UserRefreshToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;
    
    [Required]
    [StringLength(255)]
    public required string Token { get; set; }
    
    public DateTime ExpiryDate { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
