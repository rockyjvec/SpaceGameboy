class KEY 
{
	int[] _keys = new int[2] {0x0F,0x0F};
	int _colidx = 0;

	public void reset() {
		_keys = new int[2]{0x0F,0x0F};
		_colidx = 0x00;
		//    Echo("KEY: Reset.");
	}

	public int rb() {
		switch(_colidx)
		{
		case 0x00: return 0x00;
		case 0x10: return _keys[0];
		case 0x20: return _keys[1];
		default: return 0x00;
		}
	}

	public void wb(int v) {
		_colidx = v&0x30;
	}

	public void keydown(string key) {
		switch(key)
		{
		case "right": _keys[1] &= 0xE; break; // right
		case "left": _keys[1] &= 0xD; break; // left
		case "up": _keys[1] &= 0xB; break; // up
		case "down": _keys[1] &= 0x7; break; // down
		case "a": _keys[0] &= 0xE; break; // z
		case "b": _keys[0] &= 0xD; break; // x
		case "select": _keys[0] &= 0xB; break; // space
		case "start": _keys[0] &= 0x7; break; // enter
		}
	}

	public void keyup(string key) {
		switch(key)
		{
		case "right": _keys[1] |= 0x1; break;
		case "left": _keys[1] |= 0x2; break;
		case "up": _keys[1] |= 0x4; break;
		case "down": _keys[1] |= 0x8; break;
		case "a": _keys[0] |= 0x1; break;
		case "b": _keys[0] |= 0x2; break;
		case "select": _keys[0] |= 0x5; break;
		case "start": _keys[0] |= 0x8; break;
		}
	}
}
