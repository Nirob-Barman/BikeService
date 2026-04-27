namespace BikeService.Domain.Entities;

public class IntegrationSetting : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool IsEncrypted { get; set; }
}
