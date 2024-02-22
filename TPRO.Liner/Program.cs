using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Drawing;
using System.Drawing.Imaging;
using TPRO.Core.Images;
using TPRO.Core.RebbitMq;
using TPRO.Core.WebSockets;

var factory = new ConnectionFactory { HostName = "localhost", Password = "guest", UserName = "guest"};
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();
channel.QueueDeclare(queue: Queues.LinerQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);
                
var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    using var ms = new MemoryStream(body);

    try
    {
        using Image img = Image.FromStream(ms);
        using Bitmap originalBitmap = new Bitmap(img);
        Bitmap bwImg = ImageChanger.DetectEdges(originalBitmap);

        using var memoryStream = new MemoryStream();
        bwImg.Save(memoryStream, ImageFormat.Png);
        byte[] imageBytes = memoryStream.ToArray();

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