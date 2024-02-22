using System.Drawing;
using RabbitMQ.Client;
using System.Drawing.Imaging;
using Microsoft.Extensions.Configuration;

namespace TPRO.Core.RebbitMq;

public class RabbitMqService
{
    private readonly ConnectionFactory _factory;

    public RabbitMqService()
    {
        _factory = new ConnectionFactory 
        {
            HostName = "localhost",
            Password = "guest",
            UserName = "guest"
        };
    }

    public IModel CreateModel()
    {
        var connection = _factory.CreateConnection();
        return connection.CreateModel();
    }
}
