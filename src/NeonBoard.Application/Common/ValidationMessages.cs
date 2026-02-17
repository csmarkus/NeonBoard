namespace NeonBoard.Application.Common;

public static class ValidationMessages
{
    #region Common IDs

    public const string ProjectIdRequired = "Project ID is required.";
    public const string BoardIdRequired = "Board ID is required.";
    public const string ColumnIdRequired = "Column ID is required.";
    public const string CardIdRequired = "Card ID is required.";
    public const string LabelIdRequired = "Label ID is required.";
    public const string OwnerIdRequired = "Owner ID is required.";

    #endregion

    #region Board

    public const string BoardNameRequired = "Board name is required.";
    public const string BoardNameTooLong = "Board name cannot exceed 100 characters.";

    #endregion

    #region Column

    public const string ColumnNameRequired = "Column name is required.";
    public const string ColumnNameTooLong = "Column name cannot exceed 100 characters.";
    public const string ColumnIdsRequired = "Column IDs are required.";

    #endregion

    #region Card

    public const string CardTitleRequired = "Card title is required.";
    public const string CardTitleTooLong = "Card title cannot exceed 200 characters.";
    public const string CardDescriptionTooLong = "Card description cannot exceed 2000 characters.";
    public const string TargetColumnIdRequired = "Target column ID is required.";
    public const string TargetPositionNonNegative = "Target position must be greater than or equal to 0.";

    #endregion

    #region Project

    public const string ProjectNameRequired = "Project name is required.";
    public const string ProjectNameTooLong = "Project name cannot exceed 100 characters.";
    public const string ProjectDescriptionTooLong = "Project description cannot exceed 1000 characters.";

    #endregion

    #region Label

    public const string LabelNameRequired = "Label name is required.";
    public const string LabelNameTooLong = "Label name cannot exceed 50 characters.";
    public const string LabelColorRequired = "Label color is required.";

    #endregion
}
