using Swashbuckle.AspNetCore.SwaggerUI;

namespace Volo.Abp.Swagger
{
    public class AbpSwaggerUIOptions : SwaggerUIOptions
    {
        public string Authority { get; set; }

        public string SwaggerClientId { get; set; }

        public string SwaggerClientSecret { get; set; }

        public string SwaggerScope { get; set; }

        public bool MultiTenancyIsEnabled { get; set; }

        public string TenantKey { get; set; }
    }
}