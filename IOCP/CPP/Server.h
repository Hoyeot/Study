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

	bool Initialize(); // ���ϻ���, ���� ���� ����
	void StartServer(); // ���� ����
	bool CreateWorkerThread(); // �۾� ������ ����
	void WorkerThread(); // �۾� ������ ����

private:
	SOCKETINFOSERVER*	m_psocketinfo; // ���� ����
	SOCKET		m_listenSocket; // listen ����
	HANDLE		m_hIOCP;	// IOCP ��ü
	bool		m_bAccept; // ��û ���� �÷���
	bool		m_bWorkerThread; // �۾� ������ ���� �÷���
	HANDLE* m_pWorkerHandle; // �۾� ������ �ڵ�
};