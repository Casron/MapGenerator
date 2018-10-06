using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TileVertex
{

	public int y;
	public int x;
	public TileVertex left;
	public TileVertex right;
	public TileVertex center;

	public TileBorder centerBorder;
	public TileBorder leftBorder;
	public TileBorder rightBorder;
	public Vector2 pos;

	public TileVertex(Vector2 p, int tx, int ty)
	{
		x = tx;
		y = ty;
		left = null;
		right = null;
		center = null;
		pos = p;
	}
	public TileBorder GetSharedBorder(TileVertex other)
	{
		if (object.ReferenceEquals(left,other))
		{
			return leftBorder;
		}
		else if (object.ReferenceEquals(right,other))
		{
			return rightBorder;
		}
		else if (object.ReferenceEquals(center,other))
		{
			return centerBorder;
		}
		return null;
	}
}
