using UnityEngine;
using System.Collections;

public class SmoothFollowHUD : MonoBehaviour {
	public float heightView = 20f;
	public float lowView = 5f;
	public Transform player;
	public SmoothFollow sfController;

	// Use this for initialization
	void Start () {
	
	}
	
	void OnGUI () {

		if (GUILayout.Button("Up")) {
			sfController.height = heightView;
		}

		if (GUILayout.Button("Down")) {
			sfController.height = lowView;
		}
	}
}
