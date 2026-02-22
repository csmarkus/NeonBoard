namespace NeonBoard.Domain.Common;

public static class DomainMessages
{
    #region Board

    public const string BoardNameEmpty = "Board name cannot be empty.";
    public const string BoardNameTooLong = "Board name cannot exceed 100 characters.";
    public const string BoardProjectIdEmpty = "Project ID cannot be empty.";
    public const string BoardColumnCountMismatch = "Column count mismatch. All columns must be included in the reorder.";
    public const string BoardCannotDeleteColumnWithCards = "Cannot delete column with cards. Specify a target column to move cards to.";
    public const string BoardTargetPositionNegative = "Target position cannot be negative.";
    public const string BoardPrefixEmpty = "Board prefix cannot be empty.";
    public const string BoardPrefixInvalid = "Board prefix must be 2-5 uppercase letters (A-Z).";

    public static string ColumnNotFound(Guid columnId) =>
        $"Column with ID {columnId} not found.";

    public static string CardNotFound(Guid cardId) =>
        $"Card with ID {cardId} not found.";

    public static string LabelNotFound(Guid labelId) =>
        $"Label with ID {labelId} not found.";

    #endregion

    #region Column

    public const string ColumnNameEmpty = "Column name cannot be empty.";
    public const string ColumnNameTooLong = "Column name cannot exceed 50 characters.";

    #endregion

    #region Card

    public const string CardColumnIdEmpty = "Column ID cannot be empty.";
    public const string CardNumberInvalid = "Card number must be a positive integer.";
    public const string CardLabelAlreadyAssigned = "This label is already assigned to the card.";
    public const string CardLabelNotAssigned = "This label is not assigned to the card.";
    public const string CardAlreadyArchived = "Card is already archived.";
    public const string CardNotArchived = "Card is not archived.";

    #endregion

    #region Label

    public const string LabelNameEmpty = "Label name cannot be empty.";
    public const string LabelNameTooLong = "Label name cannot exceed 50 characters.";
    public const string LabelColorEmpty = "Label color cannot be empty.";

    public static string LabelColorInvalid(string color) =>
        $"'{color}' is not a valid label color.";

    #endregion

    #region Position

    public const string PositionNegative = "Position cannot be negative.";

    #endregion

    #region CardContent

    public const string CardTitleEmpty = "Card title cannot be empty.";
    public const string CardTitleTooLong = "Card title cannot exceed 200 characters.";
    public const string CardDescriptionTooLong = "Card description cannot exceed 5000 characters.";

    #endregion

    #region Project

    public const string ProjectNameEmpty = "Project name cannot be empty.";
    public const string ProjectNameTooLong = "Project name cannot exceed 100 characters.";
    public const string ProjectDescriptionTooLong = "Project description cannot exceed 1000 characters.";
    public const string ProjectOwnerIdEmpty = "Owner ID cannot be empty.";

    #endregion

    #region User

    public const string UserAuth0IdEmpty = "Auth0 User ID cannot be empty.";
    public const string UserAuth0IdTooLong = "Auth0 User ID cannot exceed 100 characters.";
    public const string UserEmailEmpty = "Email cannot be empty.";
    public const string UserEmailTooLong = "Email cannot exceed 254 characters.";
    public const string UserEmailInvalid = "Email must contain '@' symbol.";
    public const string UserDisplayNameEmpty = "Display name cannot be empty.";
    public const string UserDisplayNameTooLong = "Display name cannot exceed 100 characters.";

    #endregion
}
