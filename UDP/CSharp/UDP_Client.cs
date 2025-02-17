using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UDP_Client
{
    internal class UDP_Client
    {
        const int PortNumber = 8000;

        static void Main(string[] args)
        {
            Socket ClientSocket = null;
            try
            {
                 ClientSocket = new Socket(
                    AddressFamily.InterNetwork, // Ipv4
                    SocketType.Dgram, // 데이터그램 : 패킷 교환 네트워크와 관련된 기본 전송 단위
                    ProtocolType.Udp // UDP 방식
                    );

                EndPoint localEndPoint = new IPEndPoint(IPAddress.Any, PortNumber); // IP종단점
                EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Loopback, PortNumber); // 원격 IP종단점
                // EndPoint remoteEndPoint = new IPEndPoint(IPAdress.Parse("서버IP", PortNumber); // 서버가 다른 머신에서 실행 중이라면...

                while (true)
                {
                    Console.WriteLine("Input Text");
                    byte[] sendBuffer = Encoding.UTF8.GetBytes(Console.ReadLine());

                    if (Encoding.UTF8.GetString(sendBuffer) == "exit" || Encoding.UTF8.GetString(sendBuffer) == "EXIT")
                    {
                        break;
                    }
                    ClientSocket.SendTo(sendBuffer, remoteEndPoint);

                    byte[] receiveBuffer = new byte[1024];
                    int receivedSize = ClientSocket.ReceiveFrom(receiveBuffer, ref remoteEndPoint);
                    Console.Write("Echo : ");
                    Console.WriteLine(Encoding.UTF8.GetString(receiveBuffer, 0, receivedSize));
                }
                ClientSocket.Close();
            }
            catch (SocketException se)
            {
                Console.WriteLine($"ERROR : {se.Message}");
                ClientSocket.Close();
            }
            finally
            {
                if (ClientSocket != null)
                {
                    ClientSocket.Close();
                }
            }
        }
    }
}
