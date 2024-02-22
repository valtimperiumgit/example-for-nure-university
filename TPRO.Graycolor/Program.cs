using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.WebSockets;


var factory = new ConnectionFactory() { HostName = "localhost", Password = "guest", UserName = "guest"};
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: "bwImageQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
                
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                using (var ms = new MemoryStream(body))
                {
                    Console.WriteLine(ms);

                    try
                    {
                        using (Image img = Image.FromStream(ms))
                        {
                            using (Bitmap originalBitmap = new Bitmap(img))
                            {
                                Bitmap bwImg = MakeBlackAndWhite(originalBitmap);
                            
                                Console.WriteLine("IMAGE RECEIVED!");
                                
                                using (var memoryStream = new MemoryStream())
                                {
                                    bwImg.Save(memoryStream, ImageFormat.Png);
                                    byte[] imageBytes = memoryStream.ToArray();

                                    channel.QueueDeclare(queue: "rotatedImageQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
                                    channel.BasicPublish(exchange: "", routingKey: "rotatedImageQueue", basicProperties: null, body: imageBytes);
                                    Console.WriteLine("Image sended!!!");
                                    Console.WriteLine(imageBytes);

                                    SendImageViaWebSocket(imageBytes);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Console.WriteLine("Ошибка при изображении");
                        throw;
                    }
                }
            };
            channel.BasicConsume(queue: "bwImageQueue", autoAck: true, consumer: consumer);

            Console.WriteLine("Press [enter] to exit.");
            Console.ReadLine();


    static Bitmap MakeBlackAndWhite(Bitmap original)
    {
        Bitmap newBitmap = new Bitmap(original.Width, original.Height);

        Graphics g = Graphics.FromImage(newBitmap);
        
        ColorMatrix colorMatrix = new ColorMatrix(
            new float[][]
            {
                new float[] {.3f, .3f, .3f, 0, 0},
                new float[] {.59f, .59f, .59f, 0, 0},
                new float[] {.11f, .11f, .11f, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {0, 0, 0, 0, 1}
            });

        ImageAttributes attributes = new ImageAttributes();
        attributes.SetColorMatrix(colorMatrix);

        g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                    0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
        g.Dispose();

        return newBitmap;
    }
    
    async Task SendImageViaWebSocket(byte[] imageBytes)
    {
        using (var client = new ClientWebSocket())
        {
            await client.ConnectAsync(new Uri("ws://localhost:5184/"), CancellationToken.None);
        
            await client.SendAsync(new ArraySegment<byte>(imageBytes), WebSocketMessageType.Binary, true, CancellationToken.None);

            // Закрыть соединение после отправки
            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        }
    }
}
