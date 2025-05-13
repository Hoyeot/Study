#include <iostream>
#include "player.h"

Player::Player(SOCKET socket, const char* id, float currentHp, float x, float y)
	: socket(socket), id(id), x(x), y(y), currentHp(currentHp)
{
	
}

Player::~Player()
{

}

SOCKET Player::GetSocket() const
{
	return socket;
}

void Player::SetPosition(float x, float y)
{
	this->x = x;
	this->y = y;
}

float Player::GetHp()
{
	return currentHp;
}

float Player::GetX()
{
	return x;
}

float Player::GetY()
{
	return y;
}

const char* Player::GetId()
{
	return id;
}
