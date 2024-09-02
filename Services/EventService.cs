using Confluent.Kafka;

namespace EventStreamApp.Services;


public interface IEventService
{
    public void Subscribe(IConfiguration configuration, string eventName, Action<string> action);
    public void Unsubscribe();
}


public class EventService(ILogger<EventService> logger) : IEventService
{
    private IConsumer<Ignore, string>? consumer;
    private readonly ILogger<EventService> logger = logger;
    private Action<string> subscriberAction = default!;

    private bool running = false;

    private async Task RunSubscriber()
    {
        while (running)
        {
            ProcessMessage(CancellationToken.None);
            await Task.Delay(TimeSpan.FromSeconds(10), CancellationToken.None);
        }
        consumer?.Close();
    }

    private void ProcessMessage(CancellationToken stoppingToken)
    {
        try
        {
            var consumeResult = consumer?.Consume(stoppingToken);
            var message = consumeResult?.Message.Value;
            logger.LogInformation($"Event data received: {message}");
            if (message != null)
            {
                subscriberAction.Invoke(message);
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Error processing message: {ex.Message}");
        }
    }

    public void Subscribe(IConfiguration configuration, string eventName, Action<string> action)
    {
        if (running) return;
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = configuration["EventStreams:BootstrapServers"],
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslMechanism = SaslMechanism.Plain,
            SaslUsername = configuration[$"EventStreams:{eventName}:UserName"],
            SaslPassword = configuration[$"EventStreams:{eventName}:Password"],
            SslCaLocation = $"C:/Users/ZZ015M749/source/repos/EventStreamApp/Certificates/cert-eem.pem",
            GroupId = "app",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            SslEndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.None,
        };
        consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        running = true;
        subscriberAction = action;
        consumer.Subscribe(eventName);

        Task.Run(RunSubscriber);
    }

    public void Unsubscribe()
    {
        running = false;
        consumer?.Unsubscribe();
    }

}
