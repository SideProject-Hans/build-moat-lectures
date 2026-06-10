using qr_code_generate.Services;

namespace qr_code_generate.Tests.Unit;

public class CounterServiceTests
{
    [Fact]
    public void Increment_Should_Return_Next_Integer()
    {
        var service = new CounterService();

        var result = service.Increment(3);

        Assert.Equal(4, result);
    }
}
