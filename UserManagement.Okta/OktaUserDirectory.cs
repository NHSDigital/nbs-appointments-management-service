using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Okta.Sdk.Abstractions.Configuration;
using Okta.Sdk.Api;
using Okta.Sdk.Client;
using Okta.Sdk.Model;

namespace UserManagement.Okta
{
    public static class ServiceProviderExtensions
    {
        public static IServiceCollection AddOktaUserDirectory(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.Configure<OktaConfiguration>(opts => configuration.GetSection("Okta").Bind(opts));
            services.AddSingleton<IUserDirectory, OktaUserDirectory>();
            return services;
        }
    }

    public class OktaUserDirectory(IOptions<OktaConfiguration> oktaOptions) : IUserDirectory
    {
        public async Task<UserProvisioningStatus> CreateIfNotExists(string user)
        {
            var privateKey = new JsonWebKeyConfiguration
            {
                P = oktaOptions.Value.PrivateKeyP,
                Kty = "RSA",
                Q = oktaOptions.Value.PrivateKeyQ,
                D = oktaOptions.Value.PrivateKeyD,
                E = oktaOptions.Value.PrivateKeyE,
                Kid = oktaOptions.Value.PrivateKeyKid,
                Qi = oktaOptions.Value.PrivateKeyQi,
                Dp = oktaOptions.Value.PrivateKeyDp,
                Dq = oktaOptions.Value.PrivateKeyDq,
            };

            var userApi = new UserApi(new Configuration
            {
                OktaDomain = oktaOptions.Value.Domain,
                AuthorizationMode = AuthorizationMode.PrivateKey,
                ClientId = oktaOptions.Value.ManagementId,
                Scopes = new HashSet<string> { "okta.users.read", "okta.users.manage" },
                PrivateKey = privateKey,

            });
            var success = false;
            var failureReason = string.Empty;

            var existingUser = await userApi.GetUserAsync(user);
            if (existingUser != null && existingUser.Status == UserStatus.PROVISIONED && (DateTime.UtcNow - existingUser.Created > TimeSpan.FromDays(1)))
            {
                await userApi.ReactivateUserAsync(user, true);
                success = true;
            }
            else if (existingUser != null && existingUser.Status == UserStatus.ACTIVE)
            {
                // user exists and is active
                success = true;
            }
            else if (existingUser == null)
            {
                var createdUser = await userApi.CreateUserAsync(new CreateUserRequest
                {
                    Profile = new UserProfile { Email = user, Login = user, },
                });
                if (createdUser != null)
                {
                    success = true;
                }
                else
                {
                    failureReason = "User could not be created";
                }
            }
            else
            {
                failureReason = $"User is currently in status - {existingUser.Status.Value}";
            }
            
            return new UserProvisioningStatus
            {
                Success = success,
                FailureReason = failureReason,
            };
        }
    }
}
