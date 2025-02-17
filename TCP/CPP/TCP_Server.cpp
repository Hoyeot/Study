#include <WinSock2.h> // Windows ���� ��� ����
#include <WS2tcpip.h> // IP �ּ� ��ȯ �Լ� ��� ����
#include <iostream>

#define IP "127.0.0.1" // IP
#define PORT 8000 // PORT
#define BUFF_SIZE 50 // ���� ũ��

using namespace std;

int main()
{
	WSAData wsaData;
	WSAStartup(MAKEWORD(2, 2), &wsaData);

	SOCKET hServerSock = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP); // TCP ���� ����

	if (hServerSock == INVALID_SOCKET)
	{
		cout << "Socket Create Error" << endl;
		exit(0);
	}

	// ���� �ּ�
	SOCKADDR_IN serverAddr = { 0, };
	serverAddr.sin_family = AF_INET;
	serverAddr.sin_port = htons(PORT);
	inet_pton(AF_INET, IP, &serverAddr.sin_addr);

	// ���ε�
	if (bind(hServerSock, (LPSOCKADDR)&serverAddr, sizeof(serverAddr)) == SOCKET_ERROR)
	{
		cout << "Bind Error" << WSAGetLastError() << endl;
		closesocket(hServerSock);
		WSACleanup();
		exit(0);
	}

	// ������
	if (listen(hServerSock, SOMAXCONN) == SOCKET_ERROR)
	{
		cout << "Listen Error" << WSAGetLastError() << endl;
		closesocket(hServerSock);
		WSACleanup();
		exit(0);
	}

	cout << "TCP SERVER START" << endl;

	// Ŭ���̾�Ʈ ���� ���
	SOCKET hClientSock;
	SOCKADDR_IN clientAddr;
	int clientADDrSize = sizeof(clientAddr);
	hClientSock = accept(hServerSock, (SOCKADDR*)&clientAddr, &clientADDrSize);

	if (hClientSock == INVALID_SOCKET)
	{
		cout << "Accept Error" << WSAGetLastError() << endl;
		closesocket(hServerSock);
		WSACleanup();
		exit(0);
	}

	cout << "Client Connected" << endl;

	char message[BUFF_SIZE];
	int recvLength;

	// ������ �ۼ��� ����
	while ((recvLength = recv(hClientSock, message, BUFF_SIZE, 0)) > 0)
	{
		message[recvLength] = '\0'; // ���ڿ� ����
		cout << "Received : " << message << endl;
		send(hClientSock, message, recvLength, 0); // ����
	}

	// ���� ����
	closesocket(hClientSock);
	closesocket(hServerSock);
	WSACleanup();
	return 0;
}