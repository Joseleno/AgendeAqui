using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Users;

public static class UserErrors
{
    public static readonly Error EmptyEmail = new("User.EmptyEmail", "Email is required.");
    public static readonly Error EmptyPassword = new("User.EmptyPassword", "Password is required.");
    public static readonly Error EmptyName = new("User.EmptyName", "Name is required.");
    public static readonly Error InvalidCredentials = new("User.InvalidCredentials", "Invalid email or password.");
    public static readonly Error NotFound = new("User.NotFound", "User not found.");
    public static readonly Error AlreadyLinkedToProfessional = new("User.AlreadyLinkedToProfessional", "User is already linked to a professional.");
    public static readonly Error AlreadyLinkedToClient = new("User.AlreadyLinkedToClient", "User is already linked to a client.");
    public static readonly Error EmailAlreadyExists = new("User.EmailAlreadyExists", "A user with this email already exists.");
}
