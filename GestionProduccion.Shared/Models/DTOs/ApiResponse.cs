/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited. 
 * 
 * Proprietary and Confidential.
 */

namespace GestionProduccion.Models.DTOs
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();

        public static ApiResponse<T> SuccessResult(T data, string? message = null)
        {
            return new ApiResponse<T> { Success = true, Data = data, Message = message };
        }

        public static ApiResponse<T> FailureResult(string message, List<string>? errors = null)
        {
            return new ApiResponse<T> 
            { 
                Success = false, 
                Message = message, 
                Errors = errors ?? new List<string>() 
            };
        }
    }
}
