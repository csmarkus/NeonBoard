using NeonBoard.Domain.Boards;

namespace NeonBoard.UnitTests.Builders;

public class BoardBuilder
{
    private string _name = "Test Board";
    private Guid _projectId = Guid.NewGuid();
    private string? _prefix;
    private readonly List<string> _columns = [];
    private readonly List<(string ColumnName, string Title, string Description)> _cards = [];
    private readonly List<(string Name, string Color)> _labels = [];
    private readonly List<(int CardIndex, int LabelIndex)> _cardLabelAssignments = [];

    public BoardBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public BoardBuilder WithProjectId(Guid projectId)
    {
        _projectId = projectId;
        return this;
    }

    public BoardBuilder WithPrefix(string prefix)
    {
        _prefix = prefix;
        return this;
    }

    public BoardBuilder WithColumn(string name)
    {
        _columns.Add(name);
        return this;
    }

    public BoardBuilder WithColumns(params string[] names)
    {
        _columns.AddRange(names);
        return this;
    }

    public BoardBuilder WithCard(string columnName, string title, string description = "")
    {
        _cards.Add((columnName, title, description));
        return this;
    }

    public BoardBuilder WithLabel(string name, string color = LabelColors.Blue)
    {
        _labels.Add((name, color));
        return this;
    }

    public BoardBuilder WithCardLabel(int cardIndex, int labelIndex)
    {
        _cardLabelAssignments.Add((cardIndex, labelIndex));
        return this;
    }

    public Board Build()
    {
        var board = Board.Create(_name, _projectId, _prefix);
        board.ClearDomainEvents();

        foreach (var columnName in _columns)
        {
            board.AddColumn(columnName);
        }

        var cardIds = new List<Guid>();
        foreach (var (columnName, title, description) in _cards)
        {
            var column = board.Columns.First(c => c.Name == columnName);
            var cardId = board.AddCard(column.Id, title, description);
            cardIds.Add(cardId);
        }

        var labelIds = new List<Guid>();
        foreach (var (name, color) in _labels)
        {
            var labelId = board.AddLabel(name, color);
            labelIds.Add(labelId);
        }

        foreach (var (cardIndex, labelIndex) in _cardLabelAssignments)
        {
            board.AddLabelToCard(cardIds[cardIndex], labelIds[labelIndex]);
        }

        board.ClearDomainEvents();
        return board;
    }
}
