namespace SimpleVote.Server.Configurations;

public static class MiddlewareConfigurations
{
    public static IApplicationBuilder UseMiddlewares(this IApplicationBuilder app)
    {
        app.UseHttpsRedirection();

        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}