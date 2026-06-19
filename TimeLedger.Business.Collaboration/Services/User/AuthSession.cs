namespace BusinessCollaboration.Services.User;

public static class AuthSession
{
    public const string UserIdKey = "Auth.UserId";
    public const string UserEmailKey = "Auth.UserEmail";
    public const string UserNameKey = "Auth.UserName";
}
/*
 1. Never changes
 2. Used directly everywhere
 
 Cons: not really testable, can't swap keys per environment
 */
