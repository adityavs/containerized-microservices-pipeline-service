using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
    public class ControllerTestsBase
    {
        protected ApplicationDbContext Context { get; private set; }

        protected UserManager<ApplicationUser> UserManager { get; private set; }

        protected SignInManager<ApplicationUser> SignInManager { get; private set; }

        protected Mock<IConfiguration> ConfigMock { get; private set; }

        public virtual void Init()
        {
            ConfigMock = new Mock<IConfiguration>();

            ConfigMock.Setup(c => c["JwtKey"]).Returns(Guid.NewGuid().ToString);
            ConfigMock.Setup(c => c["JwtExpireMinutes"]).Returns("10");
            ConfigMock.Setup(c => c["JwtIssuer"]).Returns(Guid.NewGuid().ToString);
            ConfigMock.Setup(c => c["JwtAudience"]).Returns(Guid.NewGuid().ToString);

            var services = new ServiceCollection();

            services.AddDbContext<ApplicationDbContext>((options) => options.UseInMemoryDatabase("DB"));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.User.RequireUniqueEmail = true;
            });

            var httpContext = new DefaultHttpContext();

            httpContext.Features.Set<IHttpAuthenticationFeature>(new HttpAuthenticationFeature()); // todo: check if it is required

            services.AddSingleton<IHttpContextAccessor>(h => new HttpContextAccessor { HttpContext = httpContext });

            var serviceProvider = services.BuildServiceProvider();

            Context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            UserManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            SignInManager = serviceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
        }
    }
}
