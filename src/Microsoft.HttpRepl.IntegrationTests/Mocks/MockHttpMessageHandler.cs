using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.HttpRepl.IntegrationTests.Mocks
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private HttpResponseMessage response;

        public MockHttpMessageHandler(HttpResponseMessage response)
        {
            this.response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(response);
        }
    }
}
