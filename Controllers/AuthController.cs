using Microsoft.AspNetCore.Mvc;
using TdLib;

namespace AspNet_Air_Alert_Bot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly TdClient _tdClient;

        public AuthController(
            ILogger<AuthController> logger, 
            TdClient tdClient)
        {
            _logger = logger;
            _tdClient = tdClient;
        }

        [HttpPost("configure-client")]
        public async Task SendParams()
        {
            _logger.LogInformation("Sending TDLib params ...");
            try
            {
                await _tdClient.ExecuteAsync(new TdApi.SetTdlibParameters
                {
                    UseTestDc = false,
                    DatabaseDirectory = "tdlib",
                    UseFileDatabase = true,
                    UseChatInfoDatabase = true,
                    UseMessageDatabase = true,
                    UseSecretChats = false,
                    ApiId = int.Parse(Environment.GetEnvironmentVariable("API_ID")),
                    ApiHash = Environment.GetEnvironmentVariable("API_HASH"),
                    SystemLanguageCode = "en",
                    DeviceModel = "PC",
                    ApplicationVersion = "1.0",
                });

                await _tdClient.ExecuteAsync(
                  new TdApi.SetAuthenticationPhoneNumber { PhoneNumber = Environment.GetEnvironmentVariable("PHONE_NUMBER") });

                _logger.LogDebug("Client is sucessfully configured");
            }
            catch (Exception)
            {
                _logger.LogError("Error while configuring client");
                throw;
            }
        }

        [HttpPost("send-varification-code")]
        public async Task SendVerificationCode([FromBody] int code)
        {
            try
            {
                await _tdClient.ExecuteAsync(new TdApi.CheckAuthenticationCode { Code = code.ToString() });
                _logger.LogDebug("Verification code is sucessfully sent");
            }
            catch (Exception)
            {
                _logger.LogError("Error while sending verification code");
                throw;
            }
        }
    }
}
