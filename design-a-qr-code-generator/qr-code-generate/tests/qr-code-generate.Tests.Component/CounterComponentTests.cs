using Bunit;
using Microsoft.Extensions.DependencyInjection;
using qr_code_generate.Components.Pages;
using qr_code_generate.Services;

namespace qr_code_generate.Tests.Component;

public class CounterComponentTests : BunitContext
{
    [Fact]
    public void Counter_Page_Should_Increment_Count_When_Button_Clicked()
    {
        Services.AddSingleton<ICounterService, CounterService>();

        var cut = Render<Counter>();

        Assert.Equal("Current count: 0", cut.Find("p").TextContent);

        cut.Find("button").Click();

        Assert.Equal("Current count: 1", cut.Find("p").TextContent);
    }
}
