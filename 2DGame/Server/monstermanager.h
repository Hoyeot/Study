#ifndef __MONSTERMANAGER_H__
#define __MONSTERMANAGER_H__

#define MAX_MONSTER 5

#include <unordered_map>
#include <vector>
#include "monster.h"

using namespace std;

class MonsterManager
{
public:
	MonsterManager();

	bool CreateMonster(int id, float x, float y);
	bool MonsterChk() const;
	void CreateBossMonster(int id, float x, float y);
	void MoveMonster(int id, float x, float y);
	bool DamageMonster(int id, float damage);
	void DeleteMonster(int id);
	Monster* GetMonster(int id);
	unordered_map<int, Monster*>& GetMonsters();
	const vector<int>& GetDeadMonsters() const;
	void ResurrectMonsters();

private:
	unordered_map<int, Monster*> monsters; // 몬스터 저장
	vector<int> deadMonsters; // 몬스터 저장
	const int maxMonsters; // 최대 수
};

#endif
