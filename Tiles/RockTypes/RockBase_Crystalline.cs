using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockBase_Crystalline : RockBase 
{
	public override string GetName()
	{
		return "Crystalline formations";
	}
	public override void Build(int x, int y)
	{
		map.tileMap[y][x].rockBase = this;
		for(int h = -1; h <= 1; h++)
		{
			int minW;
			int maxW;
			MinMaxW(out minW, out maxW, y, h);
			for(int w = minW; w <= maxW; w++)
			{
				if (Random.Range (0.0f,100.0f) <= 20.0f)
				{
					Position p = RollOver(new Position(x+w,h+y));
					map.tileMap[p.y][p.x].rockBase = this;
				}
			}
		}
	}
    protected override void MakePotentialYields()
    {
		
    }
    protected override int GetNumYields()
    {
        float chance = Random.Range(0.0f,100.0f);
        if (chance <= 25.0f)
        {
            return 1;
        }
        else if (chance <= 80.0f)
        {
            return 2;
        }
        else
        {
            return 3;
        }
    }

}
