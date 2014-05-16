using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GUIManager : MonoBehaviour {
	private static GUIManager mInstance;
	public static GUIManager Instance{
		get {
			return mInstance;
		}
	}

	public delegate void DrawFunc();
	private DrawFunc mFunc = null;



	void Awake() {
		mInstance = this;
	}

	public void Register(DrawFunc func){
		mFunc = func;
	}


	public void Clear() {
		mFunc = null;
	}

	void OnGUI() {
		if (mFunc != null){
			mFunc();
		}
	}
}
