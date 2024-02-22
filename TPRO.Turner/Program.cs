using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TPRO.Core.Images;
using TPRO.Core.RebbitMq;
using TPRO.Core.WebSockets;

var rabbitMqService = new RabbitMqService();
var imageProcessingService = new ImageProcessingService();

using var channel = rabbitMqService.CreateModel();
channel.QueueDeclare(queue: Queues.TurnerQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);
                
var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
    try
    {
        var imageBytes = imageProcessingService.ProcessImage(ea.Body.ToArray(), ImageChanger.RotateImage);
        WebSocketsUtils.SendImageViaWebSocket(imageBytes);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }
};
channel.BasicConsume(queue: Queues.TurnerQueue, autoAck: true, consumer: consumer);

Console.WriteLine("Press [enter] to exit.");
Console.ReadLine();
