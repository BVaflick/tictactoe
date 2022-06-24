using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class TicTacPlayer : NetworkBehaviour {
	internal static readonly List<TicTacPlayer> playersList = new List<TicTacPlayer>();
	public int Type;
	static public List<int> Vector3Vars;
	public RectTransform panel;

	[SyncVar(hook = nameof(SyncType))]
	int _SyncType;

	SyncList<int> _SyncVector3Vars = new SyncList<int> {0, 0, 0, 0, 0, 0, 0, 0, 0};

	void SyncType(int oldValue, int newValue) {
		Type = newValue;
	}

	[Server]
	void ChangeVector3Vars(int index, int value) {
		_SyncVector3Vars[index] = value;
	}

	[Command]
	public void CmdChangeVector3Vars(int index, int value) {
		ChangeVector3Vars(index, value);
	}

	void SyncVector3Vars(SyncList<int>.Operation op, int index, int oldItem, int newItem) {
		switch (op) {
			case SyncList<int>.Operation.OP_ADD:
				break;
			case SyncList<int>.Operation.OP_CLEAR:
				break;
			case SyncList<int>.Operation.OP_INSERT:
				break;
			case SyncList<int>.Operation.OP_REMOVEAT:
				break;
			case SyncList<int>.Operation.OP_SET:
				Vector3Vars[index] = newItem;
				break;
		}

		updateCells();
		checkWin();
	}

	void updateCells() {
		for (int i = 0; i < Vector3Vars.Count; i++) {
			CanvasUI.instance.mainPanel.GetChild(i).GetComponentInChildren<Text>().text =
				Vector3Vars[i] == 0 ? "" : Vector3Vars[i] == 1 ? "X" : "O";
		}
	}

	void checkWin() {
		checkLine(0,1,2);
		checkLine(3,4,5);
		checkLine(6,7,8);
		checkLine(0,3,6);
		checkLine(1,4,7);
		checkLine(2,5,8);
		checkLine(0,4,8);
		checkLine(2,4,6);
	}

	void checkLine(int a, int b, int c) {
		if (Vector3Vars[a] != 0 && Vector3Vars[b] != 0 && Vector3Vars[c] != 0 && Vector3Vars[a] == Vector3Vars[b] && Vector3Vars[a] == Vector3Vars[c]) {
			CanvasUI.instance.mainPanel.GetChild(a).GetComponent<Image>().color = Color.green;
			CanvasUI.instance.mainPanel.GetChild(b).GetComponent<Image>().color = Color.green;
			CanvasUI.instance.mainPanel.GetChild(c).GetComponent<Image>().color = Color.green;
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

		Vector3Vars = new List<int>(_SyncVector3Vars.Count);
		for (int i = 0; i < _SyncVector3Vars.Count; i++) {
			Vector3Vars.Add(_SyncVector3Vars[i]);
		}

		updateCells();

		for (int i = 0; i < Vector3Vars.Count; i++) {
			int index = i;
			CanvasUI.instance.mainPanel.GetChild(i).GetComponent<Button>().onClick.AddListener(() => {
				if(isLocalPlayer) CmdChangeVector3Vars(index, Type);
			});
		}
	}

	[ServerCallback]
	internal static void ResetPlayerNumbers() {
		byte playerNumber = 0;
		foreach (TicTacPlayer player in playersList)
			player._SyncType = 1 + playerNumber++;
	}

	void Update() {
		if (isLocalPlayer) {
			if (Input.GetKeyDown(KeyCode.Z)) {
				if (isServer)
					Debug.Log(Type);
				else
					Debug.LogError(Type);
			}

			if (Input.GetKeyDown(KeyCode.X)) {
				if (isServer)
					ChangeVector3Vars(0, 1);
				else
					CmdChangeVector3Vars(0, 1);
			}
		}
	}
}