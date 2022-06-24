using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Player : NetworkBehaviour {
	internal static readonly List<Player> playersList = new List<Player>();
	public int Type;
	public int Health;
	public GameObject[] HealthGos;
	public List<Vector3> Vector3Vars;
	public GameObject PointPrefab;
	int pointsCount;

	[SyncVar(hook = nameof(SyncHealth))]
	int _SyncHealth;
	
	[SyncVar(hook = nameof(SyncType))]
	int _SyncType;


	SyncList<Vector3> _SyncVector3Vars = new SyncList<Vector3>();

	void SyncHealth(int oldValue, int newValue) {
		Health = newValue;
	}
	
	void SyncType(int oldValue, int newValue) {
		Type = newValue;
	}

	[Server]
	public void ChangeHealthValue(int newValue) {
		_SyncHealth = newValue;
	}

	[Command]
	public void CmdChangeHealth(int newValue) {
		ChangeHealthValue(newValue);
	}

	[Server]
	void ChangeVector3Vars(Vector3 newValue) {
		_SyncVector3Vars.Add(newValue);
	}

	[Command]
	public void CmdChangeVector3Vars(Vector3 newValue) {
		ChangeVector3Vars(newValue);
	}

	void SyncVector3Vars(SyncList<Vector3>.Operation op, int index, Vector3 oldItem, Vector3 newItem) {
		switch (op) {
			case SyncList<Vector3>.Operation.OP_ADD:
				Vector3Vars.Add(newItem);
				break;
			case SyncList<Vector3>.Operation.OP_CLEAR:
				break;
			case SyncList<Vector3>.Operation.OP_INSERT:
				break;
			case SyncList<Vector3>.Operation.OP_REMOVEAT:
				break;
			case SyncList<Vector3>.Operation.OP_SET:
				break;
		}
	}

	public override void OnStartServer() {
		base.OnStartServer();
		playersList.Add(this);
		ResetPlayerNumbers();
	}

	public override void OnStartClient() {
		base.OnStartClient();

		_SyncVector3Vars.Callback += SyncVector3Vars;

		Vector3Vars = new List<Vector3>(_SyncVector3Vars.Count);
		for (int i = 0; i < _SyncVector3Vars.Count; i++) {
			Vector3Vars.Add(_SyncVector3Vars[i]);
		}
	}

	[ServerCallback]
	internal static void ResetPlayerNumbers() {
		byte playerNumber = 0;
		foreach (Player player in playersList)
			player._SyncType = playerNumber++;
	}

	void Update() {
		if (isLocalPlayer) {
			float h = Input.GetAxis("Horizontal");
			float v = Input.GetAxis("Vertical");
			float speed = 5f * Time.deltaTime;
			transform.Translate(new Vector2(h * speed, v * speed));
			if (Input.GetKeyDown(KeyCode.H)) {
				if (isServer)
					ChangeHealthValue(Health - 1);
				else
					CmdChangeHealth(Health - 1);
			}

			if (Input.GetKeyDown(KeyCode.P)) {
				if (isServer)
					ChangeVector3Vars(transform.position);
				else
					CmdChangeVector3Vars(transform.position);
			}

			if (Input.GetKeyDown(KeyCode.Z)) {
				if (isServer)
					Debug.Log(Type);
				else
					Debug.LogError(Type);
			}
		}

		for (int i = 0; i < HealthGos.Length; i++) {
			HealthGos[i].SetActive(!(Health - 1 < i));
		}

		for (int i = pointsCount; i < Vector3Vars.Count; i++) {
			Instantiate(PointPrefab, Vector3Vars[i], Quaternion.identity).GetComponent<SpriteRenderer>().color = Type == 0 ? Color.blue : Color.red;
			pointsCount++;
		}
	}
}