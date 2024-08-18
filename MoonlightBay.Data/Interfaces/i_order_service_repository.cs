using MoonlightBay.Model;


namespace MoonlightBay.Data.Interfaces;



public interface IOrderServiceRepository
{


    //操作OrderServiceResource
    Task<int?> AddOrderServiceResourceAsync(OrderServiceResource orderServiceResource);
    Task<int> DeleteOrderServiceResourceByIDAsync(int? orderServiceResourceID);
    Task<int> UpdateOrderServiceResourceAsync(OrderServiceResource orderServiceResource);
    Task<OrderServiceResource?> GetOrderServiceResourceByNameAsync(string orderServiceResourceName);
    Task<OrderServiceResource?> GetOrderServiceResourceByIDAsync(int? orderServiceResourceID);
    Task<List<OrderServiceResource>?> GetOrderServiceResourcesAsync();
    

    //操作OrderService
    Task<int?> AddOrderServiceAsync(OrderService orderService);
    Task<int> DeleteOrderServiceByIDAsync(int? orderServiceID);
    Task<int> UpdateOrderServiceAsync(OrderService orderService);
    Task<OrderService?> GetOrderServiceByNameWithResourcesAsync(string orderServiceResourceName);
    Task<OrderService?> GetOrderServiceByIDWithResourcesAsync(int? orderServiceID);
    Task<List<OrderService>> GetOrderServicesAsync();
    Task<int> AddOrderServiceResourcesToOrderServiceAsync(OrderService orderService);
    Task<int> AddOrderServiceScriptToOrderServiceAsync(OrderService orderService);

    //操作OrderServiceScript
    Task<int?> AddOrderServiceScriptAsync(OrderServiceScript script);
    Task<int> DeleteOrderServiceScriptAsync(OrderServiceScript script);
    Task<int> UpdateOrderServiceSeriptAsync(OrderServiceScript script);
    Task<OrderServiceScript?> GetOrderServiceScriptAsync(int? scriptID);
    Task<List<OrderServiceScript>?> GetOrderServiceScriptsAsync();

    //操作OrderServiceClasses
    Task<int> DeleteOrderServiceClassAsync(Guid? orderServiceClassID);
    



}


