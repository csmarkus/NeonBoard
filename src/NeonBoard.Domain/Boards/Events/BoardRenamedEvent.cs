using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Boards.Events;

public record BoardRenamedEvent(Guid BoardId, string OldName, string NewName) : IDomainEvent;
