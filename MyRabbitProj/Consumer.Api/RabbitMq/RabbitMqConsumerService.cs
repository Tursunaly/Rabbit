using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Consumer.Api.RabbitMq;

public class RabbitMqConsumerService : BackgroundService
{
    private readonly ILogger<RabbitMqConsumerService> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    private const string QueueName = "queue-test";

    public RabbitMqConsumerService(ILogger<RabbitMqConsumerService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await StartConsumerAsync(stoppingToken);
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка работы RabbitMQ consumer");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        await StopConsumerAsync();
    }

    private async Task StartConsumerAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = "rabbitmq",
            Port = 5672,
            UserName = "guest",
            Password = "guest"
        };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync();

        await _channel.QueueDeclareAsync(
            QueueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken
        );

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                _logger.LogInformation("Сообщение: {Message}", message);
                await ProcessMessageAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обработки сообщения");
            }
        };

        await _channel.BasicConsumeAsync(
            QueueName,
            autoAck: true,
            consumer: consumer,
            cancellationToken: stoppingToken
        );
    }

    private Task ProcessMessageAsync(string message)
    {
        return Task.CompletedTask;
    }

    private async Task StopConsumerAsync()
    {
        try
        {
            if (_channel != null)
                await _channel.CloseAsync();

            if (_connection != null)
                await _connection.CloseAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при остановке RabbitMQ");
        }
        finally
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }

    public override void Dispose()
    {
        StopConsumerAsync().GetAwaiter().GetResult();
        base.Dispose();
    }
}
