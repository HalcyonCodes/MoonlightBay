using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using MoonlightBay.Model;

using Microsoft.AspNetCore.Identity;

namespace MoonlightBay.Data;


public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<IdentityUser>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        //builder.Entity<Terminal>().HasKey(t => t.TerminalID);
        builder.Entity<ApplicationUser>().HasKey(t => t.Id);
        builder.Entity<Terminal>().HasKey(t => t.TerminalID);
        builder.Entity<Order>().HasKey(t => t.OrderID);
        builder.Entity<OrderChannel>().HasKey(t => t.OrderChannelID);
        builder.Entity<OrderService>().HasKey(t => t.OrderServiceID);
        builder.Entity<OrderServiceScript>().HasKey(t => t.OrderServiceScriptID);
        builder.Entity<OrderServiceResource>().HasKey(t => t.OrderServiceResourceID);
        builder.Entity<OrderServiceResourceClass>().HasKey(t => t.OrderServiceResoourceClassID);
        base.OnModelCreating(builder);
    }

    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<Terminal> Terminals { get; set; }
    public DbSet<Order> Orders{ get; set; }
    public DbSet<OrderChannel> OrderChannels { get; set; }
    public DbSet<OrderService> OrderServices{ get; set; }
    public DbSet<OrderServiceScript> OrderServiceScripts{ get; set; }
    public DbSet<OrderServiceResource> OrderServiceResources{ get; set; }
    public DbSet<OrderServiceResourceClass> OrderServiceResourceClasses{ get; set; }

}


