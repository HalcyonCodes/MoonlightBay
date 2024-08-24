using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MoonlightBay.Data.Interfaces;
using MoonlightBay.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace MoonlightBay.Data.Repositories;



public class AccountRepository(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor
) : IAccountRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<ApplicationUser?> GetUserByUserNameAsync(string userName)
    {
        ApplicationUser? dbUser = await _dbContext.ApplicationUsers
            .FirstOrDefaultAsync(q => q.UserName == userName);
        return dbUser;
    }
}