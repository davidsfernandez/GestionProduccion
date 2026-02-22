namespace GestionProduccion.Models.DTOs;

public class SewingTeamDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int MemberCount { get; set; }
    public List<UserSummaryDto> Members { get; set; } = new List<UserSummaryDto>();
}

public class CreateSewingTeamDto
{
    public string Name { get; set; } = string.Empty;
    public List<int> MemberIds { get; set; } = new List<int>();
}

public class UserSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
