#pragma once
#include "Creature.h"
#include "Hero.h"
#include "Arena.h"

class Monster: public Creature
{
private:
	int playerRange;
	bool patroling;
	bool PatrolUp;
	bool PatrolRight;
	bool Verticle;
	bool onStop;

public:
	Monster(std::string name, int xpos, int ypos, Arena* arena, SDL_Renderer* renderer, std::string filename, bool patrol, bool verticle, int agro);
	void chase(Hero* hero);
	bool eaten(Hero* hero);
	bool collidesWithCreature(Hero* creature);

	void patrolVerticle();
	void patrolHorizontal();
	void move(Hero* hero);
	void monsterDraw(int x, int y);
};

