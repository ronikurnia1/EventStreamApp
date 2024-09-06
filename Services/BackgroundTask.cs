﻿using Confluent.Kafka;

namespace EventStreamApp.Services;

public static class ServiceProviderExtensions
{
    public static TBackground GetHostedService<TBackground>
        (this IServiceProvider serviceProvider) =>
        serviceProvider
            .GetServices<IHostedService>()
            .OfType<TBackground>()
            .First();
}

public class BackgroundTask(IConfiguration configuration, ILogger<BackgroundTask> logger) : BackgroundService
{
    private readonly IConfiguration configuration = configuration;
    private readonly ILogger<BackgroundTask> logger = logger;
    //private readonly IList<KafkaEventHandler> handlers = new List<KafkaEventHandler>();

    public void AddHandler(KafkaEventHandler handler)
    {
        //handlers.Add(handler);
        Task.Run(() => Subscribe(handler));
    }

    public void RemoveHandler(KafkaEventHandler? handler)
    {
        if (handler != null)
        {
            handler.StoppingToken = new CancellationToken(true);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.CompletedTask;
        //await Task.Run(() => { }, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation($"Background tasks stopped.");
        await base.StopAsync(cancellationToken);
    }

    private void Subscribe(KafkaEventHandler handler)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = configuration["EventStreams:BootstrapServers"],
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslMechanism = SaslMechanism.Plain,
            SaslUsername = configuration[$"EventStreams:{handler.Topic}:UserName"],
            SaslPassword = configuration[$"EventStreams:{handler.Topic}:Password"],
            SslCaLocation = $"config/cert-eem.pem",
            GroupId = "app",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            SslEndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.None,
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        try
        {
            consumer.Subscribe(handler.Topic);
            logger.LogInformation($"Subscribe to topic: {handler.Topic}");
            while (!handler.StoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(handler.StoppingToken);
                var message = result?.Message.Value;
                if (message != null)
                {
                    logger.LogInformation($"Data received from topic {handler.Topic}: {message}");
                    handler.Action.Invoke(message);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }
        finally
        {
            consumer.Unsubscribe();
            logger.LogInformation($"Unsubscribe to topic: {handler.Topic}");
            consumer.Close();
            consumer.Dispose();
        }
    }
}
