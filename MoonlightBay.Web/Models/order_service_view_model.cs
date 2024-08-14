


namespace MoonlightBay.Web.Models;


public class OrderServiceResourceViewModel{
    public int? orderServiceResourceID{get; set;}
    public string? orderServiceResourceName{get; set;}
    public string? orderServiceResourceDesc{get; set;}//资源说明
}

public class Test{
    public int? testInt{get; set;}
}

public class ListTest{
    public List<Test>? listTest{get; set;}
}


public  class OrderServiceResourcesResultViewModel{
    public string? code;
    public string? message;
    public List<OrderServiceResourceViewModel>? orderServiceResources{get; set;}
}

public class OrderServiceViewModel{
    public int? orderServiceID{get; set;}
    public string? orderServiceName{get; set;}
    public List<OrderServiceResourceViewModel>? orderServiceResources{get; set;}
    public DateTime? createdTime{get; set;}
    public string? orderServiceDesc{get; set;}
}

public class OrderServicesResultViewModel{
    public string? code;
    public string? message;
    public List<OrderServiceViewModel>? orderServices;
}

public class OrderServiceScriptViewModel{
    public int? orderServiceScriptID{get; set;}
    public string? orderServiceScriptName{get; set;}
    public string? orderServiceDesc{get; set;}
}


public class OrderServiceScriptsResultViewModel{
    public string? code;
    public string? message;
    public List<OrderServiceScriptViewModel>? orderServiceScripts{get; set;}
}
