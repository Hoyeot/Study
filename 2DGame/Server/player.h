#ifndef __PLAYER_H__
#define __PLAYER_H__

#include <WinSock2.h>

using namespace std;

class Player
{
public:
    Player(SOCKET socket, const char* id, float currentHp, float x, float y);
    ~Player();

    SOCKET GetSocket() const;
    void SetPosition(float x, float y);
    float GetHp();
    float GetX();
    float GetY();
    const char* GetId();

private:
    SOCKET socket; // 소켓 정보 저장
    const char* id;
    float currentHp;
    float x, y;
};

#endif
