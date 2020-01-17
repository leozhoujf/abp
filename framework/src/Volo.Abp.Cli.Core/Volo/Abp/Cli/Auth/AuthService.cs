using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using IdentityModel;
using Newtonsoft.Json;
using Volo.Abp.DependencyInjection;
using Volo.Abp.IdentityModel;
using Volo.Abp.IO;

namespace Volo.Abp.Cli.Auth
{
    public class AuthService : ITransientDependency
    {
        protected IIdentityModelAuthenticationService AuthenticationService { get; }

        public AuthService(IIdentityModelAuthenticationService authenticationService)
        {
            AuthenticationService = authenticationService;
        }

        public async Task LoginAsync(string userName, string password, string organizationName = null)
        {
            var configuration = new IdentityClientConfiguration(
                CliUrls.AccountAbpIo,
                "role email abpio abpio_www abpio_commercial offline_access",
                "abp-cli",
                "1q2w3e*",
                OidcConstants.GrantTypes.Password,
                userName,
                password
            );

            if (!organizationName.IsNullOrWhiteSpace())
            {
                configuration["[o]abp-organization-name"] = organizationName;
            }

            var tokenResponse = await AuthenticationService.GetTokenResponseAsync(configuration).ConfigureAwait(false);
            var tokenStoreModel = new TokenStoreModel(tokenResponse.AccessToken, tokenResponse.RefreshToken, tokenResponse.ExpiresIn);

            File.WriteAllText(CliPaths.AccessToken, JsonConvert.SerializeObject(tokenStoreModel, Formatting.Indented), Encoding.UTF8);
        }

        public Task LogoutAsync()
        {
            FileHelper.DeleteIfExists(CliPaths.AccessToken);
            return Task.CompletedTask;
        }

        public static bool IsLoggedIn()
        {
            return File.Exists(CliPaths.AccessToken);

        }

        public class TokenStoreModel
        {
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
            public int ExpiresIn { get; set; }

            public TokenStoreModel(string accessToken, string refreshToken, int expiresIn)
            {
                AccessToken = accessToken;
                RefreshToken = refreshToken;
                ExpiresIn = expiresIn;
            }
        }
    }
}
