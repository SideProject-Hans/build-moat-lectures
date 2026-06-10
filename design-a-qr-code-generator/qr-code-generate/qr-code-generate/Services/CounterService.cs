namespace qr_code_generate.Services;

public interface ICounterService
{
    int Increment(int currentCount);
}

public sealed class CounterService : ICounterService
{
    public int Increment(int currentCount)
    {
        return currentCount + 1;
    }
}
