using ParProg.WavDecoder;

(int, int) CountAbs(float[] data, float target)
{
    var min = 0;
    var max = 0;
    foreach (var value in data)
    {
        if (Math.Abs(value) > target) max += 1;
        else min += 1;
    }

    return (min, max);
}


var file = "./fileSample1.wav";
var target = 16000;

var wavDecoder = new WavDecoder(file);
var description = wavDecoder.Describe();

Console.WriteLine(description);
var data = wavDecoder.Decode();
Console.WriteLine("Unfillterd datas:");
Console.WriteLine($"Lenght: {data.Length} | Min: {data.Min()} | Max: {data.Max()}");
var (min, max) = CountAbs(data, target);

Console.WriteLine("Filltered:");
Console.WriteLine($"< {target}: {min} | > {target}: {max} | sum: {min + max}");