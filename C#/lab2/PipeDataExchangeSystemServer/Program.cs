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

                    using (var fileStream = new FileStream("input.txt", FileMode.Append, FileAccess.Write))
                    using (var streamWriter = new StreamWriter(fileStream))
                    {
                        streamWriter.WriteLine($"{data.Value}, {data.Confirm}");
                    }
                }
            }, token);
        }

        private static Task ServerTask(NamedPipeServerStream pipeStream, CancellationToken token)
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

                            byte[] resBuffer = new byte[Unsafe.SizeOf<Data>()];
                            pipeStream.Read(resBuffer);

                            process.Add(MemoryMarshal.Read<Data>(resBuffer));
                        }
                    }
                    catch (Exception)
                    {
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

            Task server = ServerTask(pipeStream, cancellationToken);
            Task client = ClientTask(cancellationToken);

            await Task.WhenAll(server, client);
        }
    }
}











using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

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

                    using (var fileStream = new FileStream("input.txt", FileMode.Append, FileAccess.Write))
                    using (var streamWriter = new StreamWriter(fileStream))
                    {
                        streamWriter.WriteLine($"{data.Value}, {data.Confirm}");
                    }
                }
            }, token);
        }

        private static Task ServerTask(NamedPipeServerStream pipeStream, CancellationToken token)
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

                            byte[] resBuffer = new byte[Unsafe.SizeOf<Data>()];
                            pipeStream.Read(resBuffer);

                            process.Add(MemoryMarshal.Read<Data>(resBuffer));
                        }
                    }
                    catch (Exception)
                    {
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

            using var pipeStream = new NamedPipeServerStream("pipe", PipeDirection.InOut);
            pipeStream.WaitForConnection();

            Console.WriteLine("Клиент подключен");

            Task server = ServerTask(pipeStream, cancellationToken);
            Task client = ClientTask(cancellationToken);

            await Task.WhenAll(server, client);
        }
    }
}










private static Task ServerTask(NamedPipeServerStream pipeStream, CancellationToken token)
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

                    byte[] resBuffer = new byte[Unsafe.SizeOf<Data>()];
                    pipeStream.Read(resBuffer);

                    process.Add(MemoryMarshal.Read<Data>(resBuffer));

                    using (var fileStream = new FileStream("output.txt", FileMode.Append, FileAccess.Write))
                    {
                        fileStream.Write(resBuffer, 0, resBuffer.Length);
                    }
                }
            }
            catch (Exception)
            {
                // Handle exception
            }
        }

        foreach (var entity in process)
        {
            Console.WriteLine(entity);
        }
    }, token);
}














using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

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

                    using (var fileStream = new FileStream("input.txt", FileMode.Append, FileAccess.Write))
                    using (var streamWriter = new StreamWriter(fileStream))
                    {
                        streamWriter.WriteLine($"{data.Value}, {data.Confirm}");
                    }
                }
            }, token);
        }

        private static Task ServerTask(NamedPipeServerStream pipeStream, CancellationToken token)
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

                            byte[] resBuffer = new byte[Unsafe.SizeOf<Data>()];
                            pipeStream.Read(resBuffer);

                            process.Add(MemoryMarshal.Read<Data>(resBuffer));

                            using (var fileStream = new FileStream("output.txt", FileMode.Append, FileAccess.Write))
                            {
                                fileStream.Write(resBuffer, 0, resBuffer.Length);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Handle exception
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

            Task server = ServerTask(pipeStream, cancellationToken);
            Task client = ClientTask(cancellationToken);

            await Task.WhenAll(server, client);
        }
    }
}
