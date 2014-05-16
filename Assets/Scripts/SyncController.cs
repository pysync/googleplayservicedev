using UnityEngine;
using System.Collections;

public class SyncController : MonoBehaviour {

	void Awake() {
		if (!IsSelf) {
			RTMPManager.RTMPGameInstance.RegisterSynInputCallback(OnSyncInput);
		}
	}


	// state
	protected bool synJumping = false;
	protected bool synMoving  = false;
	protected bool synReset   = false;

	// data
	protected Vector3 synTargetLocation = Vector3.zero;
	protected Vector3 syncPosition = Vector3.zero;

	public bool IsSelf;

	/// <summary>
	///  Report Input Data to Server
	// 1. Move State: M = moving, J = jumping, R = reset
	// 2. Move data: x - z - y = 0
	/// </summary>
	byte[] mInputPackage = new byte[4];
	public void SendInput(char state, float x = 0f, float z = 0f) {
		mInputPackage[0] = (byte)'I';
		mInputPackage[1] = (byte)state;
		mInputPackage[2] = (byte)x;
		mInputPackage[3] = (byte)z;
		GooglePlayGames.PlayGamesPlatform.Instance.RealTime.SendMessageToAll(false, mInputPackage);
	}

	public void OnSyncInput(byte[] data){

		if (data[1] == (byte)'M') {
			synMoving = true;
			float x = (float)data[2];
			float z = (float)data[3];
			synTargetLocation = new Vector3(x, 0, z);
		}
		if (data[1] == (byte)'P'){
			synReset = true;
			float x = (float)data[2];
			float z = (float)data[3];
			syncPosition = new Vector3(x, 0, z);
		}
		else if (data[1] == 'J') {
			synJumping = true;
		}

	}
	string inputStatus = string.Empty;
	void OnGUI() {
		int t = IsSelf ? 20 : 60, w = Screen.width, h = 20;
		if (!IsSelf){
			inputStatus = string.Format("State: M{0}:J{1}:R{2} - P: {3}", synMoving, synJumping, synReset, synTargetLocation);
		}
		else {
			inputStatus = string.Format("SendInput: M{0}- P: {1}:{2}", (char)mInputPackage[1],  (float)mInputPackage[2],  (float)mInputPackage[3]);
		}
		// draw status
		GUI.Label(new Rect(20, t, w, h), inputStatus);
	}
}
