using Confluent.Kafka;
namespace EventStreamApp.Services;

public interface IKafkaSubscription
{
    public void Run(Action<string> action, string topic);
    public void Stop();
}

public class KafkaSubscription(IConfiguration configuration, ILogger<KafkaSubscription> logger): IKafkaSubscription
{
    private readonly ILogger<KafkaSubscription> logger = logger;
    private readonly IConfiguration configuration = configuration;

    private Action<string> action = default!;
    private string topic = string.Empty;
    private readonly CancellationTokenSource stoppingToken = new();

    public void Run(Action<string> action, string topic)
    {
        this.action = action;
        this.topic = topic;
        Task.Run(() => Subscribe(logger));
    }

    public void Stop()
    {
        stoppingToken.Cancel();
    }

    private void Subscribe(ILogger<KafkaSubscription> logger)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = configuration["EventStreams:BootstrapServers"],
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslMechanism = SaslMechanism.Plain,
            SaslUsername = configuration[$"EventStreams:{topic}:UserName"],
            SaslPassword = configuration[$"EventStreams:{topic}:Password"],
            SslCaLocation = $"config/cert-eem.pem",
            GroupId = "app",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            SslEndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.None,
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        try
        {
            consumer.Subscribe(topic);
            logger.LogInformation($"STARTING subscription to topic: {topic}");

            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken.Token);
                var message = result?.Message.Value;
                if (message != null)
                {
                    string info = $"Data received from topic {topic}: {message}";
                    logger.LogInformation(info);
                    action.Invoke(message);
                }
            }
        }
        catch (OperationCanceledException)
        {
            string info = $"ENDED subscription to topic: {topic}";
            logger.LogInformation(info);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }
        finally
        {
            consumer.Unsubscribe();
            consumer.Close();
            consumer.Dispose();
        }
    }
}