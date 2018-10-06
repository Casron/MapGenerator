using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;


public class AnTile
{

    public RockBase rockBase;
    public float height;
    public int x;
    public int y;

    public float realX;
    public float realY;

    



    //Vertices start at the top point and go clockwise
    public TileVertex v0;
    public TileVertex v1;
    public TileVertex v2;
    public TileVertex v3;
    public TileVertex v4;
    public TileVertex v5;

    public TileBorder uRBorder; //v0 and v1
    public TileBorder rBorder; //v1 and v2
    public TileBorder dRBorder; //v2 and v3
    public TileBorder dLBorder;//v3 and v4
    public TileBorder lBorder;//v4 and v5
    public TileBorder uLBorder;//v5 and v0

    public AnTile(int w, int h)
    {
        x = w;
        y = h;
        rockBase = null;

        float xShift = 0.0f;
        if (y % 2 != 0)
        {
            xShift += HexParams.shortRadius;
        }

        realY = y*HexParams.heightDistance;
        realX = (x * HexParams.shortDiameter) + xShift;
    }
    public void SetBorder(string border, TileBorder b)
    {
        this.GetType().GetField(border).SetValue(this, b);
        if (b != null)
        {
            b.AddListener(this);
        }
    }
    public string GetTileInfo()
    {
        string retString = x.ToString() + " " + y.ToString() + " \n";
        if (rockBase != null)
        {
            return retString + rockBase.GetName();
        }
        return retString + "Unknown";
    }
}
