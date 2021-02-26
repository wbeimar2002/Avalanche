using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NSwag;
using NSwag.AspNetCore;
using NSwag.Generation.Processors.Security;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Avalanche.Api.Extensions
{
    [ExcludeFromCodeCoverage]
	public static class MiddlewareExtensions
	{
		public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
		{
			services.AddSwaggerDocument(config =>
			{
				config.OperationProcessors.Add(new OperationSecurityScopeProcessor("JWT token"));
				config.AddSecurity("JWT token", new OpenApiSecurityScheme
				{
					Type = OpenApiSecuritySchemeType.ApiKey,
					Name = "Authorization",
					Description = "Copy 'Bearer ' + valid JWT token into field",
					In = OpenApiSecurityApiKeyLocation.Header
				});
				config.PostProcess = document =>
				{
					document.Info.Version = "v1";
					document.Info.Title = "Avalanche API";
					document.Info.Description = "Api Gateway for Avalanche.";
					document.Info.TermsOfService = "None";
					document.Info.Contact = new NSwag.OpenApiContact
					{
						Name = "Olympus",
						//			Url = new Uri("https://www.olympus-global.com/")
					};
					document.Info.License = new NSwag.OpenApiLicense
					{
						Name = "All rights reserved",
					};
				};
			});
			return services;
		}

		public static IApplicationBuilder UseCustomSwagger(this IApplicationBuilder app)
		{
			app.UseOpenApi();
			app.UseSwaggerUi3();

			return app;
		}
	}
}
