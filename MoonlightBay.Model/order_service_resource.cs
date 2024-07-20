namespace MoonlightBay.Model;

//订单服务的资源
public class OrderServiceResource
{
    public int? OrderServiceResourceID { get; set; }
    public string? OrderServiceResourceName { get; set; }
    public string? OrderServiceResourceDesc { get; set; } //资源说明
    public DateTime? CreatedTime{get;set;}
}

