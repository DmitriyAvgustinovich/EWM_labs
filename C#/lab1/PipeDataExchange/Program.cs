using System.IO.Pipes;
using System.Text;

namespace PipeServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            using NamedPipeServerStream pipeServer = new("testpipe", PipeDirection.InOut);

            Console.WriteLine("Ожидание клиента...");
            pipeServer.WaitForConnection();

            Console.WriteLine("Клиент подключен.");

            var data = new DataStruct { Field1 = 10, Field2 = 20 };
            byte[] sendData = DataStructToBytes(data);
            pipeServer.Write(sendData, 0, sendData.Length);

            byte[] buffer = new byte[1000];
            int bytesRead = pipeServer.Read(buffer, 0, buffer.Length);
            var receivedData = BytesToDataStruct(buffer);

            Console.WriteLine("Получены данные от клиента:");
            Console.WriteLine($"Field1: {receivedData.Field1}, Field2: {receivedData.Field2}");
        }

        public struct DataStruct
        {
            public int Field1;
            public int Field2;
        }

        static byte[] DataStructToBytes(DataStruct data)
        {
            int size = sizeof(int) * 2;
            byte[] arr = new byte[size];
            BitConverter.GetBytes(data.Field1).CopyTo(arr, 0);
            BitConverter.GetBytes(data.Field2).CopyTo(arr, sizeof(int));
            return arr;
        }

        static DataStruct BytesToDataStruct(byte[] bytes)
        {
            DataStruct data;
            data.Field1 = BitConverter.ToInt32(bytes, 0);
            data.Field2 = BitConverter.ToInt32(bytes, sizeof(int));
            return data;
        }
    }
}
