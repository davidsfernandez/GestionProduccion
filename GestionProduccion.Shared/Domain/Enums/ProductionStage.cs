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
/// Defines the possible stages in the production workflow.
/// </summary>
public enum ProductionStage
{
    Cutting,
    Sewing,
    Review,
    Packaging
}


