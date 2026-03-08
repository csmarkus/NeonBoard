using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeonBoard.Domain.Projects;
using NeonBoard.Domain.Projects.Entities;

namespace NeonBoard.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(p => p.OwnerId)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired();

        builder.Property(p => p.ShortId)
            .HasMaxLength(7)
            .IsRequired();

        builder.HasIndex(p => p.ShortId)
            .IsUnique();

        builder.HasIndex(p => p.OwnerId);

        builder.OwnsMany(p => p.Members, member =>
        {
            member.ToTable("ProjectMembers");

            member.WithOwner().HasForeignKey("ProjectId");

            member.HasKey(m => m.Id);

            member.Property(m => m.Id)
                .ValueGeneratedNever();

            member.Property(m => m.UserId)
                .IsRequired();

            member.Property(m => m.Role)
                .IsRequired();

            member.Property(m => m.JoinedAt)
                .IsRequired();

            member.HasIndex("ProjectId", "UserId")
                .IsUnique();

            member.HasIndex(m => m.UserId);
        });
    }
}
