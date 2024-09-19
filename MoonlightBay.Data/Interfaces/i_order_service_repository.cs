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
    Task<List<OrderServiceResource>?> GetOrderServiceResourcesPageAsync(int pageIndex);
    Task<int> GetOrderServiceResourcesBindingCountAsync(int? orderServiceID);
    

    //操作OrderService
    Task<int?> AddOrderServiceAsync(OrderService orderService);
    Task<int> DeleteOrderServiceByIDAsync(int? orderServiceID);
    Task<int> UpdateOrderServiceAsync(OrderService orderService);
    Task<OrderService?> GetOrderServiceByNameWithResourcesAsync(string orderServiceResourceName);
    Task<OrderService?> GetOrderServiceByIDWithResourcesAsync(int? orderServiceID);
    Task<List<OrderService>> GetOrderServicesAsync();
    Task<int> AddOrderServiceResourcesToOrderServiceAsync(OrderService orderService);
    Task<List<OrderService>?> GetOrderServicesByResourcesAsync(int? orderServiceResourceID);
    Task<List<OrderService>?> GetOrderServicesByPageIndexAsync(int pageIndex);
    Task<int> GetOrderServiceBindingCountAsync(int? orderServiceID);

    //操作OrderServiceScript
    Task<int?> AddOrderServiceScriptAsync(OrderServiceScript script);
    Task<int> DeleteOrderServiceScriptAsync(OrderServiceScript script);
    Task<int> UpdateOrderServiceSeriptAsync(OrderServiceScript script);
    Task<OrderServiceScript?> GetOrderServiceScriptAsync(int? scriptID);
    Task<List<OrderServiceScript>?> GetOrderServiceScriptsAsync();
    Task<int> AddOrderServiceScriptToOrderServiceAsync(OrderService orderService);
    Task<int> GetOrderServiceScriptBindingCountAsync(int? scriptID);
   

    //操作OrderServiceClasses
    Task<int> DeleteOrderServiceClassAsync(Guid? orderServiceClassID);
    



}


