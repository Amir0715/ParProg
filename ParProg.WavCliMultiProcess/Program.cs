using System.Diagnostics;
using System.IO.Pipes;
using ParProg.WavDecoder;

public static class Program
{
    public static void Main(string[] args)
    {
        var FILE_PATH = string.Empty;
        var TARGET = 16000;
        var PROCESS_COUNT = 4;
        var IS_CHILD = false;
        if (args.Length == 0)
        {
            Console.WriteLine("Передайте путь к файлу!");
            return;
        }

        IS_CHILD = args[0].ToLower() == "child";
        if (IS_CHILD)
        {
            ChildProcess(args);
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
            case > 2 when !int.TryParse(args[2], out PROCESS_COUNT):
                Console.WriteLine($"Не могу создать {args[2]} число потоков!");
                break;
        }
        
        var wavDecoder = new WavDecoder(FILE_PATH);
        var description = wavDecoder.Describe();
        Console.WriteLine(description);

        var data = wavDecoder.Decode();
        var chunks = GetChunks(data, PROCESS_COUNT);

        ParrentProcess(TARGET, PROCESS_COUNT, chunks);
    }

    private static void ParrentProcess(int TARGET, int PROCESS_COUNT, List<List<float>> chunks)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        
        // Создаем анонимнный канал
        using var pipeServerStream = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable, 1024 * 1024);
        
        // Создаем именнованные канал
        // using var pipeServerStream = new NamedPipeServerStream("pipeTest", PipeDirection.Out, 2, PipeTransmissionMode.Byte);
        
        Console.WriteLine("[SERVER] Current TransmissionMode: {0}.", pipeServerStream.TransmissionMode);

        var executablePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var procInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            ArgumentList = { executablePath, "child", pipeServerStream.GetClientHandleAsString(), TARGET.ToString()},
            // ArgumentList = { executablePath, "child", "pipeTest", TARGET.ToString()},
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };

        var process = Process.Start(procInfo);
        if (process == null)
        {
            Console.WriteLine("Can't start the process");
            return;
        }

        pipeServerStream.DisposeLocalCopyOfClientHandle();
        try
        {
            // Read user input and send that to the client process.
            using var sw = new StreamWriter(pipeServerStream);
            
            sw.AutoFlush = true;
            
            // Send a 'sync message' and wait for client to receive it.
            sw.WriteLine("SYNC");
            pipeServerStream.WaitForPipeDrain();
            Console.WriteLine("CONNECTED");
            
            foreach (var chunk in chunks)
            {
                // GetChunks(chunk, chunk)
                sw.WriteLine(chunk.Count);
                Console.WriteLine(chunk.Count);
                foreach (var value in chunk)
                {
                    sw.WriteLine(value);
                }
                Console.WriteLine("END TRANSFER");
                break;
            }

            sw.WriteLine("END");
        }
        // Catch the IOException that is raised if the pipe is broken
        // or disconnected.
        catch (IOException e)
        {
            Console.WriteLine("[SERVER] Error: {0}", e.Message);
        }
        
        var output = process.StandardOutput.ReadToEnd();
        Console.WriteLine(output);
        stopWatch.Stop();
        Console.WriteLine(stopWatch.Elapsed.TotalMilliseconds);
        process.WaitForExit();
        process.Close();
    }


    private static List<List<T>> GetChunks<T>(List<T> array, int chunkCount)
    {
        if (chunkCount <= 1) throw new ArgumentOutOfRangeException(nameof(chunkCount));
        
        var chunks = new List<List<T>>(chunkCount);
        for (var i = 0; i < chunkCount; i++)
        {
            chunks.Add(new List<T>(array.Count / chunkCount));
        }

        using var dataEnum = array.GetEnumerator();
        var j = 0;
        while (dataEnum.MoveNext())
        {
            chunks[j % chunkCount].Add(dataEnum.Current);
            j++;
        }

        return chunks;
    }

    private static void ChildProcess(string[] args)
    {
        try
        {
            using PipeStream pipeClient = new AnonymousPipeClientStream(PipeDirection.In, args[1]);
            // var pipeClient = new NamedPipeClientStream(".", args[1], PipeDirection.In, PipeOptions.None, TokenImpersonationLevel.Impersonation);
            // pipeClient.Connect();
            var TARGET = float.Parse(args[2]);

            using var sr = new StreamReader(pipeClient);

            // Display the read text to the console
            string temp;
            var gt = 0;
            var leq = 0;
            var count = 0;

            // Wait for 'sync message' from the server.
            
            while (true)
            {
                temp = sr.ReadLine();
                if (temp == "SYNC")
                    break;
            }

            // var dataSize = int.Parse(sr.ReadLine().Trim());
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            // Read the server data and echo to the console.
            
            while (true)
            {
                temp = sr.ReadLine();
                if (temp == "END")
                    break;

                if (!float.TryParse(temp.Trim(), out var value)) continue;
                //
                count++;
                if (value > TARGET)
                    gt++;
                else
                    leq++;
            }
            stopWatch.Stop();
            Console.WriteLine($"{gt} {leq} {count} {stopWatch.Elapsed.TotalMilliseconds}");
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.ToString());
        }
    }
}