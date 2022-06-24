using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasUI : MonoBehaviour {
	public RectTransform mainPanel;
	public static CanvasUI instance;

	void Awake() {
		instance = this;
	}
}