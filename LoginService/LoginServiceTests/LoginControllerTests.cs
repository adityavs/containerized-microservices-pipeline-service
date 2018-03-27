using System;
using System.Threading.Tasks;
using LoginService.Controllers;
using LoginService.Data;
using LoginService.Models;
using LoginServiceTests.Stubs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace LoginServiceTests
{
    [TestClass]
    public class LoginControllerTests : ControllerTestsBase
    {
        [TestInitialize]
        public override void Init()
        {
            base.Init();
        }

        [TestMethod]
        public async Task LoginControllerTestValidLogin()
        {
            string password = "ABCdef123!@#";

            var expected = new ApplicationUser
            {
                UserName = Guid.NewGuid().ToString(),
                Email = $"{Guid.NewGuid()}@host.com",
            };

            var created = await UserManager.CreateAsync(expected, password);

            Assert.IsTrue(created.Succeeded);

            var target = new LoginController(UserManager, new FakeSignInManager(UserManager), ConfigMock.Object);

            var request = new ApiUserModel { UserName = expected.UserName, Password = password };

            var response = (OkObjectResult)await target.Post(request);

            Assert.AreEqual(200, response.StatusCode);

            var actual = (ApiUserModel)response.Value;

            Assert.AreEqual(expected.UserName, actual.UserName);
            Assert.AreEqual(expected.Email, actual.Email);
            Assert.AreEqual(Context.Users.SingleAsync().Result.Id, actual.Id);
            Assert.IsTrue(actual.Token.Length > 1);
        }

        [TestMethod]
        public async Task LoginControllerTestAbsentUser()
        {
            var config = new Mock<IConfiguration>();

            var target = new LoginController(UserManager, SignInManager, config.Object);

            var request = new ApiUserModel { UserName = "absent user", Password = "invalid password" };

            var actual = (UnauthorizedResult)await target.Post(request);

            Assert.AreEqual(401, actual.StatusCode);
        }

        [TestMethod]
        public async Task LoginControllerTestWrongPassword()
        {
            string password = "ABCdef123!@#";

            var expected = new ApplicationUser
            {
                UserName = Guid.NewGuid().ToString(),
                Email = $"{Guid.NewGuid()}@host.com",
            };

            var created = await UserManager.CreateAsync(expected, password);

            Assert.IsTrue(created.Succeeded);

            var config = new Mock<IConfiguration>();

            var target = new LoginController(UserManager, SignInManager, config.Object);

            var request = new ApiUserModel { UserName = expected.UserName, Password = "invalid password" };

            var actual = (UnauthorizedResult)target.Post(request).Result;

            Assert.AreEqual(401, actual.StatusCode);
        }
    }
}
