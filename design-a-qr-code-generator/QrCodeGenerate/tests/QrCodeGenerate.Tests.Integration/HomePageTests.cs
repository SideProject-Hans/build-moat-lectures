using Microsoft.AspNetCore.Mvc.Testing;

namespace QrCodeGenerate.Tests.Integration;

public class HomePageTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HomePageTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Home_Page_Should_Return_Ok()
    {
        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/");

        Assert.True(response.IsSuccessStatusCode);
    }
}
