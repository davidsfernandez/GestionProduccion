/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

namespace GestionProduccion.Models.DTOs;

public class RankingEntryDto
{
    public string UserName { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public int CompletedTasks { get; set; }
    public int CompletedOrders { get; set; }
    public int AdministrativeTasks { get; set; }
    public double Score { get; set; }
}


