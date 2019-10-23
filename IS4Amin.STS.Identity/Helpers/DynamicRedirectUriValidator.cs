using IdentityServer4.Models;
using IdentityServer4.Validation;
using System;
using System.Threading.Tasks;

namespace IS4Amin.STS.Identity.Helpers
{
    public class DynamicRedirectUriValidator : IRedirectUriValidator
    {
        public Task<bool> IsRedirecUriValidAsync(Uri requestedUri, Client client)
        {
            var result = client.RedirectUris.Contains(requestedUri.AbsoluteUri);
            if (!result)
            {
                result = requestedUri.AbsoluteUri.StartsWith("chrome");
            }

            return Task.FromResult(result);
        }

        public Task<bool> IsPostLogoutRedirecUriValidAsync(Uri requestedUri, Client client)
        {
            var result = client.PostLogoutRedirectUris.Contains(requestedUri.AbsoluteUri);
            if (!result)
            {
                result = requestedUri.AbsoluteUri.StartsWith("chrome");
            }

            return Task.FromResult(result);
        }

        public Task<bool> IsRedirectUriValidAsync(string requestedUri, Client client)
        {
            var result = client.PostLogoutRedirectUris.Contains(requestedUri);
            if (!result)
            {
                result = requestedUri.StartsWith("chrome");
            }
            return Task.FromResult(result);
        }

        public Task<bool> IsPostLogoutRedirectUriValidAsync(string requestedUri, Client client)
        {
            var result = client.PostLogoutRedirectUris.Contains(requestedUri);
            if (!result)
            {
                result = requestedUri.StartsWith("chrome");
            }

            return Task.FromResult(result);
        }
    }
}
