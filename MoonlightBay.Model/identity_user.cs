using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace MoonlightBay.Model;


public class ApplicationUser : IdentityUser<string>
{
    public ApplicationUser()
    {
        Id = Guid.NewGuid().ToString("D");
    }
    public ApplicationUser(string userName)
    {
        base.UserName = userName;
    }

   public string? Role{get; set; } //区分管理员和硬件

}

