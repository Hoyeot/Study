#ifndef __MONSTER_H__
#define __MONSTER_H__

class Monster
{
public:
	Monster(int monsterId, float currentHp, const char* state, float x, float y)
		: monsterId(monsterId), currentHp(currentHp), state(state), x(x), y(y) {}

	const int GetId() const { return monsterId; }
	float GetHp() const { return currentHp; }
	const char* GetState() const { return state; }
	float GetX() const { return x; }
	float GetY() const { return y; }

	void SetPosition(float newX, float newY) { x = newX; y = newY; }
	void SetState(const char* newState) { state = newState; }
	void TakeDamage(float damage) { currentHp -= damage; }
	bool IsDead() const { return currentHp <= 0; }

private:
	int monsterId;
	float currentHp;
	const char* state;
	float x, y;
};
#endif