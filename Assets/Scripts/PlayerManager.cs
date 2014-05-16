using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GooglePlayGames;
using GooglePlayGames.BasicApi.Multiplayer;

public class PlayerManager : MonoBehaviour {
	public static PlayerManager Instance;


	public Transform[] PlayerHolders;
	public GameObject MyPlayerPrefab;
	public GameObject OtherPlayerPrefab;
	private string MyParticipantId;

	void Awake() {
		Instance = this;
	}

	public void LoadPlayers(){
		RTMPManager.RTMPGameInstance.RegisterSetPlayerDataCallback(OnSetPlayerDataReceived);
		MyParticipantId = PlayGamesPlatform.Instance.RealTime.GetSelf().ParticipantId;
		List<Participant> participants = PlayGamesPlatform.Instance.RealTime.GetConnectedParticipants();
		foreach(Participant p in participants){
			if (p.ParticipantId.Equals(MyParticipantId)) {
				int index = Random.Range(0, PlayerHolders.Length);
				Vector3 position = PlayerHolders[index].position;
				GameObject MyPlayerParent = GameObject.Instantiate(MyPlayerPrefab) as GameObject;
				MyPlayerParent.transform.localScale = Vector3.one;
				MyPlayerParent.transform.position = new Vector3(0, 0, 10);

				GameObject MyPlayer = GameObject.FindGameObjectWithTag("Player");
				if (MyPlayer != null)MyPlayer.transform.position = position;
				

				byte[] setupPositionPackage = new byte[3];
				setupPositionPackage[0] = (byte)'S';
				setupPositionPackage[1] = (byte)position.x;
				setupPositionPackage[2] = (byte)position.z;
				GooglePlayGames.PlayGamesPlatform.Instance.RealTime.SendMessageToAll(false, setupPositionPackage);
				break;
			}
		}

	}

	/// <summary>
	/// Raises the set player data received event.
	/// </summary>
	/// <param name="pId">P identifier.</param>
	/// <param name="data">
	/// 
	/// </param>
	void OnSetPlayerDataReceived(string pId, byte[] data){
		float x = (float)data[1];
		float z = (float)data[2];
		Vector3 position = new Vector3(x, 5, z);
		List<Participant> participants = PlayGamesPlatform.Instance.RealTime.GetConnectedParticipants();
		foreach(Participant p in participants){
			if (p.ParticipantId.Equals(pId)) {
				GameObject OtherPlayer = GameObject.Instantiate(OtherPlayerPrefab) as GameObject;
				OtherPlayer.transform.localScale = Vector3.one;
				OtherPlayer.transform.position = position;
				break;
			}
		}
		BroadcastMessage("DestroyMenu");
	}
}
