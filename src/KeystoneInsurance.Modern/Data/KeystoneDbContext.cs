using KeystoneInsurance.Modern.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KeystoneInsurance.Modern.Data;

public class KeystoneDbContext : DbContext
{
    public KeystoneDbContext(DbContextOptions<KeystoneDbContext> options)
        : base(options) { }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<UnderwritingDecision> UnderwritingDecisions => Set<UnderwritingDecision>();
    public DbSet<Policy> Policies => Set<Policy>();
    public DbSet<Endorsement> Endorsements => Set<Endorsement>();
    public DbSet<RateFactor> RateFactors => Set<RateFactor>();
    public DbSet<Coverage> CoverageOptions => Set<Coverage>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KeystoneDbContext).Assembly);
    }
}
