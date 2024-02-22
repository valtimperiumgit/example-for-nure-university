using RabbitMQ.Client;

namespace TPRO.Core.RebbitMq;

public class RabbitMqService
{
    private readonly ConnectionFactory _factory = new()
    {
        HostName = "localhost",
        Password = "guest",
        UserName = "guest"
    };

    public IModel CreateModel()
    {
        var connection = _factory.CreateConnection();
        return connection.CreateModel();
    }
}
