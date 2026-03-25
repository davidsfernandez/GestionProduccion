namespace GestionProduccion.Models.DTOs;

public class AttendanceDto
{
    public int Id { get; set; }
    public int EmployeeProfileId { get; set; }
    public DateTime ClockIn { get; set; }
    public DateTime? ClockOut { get; set; }
    public decimal HoursWorked { get; set; }
    public string? Note { get; set; }
    public bool IsOngoing => ClockOut == null;
}

public class AttendanceStatusDto
{
    public bool IsClockedIn { get; set; }
    public DateTime? LastClockIn { get; set; }
    public decimal TodayHoursWorked { get; set; }
}