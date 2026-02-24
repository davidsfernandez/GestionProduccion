using System.Collections.Generic;

namespace GestionProduccion.Models.DTOs;

public class SewingTeamDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int MemberCount { get; set; }
    public List<UserDto> Members { get; set; } = new();
}
