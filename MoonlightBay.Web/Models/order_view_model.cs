
namespace MoonlightBay.Web.Models;

public class OrderServiceResourceClassesViewModel{
    public Guid? orderServiceResourceClasssID;
    public DateTime? createdTime;
    public OrderServiceResourceViewModel? orderServiceResource;
    public int? resourceIntValue;
    public string? resourceStringValue;
    public double? resourceDoubleValue;

}


public class OrderViewModel{
    public enum OrderStatus{
        waiting = -1,
        working = 1,
        locking = 0, //冻结
        complete = 2
    }
    public Guid? orderID;
    public OrderServiceViewModel? orderService;
    public DateTime? createdTime;
    public Guid? sourceTerminalID;
    public Guid? targetTerminalID;
    public OrderStatus? status;
    public List<OrderServiceResourceClassesViewModel>? orderServiceResources;
}

public class OrderChannelViewModel{
    public Guid? orderChannelID;
    public int? orderChannelLevel;
    public List<OrderViewModel>? orders;
}

public class OrderChannelsResultViewModel{
    public string? code;
    public string? message;
    public List<OrderChannelViewModel>? orderChannels;
}




