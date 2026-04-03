namespace DeviceTrackr.Api.Models;

public class Device
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public DeviceType Type { get; set; }
    public string OperatingSystem { get; set; } = string.Empty;
    public string OsVersion { get; set; } = string.Empty;
    public string Processor { get; set; } = string.Empty;
    public int RamAmountGb { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? AssignedUserId { get; set; }
    public User? AssignedUser { get; set; }
}
