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


	// UDP 소켓 생성
	SOCKET hServSock = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
	if (hServSock == INVALID_SOCKET) // 소켓 생성 실패 시
	{
		cout << "socket error!" << endl;
		exit(0);
	}

	// 서버 주소 설정
	SOCKADDR_IN servAddr = { 0, }; // 서버 주소 구조체 초기화
	servAddr.sin_family = AF_INET; // Ipv4 사용
	servAddr.sin_port = htons(PORT); // 포트 번호 설정
	inet_pton(AF_INET, IP, &servAddr.sin_addr); // IP 주소를 바이너리 형식으로 변환

	char message[BUFF_SIZE]; // 송수신을 위한 버퍼
	SOCKADDR_IN recvAddr; // 서버로부터의 응답 주소를 저장할 구조체
	int recvAddrSize = sizeof(recvAddr); // 주소 구조체의 크기
	int sendLength = 0; // 송신된 데이터의 길이

	// 메인 루프
	while (1)
	{
		cout << "Input Text" << endl; // 사용자에게 입력 요청
		fgets(message, sizeof(message), stdin); // 사용자 입력을 버퍼에 저장

		if (!strcmp(message, "exit\n") || !strcmp(message, "EXIT\n")) // exit or EXIT 입력시 종료
			break;

		sendLength = sendto(hServSock, message, strlen(message), 0, (LPSOCKADDR)&servAddr, sizeof(servAddr)); // 서버로부터 데이터 송신
		int recvLength = recvfrom(hServSock, message, BUFF_SIZE, 0, (LPSOCKADDR)&recvAddr, &recvAddrSize); // 서버로부터 데이터 수신
		message[recvLength] = 0;
		cout << "Receive : " << message << endl;
	}
	closesocket(hServSock); // 소켓 닫기
	WSACleanup(); // Winsock 종료
	return 0;
}