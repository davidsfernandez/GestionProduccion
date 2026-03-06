/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

using GestionProduccion.Domain.Entities;
using GestionProduccion.Models.DTOs;

namespace GestionProduccion.Services.Interfaces;

public interface IQAService
{
    Task<QADefect> RegisterDefectAsync(CreateQADefectDto dto, IFormFile? photoFile = null);
    Task<List<QADefect>> GetDefectsByOrderAsync(int orderId);
    Task DeleteDefectAsync(int id);
}

