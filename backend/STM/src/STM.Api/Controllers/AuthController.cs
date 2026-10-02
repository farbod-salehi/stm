using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using STM.Api.Models.Auth;
using STM.Application.Interfaces.Services;

namespace STM.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth) => _auth = auth;

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserRequestModel model)
        {
            await _auth.RegisterAsync(new RegisterUserRequest
            {
                Email = model.Email,
                Password = model.Password
            });

            return Ok("OTP sent");
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(VerifyOtpRequestModel model)
        {
            await _auth.VerifyOtpAsync(new VerifyOtpRequest
            {
                Email = model.Email,
                Code = model.Code
            });

            return Ok("Email verified");
        }
    }

}
