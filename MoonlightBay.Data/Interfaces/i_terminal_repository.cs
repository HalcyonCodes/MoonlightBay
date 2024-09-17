

using MoonlightBay.Model;


namespace MoonlightBay.Data.Interfaces;

public interface ITerminalRepository{

    //增加一个终端
    Task<Guid?> AddAysnc(Terminal terminal);

    //删除一个终端
    Task<int> DeleteAsync(Guid terminalID);

    //更新一个客户端
    Task<int> UpdateAsync(Terminal terminal);

    //通过UserID获取User的所有客户端,返回空时表示没有用户
    Task<List<Terminal>?> GetUserTerminalsAsync();

    //终端初始化，并添加给用户
    Task<int> TerminalInitAsync(Guid? terminalID);


    //获取指定页面 12个item为1页
    Task<List<Terminal>?> GetTerminalsAsync(int page);



}