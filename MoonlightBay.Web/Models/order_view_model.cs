
namespace MoonlightBay.Web.Models;

public class OrderResultViewModel{
    public string? code;
    public string? message;
    public OrderViewModel? order;
}


public class OrderServiceResourceClassesViewModel{
    public Guid? orderServiceResourceClasssID {get; set;}
    public DateTime? createdTime{get; set;}
    public OrderServiceResourceViewModel? orderServiceResource{get; set;}
    public int? resourceIntValue{get; set;}
    public string? resourceStringValue{get; set;}
    public double? resourceDoubleValue{get; set;}

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
    public Guid? orderChannelID{get; set;}
    public int? orderChannelLevel{get; set;}
    public List<OrderViewModel>? orders{get; set;}
}

public class OrderChannelsResultViewModel{
    public string? code;
    public string? message;
    public List<OrderChannelViewModel>? orderChannels;
}




