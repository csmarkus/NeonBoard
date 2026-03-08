using System.Text.Json.Serialization;

namespace NeonBoard.Domain.Projects;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProjectRole
{
    Viewer = 0,
    Editor = 1,
    Owner = 2
}
