namespace SignalRDemo.Server.Services.Errors;

public sealed record ServiceValidationError(string Message,
    IReadOnlyDictionary<string, IReadOnlyList<string>> ValidationErrors) 
    : ServiceError(Message);