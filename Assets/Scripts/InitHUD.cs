using UnityEngine;
using System.Collections;

public class InitHUD : MonoBehaviour {

	void OnGUI() {
		int t = 0, l = 0, w = 100, h = 80, s = 10;
		t = Screen.height / 2 - h/2 - s/2;
		l = Screen.width /2 - w/2;

		if (GUI.Button(new Rect(l, t, w, h), "GO TO GAME")){
			Application.LoadLevel(1);
		}
	}
}
