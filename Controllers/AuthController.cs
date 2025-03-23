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

        [HttpPost("set-params")]
        public async Task SendParams()
        {
            _logger.LogInformation("Setting TDLib params ...");
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

                _logger.LogInformation("Client is sucessfully configured");
            }
            catch (Exception)
            {
                _logger.LogError("Error while configuring client");
                throw;
            }
        }

        [HttpPost("set-phone-number")]
        public async Task SendPhoneNumber()
        {
            try
            {
                await _tdClient.ExecuteAsync(
                    new TdApi.SetAuthenticationPhoneNumber { PhoneNumber = Environment.GetEnvironmentVariable("PHONE_NUMBER") });
            }
            catch (Exception)
            {
                _logger.LogInformation("Phone number sucessfully sent");
                throw;
            }
        }

        [HttpPost("send-verification-code")]
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
