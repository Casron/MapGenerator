using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapGenerator : MonoBehaviour 
{
	public MeshController meshController;
	GameSettings settings;
	GameStarter starter;
	public static Map map;
	public void CommenceGeneration(GameSettings gS, GameStarter s)
	{
		settings = gS;
		starter = s;
		StartCoroutine(Generate_SinglePlayer());
	}
	private IEnumerator Generate_SinglePlayer()
	{
		int width = settings.width;
		int height = settings.height;
		map = new Map(width, height);


		starter.ChangeLoadingInfo("Initializing Tiles");
		yield return map.Setup();
		yield return GameObject.Find("LocalMap").GetComponent<LocalMap>().Setup(width, height);


		starter.ChangeLoadingInfo("Building Rock Layers");
		yield return RockBase.AssignRockBases(map);
		
		starter.ChangeLoadingInfo("Building LandMass");
		
		
		starter.ChangeLoadingInfo("Building HeatMap");
		starter.ChangeLoadingInfo("Building MoistureMap");
		starter.ChangeLoadingInfo("Building Soil Layers");


		starter.ChangeLoadingInfo("Building WorldMesh");
		yield return meshController.BuildMap(settings);

		starter.ChangeLoadingInfo("Placing Players");

		
		


		starter.GeneratorFinish();
	}
	public static T[][] DimensionalArray<T>(int width, int height)
	{
		T[][] ret = new T[height][];
		for(int i = 0; i < height; i++)
		{
			ret[i] = new T[width];
		}
		return ret;
	}
}
