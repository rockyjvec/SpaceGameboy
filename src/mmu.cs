class MMUmbc {
    public int rombank=0, rambank=0, ramon=0, mode=0;
}
class MMU 
{
  private char[] _bios = new char[]{
    (char)0x31, (char)0xFE, (char)0xFF, (char)0xAF, (char)0x21, (char)0xFF, (char)0x9F, (char)0x32, (char)0xCB, (char)0x7C, (char)0x20, (char)0xFB, (char)0x21, (char)0x26, (char)0xFF, (char)0x0E,
    (char)0x11, (char)0x3E, (char)0x80, (char)0x32, (char)0xE2, (char)0x0C, (char)0x3E, (char)0xF3, (char)0xE2, (char)0x32, (char)0x3E, (char)0x77, (char)0x77, (char)0x3E, (char)0xFC, (char)0xE0,
    (char)0x47, (char)0x11, (char)0x04, (char)0x01, (char)0x21, (char)0x10, (char)0x80, (char)0x1A, (char)0xCD, (char)0x95, (char)0x00, (char)0xCD, (char)0x96, (char)0x00, (char)0x13, (char)0x7B,
    (char)0xFE, (char)0x34, (char)0x20, (char)0xF3, (char)0x11, (char)0xD8, (char)0x00, (char)0x06, (char)0x08, (char)0x1A, (char)0x13, (char)0x22, (char)0x23, (char)0x05, (char)0x20, (char)0xF9,
    (char)0x3E, (char)0x19, (char)0xEA, (char)0x10, (char)0x99, (char)0x21, (char)0x2F, (char)0x99, (char)0x0E, (char)0x0C, (char)0x3D, (char)0x28, (char)0x08, (char)0x32, (char)0x0D, (char)0x20,
    (char)0xF9, (char)0x2E, (char)0x0F, (char)0x18, (char)0xF3, (char)0x67, (char)0x3E, (char)0x64, (char)0x57, (char)0xE0, (char)0x42, (char)0x3E, (char)0x91, (char)0xE0, (char)0x40, (char)0x04,
    (char)0x1E, (char)0x02, (char)0x0E, (char)0x0C, (char)0xF0, (char)0x44, (char)0xFE, (char)0x90, (char)0x20, (char)0xFA, (char)0x0D, (char)0x20, (char)0xF7, (char)0x1D, (char)0x20, (char)0xF2,
    (char)0x0E, (char)0x13, (char)0x24, (char)0x7C, (char)0x1E, (char)0x83, (char)0xFE, (char)0x62, (char)0x28, (char)0x06, (char)0x1E, (char)0xC1, (char)0xFE, (char)0x64, (char)0x20, (char)0x06,
    (char)0x7B, (char)0xE2, (char)0x0C, (char)0x3E, (char)0x87, (char)0xF2, (char)0xF0, (char)0x42, (char)0x90, (char)0xE0, (char)0x42, (char)0x15, (char)0x20, (char)0xD2, (char)0x05, (char)0x20,
    (char)0x4F, (char)0x16, (char)0x20, (char)0x18, (char)0xCB, (char)0x4F, (char)0x06, (char)0x04, (char)0xC5, (char)0xCB, (char)0x11, (char)0x17, (char)0xC1, (char)0xCB, (char)0x11, (char)0x17,
    (char)0x05, (char)0x20, (char)0xF5, (char)0x22, (char)0x23, (char)0x22, (char)0x23, (char)0xC9, (char)0xCE, (char)0xED, (char)0x66, (char)0x66, (char)0xCC, (char)0x0D, (char)0x00, (char)0x0B,
    (char)0x03, (char)0x73, (char)0x00, (char)0x83, (char)0x00, (char)0x0C, (char)0x00, (char)0x0D, (char)0x00, (char)0x08, (char)0x11, (char)0x1F, (char)0x88, (char)0x89, (char)0x00, (char)0x0E,
    (char)0xDC, (char)0xCC, (char)0x6E, (char)0xE6, (char)0xDD, (char)0xDD, (char)0xD9, (char)0x99, (char)0xBB, (char)0xBB, (char)0x67, (char)0x63, (char)0x6E, (char)0x0E, (char)0xEC, (char)0xCC,
    (char)0xDD, (char)0xDC, (char)0x99, (char)0x9F, (char)0xBB, (char)0xB9, (char)0x33, (char)0x3E, (char)0x3c, (char)0x42, (char)0xB9, (char)0xA5, (char)0xB9, (char)0xA5, (char)0x42, (char)0x4C,
    (char)0x21, (char)0x04, (char)0x01, (char)0x11, (char)0xA8, (char)0x00, (char)0x1A, (char)0x13, (char)0xBE, (char)0x20, (char)0xFE, (char)0x23, (char)0x7D, (char)0xFE, (char)0x34, (char)0x20,
    (char)0xF5, (char)0x06, (char)0x19, (char)0x78, (char)0x86, (char)0x23, (char)0x05, (char)0x20, (char)0xFB, (char)0x86, (char)0x20, (char)0xFE, (char)0x3E, (char)0x01, (char)0xE0, (char)0x50
  };
  private string _rom = "";
  private int _carttype = 0;
  private MMUmbc _mbc0 = new MMUmbc();
  private MMUmbc _mbc1 = new MMUmbc();
  int _romoffs = 0x4000;
  int _ramoffs = 0;

  char[] _eram = new char[8192];
  char[] _wram = new char[32768];
  char[] _zram = new char[127];

  public int _inbios = 1;
  public int _ie = 0;
  public int _if = 0;
  
  GPU gPU;
  TIMER tIMER;
  KEY kEY;
  Z80 z80;
  
  public void reset(GPU gPU, TIMER tIMER, KEY kEY, Z80 z80) {

    this.gPU = gPU;
    this.tIMER = tIMER;
    this.kEY = kEY;
    this.z80 = z80;

    int i;

    for(i=0; i<8192; i++) this._wram[i] = (char)0;
    for(i=0; i<32768; i++) this._eram[i] = (char)0;
    for(i=0; i<127; i++) this._zram[i] = (char)0;

    this._inbios=1;
    this._ie=0;
    this._if=0;

    this._carttype=0;
    this._mbc0 = new MMUmbc();
    this._mbc1 = new MMUmbc();
    this._romoffs=0x4000;
    this._ramoffs=0;

//    Echo("MMU: Reset.");
  }

  public void load(string file) {
    this._rom=file;
    this._carttype = this._rom[0x0147];

//    Echo("MMU: ROM loaded, "+this._rom.Length+" bytes.");
  }

  public int rb(int addr) {
    switch(addr&0xF000)
    {
      // ROM bank 0
      case 0x0000:
        if(this._inbios > 0)
        {
          if(addr<0x0100) return this._bios[addr];
          else if(z80._r.pc == 0x0100)
          {
            this._inbios = 0;
    //	    Echo("MMU: Leaving BIOS.");
          }
        }
        else
        {
          return this._rom[addr];
        }
        return this._rom[addr];

      case 0x1000:
      case 0x2000:
      case 0x3000:
        return this._rom[addr];

      // ROM bank 1
      case 0x4000: case 0x5000: case 0x6000: case 0x7000:
        return this._rom[this._romoffs+(addr&0x3FFF)];

      // VRAM
      case 0x8000: case 0x9000:
        return gPU._vram[addr&0x1FFF];

      // External RAM
      case 0xA000: case 0xB000:
        return this._eram[this._ramoffs+(addr&0x1FFF)];

      // Work RAM and echo
      case 0xC000: case 0xD000: case 0xE000:
        return this._wram[addr&0x1FFF];

      // Everything else
      case 0xF000:
        switch(addr&0x0F00)
        {
          // Echo RAM
          case 0x000: case 0x100: case 0x200: case 0x300:
          case 0x400: case 0x500: case 0x600: case 0x700:
          case 0x800: case 0x900: case 0xA00: case 0xB00:
          case 0xC00: case 0xD00:
            return this._wram[addr&0x1FFF];

              // OAM
          case 0xE00:
            return ((addr&0xFF)<0xA0) ? gPU._oam[addr&0xFF] : 0;

              // Zeropage RAM, I/O, interrupts
          case 0xF00:
            if(addr == 0xFFFF) { return this._ie; }
            else if(addr > 0xFF7F) { return this._zram[addr&0x7F]; }
            else switch(addr&0xF0)
            {
              case 0x00:
                switch(addr&0xF)
                {
                  case 0: return kEY.rb();    // JOYP
                  case 4: case 5: case 6: case 7:
                    return tIMER.rb(addr);
                  case 15: return this._if;    // Interrupt flags
                  default: return 0;
                }
              case 0x10: case 0x20: case 0x30:
                return 0;

              case 0x40: case 0x50: case 0x60: case 0x70:
                return gPU.rb(addr);
            }
            break;
        }
        break;
    }
    throw new Exception("Shouldn't have made it here");
    return 0;
  }

  public int rw(int addr) { return this.rb(addr)+(this.rb(addr+1)<<8); }

  public void wb(int addr, int val) {
    switch(addr&0xF000)
    {
      // ROM bank 0
      // MBC1: Turn external RAM on
      case 0x0000: case 0x1000:
        switch(this._carttype)
	{
	  case 1:
	    this._mbc1.ramon = ((val&0xF)==0xA)?1:0;
	    break;
	}
	break;

      // MBC1: ROM bank switch
      case 0x2000: case 0x3000:
        switch(this._carttype)
	{
	  case 1:
	    this._mbc1.rombank &= 0x60;
	    val &= 0x1F;
	    if(!(val>0)) val=1;
	    this._mbc1.rombank |= val;
	    this._romoffs = this._mbc1.rombank * 0x4000;
	    break;
	}
        break;

      // ROM bank 1
      // MBC1: RAM bank switch
      case 0x4000: case 0x5000:
        switch(this._carttype)
        {
          case 1:
            if(this._mbc1.mode > 0)
            {
              this._mbc1.rambank = (val&3);
              this._ramoffs = this._mbc1.rambank * 0x2000;
            }
            else
            {
              this._mbc1.rombank &= 0x1F;
              this._mbc1.rombank |= ((val&3)<<5);
              this._romoffs = this._mbc1.rombank * 0x4000;
            }
            break;
        }
        break;

      case 0x6000: case 0x7000:
        switch(this._carttype)
        {
          case 1:
            this._mbc1.mode = val&1;
            break;
        }
        break;

      // VRAM
      case 0x8000: case 0x9000:
        gPU._vram[addr&0x1FFF] = (char)val;
        gPU.updatetile(addr&0x1FFF, val);
        break;

      // External RAM
      case 0xA000: case 0xB000:
        this._eram[this._ramoffs+(addr&0x1FFF)] = (char)val;
        break;

      // Work RAM and echo
      case 0xC000: case 0xD000: case 0xE000:
        this._wram[addr&0x1FFF] = (char)val;
        break;

      // Everything else
      case 0xF000:
        switch(addr&0x0F00)
        {
          // Echo RAM
          case 0x000: case 0x100: case 0x200: case 0x300:
          case 0x400: case 0x500: case 0x600: case 0x700:
          case 0x800: case 0x900: case 0xA00: case 0xB00:
          case 0xC00: case 0xD00:
            this._wram[addr&0x1FFF] = (char)val;
            break;

              // OAM
          case 0xE00:
            if((addr&0xFF)<0xA0) gPU._oam[addr&0xFF] = (char)val;
            gPU.updateoam(addr,val);
            break;

              // Zeropage RAM, I/O, interrupts
          case 0xF00:
            if(addr == 0xFFFF) { this._ie = val; }
            else if(addr > 0xFF7F) { this._zram[addr&0x7F]=(char)val; }
            else switch(addr&0xF0)
            {
              case 0x00:
                switch(addr&0xF)
                {
                  case 0: kEY.wb(val); break;
                  case 4: case 5: case 6: case 7: tIMER.wb(addr, val); break;
                  case 15: this._if = val; break;
                }
                break;

              case 0x10: case 0x20: case 0x30:
                break;

              case 0x40: case 0x50: case 0x60: case 0x70:
                gPU.wb(addr,val);
                break;
            }
            break;
        }
        break;
    }
  }

  public void ww(int addr,int val) { this.wb(addr,val&255); this.wb(addr+1,val>>8); }
}
