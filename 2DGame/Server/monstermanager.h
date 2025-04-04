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
	MonsterManager() : maxMonsters(MAX_MONSTER) {}

	bool CreateMonster(int id, float x, float y)
	{
		if (monsters.size() >= maxMonsters)
		{
			return false;
		}

		Monster* monster = new Monster(id, 50, "IDLE", x, y);
		monsters[id] = monster;
		return true;
	}

	bool MonsterChk() const
	{
		return monsters.empty() && !deadMonsters.empty();
	}

	void CreateBossMonster(int id, float x, float y)
	{
		Monster* boss = new Monster(id, 250, "IDLE", x, y);
		monsters[id] = boss;
	}

	void MoveMonster(int id, float x, float y)
	{
		if (monsters.find(id) != monsters.end())
		{
			monsters[id]->SetPosition(x, y);
		}
	}

	bool DamageMonster(int id, float damage)
	{
		if (monsters.find(id) != monsters.end())
		{
			monsters[id]->TakeDamage(damage);
			if (monsters[id]->IsDead())
			{
				DeleteMonster(id);
				return true;
			}
		}
		return false;
	}

	void DeleteMonster(int id)
	{
		if (monsters.find(id) != monsters.end())
		{
			delete monsters[id];
			monsters.erase(id);
			deadMonsters.push_back(id);
		}
	}

	Monster* GetMonster(int id)
	{
		if (monsters.find(id) != monsters.end())
		{
			return monsters[id];
		}
		return nullptr;
	}

	unordered_map<int, Monster*>& GetMonsters()
	{
		return monsters;
	}

	const vector<int>& GetDeadMonsters() const
	{
		return deadMonsters;
	}

	void ResurrectMonsters()
	{
		for (int id : deadMonsters)
		{
			float x = static_cast<float>(rand() % 10);
			CreateMonster(id, x, 0.5f);
		}
		deadMonsters.clear(); // 죽은 몬스터 목록 초기화
	}

private:
	unordered_map<int, Monster*> monsters; // 몬스터 저장
	vector<int> deadMonsters; // 몬스터 저장
	const int maxMonsters; // 최대 수
};

#endif