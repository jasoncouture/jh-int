using BillManager.Api;
using BillManager.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BillManager.IntegrationTests
{
    public class PersonApiTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private WebApplicationFactory<Startup> _webApplicationFactory;

        public PersonApiTests(WebApplicationFactory<Startup> webApplicationFactory)
        {
            _webApplicationFactory = webApplicationFactory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration(ConfigureAppConfiguration));
        }

        private void ConfigureAppConfiguration(IConfigurationBuilder configuration)
        {
            // For testing, we want the in memory database to be used.
            configuration.AddInMemoryCollection(new[] { new KeyValuePair<string, string>("UseInMemoryDatabase", "true") });
        }

        [Fact]
        public void ApplicationStartsWithoutError()
        {
            var client = _webApplicationFactory.CreateClient();
            Assert.NotNull(client);
        }

        private HttpContent CreateObjectContent<T>(T model)
        {
            var json = JObject.FromObject(model).ToString(Formatting.None);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        private async Task<T> DeserializeResponseAsync<T>(HttpContent content)
        {
            JsonSerializer serializer = new JsonSerializer();

            using(var streamReader = new StreamReader(await content.ReadAsStreamAsync()))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                return serializer.Deserialize<T>(jsonReader);
            }
        }

        // Tests would normally be much smaller, but this depends on a sequence of operations.
        // So monolith test it is.
        // In addition, these tests can be relatively slow, since each test will setup and tear down the test host
        [Fact]
        public async Task PersonTestSuite()
        {
            var client = _webApplicationFactory.CreateClient();
            Assert.NotNull(client);
            var personToCreate = new PersonModel
            {
                Name = "Tester McTesterson"
            };
            const string expectedReplacementName = "Tester";
            var httpContent = CreateObjectContent(personToCreate);
            var response = await client.PostAsync("/person", httpContent);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var personCreated = await DeserializeResponseAsync<PersonModel>(response.Content);

            Assert.NotNull(personCreated);
            Assert.Equal(personToCreate.Name, personCreated.Name);
            Assert.NotEqual(0, personCreated.Id);

            personToCreate = personToCreate with { Name = expectedReplacementName };
            var expectedId = personCreated.Id;

            httpContent = CreateObjectContent(personToCreate);
            response = await client.PostAsync($"/person/{personToCreate.Id}", httpContent);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            personCreated = await DeserializeResponseAsync<PersonModel>(response.Content);
            Assert.NotNull(personCreated);
            Assert.Equal(personToCreate.Name, personCreated.Name);
            Assert.NotEqual(expectedId, personCreated.Id);

            response = await client.GetAsync($"/person/{personCreated.Id}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            personCreated = await DeserializeResponseAsync<PersonModel>(response.Content);
            Assert.NotNull(personCreated);
            Assert.Equal(personToCreate.Name, personCreated.Name);
            Assert.NotEqual(expectedId, personCreated.Id);


            response = await client.DeleteAsync($"/person/{personCreated.Id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await client.GetAsync($"/person/{personCreated.Id}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
