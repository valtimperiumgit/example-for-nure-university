using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using TPRO.Core.RebbitMq;

namespace TPRO.Api.Controllers;

[Route("api/images")]
public class ImageController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> UploadImage(IFormFile image)
    {
        if (image.Length == 0)
        {
            return BadRequest("No image uploaded.");
        }

        using var memoryStream = new MemoryStream();
        await image.CopyToAsync(memoryStream);
        var imageBytes = memoryStream.ToArray();
        
        var factory = new ConnectionFactory()
        {
            HostName = "localhost",
            Password = "guest",
            UserName = "guest"
        };
        
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: Queues.GrayColorQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);
            channel.BasicPublish(exchange: string.Empty, routingKey: Queues.GrayColorQueue, basicProperties: null, body: imageBytes);
        }
                
        return Ok("Image uploaded and sent to processing queue!");
    }
}