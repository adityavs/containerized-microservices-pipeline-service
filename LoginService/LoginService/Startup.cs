using LoginService.Data;
using LoginService.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.KeyVault;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LoginService
{
    public class Startup
    {
        private readonly TelemetryClient _telemetryClient;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            string appInsightsKey = Environment.GetEnvironmentVariable("APP_INSIGHTS_KEY");
            _telemetryClient = new TelemetryClient(new TelemetryConfiguration(appInsightsKey));
            _telemetryClient.TrackEvent("Login service started.");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
            string testing = Environment.GetEnvironmentVariable("TEST_VAL_KEY");


            if (connectionString == "DataSource=app.db")
            {
                services.AddDbContext<ApplicationDbContext>((options) => options.UseSqlite(connectionString));
            }
            else
            {
                if (connectionString.Contains("<password>"))
                {
                    string sqlPassword = GetSecret("sql-password");
                    connectionString = connectionString.Replace("<password>", sqlPassword);
                }
                services.AddDbContext<ApplicationDbContext>((options) => options.UseSqlServer(connectionString));
            }

            _telemetryClient.TrackTrace($"connection string: '{connectionString}'"); // Need to debug secrets in K8s. TODO: remove before releasing to production.

            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JWT_KEY")))
            {
                try
                {
                    Environment.GetEnvironmentVariable("JWT_KEY");
                }
                catch(Exception x) // until secrets work end-to-end have plan B
                {
                    _telemetryClient.TrackException(x);
                    Environment.GetEnvironmentVariable("JWT_KEY");
                }
            }

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin", builder =>                
                    builder
                        .WithOrigins(Environment.GetEnvironmentVariable("CORS_ORIGINS"))
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                );
            });

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = false;

                // User settings
                options.User.RequireUniqueEmail = true;
            });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); 

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

                })
                .AddJwtBearer(cfg =>
                {
                    cfg.RequireHttpsMetadata = false;
                    cfg.SaveToken = true;
                    cfg.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
                        ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY"))),
                        ClockSkew = TimeSpan.FromMinutes(5)
                    };
                });

            services.AddApplicationInsightsTelemetry(Configuration);
            //TODO: Figure out how to use ConfigMaps with App Insights Telemetry

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var dbInitializer = new DbInitializer(serviceScope.ServiceProvider);
                dbInitializer.ApplyMigrationsAsync().Wait();
                dbInitializer.SeedDataAsync().Wait();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();

            app.UseMvc();
        }

        private string GetSecret(string secretName)
        {
            using (var kv = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(GetTokenAsync)))
            {
                var sec = kv.GetSecretAsync($"{Environment.GetEnvironmentVariable("SECRETS_VAULT_URL")}/secrets/{secretName}").Result;

                return sec.Value;
            }
        }

        private async Task<string> GetTokenAsync(string authority, string resource, string scope)
        {
            /* 
             * Azure Principle password must be stored in the configured AadPasswordFilePath. 
             * In production the file will be written by deployment (Hexadite) into 
             * /secrets/secrets/mt-aad-password
             * In dev, create the file with the password and point AadPasswordFilePath to it.
             */
            string aadPassword = await File.ReadAllTextAsync(Environment.GetEnvironmentVariable("AAD_PASS_FILE_PATH"));

            var authContext = new AuthenticationContext(authority);
            var clientCred = new ClientCredential(Environment.GetEnvironmentVariable("AAD_APP_ID"), aadPassword);
            var result = await authContext.AcquireTokenAsync(resource, clientCred);

            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token");
            }

            return result.AccessToken;
        }
    }
}
