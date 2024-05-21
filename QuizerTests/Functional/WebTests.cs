using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace QuizerTests.Functional
{
    public class WebTests : IClassFixture<WebApplicationFactory<Program>>
    {
        protected readonly HttpClient _client;

        public WebTests(WebApplicationFactory<Quizer.Program> factory)
        {
            _client = factory.CreateClient();
        }

        // write tests that use _client
    }
}
