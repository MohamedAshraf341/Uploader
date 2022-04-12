using Microsoft.EntityFrameworkCore;

namespace FileAPI.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public virtual DbSet<FileData> FileDatas { get; set; }
    }
}
