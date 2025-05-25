using Microsoft.EntityFrameworkCore;
using SignalRDemo.Server.Models;

namespace SignalRDemo.Server.Datas;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Vote> Votes { get; set; }
    public DbSet<VoteSubject> VoteSubjects { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Vote>()
            .HasMany(v => v.Subjects)
            .WithOne(s => s.Vote)
            .HasForeignKey(s => s.VoteId);

        modelBuilder.Entity<VoteSubject>()
            .HasMany(v => v.Voters)
            .WithOne(i => i.VoteSubject)
            .HasForeignKey(i => i.SubjectId);

        modelBuilder.Entity<VoteSubject>().ToTable("VoteSubjects");
        modelBuilder.Entity<VoteSubjectInput>().ToTable("VoteInputs");
    }
}