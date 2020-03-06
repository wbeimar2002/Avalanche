using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;

namespace Avalanche.Security.Server.Extensions
{
	public static class MiddlewareExtensions
	{
		public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
		{
			services.AddSwaggerGen(cfg =>
			{
				//Reference: https://medium.com/@evandro.ggomes/json-web-token-authentication-with-asp-net-core-2-0-b074b0cfc870
				cfg.SwaggerDoc("v1", new OpenApiInfo
				{
					Title = "Avalanche.Security.Server",
					Version = "v1",
					Description = "Server for JSON Web Token authentication and authorization",
					Contact = new OpenApiContact
					{
						Name = "MILL5",
						Url = new Uri("https://mill5.com/")
					}
				});

				cfg.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
				{
					In = ParameterLocation.Header,
					Description = "JSON Web Token to access resources. Example: Bearer {token}",
					Name = "Authorization",
					Type = SecuritySchemeType.ApiKey
				});

				cfg.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{
						new OpenApiSecurityScheme
						{
							Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
						},
						new [] { string.Empty }
					}
				});
			});

			return services;
		}

		public static IApplicationBuilder UseCustomSwagger(this IApplicationBuilder app)
		{
			app.UseSwagger().UseSwaggerUI(options =>
			{
				options.SwaggerEndpoint("/swagger/v1/swagger.json", "Avalanche.Security.Server");
				options.DocumentTitle = "Avalanche.Security.Server";
				options.RoutePrefix = string.Empty;
			});

			return app;
		}
	}
}
