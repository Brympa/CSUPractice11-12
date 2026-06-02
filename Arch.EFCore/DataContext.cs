using Microsoft.EntityFrameworkCore;

namespace Arch.EFCore;

public class DataContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=app.db");
        optionsBuilder.LogTo(Console.WriteLine);
        optionsBuilder.EnableSensitiveDataLogging();
        base.OnConfiguring(optionsBuilder);
    }
    
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Note> Notes => Set<Note>();
}