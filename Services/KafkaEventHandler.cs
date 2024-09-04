namespace EventStreamApp.Services;

public record KafkaEventHandler(Action<string> Action, string Topic)
{
    public CancellationToken StoppingToken { get; set; } = CancellationToken.None;
}