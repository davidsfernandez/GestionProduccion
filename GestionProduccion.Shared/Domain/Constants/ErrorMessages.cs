/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

namespace GestionProduccion.Domain.Constants;

public static class ErrorMessages
{
    public const string ElementNotFound = "Elemento no encontrado";
    public const string DuplicateCode = "CÃ³digo duplicado";
    public const string CannotDeleteByBusinessRules = "No se puede eliminar por reglas de negocio";
    public const string OrderAlreadyInProgress = "La orden ya estÃ¡ en progreso y no puede eliminarse";
}

