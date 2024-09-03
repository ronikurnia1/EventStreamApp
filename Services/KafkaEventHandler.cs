namespace EventStreamApp.Services;

public record KafkaEventHandler(Action<string> Action, string Topic)
{
    private bool isStopped = false;
    public bool IsStopped => isStopped;

    public void Stop()
    {
        isStopped = true;
    }

    public void Reset()
    {
        isStopped = false;
    }
}