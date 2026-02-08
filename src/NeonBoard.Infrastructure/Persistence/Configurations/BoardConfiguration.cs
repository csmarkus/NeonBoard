using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeonBoard.Domain.Boards;
using NeonBoard.Domain.Boards.Entities;
using NeonBoard.Domain.Boards.ValueObjects;

namespace NeonBoard.Infrastructure.Persistence.Configurations;

public class BoardConfiguration : IEntityTypeConfiguration<Board>
{
    public void Configure(EntityTypeBuilder<Board> builder)
    {
        builder.ToTable("Boards");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.ProjectId)
            .IsRequired();

        builder.Property(b => b.CreatedAt)
            .IsRequired();

        builder.Property(b => b.UpdatedAt)
            .IsRequired();

        builder.HasIndex(b => b.ProjectId);

        builder.HasOne<NeonBoard.Domain.Projects.Project>()
            .WithMany()
            .HasForeignKey(b => b.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsMany(b => b.Columns, column =>
        {
            column.ToTable("Columns");

            column.WithOwner().HasForeignKey("BoardId");

            column.HasKey(c => c.Id);

            column.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(50);

            column.OwnsOne(c => c.Position, position =>
            {
                position.Property(p => p.Value)
                    .HasColumnName("Position")
                    .IsRequired();
            });

            column.Property(c => c.CreatedAt)
                .IsRequired();

            column.Navigation(c => c.Position).IsRequired();
        });

        builder.OwnsMany(b => b.Cards, card =>
        {
            card.ToTable("Cards");

            card.WithOwner().HasForeignKey("BoardId");

            card.HasKey(c => c.Id);

            card.Property(c => c.ColumnId)
                .IsRequired();

            card.OwnsOne(c => c.Content, content =>
            {
                content.Property(ct => ct.Title)
                    .HasColumnName("Title")
                    .IsRequired()
                    .HasMaxLength(200);

                content.Property(ct => ct.Description)
                    .HasColumnName("Description")
                    .IsRequired()
                    .HasMaxLength(5000);
            });

            card.OwnsOne(c => c.Position, position =>
            {
                position.Property(p => p.Value)
                    .HasColumnName("Position")
                    .IsRequired();
            });

            card.Property(c => c.CreatedAt)
                .IsRequired();

            card.Property(c => c.UpdatedAt)
                .IsRequired();

            card.HasIndex(c => c.ColumnId);

            card.Navigation(c => c.Content).IsRequired();
            card.Navigation(c => c.Position).IsRequired();
        });
    }
}
