using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerationHelper
{
	protected GameSettings settings;
	protected Map map;
	protected int width;
	protected int height;
	public GenerationHelper(GameSettings s, Map m)
	{
		settings = s;
		width = settings.width;
		height = settings.height;
		map = m;
	}
	protected void MinMaxW(out int minW, out int maxW, int h)
	{
		if (h % 2 == 0)
		{
			minW = 0;
			maxW = 1;
		}
		else
		{
			minW = -1;
			maxW = 0;
		}
	}
	protected void MinMaxW(out int minW, out int maxW, int y, int h)
	{
		if (h == 0)
		{
			minW = -1;
			maxW = 1;
		}
		else if (y % 2 == 0)
		{
			minW = 0;
			maxW = 1;
		}
		else
		{
			minW = -1;
			maxW = 0;
		}
	}
	protected Position RollOver(Position p)
	{
		int x = p.x;
		int y = p.y;
		if (x >= settings.width)
		{
			x = (x-settings.width);
		}
		if (x < 0)
		{
			x = (settings.width + x);
		}
		if (y >= settings.height)
		{
			y = settings.height - 1;
		}
		if (y < 0)
		{
			y = 0;
		}
		return new Position(x,y);
	}
}
