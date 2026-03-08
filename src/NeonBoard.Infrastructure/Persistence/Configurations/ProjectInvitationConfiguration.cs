using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeonBoard.Domain.Projects;

namespace NeonBoard.Infrastructure.Persistence.Configurations;

public class ProjectInvitationConfiguration : IEntityTypeConfiguration<ProjectInvitation>
{
    public void Configure(EntityTypeBuilder<ProjectInvitation> builder)
    {
        builder.ToTable("ProjectInvitations");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.ProjectId)
            .IsRequired();

        builder.Property(i => i.Email)
            .IsRequired()
            .HasMaxLength(254);

        builder.Property(i => i.Role)
            .IsRequired();

        builder.Property(i => i.Token)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(i => i.Status)
            .IsRequired();

        builder.Property(i => i.InvitedByUserId)
            .IsRequired();

        builder.Property(i => i.ExpiresAt)
            .IsRequired();

        builder.Property(i => i.CreatedAt)
            .IsRequired();

        builder.HasIndex(i => i.Token)
            .IsUnique();

        builder.HasIndex(i => new { i.ProjectId, i.Email });

        builder.HasIndex(i => i.ProjectId);

        builder.HasOne<NeonBoard.Domain.Projects.Project>()
            .WithMany()
            .HasForeignKey(i => i.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<NeonBoard.Domain.Users.User>()
            .WithMany()
            .HasForeignKey(i => i.InvitedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
