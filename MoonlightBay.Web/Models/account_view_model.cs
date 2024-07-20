

namespace MoonlightBay.Web.Models;

public class LoginAPIModel
{
    public required string UserID { get; set; }
    public required string Password { get; set; }
}


public class RegisterApiModel{
    public required string UserID { get; set; }
    public required string Password { get; set; }
    public required string Role { get; set; } //Role分为“admin”和“machine”
}


