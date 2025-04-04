#ifndef __PLAYER_H__
#define __PLAYER_H__

#include <WinSock2.h>

using namespace std;

class Player
{
public:
    Player(SOCKET socket, const char* id, float currentHp, float x, float y)
        : socket(socket), id(id), x(x), y(y), currentHp(currentHp) {}
    ~Player();

    SOCKET GetSocket() const { return socket; }

    void SetPosition(float x, float y)
    {
        this->x = x;
        this->y = y;
    }

    float GetHp()
    {
        return currentHp;
    }

    float GetX()
    {
        return x;
    }

    float GetY()
    {
        return y;
    }

    const char* GetId()
    {
        return id;
    }

    //void Clear();

private:
    SOCKET socket; // 소켓 정보 저장
    const char* id;
    float currentHp;
    float x, y;
};

#endif