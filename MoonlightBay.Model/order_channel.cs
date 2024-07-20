
namespace MoonlightBay.Model;

//终端的频道
public class OrderChannel{
    public Guid? OrderChannelID{get; set;}
    public int? OrderChannelLevel{get; set;} //优先级
    public List<Order>? Orders{get; set;}
}

