using BillManager.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BillManager.IntegrationTests
{
    public class ApiTestBase : IClassFixture<WebApplicationFactory<Startup>> 
    {
        private readonly WebApplicationFactory<Startup> _webApplicationFactory;

        protected WebApplicationFactory<Startup> WebApplicationFactory => _webApplicationFactory;

        public ApiTestBase(WebApplicationFactory<Startup> webApplicationFactory)
        {
            _webApplicationFactory = webApplicationFactory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration(ConfigureAppConfiguration));
        }

        protected virtual void ConfigureAppConfiguration(IConfigurationBuilder configuration)
        {
            // For testing, we want the in memory database to be used so this can be run in CI/CD without spinning up a DB for it.
            // Alternatively, docker-compose could be used to also bring up SQL Server and connect the pods for testing.
            configuration.AddInMemoryCollection(new[] { new KeyValuePair<string, string>("UseInMemoryDatabase", "true") });
        }

        protected virtual HttpContent CreateObjectContent<T>(T model)
        {
            var json = JObject.FromObject(model).ToString(Formatting.None);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        protected virtual async Task<T> DeserializeResponseAsync<T>(HttpContent content)
        {
            JsonSerializer serializer = new JsonSerializer();

            using(var streamReader = new StreamReader(await content.ReadAsStreamAsync()))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                return serializer.Deserialize<T>(jsonReader);
            }
        }

        protected virtual HttpClient CreateClient()
        {
            var client = _webApplicationFactory.CreateClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }
    }
}
