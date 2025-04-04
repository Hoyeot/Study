#ifndef __SERVER_H__
#define __SERVER_H__

#pragma warning(disable:4996)

#include <WinSock2.h>
#include "player.h"
#include "monstermanager.h"
#include <mutex>

#define MAX_BUFF_SIZE 1024
#define MAX_CLIENT 4
#define PORT 8080

struct SocketInfo
{
	WSAOVERLAPPED overlapped;
	WSABUF dataBuffer;
	SOCKET socket;
	char messageBuffer[MAX_BUFF_SIZE];
	int recvBytes;
	int sendBytes;
};

class Server
{
public:
	Server();
	~Server();

	void Initialize(unsigned short port);
	void Start();
	bool CreateWorkterThread();
	void WorkerThread();
	void BroadcastPlayerMessage(const char* message);
	void HandleNewClient(SocketInfo* socketInfo);
	void CreateRandomMonster(int id);
	void BroadcastMonsterMessage(const char* message);

private:
	SocketInfo* socketInfo;
	SOCKET listenSocket;
	HANDLE iocpHandle;
	HANDLE* workerHandle;
	Player* clientPlayers[MAX_CLIENT];
	MonsterManager monsterManager;

	bool accept;
	bool isCreateMonster;
	bool isSendMonster = false;
	bool runningWorkerThread;

	unsigned short clientCount;
};

#endif