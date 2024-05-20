using QuizerTests.Functional;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Assert = Xunit.Assert;

namespace Quizer.Functional.Tests
{

    [Collection("Sequential")]
    public class HomePageOnGet : IClassFixture<WebTestFixture>
    {
        public HomePageOnGet(WebTestFixture factory)
        {
            Client = factory.CreateClient();
        }

        public HttpClient Client { get; }

        //[Fact]
        //public async Task ReturnsHomeView()
        //{
        //    // Arrange & Act
        //    var response = await Client.GetAsync("/");
        //    response.EnsureSuccessStatusCode();
        //    var stringResponse = await response.Content.ReadAsStringAsync();

        //    // Assert
        //    Assert.Contains("building Web apps with ASP.NET Core", stringResponse);
        //}
    }
}