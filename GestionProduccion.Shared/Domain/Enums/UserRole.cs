/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

namespace GestionProduccion.Domain.Enums;

/// <summary>
/// Defines user roles in the system.
/// </summary>
public enum UserRole
{
    Administrator = 0, // Role with all permissions
    Leader = 1,        // Team leader role
    Operational = 2,   // Production operator role
    Office = 3         // Administrative/Office role
}

