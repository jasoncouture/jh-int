using BillManager.Api;
using BillManager.Core;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace BillManager.IntegrationTests
{

    public class PersonApiTests : ApiTestBase
    {
        public PersonApiTests(WebApplicationFactory<Startup> webApplicationFactory) : base(webApplicationFactory)
        {
        }


        [Fact]
        public void ApplicationStartsWithoutError()
        {
            var client = CreateClient();
            Assert.NotNull(client);
        }

        // Tests would normally be much smaller, but this depends on a sequence of operations.
        // So monolith test it is.
        // In addition, these tests can be relatively slow, since each test will setup and tear down the test host
        [Fact]
        public async Task PersonTestSuite()
        {
            var client = CreateClient();
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
