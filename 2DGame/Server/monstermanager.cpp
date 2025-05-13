#include "monstermanager.h"

MonsterManager::MonsterManager() : maxMonsters(MAX_MONSTER)
{

}

bool MonsterManager::CreateMonster(int id, float x, float y)
{
	if (monsters.size() >= maxMonsters)
	{
		return false;
	}

	Monster* monster = new Monster(id, 50, "IDLE", x, y);
	monsters[id] = monster;
	return true;
}

bool MonsterManager::MonsterChk() const
{
	return monsters.empty() && !deadMonsters.empty();
}

void MonsterManager::CreateBossMonster(int id, float x, float y)
{
	Monster* boss = new Monster(id, 250, "IDLE", x, y);
	monsters[id] = boss;
}

void MonsterManager::MoveMonster(int id, float x, float y)
{
	if (monsters.find(id) != monsters.end())
	{
		monsters[id]->SetPosition(x, y);
	}
}

bool MonsterManager::DamageMonster(int id, float damage)
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

void MonsterManager::DeleteMonster(int id)
{
	if (monsters.find(id) != monsters.end())
	{
		delete monsters[id];
		monsters.erase(id);
		deadMonsters.push_back(id);
	}
}

Monster* MonsterManager::GetMonster(int id)
{
	if (monsters.find(id) != monsters.end())
	{
		return monsters[id];
	}
	return nullptr;
}

unordered_map<int, Monster*>& MonsterManager::GetMonsters()
{
	return monsters;
}

const vector<int>& MonsterManager::GetDeadMonsters() const
{
	return deadMonsters;
}

void MonsterManager::ResurrectMonsters()
{
	for (int id : deadMonsters)
	{
		float x = static_cast<float>(rand() % 10);
		CreateMonster(id, x, 0.5f);
	}
	deadMonsters.clear();
}
