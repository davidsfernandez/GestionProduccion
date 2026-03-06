/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestionProduccion.Models.DTOs;

public class SewingTeamDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int MemberCount { get; set; }
    public List<UserDto> Members { get; set; } = new();
    public List<int> SelectedUserIds { get; set; } = new();
}

public class CreateSewingTeamRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "A equipe deve ter pelo menos um membro inicial.")]
    public List<int> InitialUserIds { get; set; } = new();
}

