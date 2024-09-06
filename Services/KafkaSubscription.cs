using Confluent.Kafka;

namespace EventStreamApp.Services;

public class KafkaSubscription(Action<string> action, string topic,
    IConfiguration configuration, ILoggerFactory factory)
{
    private readonly ILogger<KafkaSubscription> logger = factory.CreateLogger<KafkaSubscription>();
    private readonly Action<string> action = action;
    private readonly string topic = topic;
    private readonly IConfiguration configuration = configuration;
    private CancellationTokenSource stoppingToken = new CancellationTokenSource();

    public void Run()
    {
        Task.Run(() => Subscribe(logger));
        //await Task.WhenAll(task);
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