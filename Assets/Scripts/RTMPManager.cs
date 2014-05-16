using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.Multiplayer;


public class RTMPManager : MonoBehaviour, OnStateLoadedListener, RealTimeMultiplayerListener{

	private static RTMPManager mInstance;
	public static RTMPManager Instance {
		get {
			return mInstance;
		}
	}

	void Awake() {
		mInstance = this;
	}

	enum ContentTypes { None, MenuHUD, Standby, Auth, Realtime, LeaderBoard, Friends, Achv, Cloud, Connecting};
	ContentTypes contentType = ContentTypes.MenuHUD;
    ContentTypes prevType     = ContentTypes.None;

	#region GUI
	void OnGUI() {
		// Tweek color
		GUI.color = Color.white;

        // draw header
        HeaderHUD();

		// fore Auth first
		if (!PlayGamesPlatform.Instance.IsAuthenticated())
			contentType = ContentTypes.Auth;

		// draw content
		switch(contentType) {
            
        case ContentTypes.MenuHUD:
            MenuHUD(); break;
            
		case ContentTypes.Auth:
			AuthHUD(); break;
			
		case ContentTypes.Achv:
			AchvHUD(); break;

		case ContentTypes.Friends:
			FriendHUD(); break;

		case ContentTypes.LeaderBoard:
			LeaderBoardHUD(); break;

		case ContentTypes.Realtime:
			RealTimeHUD(); break;
        
            
        case ContentTypes.Standby:
            StandbyHUD(); break;
            
		case ContentTypes.Connecting:
			ConnectingHUD(); break;
			
		case ContentTypes.Cloud:
			CloudHUD(); break;

		default: break;
		}


	}
	#endregion

    #region HEADER
    string headerMessage = "";
    int T = 0;

    void HeaderHUD() {
        int t = 10, l = 0, w = 100, h = 40, s = 10;
        GUI.Label(new Rect(20, t, 300, h), headerMessage);
        
		if (contentType != ContentTypes.MenuHUD){
			l = Screen.width - (w + s);
			if (GUI.Button(new Rect(l, t, w, h), "Back")){
				contentType = ContentTypes.MenuHUD;
			}
		}


//		l = Screen.width - (w + s);
//		if (GUI.Button(new Rect(l, t, w, h), "Back")){
//			Application.LoadLevel(0);
//        }

        T = t + h + s;
    }

    #endregion

    #region HUDMENU
    void MenuHUD() {
        int t = T, l = 0, w = Screen.width, h = 80, s = 10;

        t += h + s;
        if (GUI.Button(new Rect(l, t, w, h), "Authenticate Menu")){
            contentType = ContentTypes.Auth;
        }

		t += h + s;
		if (GUI.Button(new Rect(l, t, w, h), "Friends Menu")){
			contentType = ContentTypes.Friends;
		}

    
        t += h + s;
        if (GUI.Button(new Rect(l, t, w, h), "LeaderBoard Menu")){
            contentType = ContentTypes.LeaderBoard;
        }


        t += h + s;
		if (GUI.Button(new Rect(l, t, w, h), "Achievement Menu")){
            contentType = ContentTypes.Achv;
        }

		t += h + s;
		if (GUI.Button(new Rect(l, t, w, h), "Save Clound Menu")){
			contentType = ContentTypes.Cloud;
		}

		t += h + s;
		if (GUI.Button(new Rect(l, t, w, h), "Realtime Menu")){
			contentType = ContentTypes.Realtime;
		}
    }

    #endregion

    #region Standby
    string standbyMessage = string.Empty;
    void Standby(string msg){
        prevType = contentType;
        standbyMessage = msg;
        contentType = ContentTypes.Standby;

    }

    
    void EndStandy() {
        contentType = ContentTypes.MenuHUD;
        prevType = ContentTypes.None;
        standbyMessage = string.Empty;
    }

    void StandbyHUD() {
		int t = T, l = 0, w = Screen.width, h = 80;
        
        t = (Screen.height - T)/2 - h/2;
        l = Screen.width / 2 - w/2;
        GUI.Label(new Rect(l, t, w, h), standbyMessage);
    }

    #endregion

	#region Auth
    // auto sign flag
	private bool mAuthOnStart = false;
	private System.Action<bool> mAuthCallback = null;


	void AuthHUD() {
		int t = T, l = 0, w = Screen.width, h = 80, s = 10;

		GUI.Label(new Rect(l, t, w, h), "AutoAuth: " + mAuthOnStart);
		
		t += h + s;
		if (GUI.Button(new Rect(l, t, w, h), "AuthOnStart")){
			mAuthOnStart = !mAuthOnStart;
			PlayerPrefs.SetInt("AuthOnStart", mAuthOnStart ? 1 : 0);
		}
		t += h + s;
		if (GUI.Button(new Rect(l, t, w, h), "SignIn")){
            Standby("Please wait...");
            PlayGamesPlatform.Instance.Authenticate(mAuthCallback);
		}

		t += h + s;
		if (GUI.Button(new Rect(l, t, w, h), "SignOut")){
            mAuthOnStart  = false;
            PlayGamesPlatform.Instance.SignOut();
			//Application.LoadLevel(0);
		}



	}

	void Start() {

		// handler authenticate callback function
        mAuthCallback = (bool success)=>{
            EndStandy();
            headerMessage = string.Format("Auth : {0}", success);
        };

        // make default social platform
        PlayGamesPlatform.Activate();

        // enable logger
        PlayGamesPlatform.DebugLogEnabled = true;

		// get auth setup from data
		mAuthOnStart = PlayerPrefs.GetInt("AuthOnStart", 0) == 1 ? true: false;

        // try to silent sign auto
        if (mAuthOnStart) {
            Standby("Please wait...");
            PlayGamesPlatform.Instance.Authenticate(mAuthCallback, true);
        }
	}
  	#endregion

	#region Cloud

	class Data {
		public string Desc;
		public string Index;
		public string Progress;

		public void Set(string index, string desc, string progress){
			Index = index;
			Desc = desc;
			Progress = progress;
		}

		public void MergerWith(Data other){
			Index = !string.IsNullOrEmpty(other.Index) ? other.Index : Index;
			Desc  = !string.IsNullOrEmpty(other.Desc) ? other.Desc : Desc;
			Progress = !string.IsNullOrEmpty(other.Progress) ? other.Progress : Progress; 
		}

		public override string ToString ()
		{
			return string.Format ("DT:{0}:{1}:{2}", Index, Desc, Progress);
		}


		public byte[] ToBytes(){
			return System.Text.ASCIIEncoding.Default.GetBytes(ToString());
		}

		public static Data FromBytes(byte[] data){
			string str = System.Text.ASCIIEncoding.Default.GetString(data);
			return FromString(str);
		}

		public static Data FromString(string str){
			Data data = new Data();
			if (string.IsNullOrEmpty(str)){
				return data;
			}

			string[] p = str.Split(new char[]{':'});
			if (!p[0].Equals("DT")) {
				return data;
			}
			data.Set(p[1], p[2], p[3]);
			return data;
		}
	}


	Data mLocalData  = new Data();
	Data mCloudData  = new Data();
	string mTIndex = "";
	string mTDesc  = "";
	string mTProgress = "";

	void CloudHUD() {
		int t = T, l = 0, w = Screen.width, h = 80, s = 10;

		GUI.Label(new Rect(l, t, w, h), string.Format("Local Data: {0}", mLocalData.ToString()));

		t += h + s;
		GUI.Label(new Rect(l, t, w, h), string.Format("Cloud Data: {0}", mCloudData.ToString()));

		t += h + s;
		mTIndex = GUI.TextField(new Rect(l, t, w/3 - s/3, h/2), mTIndex);
		mTDesc = GUI.TextField(new Rect(l + (w/3 + s/3), t, w/3 - s/3, h/2), mTDesc);
		mTProgress = GUI.TextField(new Rect(l + (w/3 + s/3) * 2, t, w/3 - s/3, h/2), mTProgress);

		t += h + s;
		if (GUI.Button(new Rect(l, t, w, h), "Set LocalData")){
			mLocalData.Set(mTIndex, mTDesc, mTProgress);
		}

		t += h + s;
		if (GUI.Button(new Rect(l, t, w, h), "LoadFromCloud")){
			if (PlayGamesPlatform.Instance.IsAuthenticated()){
				headerMessage = string.Format("Loading data");
				PlayGamesPlatform.Instance.LoadState(0, this);
			}
		}

		t += h + s;
		if (GUI.Button(new Rect(l, t, w, h), "SaveToCloud")){
			if (PlayGamesPlatform.Instance.IsAuthenticated()){
				headerMessage = string.Format("Saving data");
				PlayGamesPlatform.Instance.UpdateState(0, mLocalData.ToBytes(), this);
			}
		}
	}

	// Data was successfully loaded from the cloud
	public void OnStateLoaded(bool success, int slot, byte[] data) {
		headerMessage = string.Format("Loaded: {0}", success);
		if (success) {
			// Process data here
			mCloudData = Data.FromBytes(data);
		}
	}
	
	// Conflict in cloud data occurred
	public byte[] OnStateConflict(int slot, byte[] local, byte[] server) {
		headerMessage = string.Format("Resolving conflict");

		// decode byte arrays into game progress and merge them
		Data localData  = local != null ? Data.FromBytes(local) : new Data();
		Data serverData = server != null ? Data.FromBytes(server) : new Data();
		localData.MergerWith(serverData);

		// resolve conflict
		return localData.ToBytes();
	}
	
	public void OnStateSaved(bool success, int slot) {
		headerMessage = string.Format("State Saved: {0}", success);
	}

	#endregion

	#region Achv
	string[] mUnLockAchvIds = new string[] {"CgkIs4TBruUaEAIQAw", "CgkIs4TBruUaEAIQBQ"};
	int mUnlockAchvIndex = 0;

	string[] mIAchvIds = new string[] {"CgkIs4TBruUaEAIQAg"};
	int mIAchvIndex = 0; int mIAchvSteps = 5;


	void AchvHUD() {
		int t = T, l = 0, w = Screen.width, h = 80, s = 10;
		if (GUI.Button(new Rect(l, t, w, h), "ShowAchievementsUI")){
            if (PlayGamesPlatform.Instance.IsAuthenticated()){
				PlayGamesPlatform.Instance.ShowAchievementsUI();
			}
		}

		t += h + s * 3;
		if (GUI.Button(new Rect(l, t, w/2 - s/2, h/2), "Next UAchv")){
			mUnlockAchvIndex = mUnlockAchvIndex >= mUnLockAchvIds.Length - 1? 0 : mUnlockAchvIndex + 1;
		}

		GUI.Label(new Rect(l + (w/2 + s/2), t, w/2 - s/2, h/2), mUnLockAchvIds[mUnlockAchvIndex]);


		t += h + s;
		if (GUI.Button(new Rect(l, t, w, h), "UnlockAchievement")){
			if (PlayGamesPlatform.Instance.IsAuthenticated()){
				PlayGamesPlatform.Instance.ReportProgress(mUnLockAchvIds[mUnlockAchvIndex], 100.0f, (bool success)=>{
					headerMessage = string.Format("Unlock: {0}", success);
				});
			}
		}

		t += h + s * 3;
		if (GUI.Button(new Rect(l, t, w/2 - s/2, h/2), "Next IAchv")){
			mIAchvIndex = mIAchvIndex >= mIAchvIds.Length - 1? 0 : mIAchvIndex + 1;
		}

		GUI.Label(new Rect(l + (w/2 + s/2), t, w/2 - s/2, h/2), mIAchvIds[mIAchvIndex]);

		t += h + s;
		if (GUI.Button(new Rect(l, t, w, h), "IncrementAchievement")){
			if (PlayGamesPlatform.Instance.IsAuthenticated()){
				PlayGamesPlatform.Instance.IncrementAchievement(mIAchvIds[mIAchvIndex], mIAchvSteps, (bool success)=>{
					headerMessage = string.Format("Incr: {0}", success);
				});

			}
		}

	}
	#endregion

	#region Friend
	void FriendHUD() {

	}
	#endregion

	#region LeaderBoard
	int mHighestPostedScore = 0;
	int mScore = 0;
	string mLeaderboardId = "CgkIs4TBruUaEAIQBA";

	void LeaderBoardHUD() {
		int t = T, l = 0, w = Screen.width, h = 80, s = 10;
		if (GUI.Button(new Rect(l, t, w, h), "LeaderBoard")){
			if (PlayGamesPlatform.Instance.IsAuthenticated()){
				PlayGamesPlatform.Instance.ShowLeaderboardUI(mLeaderboardId);
			}
		}

		t += h + s;
		GUI.Label(new Rect(l, t, w/2 - s/2, h/2), "Score: ");

		string scoreStr = GUI.TextField(new Rect(l + (w/2 + s/2), t, w/2 - s/2, h/2), mScore.ToString());
		int.TryParse(scoreStr, out mScore);

		t += h + s;
		if (GUI.Button(new Rect(l, t, w, h), "PostToLeaderboard")){
			if (PlayGamesPlatform.Instance.IsAuthenticated() && mScore > mHighestPostedScore){
				PlayGamesPlatform.Instance.ReportScore(mScore, mLeaderboardId, (bool success)=>{
					headerMessage = string.Format("PostScore: {0}", success);
					if (success) mHighestPostedScore = mScore;
				});
			}
		}
	}
	#endregion


	
	#region Connecting & Room
	public Texture BGTexture, FGTexture;
	string connectStatus  = "....";
	float connectProgress = 0f;
	string mPId = null;

	Dictionary<string, Participant> JoinedParticipants = new Dictionary<string, Participant>();
	Dictionary<string, Vector2> ParticipantPositions = new Dictionary<string, Vector2>();
	HashSet<string> FinishedParticipants = new HashSet<string>();
	Dictionary<string, string> ParticipantStatus = new Dictionary<string, string>();
	Dictionary<string, float>  ParticipantTime = new Dictionary<string, float>();


	void StartConnect(string msg){
		prevType = contentType;
		connectStatus = msg;
		connectProgress = 0f;
		mPId = string.Empty;

		mRoomSetupStartTime = Time.time;
		JoinedParticipants.Clear();
		ParticipantPositions.Clear();
		FinishedParticipants.Clear();
		ParticipantStatus.Clear();
		ParticipantTime.Clear();
		contentType = ContentTypes.Connecting;
	}
	
	
	void FinishConnect(bool success) {
		connectStatus = string.Format("Setup Finished");
		connectProgress = 100f;
		mPId = PlayGamesPlatform.Instance.RealTime.GetSelf().ParticipantId;


		JoinedParticipants.Clear();
		List<Participant> participants = PlayGamesPlatform.Instance.RealTime.GetConnectedParticipants();
		foreach(Participant p in participants){
			if (!JoinedParticipants.ContainsKey(p.ParticipantId)){
				JoinedParticipants.Add(p.ParticipantId, p);
			}
			ParticipantStatus[p.ParticipantId]    = string.Format("Connected");
			ParticipantTime[p.ParticipantId]      = 0f;


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
		int t = T, w = Screen.width, h = 80, s = 10;

		// draw status
		GUI.Label(new Rect(20, t, w, h/2), connectStatus);
		t += h/2 + s;

		// draw setup progress
		Rect prect = new Rect(20, t, Screen.width - 40, 40);
		GUI.DrawTexture(prect, BGTexture);
		prect.width =  prect.width * RoomSetupProgress * 0.01f;
		GUI.DrawTexture(prect, FGTexture);

		// draw abort button
		if (GUI.Button(new Rect(Screen.width - 90, Screen.height - 90, 80, 60), "Abort")){
			FinishConnect(false);
			PlayGamesPlatform.Instance.RealTime.LeaveRoom();
		}
	}

	byte[] mPosPacket = new byte[3];
	public void BroadCastPosition(Vector2 pos) {
		mPosPacket[0] = (byte)'I'; // interim update
		mPosPacket[1] = (byte)pos.x;
		mPosPacket[2] = (byte)pos.y;

		PlayGamesPlatform.Instance.RealTime.SendMessageToAll(false, mPosPacket);
	}
	
	byte[] mFinalPacket = new byte[3];
	public void FinishRace() {
		FinishedParticipants.Add(mPId);

		// send final score packet to peers
		mFinalPacket[0] = (byte)'F'; // final update
		mFinalPacket[1] = (byte)ParticipantPositions[mPId].x;
		mFinalPacket[2] = (byte)ParticipantPositions[mPId].y;

		PlayGamesPlatform.Instance.RealTime.SendMessageToAll(true, mFinalPacket);
	}

	public void OnRealTimeMessageReceived(bool isReliable, string senderId, byte[] data) {
		
		Vector2 pos = new Vector2((int)data[1], (int)data[2]);


		if (data[0] == (byte)'I'){
			ParticipantPositions[senderId] = pos;

		}
		else if (data[0] == (byte)'F'){
			if (!FinishedParticipants.Contains(senderId)){
				ParticipantPositions[senderId] = pos;
				FinishedParticipants.Add(senderId);
			}	
		}
	}

	#endregion


	#region Realtime

	const int MinOpponents = 1, MaxOpponents = 3;
	const int GameVariant = 0;
	public RTMPGame mRtmp;


	public static RTMPGame RTMPGameInstance {
		get {
			return Instance.mRtmp;
		}
	}

	void RealTimeHUD() {
		int t = T, l = 0, w = Screen.width, h = 60, s = 10;

		// ================== Handler Invitation ==========================

		if (GUI.Button(new Rect(l, t, w, h), "Register Invitation")){
			PlayGamesPlatform.Instance.RegisterInvitationDelegate(OnInvitationReceived);
		}

		t += h + s;
		if (GUI.Button(new Rect(l, t, w, h), "Decline Invitation")){
			PlayGamesPlatform.Instance.RealTime.DeclineInvitation(mIncomingInvitation.InvitationId);
			mIncomingInvitation = null;
		}

		// ================== Create or Join a Room ==========================

		t += h + s;
		if (GUI.Button(new Rect(l, t, w, h), "Quick Match")){
			//StartConnect(string.Format("Please wait for connect"));
			mRtmp = new RTMPGame();
			PlayGamesPlatform.Instance.RealTime.CreateQuickGame(MinOpponents, MaxOpponents, GameVariant, mRtmp);
		}

		t += h + s;
		if (GUI.Button(new Rect(l, t, w, h), "Invite Friends")){
			//StartConnect(string.Format("Please wait for connect"));
			mRtmp = new RTMPGame();
			PlayGamesPlatform.Instance.RealTime.CreateWithInvitationScreen(MinOpponents, MaxOpponents, GameVariant, mRtmp);
		}

		
		t += h + s;
		if (GUI.Button(new Rect(l, t, w, h), "Accept From Inbox")){
			//StartConnect(string.Format("Please wait for connect"));
			mRtmp = new RTMPGame();
			PlayGamesPlatform.Instance.RealTime.AcceptFromInbox(mRtmp);
		}

		t += h + s;
		if (mIncomingInvitation != null){
			string inviter = (mIncomingInvitation.Inviter != null && mIncomingInvitation.Inviter.DisplayName != null) 
												      ? mIncomingInvitation.Inviter.DisplayName: "Someone";
			GUI.Label(new Rect(l, t, w, h), string.Format("Recevied Inv: {0}: {1}", mIncomingInvitation.InvitationId, inviter));
		}
		else {
			GUI.Label(new Rect(l, t, w, h), string.Format("Empty Invitation"));
		}


		t += h + s;
		if (GUI.Button(new Rect(l, t, w, h), "Accept With Invitation")){
			if (mIncomingInvitation != null) {
				//StartConnect(string.Format("Please wait for connect"));
				mRtmp = new RTMPGame();
				PlayGamesPlatform.Instance.RealTime.AcceptInvitation(mIncomingInvitation.InvitationId, mRtmp);
			}
		}

		// Wait for Connection.................

		// ================== Room Action =========================
		
		t += h + s;
		if (GUI.Button(new Rect(l, t, w, h), "Leave the Room")){
			mRtmp = null;
			PlayGamesPlatform.Instance.RealTime.LeaveRoom();
		}

		t += h + s;
		if (GUI.Button(new Rect(l, t, w, h), "Abord")){
			FinishConnect(false);
			mRtmp = null;
			PlayGamesPlatform.Instance.RealTime.LeaveRoom();
		}

	}

	Invitation mIncomingInvitation = null;
	// called when an invitation is received:
	public void OnInvitationReceived(Invitation invitation, bool shouldAutoAccept) {
		if (shouldAutoAccept) {
			// Invitation should be accepted immediately. This happens if the user already
			// indicated (through the notification UI) that they wish to accept the invitation,
			// so we should not prompt again.
			//StartConnect(string.Format("Please wait for connect"));
			mRtmp = new RTMPGame();
			PlayGamesPlatform.Instance.RealTime.AcceptInvitation(mIncomingInvitation.InvitationId, mRtmp);
		} else {
			// The user has not yet indicated that they want to accept this invitation.
			// We should *not* automatically accept it. Rather we store it and 
			// display an in-game popup:
			mIncomingInvitation = invitation;
		}
	}

	// Handler setup room progresss
	public void OnRoomSetupProgress(float progress) {
		// update progress bar
		// (progress goes from 0.0 to 100.0)
		connectProgress = progress;
	}

	// Handler setup room finished
	public void OnRoomConnected(bool success) {
		headerMessage = string.Format("Connceted Room {0}", success);
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
		
		// (do NOT call PlayGamesPlatform.Instance.RealTime.LeaveRoom() here --
		// you have already left the room!)
	}

	public void OnPeersConnected(string[] participantIds) {
		// react appropriately (e.g. add new avatars to the game)
		foreach(string pId in participantIds){
			if (!JoinedParticipants.ContainsKey(pId)){
				Participant p = PlayGamesPlatform.Instance.RealTime.GetParticipant(pId);
				if (p!= null) {
					JoinedParticipants.Add(pId, p);
				}
			}
		}
	}
	
	public void OnPeersDisconnected(string[] participantIds) {
		// react appropriately (e.g. remove avatars from the game)
		foreach(string pId in participantIds){
			if (JoinedParticipants.ContainsKey(pId)){
				JoinedParticipants.Remove(pId);
			}
		}
	}

	public void ConnectionAbord() {
		mRtmp = null;
		enabled = true;
		Application.LoadLevel(0);
	}
	#endregion
}
