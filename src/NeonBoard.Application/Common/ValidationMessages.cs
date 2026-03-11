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
    public const string BoardPrefixInvalid = "Board prefix must be 2-5 uppercase letters (A-Z).";
    public const string BoardPrefixNotUnique = "A board with this prefix already exists in the project.";

    #endregion

    #region Column

    public const string ColumnNameRequired = "Column name is required.";
    public const string ColumnNameTooLong = "Column name cannot exceed 50 characters.";
    public const string ColumnIdsRequired = "Column IDs are required.";

    #endregion

    #region Card

    public const string CardTitleRequired = "Card title is required.";
    public const string CardTitleTooLong = "Card title cannot exceed 200 characters.";
    public const string CardDescriptionTooLong = "Card description cannot exceed 5000 characters.";
    public const string TargetColumnIdRequired = "Target column ID is required.";
    public const string PositionRequired = "Position is required.";

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

    #region Membership

    public const string MemberUserIdRequired = "Member user ID is required.";
    public const string InvitationEmailRequired = "Email address is required.";
    public const string InvitationEmailInvalid = "A valid email address is required.";
    public const string InvitationTokenRequired = "Invitation token is required.";
    public const string InvitationRoleRequired = "Role is required.";
    public const string InvitationRoleInvalid = "Role must be Editor or Viewer.";

    #endregion
}
