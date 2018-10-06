using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Map //Maps should always have both and odd height and width
{
	public int width;
	public int height;
	public TileVertex[][] vertexMap;
	public AnTile[][] tileMap;
	public float[][] heightMap;
	public float[][] heatMap;
	public float[][] moistureMap;

	public Map(int w, int h)
	{
		width = w;
		height = h;
	}
	public IEnumerator Setup()
	{
		vertexMap = InitializeVertex();
		yield return BuildVertexMap();
		yield return TileSetup();
	}
	public IEnumerator TileSetup()
	{
		tileMap = new AnTile[height][];
		for(int h = 0; h < height; h++)
		{
			tileMap[h] = new AnTile[width];
			for(int w = 0; w < width; w++)
			{
				AnTile localTile = new AnTile(w,h);
				AssignTileVertices(localTile);
				tileMap[h][w] = localTile;
			}
			yield return null;
		}
		yield return null;
	}
	private TileVertex[][] InitializeVertex()
	{
		TileVertex[][] vertMap;
		int vertexHeight = (height * 2) + 2;
		vertMap = new TileVertex[vertexHeight][];
		for(int i = 0; i < vertexHeight; i++)
		{
			vertMap[i] = new TileVertex[width];
		}
		return vertMap;
	}
	private IEnumerator BuildVertexMap()
	{
		if (width % 2 == 0 || height % 2 == 0)
		{
			throw new Exception("Both map dimensions must be odd");
		}

		yield return SetVertexPositions();
		yield return SetVertexAdjacency();
		yield return BuildVertexBorders();
	}
	private IEnumerator SetVertexPositions()
	{
		int section = 3;
		int vertexHeight = vertexMap.Length;
		int width = vertexMap[0].Length;
		float y = (HexParams.longRadius * -1.0f) - HexParams.longRadius;
		for(int h = 0; h < vertexHeight; h++)
		{
			if (section == 0 || section == 2)
			{
				y += HexParams.longHalfRadius;
			}
			else
			{
				y += HexParams.longRadius;
			}
			for(int w = 0; w < width; w++)
			{
				float x = (HexParams.shortDiameter * w) - HexParams.shortRadius;
				if (section >= 2)
				{
					x += HexParams.shortRadius;
				}
				vertexMap[h][w] = new TileVertex(new Vector2(x,y), w, h);
			}

			section++;
			if (section >= 4)
			{
				section = 0;
			}
			yield return null;
		}		
		yield return null;
	}
	private IEnumerator SetVertexAdjacency()
	{
		int section = 3;
		int vertexHeight = vertexMap.Length;
		int width = vertexMap[0].Length;
		for(int h = 0; h < vertexHeight; h++)
		{
			for(int w = 0; w < width; w++)
			{
				HandleSectionAdjacency(section, w, h);
			}
			section += 1;
			if (section >= 4)
			{
				section = 0;
			}
			yield return null;
		}
		yield return null;
	}
	private IEnumerator BuildVertexBorders()
	{
		int width = vertexMap[0].Length;
		for(int h = 0; h < vertexMap.Length; h++)
		{
			for(int w = 0; w < width; w++)
			{
				TileVertex localVertex = vertexMap[h][w];
				BuildBorders(localVertex);
			}
			yield return null;
		}
		yield return null;
	}
	private void AssignTileVertices(AnTile localTile)
	{
		int x = localTile.x;
		int y = localTile.y;
		int wShift = y % 2;
		int baseY = y * 2;

		TileVertex v0 = null;
		TileVertex v1 = null;
		TileVertex v2 = null;
		TileVertex v3 = null;
		TileVertex v4 = null;
		TileVertex v5 = null;
		if (x != width - 1)
		{
			v0 = vertexMap[baseY + 3][x + wShift];
			v1 = vertexMap[baseY + 2][x + 1];
			v2 = vertexMap[baseY + 1][x + 1];
			v3 = vertexMap[baseY][x + wShift];
			v4 = vertexMap[baseY + 1][x];
			v5 = vertexMap[baseY + 2][x];
		}
		else
		{
			if (wShift == 0)
			{
				v0 = vertexMap[baseY + 3][x];
				v1 = vertexMap[baseY + 2][0];
				v2 = vertexMap[baseY + 1][0];
				v3 = vertexMap[baseY][x];
				v4 = vertexMap[baseY + 1][x];
				v5 = vertexMap[baseY + 2][x];
			}
			else
			{
				v0 = vertexMap[baseY + 3][0];
				v1 = vertexMap[baseY + 2][0];
				v2 = vertexMap[baseY + 1][0];
				v3 = vertexMap[baseY][0];
				v4 = vertexMap[baseY + 1][x];
				v5 = vertexMap[baseY + 2][x];
			}
		}



		localTile.v0 = v0;
		localTile.v1 = v1;
		localTile.v2 = v2;
		localTile.v3 = v3;
		localTile.v4 = v4;
		localTile.v5 = v5;
	

		localTile.SetBorder("uRBorder",v0.GetSharedBorder(v1));

		localTile.SetBorder("rBorder", v1.GetSharedBorder(v2));	

		localTile.SetBorder("dRBorder", v2.GetSharedBorder(v3));

		localTile.SetBorder("dLBorder", v3.GetSharedBorder(v4));

		localTile.SetBorder("lBorder",v4.GetSharedBorder(v5));

		localTile.SetBorder ("uLBorder", v5.GetSharedBorder(v0));

	}
	void BuildBorders(TileVertex localVertex)
	{
		if (localVertex.center != null)
		{
			if (localVertex.centerBorder == null)
			{
				localVertex.centerBorder = new TileBorder(localVertex,localVertex.center);
			}
		}
		if (localVertex.rightBorder == null)
		{
			localVertex.rightBorder = new TileBorder(localVertex,localVertex.right);
		}
		if (localVertex.leftBorder == null)
		{
			localVertex.leftBorder = new TileBorder(localVertex, localVertex.left);
		}
	}
	void HandleSectionAdjacency(int section, int w, int h)
	{
		TileVertex localVertex = vertexMap[h][w];
		int height = vertexMap.Length;
		int width = vertexMap[0].Length;
		if (section == 0)
		{
			if (w != 0)
			{
				localVertex.left = vertexMap[h-1][w-1];
			}
			else
			{
				localVertex.left = vertexMap[h-1][width - 1];	
			}
			localVertex.right = vertexMap[h-1][w];
			localVertex.center = vertexMap[h+1][w];
		}
		else if (section == 1)
		{
			if (w != 0)
			{
				localVertex.left = vertexMap[h+1][w-1];
			}
			else
			{
				localVertex.left = vertexMap[h+1][width - 1];	
			}
			localVertex.right = vertexMap[h+1][w];
			localVertex.center = vertexMap[h-1][w];
		}
		else if (section == 2)
		{
			if (h != height-1)
			{
				localVertex.center = vertexMap[h+1][w];
			}
			if (w != width - 1)
			{
				localVertex.right = vertexMap[h-1][w+1];
			}
			else
			{
				localVertex.right = vertexMap[h-1][0];
			}
			localVertex.left = vertexMap[h-1][w];
		}
		else if (section == 3)
		{
			if (h != 0)
			{
				localVertex.center = vertexMap[h-1][w];
			}
			if (w != width - 1)
			{
				localVertex.right = vertexMap[h+1][w+1];
			}
			else
			{
				localVertex.right = vertexMap[h+1][0];
			}
			localVertex.left = vertexMap[h+1][w];
		}
	}

}
