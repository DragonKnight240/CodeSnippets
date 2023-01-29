#pragma once
#include <SDL.h>
#include "Monster.h"
#include "Coin.h"
#include "Hero.h"
#include "Bitmap.h"

struct SDL_Window;
struct SDL_Renderer;
class Bitmap;

class Game
{
private:
	SDL_Window* m_window{ nullptr };
	SDL_Renderer* m_renderer{ nullptr };
	bool isRunning{ false };
	bool coinsGenerated{ false };
	int currentCoin = 0;

	Hero* controled;
	Arena* arena;
	
	std::vector<Coin*> coins;
	std::vector<Monster*> enemies;
	

public:
	Game();
	~Game();

	void Update();
	Arena* getArena();
	Hero* getHero();

	void render();
	void generateObjects();
	void drawObjects();
};
