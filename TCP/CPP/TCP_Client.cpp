#include <WinSock2.h> // Windows 소켓 헤더 파일
#include <WS2tcpip.h> // IP 주소 변환 함수 헤더 파일
#include <iostream>

#define IP "127.0.0.1" // IP
#define PORT 8000 // PORT
#define BUFF_SIZE 50 // 버퍼 크기

using namespace std;

int main()
{
	WSADATA wsaData;
	WSAStartup(MAKEWORD(2, 2), &wsaData);

	// TCP 소켓 생성
	SOCKET hClientSock = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
	if (hClientSock == INVALID_SOCKET)
	{
		cout << "Socket Error" << WSAGetLastError() << endl;
		WSACleanup();
		exit(0);
	}

	// 서버 주소 설정
	SOCKADDR_IN servAddr = { 0, };
	servAddr.sin_family = AF_INET;
	servAddr.sin_port = htons(PORT);
	inet_pton(AF_INET, IP, &servAddr.sin_addr);

	// 서버와 연결
	if (connect(hClientSock, (LPSOCKADDR)&servAddr, sizeof(servAddr)) == SOCKET_ERROR)
	{
		cout << "Connect Error : " << WSAGetLastError() << endl;
		closesocket(hClientSock);
		WSACleanup();
		exit(0);
	}

	cout << "Server Connected!" << endl;

	char message[BUFF_SIZE]; // 송수신을 위한 버퍼
	int sendLength = 0; // 송신된 데이터의 길이

	// 메인 루프
	while (1)
	{
		cout << "Input Text" << endl;
		fgets(message, sizeof(message), stdin); // 입력을 버퍼에 저장

		if (!strcmp(message, "exit\n") || !strcmp(message, "EXIT\n")) break;

		sendLength = send(hClientSock, message, strlen(message), 0); // 서버로 데이터 송신
		int recvLength = recv(hClientSock, message, BUFF_SIZE, 0); // 서버로부터 데이터 수신
		message[recvLength] = 0;
		cout << "Receive : " << message << endl;
	}

	// 종료
	closesocket(hClientSock);
	WSACleanup();
	return 0;
}