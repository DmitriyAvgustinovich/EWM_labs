using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PipeServer
{
    public struct DataForm
    {
        public double ArgumentOne;
        public double ArgumentTwo;
    }

    public struct IntegralResultScreen
    {
        public double IntegralResult;

        public override string ToString() => $"Результат вычисления интеграла = {IntegralResult}";
    }

    internal class Program
    {
        private static int id = 0;
        private static CancellationTokenSource up = new CancellationTokenSource();
        private static CancellationToken token = up.Token;
        private static PriorityQueue<DataForm, int> queue = new PriorityQueue<DataForm, int>();
        private static Mutex mutex = new Mutex();

        private static async Task ClientTask(CancellationToken token)
        {
            await Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    var data = new DataForm();
                    Console.WriteLine($"Введите первый аргумент -> ");

                    var readArgumentOne = Console.ReadLine();
                    if (!double.TryParse(readArgumentOne, out double argumentOne))
                    {
                        Console.WriteLine("Вводить можно только цифры\n");
                        continue;
                    }

                    else data.ArgumentOne = argumentOne;
                    Console.WriteLine($"Введите второй аргумент -> ");

                    var readArgumentTwo = Console.ReadLine();
                    if (!double.TryParse(readArgumentTwo, out double argumentTwo))
                    {
                        Console.WriteLine("Вводить можно только цифры\n");
                        continue;
                    }

                    else data.ArgumentTwo = argumentTwo;
                    await EnqueueDataAsync(data);
                }
            }, token);
        }

        private static async Task EnqueueDataAsync(DataForm data)
        {
            await Task.Run(() =>
            {
                mutex.WaitOne();
                queue.Enqueue(data, 1);
                mutex.ReleaseMutex();
            });
        }

        private static async Task ServerTask(CancellationToken token)
        {
            await Task.Run(async () =>
            {
                List<IntegralResultScreen> count = new();
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        if (queue.Count >= 1)
                        {
                            mutex.WaitOne();
                            var data = queue.Dequeue();
                            mutex.ReleaseMutex();
                            await Client(token, count, data);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

                foreach (var exampleIntegral in count)
                {
                    Console.WriteLine(exampleIntegral);
                }
            }, token);
        }

        private static async Task Client(CancellationToken token, List<IntegralResultScreen> exampleIntegral, DataForm data)
        {
            id++;
            string name = $"tonel_{id}";
            using (Process myProcess = new Process())
            {

                myProcess.StartInfo.FileName = "C:\\Users\\User\\Desktop\\EWM_labs\\C#\\lab3\\PipeDataExchangeSystemClient\\bin\\Debug\\net7.0\\Client.exe";
                myProcess.StartInfo.Arguments = name;
                myProcess.Start();

                var stream = new NamedPipeServerStream($"{name}", PipeDirection.InOut);
                await stream.WaitForConnectionAsync();

                byte[] spam = new byte[Unsafe.SizeOf<DataForm>()];
                MemoryMarshal.Write(spam, ref data);
                await stream.WriteAsync(spam, token);

                byte[] array = new byte[Unsafe.SizeOf<IntegralResultScreen>()];
                await stream.ReadAsync(array, token);

                exampleIntegral.Add(MemoryMarshal.Read<IntegralResultScreen>(array));

                myProcess.WaitForExit();
            }
        }

        static async Task Main(string[] args)
        {
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                up.Cancel();
            };
            Console.WriteLine("Клиент подключен\n");

            Task task_1 = ServerTask(token);
            Task task_2 = ClientTask(token);
            await Task.WhenAll(task_1, task_2);
        }
    }
}