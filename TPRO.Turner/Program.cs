using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.WebSockets;


var factory = new ConnectionFactory() { HostName = "localhost", Password = "guest", UserName = "guest"};
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: "rotatedImageQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
                
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
                                Bitmap bwImg = RotateImage(originalBitmap);
                            
                                Console.WriteLine("IMAGE RECEIVED!");
                                
                                using (var memoryStream = new MemoryStream())
                                {
                                    bwImg.Save(memoryStream, ImageFormat.Png);
                                    byte[] imageBytes = memoryStream.ToArray();
                                    
                                    channel.QueueDeclare(queue: "linedImageQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
                                    channel.BasicPublish(exchange: "", routingKey: "linedImageQueue", basicProperties: null, body: imageBytes);
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
            channel.BasicConsume(queue: "rotatedImageQueue", autoAck: true, consumer: consumer);

            Console.WriteLine("Press [enter] to exit.");
            Console.ReadLine();
            
            Bitmap RotateImage(Bitmap original)
            {
                original.RotateFlip(RotateFlipType.Rotate180FlipNone);
                return original;
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
