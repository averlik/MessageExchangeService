using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
    

    // Добавьте DbSet для моделей
    public DbSet<Message> Messages { get; set; }
}
