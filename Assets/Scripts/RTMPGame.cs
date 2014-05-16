using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.Multiplayer;

public class RTMPGame : RealTimeMultiplayerListener {
	public delegate void OnReceivedMessageData(byte[] data);
	public delegate void OnReceivedSetupPlayerData(string senderId, byte[] data);

	private OnReceivedMessageData synInputDataFunc;
	public void RegisterSynInputCallback(OnReceivedMessageData func){
		synInputDataFunc = func;
	}

	private OnReceivedSetupPlayerData setupPlayerDataFunc;
	public void RegisterSetPlayerDataCallback(OnReceivedSetupPlayerData func){
		setupPlayerDataFunc = func;
	}

	RTMPManager mRTMPManager;

	public RTMPGame() {

		StartConnect("Waiting for connect to server");
		RTMPManager.Instance.enabled = false;
		GUIManager.Instance.Register(ConnectingHUD);
		mRTMPManager = RTMPManager.Instance;

	}

	#region Connecting & Room
	string connectStatus  = "....";
	float connectProgress = 0f;

	void StartConnect(string msg){

		connectStatus = msg;
		connectProgress = 0f;
		mRoomSetupStartTime = Time.time;
	}
	
	
	void FinishConnect(bool success) {
		GUIManager.Instance.Register(null);
		if (success) {
			connectStatus = string.Format("Setup Finished");
			connectProgress = 100f;
			PlayerManager.Instance.LoadPlayers();
		}
		else 
		{
			PlayGamesPlatform.Instance.RealTime.LeaveRoom();
			GUIManager.Instance.Clear();
			RTMPManager.Instance.ConnectionAbord();
		}
	}
	
	
	// speed of the "fake progress" (to keep the player happy)
	// during room setup
	const float FakeProgressSpeed = 1.0f;
	const float MaxFakeProgress = 30.0f;
	float mRoomSetupStartTime = 0.0f;

	
	public float RoomSetupProgress {
		get {
			float fakeProgress = (Time.time - mRoomSetupStartTime) * FakeProgressSpeed;
			if (fakeProgress > MaxFakeProgress) {
				fakeProgress = MaxFakeProgress;
			}
			float progress = connectProgress + fakeProgress;
			return progress < 99.0f ? progress : 100.0f;
		}
	}
	
	
	void ConnectingHUD() {
		int t = 100, w = Screen.width, h = 80, s = 10;
		
		// draw status
		GUI.Label(new Rect(20, t, w, h/2), connectStatus);
		t += h/2 + s;
		
		// draw setup progress
		Rect prect = new Rect(20, t, Screen.width - 40, 40);
		GUI.DrawTexture(prect, mRTMPManager.BGTexture);
		prect.width =  prect.width * RoomSetupProgress * 0.01f;
		GUI.DrawTexture(prect, mRTMPManager.FGTexture);
		
		// draw abort button
//		if (GUI.Button(new Rect(Screen.width - 90, Screen.height - 90, 80, 60), "Abort")){
//			FinishConnect(false);
//		}
	}

	public void OnRealTimeMessageReceived(bool isReliable, string senderId, byte[] data) {
	
		
		if (data[0] == (byte)'I'){
			if (synInputDataFunc != null)
				synInputDataFunc(data);
		}
		else if (data[0] == (byte)'S'){
			if (setupPlayerDataFunc != null)
				setupPlayerDataFunc(senderId, data);
		}
	}
	
	#endregion

	#region Realtime

	// Handler setup room progresss
	public void OnRoomSetupProgress(float progress) {
		// update progress bar
		// (progress goes from 0.0 to 100.0)
		connectProgress = progress;
	}
	
	// Handler setup room finished
	public void OnRoomConnected(bool success) {
		if (success) {
			// Successfully connected to room!
			// ...start playing game...
			
			FinishConnect(true);
		} else {
			// Error!
			// ...show error message to user...
			FinishConnect(false);
		}
	}
	
	// Handle Connection Events
	public void OnLeftRoom() {
		// display error message and go back to the menu screen
		FinishConnect(false);
		// (do NOT call PlayGamesPlatform.Instance.RealTime.LeaveRoom() here --
		// you have already left the room!)
	}
	
	public void OnPeersConnected(string[] participantIds) {
		// react appropriately (e.g. add new avatars to the game)
	}
	
	public void OnPeersDisconnected(string[] participantIds) {
		// react appropriately (e.g. remove avatars from the game)
		FinishConnect(false);
	}
	#endregion
}
