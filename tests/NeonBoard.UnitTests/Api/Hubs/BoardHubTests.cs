using MediatR;
using Microsoft.AspNetCore.SignalR;
using NeonBoard.Api.Hubs;
using NeonBoard.Application.Cards.Commands.AddCard;
using NeonBoard.Application.Cards.Commands.MoveCard;
using NeonBoard.Application.Columns.Commands.AddColumn;
using NeonBoard.Application.Columns.Commands.DeleteColumn;
using NeonBoard.Application.Columns.Commands.MoveColumn;
using NeonBoard.Application.Columns.Commands.RenameColumn;
using NeonBoard.Application.Columns.Commands.ReorderColumns;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Common;
using NeonBoard.Domain.Projects;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace NeonBoard.UnitTests.Api.Hubs;

public class BoardHubTests
{
    private readonly IProjectAuthorizationService _authService = Substitute.For<IProjectAuthorizationService>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IBoardRepository _boardRepository = Substitute.For<IBoardRepository>();
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly BoardHub _hub;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _boardId = Guid.NewGuid();
    private readonly Guid _projectId = Guid.NewGuid();
    private readonly string _connectionId = Guid.NewGuid().ToString();

    public BoardHubTests()
    {
        _currentUserService.GetUserIdAsync(Arg.Any<CancellationToken>()).Returns(_userId);
        _authService.HasRoleAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<ProjectRole>(), Arg.Any<CancellationToken>())
            .Returns(true);

        _hub = new BoardHub(_authService, _currentUserService, _boardRepository, _sender);

        var mockContext = Substitute.For<HubCallerContext>();
        mockContext.ConnectionId.Returns(_connectionId);
        _hub.Context = mockContext;

        var mockGroups = Substitute.For<IGroupManager>();
        _hub.Groups = mockGroups;
    }

    private async Task JoinBoardForTest()
    {
        var board = CreateTestBoard();
        _boardRepository.GetBoardWithDetailsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(board);

        await _hub.JoinBoard(_boardId);
    }

    private NeonBoard.Domain.Boards.Board CreateTestBoard()
    {
        var board = NeonBoard.Domain.Boards.Board.Create("Test Board", _projectId, "TST");

        // Set the Id via reflection since it's protected init
        var idProperty = typeof(NeonBoard.Domain.Common.Entity).GetProperty("Id");
        idProperty?.SetValue(board, _boardId);

        return board;
    }

    // --- MoveCard Tests ---

    [Fact]
    public async Task MoveCard_WhenConnected_ShouldDispatchCommand()
    {
        await JoinBoardForTest();
        var cardId = Guid.NewGuid();
        var targetColumnId = Guid.NewGuid();
        var newPosition = "a1";

        await _hub.MoveCard(cardId, targetColumnId, newPosition);

        await _sender.Received(1).Send(
            Arg.Is<MoveCardCommand>(cmd =>
                cmd.ProjectId == _projectId &&
                cmd.BoardId == _boardId &&
                cmd.CardId == cardId &&
                cmd.TargetColumnId == targetColumnId &&
                cmd.NewPosition == newPosition),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task MoveCard_WhenNotJoined_ShouldThrowHubException()
    {
        var act = () => _hub.MoveCard(Guid.NewGuid(), Guid.NewGuid(), "a1");

        await act.Should().ThrowAsync<HubException>()
            .WithMessage("*JoinBoard*");
    }

    [Fact]
    public async Task MoveCard_WhenNotEditor_ShouldThrowHubException()
    {
        await JoinBoardForTest();
        _authService.HasRoleAsync(_projectId, _userId, ProjectRole.Editor, Arg.Any<CancellationToken>())
            .Returns(false);

        var act = () => _hub.MoveCard(Guid.NewGuid(), Guid.NewGuid(), "a1");

        await act.Should().ThrowAsync<HubException>()
            .WithMessage("*permission*");
    }

    [Fact]
    public async Task MoveCard_WhenDomainException_ShouldThrowHubException()
    {
        await JoinBoardForTest();
        _sender.Send(Arg.Any<MoveCardCommand>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new DomainException("Card not found in board"));

        var act = () => _hub.MoveCard(Guid.NewGuid(), Guid.NewGuid(), "a1");

        await act.Should().ThrowAsync<HubException>()
            .WithMessage("Card not found in board");
    }

    // --- AddCard Tests ---

    [Fact]
    public async Task AddCard_WhenConnected_ShouldDispatchCommand()
    {
        await JoinBoardForTest();
        var columnId = Guid.NewGuid();

        await _hub.AddCard(columnId, "New Card", "Description");

        await _sender.Received(1).Send(
            Arg.Is<AddCardCommand>(cmd =>
                cmd.ProjectId == _projectId &&
                cmd.BoardId == _boardId &&
                cmd.ColumnId == columnId &&
                cmd.Title == "New Card" &&
                cmd.Description == "Description"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddCard_WhenNotJoined_ShouldThrowHubException()
    {
        var act = () => _hub.AddCard(Guid.NewGuid(), "Title", "Desc");

        await act.Should().ThrowAsync<HubException>()
            .WithMessage("*JoinBoard*");
    }

    // --- AddColumn Tests ---

    [Fact]
    public async Task AddColumn_WhenConnected_ShouldDispatchCommand()
    {
        await JoinBoardForTest();

        await _hub.AddColumn("New Column");

        await _sender.Received(1).Send(
            Arg.Is<AddColumnCommand>(cmd =>
                cmd.ProjectId == _projectId &&
                cmd.BoardId == _boardId &&
                cmd.Name == "New Column"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddColumn_WhenNotJoined_ShouldThrowHubException()
    {
        var act = () => _hub.AddColumn("Test");

        await act.Should().ThrowAsync<HubException>()
            .WithMessage("*JoinBoard*");
    }

    // --- RenameColumn Tests ---

    [Fact]
    public async Task RenameColumn_WhenConnected_ShouldDispatchCommand()
    {
        await JoinBoardForTest();
        var columnId = Guid.NewGuid();

        await _hub.RenameColumn(columnId, "Renamed Column");

        await _sender.Received(1).Send(
            Arg.Is<RenameColumnCommand>(cmd =>
                cmd.ProjectId == _projectId &&
                cmd.BoardId == _boardId &&
                cmd.ColumnId == columnId &&
                cmd.NewName == "Renamed Column"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RenameColumn_WhenNotJoined_ShouldThrowHubException()
    {
        var act = () => _hub.RenameColumn(Guid.NewGuid(), "New Name");

        await act.Should().ThrowAsync<HubException>()
            .WithMessage("*JoinBoard*");
    }

    // --- MoveColumn Tests ---

    [Fact]
    public async Task MoveColumn_WhenConnected_ShouldDispatchCommand()
    {
        await JoinBoardForTest();
        var columnId = Guid.NewGuid();

        await _hub.MoveColumn(columnId, "a2");

        await _sender.Received(1).Send(
            Arg.Is<MoveColumnCommand>(cmd =>
                cmd.ProjectId == _projectId &&
                cmd.BoardId == _boardId &&
                cmd.ColumnId == columnId &&
                cmd.NewPosition == "a2"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task MoveColumn_WhenNotJoined_ShouldThrowHubException()
    {
        var act = () => _hub.MoveColumn(Guid.NewGuid(), "a1");

        await act.Should().ThrowAsync<HubException>()
            .WithMessage("*JoinBoard*");
    }

    // --- DeleteColumn Tests ---

    [Fact]
    public async Task DeleteColumn_WhenConnected_ShouldDispatchCommand()
    {
        await JoinBoardForTest();
        var columnId = Guid.NewGuid();

        await _hub.DeleteColumn(columnId);

        await _sender.Received(1).Send(
            Arg.Is<DeleteColumnCommand>(cmd =>
                cmd.ProjectId == _projectId &&
                cmd.BoardId == _boardId &&
                cmd.ColumnId == columnId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteColumn_WhenNotJoined_ShouldThrowHubException()
    {
        var act = () => _hub.DeleteColumn(Guid.NewGuid());

        await act.Should().ThrowAsync<HubException>()
            .WithMessage("*JoinBoard*");
    }

    [Fact]
    public async Task DeleteColumn_WhenDomainException_ShouldThrowHubException()
    {
        await JoinBoardForTest();
        _sender.Send(Arg.Any<DeleteColumnCommand>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new DomainException("Cannot delete the last column"));

        var act = () => _hub.DeleteColumn(Guid.NewGuid());

        await act.Should().ThrowAsync<HubException>()
            .WithMessage("Cannot delete the last column");
    }

    // --- ReorderColumns Tests ---

    [Fact]
    public async Task ReorderColumns_WhenConnected_ShouldDispatchCommand()
    {
        await JoinBoardForTest();
        var columnIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        await _hub.ReorderColumns(columnIds);

        await _sender.Received(1).Send(
            Arg.Is<ReorderColumnsCommand>(cmd =>
                cmd.ProjectId == _projectId &&
                cmd.BoardId == _boardId &&
                cmd.ColumnIds.SequenceEqual(columnIds)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReorderColumns_WhenNotJoined_ShouldThrowHubException()
    {
        var act = () => _hub.ReorderColumns([Guid.NewGuid()]);

        await act.Should().ThrowAsync<HubException>()
            .WithMessage("*JoinBoard*");
    }

    [Fact]
    public async Task ReorderColumns_WhenNotEditor_ShouldThrowHubException()
    {
        await JoinBoardForTest();
        _authService.HasRoleAsync(_projectId, _userId, ProjectRole.Editor, Arg.Any<CancellationToken>())
            .Returns(false);

        var act = () => _hub.ReorderColumns([Guid.NewGuid()]);

        await act.Should().ThrowAsync<HubException>()
            .WithMessage("*permission*");
    }
}
