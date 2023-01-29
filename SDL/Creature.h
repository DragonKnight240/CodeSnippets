#pragma once
#include "Arena.h"
#include <string>
#include "Bitmap.h"

class Creature
{
protected:
	int m_xpos;
	int m_ypos;
	int arenaXPosition;
	int arenaYPosition;
	int m_height;
	int m_width;
	int moveValue = 4;
	int jumpValue = 110;
	int fallValue = 3;
	int jumpRepeats = 0;
	bool isJump = false;
	std::vector<Bitmap*> m_bitmaps;
	Arena* m_arena;

	std::string m_name;

	SDL_Renderer* m_renderer;
	int m_current_frame = 0;

public:
	Creature(std::string name, int xpos, int ypos, Arena* arena, SDL_Renderer* renderer, std::string filename);

	void display();
	void left();
	void right();
	void up(bool useDefault = true);
	void jump();
	void down(bool useDefault = true);

	void setX(int xpos);
	void setY(int ypos);
	void setarenaXPosition(int xPos);

	int getX();
	int getY();
	int getHeight();
	int getWidth();
	bool getIsJump();
	void setIsJump(bool change);

	Arena* getArena();

	void draw();
	void addFrame(std::string filename);
	void nextFrame();



};

