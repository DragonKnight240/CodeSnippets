#include "Game.h"
#include <iostream>
#include <conio.h>


Game::Game()
{
	SDL_Init(SDL_INIT_VIDEO);

	m_window = SDL_CreateWindow(
		"My first window",
		250,
		50,
		740,
		580,
		0
	);

	if (nullptr == m_window)
	{
		std::cout << "Window initialisation failed: " << SDL_GetError() << std::endl;
		std::cout << "Press a key to continue" << std::endl;
		_getch();
		return;
	}

	m_renderer = SDL_CreateRenderer(
		m_window,
		-1,
		0
	);

	if (nullptr == m_renderer)
	{
		std::cout << "Renderer initialisation failed" << SDL_GetError() << std::endl;
	}

	arena = new Arena(m_renderer, 0, 0, 740, 580,1);
	generateObjects();

	controled = new Hero("Person", 32 , 32, 3, arena, m_renderer, "Assets/Hero/1.bmp");
}

Game::~Game()
{
	if (nullptr != controled)
	{
		delete controled;
		controled = nullptr;
	}

	if (nullptr != m_window)
	{
		SDL_DestroyWindow(m_window);
		m_window = nullptr;
	}
	if (nullptr != m_renderer)
	{
		SDL_DestroyRenderer(m_renderer);
		m_renderer = nullptr;
	}

	SDL_Quit();
}


void Game::Update()
{
	if (nullptr == m_renderer)
	{
		return;
	}

	if ((!arena->isWall(controled->getX(), controled->getY() + controled->getHeight()) ||
		!arena->isWall(controled->getX() + controled->getWidth() - 1, controled->getY() + controled->getHeight())))
	{
		if (!controled->getIsJump())
		{
			controled->Creature::down(false);
		}

	}

	if (getHero()->getIsJump())
	{
		getHero()->jump();
	}

	SDL_RenderClear(m_renderer);

	render();
	getHero()->positionCheck();
	controled->draw();
	drawObjects();

	SDL_RenderPresent(m_renderer);
	//SDL_Delay(100);
}

void Game::render()
{
	for (int i = arena->getViewportY(); i < arena->getHeight(); i++)
	{
		for (int j = arena->getViewportX(); j < arena->getWidth(); j++)
		{

			switch (arena->getLayout()[i].at(j))
			{
			case 'W':
			{
				arena->drawWall((j * arena->getBlockWidth()) - (arena->getViewportX() * arena->getScreenWidth()), (i * arena->getBlockHeight()) - (arena->getViewportY() * arena->getScreenHeight()), true);
				break;
			}
			case 'G':
			{
				arena->drawWall((j * arena->getBlockWidth()) - (arena->getViewportX() * arena->getScreenWidth()), (i * arena->getBlockHeight()) - (arena->getViewportY() * arena->getScreenHeight()), false);
				break;
			}
			default:
				break;
			}
		}
	}
}

void Game::generateObjects()
{
	Coin* coin;
	Monster* enemy;

	for (int i = 0; i < arena->getHeight(); i++)
	{
		for (int j = 0; j < arena->getWidth(); j++)
		{
			switch (arena->getLayout()[i].at(j))
			{
			case 'C':
			{
				coin = new Coin(j * arena->getBlockWidth(), i * arena->getBlockHeight(), 100, "Assets/Coin.bmp", m_renderer, arena);
				coins.push_back(coin);
				break;
			}

			case 'V':
			{
				enemy = new Monster("VertPatrol", j * arena->getBlockWidth(), i * arena->getBlockHeight(), arena, m_renderer, "Assets/Beetle/9.bmp", true, true, 0);
				enemies.push_back(enemy);
				break;
			}
			case 'H':
			{
				enemy = new Monster("HorizPatrol", j * arena->getBlockWidth(), i * arena->getBlockHeight(), arena, m_renderer, "Assets/Beetle/9.bmp", true, false, 0);
				enemies.push_back(enemy);
				break;
			}
			case 'A':
			{
				enemy = new Monster("Chase", j * arena->getBlockWidth(), i * arena->getBlockHeight(), arena, m_renderer, "Assets/Beetle/9.bmp", false, false, 100);
				enemies.push_back(enemy);
				break;
			}
			default:
				break;
			}
		}
	}
}

void Game::drawObjects()
{
	for (Coin* coin:coins)
	{
		if (!coin->collideWith(controled))
		{
			if (!coin->getPickedUp())
			{
				coin->getBitmap()->draw((coin->getxPos() - (arena->getScreenWidth() * arena->getViewportX())), (coin->getyPos()));
			}
		}
		else
		{
			coin->setPickedUp(true);
			controled->addScore(coin->getPointsValue());
		}
	}

	for (Monster* enemy:enemies)
	{
		enemy->move(controled);
		if (enemy->collidesWithCreature(controled))
		{
			controled->loseLife();
		}
		if (!(enemy->getX() < arena->getScreenWidth() * arena->getViewportX()))
		{
			enemy->monsterDraw(enemy->getX() - (arena->getScreenWidth() * arena->getViewportX()), enemy->getY());
		}
	}
}

Arena* Game::getArena()
{
	return arena;
}

Hero* Game::getHero()
{
	return controled;
}



