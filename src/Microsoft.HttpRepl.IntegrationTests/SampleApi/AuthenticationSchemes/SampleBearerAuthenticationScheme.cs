using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Microsoft.HttpRepl.IntegrationTests.SampleApi.AuthenticationSchemes
{
    public class SampleBearerAuthenticationScheme : AuthenticationHandler<AuthenticationSchemeOptions>
    {

        public const string SchemeName = "SampleBearer";

        public SampleBearerAuthenticationScheme(IOptionsMonitor<AuthenticationSchemeOptions> options,
                                                ILoggerFactory logger,
                                                UrlEncoder encoder,
                                                ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string[] authHeaderValues = ((string)Context.Request.Headers["Authorization"])?.Split(" ");

            if (authHeaderValues?.Length != 2 || authHeaderValues[0] != "bearer" || authHeaderValues[1] != "validToken")
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid authentication"));
            }
            else
            {
                var principle = new ClaimsPrincipal(new ClaimsIdentity("local"));
                return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principle, SchemeName)));
            }
        }
    }
}
