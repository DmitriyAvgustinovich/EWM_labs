using System.IO.Pipes;
using System.Text;

namespace PipeClient
{
    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            using NamedPipeClientStream pipeClient = new(".", "testpipe", PipeDirection.InOut);

            pipeClient.Connect();
            Console.WriteLine("Соединение установлено.");

            byte[] buffer = new byte[1000];
            pipeClient.Read(buffer, 0, buffer.Length);
            var receivedData = DeserializeData(buffer);

            Console.WriteLine("Получены данные от сервера:");
            Console.WriteLine($"Field1: {receivedData.Field1}, Field2: {receivedData.Field2}");

            var responseData = new CustomData { Field1 = 100, Field2 = 200 };
            byte[] sendData = SerializeData(responseData);
            pipeClient.Write(sendData, 0, sendData.Length);
        }

        public struct CustomData
        {
            public int Field1 { get; set; }
            public int Field2 { get; set; }
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

        static CustomData DeserializeData(byte[] bytes)
        {
            Span<byte> span = new(bytes);
            int field1 = span[0] | (span[1] << 8);
            int field2 = span[2] | (span[3] << 8);
            return new CustomData { Field1 = field1, Field2 = field2 };
        }
    }
}
