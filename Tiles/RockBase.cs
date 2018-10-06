using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RockBase
{
	protected static Map map;
	protected int ID;
	//protected YieldSet yieldSet;


	public abstract void Build(int w, int h);
	protected abstract int GetNumYields();
	protected abstract void MakePotentialYields();
	public abstract string GetName();
	public RockBase()
	{
		MakePotentialYields();
	}
	public static IEnumerator AssignRockBases(Map m)
	{
		map = m;
		RockBase[] rockBase = new RockBase[]{new RockBase_Standard(), new RockBase_Biomass(), new RockBase_Volcanic(), new RockBase_Tectonic(), new RockBase_Crystalline()};
		for(int h = 0; h < map.height; h++)
		{
			for(int w = 0; w < map.width; w++)
			{
				float chance = Random.Range(0.0f,100.0f);
				if (chance < 1.0f)
				{
					rockBase[3].Build(w,h);
				}
				else if (chance < 4.0f)
				{
					rockBase[2].Build(w,h);
				}
				else if (chance < 9.0f)
				{
					rockBase[1].Build(w,h);
				}
				else if (chance < 10.0f)
				{
					rockBase[4].Build(w,h);
				}
			}
			yield return null;
		}
		for(int h = 0; h < map.height; h++)
		{
			for(int w = 0; w < map.width; w++)
			{
				if (map.tileMap[h][w].rockBase == null)
				{
					rockBase[0].Build(w,h);
				}
			}
			yield return null;
		}
		yield return null;
	}
	protected Position RollOver(Position p)
	{
		int width = map.width;
		int height = map.height;
		int y = p.y;
		int x = p.x;
		if (x < 0)
		{
			x = width + x;
		}
		else if (x >= width)
		{
			x = x - width;
		}
		if (y >= height)
		{
			y = height-1;
		}
		else if (y < 0)
		{
			y = 0;
		}
		return new Position(x,y);
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
}