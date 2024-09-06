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

    private CancellationTokenSource stoppingToken = new CancellationTokenSource();

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


    private void Subscribe(ILogger<KafkaSubscription> runLogger)
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
        bool cancelledByUser = false;
        try
        {
            consumer.Subscribe(topic);
            runLogger.LogInformation($"Start subscription to topic: {topic}");

            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken.Token);
                var message = result?.Message.Value;
                if (message != null)
                {
                    runLogger.LogInformation($"Data received from topic {topic}: {message}");
                    action.Invoke(message);
                }
            }
        }
        catch (OperationCanceledException)
        {
            runLogger.LogInformation($"End subscription to topic: {topic}");
            cancelledByUser = true;
        }
        catch (Exception ex)
        {
            runLogger.LogError(ex, ex.Message);
        }
        finally
        {
            if (!cancelledByUser)
            {
                runLogger.LogInformation($"End subscription to topic: {topic}");
            }
            consumer.Unsubscribe();
            consumer.Close();
            consumer.Dispose();
        }
    }
}