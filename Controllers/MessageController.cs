using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessagingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly string _connectionString;

        public MessageController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public async Task<IActionResult> PostMessage([FromBody] Message message)
        {
            if (message == null || string.IsNullOrEmpty(message.Text))
            {
                return BadRequest("Invalid message.");
            }

            message.CreatedAt = DateTime.UtcNow;

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var cmd = new NpgsqlCommand(
                        "INSERT INTO \"Messages\" (\"Text\", \"CreatedAt\", \"ClientNumber\") VALUES (@text, @createdAt, @clientNumber)", connection))
                    {
                        cmd.Parameters.AddWithValue("text", message.Text);
                        cmd.Parameters.AddWithValue("createdAt", message.CreatedAt);
                        cmd.Parameters.AddWithValue("clientNumber", message.ClientNumber);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                // Здесь можно реализовать отправку сообщения по WebSocket второму клиенту

                return Ok();
            }
            catch (Exception ex)
            {
                // Логируем ошибку (можно использовать встроенные инструменты логирования ASP.NET Core)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMessages([FromQuery] DateTime startTime, [FromQuery] DateTime endTime)
        {
            var messages = new List<Message>();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var cmd = new NpgsqlCommand(
                        "SELECT \"Id\", \"Text\", \"CreatedAt\", \"ClientNumber\" FROM \"Messages\" WHERE \"CreatedAt\" BETWEEN @startTime AND @endTime", connection))
                    {
                        cmd.Parameters.AddWithValue("startTime", startTime);
                        cmd.Parameters.AddWithValue("endTime", endTime);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                messages.Add(new Message
                                {
                                    Id = reader.GetInt32(0),
                                    Text = reader.GetString(1),
                                    CreatedAt = reader.GetDateTime(2),
                                    ClientNumber = reader.GetInt32(3)
                                });
                            }
                        }
                    }
                }

                return Ok(messages);
            }
            catch (Exception ex)
            {
                // Логируем ошибку
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
