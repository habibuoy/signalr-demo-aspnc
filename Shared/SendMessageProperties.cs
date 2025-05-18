namespace SignalRDemo.Shared;

public struct SendMessageProperties
{
    public string Sender { get; set; }
    public string Message { get; set; }

    public static SendMessageProperties ServerNotification(string message)
    {
        return new SendMessageProperties()
        {
            Sender = "SERVER",
            Message = message
        };
    }
}