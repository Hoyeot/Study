#include "server.h"
#include <process.h>
#include <iostream>r
#include <fstream>

using namespace std;
ofstream logFile("Server.log");

mutex socketMutex;

// ���� �۾� �����忡�� ����
unsigned int WINAPI CallWorkerThread(LPVOID p)
{
	Server* overlapeedEvent = (Server*)p;
	overlapeedEvent->WorkerThread();
	return 0;
}
void RemoveNewLine(char* buffer)
{
	buffer[strcspn(buffer, "\n")] = 0;
}

Server::Server() : socketInfo(), listenSocket(), iocpHandle(), workerHandle(), accept(true), runningWorkerThread(true), clientPlayers(), monsterManager(), clientCount()
{
	CONSOLE_CURSOR_INFO consoleCursorInfo;

	consoleCursorInfo.bVisible = 0;
	consoleCursorInfo.dwSize = 1;
	SetConsoleCursorInfo(GetStdHandle(STD_OUTPUT_HANDLE), &consoleCursorInfo);
}

Server::~Server()
{
	WSACleanup();

	if (socketInfo)
	{
		delete[] socketInfo;
		socketInfo = nullptr;
	}

	if (workerHandle)
	{
		delete[] workerHandle;
		workerHandle = nullptr;
	}

	for (int i = 0; i < MAX_CLIENT; i++)
	{
		if (clientPlayers[i] != nullptr)
		{
			delete clientPlayers[i];
			clientPlayers[i] = nullptr;
		}
	}
}

void Server::Initialize(unsigned short port)
{
	WSADATA wsaData;

	// Winsock �ʱ�ȭ
	if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0) return;

	// Listenning socket ����
	listenSocket = WSASocket(AF_INET, SOCK_STREAM, 0, nullptr, 0, WSA_FLAG_OVERLAPPED);
	if (listenSocket == INVALID_SOCKET) return;

	// ���� ����
	SOCKADDR_IN serverAddr;
	serverAddr.sin_family = PF_INET;
	serverAddr.sin_port = htons(PORT);
	serverAddr.sin_addr.S_un.S_addr = htonl(INADDR_ANY);

	// ���� ���ε�
	if (bind(listenSocket, (SOCKADDR*)&serverAddr, sizeof(SOCKADDR_IN)) == SOCKET_ERROR)
	{
		closesocket(listenSocket);
		WSACleanup();
		return;
	}

	// Listenning
	if (listen(listenSocket, 5) == SOCKET_ERROR)
	{
		closesocket(listenSocket);
		WSACleanup();
		return;
	}

	cout << "Server start" << endl;
}

void Server::Start()
{
	SOCKET clientSocket;
	SOCKADDR_IN clientAddr;
	int addrLength = sizeof(SOCKADDR_IN);
	DWORD recvBytes;
	DWORD flags;

	// IOCP �ڵ� ����
	iocpHandle = CreateIoCompletionPort(INVALID_HANDLE_VALUE, nullptr, 0, 0);

	// �۾��� ������ ����
	if (!CreateWorkterThread()) return;

	// Ŭ���̾�Ʈ ���� ���
	while (accept)
	{
		clientSocket = WSAAccept(listenSocket, (SOCKADDR*)&clientAddr, &addrLength, nullptr, 0);

		if (clientSocket == INVALID_SOCKET)	return;

		// ���� ���� ����
		socketInfo = new SocketInfo();
		socketInfo->dataBuffer.len = MAX_BUFF_SIZE;
		socketInfo->dataBuffer.buf = socketInfo->messageBuffer;
		socketInfo->socket = clientSocket;
		socketInfo->recvBytes = 0;
		socketInfo->sendBytes = 0;
		flags = 0;

		// �α� �߰�
		if (CreateIoCompletionPort((HANDLE)clientSocket, iocpHandle, (DWORD)socketInfo, 0) == NULL) {
			cout << "Failed to associate client socket with IOCP: " << GetLastError() << endl;
			closesocket(clientSocket);
			delete socketInfo;
			continue;
		}

		// ������ ���� ���
		if (WSARecv(socketInfo->socket, &socketInfo->dataBuffer, 1, &recvBytes, &flags, &(socketInfo->overlapped), nullptr) == SOCKET_ERROR && WSAGetLastError() != WSA_IO_PENDING)
		{
			cout << "WSARecv failed" << WSAGetLastError() << endl;
			return;
		}
	}
}

bool Server::CreateWorkterThread()
{
	unsigned int threadId;
	SYSTEM_INFO systemInfo;

	GetSystemInfo(&systemInfo);

	// ������ = �ý��� ���μ��� * 2��
	int threadCount = systemInfo.dwNumberOfProcessors * 2;

	workerHandle = new HANDLE[threadCount];

	// ������ ���� �� ����
	for (int i = 0; i < threadCount; i++)
	{
		workerHandle[i] = (HANDLE*)_beginthreadex(nullptr, 0, &CallWorkerThread, this, CREATE_SUSPENDED, &threadId);

		if (workerHandle[i] == nullptr) return false;
		ResumeThread(workerHandle[i]);
	}
	return true;
}

void Server::WorkerThread()
{
	DWORD recvBytes;
	DWORD sendBytes;
	DWORD flags = 0;
	SocketInfo* completionKey;
	SocketInfo* socketInfo;

	while (runningWorkerThread)
	{
		// IOCP���� �Ϸ�� �۾� ��������
		if (!GetQueuedCompletionStatus(iocpHandle, &recvBytes, (PULONG_PTR)&completionKey, (LPOVERLAPPED*)&socketInfo, INFINITE) && recvBytes == 0)
		{
			{
				lock_guard<mutex> lock(socketMutex);
				// ��������ó�� �־
				for (int i = 0; i < MAX_CLIENT; i++)
				{
					if (clientPlayers[i] != nullptr && clientPlayers[i]->GetSocket() == socketInfo->socket)
					{
						char buffer[MAX_BUFF_SIZE];
						sprintf_s(buffer, MAX_BUFF_SIZE, "1|%s", clientPlayers[i]->GetId());
						BroadcastPlayerMessage(buffer);

						delete clientPlayers[i];
						clientPlayers[i] = nullptr;
						clientCount--;
						break;
					}
				}
			}
			closesocket(socketInfo->socket);
			free(socketInfo);
			continue;
		}

		socketInfo->dataBuffer.len = recvBytes;

		// ���ŵ� �����Ͱ� ������ ���� ����
		if (recvBytes == 0)
		{
			closesocket(socketInfo->socket);
			free(socketInfo);
			continue;
		}

		char buffer[MAX_BUFF_SIZE];
		char* context = nullptr;
		char* context2 = nullptr;
		char message[MAX_BUFF_SIZE];
		strcpy(message, socketInfo->dataBuffer.buf);
		char* ptr = strtok_s(socketInfo->dataBuffer.buf, "|", &context);
		if (context != nullptr) RemoveNewLine(context);

		switch (atoi(ptr))
		{
		case 0: // ����
		{
			if (clientCount >= MAX_CLIENT) continue;

			ptr = strtok_s(nullptr, "|", &context);
			char* playerId(ptr);
			if (playerId == nullptr) break;
			RemoveNewLine(playerId);
			ptr = strtok_s(nullptr, "|", &context);
			{
				for (int i = 0; i < MAX_CLIENT; i++)
				{
					if (clientPlayers[i] == nullptr)
					{
						clientPlayers[i] = new Player(socketInfo->socket, playerId, 100, 0, 0);
						clientCount++;
						break;
					}
				}
			}

			cout << "Connect : " << playerId << "| Count : " << clientCount << endl;
			sprintf_s(buffer, MAX_BUFF_SIZE, "0|%s|%f|%.2f,%.2f", playerId, 100.0, (float)0, (float)0);
			BroadcastPlayerMessage(buffer);
			_sleep(100);

			if (isCreateMonster == false)
			{
				int id = 1;

				for (int i = 0; i < MAX_MONSTER; i++)
				{
					CreateRandomMonster(id);
					id++;
				}
			}

			HandleNewClient(socketInfo);
			break;
		}

		case 1: // ���� ����
		{
			ptr = strtok_s(nullptr, "|", &context);
			char* leavePlayerId(ptr);
			if (leavePlayerId == nullptr) break;

			RemoveNewLine(leavePlayerId);

			{
				lock_guard<mutex> lock(socketMutex);
				for (int i = 0; i < MAX_CLIENT; i++)
				{
					if (clientPlayers[i] != nullptr && clientPlayers[i]->GetId() == leavePlayerId)
					{
						sprintf_s(buffer, MAX_BUFF_SIZE, "1|%s", leavePlayerId);
						BroadcastPlayerMessage(buffer);

						delete clientPlayers[i];
						clientPlayers[i] = nullptr;
						clientCount--;

						break;
					}
				}
			}
			cout << "Disconnected : " << leavePlayerId << "| Count : " << clientCount << endl;
			isSendMonster = false;
			_sleep(500);
			break;
		}

		case 2: // �÷��̾� �̵�
		{
			ptr = strtok_s(nullptr, "|", &context);
			char* movedPlayerId(ptr);
			if (movedPlayerId == nullptr) break;

			ptr = strtok_s(nullptr, "|", &context);
			float hp2 = atof(ptr);
			char* data = strtok_s(context, ",", &context2);
			float x = atof(data);

			data = strtok_s(nullptr, ",", &context2);
			float y = atof(data);

			data = strtok_s(nullptr, ",", &context2);
			char* stop = data;

			data = strtok_s(nullptr, ",", &context2);
			float speed = atof(data);

			data = strtok_s(nullptr, ",", &context2);
			char* flip = data;

			float velocityX = 0.0f;
			float velocityY = 0.0f;
			for (int i = 0; i < MAX_CLIENT; i++)
			{
				if (clientPlayers[i] != nullptr && clientPlayers[i]->GetId() == movedPlayerId)
				{
					velocityX = x - clientPlayers[i]->GetX();
					velocityY = y - clientPlayers[i]->GetY();
					clientPlayers[i]->SetPosition(x, y);
					break;
				}
			}
			sprintf_s(buffer, MAX_BUFF_SIZE, "2|%s|%f|%.2f,%.2f|%.2f,%.2f|%s|%f|%s", movedPlayerId, hp2, x, y, velocityX, velocityY, stop, speed, flip);
			BroadcastPlayerMessage(buffer);
			break;
		}

		case 4: // ����
		{
			ptr = strtok_s(nullptr, "|", &context);
			char* jumpId(ptr);

			sprintf_s(buffer, MAX_BUFF_SIZE, "4|%s", jumpId);
			BroadcastPlayerMessage(buffer);
			break;
		}

		case 5: // ä��
		{
			ptr = strtok_s(nullptr, "|", &context);
			char* chatPlayerId(ptr);

			char* chatMessage = strtok_s(nullptr, "|", &context);
			RemoveNewLine(chatMessage);
			sprintf_s(buffer, MAX_BUFF_SIZE, "5|%s|%s", chatPlayerId, chatMessage);
			BroadcastPlayerMessage(buffer);
			break;
		}

		case 6: // ���ݸ��
		{
			ptr = strtok_s(nullptr, "|", &context);
			char* atkPlayerId(ptr);

			sprintf_s(buffer, MAX_BUFF_SIZE, "6|%s", atkPlayerId);
			BroadcastPlayerMessage(buffer);
			break;
		}

		case 7: // �絵��
		{
			int id = 1;

			for (int i = 0; i < MAX_MONSTER; i++)
			{
				CreateRandomMonster(id);
				id++;
			}

			HandleNewClient(socketInfo);
		}

		case 12: // ���� �̵�
		{
			ptr = strtok_s(nullptr, "|", &context);
			int moveMonsterId = atoi(ptr);
			if (moveMonsterId == NULL) break;

			char* data = strtok_s(context, ",", &context2);
			float x = atof(data);

			data = strtok_s(nullptr, ",", &context2);
			float y = atof(data);

			monsterManager.MoveMonster(moveMonsterId, x, y);

			sprintf_s(buffer, MAX_BUFF_SIZE, "12|%d|%.2f,%.2f", moveMonsterId, x, y);
			BroadcastMonsterMessage(buffer);
			break;
		}

		case 13: // ���� ��ġ ���� ����
		{
			char* monsterToken = strtok_s(nullptr, ";", &context);

			while (monsterToken != nullptr)
			{
				char* dataContext = nullptr;
				char* idStr = strtok_s(monsterToken, ",", &dataContext);
				char* xStr = strtok_s(nullptr, ",", &dataContext);
				char* yStr = strtok_s(nullptr, ",", &dataContext);

				if (idStr && xStr && yStr)
				{
					int monsterId = atoi(idStr);
					float x = (float)atof(xStr);
					float y = (float)atof(yStr);

					monsterManager.MoveMonster(monsterId, x, y);
				}

				monsterToken = strtok_s(nullptr, ";", &context);
			}
			break;
		}

		case 17: // ���� ����
		{
			ptr = strtok_s(nullptr, "|", &context);
			int damagedMonsterId = atoi(ptr);
			if (damagedMonsterId == NULL) break;

			ptr = strtok_s(nullptr, "|", &context);
			float damage = atof(ptr);

			bool isDead = monsterManager.DamageMonster(damagedMonsterId, damage);

			sprintf_s(buffer, MAX_BUFF_SIZE, "17|%d|%f", damagedMonsterId, damage);
			BroadcastMonsterMessage(buffer);

			if (isDead)
			{
				cout << damagedMonsterId << " - Monster Deaths" << endl;
				if (damagedMonsterId == 999)
				{
					sprintf_s(buffer, MAX_BUFF_SIZE, "21|%d", damagedMonsterId);
					BroadcastMonsterMessage(buffer);
					break;
				}
				else
				{
					sprintf_s(buffer, MAX_BUFF_SIZE, "11|%d", damagedMonsterId);
				}
				BroadcastMonsterMessage(buffer);

				if (monsterManager.MonsterChk())
				{
					int bossId = 999;
					monsterManager.CreateBossMonster(bossId, 0.0f, 0.0f);
					sprintf_s(buffer, MAX_BUFF_SIZE, "20|%d|250", bossId);
					BroadcastMonsterMessage(buffer);
				}
			}
			break;
		}

		case 20: // ���� ��ȯ
		{
			monsterManager.ResurrectMonsters();

			for (auto& pair : monsterManager.GetMonsters())
			{
				Monster* monster = pair.second;
				sprintf_s(buffer, MAX_BUFF_SIZE, "10|%d,%f,%s,%.2f,%.2f", monster->GetId(), monster->GetHp(), monster->GetState(), monster->GetX(), monster->GetY());
				BroadcastMonsterMessage(buffer);
			}
			break;
		}
		}

		char tempBuffer[MAX_BUFF_SIZE];
		memcpy(tempBuffer, socketInfo->messageBuffer, MAX_BUFF_SIZE);

		// OVERLAPPED ����ü �ʱ�ȭ
		ZeroMemory(&(socketInfo->overlapped), sizeof(OVERLAPPED));

		socketInfo->dataBuffer.len = MAX_BUFF_SIZE;
		socketInfo->dataBuffer.buf = socketInfo->messageBuffer;

		ZeroMemory(socketInfo->messageBuffer, MAX_BUFF_SIZE);
		memcpy(socketInfo->messageBuffer, tempBuffer, MAX_BUFF_SIZE);

		socketInfo->recvBytes = 0;
		socketInfo->sendBytes = 0;
		flags = 0;

		// ������ ���� ���
		if (WSARecv(socketInfo->socket, &(socketInfo->dataBuffer), 1, &recvBytes, &flags, (LPWSAOVERLAPPED) & (socketInfo->overlapped), nullptr) == SOCKET_ERROR && WSAGetLastError() != WSA_IO_PENDING)
		{
			cout << "WSARecv failed" << WSAGetLastError() << endl;
		}
	}
}

void Server::BroadcastPlayerMessage(const char* message)
{
	char buffer[MAX_BUFF_SIZE];
	sprintf_s(buffer, MAX_BUFF_SIZE, "%s\n", message);

	for (int i = 0; i < MAX_CLIENT; i++)
	{
		if (clientPlayers[i] != nullptr)
		{
			SOCKET clientSocket = clientPlayers[i]->GetSocket();
			SocketInfo* socketInfo = new SocketInfo();
			socketInfo->dataBuffer.len = strlen(buffer);
			socketInfo->dataBuffer.buf = buffer;
			socketInfo->socket = clientSocket;

			DWORD sendBytes;
			DWORD flags = 0;

			if (WSASend(socketInfo->socket, &(socketInfo->dataBuffer), 1, &sendBytes, flags, nullptr, nullptr) == SOCKET_ERROR && WSAGetLastError() != WSA_IO_PENDING)
			{
				cout << "WSASend failed" << WSAGetLastError() << endl;
			}
			delete socketInfo; // ���� �Ҵ�� �޸� ����
		}
	}
}

void Server::HandleNewClient(SocketInfo* socketInfo)
{
	char buffer[MAX_BUFF_SIZE];
	char tempBuffer[80];

	// ���� �÷��̾� ������ ���� �޽��� ����
	sprintf_s(buffer, MAX_BUFF_SIZE, "3"); // �޽��� Ÿ��: 3 (���� �÷��̾� ����)
	{
		lock_guard<mutex> lock(socketMutex);
		for (int i = 0; i < MAX_CLIENT; i++)
		{
			if (clientPlayers[i] != nullptr && clientPlayers[i]->GetSocket() != socketInfo->socket)
			{
				if (clientPlayers[i]->GetId() == nullptr) continue;
				sprintf_s(tempBuffer, 80, "|%s|%f|%.2f,%.2f", clientPlayers[i]->GetId(), clientPlayers[i]->GetHp(), clientPlayers[i]->GetX(), clientPlayers[i]->GetY());
				//strcat_s(buffer, MAX_BUFF_SIZE, tempBuffer);
				if (strcat_s(buffer, MAX_BUFF_SIZE, tempBuffer) != 0) break;
			}
		}
	}

	// ���ο� Ŭ���̾�Ʈ���� ���� �÷��̾� ���� ����
	socketInfo->dataBuffer.len = strlen(buffer);
	socketInfo->dataBuffer.buf = buffer;

	DWORD sendBytes;
	DWORD flags = 0;
	if (WSASend(socketInfo->socket, &(socketInfo->dataBuffer), 1, &sendBytes, flags, nullptr, nullptr) == SOCKET_ERROR && WSAGetLastError() != WSA_IO_PENDING)
	{
		cout << "WSASend failed: " << WSAGetLastError() << endl;
	}
	_sleep(150);

	if (!isSendMonster)
	{
		sprintf_s(buffer, MAX_BUFF_SIZE, "10");

		for (auto& pair : monsterManager.GetMonsters())
		{
			Monster* monster = pair.second;
			if (monster == nullptr) continue;
			const char* state = monster->GetState();
			if (state == nullptr) {
				state = "IDLE"; // �⺻�� ����
			}
			sprintf_s(tempBuffer, 80, "|%d,%f,%s,%.2f,%.2f", monster->GetId(), monster->GetHp(), monster->GetState(), monster->GetX(), monster->GetY());
			if (strcat_s(buffer, MAX_BUFF_SIZE, tempBuffer) != 0) break;
		}
		socketInfo->dataBuffer.len = strlen(buffer);
		socketInfo->dataBuffer.buf = buffer;
		if (WSASend(socketInfo->socket, &(socketInfo->dataBuffer), 1, &sendBytes, flags, nullptr, nullptr) == SOCKET_ERROR && WSAGetLastError() != WSA_IO_PENDING)
		{
			std::cout << "WSASend failed: " << WSAGetLastError() << endl;
		}
		isSendMonster = true;
	}
	else
	{
		sprintf_s(buffer, MAX_BUFF_SIZE, "13");

		for (auto& pair : monsterManager.GetMonsters())
		{
			Monster* monster = pair.second;
			if (monster == nullptr) continue;
			const char* state = monster->GetState();
			if (state == nullptr) {
				state = "IDLE"; // �⺻�� ����
			}
			sprintf_s(tempBuffer, 80, "|%d,%f,%s,%.2f,%.2f", monster->GetId(), monster->GetHp(), monster->GetState(), monster->GetX(), monster->GetY());
			if (strcat_s(buffer, MAX_BUFF_SIZE, tempBuffer) != 0) break;
		}

		socketInfo->dataBuffer.len = strlen(buffer);
		socketInfo->dataBuffer.buf = buffer;
		if (WSASend(socketInfo->socket, &(socketInfo->dataBuffer), 1, &sendBytes, flags, nullptr, nullptr) == SOCKET_ERROR && WSAGetLastError() != WSA_IO_PENDING)
		{
			std::cout << "WSASend failed: " << WSAGetLastError() << endl;
		}
	}
}

void Server::CreateRandomMonster(int id)
{
	isCreateMonster = true;
	int monsterId = id;

	monsterManager.CreateMonster(monsterId, 0.0f, 0.0f);
}

void Server::BroadcastMonsterMessage(const char* message)
{
	char buffer[MAX_BUFF_SIZE];
	sprintf_s(buffer, MAX_BUFF_SIZE, "%s\n", message);

	for (int i = 0; i < MAX_CLIENT; i++)
	{
		if (clientPlayers[i] != nullptr)
		{
			SOCKET clientSocket = clientPlayers[i]->GetSocket();
			SocketInfo* socketInfo = new SocketInfo();
			socketInfo->dataBuffer.len = strlen(buffer);
			socketInfo->dataBuffer.buf = buffer;
			socketInfo->socket = clientSocket;

			DWORD sendBytes;
			DWORD flags = 0;

			if (WSASend(socketInfo->socket, &(socketInfo->dataBuffer), 1, &sendBytes, flags, nullptr, nullptr) == SOCKET_ERROR && WSAGetLastError() != WSA_IO_PENDING)
			{
				std::cout << "WSASend failed: " << WSAGetLastError() << std::endl;
			}
			delete socketInfo; // ���� �Ҵ�� �޸� ����
		}
	}
}