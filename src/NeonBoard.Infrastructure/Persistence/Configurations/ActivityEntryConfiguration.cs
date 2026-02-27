using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeonBoard.Domain.Boards.Activity;

namespace NeonBoard.Infrastructure.Persistence.Configurations;

public class ActivityEntryConfiguration : IEntityTypeConfiguration<ActivityEntry>
{
    public void Configure(EntityTypeBuilder<ActivityEntry> builder)
    {
        builder.ToTable("ActivityEntries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.BoardId)
            .IsRequired();

        builder.Property(e => e.EntityType)
            .IsRequired();

        builder.Property(e => e.EntityId)
            .IsRequired();

        builder.Property(e => e.ActionType)
            .IsRequired();

        var dataProperty = builder.Property(e => e.Data)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null)
                    ?? new Dictionary<string, object>())
            .IsRequired();

        dataProperty.Metadata.SetValueComparer(
            new ValueComparer<Dictionary<string, object>>(
                (c1, c2) => JsonSerializer.Serialize(c1, (JsonSerializerOptions?)null)
                    == JsonSerializer.Serialize(c2, (JsonSerializerOptions?)null),
                c => JsonSerializer.Serialize(c, (JsonSerializerOptions?)null).GetHashCode(),
                c => JsonSerializer.Deserialize<Dictionary<string, object>>(
                    JsonSerializer.Serialize(c, (JsonSerializerOptions?)null),
                    (JsonSerializerOptions?)null) ?? new Dictionary<string, object>()));

        builder.Property(e => e.OccurredAt)
            .IsRequired();

        // Board-level feed: paginate by OccurredAt DESC
        builder.HasIndex(e => new { e.BoardId, e.OccurredAt })
            .IsDescending(false, true)
            .HasDatabaseName("IX_ActivityEntries_BoardId_OccurredAt");

        // Card-level feed: filter by EntityId, paginate by OccurredAt DESC
        builder.HasIndex(e => new { e.EntityId, e.OccurredAt })
            .IsDescending(false, true)
            .HasDatabaseName("IX_ActivityEntries_EntityId_OccurredAt");

        // FK to Board with cascade delete
        builder.HasOne<NeonBoard.Domain.Boards.Board>()
            .WithMany()
            .HasForeignKey(e => e.BoardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
