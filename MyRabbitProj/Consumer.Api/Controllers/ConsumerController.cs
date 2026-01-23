using Microsoft.AspNetCore.Mvc;
using Consumer.Api.RabbitMq;


namespace Consumer.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ConsumerController : ControllerBase
{
    private readonly RabbitMqConsumerService _service;

    public ConsumerController(RabbitMqConsumerService service)
    {
        _service = service;
    }

    [HttpGet("status")]
    public IActionResult Status()
    {
        return Ok(new
        {
            service = "RabbitMQ Consumer",
            running = true,
            timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "OK",
            service = "RabbitMQ Consumer"
        });
    }
}
