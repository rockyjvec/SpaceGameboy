class TIMERclock {
    public int main = 0, sub = 0, div = 0;
}

class TIMER 
{
  private int _div = 0;
  private int _tma = 0;
  private int _tima = 0;
  private int _tac = 0;

  private TIMERclock _clock = new TIMERclock();

  MMU mMU;
  Z80 z80;
  
  public void reset(MMU mMU, Z80 z80) {
    this.mMU = mMU;
    this.z80 = z80;
    this._div = 0;
    this._tma = 0;
    this._tima = 0;
    this._tac = 0;
    this._clock.main = 0;
    this._clock.sub = 0;
    this._clock.div = 0;
//    Echo("TIMER: Reset.");
  }

  public void step() {
    this._tima++;
    this._clock.main = 0;
    if(this._tima > 255)
    {
      this._tima = this._tma;
      mMU._if |= 4;
    }
  }

  public void inc() {
    var oldclk = this._clock.main;

    this._clock.sub += z80._r.m;
    if(this._clock.sub > 3)
    {
      this._clock.main++;
      this._clock.sub -= 4;

      this._clock.div++;
      if(this._clock.div==16)
      {
        this._clock.div = 0;
	this._div++;
	this._div &= 255;
      }
    }

    if((this._tac & 4)>0)
    {
      switch(this._tac & 3)
      {
        case 0:
	  if(this._clock.main >= 64) this.step();
	  break;
	case 1:
	  if(this._clock.main >=  1) this.step();
	  break;
	case 2:
	  if(this._clock.main >=  4) this.step();
	  break;
	case 3:
	  if(this._clock.main >= 16) this.step();
	  break;
      }
    }
  }

  public int rb(int addr) {
    switch(addr)
    {
      case 0xFF04: return this._div;
      case 0xFF05: return this._tima;
      case 0xFF06: return this._tma;
      case 0xFF07: return this._tac;
    }
    return 0;
  }

  public void wb(int addr, int val) {
    switch(addr)
    {
      case 0xFF04: this._div = 0; break;
      case 0xFF05: this._tima = val; break;
      case 0xFF06: this._tma = val; break;
      case 0xFF07: this._tac = val&7; break;
    }
  }
}
