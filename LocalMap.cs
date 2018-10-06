using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalMap : MonoBehaviour 
{
	Map map;
	public IEnumerator Setup(int width, int height)
	{
		map = new Map(width, height);
		yield return map.Setup();
		Camera.main.gameObject.GetComponent<CameraController>().localMap = map;
	}
}
