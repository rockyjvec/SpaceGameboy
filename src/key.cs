class KEY 
{
  char[] _keys = new char[2] {(char)0x0F,(char)0x0F};
  int _colidx = 0;

  public void reset() {
    this._keys = new char[2]{(char)0x0F,(char)0x0F};
    this._colidx = 0;
//    Echo("KEY: Reset.");
  }

  public int rb() {
    switch(this._colidx)
    {
      case 0x00: return 0x00; break;
      case 0x10: return this._keys[0]; break;
      case 0x20: return this._keys[1]; break;
      default: return 0x00; break;
    }
  }

  public void wb(int v) {
    this._colidx = v&0x30;
  }

  public void keydown(int keyCode) {
    switch(keyCode)
    {
      case 39: this._keys[1] &= (char)0xE; break;
      case 37: this._keys[1] &= (char)0xD; break;
      case 38: this._keys[1] &= (char)0xB; break;
      case 40: this._keys[1] &= (char)0x7; break;
      case 90: this._keys[0] &= (char)0xE; break;
      case 88: this._keys[0] &= (char)0xD; break;
      case 32: this._keys[0] &= (char)0xB; break;
      case 13: this._keys[0] &= (char)0x7; break;
    }
  }

  public void keyup(int keyCode) {
    switch(keyCode)
    {
      case 39: this._keys[1] |= (char)0x1; break;
      case 37: this._keys[1] |= (char)0x2; break;
      case 38: this._keys[1] |= (char)0x4; break;
      case 40: this._keys[1] |= (char)0x8; break;
      case 90: this._keys[0] |= (char)0x1; break;
      case 88: this._keys[0] |= (char)0x2; break;
      case 32: this._keys[0] |= (char)0x5; break;
      case 13: this._keys[0] |= (char)0x8; break;
    }
  }
};
