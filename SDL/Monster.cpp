#include "Monster.h"
#include <iostream>

Monster::Monster(std::string name, int xpos, int ypos, Arena* arena, SDL_Renderer* renderer, std::string filename, bool patrol, bool verticle, int agro)
	:Creature{name, xpos, ypos, arena, renderer, filename}, patroling{patrol}, Verticle{verticle}, playerRange{agro}
{
}

void Monster::chase(Hero* hero)
{
	if (playerRange < (hero->getX() - (m_xpos - m_arena->getViewportX() * m_arena->getScreenWidth())) && -playerRange > (hero->getX() - (m_xpos - m_arena->getViewportX() * m_arena->getScreenWidth())))
	{
		if (hero->getX() < (m_xpos - m_arena->getViewportX() * m_arena->getScreenWidth()))
		{
			left();
		}
		if (hero->getX() > (m_xpos - m_arena->getViewportX() * m_arena->getScreenWidth()))
		{
			right();
		}
	}
}

bool Monster::eaten(Hero* hero)
{
	if (collidesWithCreature(hero))
	{
		return true;
	}
	else
	{
		return false;
	}
}

bool Monster::collidesWithCreature(Hero* creature)
{
	if (m_xpos - (m_arena->getScreenWidth() * m_arena->getViewportX()) < creature->getX() + (creature->getWidth() - 1) &&
		m_xpos - (m_arena->getScreenWidth() * m_arena->getViewportX()) + (m_height - 1) > creature->getX())
	{
		if (m_ypos < creature->getY() + (creature->getHeight() - 1) &&
			m_ypos + (m_width - 1) > creature->getY())
		{
			return true;
		}
	}
	return false;
}

void Monster::patrolVerticle()
{
	if (PatrolUp)
	{
		Creature::up();
	}
	else
	{
		Creature::down();
	}
}

void Monster::patrolHorizontal()
{
	if (PatrolRight)
	{
		Creature::right();
	}
	else
	{
		Creature::left();
	}
}

void Monster::move(Hero* hero)
{
	if (patroling)
	{
		if (Verticle)
		{
			patrolVerticle();

			if (m_arena->isStop(m_xpos - m_arena->getViewportX()* m_arena->getScreenWidth(), m_ypos) || m_arena->isStop((m_xpos -m_arena->getViewportX() * m_arena->getScreenWidth() ) + m_width - 1, m_ypos) || m_arena->isStop((m_xpos - m_arena->getViewportX() * m_arena->getScreenWidth())+ m_width - 1, m_ypos + m_height - 1))
			{
				//std::cout << "Swith Direction" << std::endl;
				PatrolUp = !PatrolUp;
			}
			else
			{
				//std::cout << "Not stop" << std::endl;
				onStop = false;
			}
		}
		else
		{
			patrolHorizontal();

			if (m_arena->isStop((m_xpos - m_arena->getViewportX() * m_arena->getScreenWidth()), m_ypos)|| m_arena->isStop((m_xpos - m_arena->getViewportX() * m_arena->getScreenWidth()) + m_width - 1, m_ypos))
			{
				//std::cout << "Switch Direction" << std::endl;
				PatrolRight = !PatrolRight;
			}
		}


	}
	else
	{
		chase(hero);
	}
}

void Monster::monsterDraw(int x, int y)
{
	m_bitmaps[m_current_frame]->draw(x, y);
}
