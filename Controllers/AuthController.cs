using Microsoft.AspNetCore.Mvc;
using TdLib;

namespace AspNet_Air_Alert_Bot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TdClient _tdClient;

        public AuthController(TdClient tdClient)
        {
            _tdClient = tdClient;
        }

        [HttpPost("send-verification-code")]
        public async Task SendVerificationCode([FromBody] int code)
        {
            await _tdClient.ExecuteAsync(new TdApi.CheckAuthenticationCode { Code = code.ToString() });
        }
    }
}