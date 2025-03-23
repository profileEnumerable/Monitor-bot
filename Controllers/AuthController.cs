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

        [HttpPost("send-params")]
        public async Task SendParams()
        {
            _logger.LogInformation("Sending TDLib params ...");

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
        }

        [HttpPost("send-phone-number")]
        public async Task SendPhoneNumber()
        {
            _logger.LogInformation("Sending phone number ...");
            await _tdClient.ExecuteAsync(
                new TdApi.SetAuthenticationPhoneNumber { PhoneNumber = Environment.GetEnvironmentVariable("PHONE_NUMBER") });
        }

        [HttpPost("send-verification-code")]
        public async Task SendVerificationCode([FromBody] int code)
        {
            await _tdClient.ExecuteAsync(new TdApi.CheckAuthenticationCode { Code = code.ToString() });
            _logger.LogInformation("Verification code was sent");
        }
    }
}
