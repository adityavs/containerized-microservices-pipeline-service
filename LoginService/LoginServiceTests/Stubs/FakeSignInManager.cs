using System.Threading.Tasks;
using LoginService.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace LoginServiceTests.Stubs
{
    public class FakeSignInManager : SignInManager<ApplicationUser>
    {
        public FakeSignInManager(UserManager<ApplicationUser> userManager)
        : base(userManager,
              new Mock<IHttpContextAccessor>().Object,
              new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
              new Mock<IOptions<IdentityOptions>>().Object,
              new Mock<ILogger<SignInManager<ApplicationUser>>>().Object,
              new Mock<IAuthenticationSchemeProvider>().Object)
        {
        }

        public override Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            return Task.FromResult(SignInResult.Success);
        }
    }
}
