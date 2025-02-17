#include <WinSock2.h> // Windows ���� ��� ����
#include <WS2tcpip.h> // IP �ּ� ��ȯ �Լ� ��� ����
#include <iostream>

#define IP "127.0.0.1" // IP
#define PORT 8000 // PORT
#define BUFF_SIZE 50 // ���� ũ��

using namespace std;

int main()
{
	WSADATA wsaData;
	WSAStartup(MAKEWORD(2, 2), &wsaData);

	// TCP ���� ����
	SOCKET hClientSock = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
	if (hClientSock == INVALID_SOCKET)
	{
		cout << "Socket Error" << WSAGetLastError() << endl;
		WSACleanup();
		exit(0);
	}

	// ���� �ּ� ����
	SOCKADDR_IN servAddr = { 0, };
	servAddr.sin_family = AF_INET;
	servAddr.sin_port = htons(PORT);
	inet_pton(AF_INET, IP, &servAddr.sin_addr);

	// ������ ����
	if (connect(hClientSock, (LPSOCKADDR)&servAddr, sizeof(servAddr)) == SOCKET_ERROR)
	{
		cout << "Connect Error : " << WSAGetLastError() << endl;
		closesocket(hClientSock);
		WSACleanup();
		exit(0);
	}

	cout << "Server Connected!" << endl;

	char message[BUFF_SIZE]; // �ۼ����� ���� ����
	int sendLength = 0; // �۽ŵ� �������� ����

	// ���� ����
	while (1)
	{
		cout << "Input Text" << endl;
		fgets(message, sizeof(message), stdin); // �Է��� ���ۿ� ����

		if (!strcmp(message, "exit\n") || !strcmp(message, "EXIT\n")) break;

		sendLength = send(hClientSock, message, strlen(message), 0); // ������ ������ �۽�
		int recvLength = recv(hClientSock, message, BUFF_SIZE, 0); // �����κ��� ������ ����
		message[recvLength] = 0;
		cout << "Receive : " << message << endl;
	}

	// ����
	closesocket(hClientSock);
	WSACleanup();
	return 0;
}