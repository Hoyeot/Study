#include <WinSock2.h>
#include <Ws2tcpip.h>
#include <iostream>

using namespace std;

#define MAX_BUFFER	1024
#define SERVER_PORT	8000
#define SERVER_IP	"127.0.0.1"

struct CLIENTSOCKETINFO
{
	WSAOVERLAPPED	overlapped;
	WSABUF			dataBuf;
	SOCKET			socket;
	char			messageBuffer[MAX_BUFFER];
	int				recvBytes;
	int				snedBytes;
};


int main()
{
	WSADATA wsaData;
	int nRet = WSAStartup(MAKEWORD(2, 2), &wsaData);

	if (nRet != 0)
	{
		cout << "ERROR : " << WSAGetLastError() << endl;
		return false;
	}

	//SOCKET clientSocket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP); // TCP 소켓 생성
	SOCKET clientSocket = WSASocket(AF_INET, SOCK_STREAM, IPPROTO_TCP, NULL, 0, WSA_FLAG_OVERLAPPED); // TCP 소켓 생성
	if (clientSocket == INVALID_SOCKET)
	{
		cout << "ERROR : " << WSAGetLastError() << endl;
		return false;
	}

	cout << "소켓 생성 성공" << endl;
	
	SOCKADDR_IN stServerAddr;

	char szOutMsg[MAX_BUFFER];
	char sz_socketbuf_[MAX_BUFFER]; 
	stServerAddr.sin_family = AF_INET;

	// 서버 포트 및 IP 세팅
	stServerAddr.sin_port = htons(SERVER_PORT);
	stServerAddr.sin_addr.s_addr = inet_addr(SERVER_IP);
	//stServerAddr.sin_addr.s_addr = inet_pton(AF_INET, SERVER_IP, )
	

	//nRet = connect(clientSocket, (sockaddr*)&stServerAddr, sizeof(sockaddr));
	nRet = WSAConnect(clientSocket, (sockaddr*)&stServerAddr, sizeof(sockaddr), NULL, NULL, NULL, NULL);

	if (nRet == SOCKET_ERROR)
	{
		cout << "ERROR : 서버 접속 실패" << WSAGetLastError() << endl;
		return false;
	}

	cout << "접속 성공" << endl;


	while (true)
	{
		cout << "\n입력 : ";
		cin.getline(szOutMsg, sizeof(szOutMsg)); // 띄어쓰기 버퍼 때문에 getline으로 바꿈
		//cin >> szOutMsg;

		if (_strcmpi(szOutMsg, "quit") == 0) break;

		int nSendLen = send(clientSocket, szOutMsg, strlen(szOutMsg), 0);

		if (nSendLen == -1)
		{
			cout << "ERROR : " << WSAGetLastError() << endl;
			return false;
		}

		//cout << "메시지 전송 : bytes[" << nSendLen << "] << nSendLen<< [" << szOutMsg << "]" << endl;
		cout << "\n전송 : " << szOutMsg << endl;

		int nRecvLen = recv(clientSocket, sz_socketbuf_, 1024, 0);

		if (nRecvLen == 0)
		{
			cout << "클라이언트 접속 종료" << endl;
			closesocket(clientSocket);
			return false;
		}
		else if (nRecvLen == -1)
		{
			cout << "ERROR : " << WSAGetLastError() << endl;
			closesocket(clientSocket);
			return false;
		}

		sz_socketbuf_[nRecvLen] = NULL;
		cout << "송신 : " << sz_socketbuf_ << endl;
	}
	closesocket(clientSocket);
	cout << "클라이언트 종료" << endl;

	return 0;
}