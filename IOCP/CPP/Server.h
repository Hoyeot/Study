#pragma once
#include <WinSock2.h>

#define MAX_BUFFER	1024
#define SERVER_PORT	8000

struct SOCKETINFOSERVER
{
	WSAOVERLAPPED	overlapped;
	WSABUF			dataBuf;
	SOCKET			socket;
	char			messageBuffer[MAX_BUFFER];
	int				recvBytes;
	int				sendBytes;
};

class Server
{
public:
	Server();
	~Server();

	bool Initialize(); // 소켓생성, 서버 정보 세팅
	void StartServer(); // 서버 시작
	bool CreateWorkerThread(); // 작업 스레드 생성
	void WorkerThread(); // 작업 스레드 동작

private:
	SOCKETINFOSERVER*	m_psocketinfo; // 소켓 정보
	SOCKET		m_listenSocket; // listen 소켓
	HANDLE		m_hIOCP;	// IOCP 객체
	bool		m_bAccept; // 요청 동작 플래그
	bool		m_bWorkerThread; // 작업 스레드 동작 플래그
	HANDLE* m_pWorkerHandle; // 작업 스래드 핸들
};