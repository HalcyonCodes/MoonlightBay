

using MoonlightBay.Model;


namespace MoonlightBay.Data.Interfaces;

public interface IOrderRepository{

   Task<Guid?> AddOrderAsync(Order order);
   Task<int> DeleteOrderAsync(Order order);
   Task<int> UpdateOrderAsync(Order order);
   Task<List<OrderChannel>?> GetOrderChannelByTerminalIDAsync(Guid? terminalID);
   Task<int> UpdateOrderStatusAsync(Order order);

   Task<int> AddOrderToOrderChannelAsync(Guid orderID, int channelLevel);
   Task<Order?> GetOrderAsync();
   Task<List<OrderChannel>?> GetOrderChannelsAsync();
   Task<Order?> GetOrderByIDAsync(Guid? orderID);
}



