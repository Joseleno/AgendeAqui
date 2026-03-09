using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Users;

public static class UserErrors
{
    public static readonly Error EmptyEmail = new("User.EmptyEmail", "Email is required.");
    public static readonly Error EmptyPassword = new("User.EmptyPassword", "Password is required.");
    public static readonly Error EmptyName = new("User.EmptyName", "Name is required.");
    public static readonly Error InvalidCredentials = new("User.InvalidCredentials", "Invalid email or password.");
    public static readonly Error NotFound = new("User.NotFound", "User not found.");
}
