using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

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

			var signingConfigurations = new SigningConfigurations();
			services.AddSingleton(signingConfigurations);

			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

			services.AddCors(o => o.AddDefaultPolicy(builder =>
			{
				builder
					.WithOrigins("http://localhost:6001") //Dev Mode
												 //configSettings.IpAddress)
												 //.AllowAnyOrigin()
					.AllowAnyMethod()
					.AllowAnyHeader()
					.AllowCredentials();
			}));
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseCustomSwagger();

			app.UseHttpsRedirection();
			app.UseRouting();
			app.UseAuthorization();
			app.UseCors();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}