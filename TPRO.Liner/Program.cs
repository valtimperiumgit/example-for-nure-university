using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TPRO.Core.Images;
using TPRO.Core.RebbitMq;
using TPRO.Core.WebSockets;

var rabbitMqService = new RabbitMqService();
var imageProcessingService = new ImageProcessingService();

using var channel = rabbitMqService.CreateModel();
channel.QueueDeclare(queue: Queues.LinerQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);
                
var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
    try
    {
        var imageBytes = imageProcessingService.ProcessImage(ea.Body.ToArray(), ImageChanger.DetectEdges);

        channel.QueueDeclare(queue: Queues.TurnerQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);
        channel.BasicPublish(exchange: string.Empty, routingKey: Queues.TurnerQueue, basicProperties: null, body: imageBytes);
        
        WebSocketsUtils.SendImageViaWebSocket(imageBytes);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        Console.WriteLine("Ошибка при изображении");
        throw;
    }
};

channel.BasicConsume(queue: Queues.LinerQueue, autoAck: true, consumer: consumer);

Console.WriteLine("Press [enter] to exit.");
Console.ReadLine();