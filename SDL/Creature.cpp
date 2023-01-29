#include "Creature.h"
#include <iostream>

Creature::Creature(std::string name, int xpos, int ypos, Arena* arena, SDL_Renderer* renderer, std::string filename)
	:m_name{ name }, m_xpos{ xpos }, m_ypos{ ypos }, m_arena{ arena }, m_renderer{ renderer }, arenaXPosition{xpos}, arenaYPosition{ypos}
{
	m_bitmaps.push_back(new Bitmap(renderer, filename, 32, 32, true));

	m_height = m_bitmaps.at(0)->getHeight();
	m_width = m_bitmaps.at(0)->getWidth();

}

void Creature::display()
{
	std::cout << m_name << ": " << m_xpos << ", " << m_ypos;
}

void Creature::left()
{
	for (int i = 0; i < moveValue; i++)
	{
		if (!m_arena->isWall(m_xpos - 1, m_ypos) && !m_arena->isWall(m_xpos - 1, m_ypos + m_height))
		{
			m_xpos -= 1;
			arenaXPosition -= 1;
		}
	}

}

void Creature::right()
{
	for (int i = 0; i < moveValue; i++)
	{
		if (!m_arena->isWall(m_xpos + m_width, m_ypos) && !m_arena->isWall(m_xpos + m_width, m_ypos + m_height - 1))
		{
			m_xpos += 1;
			arenaXPosition += 1;
		}
	}
}

void Creature::up(bool useDefault)
{
	if (useDefault)
	{
		for (int i = 0; i < moveValue; i++)
		{
			if (!m_arena->isWall(m_xpos, m_ypos - 1) && !m_arena->isWall(m_xpos + m_width - 1, m_ypos - 1))
			{
				m_ypos -= 1;
				arenaYPosition -= 1;
			}
		}
	}
	else
	{
		if (!m_arena->isWall(m_xpos, m_ypos - 1) && !m_arena->isWall(m_xpos + m_width - 1, m_ypos - 1))
		{
			m_ypos -= 1;
			arenaYPosition -= 1;

		}

		if (jumpRepeats < (jumpValue*fallValue))
		{
			jumpRepeats += fallValue;
		}
		else
		{
			//std::cout << "Jumpend" << std::endl;
			isJump = false;
			jumpRepeats = 0;
		}
	}

}

void Creature::jump()
{
	up(false);
}

void Creature::down(bool useDefault)
{
	if (useDefault)
	{
		for (int i = 0; i < moveValue; i++)
		{
			if (!m_arena->isWall(m_xpos, m_ypos + m_height + 1) && !m_arena->isWall(m_xpos + m_width - 1, m_ypos + m_height + 1))
			{
				if (useDefault)
				{
					m_ypos += 1;
					arenaYPosition += 1;
				}
			}
		}
	}
	else
	{
		for (int i = 0; i < fallValue; i++)
		{
			if (!m_arena->isWall(m_xpos, m_ypos + m_height + 1) && !m_arena->isWall(m_xpos + m_width - 1, m_ypos + m_height + 1))
			{
				m_ypos += 1;
				arenaYPosition += 1;
			}
		}
	}

}

void Creature::setX(int xpos)
{
	m_xpos = xpos;
}

void Creature::setY(int ypos)
{
	m_ypos = ypos;
}

void Creature::setarenaXPosition(int xPos)
{
	arenaXPosition = xPos;
}

int Creature::getX()
{
	return m_xpos;
}

int Creature::getY()
{
	return m_ypos;
}

int Creature::getHeight()
{
	return m_height;
}

int Creature::getWidth()
{
	return m_width;
}

bool Creature::getIsJump()
{
	return isJump;
}

void Creature::setIsJump(bool change)
{
	isJump = change;
}

Arena* Creature::getArena()
{
	return m_arena;
}


void Creature::draw()
{
	m_bitmaps.at(m_current_frame)->draw(m_xpos, m_ypos);
}

void Creature::addFrame(std::string filename)
{
	m_bitmaps.push_back(new Bitmap(m_renderer, filename, m_xpos, m_ypos, true));
}

void Creature::nextFrame()
{
	m_current_frame++;
	if (m_current_frame > (m_bitmaps.size() - 1))
	{
		m_current_frame = 0;
	}
}



