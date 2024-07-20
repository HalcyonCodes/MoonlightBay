

namespace MoonlightBay.Model;


//终端的订单
public class Order{

    public enum OrderStatus{
        waiting = -1,
        working = 1,
        locking = 0, //冻结
        complete = 2
    }

    public Guid? OraderID{get; set;}
    public Terminal? SourceTerminal{get; set;} //发起终端
    public Terminal? TargetTerminal{get; set;} //目标终端
    public DateTime? CreatedTime{get; set;} //创建时间
    public OrderService? OrderService{get; set;} //订单内容
    public List<OrderServiceResourceClass>? OrderResources{get; set;} //订单资源和参数
    public OrderStatus? Status{get; set;} //订单状态
}
