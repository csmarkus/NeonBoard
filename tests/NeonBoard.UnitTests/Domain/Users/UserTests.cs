using NeonBoard.Domain.Common;
using NeonBoard.Domain.Users;
using NeonBoard.Domain.Users.Events;

namespace NeonBoard.UnitTests.Domain.Users;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateUser()
    {
        var user = User.Create("auth0|123", "test@example.com", "John Doe");

        user.Should().NotBeNull();
        user.Id.Should().NotBeEmpty();
        user.Auth0UserId.Should().Be("auth0|123");
        user.Email.Should().Be("test@example.com");
        user.DisplayName.Should().Be("John Doe");
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_ShouldRaiseUserCreatedEvent()
    {
        var user = User.Create("auth0|123", "test@example.com", "John Doe");

        var domainEvent = user.GetDomainEvents().Should().ContainSingle()
            .Which.Should().BeOfType<UserCreatedEvent>().Subject;

        domainEvent.UserId.Should().Be(user.Id);
        domainEvent.Email.Should().Be("test@example.com");
        domainEvent.DisplayName.Should().Be("John Doe");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyAuth0UserId_ShouldThrow(string? auth0UserId)
    {
        var act = () => User.Create(auth0UserId!, "test@example.com", "John");

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.UserAuth0IdEmpty);
    }

    [Fact]
    public void Create_WithAuth0UserIdExceedingMaxLength_ShouldThrow()
    {
        var longId = new string('a', 101);

        var act = () => User.Create(longId, "test@example.com", "John");

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.UserAuth0IdTooLong);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyEmail_ShouldThrow(string? email)
    {
        var act = () => User.Create("auth0|123", email!, "John");

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.UserEmailEmpty);
    }

    [Fact]
    public void Create_WithEmailMissingAtSymbol_ShouldThrow()
    {
        var act = () => User.Create("auth0|123", "invalid-email", "John");

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.UserEmailInvalid);
    }

    [Fact]
    public void Create_WithEmailExceedingMaxLength_ShouldThrow()
    {
        var longEmail = new string('a', 251) + "@b.c";

        var act = () => User.Create("auth0|123", longEmail, "John");

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.UserEmailTooLong);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyDisplayName_ShouldThrow(string? displayName)
    {
        var act = () => User.Create("auth0|123", "test@example.com", displayName!);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.UserDisplayNameEmpty);
    }

    [Fact]
    public void Create_WithDisplayNameExceedingMaxLength_ShouldThrow()
    {
        var longName = new string('a', 101);

        var act = () => User.Create("auth0|123", "test@example.com", longName);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.UserDisplayNameTooLong);
    }
}
