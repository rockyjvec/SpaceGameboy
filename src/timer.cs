class TIMER 
{
	private int _div = 0;
	private int _tma = 0;
	private int _tima = 0;
	private int _tac = 0;

	int main = 0, sub = 0, div = 0;

	MMU mMU;
	Z80 z80;
	int oldclk;

	public void reset(MMU mMU, Z80 z80) {
		this.mMU = mMU;
		this.z80 = z80;
		_div = 0;
		_tma = 0;
		_tima = 0;
		_tac = 0;
		main = 0;
		sub = 0;
		div = 0;
		//    Echo("TIMER: Reset.");
	}

	public void step() {
		_tima++;
		main = 0;
		if(_tima > 255)
		{
			_tima = _tma;
			mMU._if |= 4;
		}
	}

	public void inc() {
		oldclk = main;

		sub += z80.r.m;
		if(sub > 3)
		{
			main++;
			sub -= 4;

			div++;
			if(div==16)
			{
				div = 0;
				_div++;
				_div &= 255;
			}
		}

		if((_tac & 4)>0)
		{
			switch(_tac & 3)
			{
			case 0:
				if(main >= 64) step();
				break;
			case 1:
				if(main >=  1) step();
				break;
			case 2:
				if(main >=  4) step();
				break;
			case 3:
				if(main >= 16) step();
				break;
			}
		}
	}

	public int rb(int addr) {
		switch(addr)
		{
		case 0xFF04: return _div;
		case 0xFF05: return _tima;
		case 0xFF06: return _tma;
		case 0xFF07: return _tac;
		}
		return 0x00;
	}

	public void wb(int addr, int val) {
		switch(addr)
		{
		case 0xFF04: _div = 0x00; break;
		case 0xFF05: _tima = val; break;
		case 0xFF06: _tma = val; break;
		case 0xFF07: _tac = val&7; break;
		}
	}
}