using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

using System;

namespace Avalanche.Security.Server.Extensions
{
    public static class MiddlewareExtensions
	{
#pragma warning disable S1075 // URIs should not be hardcoded
        private const string OlympusUri = "https://www.olympus-global.com/";
#pragma warning restore S1075 // URIs should not be hardcoded

        public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
		{
			services.AddSwaggerGen(cfg =>
			{
				cfg.SwaggerDoc("v1", new OpenApiInfo
				{
					Title = "Avalanche Security Server",
					Version = "v1",
					Description = "Api Security Server for Avalanche.",
					Contact = new OpenApiContact
					{
						Name = "Olympus",
						Url = new Uri(OlympusUri)
					},
					License = new OpenApiLicense
					{
						Name = "All rights reserved",
					},
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
				options.DocumentTitle = "Avalanche Security Server";
			});

			return app;
		}
	}
}
