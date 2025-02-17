#include "Server.h"
#include <process.h>
#include <stdio.h>

unsigned int WINAPI CallWorkerThread(LPVOID p)
{
	IOCP* pOverlappedEvent = (IOCP*)p;
	pOverlappedEvent->WorkerThread();
	return 0;
}

Server::Server() // ������
{
	m_bWorkerThread = true;
	m_bAccept = true;
}

Server::~Server() // �Ҹ���
{
	// winsocket ��� ����
	WSACleanup();
	// ��ü �Ҹ�
	if (m_psocketinfo)
	{
		delete[] m_psocketinfo;
		m_psocketinfo = NULL;
	}

	if (m_pWorkerHandle)
	{
		delete[] m_pWorkerHandle;
		m_pWorkerHandle = NULL;
	}
}

bool Server::Initialize() // ���� ����, ���� ���� ���� -> �ʱ�ȭ
{
	WSADATA wsaData;
	int nResult;
	/*
	WSAStartup(wVersionRequired, lpWSAData)
	 - [in] WORD wVersionRequired : ����� �� �ִ� ���� ���� ������ Windows ���� ���
	 - [out] LPWSADATA lpWSAData : ���� ������ ���� ������ �����ϴ� WSADATA ������ ������
	 - �����ϸ� 0�� return�Ѵ�.
	 - WSAStartup�� ���������� ȣ���� �Ŀ��� �߰� Windows ���� �Լ��� ������ �� �ִ�.
	*/
	nResult = WSAStartup(MAKEWORD(2, 2), &wsaData);

	if (nResult != 0)
	{
		printf("ERROR : Initialize Fail\n"); // �ʱ�ȭ ����
		return false;
	}
	
	/*
	WSASocketA (int af, int type, int protocol, LPWSAPROTOCOL_INFOA lpProtocolInfo, GROUP g, DWORD dwFlags)
	 - [in] af : �ּ� �йи� ���
	 - [in] type : �� ������ ���� ���
	 - [in] lpProtocolInfo : ��� �� ��������
	 - [in] g : �׷��۾�, NULL�̸� ���� X
	 - [in] dwFlags : �߰� ����Ư���� �����ϴ� �÷��� ����
	*/
	m_listenSocket = WSASocket(AF_INET, SOCK_STREAM, 0, NULL, 0, WSA_FLAG_OVERLAPPED); // listening ���� ����
	// AF_INET : Ipv4
	// SOCK_STREAM : ����⽺Ʈ��
	//WSA_FLAG_OVERLAPPED : Overlapped I/O ���� ����

	//m_listenSocket = socket(AF_INET, SOCK_STREAM, 0); -> socket() �Լ����ٴ� WSASocket() �Լ��� �����쿡 �� ����ȭ �ž�����.
	if (m_listenSocket == INVALID_SOCKET)
	{
		printf("ERROR : Socoket Create Fail\n"); // ���� ���� ����
		return false;
	}

	// ���� ���� ����
	SOCKADDR_IN serverAddr;
	serverAddr.sin_family = PF_INET;
	serverAddr.sin_port = htons(SERVER_PORT);
	serverAddr.sin_addr.S_un.S_addr = htonl(INADDR_ANY);

	// ���� ����
	/*
	bind(SOCKET s, const sockaddr* name, int namelen)
	 - [in] s : ������� ���� ������ �ĺ��ϴ� ������
	 - [in] name : ���ε��� ���Ͽ� �Ҵ��� ���� �ּ� ����ü�� ���� ������
	 - [in] namelen : �Ű����� ���� ����
	*/
	nResult = bind(m_listenSocket, (struct sockaddr*)&serverAddr, sizeof(SOCKADDR_IN)); // ip�ּҿ� port��ȣ�� ���� ������ ���Ͽ� bind
	if (nResult == SOCKET_ERROR)
	{
		printf("ERROR : Bind Fail\n"); // ���� bind ����
		closesocket(m_listenSocket); // ���� �ݾ� ��
		WSACleanup(); // winsocket ��� ����
		return false;
	}

	// ���� ��⿭ ����
	/*
	listen(SOCKET s, int backlog)
	 - [in] s : ������� ���� ������ �ĺ��ϴ� ������
	 - [in] blacklog : ���� ť�� �ִ� ����
	*/
	nResult = listen(m_listenSocket, 5);
	if (nResult == SOCKET_ERROR)
	{
		printf("ERROR : Listen Fail\n"); // listen ����
		closesocket(m_listenSocket); // ���� �ݾ� ��
		WSACleanup(); // winsocket ��� ����
		return false;
	}
	return true;
}

void Server::StartServer()
{
	int nResult;
	// Ŭ���̾�Ʈ ����
	SOCKADDR_IN clientAddr; // Ŭ���̾�Ʈ �ּ�
	int addrLen = sizeof(SOCKADDR_IN);
	SOCKET clientSocket; // Ŭ���̾�Ʈ ����
	DWORD recvBytes;
	DWORD flags;

	// �Ϸ���Ʈ(Completion Port) ��ü ����
	/*
	CreateIoCompletionPort(HANDLE FileHandle, HANDLE ExistingCompletionPort, ULONG_PTR CompletionKey, DWORD NumberOfConcurrentThreads)
	 - [in] FileHandle : INVALID_HANDLE_VALUE�� �����ϸ� �Լ��� ���� �ڵ�� �������� �ʰ� IOCP�� ����
	 - [in] ExistingCompletionPort  : FileHandle�� INVALID_HANDLE_VALUE��� NULL
	 - [in] CompletionKey : ����� ���� �Ϸ�Ű
	 - [in] NumberOfConcurrentThreads : IOCP ��Ŷ�� ���ÿ� ó���ϵ��� ����� �� �ִ� �ִ� ������ ��, 0�� ��� �ý��� ���μ��� ����ŭ ���� ����
	*/
	m_hIOCP = CreateIoCompletionPort(INVALID_HANDLE_VALUE, NULL, 0, 0);

	// Worker Thread ����
	if (!CreateWorkerThread()) return;
	
	printf("���� ����\n");

	// Ŭ���̾�Ʈ ���� �ޱ�
	while (m_bAccept)
	{
		/*
		WSAAccept(SOCKET s, sockaddr* addr, LPINT addrlen, LPCONDITIONPROC lpfnCondition, DWORD_PTR dwCCallbackData)
		 - [in] s : listen() �Լ��� ȣ���� �� ���� ���� ����ϴ� ���� �ĺ�
		 - [out] addr : ����� �ּҸ� �����ϴ� ������ ������
		 - [in, out] addrlen : ����ü ���� ������ ������
		 - [in] lpfnCondition : ����/�ź� ����, NULL�̸� ȣ��X
		 - [in] dwCallbackData : �����Լ�
		*/
		clientSocket = WSAAccept(m_listenSocket, (struct sockaddr*)&clientAddr, &addrLen, NULL, NULL); // Ŭ���̾�Ʈ�� ��û ���������� ��ٸ�

		if (clientSocket == INVALID_SOCKET)
		{
			printf("ERROR : Client Accept Fail\n"); // ���� ����
			return;
		}

		m_psocketinfo = new SOCKETINFOSERVER();
		m_psocketinfo->socket = clientSocket;
		m_psocketinfo->recvBytes = 0;
		m_psocketinfo->sendBytes = 0;
		m_psocketinfo->dataBuf.len = MAX_BUFFER;
		m_psocketinfo->dataBuf.buf = m_psocketinfo->messageBuffer;
		flags = 0;

		m_hIOCP = CreateIoCompletionPort((HANDLE)clientSocket, m_hIOCP, (DWORD)m_psocketinfo, 0); // Ŭ���̾�Ʈ ��û �ޱ�

		// Ŭ���̾�Ʈ ������ ���� �����û �޽����� ����
		nResult = WSARecv(m_psocketinfo->socket,
			&m_psocketinfo->dataBuf,
			1,
			&recvBytes,
			&flags,
			&(m_psocketinfo->overlapped),
			NULL
		);

		if (nResult == SOCKET_ERROR && WSAGetLastError() != WSA_IO_PENDING)
		{
			printf("ERROR : IO Pending Fail (%d)\n", WSAGetLastError()); // Pending ����
			return;
		}
	}
}

bool Server::CreateWorkerThread()
{
	unsigned int threadId;

	// �ý��� ���� ��������
	SYSTEM_INFO sysInfo;
	GetSystemInfo(&sysInfo);
	printf("CPU ���� : %d\n", sysInfo.dwNumberOfProcessors);

	// ������ �۾� �������� ������ (CPU * 2) + 1
	int nThreadCnt = sysInfo.dwNumberOfProcessors * 2;

	// ������ handler
	m_pWorkerHandle = new HANDLE[nThreadCnt];

	// ������ ����
	for (int i = 0; i < nThreadCnt; i++)
	{
		m_pWorkerHandle[i] = (HANDLE*)_beginthreadex(NULL, 0, &CallWorkerThread, this, CREATE_SUSPENDED, &threadId);
		
		if (m_pWorkerHandle[i] == NULL)
		{
			printf("ERROR : �۾� ������ ���� ����\n");
			return false;
		}
		ResumeThread(m_pWorkerHandle[i]);
	}
	printf("�۾� ������ ����\n");
	return true;
}

void Server::WorkerThread()
{
	// �Լ� ȣ�� ���� ����
	BOOL bResult;
	int nResult;
	// Overlapped IO �۾����� ���۵� ������ ũ��
	DWORD recvBytes;
	DWORD sendBytes;
	// Completion Key�� ���� ������
	SOCKETINFOSERVER* pCompletionKey;
	// IO�۾��� ���� ��û�� Overlapped ����ü�� ���� ������
	SOCKETINFOSERVER* pSocketInfo;
	DWORD dwFlags = 0;

	while (m_bWorkerThread)
	{
		/*
		 �� �Լ��� ���� ��������� WaitingThre.ad Queue �� �����·� ���� ��
		 �Ϸ�� Overlapped I/O �۾��� �߻��ϸ� IOCP Queue ���� �Ϸ�� �۾��� ������ ��ó���� ��
		*/
		bResult = GetQueuedCompletionStatus(m_hIOCP,
			&recvBytes, // ������ ���۵� ����Ʈ
			(LPDWORD)&pCompletionKey, // completion key
			(LPOVERLAPPED*)&pSocketInfo, // overlapped I/O ��ü
			INFINITE // ����� �ð�
		);

		if (bResult && recvBytes == 0)
		{
			printf("����(%d) ���� ����\n", pSocketInfo->socket);
			closesocket(pSocketInfo->socket);
			free(pSocketInfo);
			continue;
		}

		pSocketInfo->dataBuf.len = recvBytes;

		if (recvBytes == 0) 
		{
			closesocket(pSocketInfo->socket);
			free(pSocketInfo);
			continue;
		}
		else
		{
			//printf("�޽��� ���� - ���� : [%d], Msg : %s\n", pSocketInfo->socket, pSocketInfo->dataBuf.buf);
			printf("���� : %s\n", pSocketInfo->dataBuf.buf);

			nResult = WSASend(pSocketInfo->socket, &(pSocketInfo->dataBuf), 1, &sendBytes, dwFlags, NULL, NULL);

			if (nResult == SOCKET_ERROR && WSAGetLastError() != WSA_IO_PENDING)
			{
				printf("ERROR : WSASend ���� (%d)", WSAGetLastError());
			}
			
			//printf("�޽��� �۽� - ���� : [%d], Msg : %s\n", pSocketInfo->socket, pSocketInfo->dataBuf.buf);
			printf("�۽� : %s\n", pSocketInfo->dataBuf.buf);

			// SOCKETINFO ������ �ʱ�ȭ
			ZeroMemory(&(pSocketInfo->overlapped), sizeof(OVERLAPPED));
			pSocketInfo->dataBuf.len = MAX_BUFFER;
			pSocketInfo->dataBuf.buf = pSocketInfo->messageBuffer;
			ZeroMemory(pSocketInfo->messageBuffer, MAX_BUFFER);
			pSocketInfo->recvBytes = 0;
			pSocketInfo->sendBytes = 0;

			dwFlags = 0;

			// Ŭ���̾�Ʈ�κ��� �ٽ� ������ �ޱ� ���� WSARecv�� ȣ��
			nResult = WSARecv(pSocketInfo->socket,
				&(pSocketInfo->dataBuf),
				1,
				&recvBytes,
				&dwFlags,
				(LPWSAOVERLAPPED)& (pSocketInfo->overlapped),
				NULL
			);
			
			if (nResult == SOCKET_ERROR && WSAGetLastError() != WSA_IO_PENDING)
			{
				printf("ERROR : WSARecv ���� (%d)", WSAGetLastError());
			}
		}
	}
}