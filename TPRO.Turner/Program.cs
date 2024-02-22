using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.WebSockets;
using TPRO.Core.Images;
using TPRO.Core.RebbitMq;
using TPRO.Core.WebSockets;


var factory = new ConnectionFactory { HostName = "localhost", Password = "guest", UserName = "guest"};
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();
channel.QueueDeclare(queue: Queues.TurnerQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);
                
var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    using var ms = new MemoryStream(body);

    try
    {
        using Image img = Image.FromStream(ms);
        using Bitmap originalBitmap = new Bitmap(img);
        Bitmap bwImg = ImageChanger.RotateImage(originalBitmap);
        
        using var memoryStream = new MemoryStream();
        bwImg.Save(memoryStream, ImageFormat.Png);
        byte[] imageBytes = memoryStream.ToArray();
        
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