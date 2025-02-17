#include "Server.h"
#include <process.h>
#include <stdio.h>

unsigned int WINAPI CallWorkerThread(LPVOID p)
{
	IOCP* pOverlappedEvent = (IOCP*)p;
	pOverlappedEvent->WorkerThread();
	return 0;
}

Server::Server() // 생성자
{
	m_bWorkerThread = true;
	m_bAccept = true;
}

Server::~Server() // 소멸자
{
	// winsocket 사용 종료
	WSACleanup();
	// 객체 소멸
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

bool Server::Initialize() // 소켓 생성, 서버 정보 세팅 -> 초기화
{
	WSADATA wsaData;
	int nResult;
	/*
	WSAStartup(wVersionRequired, lpWSAData)
	 - [in] WORD wVersionRequired : 사용할 수 있는 가장 높은 버전의 Windows 소켓 사양
	 - [out] LPWSADATA lpWSAData : 소켓 구현의 세부 정보를 수신하는 WSADATA 구조의 포인터
	 - 성공하면 0을 return한다.
	 - WSAStartup을 성공적으로 호출한 후에만 추가 Windows 소켓 함수를 실행할 수 있다.
	*/
	nResult = WSAStartup(MAKEWORD(2, 2), &wsaData);

	if (nResult != 0)
	{
		printf("ERROR : Initialize Fail\n"); // 초기화 실패
		return false;
	}
	
	/*
	WSASocketA (int af, int type, int protocol, LPWSAPROTOCOL_INFOA lpProtocolInfo, GROUP g, DWORD dwFlags)
	 - [in] af : 주소 패밀리 사양
	 - [in] type : 새 소켓의 형식 사양
	 - [in] lpProtocolInfo : 사용 할 프로토콜
	 - [in] g : 그룹작업, NULL이면 수행 X
	 - [in] dwFlags : 추가 소켓특성을 설정하는 플래그 집합
	*/
	m_listenSocket = WSASocket(AF_INET, SOCK_STREAM, 0, NULL, 0, WSA_FLAG_OVERLAPPED); // listening 소켓 생성
	// AF_INET : Ipv4
	// SOCK_STREAM : 양방향스트림
	//WSA_FLAG_OVERLAPPED : Overlapped I/O 소켓 생성

	//m_listenSocket = socket(AF_INET, SOCK_STREAM, 0); -> socket() 함수보다는 WSASocket() 함수가 윈도우에 더 최적화 돼어있음.
	if (m_listenSocket == INVALID_SOCKET)
	{
		printf("ERROR : Socoket Create Fail\n"); // 소켓 생성 실패
		return false;
	}

	// 서버 정보 세팅
	SOCKADDR_IN serverAddr;
	serverAddr.sin_family = PF_INET;
	serverAddr.sin_port = htons(SERVER_PORT);
	serverAddr.sin_addr.S_un.S_addr = htonl(INADDR_ANY);

	// 소켓 설정
	/*
	bind(SOCKET s, const sockaddr* name, int namelen)
	 - [in] s : 연결되지 않은 소켓을 식별하는 설명자
	 - [in] name : 바인딩된 소켓에 할당할 로컬 주소 구조체에 대한 포인터
	 - [in] namelen : 매개변수 값의 길이
	*/
	nResult = bind(m_listenSocket, (struct sockaddr*)&serverAddr, sizeof(SOCKADDR_IN)); // ip주소와 port번호와 같은 정보를 소켓에 bind
	if (nResult == SOCKET_ERROR)
	{
		printf("ERROR : Bind Fail\n"); // 소켓 bind 실패
		closesocket(m_listenSocket); // 소켓 닫아 줌
		WSACleanup(); // winsocket 사용 종료
		return false;
	}

	// 수신 대기열 생성
	/*
	listen(SOCKET s, int backlog)
	 - [in] s : 연결되지 않은 소켓을 식별하는 설명자
	 - [in] blacklog : 연결 큐의 최대 길이
	*/
	nResult = listen(m_listenSocket, 5);
	if (nResult == SOCKET_ERROR)
	{
		printf("ERROR : Listen Fail\n"); // listen 실패
		closesocket(m_listenSocket); // 소켓 닫아 줌
		WSACleanup(); // winsocket 사용 종료
		return false;
	}
	return true;
}

void Server::StartServer()
{
	int nResult;
	// 클라이언트 정보
	SOCKADDR_IN clientAddr; // 클라이언트 주소
	int addrLen = sizeof(SOCKADDR_IN);
	SOCKET clientSocket; // 클라이언트 소켓
	DWORD recvBytes;
	DWORD flags;

	// 완료포트(Completion Port) 객체 생성
	/*
	CreateIoCompletionPort(HANDLE FileHandle, HANDLE ExistingCompletionPort, ULONG_PTR CompletionKey, DWORD NumberOfConcurrentThreads)
	 - [in] FileHandle : INVALID_HANDLE_VALUE로 지정하면 함수가 파일 핸들과 연결하지 않고 IOCP를 만듦
	 - [in] ExistingCompletionPort  : FileHandle가 INVALID_HANDLE_VALUE라면 NULL
	 - [in] CompletionKey : 사용자 정의 완료키
	 - [in] NumberOfConcurrentThreads : IOCP 패킷을 동시에 처리하도록 허용할 수 있는 최대 스레드 수, 0인 경우 시스템 프로세스 수만큼 동시 실행
	*/
	m_hIOCP = CreateIoCompletionPort(INVALID_HANDLE_VALUE, NULL, 0, 0);

	// Worker Thread 생성
	if (!CreateWorkerThread()) return;
	
	printf("서버 시작\n");

	// 클라이언트 접속 받기
	while (m_bAccept)
	{
		/*
		WSAAccept(SOCKET s, sockaddr* addr, LPINT addrlen, LPCONDITIONPROC lpfnCondition, DWORD_PTR dwCCallbackData)
		 - [in] s : listen() 함수를 호출한 후 연결 수신 대기하는 소켓 식별
		 - [out] addr : 연결된 주소를 수신하는 선택적 포인터
		 - [in, out] addrlen : 구조체 길이 선택적 포인터
		 - [in] lpfnCondition : 수락/거부 결정, NULL이면 호출X
		 - [in] dwCallbackData : 조건함수
		*/
		clientSocket = WSAAccept(m_listenSocket, (struct sockaddr*)&clientAddr, &addrLen, NULL, NULL); // 클라이언트가 요청 보낼때까지 기다림

		if (clientSocket == INVALID_SOCKET)
		{
			printf("ERROR : Client Accept Fail\n"); // 접속 실패
			return;
		}

		m_psocketinfo = new SOCKETINFOSERVER();
		m_psocketinfo->socket = clientSocket;
		m_psocketinfo->recvBytes = 0;
		m_psocketinfo->sendBytes = 0;
		m_psocketinfo->dataBuf.len = MAX_BUFFER;
		m_psocketinfo->dataBuf.buf = m_psocketinfo->messageBuffer;
		flags = 0;

		m_hIOCP = CreateIoCompletionPort((HANDLE)clientSocket, m_hIOCP, (DWORD)m_psocketinfo, 0); // 클라이언트 요청 받기

		// 클라이언트 소켓이 보낸 연결요청 메시지를 받음
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
			printf("ERROR : IO Pending Fail (%d)\n", WSAGetLastError()); // Pending 실패
			return;
		}
	}
}

bool Server::CreateWorkerThread()
{
	unsigned int threadId;

	// 시스템 정보 가져오기
	SYSTEM_INFO sysInfo;
	GetSystemInfo(&sysInfo);
	printf("CPU 갯수 : %d\n", sysInfo.dwNumberOfProcessors);

	// 적절한 작업 스레드의 갯수는 (CPU * 2) + 1
	int nThreadCnt = sysInfo.dwNumberOfProcessors * 2;

	// 스레드 handler
	m_pWorkerHandle = new HANDLE[nThreadCnt];

	// 스레드 생성
	for (int i = 0; i < nThreadCnt; i++)
	{
		m_pWorkerHandle[i] = (HANDLE*)_beginthreadex(NULL, 0, &CallWorkerThread, this, CREATE_SUSPENDED, &threadId);
		
		if (m_pWorkerHandle[i] == NULL)
		{
			printf("ERROR : 작업 스레드 생성 실패\n");
			return false;
		}
		ResumeThread(m_pWorkerHandle[i]);
	}
	printf("작업 스레드 시작\n");
	return true;
}

void Server::WorkerThread()
{
	// 함수 호출 성공 여부
	BOOL bResult;
	int nResult;
	// Overlapped IO 작업에서 전송된 데이터 크기
	DWORD recvBytes;
	DWORD sendBytes;
	// Completion Key를 받을 포인터
	SOCKETINFOSERVER* pCompletionKey;
	// IO작업을 위해 요청한 Overlapped 구조체를 받을 포인터
	SOCKETINFOSERVER* pSocketInfo;
	DWORD dwFlags = 0;

	while (m_bWorkerThread)
	{
		/*
		 이 함수로 인해 쓰레드들은 WaitingThre.ad Queue 에 대기상태로 들어가게 됨
		 완료된 Overlapped I/O 작업이 발생하면 IOCP Queue 에서 완료된 작업을 가져와 뒷처리를 함
		*/
		bResult = GetQueuedCompletionStatus(m_hIOCP,
			&recvBytes, // 실제로 전송된 바이트
			(LPDWORD)&pCompletionKey, // completion key
			(LPOVERLAPPED*)&pSocketInfo, // overlapped I/O 객체
			INFINITE // 대기할 시간
		);

		if (bResult && recvBytes == 0)
		{
			printf("소켓(%d) 접속 끊김\n", pSocketInfo->socket);
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
			//printf("메시지 수신 - 소켓 : [%d], Msg : %s\n", pSocketInfo->socket, pSocketInfo->dataBuf.buf);
			printf("수신 : %s\n", pSocketInfo->dataBuf.buf);

			nResult = WSASend(pSocketInfo->socket, &(pSocketInfo->dataBuf), 1, &sendBytes, dwFlags, NULL, NULL);

			if (nResult == SOCKET_ERROR && WSAGetLastError() != WSA_IO_PENDING)
			{
				printf("ERROR : WSASend 실패 (%d)", WSAGetLastError());
			}
			
			//printf("메시지 송신 - 소켓 : [%d], Msg : %s\n", pSocketInfo->socket, pSocketInfo->dataBuf.buf);
			printf("송신 : %s\n", pSocketInfo->dataBuf.buf);

			// SOCKETINFO 데이터 초기화
			ZeroMemory(&(pSocketInfo->overlapped), sizeof(OVERLAPPED));
			pSocketInfo->dataBuf.len = MAX_BUFFER;
			pSocketInfo->dataBuf.buf = pSocketInfo->messageBuffer;
			ZeroMemory(pSocketInfo->messageBuffer, MAX_BUFFER);
			pSocketInfo->recvBytes = 0;
			pSocketInfo->sendBytes = 0;

			dwFlags = 0;

			// 클라이언트로부터 다시 응답을 받기 위해 WSARecv를 호출
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
				printf("ERROR : WSARecv 실패 (%d)", WSAGetLastError());
			}
		}
	}
}