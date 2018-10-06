using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockBase_Standard : RockBase
{
    public override string GetName()
    {
        return "Standard minerals";
    }
    public override void Build(int w, int h)
    {
        map.tileMap[h][w].rockBase = this;
    }
    protected override void MakePotentialYields()
    {
        
    }
    protected override int GetNumYields()
    {
        float chance = Random.Range(0.0f,100.0f);
        if (chance <= 25.0f)
        {
            return 0;
        }
        else if (chance <= 80.0f)
        {
            return 1;
        }
        else if (chance <= 95.0f)
        {
            return 2;
        }
        else
        {
            return 3;
        }
    }
}
