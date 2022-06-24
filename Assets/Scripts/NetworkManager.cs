using Mirror;
using UnityEngine;

public class NetworkManager : Mirror.NetworkManager {
	NetworkConnection connection;
	bool playerConnected;
	bool playerSpawned;

	public void OnCreateCharacter(NetworkConnection conn, PosMessage message) {
		GameObject go = Instantiate(playerPrefab, message.Vector2, Quaternion.identity);
		NetworkServer.AddPlayerForConnection(conn as NetworkConnectionToClient, go);
	}

	public void ActivatePlayerSpawn() {
		Vector3 pos = Input.mousePosition;
		pos.z = 10f;
		pos = Camera.main.ScreenToWorldPoint(pos);

		PosMessage m = new PosMessage() {Vector2 = pos, type = numPlayers};
		connection.Send(m);
		playerSpawned = true;
	}

	private void Update() {
		if (Input.GetKeyDown(KeyCode.Mouse0) && !playerSpawned && playerConnected) {
			ActivatePlayerSpawn();
		}
	}

	public override void OnStartServer() {
		base.OnStartServer();
		NetworkServer.RegisterHandler<PosMessage>(OnCreateCharacter);
	}

	public override void OnClientConnect(NetworkConnection conn) {
		base.OnClientConnect(conn);
		connection = conn;
		playerConnected = true;
	}
}

public struct PosMessage : NetworkMessage {
	public Vector2 Vector2;
	public int type;
}