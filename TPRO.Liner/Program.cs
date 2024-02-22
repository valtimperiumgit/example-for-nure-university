using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.WebSockets;


var factory = new ConnectionFactory() { HostName = "localhost", Password = "guest", UserName = "guest"};
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: "linedImageQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
                
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
                                Bitmap bwImg = DetectEdges(originalBitmap);
                            
                                Console.WriteLine("IMAGE RECEIVED!");
                                
                                using (var memoryStream = new MemoryStream())
                                {
                                    bwImg.Save(memoryStream, ImageFormat.Png);
                                    byte[] imageBytes = memoryStream.ToArray();

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
            channel.BasicConsume(queue: "linedImageQueue", autoAck: true, consumer: consumer);

            Console.WriteLine("Press [enter] to exit.");
            Console.ReadLine();
            
            Bitmap DetectEdges(Bitmap original)
            {
                Bitmap edgeBitmap = new Bitmap(original.Width, original.Height);

                for (int x = 1; x < original.Width - 1; x++)
                {
                    for (int y = 1; y < original.Height - 1; y++)
                    {
                        Color prevX = original.GetPixel(x - 1, y);
                        Color nextX = original.GetPixel(x + 1, y);
                        Color prevY = original.GetPixel(x, y - 1);
                        Color nextY = original.GetPixel(x, y + 1);

                        int diffX = Math.Abs(prevX.R - nextX.R) + Math.Abs(prevX.G - nextX.G) + Math.Abs(prevX.B - nextX.B);
                        int diffY = Math.Abs(prevY.R - nextY.R) + Math.Abs(prevY.G - nextY.G) + Math.Abs(prevY.B - nextY.B);

                        int diff = (diffX + diffY) / 2;
                        diff = Math.Clamp(diff, 0, 255);

                        edgeBitmap.SetPixel(x, y, Color.FromArgb(diff, diff, diff));
                    }
                }

                return edgeBitmap;
            }
    
    async Task SendImageViaWebSocket(byte[] imageBytes)
    {
        using (var client = new ClientWebSocket())
        {
            await client.ConnectAsync(new Uri("ws://localhost:5184/"), CancellationToken.None);
        
            await client.SendAsync(new ArraySegment<byte>(imageBytes), WebSocketMessageType.Binary, true, CancellationToken.None);
            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        }
    }
}
