namespace SignalRDemo.Server.Configurations;

public static class AppConstants
{
    // Data
    public const string MainDbName = "MainDb";

    // Security
    public const string AuthCookieName = "auth";
    public const string RoleManagerAuthorizationPolicyName = "RoleManager";
    public const string VoteAdministratorAuthorizationPolicyName = "VoteAdministrator";
    public const string VoteInspectorAuthorizationPolicyName = "VoteInspector";
    public const string AdminRoleName = "admin";
    public const string VoteAdminRoleName = "voteadmin";
    public const string VoteInspectorRoleName = "voteinspector";

    // Validations
    public const int UserPasswordMinimumLength = 8;
    public const int RoleNameMinimumLength = 3;
}