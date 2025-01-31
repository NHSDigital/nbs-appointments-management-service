using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Nhs.Appointments.Core;
using Okta.Sdk.Api;
using Okta.Sdk.Client;
using Okta.Sdk.Model;

namespace Nhs.Appointments.UserManagement.Okta
{
    public class OktaUserDirectory(IOptions<OktaConfiguration> oktaOptions) : IUserDirectory
    {
        public async Task<UserProvisioningStatus> CreateIfNotExists(string user)
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(oktaOptions.Value.PEM);
            var keyParams = rsa.ExportParameters(true);

            var privateKey = new JsonWebKeyConfiguration
            {
                P = Convert.ToBase64String(keyParams.P),
                Kty = "RSA",
                Q = Convert.ToBase64String(keyParams.Q),
                D = Convert.ToBase64String(keyParams.D),
                E = Convert.ToBase64String(keyParams.Exponent),
                Kid =  oktaOptions.Value.PrivateKeyKid,
                Qi = Convert.ToBase64String(keyParams.InverseQ),
                Dp = Convert.ToBase64String(keyParams.DP),
                Dq = Convert.ToBase64String(keyParams.DQ),
                N = Convert.ToBase64String(keyParams.Modulus),
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
            var existingUsers = userApi.ListUsers(user);
            if (await existingUsers.AnyAsync())
            {
                var existingUser = await existingUsers.FirstAsync();
                if (existingUser != null && existingUser.Status == UserStatus.PROVISIONED &&
                    (DateTime.UtcNow - existingUser.Created > TimeSpan.FromDays(1)))
                {
                    await userApi.ReactivateUserAsync(user, true);
                    success = true;
                }
                else if (existingUser != null && existingUser.Status == UserStatus.ACTIVE)
                {
                    // user exists and is active
                    success = true;
                }
            }
            else
            {
                var createdUser = await userApi.CreateUserAsync(new CreateUserRequest
                {
                    Profile = new UserProfile { Email = user, Login = user, FirstName = "Not", LastName = "Applicable" },
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
            
            return new UserProvisioningStatus
            {
                Success = success,
                FailureReason = failureReason,
            };
        }
    }
}
