class TIMERclock {
    public int main = 0, sub = 0, div = 0;
}

class TIMER 
{
  private byte _div = 0;
  private byte _tma = 0;
  private byte _tima = 0;
  private byte _tac = 0;

  private TIMERclock _clock = new TIMERclock();

  MMU mMU;
  Z80 z80;
  
  public void reset(MMU mMU, Z80 z80) {
    this.mMU = mMU;
    this.z80 = z80;
    _div = 0;
    _tma = 0;
    _tima = 0;
    _tac = 0;
    _clock.main = 0;
    _clock.sub = 0;
    _clock.div = 0;
//    Echo("TIMER: Reset.");
  }

  public void step() {
    _tima++;
    _clock.main = 0;
    if(_tima > 255)
    {
      _tima = _tma;
      mMU._if |= 4;
    }
  }

  public void inc() {
    var oldclk = _clock.main;

    _clock.sub += z80.r.m;
    if(_clock.sub > 3)
    {
      _clock.main++;
      _clock.sub -= 4;

      _clock.div++;
      if(_clock.div==16)
      {
        _clock.div = 0;
	_div++;
	_div &= 255;
      }
    }

    if((_tac & 4)>0)
    {
      switch(_tac & 3)
      {
        case 0:
	  if(_clock.main >= 64) step();
	  break;
	case 1:
	  if(_clock.main >=  1) step();
	  break;
	case 2:
	  if(_clock.main >=  4) step();
	  break;
	case 3:
	  if(_clock.main >= 16) step();
	  break;
      }
    }
  }

  public byte rb(int addr) {
    switch(addr)
    {
      case 0xFF04: return _div;
      case 0xFF05: return _tima;
      case 0xFF06: return _tma;
      case 0xFF07: return _tac;
    }
    return 0x00;
  }

  public void wb(int addr, byte val) {
    switch(addr)
    {
      case 0xFF04: _div = 0x00; break;
      case 0xFF05: _tima = val; break;
      case 0xFF06: _tma = val; break;
      case 0xFF07: _tac = (byte)(val&7); break;
    }
  }
}
