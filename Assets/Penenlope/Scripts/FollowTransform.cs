//////////////////////////////////////////////////////////////
// FollowTransform.js
// Penelope iPhone Tutorial
//
// FollowTransform will follow any assigned Transform and 
// optionally face the forward vector to match for the Transform
// where this script is attached.
//////////////////////////////////////////////////////////////
/// 
using UnityEngine;
using System.Collections;


public class FollowTransform : MonoBehaviour {

	public Transform targetTransform ;		// Transform to follow
	public bool faceForward = false;		// Match forward vector?
	private Transform thisTransform;


	// Use this for initialization
	void Start () {
		// Cache component lookup at startup instead of doing this every frame
		thisTransform = transform;
	}
	
	// Update is called once per frame
	void Update () {
		thisTransform.position = targetTransform.position;
		
		if ( faceForward )
			thisTransform.forward = targetTransform.forward;
	}
}
