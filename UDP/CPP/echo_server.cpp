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
	WSAStartup(MAKEWORD(2, 2), &wsaData); // Winsock 2.2 버전 초기화

	SOCKET hServerSock = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP); // UDP 소켓 생성
	/*
	UDP는 비연결형 프로토콜로 SOCK_DGRAM 사용
	*/

	if (hServerSock == INVALID_SOCKET) // 소켓 생성 실패 시
	{
		cout << "Error" << endl;
		exit(0);
	}

	// 서버 주소 설정
	SOCKADDR_IN serverAddr = { 0, }; // 서버 주소 구조체 초기화
	serverAddr.sin_family = AF_INET; // IPv4 사용
	serverAddr.sin_port = htons(PORT); // 포트 번호 설정
	inet_pton(AF_INET, IP, &serverAddr.sin_addr); // IP주소를 바이너리 형식으로 변환

	// 바인딩
	size_t addrSize = sizeof(SOCKADDR_IN); 
	if (bind(hServerSock, (LPSOCKADDR)&serverAddr, addrSize) == SOCKET_ERROR) // 바인딩 실패 시
	{
		cout << "Error";
		exit(0);
	}

	// 클라이언트와 통신을 위한 변수 설정
	SOCKADDR_IN clientAddr = { 0, }; // 클라이언트 주소 구조체 초기화
	int recvLength = 0; // 수신된 데이터의 길이
	char message[BUFF_SIZE]; // 수신 및 송신을 위한 버퍼

	// 메인 루프 : 클라이언트로부터 데이터를 받고 ECHO 응답
	while (1)
	{
		int clientAddrSize = sizeof(clientAddr); // 클라이언트 주소 구조체의 크기

		// 데이터 수신
		recvLength = recvfrom(hServerSock, message, BUFF_SIZE, 0, (LPSOCKADDR)&clientAddr, &clientAddrSize); 
		sendto(hServerSock, message, recvLength, 0, (LPSOCKADDR)&clientAddr, clientAddrSize); // 수신된 데이터를 클라이언트에게 다시 송신
		message[recvLength] = 0;
		cout << message << endl;
	}
	// 소켓 종료
	closesocket(hServerSock); // 소켓 닫기
	WSACleanup(); // Winsock 종료
	return 0;
}