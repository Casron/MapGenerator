using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public enum BorderType{Normal,River,Canyon, Cliff, Beach};
public class TileBorder
{
	public BorderType borderType;

	public TileVertex v0;
	public TileVertex v1;

	public AnTile t0;
	public AnTile t1;


	public TileBorder(TileVertex from, TileVertex to)
	{
		v0 = from;
		v1 = to;
		borderType = BorderType.Normal;

		if (object.ReferenceEquals(from, to.center))
		{
			to.centerBorder = this;
		}
		else if (object.ReferenceEquals(from, to.right))
		{
			to.rightBorder = this;
		}
		else if (object.ReferenceEquals(from, to.left))
		{
			to.leftBorder = this;
		}
	}
	public void AddListener(AnTile t)
	{
		if (t0 == null)
		{
			t0 = t;
		}
		else if (t1 == null)
		{
			t1 = t;
		}
		else
		{
			Debug.Log("We've got a problem (" + t0.x + " " + t0.y + ") (" +t1.x + " " + t1.y + ") (" +  " " + t.x + " " + t.y + ")");
		}
	}
}
