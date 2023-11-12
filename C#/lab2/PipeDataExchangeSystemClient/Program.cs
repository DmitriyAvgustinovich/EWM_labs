using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PipeClient
{
    public struct Data
    {
        public int Value;
        public bool Confirm;
    }

    class Program
    {
        static void Main()
        {
            try
            {
                Console.WriteLine("Соединение с сервером...");

                var pipeStream = new NamedPipeClientStream(".", "pipe", PipeDirection.InOut);
                pipeStream.Connect();

                Console.WriteLine("Соединение установлено");
                Console.WriteLine("Ожидание данных...");

                while (true)
                {
                    byte[] buffer = new byte[Unsafe.SizeOf<Data>()];
                    pipeStream.Read(buffer);

                    var data = MemoryMarshal.Read<Data>(buffer);
                    Console.WriteLine($"Получено: {data.Value}, {data.Confirm}");

                    data.Confirm = true;
                    Console.WriteLine($"Отправлено: {data.Value}, {data.Confirm}");

                    byte[] resBuffer = new byte[Unsafe.SizeOf<Data>()];
                    MemoryMarshal.Write(resBuffer, ref data);

                    using (var fileStream = new FileStream("output.txt", FileMode.Append, FileAccess.Write))
                    {
                        fileStream.Write(resBuffer, 0, resBuffer.Length);
                    }
                    
                    pipeStream.Write(resBuffer);
                }
            }
            catch
            {
            }
        }
    }
}
