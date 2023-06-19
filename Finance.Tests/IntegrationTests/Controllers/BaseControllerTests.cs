using GST.Fake.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace Finance.IntegrationTests.Controllers
{
    public class BaseControllerTests : IDisposable
    {
        protected readonly CustomWebApplicationFactory<Program> Factory;
        protected readonly HttpClient TestClient;
        protected readonly IConfiguration Configuration;

        public BaseControllerTests()
        {
            Factory = new CustomWebApplicationFactory<Program>();
            Configuration = Factory.Services.GetRequiredService<IConfiguration>();
            TestClient = Factory.CreateClient();
            TestClient.SetFakeBearerToken("admin");
        }

        public void Dispose()
        {
            Factory.Dispose();
        }

        protected async Task<(T, HttpStatusCode)> SendRequestAsync<T>(HttpMethod method, string uri, object requestBody = null)
        {
            var request = new HttpRequestMessage(method, uri);
            if (requestBody != null)
            {
                var json = JsonConvert.SerializeObject(requestBody);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = await TestClient.SendAsync(request);
            var statusCode = response.StatusCode;
            var content = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<T>(content);

            return (responseObject, statusCode);
        }
    }
}
