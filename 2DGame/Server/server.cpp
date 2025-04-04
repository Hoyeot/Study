#include "server.h"
#include <process.h>
#include <iostream>r
#include <fstream>

using namespace std;
ofstream logFile("Server.log");

mutex socketMutex;

// 서버 작업 스레드에서 실행
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

	// Winsock 초기화
	if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0) return;

	// Listenning socket 설정
	listenSocket = WSASocket(AF_INET, SOCK_STREAM, 0, nullptr, 0, WSA_FLAG_OVERLAPPED);
	if (listenSocket == INVALID_SOCKET) return;

	// 서버 설정
	SOCKADDR_IN serverAddr;
	serverAddr.sin_family = PF_INET;
	serverAddr.sin_port = htons(PORT);
	serverAddr.sin_addr.S_un.S_addr = htonl(INADDR_ANY);

	// 소켓 바인딩
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

	// IOCP 핸들 생성
	iocpHandle = CreateIoCompletionPort(INVALID_HANDLE_VALUE, nullptr, 0, 0);

	// 작업자 스레드 생성
	if (!CreateWorkterThread()) return;

	// 클라이언트 연결 대기
	while (accept)
	{
		clientSocket = WSAAccept(listenSocket, (SOCKADDR*)&clientAddr, &addrLength, nullptr, 0);

		if (clientSocket == INVALID_SOCKET)	return;

		// 소켓 정보 설정
		socketInfo = new SocketInfo();
		socketInfo->dataBuffer.len = MAX_BUFF_SIZE;
		socketInfo->dataBuffer.buf = socketInfo->messageBuffer;
		socketInfo->socket = clientSocket;
		socketInfo->recvBytes = 0;
		socketInfo->sendBytes = 0;
		flags = 0;

		// 로그 추가
		if (CreateIoCompletionPort((HANDLE)clientSocket, iocpHandle, (DWORD)socketInfo, 0) == NULL) {
			cout << "Failed to associate client socket with IOCP: " << GetLastError() << endl;
			closesocket(clientSocket);
			delete socketInfo;
			continue;
		}

		// 데이터 수신 대기
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

	// 스레드 = 시스템 프로세서 * 2개
	int threadCount = systemInfo.dwNumberOfProcessors * 2;

	workerHandle = new HANDLE[threadCount];

	// 스레드 생성 및 시작
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
		// IOCP에서 완료된 작업 가져오기
		if (!GetQueuedCompletionStatus(iocpHandle, &recvBytes, (PULONG_PTR)&completionKey, (LPOVERLAPPED*)&socketInfo, INFINITE) && recvBytes == 0)
		{
			{
				lock_guard<mutex> lock(socketMutex);
				// 소켓종료처리 넣어봄
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

		// 수신된 데이터가 없으면 소켓 종료
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
		case 0: // 접속
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

		case 1: // 접속 해제
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

		case 2: // 플레이어 이동
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

		case 4: // 점프
		{
			ptr = strtok_s(nullptr, "|", &context);
			char* jumpId(ptr);

			sprintf_s(buffer, MAX_BUFF_SIZE, "4|%s", jumpId);
			BroadcastPlayerMessage(buffer);
			break;
		}

		case 5: // 채팅
		{
			ptr = strtok_s(nullptr, "|", &context);
			char* chatPlayerId(ptr);

			char* chatMessage = strtok_s(nullptr, "|", &context);
			RemoveNewLine(chatMessage);
			sprintf_s(buffer, MAX_BUFF_SIZE, "5|%s|%s", chatPlayerId, chatMessage);
			BroadcastPlayerMessage(buffer);
			break;
		}

		case 6: // 공격모션
		{
			ptr = strtok_s(nullptr, "|", &context);
			char* atkPlayerId(ptr);

			sprintf_s(buffer, MAX_BUFF_SIZE, "6|%s", atkPlayerId);
			BroadcastPlayerMessage(buffer);
			break;
		}

		case 7: // 재도전
		{
			int id = 1;

			for (int i = 0; i < MAX_MONSTER; i++)
			{
				CreateRandomMonster(id);
				id++;
			}

			HandleNewClient(socketInfo);
		}

		case 12: // 몬스터 이동
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

		case 13: // 몬스터 위치 정보 저장
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

		case 17: // 몬스터 피해
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

		case 20: // 몬스터 소환
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

		// OVERLAPPED 구조체 초기화
		ZeroMemory(&(socketInfo->overlapped), sizeof(OVERLAPPED));

		socketInfo->dataBuffer.len = MAX_BUFF_SIZE;
		socketInfo->dataBuffer.buf = socketInfo->messageBuffer;

		ZeroMemory(socketInfo->messageBuffer, MAX_BUFF_SIZE);
		memcpy(socketInfo->messageBuffer, tempBuffer, MAX_BUFF_SIZE);

		socketInfo->recvBytes = 0;
		socketInfo->sendBytes = 0;
		flags = 0;

		// 데이터 수신 대기
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
			delete socketInfo; // 동적 할당된 메모리 해제
		}
	}
}

void Server::HandleNewClient(SocketInfo* socketInfo)
{
	char buffer[MAX_BUFF_SIZE];
	char tempBuffer[80];

	// 기존 플레이어 정보를 담을 메시지 생성
	sprintf_s(buffer, MAX_BUFF_SIZE, "3"); // 메시지 타입: 3 (기존 플레이어 정보)
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

	// 새로운 클라이언트에게 기존 플레이어 정보 전송
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
				state = "IDLE"; // 기본값 설정
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
				state = "IDLE"; // 기본값 설정
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
			delete socketInfo; // 동적 할당된 메모리 해제
		}
	}
}