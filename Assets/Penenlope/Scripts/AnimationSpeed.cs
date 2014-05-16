

//////////////////////////////////////////////////////////////
// AnimationSpeed.js
// Penelope iPhone Tutorial
//
// AnimationSpeed sets the speed for the default clip of an
// Animation component. This was used for adjusting the playback
// speed of the introductory flythrough.
//////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;

public class AnimationSpeed : MonoBehaviour {

	public Animation animationTarget;
	public float speed = 1.0f;
	
	void Start() 
	{
		animationTarget[ animationTarget.clip.name ].speed = speed;
	}
}
