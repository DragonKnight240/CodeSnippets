#include "Hero.h"
#include <iostream>

Hero::Hero(std::string name, int xpos, int ypos, int lives, Arena* arena, SDL_Renderer* renderer, std::string filename)
	: Creature{ name, xpos, ypos, arena, renderer, filename }, m_lives{ lives }
{
	addFrame("Assets/Hero/2.bmp");
	addFrame("Assets/Hero/3.bmp");
	addFrame("Assets/Hero/4.bmp");
	addFrame("Assets/Hero/5.bmp");
	addFrame("Assets/Hero/6.bmp");
	addFrame("Assets/Hero/7.bmp");
	addFrame("Assets/Hero/8.bmp");
	addFrame("Assets/Hero/9.bmp");
	addFrame("Assets/Hero/10.bmp");
	addFrame("Assets/Hero/11.bmp");
	addFrame("Assets/Hero/12.bmp");
}

void Hero::display()
{
	Creature::display();
	std::cout << "Lives: " << m_lives << std::endl << "Score: " << m_score;
}

void Hero::left()
{
	Creature::left();
	nextFrame();
}

void Hero::right()
{
	Creature::right();
	nextFrame();
}

bool Hero::getJump()
{
	if (m_arena->isWall(m_xpos, m_ypos + m_height + 1) ||
		m_arena->isWall(m_xpos + m_width - 1, m_ypos + m_height + 1))
	{
		return false;
	}
	else
	{
		return true;
	}
}

void Hero::positionCheck()
{
	if ((m_xpos) > m_arena->getScreenWidth())
	{
		if (m_arena->getViewportX() != 3)
		{
			m_xpos = m_arena->getBlockWidth();
			m_arena->setViewportX(m_arena->getViewportX() + 1);
		}
	}
	else if ((m_xpos) < 1)
	{
		if (m_arena->getViewportX() != 0)
		{
			m_xpos = m_arena->getScreenWidth() - m_arena->getBlockWidth();
			m_arena->setViewportX(m_arena->getViewportX() - 1);
		}
	}
}

void Hero::addScore(int amount)
{
	m_score += amount;
}

void Hero::loseLife()
{
	m_lives--;
	std::cout << "Life lost" << std::endl;
}
