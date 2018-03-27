using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using LoginService.Models;
using LoginService.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LoginService.Controllers
{
    /// <summary>
    /// Controller producing authorization tokens.
    /// </summary>
    [Produces("application/json")]
    [Route("api/login")]
    [EnableCors("AllowSpecificOrigin")]
    public class LoginController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly AccountManager _acountManager;

        public LoginController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _acountManager = new AccountManager(userManager, configuration);
        }

        // POST: api/login
        [HttpPost]        
        public async Task<IActionResult> Post([FromBody]ApiUserModel model)
        {
            var signIn = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);

            if (signIn.Succeeded)
            {
                var appUser = _userManager.Users.SingleOrDefault(r => r.UserName == model.UserName);

                string token = await _acountManager.GenerateJwtToken(appUser);

                var result = new ApiUserModel { Token = token, Id = appUser.Id, UserName = appUser.UserName, Email = appUser.Email };

                return Ok(result);
            }

            return Unauthorized();
        }

        [HttpGet("{value}")]
        public string Get(string value)
        {
            //Test method to troubleshoot connectivity. Will be removed once CI/CD with ACR works.
            return "Echo > " + value;
        }
    }
}
