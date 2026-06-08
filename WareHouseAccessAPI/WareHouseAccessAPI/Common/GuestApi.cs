namespace WarehouseAccessAPI.Common;

public class GuestApi
{
    public const string SectionName = "GuestApi";
    public string BaseUrl { get; set; } = "http://192.168.0.38:8000";
    public int TimeoutSeconds { get; set; } = 8;
}
