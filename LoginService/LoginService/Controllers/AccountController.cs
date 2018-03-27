using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LoginService.Models;
using LoginService.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Cors;

namespace LoginService.Controllers
{
    [Produces("application/json")]
    [Route("api/account")]
    [EnableCors("AllowSpecificOrigin")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AccountManager _acountManager;

        public AccountController(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _acountManager = new AccountManager(userManager, configuration);
        }

        /// <summary>
        /// GET: api/account 
        /// Returns information about current account.
        /// </summary>
        /// <returns>Account details.</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user.");
            }

            return Ok(new ApiUserModel { Id = user.Id, UserName = user.UserName, Email = user.Email });
        }

        /// <summary>
        /// POST: api/account
        /// Creates new account.
        /// </summary>
        /// <param name="model">Account details. Must include UserName, Password and Email.</param>
        /// <returns>Created account.</returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]ApiUserModel model)
        {
            var user = new ApplicationUser { UserName = model.UserName, Email = model.Email };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest($"Cannot create account '{errors}'.");
            }

            ApiUserModel response = new ApiUserModel { Email = user.Email, Id = user.Id, UserName = user.UserName };

            return Ok(response);
        }

        /// <summary>
        /// PUT: api/account
        /// </summary>
        /// <param name="model">Parts of the account to be modified.</param>
        /// <returns>Operation status.</returns>
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Put([FromBody]ApiUserModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                await _acountManager.ChangePassword(user, model.Password, model.NewPassword);
                return Ok();
            }
            else if (!string.IsNullOrEmpty(model.Email))
            {
                await _acountManager.ChangeEmail(user, model.Email);
                return Ok();
            }

            return BadRequest();
        }

        /// <summary>
        /// DELETE: api/account
        /// Deletes given account. Available only to Admin users.
        /// </summary>
        /// <param name="id">Id of the account to be deleted.</param>
        /// <returns>Operation status.</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = _userManager.Users.SingleOrDefault(r => r.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            await _userManager.DeleteAsync(user);

            return Ok();
        }
    }
}
