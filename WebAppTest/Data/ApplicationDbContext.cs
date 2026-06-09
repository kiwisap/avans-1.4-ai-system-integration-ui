using Microsoft.EntityFrameworkCore;
using WebAppTest.Models;

namespace WebAppTest.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<WeatherForecast> WeatherForecast { get; set; }
    }
}
