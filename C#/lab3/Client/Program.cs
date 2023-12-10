using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Client;
class Program
{
    public struct DataForm
    {
        public double ArgumentOne;
        public double ArgumentTwo;
    }
    public struct IntegralResultScreen
    {
        public double IntegralResult;
    }
    static async Task Main(string[] args)
    {
        try
        {
            int i;
            var name = args[0];
            var stream = new NamedPipeClientStream(".", name, PipeDirection.InOut);
            await stream.ConnectAsync();

            byte[] array = new byte[Unsafe.SizeOf<DataForm>()];
            await stream.ReadAsync(array);
            var answer = MemoryMarshal.Read<DataForm>(array);

            var result = 0.0;
            double h = (answer.ArgumentTwo - answer.ArgumentOne) / 1000;
            double h2 = (answer.ArgumentTwo - answer.ArgumentOne) * h;

            for (i = 0; i < 1000; i++)
            {
                double xi = i * h + h2;
                result += -2 * Math.Sin(xi);
            }

            result *= h;
            byte[] spam = new byte[Unsafe.SizeOf<IntegralResultScreen>()];

            var writeResult = new IntegralResultScreen { IntegralResult = result };
            MemoryMarshal.Write(spam, ref writeResult);
            
            await stream.WriteAsync(spam);
        }
        catch
        {
        }
    }
}
