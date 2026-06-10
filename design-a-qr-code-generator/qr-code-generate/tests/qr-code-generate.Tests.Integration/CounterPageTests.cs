using Microsoft.AspNetCore.Mvc.Testing;

namespace qr_code_generate.Tests.Integration;

public class CounterPageTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CounterPageTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Counter_Page_Should_Return_Ok()
    {
        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/counter");

        Assert.True(response.IsSuccessStatusCode);
    }
}
