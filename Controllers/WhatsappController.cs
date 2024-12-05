using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WhatsAppService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WhatsAppController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "https://graph.facebook.com/v21.0/443109738879553/messages";
        private readonly string _whatsappToken = ("EAAXh7Cbv41oBO9hqQFGq0sEuHAcYfi4KJYBfijVJHY7AkoqYH3rr2yESbZCax07ljl5hQiqOxh7QBTgfgSUwrmy95kUwSM5ZCyJVwq52ZBClEJDoMYtZBJgh3alXyyhG1634x7WLAMN54QpQcLOV0rToRjpJFk9DxcZB6pskDOhYHQ1CgnixrbI2K1krd9MTZBBfVg5rYBmGO4cKPZBFgEtpUzpW7TW");
  
        public WhatsAppController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public IActionResult VerifyToken([FromQuery] string hub_verify_token, [FromQuery] string hub_challenge)
        {
            try
            {
                string accessToken = "myaccestokensecreto";
                if (hub_verify_token == accessToken)
                {
                    return Ok(hub_challenge);
                }
                return BadRequest("Error");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveMessage([FromBody] JsonElement body)
        {
            Console.WriteLine($"Mensaje recibido: '{body}'");
            try
            {
                var entry = body.GetProperty("entry")[0];
                var changes = entry.GetProperty("changes")[0];
                var value = changes.GetProperty("value");
                var messages = value.GetProperty("messages")[0];
                var from = messages.GetProperty("from").GetString();
                var text = messages.GetProperty("text").GetProperty("body").GetString();

                string questionUser = text.Trim();

                Console.WriteLine($"Mensaje recibido: '{questionUser}', por el numer: '{from}'");

                var msjpredeterminado = "hola, estas enviando mensajes.";
                var bodyAnswer = CreateMessageBody(msjpredeterminado, from);

                var sendMessageResult = await SendMessageAsync(bodyAnswer);
                if (sendMessageResult)
                {
                    Console.WriteLine("Mensaje enviado correctamente");
                }
                else
                {
                    Console.WriteLine("Error al enviar el mensaje");
                }

                return Ok("EVENT_RECEIVED");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest("Error");
            }
        }

        private async Task<bool> SendMessageAsync(object body)
        {
            try
            {
                var jsonBody = JsonSerializer.Serialize(body);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
                {
                    Headers = { { "Authorization", $"Bearer {_whatsappToken}" } },
                    Content = content
                };

                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private object CreateMessageBody(string text, string number)
        {
            return new
            {
                messaging_product = "whatsapp",
                recipient_type = "individual",
                to = number,
                type = "text",
                text = new
                {
                    preview_url = true,
                    body = text
                }
            };
        }
    }
}
