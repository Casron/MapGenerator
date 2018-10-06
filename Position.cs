using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Position
{

	public int x;
	public int y;

	public Position(int X, int Y)
	{
		x = X;
		y = Y;
	}
	public static float Distance(Position p1, Position p2)
	{
		int x1 = p1.x;
		int x2 = p2.x;
		int y1 = p1.y;
		int y2 = p2.y;

		float xDist = Mathf.Abs(x2-x1);
		float yDist = Mathf.Abs(y2-y1);

		xDist *= xDist;
		yDist *= yDist;

		return Mathf.Sqrt(xDist + yDist);
	}
}
