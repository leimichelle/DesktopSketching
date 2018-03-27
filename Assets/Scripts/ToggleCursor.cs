using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleCursor : MonoBehaviour {
	public Texture2D curTexture;
	private enum Mode{Drawing, Erasing};
	private Mode mode = Mode.Drawing;
	// Use this for initialization
	public void ToggleCursorMode() {
		if(mode == Mode.Drawing){
			mode = Mode.Erasing;
			Cursor.SetCursor(curTexture, Vector2.zero, CursorMode.Auto);
		}
		else {
			mode = Mode.Drawing;
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
		}
	}
}
