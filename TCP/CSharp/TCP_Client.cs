using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TCP_Client
{
    internal class TCP_Client
    {
        static void Main(string[] args)
        {
            const string Ip = "127.0.0.1";
            const int Port = 8000;
            const int BUFF_SIZE = 50;

            try
            {
                // 서버와 연결할 TCP 클라이언트 생성
                TcpClient client = new TcpClient();
                client.Connect(Ip, Port); // 서버와 연결 시도
                Console.WriteLine("Server Connected!");

                // 네트워크 스트림 생성
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[BUFF_SIZE];

                while (true)
                {
                    Console.Write("Input Text : ");
                    string message = Console.ReadLine();
                    if (message.Equals("exit", StringComparison.OrdinalIgnoreCase)) break; // exit 입력 시 종료

                    // 서버로 메시지 전송
                    byte[] data = Encoding.UTF8.GetBytes(message); // 입력된 메시지를 바이트 배열로 변환 후 서버로 전송
                    stream.Write(data, 0, data.Length);

                    // 서버로부터 메시지 수신
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("Receive : " + receivedMessage);
                }
                // 종료
                client.Close();
            }
            catch (SocketException se)
            {
                Console.WriteLine($"ERROR : {se.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR : {ex.Message}");
            }
        }
    }
}