using Reqnroll;

namespace qr_code_generate.Tests.Acceptance.Steps;

[Binding]
public sealed class SmokeSteps
{
    private bool _configured;

    [Given("the acceptance test stack is configured")]
    public void GivenTheAcceptanceTestStackIsConfigured()
    {
        _configured = true;
    }

    [Then("the acceptance project runs")]
    public void ThenTheAcceptanceProjectRuns()
    {
        Assert.True(_configured);
    }
}
