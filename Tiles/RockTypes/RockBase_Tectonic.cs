using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockBase_Tectonic : RockBase 
{

    public override string GetName()
    {
        return "Tectonically active";
    }
    public override void Build(int w, int h)
    {
        Spread(new Position(w,h),100.0f, (int)Mathf.Floor(Random.Range(0.0f,2.95f)));
    }
    protected override void MakePotentialYields()
    {
        
    }
    void Spread(Position p, float chance, int direction)
    {
        int x = p.x;
        int y = p.y;
        if (Random.Range(0.0f,100.0f) < chance)
        {
            map.tileMap[y][x].rockBase = this;
            int minW;
            int maxW;
            if (Random.Range(0.0f,100.0f) < 15.0f)
            {
                direction = (int)Mathf.Floor(Random.Range(0.0f,2.95f));
            }

            if (direction == 0)
            {
                MinMaxW(out minW, out maxW, y, 0);
                Spread(RollOver(new Position(x + minW, y)),chance - 30.0f, direction);
                Spread(RollOver(new Position(x + maxW, y)),chance - 30.0f, direction);
            }
            else if (direction == 1)
            {
                MinMaxW(out minW, out maxW, y, 1);
                Spread(RollOver(new Position(x + minW, y+1)),chance - 30.0f, direction);
                Spread(RollOver(new Position(x + maxW, y-1)),chance - 30.0f, direction);
            }
            else if (direction == 2)
            {
                MinMaxW(out minW, out maxW, y, 1);
                Spread(RollOver(new Position(x + minW, y-1)),chance - 30.0f, direction);
                Spread(RollOver(new Position(x + maxW, y+1)),chance - 30.0f, direction);
            }
        }
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
        else if (chance <= 95.0f)
        {
            return 3;
        }
        else
        {
            return 4;
        }
    }
}
