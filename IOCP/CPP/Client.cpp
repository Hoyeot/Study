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

	//SOCKET clientSocket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP); // TCP ���� ����
	SOCKET clientSocket = WSASocket(AF_INET, SOCK_STREAM, IPPROTO_TCP, NULL, 0, WSA_FLAG_OVERLAPPED); // TCP ���� ����
	if (clientSocket == INVALID_SOCKET)
	{
		cout << "ERROR : " << WSAGetLastError() << endl;
		return false;
	}

	cout << "���� ���� ����" << endl;
	
	SOCKADDR_IN stServerAddr;

	char szOutMsg[MAX_BUFFER];
	char sz_socketbuf_[MAX_BUFFER]; 
	stServerAddr.sin_family = AF_INET;

	// ���� ��Ʈ �� IP ����
	stServerAddr.sin_port = htons(SERVER_PORT);
	stServerAddr.sin_addr.s_addr = inet_addr(SERVER_IP);
	//stServerAddr.sin_addr.s_addr = inet_pton(AF_INET, SERVER_IP, )
	

	//nRet = connect(clientSocket, (sockaddr*)&stServerAddr, sizeof(sockaddr));
	nRet = WSAConnect(clientSocket, (sockaddr*)&stServerAddr, sizeof(sockaddr), NULL, NULL, NULL, NULL);

	if (nRet == SOCKET_ERROR)
	{
		cout << "ERROR : ���� ���� ����" << WSAGetLastError() << endl;
		return false;
	}

	cout << "���� ����" << endl;


	while (true)
	{
		cout << "\n�Է� : ";
		cin.getline(szOutMsg, sizeof(szOutMsg)); // ���� ���� ������ getline���� �ٲ�
		//cin >> szOutMsg;

		if (_strcmpi(szOutMsg, "quit") == 0) break;

		int nSendLen = send(clientSocket, szOutMsg, strlen(szOutMsg), 0);

		if (nSendLen == -1)
		{
			cout << "ERROR : " << WSAGetLastError() << endl;
			return false;
		}

		//cout << "�޽��� ���� : bytes[" << nSendLen << "] << nSendLen<< [" << szOutMsg << "]" << endl;
		cout << "\n���� : " << szOutMsg << endl;

		int nRecvLen = recv(clientSocket, sz_socketbuf_, 1024, 0);

		if (nRecvLen == 0)
		{
			cout << "Ŭ���̾�Ʈ ���� ����" << endl;
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
		cout << "�۽� : " << sz_socketbuf_ << endl;
	}
	closesocket(clientSocket);
	cout << "Ŭ���̾�Ʈ ����" << endl;

	return 0;
}