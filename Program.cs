var random = new Random();
while (true)
{
    Lazy<string> maybeBreaks = new Lazy<string>(() =>
    {
        var v = random.Next(100);
        return v < 50 ? $"lazy-{v}" : throw new InvalidOperationException($"Failed lazy-{v}");
    }, System.Threading.LazyThreadSafetyMode.PublicationOnly);

    Parallel.For(0, 10, n =>
    {
        try
        {
            Console.WriteLine("{0} succeeded with value: {1}", n, maybeBreaks.Value);
        }
        catch (Exception ex)
        {
            Console.WriteLine("{0} got exception {1}", n, ex);
        }
    });
    Console.ReadKey();
    Console.Clear();
}