using System;
using AutoMapper;
using Avalanche.Security.Server.Core.Repositories;
using Avalanche.Security.Server.Core.Security.Hashing;
using Avalanche.Security.Server.Core.Security.Tokens;
using Avalanche.Security.Server.Core.Services;
using Avalanche.Security.Server.Extensions;
using Avalanche.Security.Server.Persistence;
using Avalanche.Security.Server.Security.Hashing;
using Avalanche.Security.Server.Security.Tokens;
using Avalanche.Security.Server.Services;
using Avalanche.Shared.Infrastructure.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Avalanche.Security.Server
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			/*services.AddDbContext<AppDbContext>(options =>
			{
				options.UseInMemoryDatabase("Avalanche.Security.Server");
			});*/

			services.AddDbContext<AppDbContext>(options =>
				  options.UseSqlite(Configuration.GetConnectionString("ConnectionSqlite")));

			services.AddControllers();

			services.AddCustomSwagger();

			services.AddScoped<IUserRepository, UserRepository>();
			services.AddScoped<IUnitOfWork, UnitOfWork>();

			services.AddSingleton<IPasswordHasher, PasswordHasher>();
			services.AddSingleton<ITokenHandler, Security.Tokens.TokenHandler>();

			services.AddScoped<IUserService, UserService>();
			services.AddScoped<IAuthenticationService, AuthenticationService>();

			services.Configure<TokenOptions>(Configuration.GetSection("TokenOptions"));
			var tokenOptions = Configuration.GetSection("TokenOptions").Get<TokenOptions>();

			services.Configure<TokenOptions>(Configuration.GetSection("AuthSettings"));
			var authSettings = Configuration.GetSection("AuthSettings").Get<AuthSettings>();

			var signingConfigurations = new SigningConfigurations(authSettings.SecretKey);
			services.AddSingleton(signingConfigurations);

			services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(jwtBearerOptions =>
			{
				jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters()
				{
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = tokenOptions.Issuer,
					ValidAudience = tokenOptions.Audience,
					IssuerSigningKey = signingConfigurations.Key,
					ClockSkew = TimeSpan.Zero
				};
			});

			services.AddAutoMapper(this.GetType().Assembly);
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseDeveloperExceptionPage();

			app.UseRouting();

			app.UseCustomSwagger();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}