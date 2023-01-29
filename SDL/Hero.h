#pragma once
#include "Creature.h"

class Hero : public  Creature
{
	int m_lives;
	int m_score;

public:
	Hero(std::string name, int xpos, int ypos, int lives, Arena* arena, SDL_Renderer* renderer, std::string filename);
	void display();
	void left();
	void right();
	bool getJump();
	void positionCheck();
	void addScore(int amount);
	void loseLife();
};

