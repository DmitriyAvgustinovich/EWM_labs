using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PipeServer
{
    public struct Data
    {
        public int Value;
        public bool Confirm;

        public override string ToString()
        {
            return $"Введенные данные = {Value}, Изменения = {Confirm}";
        }
    }

    public static class IntegrationCalculator
    {
        public static double TrapezoidalRule(double a, double b, int n)
        {
            double h = (b - a) / n;
            double result = (Function(a) + Function(b)) / 2.0;

            for (int i = 1; i < n; i++)
            {
                double x = a + i * h;
                result += Function(x);
            }

            return result * h;
        }

        private static double Function(double x)
        {
            return 2 * Math.Sin(x); // Замените на вашу функцию
        }
    }

    internal class Program
    {
        private static CancellationTokenSource cancellationTokenSource = new();
        private static CancellationToken cancellationToken = cancellationTokenSource.Token;
        private static PriorityQueue<Data, int> adQueue = new();
        private static Mutex mutex = new();

        private static Task ClientTask(CancellationToken token)
        {
            return Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    Console.WriteLine("Значение: ");
                    var value = Console.ReadLine();

                    Console.WriteLine("Приоритет: ");
                    var priority = Console.ReadLine();

                    var data = new Data { Value = Convert.ToInt32(value), Confirm = false };
                    mutex.WaitOne();

                    adQueue.Enqueue(data, Convert.ToInt32(priority));
                    mutex.ReleaseMutex();
                }
            }, token);
        }

        private static Task ServerTask(NamedPipeServerStream pipeStream, CancellationToken token, double a, double b, int n)
        {
            return Task.Run(() =>
            {
                List<Data> process = new();

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        if (adQueue.Count >= 1)
                        {
                            mutex.WaitOne();
                            var data = adQueue.Dequeue();
                            mutex.ReleaseMutex();

                            byte[] buffer = new byte[Unsafe.SizeOf<Data>()];
                            MemoryMarshal.Write(buffer, ref data);

                            pipeStream.Write(buffer);

                            double integralResult = IntegrationCalculator.TrapezoidalRule(a, b, n);
                            Console.WriteLine($"Приближенное значение интеграла: {integralResult}");

                            using (var fileStream = new FileStream("output.txt", FileMode.Append, FileAccess.Write))
                            using (var streamWriter = new StreamWriter(fileStream))
                            {
                                streamWriter.WriteLine($"Integral Result: {integralResult}");
                            }

                            byte[] resBuffer = new byte[Unsafe.SizeOf<Data>()];
                            MemoryMarshal.Write(resBuffer, ref data);

                            pipeStream.Read(resBuffer);

                            process.Add(MemoryMarshal.Read<Data>(resBuffer));

                            using (var fileStream = new FileStream("input.txt", FileMode.Append, FileAccess.Write))
                            using (var streamWriter = new StreamWriter(fileStream))
                            {
                                streamWriter.WriteLine($"{data.Value}, {data.Confirm}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка в сервере: {ex.Message}");
                    }
                }

                foreach (var entity in process)
                {
                    Console.WriteLine(entity);
                }
            }, token);
        }

        static async Task Main(string[] args)
        {
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cancellationTokenSource.Cancel();
            };

            Console.WriteLine("Ожидание клиента...");

            var pipeStream = new NamedPipeServerStream("pipe", PipeDirection.InOut);
            pipeStream.WaitForConnection();

            Console.WriteLine("Клиент подключен");

            double a = 0;
            double b = Math.PI;
            int n = 1000;

            Task server = ServerTask(pipeStream, cancellationToken, a, b, n);
            Task client = ClientTask(cancellationToken);

            await Task.WhenAll(server, client);
        }
    }
}
