using ParProg.WavDecoder;

public static class Program
{
    public static void Main(string[] args)
    {
        var FILE_PATH = string.Empty;
        var TARGET = 16000;
        var THREAD_COUNT = 4;

        if (args.Length == 0)
        {
            Console.WriteLine("Передайте путь к файлу!");
            return;
        }

        if (!File.Exists(args[0]))
        {
            Console.WriteLine($"Файл {args[0]} не существует!");
            return;
        }

        FILE_PATH = args[0];

        switch (args.Length)
        {
            case > 1 when !int.TryParse(args[1], out TARGET):
                Console.WriteLine($"Параметр {args[1]} должен быть числом!");
                return;
            case > 2 when !int.TryParse(args[2], out THREAD_COUNT):
                Console.WriteLine($"Не могу создать {args[2]} число потоков!");
                break;
        }

        var wavDecoder = new WavDecoder(FILE_PATH);
        var description = wavDecoder.Describe();
        Console.WriteLine(description);

        var data = wavDecoder.Decode();

        var chunks = new List<List<float>>(THREAD_COUNT);
        var tasks = new List<Task<(int, int)>>(THREAD_COUNT);

        for (var i = 0; i < THREAD_COUNT; i++)
        {
            chunks.Add(new List<float>(data.Count / THREAD_COUNT));
        }

        using var dataEnum = data.GetEnumerator();
        var j = 0;

        while (dataEnum.MoveNext())
        {
            chunks[j % THREAD_COUNT].Add(dataEnum.Current);
            j++;
        }

        foreach (var chunk in chunks)
        {
            tasks.Add(Task.Factory.StartNew(() => CountAbs(chunk, TARGET)));
        }

        Console.WriteLine("Thread id: " + Environment.CurrentManagedThreadId);

        Task.WaitAll(tasks.ToArray());

        var leq = 0;
        var gt = 0;

        foreach (var (x, y) in tasks.Select(t => t.Result))
        {
            leq += x;
            gt += y;
        }

        Console.WriteLine(new string('-', 20));
        Console.WriteLine("Unfillterd datas:");
        Console.WriteLine($"Lenght: {data.Count} | Min: {data.Min()} | Max: {data.Max()}");

        Console.WriteLine("Filltered:");
        Console.WriteLine($"< {TARGET}: {leq} | > {TARGET}: {gt} | sum: {gt + leq}");
    }

    public static (int, int) CountAbs(IEnumerable<float> data, float target)
    {
        var leq = 0;
        var gt = 0;
        foreach (var value in data)
        {
            if (Math.Abs(value) > target) gt += 1;
            else leq += 1;
        }

        Console.WriteLine("Thread id: " + Environment.CurrentManagedThreadId);
        return (leq, gt);
    }
}