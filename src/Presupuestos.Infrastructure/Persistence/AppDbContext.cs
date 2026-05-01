using Microsoft.EntityFrameworkCore;
using Presupuestos.Domain.Entities;

namespace Presupuestos.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<ServiceItem> ServiceItems => Set<ServiceItem>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<BudgetDetail> BudgetDetails => Set<BudgetDetail>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<AppConfig> AppConfigs => Set<AppConfig>();
    public DbSet<PricingRule> PricingRules => Set<PricingRule>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200);
            e.HasIndex(x => x.Name);
        });

        modelBuilder.Entity<PricingRule>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Type).HasConversion<int>();
            e.Property(x => x.Value).HasPrecision(18, 4);
            e.Property(x => x.Expression).HasMaxLength(4000);
            e.HasIndex(x => x.TenantId);
            e.HasOne(x => x.Tenant).WithMany(t => t.PricingRules).HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Email).HasMaxLength(320);
            e.Property(x => x.PasswordHash).HasMaxLength(500);
            e.Property(x => x.GlobalMarginPercent).HasPrecision(9, 4);
            e.Property(x => x.Role).HasConversion<int>();
            e.HasIndex(x => x.Email).IsUnique();
            e.HasOne(x => x.Tenant)
                .WithMany(t => t.Users)
                .HasForeignKey(x => x.TenantId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Token).HasMaxLength(500);
            e.HasIndex(x => x.Token);
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.DeviceRecordId);
            e.HasOne(x => x.User).WithMany(u => u.RefreshTokens).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Device).WithMany(d => d.RefreshTokens).HasForeignKey(x => x.DeviceRecordId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Device>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.DeviceId).HasMaxLength(200);
            e.Property(x => x.Name).HasMaxLength(200);
            e.HasIndex(x => new { x.UserId, x.DeviceId }).IsUnique();
            e.HasIndex(x => x.UserId);
            e.HasOne(x => x.User).WithMany(u => u.Devices).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AppConfig>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.MinimumVersion).HasMaxLength(50);
            e.Property(x => x.BlockedVersions).HasMaxLength(4000);
            e.Property(x => x.Message).HasMaxLength(2000);
        });

        modelBuilder.Entity<Item>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Unit).HasMaxLength(50);
            e.Property(x => x.CostPerUnit).HasPrecision(18, 4);
            e.HasIndex(x => x.TenantId);
            e.HasOne(x => x.Tenant).WithMany(t => t.Items).HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Service>(e =>
        {
            e.ToTable("Services");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.BasePrice).HasPrecision(18, 4);
            e.Property(x => x.MarginPercent).HasPrecision(9, 4);
            e.HasIndex(x => x.TenantId);
            e.HasOne(x => x.Tenant).WithMany(t => t.Services).HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ServiceItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.QuantityUsed).HasPrecision(18, 6);
            e.HasIndex(x => x.ServiceId);
            e.HasIndex(x => x.ItemId);
            e.HasOne(x => x.Service).WithMany(s => s.ServiceItems).HasForeignKey(x => x.ServiceId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Item).WithMany(i => i.ServiceItems).HasForeignKey(x => x.ItemId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Budget>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.TotalCost).HasPrecision(18, 4);
            e.Property(x => x.TotalPrice).HasPrecision(18, 4);
            e.HasIndex(x => x.TenantId);
            e.HasIndex(x => x.CreatedAt);
            e.HasOne(x => x.Tenant).WithMany(t => t.Budgets).HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BudgetDetail>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Quantity).HasPrecision(18, 4);
            e.Property(x => x.CalculatedCost).HasPrecision(18, 4);
            e.Property(x => x.CalculatedPrice).HasPrecision(18, 4);
            e.Property(x => x.ManualPriceOverride).HasPrecision(18, 4);
            e.HasIndex(x => x.BudgetId);
            e.HasIndex(x => x.ServiceId);
            e.HasOne(x => x.Budget).WithMany(b => b.Details).HasForeignKey(x => x.BudgetId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Service).WithMany(s => s.BudgetDetails).HasForeignKey(x => x.ServiceId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Plan>(e =>
        {
            e.ToTable("Plans");
            e.HasKey(x => x.Name);
            e.Property(x => x.Name).HasMaxLength(50);
            e.Property(x => x.Price).HasPrecision(18, 4);
            e.HasData(
                new Plan { Name = "Free", Price = 0m, DurationDays = 0 },
                new Plan { Name = "Pro", Price = 14900m, DurationDays = 30 },
                new Plan { Name = "Premium", Price = 29900m, DurationDays = 30 });
        });

        modelBuilder.Entity<Subscription>(e =>
        {
            e.ToTable("Subscriptions");
            e.HasKey(x => x.Id);
            e.Property(x => x.PlanName).HasMaxLength(50);
            e.Property(x => x.Status).HasMaxLength(32);
            e.Property(x => x.ExternalPaymentId).HasMaxLength(64);
            e.Property(x => x.PreferenceId).HasMaxLength(128);
            e.HasIndex(x => x.TenantId);
            e.HasIndex(x => x.ExternalPaymentId);
            e.HasIndex(x => x.PreferenceId);
            e.HasOne(x => x.Tenant)
                .WithMany(t => t.Subscriptions)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Plan)
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(x => x.PlanName)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
