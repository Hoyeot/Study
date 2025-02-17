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


	// UDP ���� ����
	SOCKET hServSock = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
	if (hServSock == INVALID_SOCKET) // ���� ���� ���� ��
	{
		cout << "socket error!" << endl;
		exit(0);
	}

	// ���� �ּ� ����
	SOCKADDR_IN servAddr = { 0, }; // ���� �ּ� ����ü �ʱ�ȭ
	servAddr.sin_family = AF_INET; // Ipv4 ���
	servAddr.sin_port = htons(PORT); // ��Ʈ ��ȣ ����
	inet_pton(AF_INET, IP, &servAddr.sin_addr); // IP �ּҸ� ���̳ʸ� �������� ��ȯ

	char message[BUFF_SIZE]; // �ۼ����� ���� ����
	SOCKADDR_IN recvAddr; // �����κ����� ���� �ּҸ� ������ ����ü
	int recvAddrSize = sizeof(recvAddr); // �ּ� ����ü�� ũ��
	int sendLength = 0; // �۽ŵ� �������� ����

	// ���� ����
	while (1)
	{
		cout << "Input Text" << endl; // ����ڿ��� �Է� ��û
		fgets(message, sizeof(message), stdin); // ����� �Է��� ���ۿ� ����

		if (!strcmp(message, "exit\n") || !strcmp(message, "EXIT\n")) // exit or EXIT �Է½� ����
			break;

		sendLength = sendto(hServSock, message, strlen(message), 0, (LPSOCKADDR)&servAddr, sizeof(servAddr)); // �����κ��� ������ �۽�
		int recvLength = recvfrom(hServSock, message, BUFF_SIZE, 0, (LPSOCKADDR)&recvAddr, &recvAddrSize); // �����κ��� ������ ����
		message[recvLength] = 0;
		cout << "Receive : " << message << endl;
	}
	closesocket(hServSock); // ���� �ݱ�
	WSACleanup(); // Winsock ����
	return 0;
}