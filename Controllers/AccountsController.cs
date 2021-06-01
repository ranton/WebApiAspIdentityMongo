using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using POC_WebApiAspIdentityMongo.Models;
using POC_WebApiAspIdentityMongo.Services;
using System;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace POC_WebApiAspIdentityMongo.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly UserManager<TestUser> _userManager;
        private readonly IEmailSender _emailSender;

        public AccountsController(UserManager<TestUser> userManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserCommand command)
        {
            var user = new TestUser { UserName = command.Email, Email = command.Email };
            var result = await _userManager.CreateAsync(user, command.Password);

            if (result.Succeeded)
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));


                string callbackUrl = Url.Link("ConfirmEmail", new { userId = user.Id, code = code });

                var message = $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.";

                await _emailSender.SendEmailAsync(command.Email,
                    "Confirm your email",
                    message);

                return Ok(message);
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }
        }

        [HttpGet("{userId}/email/confirm", Name = "ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(Guid userId, string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null || user.EmailConfirmed)
            {
                return BadRequest();
            }
            else if ((await _userManager.ConfirmEmailAsync(user, Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code)))).Succeeded)
            {
                return Ok("Your email is confirmed");
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
