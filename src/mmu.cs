class MMUmbc {
    public int rombank=0, rambank=0, ramon=0, mode=0;
}
class MMU 
{
  public byte[] _bios = Convert.FromBase64String("Mf7/ryH/nzLLfCD7ISb/DhE+gDLiDD7z4jI+d3c+/OBHEQQBIRCAGs2VAM2WABN7/jQg8xHYAAYIGhMiIwUg+T4Z6hCZIS+ZDgw9KAgyDSD5Lg8Y82c+ZFfgQj6R4EAEHgIODPBE/pAg+g0g9x0g8g4TJHweg/5iKAYewf5kIAZ74gw+h/LwQpDgQhUg0gUgTxYgGMtPBgTFyxEXwcsRFwUg9SIjIiPJzu1mZswNAAsDcwCDAAwADQAIER+IiQAO3Mxu5t3d2Zm7u2djbg7szN3cmZ+7uTM+PEK5pbmlQkwhBAERqAAaE74g/iN9/jQg9QYZeIYjBSD7hiD+PgHgUA==");
  byte[] _rom;
  int _carttype = 0;
  MMUmbc _mbc0 = new MMUmbc();
  MMUmbc _mbc1 = new MMUmbc();
  int _romoffs = 0x4000;
  int _ramoffs = 0;

  byte[] _wram = new byte[8192];
  byte[] _eram = new byte[32768];
  byte[] _zram = new byte[127];

  public byte _inbios = 1;
  public byte _ie = 0;
  public byte _if = 0;
  
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

    //SpaceGameboy.Echo("Clearing wram");
    for(i=0; i<8192; i++) _wram[i] = 0;
    //SpaceGameboy.Echo("Clearing eram");
    for(i=0; i<32768; i++) _eram[i] = 0;
    //SpaceGameboy.Echo("Clearing zram");
    for(i=0; i<127; i++) _zram[i] = 0;

    _inbios=1;
    _ie=0;
    _if=0;

    _carttype=0;
    _mbc0 = new MMUmbc();
    _mbc1 = new MMUmbc();
    _romoffs=0x4000;
    _ramoffs=0;

//    Echo("MMU: Reset.");
  }

  public void load(byte[] rom) {
    _rom=rom;
    _carttype = _rom[0x0147];

//    Echo("MMU: ROM loaded, "+_rom.Length+" bytes.");
  }

  public byte rb(int addr) {
    int v = addr&0xF000;   
    if(v == 0x0000)
    {
      // ROM bank 0
        if(_inbios > 0)
        {
          if(addr<0x0100) return _bios[addr];
          else if(z80.r.pc == 0x0100)
          {
            _inbios = 0;
    //	    Echo("MMU: Leaving BIOS.");
          }
        }
        else
        {
          return _rom[addr];
        }
        return _rom[addr];
    }
    else if(v <= 0x3000)
    {
        return _rom[addr];
    }
    else if(v <= 0x7000)
    {
      // ROM bank 1
        return _rom[_romoffs+(addr&0x3FFF)];
    }
    else if(v <= 0x9000)
    {
      // VRAM
        return gPU._vram[addr&0x1FFF];
    }
    else if(v <= 0xB000)
    {
      // External RAM
        return _eram[_ramoffs+(addr&0x1FFF)];
    }
    else if(v <= 0xE000)
    {
      // Work RAM and echo
        return _wram[addr&0x1FFF];
    }
    else if(v <= 0xF000)
    {
        int w = addr&0x0F00;
      // Everything else
        if(w <= 0xD00)
        {
          // Echo RAM
            return _wram[addr&0x1FFF];
        }
        else if(w <= 0xE00)
        {
              // OAM
            return (byte)(((addr&0xFF)<0xA0) ? gPU._oam[addr&0xFF] : 0x00);
        }
        else if(w <= 0xF00)
        {
              // Zeropage RAM, I/O, interrupts
            if(addr == 0xFFFF) { return _ie; }
            else if(addr > 0xFF7F) { return _zram[addr&0x7F]; }
            else switch(addr&0xF0)
            {
              case 0x00:
                switch(addr&0xF)
                {
                  case 0: return kEY.rb();    // JOYP
                  case 4: case 5: case 6: case 7:
                    return tIMER.rb(addr);
                  case 15: return _if;    // Interrupt flags
                  default: return 0x00;
                }
              case 0x10: case 0x20: case 0x30:
                return 0x00;

              case 0x40: case 0x50: case 0x60: case 0x70:
                return gPU.rb(addr);
            }
        }
    }
    //throw new Exception("Shouldn't have made it here");
    return 0x00;
  }

  public int rw(int addr) { return rb(addr)+(rb(addr+1)<<8); }

  public void wb(int addr, byte val) {
    int v = addr&0xF000;
    if(v == 0x0000 || v == 0x1000)
    {
      // ROM bank 0
      // MBC1: Turn external RAM on
        switch(_carttype)
        {
          case 1:
            _mbc1.ramon = ((val&0xF)==0xA)?1:0;
            break;
        }
    }
    else if(v == 0x2000 || v == 0x3000)
    {
      // MBC1: ROM bank switch
        switch(_carttype)
        {
          case 1:
            _mbc1.rombank &= 0x60;
            val &= 0x1F;
            if(!(val>0)) val=1;
            _mbc1.rombank |= val;
            _romoffs = _mbc1.rombank * 0x4000;
            break;
        }

    }
    else if(v == 0x4000 || v == 0x5000)
    {
      // ROM bank 1
      // MBC1: RAM bank switch
        switch(_carttype)
        {
          case 1:
            if(_mbc1.mode > 0)
            {
              _mbc1.rambank = (val&3);
              _ramoffs = _mbc1.rambank * 0x2000;
            }
            else
            {
              _mbc1.rombank &= 0x1F;
              _mbc1.rombank |= ((val&3)<<5);
              _romoffs = _mbc1.rombank * 0x4000;
            }
            break;
        }
    }
    else if(v == 0x6000 || v == 0x7000)
    {
        switch(_carttype)
        {
          case 1:
            _mbc1.mode = val&1;
            break;
        }
    }
    else if(v == 0x8000 || v == 0x9000) // VRAM
    {
        gPU._vram[addr&0x1FFF] = val;
        gPU.updatetile(addr&0x1FFF, val);
    }
    else if(v == 0xA000 || v == 0xB000) // External RAM
    {
        _eram[_ramoffs+(addr&0x1FFF)] = val;
    }
    else if(v == 0xC000 || v == 0xD000 || v == 0xE000) // Work RAM and echo
    {
        _wram[addr&0x1FFF] = val;
    }
    else if(v == 0xF000) // Everything else
    {
        var w = addr&0x0F00;
        if(w <= 0xD00)
        {
          // Echo RAM
            _wram[addr&0x1FFF] = val;
        }
        else if(w == 0xE00)
        {
              // OAM
            if((addr&0xFF)<0xA0) gPU._oam[addr&0xFF] = val;
            gPU.updateoam(addr,val);
        }
        else if(w == 0xF00)
        {
              // Zeropage RAM, I/O, interrupts
            if(addr == 0xFFFF) { _ie = val; }
            else if(addr > 0xFF7F) { _zram[addr&0x7F]=val; }
            else switch(addr&0xF0)
            {
              case 0x00:
                switch(addr&0xF)
                {
                  case 0: kEY.wb(val); break;
                  case 4: case 5: case 6: case 7: tIMER.wb(addr, val); break;
                  case 15: _if = val; break;
                }
                break;

              case 0x10: case 0x20: case 0x30:
                break;

              case 0x40: case 0x50: case 0x60: case 0x70:
                gPU.wb(addr,val);
                break;
            }
        }
    }
  }

  public void ww(int addr,int val) { wb(addr,(byte)(val&0xFF)); wb(addr+1,(byte)(val>>8)); }
}
