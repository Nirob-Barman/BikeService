namespace BikeService.Domain.Entities;

public class AppNotification : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Link { get; set; }
    public bool IsRead { get; set; }

    public string UserId { get; set; } = string.Empty;
}
