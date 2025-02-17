using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TCP_Server
{
    internal class TCP_Server
    {
        const string Ip = "127.0.0.1";
        const int Port = 8000;
        const int BUFF_SIZE = 50;
        static void Main(string[] args)
        {
            try
            {
                // TCP 소켓 생성 및 서버 시작
                TcpListener server = new TcpListener(IPAddress.Parse(Ip), Port);
                server.Start();
                Console.WriteLine("TCP Server Start");

                // 클라이언트 연결 대기
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Client Connected");

                // 네트워크 스트림 생성 (데이터 송수신을 위함)
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[BUFF_SIZE]; // 데이터 수신을 위한 버퍼
                int bytesRead; // 수신된 데이터 크기 저장

                try
                {
                    // 데이터 송수신 루프
                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead); // 받은 데이터를 문자열로 변환
                        Console.WriteLine("Received : " + receivedMessage);

                        stream.Write(buffer, 0, bytesRead); // 클라이언트에게 받은 데이터 다시 전송
                    }
                    // 클라이언트 연결 종료
                    client.Close();
                    server.Stop();
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
            catch (SocketException se)
            {
                Console.WriteLine($"ERROR : {se.Message}");
            }
        }
    }
}