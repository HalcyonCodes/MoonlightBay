namespace MoonlightBay.Web.Models;


public class OrderServiceUIResoultViewModel{
    public string? code{get; set;}
    public string? message{get; set;}
    public OrderServiceUIDataViewModel? data{get; set;}
}



public class OrderServiceUIDataViewModel{
    public List<OrderServiceUIViewModel>? orderServices {get; set;}

}



public class OrderServiceUIViewModel{
    public string? id {get; set;}
    public string? desc{get; set;}
    public string? name{get; set;}
}


public class  OrderServiceUIPageResultViewModel{
    public string? code{get; set;}
    public string? message{get; set;}
    public OrderServiceUIPageDataViewModel? data{get; set;}
}

public class OrderServiceUIPageDataViewModel{
    public List<OrderServiceUIPageViewModel>? orderServices {get; set;}
}


public class OrderServiceUIPageViewModel{
    public string? id {get; set;}
    public string? bindingCount {get; set;}
    public string? name {get; set;}
    public string? desc {get; set;}
    public List<string>? resources {get; set;}
    public string? workScript {get; set;}

}



public class OrderServiceUIWorkScriptViewModel{
    public string? id {get; set;}
    public string? bindingCount{get; set;}
    public string? name{get; set;}
    public string? desc {get; set;}
}

public class OrderServiceUIWorkScriptDataViewModel{
    public List<OrderServiceUIWorkScriptViewModel>? orderWorkScripts {get; set;}
}

public class OrderServiceUIWorkScriptResultViewModel{
    public string? code{get; set;}
    public string? message{get; set;}
    public OrderServiceUIWorkScriptDataViewModel? data{get; set;}
}

public class OrderServiceAddUIViewModel{

    public string? orderServiceName{get; set;}
    public string? orderServiceDesc{get; set;}
}

public class OrderServiceDeleteUIViewModel{
    public int? orderServiceID{get; set;}
}



public class OrderRequestLiteViewModel
{
    public int? orderChannelLevel { get; set; }
    public OrderLite? order { get; set; }
}

public class OrderLite
{
    public string? orderID { get; set; }
    public OrderServiceLite? orderService { get; set; }
    public string? sourceTerminalID { get; set; }
    public string? targetTerminalID { get; set; }
    public List<OrderServiceResourceLite>? orderServiceResources { get; set; }
}

public class OrderServiceLite
{
    public int? orderServiceID { get; set; }
}

public class OrderServiceResourceLite
{
    public int? resourceIntValue { get; set; }
    public string? resourceStringValue { get; set; }
    public double? resourceDoubleValue { get; set; }
}