using Microsoft.AspNetCore.Builder;
using System;
using Swashbuckle.AspNetCore.SwaggerUI;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.VirtualFileSystem;

namespace Volo.Abp.Swagger
{
    public static class AbpSwaggerApplicationBuilderExtension
    {
        public static IApplicationBuilder UseSwaggerUIWithAbpOAuthLogin(this IApplicationBuilder app, Action<AbpSwaggerUIOptions> setupAction = null)
        {
            var virtualFileProvider = app.ApplicationServices.GetRequiredService<IVirtualFileProvider>();

            var fileInfo = virtualFileProvider.GetFileInfo("/Volo/Abp/Swagger/UI/index.html");

            var swaggerUiOptions = new AbpSwaggerUIOptions
            {
                IndexStream = () => fileInfo.CreateReadStream()
            };

            setupAction?.Invoke(swaggerUiOptions);

            swaggerUiOptions.ConfigObject.AdditionalItems["Authority"] = swaggerUiOptions.Authority;
            swaggerUiOptions.ConfigObject.AdditionalItems["SwaggerClientId"] = swaggerUiOptions.SwaggerClientId;
            swaggerUiOptions.ConfigObject.AdditionalItems["SwaggerClientSecret"] = swaggerUiOptions.SwaggerClientSecret;
            swaggerUiOptions.ConfigObject.AdditionalItems["SwaggerScope"] = swaggerUiOptions.SwaggerScope;
            swaggerUiOptions.ConfigObject.AdditionalItems["MultiTenancyIsEnabled"] = swaggerUiOptions.MultiTenancyIsEnabled;
            swaggerUiOptions.ConfigObject.AdditionalItems["TenantKey"] = swaggerUiOptions.TenantKey;

            return app.UseMiddleware<SwaggerUIMiddleware>((object)swaggerUiOptions);
        }
    }
}
