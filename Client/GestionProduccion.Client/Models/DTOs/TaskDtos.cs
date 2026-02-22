using System;
using System.Collections.Generic;

namespace GestionProduccion.Client.Models.DTOs;

public class TaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AssignedUserName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public DateTime? Deadline { get; set; }
    public double ProgressPercentage { get; set; }
}

public class CreateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int AssignedUserId { get; set; }
    public DateTime? Deadline { get; set; }
}