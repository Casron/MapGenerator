using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshController : MonoBehaviour 
{
	GameSettings settings;
	public GameObject chunkPrefab;
	GameObject[][] chunks;

	public IEnumerator BuildMap(GameSettings s)
	{
		//Initializing Variables
		settings = s;
		int maxHeight = ChunkParams.maxHeight;
		int maxWidth = ChunkParams.maxWidth;
		int wChunks = (int)Mathf.Ceil(settings.width/(float)maxWidth);
		int hChunks = (int)Mathf.Ceil(settings.height/(float)maxHeight);
		chunks = new GameObject[hChunks][];
		MapMesh baseMesh = null;


		//Loops through and creates each mesh chunk one at a time
		for(int bH = 0; bH < hChunks; bH++)
		{
			int locHeight = GetChunkSize(bH,settings.height,maxHeight);
			float zPos = bH * maxHeight * HexParams.heightDistance;
			chunks[bH] = new GameObject[wChunks];

			for(int bW = 0; bW < wChunks; bW++)
			{
				float xPos = bW * maxWidth * HexParams.shortDiameter;
				Vector3 pos = new Vector3(xPos,0.0f,zPos);
				int locWidth = GetChunkSize(bW,settings.width,maxWidth);
				chunks[bH][bW] = (GameObject)Instantiate(chunkPrefab,pos,Quaternion.identity);
				if (bW == 0 && bH == 0)
				{
					baseMesh = chunks[bH][bW].GetComponent<MapMesh>();
					yield return baseMesh.Build(locWidth, locHeight, this, settings);
				}
				else
				{
					yield return chunks[bH][bW].GetComponent<MapMesh>().CopyMesh(locWidth, locHeight, this, settings, baseMesh);
				}
			}
		}

		//Makes the chunks visible
		for(int bH = 0; bH < hChunks; bH++)
		{
			for(int bW = 0; bW < wChunks; bW++)
			{
				chunks[bH][bW].GetComponent<MeshCollider>().enabled = true;
				chunks[bH][bW].GetComponent<MeshRenderer>().enabled = true;
			}
			yield return null;
		}

		yield return null;
	}
	int GetChunkSize(int iterator, int baseNumber, int max)
	{
		int remHeight = baseNumber - (max * iterator);
		int ret;
		if (remHeight >= max)
		{
			ret = max;
		}
		else
		{
			ret = remHeight;
		}	
		return ret;	
	}
}
