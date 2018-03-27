using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using LoginService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LoginService.Services
{
    public class AccountManager
    {
        private readonly IConfiguration _configuration;

        private readonly UserManager<ApplicationUser> _userManager;

        public AccountManager(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _configuration = configuration;
            _userManager = userManager;
        }

        public async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var roles = await _userManager.GetRolesAsync(user);

            var roleClaims = roles.Select(r => new Claim(ClaimTypes.Role, r));

            claims.AddRange(roleClaims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["JwtExpireMinutes"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtIssuer"],
                audience: _configuration["JwtAudience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task ChangeEmail(ApplicationUser user, string email)
        {
            if (user.Email != email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, email);
                if (!setEmailResult.Succeeded)
                {
                    string errors = string.Join(", ", setEmailResult.Errors.Select(e => e.Description));
                    throw new ApplicationException($"Unable to change email '{errors}'.");
                }
            }
        }

        public async Task ChangePassword(ApplicationUser user, string oldPassword, string newPassword)
        {
            var changePasswordResult = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            if (!changePasswordResult.Succeeded)
            {
                string errors = string.Join(", ", changePasswordResult.Errors.Select(e => e.Description));
                throw new ApplicationException($"Unable to change password '{errors}'.");
            }
        }
    }
}
