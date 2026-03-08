using System.Text.Json.Serialization;

namespace NeonBoard.Domain.Projects;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InvitationStatus
{
    Pending,
    Accepted,
    Expired,
    Revoked
}
