using UnityEngine;
using System.Collections;

public class ScaleObjToCamera : MonoBehaviour {
	
	private float startingDistance;
	private Vector3 startingScale;
	
	void Start()
	{
		startingScale = transform.localScale;
	}
	
	void Update()
	{
		//float distance = Vector3.Distance (Camera.main.transform.position, transform.position);
		//transform.localScale = startingScale * (1-Camera.main.transform.position.y/distance);
	}
}