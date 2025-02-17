#include <WinSock2.h> // Windows 소켓 헤더 파일
#include <WS2tcpip.h> // IP 주소 변환 함수 헤더 파일
#include <iostream>

#define IP "127.0.0.1" // IP
#define PORT 8000 // PORT
#define BUFF_SIZE 50 // 버퍼 크기

using namespace std;

int main()
{
	WSAData wsaData;
	WSAStartup(MAKEWORD(2, 2), &wsaData);

	SOCKET hServerSock = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP); // TCP 소켓 생성

	if (hServerSock == INVALID_SOCKET)
	{
		cout << "Socket Create Error" << endl;
		exit(0);
	}

	// 서버 주소
	SOCKADDR_IN serverAddr = { 0, };
	serverAddr.sin_family = AF_INET;
	serverAddr.sin_port = htons(PORT);
	inet_pton(AF_INET, IP, &serverAddr.sin_addr);

	// 바인딩
	if (bind(hServerSock, (LPSOCKADDR)&serverAddr, sizeof(serverAddr)) == SOCKET_ERROR)
	{
		cout << "Bind Error" << WSAGetLastError() << endl;
		closesocket(hServerSock);
		WSACleanup();
		exit(0);
	}

	// 리스닝
	if (listen(hServerSock, SOMAXCONN) == SOCKET_ERROR)
	{
		cout << "Listen Error" << WSAGetLastError() << endl;
		closesocket(hServerSock);
		WSACleanup();
		exit(0);
	}

	cout << "TCP SERVER START" << endl;

	// 클라이언트 연결 대기
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

	// 데이터 송수신 루프
	while ((recvLength = recv(hClientSock, message, BUFF_SIZE, 0)) > 0)
	{
		message[recvLength] = '\0'; // 문자열 종료
		cout << "Received : " << message << endl;
		send(hClientSock, message, recvLength, 0); // 에코
	}

	// 소켓 종료
	closesocket(hClientSock);
	closesocket(hServerSock);
	WSACleanup();
	return 0;
}