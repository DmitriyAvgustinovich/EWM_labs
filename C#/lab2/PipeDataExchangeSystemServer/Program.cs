using System.IO.Pipes;
using System.Text;

namespace PipeServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            using NamedPipeServerStream pipeServer = new("testpipe", PipeDirection.InOut);

            Console.WriteLine("Ожидание клиента...");
            pipeServer.WaitForConnection();

            Console.WriteLine("Клиент подключен.");

            var dataQueue = new PriorityQueue<CustomData, int>(Comparer<int>.Create((x, y) => y.CompareTo(x)));
            var cancellationTokenSource = new CancellationTokenSource();

            var inputDataTask = Task.Run(() => InputData(dataQueue, cancellationTokenSource.Token));
            var processDataTask = Task.Run(() => ProcessData(dataQueue, pipeServer, cancellationTokenSource.Token));

            Console.WriteLine("Для завершения нажмите Ctrl+C.");
            Console.CancelKeyPress += (s, e) => {
                e.Cancel = true;
                cancellationTokenSource.Cancel();
            };

            await Task.WhenAll(inputDataTask, processDataTask);
        }

        public struct CustomData
        {
            public int Field1 { get; set; }
            public int Field2 { get; set; }
        }

        static async Task InputData(PriorityQueue<CustomData, int> dataQueue, CancellationToken cancellationToken)
        {
            Random random = new Random();

            while (!cancellationToken.IsCancellationRequested)
            {
                var data = new CustomData
                {
                    Field1 = random.Next(100),
                    Field2 = random.Next(100),
                };
                dataQueue.Enqueue(data, random.Next(5));
                Console.WriteLine($"Добавлены данные: Field1: {data.Field1}, Field2: {data.Field2}");
                await Task.Delay(1000);
            }
        }

        static async Task ProcessData(PriorityQueue<CustomData, int> dataQueue, NamedPipeServerStream pipeServer, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (dataQueue.TryDequeue(out var data, out _))
                {
                    byte[] sendData = SerializeData(data);
                    pipeServer.Write(sendData, 0, sendData.Length);
                    Console.WriteLine($"Отправлены данные: Field1: {data.Field1}, Field2: {data.Field2}");
                }
                await Task.Delay(100);
            }
        }

        static byte[] SerializeData(CustomData data)
        {
            byte[] buffer = new byte[8];
            Span<byte> span = new(buffer);
            span[0] = (byte)data.Field1;
            span[1] = (byte)(data.Field1 >> 8);
            span[2] = (byte)data.Field2;
            span[3] = (byte)(data.Field2 >> 8);
            return buffer;
        }
    }
}
