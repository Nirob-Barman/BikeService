namespace BikeService.Application.DTOs.Mechanic
{
    public class MechanicDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Specialty { get; set; }
        public bool IsAvailable { get; set; }
        public string? UserId { get; set; }
        public string? LinkedEmail { get; set; }
        public bool IsLoginActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
