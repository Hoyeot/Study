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
	WSAStartup(MAKEWORD(2, 2), &wsaData); // Winsock 2.2 ���� �ʱ�ȭ

	SOCKET hServerSock = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP); // UDP ���� ����
	/*
	UDP�� �񿬰��� �������ݷ� SOCK_DGRAM ���
	*/

	if (hServerSock == INVALID_SOCKET) // ���� ���� ���� ��
	{
		cout << "Error" << endl;
		exit(0);
	}

	// ���� �ּ� ����
	SOCKADDR_IN serverAddr = { 0, }; // ���� �ּ� ����ü �ʱ�ȭ
	serverAddr.sin_family = AF_INET; // IPv4 ���
	serverAddr.sin_port = htons(PORT); // ��Ʈ ��ȣ ����
	inet_pton(AF_INET, IP, &serverAddr.sin_addr); // IP�ּҸ� ���̳ʸ� �������� ��ȯ

	// ���ε�
	size_t addrSize = sizeof(SOCKADDR_IN); 
	if (bind(hServerSock, (LPSOCKADDR)&serverAddr, addrSize) == SOCKET_ERROR) // ���ε� ���� ��
	{
		cout << "Error";
		exit(0);
	}

	// Ŭ���̾�Ʈ�� ����� ���� ���� ����
	SOCKADDR_IN clientAddr = { 0, }; // Ŭ���̾�Ʈ �ּ� ����ü �ʱ�ȭ
	int recvLength = 0; // ���ŵ� �������� ����
	char message[BUFF_SIZE]; // ���� �� �۽��� ���� ����

	// ���� ���� : Ŭ���̾�Ʈ�κ��� �����͸� �ް� ECHO ����
	while (1)
	{
		int clientAddrSize = sizeof(clientAddr); // Ŭ���̾�Ʈ �ּ� ����ü�� ũ��

		// ������ ����
		recvLength = recvfrom(hServerSock, message, BUFF_SIZE, 0, (LPSOCKADDR)&clientAddr, &clientAddrSize); 
		sendto(hServerSock, message, recvLength, 0, (LPSOCKADDR)&clientAddr, clientAddrSize); // ���ŵ� �����͸� Ŭ���̾�Ʈ���� �ٽ� �۽�
		message[recvLength] = 0;
		cout << message << endl;
	}
	// ���� ����
	closesocket(hServerSock); // ���� �ݱ�
	WSACleanup(); // Winsock ����
	return 0;
}