using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockBase_Biomass : RockBase 
{

    public override string GetName()
    {
        return "Biomass deposits";
    }
    public override void Build(int w, int h)
    {
        Spread(new Position(w,h),100.0f);
    }
    protected override void MakePotentialYields()
    {
        
    }
    void Spread(Position p, float chance)
    {
        int x = p.x;
        int y = p.y;
        if (map.tileMap[y][x].rockBase == null)
        {
            if (Random.Range(0.0f,100.0f) < chance)
            {
                map.tileMap[y][x].rockBase = this;

                for(int h = -1; h <= 1; h++)
                {
                    int minW;
                    int maxW;
                    MinMaxW(out minW, out maxW, y, h);
                    for(int w = minW; w <= maxW; w++)
                    {
                        if (h != 0 || w != 0)
                        {
                            Spread(RollOver(new Position(x+w,y+h)),chance - 20.0f);
                        }
                    }
                }
            }
        }
    }
    protected override int GetNumYields()
    {
        float chance = Random.Range(0.0f,100.0f);
        if (chance <= 20.0f)
        {
            return 0;
        }
        else if (chance <= 50.0f)
        {
            return 1;
        }
        else if (chance <= 90.0f)
        {
            return 2;
        }
        else
        {
            return 3;
        }
    }
}
