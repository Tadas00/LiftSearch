using System.Data.Common;
using LiftSearch.Data.Entities;
using LiftSearch.Data.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace LiftSearch.Data;

public class LsDbContext : DbContext
{
    private readonly IConfiguration _configuration;
    public DbSet<Driver> Drivers { get; set; }
    public DbSet<Trip> Trips { get; set; }
    public DbSet<Passenger> Passengers { get; set; }
    
    public DbSet<User> Users { get; set; }

    public LsDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_configuration.GetConnectionString("PostgreSQL"));
    }
    
}