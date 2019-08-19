using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.HttpRepl.IntegrationTests.SampleApi.AuthenticationSchemes;

namespace Microsoft.HttpRepl.IntegrationTests.SampleApi.Controllers
{

    [Authorize(AuthenticationSchemes = SampleBearerAuthenticationScheme.SchemeName)]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizedEndpointController : ControllerBase
    {

        [HttpGet("bearer")]
        public IActionResult Index()
        {
            return Ok();
        }
    }
}
