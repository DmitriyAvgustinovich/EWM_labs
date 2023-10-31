using System.IO.Pipes;
using System.Text;

namespace PipeClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            using NamedPipeClientStream pipeClient = new(".", "testpipe", PipeDirection.InOut);

            pipeClient.Connect();
            Console.WriteLine("Соединение установлено.");

            var buffer = new List<byte>();

            byte[] readBuffer = new byte[12];

            while (true)
            {
                int bytesRead = pipeClient.Read(readBuffer, 0, readBuffer.Length);
                if (bytesRead > 0)
                {
                    buffer.AddRange(readBuffer);
                    if (buffer.Count >= 12)
                    {
                        var receivedData = DeserializeData(buffer.GetRange(0, 12).ToArray());
                        Console.WriteLine("Получены данные от сервера:");
                        Console.WriteLine($"Field1: {receivedData.Field1}, Field2: {receivedData.Field2}, Priority: {receivedData.Priority}");
                        buffer.RemoveRange(0, 12);
                    }
                }
            }
        }

        public struct CustomData
        {
            public int Field1 { get; set; }
            public int Field2 { get; set; }
            public int Priority { get; set; }
        }

        static CustomData DeserializeData(byte[] bytes)
        {
            Span<byte> span = new(bytes);
            int field1 = span[0] | (span[1] << 8);
            int field2 = span[2] | (span[3] << 8);
            int priority = span[4] | (span[5] << 8);
            return new CustomData { Field1 = field1, Field2 = field2, Priority = priority };
        }
    }
}
