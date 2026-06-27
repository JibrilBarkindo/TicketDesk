using Microsoft.EntityFrameworkCore;
using TicketDesk.Api.Domain;

namespace TicketDesk.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Ticket> Tickets => Set<Ticket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var ticket = modelBuilder.Entity<Ticket>();

        ticket.HasKey(t => t.Id);
        ticket.Property(t => t.Title).IsRequired().HasMaxLength(200);
        ticket.Property(t => t.Description).IsRequired().HasMaxLength(2000);
        ticket.Property(t => t.AssignedTo).HasMaxLength(100);

        // Persist enums as readable strings rather than ints.
        ticket.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
        ticket.Property(t => t.Priority).HasConversion<string>().HasMaxLength(20);

        ticket.HasIndex(t => t.Status);
        ticket.HasIndex(t => t.Priority);
    }
}
