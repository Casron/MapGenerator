using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//A 3d mesh generator designed to take in a height and width, and create a high-poly hexagonal map
//Procedurally generates mesh geometry using hexagon properties and trigonometry
//This code snippet is written in C# and is designed to for use with the Unity engine
public sealed class MapMesh : MonoBehaviour 
{
	
	//Textures for various tiles
	public Texture2D grassReference; //A reference to a square seamless texture that is used to paint grass tiles
	public Texture2D sandReference; //A reference to a square seamless texture that is used to paint sand tiles
	MeshController meshController;
	
	//The width and height of the chunk
	int width;
	int height;
	
	//Variables pertaining the the mesh
	int[][][][] vertMap;//Maps vertiex indices onto a 4 dimensional array. This makes it easier to deal with vertices that are shared by multiple tiles
	Vector3[] verts; //Contains an array of 3d coordinates detailing where each vertice is located in 3d world space
	Vector2[] uvs; //Contains an array of 2d coordinates detailing where each vertice is located on a UV map
	Vector3[] normals; //Contains an array of 3d coordinates detailing the normal vector of each vertices
	int[] tris;//Contains an array of integers. Every set of 3 integers in the array are representative of a triangle. 
	//IE 0,1,2 would connect verts[0],verts[1],verts[2] as a triangle
	Texture2D chunkTexture; //The mesh's albedo texture
	
	//Overall game settings
	GameSettings settings;

	//Main build function called by the map generator
	public IEnumerator Build(int w, int h, MeshController mC, GameSettings s)
	{
		//Initializing necessary variables
		settings = s;
		meshController = mC;
		width = w;
		height = h;
		int numVerts = CalculateVertices(w,h);
		int numTris = TriArrayCalculator(w,h);
		InitializeMeshData(numVerts, numTris);
		
		//Lays out the geometry for the mesh as well as the UV coordinates
		yield return PlaceVertices(numVerts);
		
		//Connects the vertices by a triangle array
		yield return SetTriangles();
		
		//Calculates the normal values of each vertices
		yield return SetNormals(numVerts, numTris);
		
		//Assigns these variables to a mesh object
		CreateMesh();
		
		//Draws the texture
		yield return CreateTexture();
		
		yield return null;
	}
	
	//Paints one hexagon using a pre-defined texture
	void ChangeTextureAtPoint(int x, int y, Texture2D reference)
	{

		//Finds the starting points on the UV map
		int startX = (UVParams.hexWidth * x) + UVParams.halfHexWidth;
		if (y % 2 == 1)
		{
			startX += UVParams.hexWidth/2;
		}
		int startY = UVParams.hexHeightDiff * y;
		int maxW = UVParams.halfHexWidth + 1;

		//Starting from the bottom left of a hexagon, paint each pixel
		for(int w = -UVParams.halfHexWidth - 1; w <= maxW; w++)
		{
			int wPos = startX + w;
			int shift = (int)Mathf.Abs(w);
			int hStart = (int)Mathf.Floor(0.5773503f * shift);
			int hSize = (int)Mathf.Ceil((UVParams.halfHexHeight * 2.0f) - (1.15625f * shift));
			int maxArrayHeight = hStart + hSize + 1;
			for(int h = hStart - 1; h <= maxArrayHeight; h++)
			{
				int hPos = startY + h;

				chunkTexture.SetPixel(wPos, hPos, reference.GetPixel(wPos % 64, hPos % 64));
			}
		}
		chunkTexture.Apply();
	}

	//Copies the vertices from one mesh onto another
	//This helps reduce loading time by preventing thousands of floating point operations from having to be recalculated in identical meshes
	public IEnumerator CopyMesh(int w, int h, MeshController mC, GameSettings s, MapMesh other)
	{
		width = w;
		height = h;

		if (width != other.GetWidth())
		{
			//The algorithm breaks if the widths are different so thinner chunks are built normally
			yield return Build(w,h,mC,s);
		}
		else
		{
			//Initializes necessary variables
			settings = s;
			meshController = mC;
			int numVerts = CalculateVertices(w,h);
			int numTris = TriArrayCalculator(w,h);
			InitializeMeshData(numVerts, numTris);

			//Copies the variables from the reference mesh
			yield return CopyVariables(other);
			CreateMesh();
			yield return CreateTexture();
			yield return null;
		}

	}

	//Copies variables from a reference mesh to avoid performing thousands of redundant floating point operation
	IEnumerator CopyVariables(MapMesh other)
	{
		int[][][][] otherVertMap = other.GetVertMap();
		Vector3[] otherVerts = other.GetVerts();
		Vector2[] otheruvs = other.GetUVs();
		Vector3[] otherNormals = other.GetNormals();
		int[] otherTris = other.GetTris();


		

		//Assigns each variable in N time
		for(int i = 0; i < verts.Length; i++)
		{
			verts[i] = otherVerts[i];
			uvs[i] = otheruvs[i];
			normals[i] = otherNormals[i];
		}
		for(int i = 0; i < tris.Length; i++)
		{
			tris[i] = otherTris[i];
		}
		for(int sH = 0; sH < height; sH++)
		{
			for (int sW = 0; sW < width; sW++)
			{
				for(int w = 0; w < VertParams.longHeight; w++)
				{
					for(int h = 0; h < vertMap[sH][sW][w].Length; h++)
					{
						vertMap[sH][sW][w][h] = otherVertMap[sH][sW][w][h];
					}
				}
			}
		}

		yield return null;
	}

	//To give the illusion of a round world, the map mesh objects will change their positions as the camera moves right or left
	void Update()
	{
		Vector3 pos = transform.position;
		float x = pos.x;
		float camX = Camera.main.gameObject.transform.position.x;

		//If the camera is greater than half a map's worth of distance, move over to the other side
		if (x - camX > settings.width/2.0f)
		{
			transform.position = new Vector3(x - (settings.width * HexParams.shortDiameter),pos.y,pos.z);
		}
		else if (x - camX < settings.width/-2.0f)
		{
			transform.position = new Vector3(x + (settings.width * HexParams.shortDiameter),pos.y,pos.z);
		}
	}
	public void UpdateTile(AnTile anTile, int sW, int sH)
	{
		//TODO allow tiles to be updated from outside sources
	}
	IEnumerator PlaceVertices(int numVerts)
	{
		int verticeIterator = 0;

		//Assigns constants that will be used to place UV coordinates
		float uvXDistance = 1.0f/(ChunkParams.maxWidth + 0.5f);//The X distance between the center of 2 hexagons on the UV map
		float uvXRadius = uvXDistance/2.0f;
		float uvYHalfRadius = 1.0f/(1.0f + (3.0f*ChunkParams.maxHeight));//One quarter of the way from the bottom of a hexagon to the top
		float uvYRadius = uvYHalfRadius * 2.0f;
		float uvHeightDistance = uvYHalfRadius * 3.0f; //The distance along the Y axis for 2 hexagons on the UV map

		float uvWDistance = (uvXDistance/VertParams.endPoint);//The X distance between 2 indivdual vertices on the UV map
		float uvYDistance = (uvYRadius/VertParams.midPoint);//The Y distance between 2 vertices on the UV map
		float uvYShiftLength = uvYDistance/2.0f;//The amount vertices are raised upwards by for each W value they are away from the midpoint

		for(int sH = 0; sH < height; sH++)
		{
			
			float centerZ = sH * HexParams.heightDistance;//Finds the Z coordinate of the center of the current tile
			float uvCenterZ = ((sH * uvHeightDistance) + uvYRadius);
			for(int sW = 0; sW < width; sW++)
			{
				//Finds the X coordinate of the center of the current tile
				float centerX = sW * HexParams.shortDiameter;
				float uvCenterX = (uvXRadius) + (uvXDistance * sW);

				//If this is an odd row, shift over the center by half a hexagon
				if (sH % 2 == 1)
				{
					centerX += HexParams.shortRadius;
					uvCenterX += (uvXRadius);
				}

				//Deals with overlapping vertices
				HandleOverlap(sW,sH);

				for(int w = 0; w < VertParams.longHeight; w++)
				{
					//Finds the X coordiate of the individual vertice
					float wPosition = (centerX - HexParams.shortRadius) + (VertParams.widthDistance * w);
					float uvWPosition = (uvCenterX - uvXRadius) + (uvWDistance * w);

					//Lifts the Z coordinate based on the distance of the X coordinate from the midpoint
					float hShift = Mathf.Abs(w - VertParams.midPoint) * VertParams.halfHeight;
					float uvHShift = Mathf.Abs(w - VertParams.midPoint) * uvYShiftLength;

					for(int h = 0; h < vertMap[sH][sW][w].Length; h++)
					{
						//Finds the Z coordinate of the vertice
						float hPosition = (centerZ - HexParams.longRadius) + hShift + (VertParams.heightDistance * h);
						float uvHPosition = (uvCenterZ - uvYRadius) + uvHShift + (uvYDistance * h);

						if (vertMap[sH][sW][w][h] == -1)
						{
							//If the overlap detector hasnt already placed this vertice, create a new vertice
							vertMap[sH][sW][w][h] = verticeIterator;
							verts[verticeIterator] = new Vector3(wPosition, 0.0f, hPosition);
							uvs[verticeIterator] = new Vector2(uvWPosition,uvHPosition);
							
							verticeIterator++;
						}
					}
				}
			}
			yield return null;
		}
		yield return null;
	}
	IEnumerator SetNormals(int numVerts, int numTris)
	{
		Vector3[] normalSet = new Vector3[numVerts];
		int[] normalLengths = new int[numVerts];
		
  
		//Takes the cross product of each triangle and uses it to find the normal vector for the vertice
		int q = 0;
		while(q < numTris)
		{
			Vector3 vert1 = verts[tris[q]];
			Vector3 vert2 = verts[tris[q+1]];
			Vector3 vert3 = verts[tris[q+2]];
			q+=3;

			Vector3 v1 = vert2 - vert1;
			Vector3 v2 = vert3 - vert1;
			Vector3 planeNormal = Vector3.Cross(v1,v2);

			planeNormal.Normalize();

			for(int z = 3; z > 0; z--)
			{
				normalSet[tris[q-z]] += planeNormal;
				normalLengths[tris[q-z]] += 1;
			}
			if (q % 9000 == 0)
			{
				yield return null;
			}
		}
		yield return null;

		//Checks if the normal vector for a vertice has already been set, if not, a new normal vector is created
		for(int i = 0; i < numVerts; i++)
		{
			if (normals[i].x < -5.0f && normals[i].y < -5.0f && normals[i].z < -5.0f)
			{
				normals[i] = new Vector3(0.0f,0.0f,0.0f);
				normals[i] += (normalSet[i]/((float)normalLengths[i]));
			}
		}
		yield return null;
	}	

	//Builds the triangle array
	IEnumerator SetTriangles()
	{
		int triIterator = 0;
		int midPoint = VertParams.midPoint;
		int endPoint = VertParams.endPoint;

		//Sets the triangle array for each tile, building from the bottom of the hexagon to the top
		for(int sH = 0; sH < height; sH++)
		{
			for(int sW = 0; sW < width; sW++)
			{
				for(int w = 0; w < midPoint; w++)
				{
					//All triangles on the left side of the hexagon can be built according to a pattern save for the very last one
					int localLen = vertMap[sH][sW][w].Length;
					for(int h = 0; h < localLen - 1; h++)
					{
						tris[triIterator] = vertMap[sH][sW][w][h];
						triIterator++;
						tris[triIterator] = vertMap[sH][sW][w+1][h+1];
						triIterator++;
						tris[triIterator] = vertMap[sH][sW][w+1][h];
						triIterator++;
						tris[triIterator] = vertMap[sH][sW][w][h];
						triIterator++;
						tris[triIterator] = vertMap[sH][sW][w][h+1];
						triIterator++;
						tris[triIterator] = vertMap[sH][sW][w+1][h+1];
						triIterator++;
					}
					tris[triIterator] = vertMap[sH][sW][w][localLen - 1];
					triIterator++;
					tris[triIterator] = vertMap[sH][sW][w+1][localLen];
					triIterator++;
					tris[triIterator] = vertMap[sH][sW][w+1][localLen-1];
					triIterator++;
				}
				//On the other hand, all triangles on the right side of the hexagon can be built according to a pattern except for the first
				for(int w = midPoint; w < endPoint; w++)
				{
					int localLen = vertMap[sH][sW][w].Length;
					tris[triIterator] = vertMap[sH][sW][w][0];
					triIterator++;
					tris[triIterator] = vertMap[sH][sW][w][1];
					triIterator++;
					tris[triIterator] = vertMap[sH][sW][w+1][0];
					triIterator++;
					for(int h = 1; h < localLen - 1; h++)
					{
						tris[triIterator] = vertMap[sH][sW][w][h];
						triIterator++;
						tris[triIterator] = vertMap[sH][sW][w+1][h];
						triIterator++;
						tris[triIterator] = vertMap[sH][sW][w+1][h-1];
						triIterator++;
						tris[triIterator] = vertMap[sH][sW][w][h];
						triIterator++;
						tris[triIterator] = vertMap[sH][sW][w][h+1];
						triIterator++;
						tris[triIterator] = vertMap[sH][sW][w+1][h];
						triIterator++;
					}
				}
				yield return null;
			}
		}
		yield return null;
	}

	//Checks edge vertices to see if they have already been created for another tile
	void HandleOverlap(int sW, int sH)
	{

		int midPoint = VertParams.midPoint;
		int endPoint = VertParams.endPoint;


		//All tiles not in the sW = 0 position will share vertices with the tile to their left
		if (sW > 0)
		{
			for(int h = 0; h <= midPoint; h++)
			{
				vertMap[sH][sW][0][h] = vertMap[sH][sW-1][endPoint][h];
			}
		}

		//All tiles at sH > 0 will share vertices with the tiles to their bottom left or right. Most tiles will share both.
		if (sH > 0)
		{
			if (sW > 0 || sH % 2 == 1)
			{
				int otherSW;
				if (sH % 2 == 1)
				{
					otherSW = sW;
				}
				else
				{
					otherSW = sW-1;
				}
				for(int w = 0; w <= midPoint; w++)
				{
					int len = vertMap[sH-1][otherSW][w+midPoint].Length;
					vertMap[sH][sW][w][0] = vertMap[sH-1][otherSW][w + midPoint][len-1];
				}
			}
			if (sW < width - 1 || sH % 2 == 0)
			{
				int otherSW;
				if (sH % 2 == 1)
				{
					otherSW = sW+1;
				}
				else
				{
					otherSW = sW;
				}
				for(int w = midPoint; w <= endPoint; w++)
				{
					int len = vertMap[sH-1][otherSW][w - midPoint].Length;
					vertMap[sH][sW][w][0] = vertMap[sH-1][otherSW][w - midPoint][len-1];
				}				
			}
		}

	}

	IEnumerator CreateTexture()
	{
		//Creates a copy of a base reference texture and sets it as the texture for the mesh
		chunkTexture = new Texture2D(UVParams.texWidth,UVParams.texHeight);
		Texture2D reference = (Texture2D)gameObject.GetComponent<MeshRenderer>().material.GetTexture("_MainTex");
		for(int w = 0; w < UVParams.texWidth; w++)
		{	
			for(int h = 0; h < UVParams.texHeight; h++)
			{
				chunkTexture.SetPixel(w,h,reference.GetPixel(w,h));
			}
			if (w % 10 == 0)
			{
				yield return null;
			}
		}
		chunkTexture.Apply();
		gameObject.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", chunkTexture);
		yield return null;
	}
	//Creates the actual mesh object the players will see and interact with
	void CreateMesh()
	{
		Mesh workingMesh = new Mesh();
		MeshFilter filter = gameObject.GetComponent<MeshFilter>();
		workingMesh.vertices = verts;
		workingMesh.uv = uvs;
		workingMesh.normals = normals;
		workingMesh.triangles = tris;
		gameObject.GetComponent<MeshCollider>().sharedMesh = workingMesh;
		filter.mesh = workingMesh;
	}

	//Calculates the number of vertices of a hexagon minus all of the edge vertices
	int InnerTile()
	{
		int addon = 0;
		int val = 1;
		for(int i = 0; i < VertParams.minorVerts; i++)
		{
			addon += 6;
			val += addon;
		}
		return val;
	}

	//Calculates the number of triangles for a single hexagon
	int SingleTriCalculator()
	{
		int x = VertParams.minorVerts + 1;
		return (18 * (x) * (x));	
	}

	//Calculates the number of triangles required by the mesh
	int TriArrayCalculator(int w, int h)
	{
		return SingleTriCalculator() * w * h;
	}

	//Calculates the number of vertices required by the mesh
	int CalculateVertices(int w, int h)
	{
		int width = w;
		int height = h;
		int minorVerts = VertParams.minorVerts;
		//The following equation calculates the number of vertices required by a mesh, taking into account that edge vertices can be shared by multiple tiles
		int val = (width * (2 + (4 * minorVerts) + (height * InnerTile()))) + (((2*width)+1)*minorVerts * (height-1)) + ((minorVerts + 2)*(width + 1)*height);
		return val;

		
	}

	//Initializes the array sizes for the mesh data
	void InitializeMeshData(int numVerts, int numTris)
	{
		uvs = new Vector2[numVerts];
		normals = new Vector3[numVerts];
		verts = new Vector3[numVerts];
		tris = new int[numTris];
		vertMap = new int[height][][][];
		for(int sH = 0; sH < height; sH++)
		{
			vertMap[sH] = new int[width][][];
			for(int sW = 0; sW < width; sW++)
			{
				SetVerticeArray(sW,sH);
			}
		}
		InitializeNormalData(numVerts);
	}

	//Initalizes the vertice array for each hexagon tile
	void SetVerticeArray(int sW, int sH)
	{
		vertMap[sH][sW] = new int[VertParams.longHeight][];
		for(int w = 0; w < VertParams.longHeight; w++)
		{
			int overMid = w - VertParams.midPoint;
			if (w < VertParams.midPoint)
			{
				vertMap[sH][sW][w] = new int[VertParams.shortHeight+w];
			}
			else
			{
				vertMap[sH][sW][w] = new int[VertParams.longHeight-overMid];
			}
			for(int h = 0; h < vertMap[sH][sW][w].Length; h++)
			{
				vertMap[sH][sW][w][h] = -1;
			}
		}
	}

	//By default all normal vectors are set to -10
	void InitializeNormalData(int numVerts)
	{
		for(int i = 0; i < numVerts; i++)
		{
			normals[i] = new Vector3(-10.0f,-10.0f,-10.0f);
		}
	}

	//Prevents an out of bounds exception by looping x and y coordinates that exceed the array limit
	int WidthRollover(int sW)
	{
		if (sW < 0)
		{
			sW = width + sW;
		}
		else if (sW >= width)
		{
			sW = sW - width;
		}
		return sW;
	}
	public int[][][][] GetVertMap()
	{
		return vertMap;
	}
	public Vector3[] GetVerts()
	{
		return verts;
	}
	public Vector2[] GetUVs()
	{
		return uvs;
	}
	public Vector3[] GetNormals()
	{
		return normals;
	}
	public int[] GetTris()
	{
		return tris;
	}
	public int GetWidth()
	{
		return width;
	}
	public int GetHeight()
	{
		return height;
	}
	public Texture2D GetChunkTexture()
	{
		return chunkTexture;
	}

}
