using AcxiomCRM.Models.Domain;
using AcxiomCRM.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Customer>    Customers    { get; set; }
        public DbSet<Lead>        Leads        { get; set; }
        public DbSet<Opportunity> Opportunities { get; set; }
        public DbSet<FollowUp>    FollowUps    { get; set; }
        public DbSet<Activity>    Activities   { get; set; }
        public DbSet<AuditLog>    AuditLogs    { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Unique email + phone per customer
            builder.Entity<Customer>().HasIndex(c => c.Email).IsUnique();
            builder.Entity<Customer>().HasIndex(c => c.Phone).IsUnique();

            // Decimal precision
            builder.Entity<Lead>()
                .Property(l => l.ExpectedValue)
                .HasColumnType("decimal(18,2)");

            // FollowUp FK — no cascade to avoid multiple cascade paths
            builder.Entity<FollowUp>()
                .HasOne(f => f.Customer)
                .WithMany(c => c.FollowUps)
                .HasForeignKey(f => f.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<FollowUp>()
                .HasOne(f => f.Lead)
                .WithMany(l => l.FollowUps)
                .HasForeignKey(f => f.LeadId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<FollowUp>()
                .HasOne(f => f.Opportunity)
                .WithMany(o => o.FollowUps)
                .HasForeignKey(f => f.OpportunityId)
                .OnDelete(DeleteBehavior.Restrict);

            // Activity FK
            builder.Entity<Activity>()
                .HasOne(a => a.Customer)
                .WithMany(c => c.Activities)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Activity>()
                .HasOne(a => a.Lead)
                .WithMany(l => l.Activities)
                .HasForeignKey(a => a.LeadId)
                .OnDelete(DeleteBehavior.Restrict);

            // Opportunity → Lead
            builder.Entity<Opportunity>()
                .HasOne(o => o.Lead)
                .WithMany()
                .HasForeignKey(o => o.LeadId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
