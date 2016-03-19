void Main(string argument)
{
    
}
class GPUData {
    public int y = -16, x = -8, tile = 0, palette = 0, yflip = 0, xflip = 0, prio = 0, num = 0;
    public GPUData(int num)
    {
        this.num = 0;
    }
}

class GPUPalette {
    public char[] bg = new char[4];
    public char[] obj0 = new char[4];
    public char[] obj1 = new char[4];
}

class GPUScreen {
    public int width = 160;
    public int height = 144;
    public char[] data = new char[160*144*4];
}

class GPU 
{
  public char[] _vram = new char[8192];
  public char[] _oam = new char[160];
  private char[] _reg = new char[256];
  private char[][][] _tilemap = new char[512][][];
  private GPUData[] _objdata = new GPUData[40];
  private List<GPUData> _objdatasorted;
  private GPUPalette _palette = new GPUPalette();
  private char[] _scanrow = new char[160];

  private int _curline = 0;
  private int _curscan = 0;
  private int _linemode = 0;
  private int _modeclocks = 0;

  private int _yscrl = 0;
  private int _xscrl = 0;
  private int _raster = 0;
  private int _ints = 0;
  
  private int _lcdon = 0;
  private int _bgon = 0;
  private int _objon = 0;
  private int _winon = 0;

  private int _objsize = 0;

  private int _bgtilebase = 0x0000;
  private int _bgmapbase = 0x1800;
  private int _wintilebase = 0x1800;
  
  char[] colors = new char[] {'\uE00F', '\uE00D', '\uE00E', '\uE006'};
  
  IMyTextPanel screen;
  private GPUScreen _scrn = new GPUScreen();
  
  private Z80 z80;
  private MMU mMU;
  
  public GPU(IMyTextPanel screen)
  {
      this.screen = screen;
  }
  
  public void update()
  {
    screen.ShowTextureOnScreen();
    screen.ShowPublicTextOnScreen();
    
    string buffer = "";    

    for(int y = 0; y < _scrn.height; y++)
    {
        for(int x = 0; x < _scrn.width; x++)
        {
            buffer += colors[_scrn.data[y*_scrn.width+x]/256*colors.Length];
        }
        buffer += "\n";
    }
    
    screen.WritePublicText(buffer, false);      
  }

  public void reset(Z80 z80, MMU mMU)
  {
    this.z80 = z80;
    this.mMU = mMU;

    Array.Clear(this._vram, (char)0, this._vram.Length);
    Array.Clear(this._oam, (char)0, this._oam.Length);
    for(int i=0; i<4; i++) 
    {
      this._palette.bg[i] = (char)255;
      this._palette.obj0[i] = (char)255;
      this._palette.obj1[i] = (char)255;
    }
    for(int i=0;i<512;i++)
    {
      this._tilemap[i] = new char[8][];
      for(int j=0;j<8;j++)
      {
        this._tilemap[i][j] = new char[8];
        for(int k=0;k<8;k++)
        {
          this._tilemap[i][j][k] = (char)0;
        }
      }
    }

//    Echo("GPU: Initialising screen.");
 
    this._scrn.width = 160;
    this._scrn.height = 144;
    Array.Clear(this._scrn.data, (char)255, this._scrn.data.Length);
        
    update();
    
    this._curline=0;
    this._curscan=0;
    this._linemode=2;
    this._modeclocks=0;
    this._yscrl=0;
    this._xscrl=0;
    this._raster=0;
    this._ints = 0;

    this._lcdon = 0;
    this._bgon = 0;
    this._objon = 0;
    this._winon = 0;

    this._objsize = 0;
    for(int i=0; i<160; i++) this._scanrow[i] = (char)0;

    for(int i=0; i<40; i++)
    {
      this._objdata[i] = new GPUData(i);
    }

    // Set to values expected by BIOS, to start
    this._bgtilebase = 0x0000;
    this._bgmapbase = 0x1800;
    this._wintilebase = 0x1800;

//    Echo("GPU: Reset.");
  }

  public void checkline() {
    this._modeclocks += z80._r.m;
    switch(this._linemode)
    {
      // In hblank
      case 0:
        if(this._modeclocks >= 51)
        {
          // End of hblank for last scanline; render screen
          if(this._curline == 143)
          {
            this._linemode = 1;
            this.update();
            mMU._if |= 1;
          }
          else
          {
            this._linemode = 2;
          }
          this._curline++;
	  this._curscan += 640;
          this._modeclocks=0;
        }
        break;

      // In vblank
      case 1:
        if(this._modeclocks >= 114)
        {
          this._modeclocks = 0;
          this._curline++;
          if(this._curline > 153)
          {
            this._curline = 0;
	    this._curscan = 0;
            this._linemode = 2;
          }
        }
        break;

      // In OAM-read mode
      case 2:
        if(this._modeclocks >= 20)
        {
          this._modeclocks = 0;
          this._linemode = 3;
        }
        break;

      // In VRAM-read mode
      case 3:
        // Render scanline at end of allotted time
        if(this._modeclocks >= 43)
        {
          this._modeclocks = 0;
          this._linemode = 0;
          if(this._lcdon > 0)
          {
            if(this._bgon > 0)
            {
              var linebase = this._curscan;
              var mapbase = this._bgmapbase + ((((this._curline+this._yscrl)&255)>>3)<<5);
              var y = (this._curline+this._yscrl)&7;
              var x = this._xscrl&7;
              var t = (this._xscrl>>3)&31;
//              var pixel;
              var w=160;

              if(this._bgtilebase > 0)
              {
	        var tile = this._vram[mapbase+t];
		if(tile<128) tile=(char)(256+tile);
                var tilerow = this._tilemap[tile][y];
                do
                {
		  this._scanrow[160-x] = tilerow[x];
                  this._scrn.data[linebase+3] = this._palette.bg[tilerow[x]];
                  x++;
                  if(x==8) { t=(t+1)&31; x=0; tile=this._vram[mapbase+t]; if(tile<128) tile=(char)(256+tile); tilerow = this._tilemap[tile][y]; }
                  linebase+=4;
                } while(--w > 0);
              }
              else
              {
                var tilerow=this._tilemap[this._vram[mapbase+t]][y];
                do
                {
		  this._scanrow[160-x] = tilerow[x];
                  this._scrn.data[linebase+3] = this._palette.bg[tilerow[x]];
                  x++;
                  if(x==8) { t=(t+1)&31; x=0; tilerow=this._tilemap[this._vram[mapbase+t]][y]; }
                  linebase+=4;
                } while(--w > 0);
	      }
            }
            if(this._objon > 0)
            {
              var cnt = 0;
              if(this._objsize > 0)
              {
                for(var i=0; i<40; i++)
                {
                }
              }
              else
              {
                char[] tilerow;
                var obj = this._objdatasorted[0];
                var pal = this._palette.obj0;
//                var pixel;
                var x = 0;
                var linebase = this._curscan;
                for(var i=0; i<40; i++)
                {
                  obj = this._objdatasorted[i];
                  if(obj.y <= this._curline && (obj.y+8) > this._curline)
                  {
                    if(obj.yflip > 0)
                      tilerow = this._tilemap[obj.tile][7-(this._curline-obj.y)];
                    else
                      tilerow = this._tilemap[obj.tile][this._curline-obj.y];

                    if(obj.palette > 0) pal=this._palette.obj1;
                    else pal=this._palette.obj0;

                    linebase = (this._curline*160+obj.x)*4;
                    if(obj.xflip > 0)
                    {
                      for(x=0; x<8; x++)
                      {
                        if(obj.x+x >=0 && obj.x+x < 160)
                        {
                          if(tilerow[7-x] > 0 && (obj.prio > 0 || !(this._scanrow[x] > 0)))
                          {
                            this._scrn.data[linebase+3] = pal[tilerow[7-x]];
                          }
                        }
                        linebase+=4;
                      }
                    }
                    else
                    {
                      for(x=0; x<8; x++)
                      {
                        if(obj.x+x >=0 && obj.x+x < 160)
                        {
                          if(tilerow[x] > 0 && (obj.prio > 0 || !(this._scanrow[x] > 0)))
                          {
                            this._scrn.data[linebase+3] = pal[tilerow[x]];
                          }
                        }
                        linebase+=4;
                      }
                    }
                    cnt++; if(cnt>10) break;
                  }
                }
              }
            }
          }
        }
        break;
    }
  }

  public void updatetile(int addr,int val) {
    var saddr = addr;
    if((addr&1) > 0) { saddr--; addr--; }
    var tile = (addr>>4)&511;
    var y = (addr>>1)&7;
    int sx;
    for(var x=0;x<8;x++)
    {
      sx=1<<(7-x);
      this._tilemap[tile][y][x] = (char)((((this._vram[saddr]&sx)>0)?1:0) | (((this._vram[saddr+1]&sx)>0)?2:0));
    }
  }

  public void updateoam(int addr,int val) {
    addr-=0xFE00;
    var obj=addr>>2;
    if(obj<40)
    {
      switch(addr&3)
      {
        case 0: this._objdata[obj].y=val-16; break;
        case 1: this._objdata[obj].x=val-8; break;
        case 2:
          if(this._objsize>0) this._objdata[obj].tile = (val&0xFE);
          else this._objdata[obj].tile = val;
          break;
        case 3:
          this._objdata[obj].palette = ((val&0x10)>0)?1:0;
          this._objdata[obj].xflip = ((val&0x20)>0)?1:0;
          this._objdata[obj].yflip = ((val&0x40)>0)?1:0;
          this._objdata[obj].prio = ((val&0x80)>0)?1:0;
          break;
     }
    }
    this._objdatasorted = new List<GPUData>(this._objdata);
    this._objdatasorted.Sort(delegate(GPUData a,GPUData b){
      if(a.x>b.x) return -1;
      if(a.num>b.num) return -1;
      return 0;
    });
  }

  public int rb(int addr) {
    var gaddr = addr-0xFF40;
    switch(gaddr)
    {
      case 0:
        return ((this._lcdon>0)?0x80:0)|
               ((this._bgtilebase==0x0000)?0x10:0)|
               ((this._bgmapbase==0x1C00)?0x08:0)|
               ((this._objsize>0)?0x04:0)|
               ((this._objon>0)?0x02:0)|
               ((this._bgon>0)?0x01:0);

      case 1:
        return (this._curline==this._raster?4:0)|this._linemode;

      case 2:
        return this._yscrl;

      case 3:
        return this._xscrl;

      case 4:
        return this._curline;

      case 5:
        return this._raster;

      default:
        return this._reg[gaddr];
    }
  }

  public void wb(int addr,int val) {
    var gaddr = addr-0xFF40;
    this._reg[gaddr] = (char)val;
    switch(gaddr)
    {
      case 0:
        this._lcdon = ((val&0x80)>0)?1:0;
        this._bgtilebase = ((val&0x10)>0)?0x0000:0x0800;
        this._bgmapbase = ((val&0x08)>0)?0x1C00:0x1800;
        this._objsize = ((val&0x04)>0)?1:0;
        this._objon = ((val&0x02)>0)?1:0;
        this._bgon = ((val&0x01)>0)?1:0;
        break;

      case 2:
        this._yscrl = val;
        break;

      case 3:
        this._xscrl = val;
        break;

      case 5:
        this._raster = val;
        break; // this was missing, should it be?
      // OAM DMA
      case 6:
        int v;
        for(var i=0; i<160; i++)
        {
          v = mMU.rb((val<<8)+i);
          this._oam[i] = (char)v;
          this.updateoam(0xFE00+i, v);
        }
        break;

      // BG palette mapping
      case 7:
        for(var i=0;i<4;i++)
        {
          switch((val>>(i*2))&3)
          {
            case 0: this._palette.bg[i] = (char)255; break;
            case 1: this._palette.bg[i] = (char)192; break;
            case 2: this._palette.bg[i] = (char)96; break;
            case 3: this._palette.bg[i] = (char)0; break;
          }
        }
        break;

      // OBJ0 palette mapping
      case 8:
        for(var i=0;i<4;i++)
        {
          switch((val>>(i*2))&3)
          {
            case 0: this._palette.obj0[i] = (char)255; break;
            case 1: this._palette.obj0[i] = (char)192; break;
            case 2: this._palette.obj0[i] = (char)96; break;
            case 3: this._palette.obj0[i] = (char)0; break;
          }
        }
        break;

      // OBJ1 palette mapping
      case 9:
        for(var i=0;i<4;i++)
        {
          switch((val>>(i*2))&3)
          {
            case 0: this._palette.obj1[i] = (char)255; break;
            case 1: this._palette.obj1[i] = (char)192; break;
            case 2: this._palette.obj1[i] = (char)96; break;
            case 3: this._palette.obj1[i] = (char)0; break;
          }
        }
        break;
    }
  }
}
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
class SpaceGameboy
{
  int run_interval = 0;
  string trace = "";

  private GPU gPU;
  private MMU mMU;
  private Z80 z80;
  private KEY kEY;
  private TIMER tIMER;
  
  public SpaceGameboy(IMyTextPanel screen)
  {
      this.gPU = new GPU(screen);
      this.mMU = new MMU();
      this.z80 = new Z80();
      this.kEY = new KEY();
      this.tIMER = new TIMER();
  }
  
  public void frame() {
    var fclock = z80._clock.m+17556;
    //var brk = document.getElementById('breakpoint').value;
    do {
      if(z80._halt>0) z80._r.m=1;
      else
      {
      //  z80._r.r = (z80._r.r+1) & 127;
        z80._map[mMU.rb(z80._r.pc++)]();
        z80._r.pc &= 65535;
      }
      if(z80._r.ime >0 && mMU._ie>0 && mMU._if>0)
      {
        z80._halt=0; z80._r.ime=0;
	var ifired = mMU._ie & mMU._if;
        if((ifired&1)>0) { mMU._if &= 0xFE; z80._ops.RST40(); }
        else if((ifired&2)>0) { mMU._if &= 0xFD; z80._ops.RST48(); }
        else if((ifired&4)>0) { mMU._if &= 0xFB; z80._ops.RST50(); }
        else if((ifired&8)>0) { mMU._if &= 0xF7; z80._ops.RST58(); }
        else if((ifired&16)>0) { mMU._if &= 0xEF; z80._ops.RST60(); }
	else { z80._r.ime=1; }
      }
      z80._clock.m += z80._r.m;
      gPU.checkline();
      tIMER.inc();
    } while(z80._clock.m < fclock);

  }
  
  public void reset(string file) {
    gPU.reset(this.z80, this.mMU); mMU.reset(gPU, tIMER, kEY, z80); z80.reset(mMU); kEY.reset(); tIMER.reset(mMU, z80);
    z80._r.pc=0x100;mMU._inbios=0;z80._r.sp=0xFFFE;/*z80._r.hl=0x014D;*/z80._r.c=0x13;z80._r.e=0xD8;z80._r.a=1;
    //TODO:                                              ^ this was missing, I don't know if it is supposed to be set
    mMU.load(file);
    this.run();
  
//    Echo("MAIN: Reset.");
  }
  
  public void run() {
    z80._stop = 0;
  }
 
    public void keydown(int keyCode)
    {
        kEY.keydown(keyCode);
    }
    
    public void keyup(int keyCode)
    {
        kEY.keyup(keyCode);        
    }
}
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
/**
 * jsGB: Z80 core
 * Imran Nazar, May 2009
 * Notes: This is a GameBoy Z80, not a this. There are differences.
 * Bugs: If PC wraps at the top of memory, this will not be caught until the end of an instruction
 */

 class Z80r {
    public int a = 0, b = 0, c = 0, d = 0, e = 0, h = 0, l = 0, f = 0,
    sp = 0, pc = 0, i = 0, r = 0,
    m = 0,
    ime = 0;     
 }
 
class Z80rsv {
    public int a = 0, b = 0, c = 0, d = 0, e = 0, h = 0, l = 0, f = 0;     
}
 
class Z80clock {
    public int m = 0;
}

class Z80ops {
    Z80 z80;
    MMU mMU;
    public Z80ops(Z80 z80, MMU mMU)
    {
        this.z80 = z80;
        this.mMU = mMU;
    }

    /*--- Load/store ---*/
    public void LDrr_bb() { z80._r.b=z80._r.b; z80._r.m=1; }
    public void LDrr_bc() { z80._r.b=z80._r.c; z80._r.m=1; }
    public void LDrr_bd() { z80._r.b=z80._r.d; z80._r.m=1; }
    public void LDrr_be() { z80._r.b=z80._r.e; z80._r.m=1; }
    public void LDrr_bh() { z80._r.b=z80._r.h; z80._r.m=1; }
    public void LDrr_bl() { z80._r.b=z80._r.l; z80._r.m=1; }
    public void LDrr_ba() { z80._r.b=z80._r.a; z80._r.m=1; }
    public void LDrr_cb() { z80._r.c=z80._r.b; z80._r.m=1; }
    public void LDrr_cc() { z80._r.c=z80._r.c; z80._r.m=1; }
    public void LDrr_cd() { z80._r.c=z80._r.d; z80._r.m=1; }
    public void LDrr_ce() { z80._r.c=z80._r.e; z80._r.m=1; }
    public void LDrr_ch() { z80._r.c=z80._r.h; z80._r.m=1; }
    public void LDrr_cl() { z80._r.c=z80._r.l; z80._r.m=1; }
    public void LDrr_ca() { z80._r.c=z80._r.a; z80._r.m=1; }
    public void LDrr_db() { z80._r.d=z80._r.b; z80._r.m=1; }
    public void LDrr_dc() { z80._r.d=z80._r.c; z80._r.m=1; }
    public void LDrr_dd() { z80._r.d=z80._r.d; z80._r.m=1; }
    public void LDrr_de() { z80._r.d=z80._r.e; z80._r.m=1; }
    public void LDrr_dh() { z80._r.d=z80._r.h; z80._r.m=1; }
    public void LDrr_dl() { z80._r.d=z80._r.l; z80._r.m=1; }
    public void LDrr_da() { z80._r.d=z80._r.a; z80._r.m=1; }
    public void LDrr_eb() { z80._r.e=z80._r.b; z80._r.m=1; }
    public void LDrr_ec() { z80._r.e=z80._r.c; z80._r.m=1; }
    public void LDrr_ed() { z80._r.e=z80._r.d; z80._r.m=1; }
    public void LDrr_ee() { z80._r.e=z80._r.e; z80._r.m=1; }
    public void LDrr_eh() { z80._r.e=z80._r.h; z80._r.m=1; }
    public void LDrr_el() { z80._r.e=z80._r.l; z80._r.m=1; }
    public void LDrr_ea() { z80._r.e=z80._r.a; z80._r.m=1; }
    public void LDrr_hb() { z80._r.h=z80._r.b; z80._r.m=1; }
    public void LDrr_hc() { z80._r.h=z80._r.c; z80._r.m=1; }
    public void LDrr_hd() { z80._r.h=z80._r.d; z80._r.m=1; }
    public void LDrr_he() { z80._r.h=z80._r.e; z80._r.m=1; }
    public void LDrr_hh() { z80._r.h=z80._r.h; z80._r.m=1; }
    public void LDrr_hl() { z80._r.h=z80._r.l; z80._r.m=1; }
    public void LDrr_ha() { z80._r.h=z80._r.a; z80._r.m=1; }
    public void LDrr_lb() { z80._r.l=z80._r.b; z80._r.m=1; }
    public void LDrr_lc() { z80._r.l=z80._r.c; z80._r.m=1; }
    public void LDrr_ld() { z80._r.l=z80._r.d; z80._r.m=1; }
    public void LDrr_le() { z80._r.l=z80._r.e; z80._r.m=1; }
    public void LDrr_lh() { z80._r.l=z80._r.h; z80._r.m=1; }
    public void LDrr_ll() { z80._r.l=z80._r.l; z80._r.m=1; }
    public void LDrr_la() { z80._r.l=z80._r.a; z80._r.m=1; }
    public void LDrr_ab() { z80._r.a=z80._r.b; z80._r.m=1; }
    public void LDrr_ac() { z80._r.a=z80._r.c; z80._r.m=1; }
    public void LDrr_ad() { z80._r.a=z80._r.d; z80._r.m=1; }
    public void LDrr_ae() { z80._r.a=z80._r.e; z80._r.m=1; }
    public void LDrr_ah() { z80._r.a=z80._r.h; z80._r.m=1; }
    public void LDrr_al() { z80._r.a=z80._r.l; z80._r.m=1; }
    public void LDrr_aa() { z80._r.a=z80._r.a; z80._r.m=1; }

    public void LDrHLm_b() { z80._r.b=mMU.rb((z80._r.h<<8)+z80._r.l); z80._r.m=2; }
    public void LDrHLm_c() { z80._r.c=mMU.rb((z80._r.h<<8)+z80._r.l); z80._r.m=2; }
    public void LDrHLm_d() { z80._r.d=mMU.rb((z80._r.h<<8)+z80._r.l); z80._r.m=2; }
    public void LDrHLm_e() { z80._r.e=mMU.rb((z80._r.h<<8)+z80._r.l); z80._r.m=2; }
    public void LDrHLm_h() { z80._r.h=mMU.rb((z80._r.h<<8)+z80._r.l); z80._r.m=2; }
    public void LDrHLm_l() { z80._r.l=mMU.rb((z80._r.h<<8)+z80._r.l); z80._r.m=2; }
    public void LDrHLm_a() { z80._r.a=mMU.rb((z80._r.h<<8)+z80._r.l); z80._r.m=2; }

    public void LDHLmr_b() { mMU.wb((z80._r.h<<8)+z80._r.l,z80._r.b); z80._r.m=2; }
    public void LDHLmr_c() { mMU.wb((z80._r.h<<8)+z80._r.l,z80._r.c); z80._r.m=2; }
    public void LDHLmr_d() { mMU.wb((z80._r.h<<8)+z80._r.l,z80._r.d); z80._r.m=2; }
    public void LDHLmr_e() { mMU.wb((z80._r.h<<8)+z80._r.l,z80._r.e); z80._r.m=2; }
    public void LDHLmr_h() { mMU.wb((z80._r.h<<8)+z80._r.l,z80._r.h); z80._r.m=2; }
    public void LDHLmr_l() { mMU.wb((z80._r.h<<8)+z80._r.l,z80._r.l); z80._r.m=2; }
    public void LDHLmr_a() { mMU.wb((z80._r.h<<8)+z80._r.l,z80._r.a); z80._r.m=2; }

    public void LDrn_b() { z80._r.b=mMU.rb(z80._r.pc); z80._r.pc++; z80._r.m=2; }
    public void LDrn_c() { z80._r.c=mMU.rb(z80._r.pc); z80._r.pc++; z80._r.m=2; }
    public void LDrn_d() { z80._r.d=mMU.rb(z80._r.pc); z80._r.pc++; z80._r.m=2; }
    public void LDrn_e() { z80._r.e=mMU.rb(z80._r.pc); z80._r.pc++; z80._r.m=2; }
    public void LDrn_h() { z80._r.h=mMU.rb(z80._r.pc); z80._r.pc++; z80._r.m=2; }
    public void LDrn_l() { z80._r.l=mMU.rb(z80._r.pc); z80._r.pc++; z80._r.m=2; }
    public void LDrn_a() { z80._r.a=mMU.rb(z80._r.pc); z80._r.pc++; z80._r.m=2; }

    public void LDHLmn() { mMU.wb((z80._r.h<<8)+z80._r.l, mMU.rb(z80._r.pc)); z80._r.pc++; z80._r.m=3; }

    public void LDBCmA() { mMU.wb((z80._r.b<<8)+z80._r.c, z80._r.a); z80._r.m=2; }
    public void LDDEmA() { mMU.wb((z80._r.d<<8)+z80._r.e, z80._r.a); z80._r.m=2; }

    public void LDmmA() { mMU.wb(mMU.rw(z80._r.pc), z80._r.a); z80._r.pc+=2; z80._r.m=4; }

    public void LDmmSP() { throw new Exception("Z80: LDmmSP not implemented"); }

    public void LDABCm() { z80._r.a=mMU.rb((z80._r.b<<8)+z80._r.c); z80._r.m=2; }
    public void LDADEm() { z80._r.a=mMU.rb((z80._r.d<<8)+z80._r.e); z80._r.m=2; }

    public void LDAmm() { z80._r.a=mMU.rb(mMU.rw(z80._r.pc)); z80._r.pc+=2; z80._r.m=4; }

    public void LDBCnn() { z80._r.c=mMU.rb(z80._r.pc); z80._r.b=mMU.rb(z80._r.pc+1); z80._r.pc+=2; z80._r.m=3; }
    public void LDDEnn() { z80._r.e=mMU.rb(z80._r.pc); z80._r.d=mMU.rb(z80._r.pc+1); z80._r.pc+=2; z80._r.m=3; }
    public void LDHLnn() { z80._r.l=mMU.rb(z80._r.pc); z80._r.h=mMU.rb(z80._r.pc+1); z80._r.pc+=2; z80._r.m=3; }
    public void LDSPnn() { z80._r.sp=mMU.rw(z80._r.pc); z80._r.pc+=2; z80._r.m=3; }

    public void LDHLmm() { var i=mMU.rw(z80._r.pc); z80._r.pc+=2; z80._r.l=mMU.rb(i); z80._r.h=mMU.rb(i+1); z80._r.m=5; }
    public void LDmmHL() { var i=mMU.rw(z80._r.pc); z80._r.pc+=2; mMU.ww(i,(z80._r.h<<8)+z80._r.l); z80._r.m=5; }

    public void LDHLIA() { mMU.wb((z80._r.h<<8)+z80._r.l, z80._r.a); z80._r.l=(z80._r.l+1)&255; if(!(z80._r.l>0)) z80._r.h=(z80._r.h+1)&255; z80._r.m=2; }
    public void LDAHLI() { z80._r.a=mMU.rb((z80._r.h<<8)+z80._r.l); z80._r.l=(z80._r.l+1)&255; if(!(z80._r.l>0)) z80._r.h=(z80._r.h+1)&255; z80._r.m=2; }

    public void LDHLDA() { mMU.wb((z80._r.h<<8)+z80._r.l, z80._r.a); z80._r.l=(z80._r.l-1)&255; if(z80._r.l==255) z80._r.h=(z80._r.h-1)&255; z80._r.m=2; }
    public void LDAHLD() { z80._r.a=mMU.rb((z80._r.h<<8)+z80._r.l); z80._r.l=(z80._r.l-1)&255; if(z80._r.l==255) z80._r.h=(z80._r.h-1)&255; z80._r.m=2; }

    public void LDAIOn() { z80._r.a=mMU.rb(0xFF00+mMU.rb(z80._r.pc)); z80._r.pc++; z80._r.m=3; }
    public void LDIOnA() { mMU.wb(0xFF00+mMU.rb(z80._r.pc),z80._r.a); z80._r.pc++; z80._r.m=3; }
    public void LDAIOC() { z80._r.a=mMU.rb(0xFF00+z80._r.c); z80._r.m=2; }
    public void LDIOCA() { mMU.wb(0xFF00+z80._r.c,z80._r.a); z80._r.m=2; }

    public void LDHLSPn() { var i=mMU.rb(z80._r.pc); if(i>127) i=-((~i+1)&255); z80._r.pc++; i+=z80._r.sp; z80._r.h=(i>>8)&255; z80._r.l=i&255; z80._r.m=3; }

    public void SWAPr_b() { var tr=z80._r.b; z80._r.b=((tr&0xF)<<4)|((tr&0xF0)>>4); z80._r.f=(z80._r.b>0)?0:0x80; z80._r.m=1; }
    public void SWAPr_c() { var tr=z80._r.c; z80._r.c=((tr&0xF)<<4)|((tr&0xF0)>>4); z80._r.f=(z80._r.c>0)?0:0x80; z80._r.m=1; }
    public void SWAPr_d() { var tr=z80._r.d; z80._r.d=((tr&0xF)<<4)|((tr&0xF0)>>4); z80._r.f=(z80._r.d>0)?0:0x80; z80._r.m=1; }
    public void SWAPr_e() { var tr=z80._r.e; z80._r.e=((tr&0xF)<<4)|((tr&0xF0)>>4); z80._r.f=(z80._r.e>0)?0:0x80; z80._r.m=1; }
    public void SWAPr_h() { var tr=z80._r.h; z80._r.h=((tr&0xF)<<4)|((tr&0xF0)>>4); z80._r.f=(z80._r.h>0)?0:0x80; z80._r.m=1; }
    public void SWAPr_l() { var tr=z80._r.l; z80._r.l=((tr&0xF)<<4)|((tr&0xF0)>>4); z80._r.f=(z80._r.l>0)?0:0x80; z80._r.m=1; }
    public void SWAPr_a() { var tr=z80._r.a; z80._r.a=((tr&0xF)<<4)|((tr&0xF0)>>4); z80._r.f=(z80._r.a>0)?0:0x80; z80._r.m=1; }

    /*--- Data processing ---*/
    public void ADDr_b() { var a=z80._r.a; z80._r.a+=z80._r.b; z80._r.f=(z80._r.a>255)?0x10:0; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.b^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void ADDr_c() { var a=z80._r.a; z80._r.a+=z80._r.c; z80._r.f=(z80._r.a>255)?0x10:0; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.c^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void ADDr_d() { var a=z80._r.a; z80._r.a+=z80._r.d; z80._r.f=(z80._r.a>255)?0x10:0; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.d^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void ADDr_e() { var a=z80._r.a; z80._r.a+=z80._r.e; z80._r.f=(z80._r.a>255)?0x10:0; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.e^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void ADDr_h() { var a=z80._r.a; z80._r.a+=z80._r.h; z80._r.f=(z80._r.a>255)?0x10:0; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.h^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void ADDr_l() { var a=z80._r.a; z80._r.a+=z80._r.l; z80._r.f=(z80._r.a>255)?0x10:0; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.l^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void ADDr_a() { var a=z80._r.a; z80._r.a+=z80._r.a; z80._r.f=(z80._r.a>255)?0x10:0; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.a^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void ADDHL() { var a=z80._r.a; var m=mMU.rb((z80._r.h<<8)+z80._r.l); z80._r.a+=m; z80._r.f=(z80._r.a>255)?0x10:0; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^a^m)&0x10)>0) z80._r.f|=0x20; z80._r.m=2; }
    public void ADDn() { var a=z80._r.a; var m=mMU.rb(z80._r.pc); z80._r.a+=m; z80._r.pc++; z80._r.f=(z80._r.a>255)?0x10:0; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^a^m)&0x10)>0) z80._r.f|=0x20; z80._r.m=2; }
    public void ADDHLBC() { var hl=(z80._r.h<<8)+z80._r.l; hl+=(z80._r.b<<8)+z80._r.c; if(hl>65535) z80._r.f|=0x10; else z80._r.f&=0xEF; z80._r.h=(hl>>8)&255; z80._r.l=hl&255; z80._r.m=3; }
    public void ADDHLDE() { var hl=(z80._r.h<<8)+z80._r.l; hl+=(z80._r.d<<8)+z80._r.e; if(hl>65535) z80._r.f|=0x10; else z80._r.f&=0xEF; z80._r.h=(hl>>8)&255; z80._r.l=hl&255; z80._r.m=3; }
    public void ADDHLHL() { var hl=(z80._r.h<<8)+z80._r.l; hl+=(z80._r.h<<8)+z80._r.l; if(hl>65535) z80._r.f|=0x10; else z80._r.f&=0xEF; z80._r.h=(hl>>8)&255; z80._r.l=hl&255; z80._r.m=3; }
    public void ADDHLSP() { var hl=(z80._r.h<<8)+z80._r.l; hl+=z80._r.sp; if(hl>65535) z80._r.f|=0x10; else z80._r.f&=0xEF; z80._r.h=(hl>>8)&255; z80._r.l=hl&255; z80._r.m=3; }
    public void ADDSPn() { var i=mMU.rb(z80._r.pc); if(i>127) i=-((~i+1)&255); z80._r.pc++; z80._r.sp+=i; z80._r.m=4; }

    public void ADCr_b() { var a=z80._r.a; z80._r.a+=z80._r.b; z80._r.a+=((z80._r.f&0x10)>0)?1:0; z80._r.f=(z80._r.a>255)?0x10:0; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.b^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void ADCr_c() { var a=z80._r.a; z80._r.a+=z80._r.c; z80._r.a+=((z80._r.f&0x10)>0)?1:0; z80._r.f=(z80._r.a>255)?0x10:0; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.c^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void ADCr_d() { var a=z80._r.a; z80._r.a+=z80._r.d; z80._r.a+=((z80._r.f&0x10)>0)?1:0; z80._r.f=(z80._r.a>255)?0x10:0; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.d^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void ADCr_e() { var a=z80._r.a; z80._r.a+=z80._r.e; z80._r.a+=((z80._r.f&0x10)>0)?1:0; z80._r.f=(z80._r.a>255)?0x10:0; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.e^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void ADCr_h() { var a=z80._r.a; z80._r.a+=z80._r.h; z80._r.a+=((z80._r.f&0x10)>0)?1:0; z80._r.f=(z80._r.a>255)?0x10:0; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.h^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void ADCr_l() { var a=z80._r.a; z80._r.a+=z80._r.l; z80._r.a+=((z80._r.f&0x10)>0)?1:0; z80._r.f=(z80._r.a>255)?0x10:0; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.l^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void ADCr_a() { var a=z80._r.a; z80._r.a+=z80._r.a; z80._r.a+=((z80._r.f&0x10)>0)?1:0; z80._r.f=(z80._r.a>255)?0x10:0; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.a^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void ADCHL() { var a=z80._r.a; var m=mMU.rb((z80._r.h<<8)+z80._r.l); z80._r.a+=m; z80._r.a+=((z80._r.f&0x10)>0)?1:0; z80._r.f=(z80._r.a>255)?0x10:0; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^m^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=2; }
    public void ADCn() { var a=z80._r.a; var m=mMU.rb(z80._r.pc); z80._r.a+=m; z80._r.pc++; z80._r.a+=((z80._r.f&0x10)>0)?1:0; z80._r.f=(z80._r.a>255)?0x10:0; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^m^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=2; }

    public void SUBr_b() { var a=z80._r.a; z80._r.a-=z80._r.b; z80._r.f=(z80._r.a<0)?0x50:0x40; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.b^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void SUBr_c() { var a=z80._r.a; z80._r.a-=z80._r.c; z80._r.f=(z80._r.a<0)?0x50:0x40; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.c^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void SUBr_d() { var a=z80._r.a; z80._r.a-=z80._r.d; z80._r.f=(z80._r.a<0)?0x50:0x40; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.d^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void SUBr_e() { var a=z80._r.a; z80._r.a-=z80._r.e; z80._r.f=(z80._r.a<0)?0x50:0x40; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.e^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void SUBr_h() { var a=z80._r.a; z80._r.a-=z80._r.h; z80._r.f=(z80._r.a<0)?0x50:0x40; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.h^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void SUBr_l() { var a=z80._r.a; z80._r.a-=z80._r.l; z80._r.f=(z80._r.a<0)?0x50:0x40; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.l^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void SUBr_a() { var a=z80._r.a; z80._r.a-=z80._r.a; z80._r.f=(z80._r.a<0)?0x50:0x40; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.a^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void SUBHL() { var a=z80._r.a; var m=mMU.rb((z80._r.h<<8)+z80._r.l); z80._r.a-=m; z80._r.f=(z80._r.a<0)?0x50:0x40; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^m^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=2; }
    public void SUBn() { var a=z80._r.a; var m=mMU.rb(z80._r.pc); z80._r.a-=m; z80._r.pc++; z80._r.f=(z80._r.a<0)?0x50:0x40; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^m^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=2; }

    public void SBCr_b() { var a=z80._r.a; z80._r.a-=z80._r.b; z80._r.a-=((z80._r.f&0x10)>0)?1:0; z80._r.f=(z80._r.a<0)?0x50:0x40; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.b^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void SBCr_c() { var a=z80._r.a; z80._r.a-=z80._r.c; z80._r.a-=((z80._r.f&0x10)>0)?1:0; z80._r.f=(z80._r.a<0)?0x50:0x40; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.c^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void SBCr_d() { var a=z80._r.a; z80._r.a-=z80._r.d; z80._r.a-=((z80._r.f&0x10)>0)?1:0; z80._r.f=(z80._r.a<0)?0x50:0x40; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.d^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void SBCr_e() { var a=z80._r.a; z80._r.a-=z80._r.e; z80._r.a-=((z80._r.f&0x10)>0)?1:0; z80._r.f=(z80._r.a<0)?0x50:0x40; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.e^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void SBCr_h() { var a=z80._r.a; z80._r.a-=z80._r.h; z80._r.a-=((z80._r.f&0x10)>0)?1:0; z80._r.f=(z80._r.a<0)?0x50:0x40; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.h^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void SBCr_l() { var a=z80._r.a; z80._r.a-=z80._r.l; z80._r.a-=((z80._r.f&0x10)>0)?1:0; z80._r.f=(z80._r.a<0)?0x50:0x40; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.l^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void SBCr_a() { var a=z80._r.a; z80._r.a-=z80._r.a; z80._r.a-=((z80._r.f&0x10)>0)?1:0; z80._r.f=(z80._r.a<0)?0x50:0x40; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.a^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void SBCHL() { var a=z80._r.a; var m=mMU.rb((z80._r.h<<8)+z80._r.l); z80._r.a-=m; z80._r.a-=((z80._r.f&0x10)>0)?1:0; z80._r.f=(z80._r.a<0)?0x50:0x40; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^m^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=2; }
    public void SBCn() { var a=z80._r.a; var m=mMU.rb(z80._r.pc); z80._r.a-=m; z80._r.pc++; z80._r.a-=((z80._r.f&0x10)>0)?1:0; z80._r.f=(z80._r.a<0)?0x50:0x40; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; if(((z80._r.a^m^a)&0x10)>0) z80._r.f|=0x20; z80._r.m=2; }

    public void CPr_b() { var i=z80._r.a; i-=z80._r.b; z80._r.f=(i<0)?0x50:0x40; i&=255; if(!(i>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.b^i)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void CPr_c() { var i=z80._r.a; i-=z80._r.c; z80._r.f=(i<0)?0x50:0x40; i&=255; if(!(i>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.c^i)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void CPr_d() { var i=z80._r.a; i-=z80._r.d; z80._r.f=(i<0)?0x50:0x40; i&=255; if(!(i>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.d^i)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void CPr_e() { var i=z80._r.a; i-=z80._r.e; z80._r.f=(i<0)?0x50:0x40; i&=255; if(!(i>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.e^i)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void CPr_h() { var i=z80._r.a; i-=z80._r.h; z80._r.f=(i<0)?0x50:0x40; i&=255; if(!(i>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.h^i)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void CPr_l() { var i=z80._r.a; i-=z80._r.l; z80._r.f=(i<0)?0x50:0x40; i&=255; if(!(i>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.l^i)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void CPr_a() { var i=z80._r.a; i-=z80._r.a; z80._r.f=(i<0)?0x50:0x40; i&=255; if(!(i>0)) z80._r.f|=0x80; if(((z80._r.a^z80._r.a^i)&0x10)>0) z80._r.f|=0x20; z80._r.m=1; }
    public void CPHL() { var i=z80._r.a; var m=mMU.rb((z80._r.h<<8)+z80._r.l); i-=m; z80._r.f=(i<0)?0x50:0x40; i&=255; if(!(i>0)) z80._r.f|=0x80; if(((z80._r.a^i^m)&0x10)>0) z80._r.f|=0x20; z80._r.m=2; }
    public void CPn() { var i=z80._r.a; var m=mMU.rb(z80._r.pc); i-=m; z80._r.pc++; z80._r.f=(i<0)?0x50:0x40; i&=255; if(!(i>0)) z80._r.f|=0x80; if(((z80._r.a^i^m)&0x10)>0) z80._r.f|=0x20; z80._r.m=2; }

    public void DAA() { var a=z80._r.a; if((z80._r.f&0x20)>0||((z80._r.a&15)>9)) z80._r.a+=6; z80._r.f&=0xEF; if(((z80._r.f&0x20) > 0)||(a>0x99)) { z80._r.a+=0x60; z80._r.f|=0x10; } z80._r.m=1; }

    public void ANDr_b() { z80._r.a&=z80._r.b; z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.m=1; }
    public void ANDr_c() { z80._r.a&=z80._r.c; z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.m=1; }
    public void ANDr_d() { z80._r.a&=z80._r.d; z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.m=1; }
    public void ANDr_e() { z80._r.a&=z80._r.e; z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.m=1; }
    public void ANDr_h() { z80._r.a&=z80._r.h; z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.m=1; }
    public void ANDr_l() { z80._r.a&=z80._r.l; z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.m=1; }
    public void ANDr_a() { z80._r.a&=z80._r.a; z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.m=1; }
    public void ANDHL() { z80._r.a&=mMU.rb((z80._r.h<<8)+z80._r.l); z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.m=2; }
    public void ANDn() { z80._r.a&=mMU.rb(z80._r.pc); z80._r.pc++; z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.m=2; }

    public void ORr_b() { z80._r.a|=z80._r.b; z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.m=1; }
    public void ORr_c() { z80._r.a|=z80._r.c; z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.m=1; }
    public void ORr_d() { z80._r.a|=z80._r.d; z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.m=1; }
    public void ORr_e() { z80._r.a|=z80._r.e; z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.m=1; }
    public void ORr_h() { z80._r.a|=z80._r.h; z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.m=1; }
    public void ORr_l() { z80._r.a|=z80._r.l; z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.m=1; }
    public void ORr_a() { z80._r.a|=z80._r.a; z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.m=1; }
    public void ORHL() { z80._r.a|=mMU.rb((z80._r.h<<8)+z80._r.l); z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.m=2; }
    public void ORn() { z80._r.a|=mMU.rb(z80._r.pc); z80._r.pc++; z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.m=2; }

    public void XORr_b() { z80._r.a^=z80._r.b; z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.m=1; }
    public void XORr_c() { z80._r.a^=z80._r.c; z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.m=1; }
    public void XORr_d() { z80._r.a^=z80._r.d; z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.m=1; }
    public void XORr_e() { z80._r.a^=z80._r.e; z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.m=1; }
    public void XORr_h() { z80._r.a^=z80._r.h; z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.m=1; }
    public void XORr_l() { z80._r.a^=z80._r.l; z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.m=1; }
    public void XORr_a() { z80._r.a^=z80._r.a; z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.m=1; }
    public void XORHL() { z80._r.a^=mMU.rb((z80._r.h<<8)+z80._r.l); z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.m=2; }
    public void XORn() { z80._r.a^=mMU.rb(z80._r.pc); z80._r.pc++; z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.m=2; }

    public void INCr_b() { z80._r.b++; z80._r.b&=255; z80._r.f=(z80._r.b>0)?0:0x80; z80._r.m=1; }
    public void INCr_c() { z80._r.c++; z80._r.c&=255; z80._r.f=(z80._r.c>0)?0:0x80; z80._r.m=1; }
    public void INCr_d() { z80._r.d++; z80._r.d&=255; z80._r.f=(z80._r.d>0)?0:0x80; z80._r.m=1; }
    public void INCr_e() { z80._r.e++; z80._r.e&=255; z80._r.f=(z80._r.e>0)?0:0x80; z80._r.m=1; }
    public void INCr_h() { z80._r.h++; z80._r.h&=255; z80._r.f=(z80._r.h>0)?0:0x80; z80._r.m=1; }
    public void INCr_l() { z80._r.l++; z80._r.l&=255; z80._r.f=(z80._r.l>0)?0:0x80; z80._r.m=1; }
    public void INCr_a() { z80._r.a++; z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.m=1; }
    public void INCHLm() { var i=mMU.rb((z80._r.h<<8)+z80._r.l)+1; i&=255; mMU.wb((z80._r.h<<8)+z80._r.l,i); z80._r.f=(i>0)?0:0x80; z80._r.m=3; }

    public void DECr_b() { z80._r.b--; z80._r.b&=255; z80._r.f=(z80._r.b>0)?0:0x80; z80._r.m=1; }
    public void DECr_c() { z80._r.c--; z80._r.c&=255; z80._r.f=(z80._r.c>0)?0:0x80; z80._r.m=1; }
    public void DECr_d() { z80._r.d--; z80._r.d&=255; z80._r.f=(z80._r.d>0)?0:0x80; z80._r.m=1; }
    public void DECr_e() { z80._r.e--; z80._r.e&=255; z80._r.f=(z80._r.e>0)?0:0x80; z80._r.m=1; }
    public void DECr_h() { z80._r.h--; z80._r.h&=255; z80._r.f=(z80._r.h>0)?0:0x80; z80._r.m=1; }
    public void DECr_l() { z80._r.l--; z80._r.l&=255; z80._r.f=(z80._r.l>0)?0:0x80; z80._r.m=1; }
    public void DECr_a() { z80._r.a--; z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.m=1; }
    public void DECHLm() { var i=mMU.rb((z80._r.h<<8)+z80._r.l)-1; i&=255; mMU.wb((z80._r.h<<8)+z80._r.l,i); z80._r.f=(i>0)?0:0x80; z80._r.m=3; }

    public void INCBC() { z80._r.c=(z80._r.c+1)&255; if(!(z80._r.c>0)) z80._r.b=(z80._r.b+1)&255; z80._r.m=1; }
    public void INCDE() { z80._r.e=(z80._r.e+1)&255; if(!(z80._r.e>0)) z80._r.d=(z80._r.d+1)&255; z80._r.m=1; }
    public void INCHL() { z80._r.l=(z80._r.l+1)&255; if(!(z80._r.l>0)) z80._r.h=(z80._r.h+1)&255; z80._r.m=1; }
    public void INCSP() { z80._r.sp=(z80._r.sp+1)&65535; z80._r.m=1; }

    public void DECBC() { z80._r.c=(z80._r.c-1)&255; if(z80._r.c==255) z80._r.b=(z80._r.b-1)&255; z80._r.m=1; }
    public void DECDE() { z80._r.e=(z80._r.e-1)&255; if(z80._r.e==255) z80._r.d=(z80._r.d-1)&255; z80._r.m=1; }
    public void DECHL() { z80._r.l=(z80._r.l-1)&255; if(z80._r.l==255) z80._r.h=(z80._r.h-1)&255; z80._r.m=1; }
    public void DECSP() { z80._r.sp=(z80._r.sp-1)&65535; z80._r.m=1; }

    /*--- Bit manipulation ---*/
    public void BIT0b() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.b&0x01)>0)?0:0x80; z80._r.m=2; }
    public void BIT0c() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.c&0x01)>0)?0:0x80; z80._r.m=2; }
    public void BIT0d() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.d&0x01)>0)?0:0x80; z80._r.m=2; }
    public void BIT0e() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.e&0x01)>0)?0:0x80; z80._r.m=2; }
    public void BIT0h() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.h&0x01)>0)?0:0x80; z80._r.m=2; }
    public void BIT0l() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.l&0x01)>0)?0:0x80; z80._r.m=2; }
    public void BIT0a() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.a&0x01)>0)?0:0x80; z80._r.m=2; }
    public void BIT0m() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((mMU.rb((z80._r.h<<8)+z80._r.l)&0x01)>0)?0:0x80; z80._r.m=3; }

    public void RES0b() { z80._r.b&=0xFE; z80._r.m=2; }
    public void RES0c() { z80._r.c&=0xFE; z80._r.m=2; }
    public void RES0d() { z80._r.d&=0xFE; z80._r.m=2; }
    public void RES0e() { z80._r.e&=0xFE; z80._r.m=2; }
    public void RES0h() { z80._r.h&=0xFE; z80._r.m=2; }
    public void RES0l() { z80._r.l&=0xFE; z80._r.m=2; }
    public void RES0a() { z80._r.a&=0xFE; z80._r.m=2; }
    public void RES0m() { var i=mMU.rb((z80._r.h<<8)+z80._r.l); i&=0xFE; mMU.wb((z80._r.h<<8)+z80._r.l,i); z80._r.m=4; }

    public void SET0b() { z80._r.b|=0x01; z80._r.m=2; }
    public void SET0c() { z80._r.b|=0x01; z80._r.m=2; }
    public void SET0d() { z80._r.b|=0x01; z80._r.m=2; }
    public void SET0e() { z80._r.b|=0x01; z80._r.m=2; }
    public void SET0h() { z80._r.b|=0x01; z80._r.m=2; }
    public void SET0l() { z80._r.b|=0x01; z80._r.m=2; }
    public void SET0a() { z80._r.b|=0x01; z80._r.m=2; }
    public void SET0m() { var i=mMU.rb((z80._r.h<<8)+z80._r.l); i|=0x01; mMU.wb((z80._r.h<<8)+z80._r.l,i); z80._r.m=4; }

    public void BIT1b() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.b&0x02)>0)?0:0x80; z80._r.m=2; }
    public void BIT1c() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.c&0x02)>0)?0:0x80; z80._r.m=2; }
    public void BIT1d() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.d&0x02)>0)?0:0x80; z80._r.m=2; }
    public void BIT1e() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.e&0x02)>0)?0:0x80; z80._r.m=2; }
    public void BIT1h() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.h&0x02)>0)?0:0x80; z80._r.m=2; }
    public void BIT1l() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.l&0x02)>0)?0:0x80; z80._r.m=2; }
    public void BIT1a() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.a&0x02)>0)?0:0x80; z80._r.m=2; }
    public void BIT1m() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((mMU.rb((z80._r.h<<8)+z80._r.l)&0x02)>0)?0:0x80; z80._r.m=3; }

    public void RES1b() { z80._r.b&=0xFD; z80._r.m=2; }
    public void RES1c() { z80._r.c&=0xFD; z80._r.m=2; }
    public void RES1d() { z80._r.d&=0xFD; z80._r.m=2; }
    public void RES1e() { z80._r.e&=0xFD; z80._r.m=2; }
    public void RES1h() { z80._r.h&=0xFD; z80._r.m=2; }
    public void RES1l() { z80._r.l&=0xFD; z80._r.m=2; }
    public void RES1a() { z80._r.a&=0xFD; z80._r.m=2; }
    public void RES1m() { var i=mMU.rb((z80._r.h<<8)+z80._r.l); i&=0xFD; mMU.wb((z80._r.h<<8)+z80._r.l,i); z80._r.m=4; }

    public void SET1b() { z80._r.b|=0x02; z80._r.m=2; }
    public void SET1c() { z80._r.b|=0x02; z80._r.m=2; }
    public void SET1d() { z80._r.b|=0x02; z80._r.m=2; }
    public void SET1e() { z80._r.b|=0x02; z80._r.m=2; }
    public void SET1h() { z80._r.b|=0x02; z80._r.m=2; }
    public void SET1l() { z80._r.b|=0x02; z80._r.m=2; }
    public void SET1a() { z80._r.b|=0x02; z80._r.m=2; }
    public void SET1m() { var i=mMU.rb((z80._r.h<<8)+z80._r.l); i|=0x02; mMU.wb((z80._r.h<<8)+z80._r.l,i); z80._r.m=4; }

    public void BIT2b() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.b&0x04)>0)?0:0x80; z80._r.m=2; }
    public void BIT2c() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.c&0x04)>0)?0:0x80; z80._r.m=2; }
    public void BIT2d() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.d&0x04)>0)?0:0x80; z80._r.m=2; }
    public void BIT2e() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.e&0x04)>0)?0:0x80; z80._r.m=2; }
    public void BIT2h() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.h&0x04)>0)?0:0x80; z80._r.m=2; }
    public void BIT2l() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.l&0x04)>0)?0:0x80; z80._r.m=2; }
    public void BIT2a() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.a&0x04)>0)?0:0x80; z80._r.m=2; }
    public void BIT2m() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((mMU.rb((z80._r.h<<8)+z80._r.l)&0x04)>0)?0:0x80; z80._r.m=3; }

    public void RES2b() { z80._r.b&=0xFB; z80._r.m=2; }
    public void RES2c() { z80._r.c&=0xFB; z80._r.m=2; }
    public void RES2d() { z80._r.d&=0xFB; z80._r.m=2; }
    public void RES2e() { z80._r.e&=0xFB; z80._r.m=2; }
    public void RES2h() { z80._r.h&=0xFB; z80._r.m=2; }
    public void RES2l() { z80._r.l&=0xFB; z80._r.m=2; }
    public void RES2a() { z80._r.a&=0xFB; z80._r.m=2; }
    public void RES2m() { var i=mMU.rb((z80._r.h<<8)+z80._r.l); i&=0xFB; mMU.wb((z80._r.h<<8)+z80._r.l,i); z80._r.m=4; }

    public void SET2b() { z80._r.b|=0x04; z80._r.m=2; }
    public void SET2c() { z80._r.b|=0x04; z80._r.m=2; }
    public void SET2d() { z80._r.b|=0x04; z80._r.m=2; }
    public void SET2e() { z80._r.b|=0x04; z80._r.m=2; }
    public void SET2h() { z80._r.b|=0x04; z80._r.m=2; }
    public void SET2l() { z80._r.b|=0x04; z80._r.m=2; }
    public void SET2a() { z80._r.b|=0x04; z80._r.m=2; }
    public void SET2m() { var i=mMU.rb((z80._r.h<<8)+z80._r.l); i|=0x04; mMU.wb((z80._r.h<<8)+z80._r.l,i); z80._r.m=4; }

    public void BIT3b() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.b&0x08)>0)?0:0x80; z80._r.m=2; }
    public void BIT3c() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.c&0x08)>0)?0:0x80; z80._r.m=2; }
    public void BIT3d() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.d&0x08)>0)?0:0x80; z80._r.m=2; }
    public void BIT3e() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.e&0x08)>0)?0:0x80; z80._r.m=2; }
    public void BIT3h() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.h&0x08)>0)?0:0x80; z80._r.m=2; }
    public void BIT3l() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.l&0x08)>0)?0:0x80; z80._r.m=2; }
    public void BIT3a() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.a&0x08)>0)?0:0x80; z80._r.m=2; }
    public void BIT3m() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((mMU.rb((z80._r.h<<8)+z80._r.l)&0x08)>0)?0:0x80; z80._r.m=3; }

    public void RES3b() { z80._r.b&=0xF7; z80._r.m=2; }
    public void RES3c() { z80._r.c&=0xF7; z80._r.m=2; }
    public void RES3d() { z80._r.d&=0xF7; z80._r.m=2; }
    public void RES3e() { z80._r.e&=0xF7; z80._r.m=2; }
    public void RES3h() { z80._r.h&=0xF7; z80._r.m=2; }
    public void RES3l() { z80._r.l&=0xF7; z80._r.m=2; }
    public void RES3a() { z80._r.a&=0xF7; z80._r.m=2; }
    public void RES3m() { var i=mMU.rb((z80._r.h<<8)+z80._r.l); i&=0xF7; mMU.wb((z80._r.h<<8)+z80._r.l,i); z80._r.m=4; }

    public void SET3b() { z80._r.b|=0x08; z80._r.m=2; }
    public void SET3c() { z80._r.b|=0x08; z80._r.m=2; }
    public void SET3d() { z80._r.b|=0x08; z80._r.m=2; }
    public void SET3e() { z80._r.b|=0x08; z80._r.m=2; }
    public void SET3h() { z80._r.b|=0x08; z80._r.m=2; }
    public void SET3l() { z80._r.b|=0x08; z80._r.m=2; }
    public void SET3a() { z80._r.b|=0x08; z80._r.m=2; }
    public void SET3m() { var i=mMU.rb((z80._r.h<<8)+z80._r.l); i|=0x08; mMU.wb((z80._r.h<<8)+z80._r.l,i); z80._r.m=4; }

    public void BIT4b() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.b&0x10)>0)?0:0x80; z80._r.m=2; }
    public void BIT4c() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.c&0x10)>0)?0:0x80; z80._r.m=2; }
    public void BIT4d() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.d&0x10)>0)?0:0x80; z80._r.m=2; }
    public void BIT4e() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.e&0x10)>0)?0:0x80; z80._r.m=2; }
    public void BIT4h() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.h&0x10)>0)?0:0x80; z80._r.m=2; }
    public void BIT4l() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.l&0x10)>0)?0:0x80; z80._r.m=2; }
    public void BIT4a() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.a&0x10)>0)?0:0x80; z80._r.m=2; }
    public void BIT4m() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((mMU.rb((z80._r.h<<8)+z80._r.l)&0x10)>0)?0:0x80; z80._r.m=3; }

    public void RES4b() { z80._r.b&=0xEF; z80._r.m=2; }
    public void RES4c() { z80._r.c&=0xEF; z80._r.m=2; }
    public void RES4d() { z80._r.d&=0xEF; z80._r.m=2; }
    public void RES4e() { z80._r.e&=0xEF; z80._r.m=2; }
    public void RES4h() { z80._r.h&=0xEF; z80._r.m=2; }
    public void RES4l() { z80._r.l&=0xEF; z80._r.m=2; }
    public void RES4a() { z80._r.a&=0xEF; z80._r.m=2; }
    public void RES4m() { var i=mMU.rb((z80._r.h<<8)+z80._r.l); i&=0xEF; mMU.wb((z80._r.h<<8)+z80._r.l,i); z80._r.m=4; }

    public void SET4b() { z80._r.b|=0x10; z80._r.m=2; }
    public void SET4c() { z80._r.b|=0x10; z80._r.m=2; }
    public void SET4d() { z80._r.b|=0x10; z80._r.m=2; }
    public void SET4e() { z80._r.b|=0x10; z80._r.m=2; }
    public void SET4h() { z80._r.b|=0x10; z80._r.m=2; }
    public void SET4l() { z80._r.b|=0x10; z80._r.m=2; }
    public void SET4a() { z80._r.b|=0x10; z80._r.m=2; }
    public void SET4m() { var i=mMU.rb((z80._r.h<<8)+z80._r.l); i|=0x10; mMU.wb((z80._r.h<<8)+z80._r.l,i); z80._r.m=4; }

    public void BIT5b() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.b&0x20)>0)?0:0x80; z80._r.m=2; }
    public void BIT5c() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.c&0x20)>0)?0:0x80; z80._r.m=2; }
    public void BIT5d() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.d&0x20)>0)?0:0x80; z80._r.m=2; }
    public void BIT5e() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.e&0x20)>0)?0:0x80; z80._r.m=2; }
    public void BIT5h() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.h&0x20)>0)?0:0x80; z80._r.m=2; }
    public void BIT5l() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.l&0x20)>0)?0:0x80; z80._r.m=2; }
    public void BIT5a() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.a&0x20)>0)?0:0x80; z80._r.m=2; }
    public void BIT5m() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((mMU.rb((z80._r.h<<8)+z80._r.l)&0x20)>0)?0:0x80; z80._r.m=3; }

    public void RES5b() { z80._r.b&=0xDF; z80._r.m=2; }
    public void RES5c() { z80._r.c&=0xDF; z80._r.m=2; }
    public void RES5d() { z80._r.d&=0xDF; z80._r.m=2; }
    public void RES5e() { z80._r.e&=0xDF; z80._r.m=2; }
    public void RES5h() { z80._r.h&=0xDF; z80._r.m=2; }
    public void RES5l() { z80._r.l&=0xDF; z80._r.m=2; }
    public void RES5a() { z80._r.a&=0xDF; z80._r.m=2; }
    public void RES5m() { var i=mMU.rb((z80._r.h<<8)+z80._r.l); i&=0xDF; mMU.wb((z80._r.h<<8)+z80._r.l,i); z80._r.m=4; }

    public void SET5b() { z80._r.b|=0x20; z80._r.m=2; }
    public void SET5c() { z80._r.b|=0x20; z80._r.m=2; }
    public void SET5d() { z80._r.b|=0x20; z80._r.m=2; }
    public void SET5e() { z80._r.b|=0x20; z80._r.m=2; }
    public void SET5h() { z80._r.b|=0x20; z80._r.m=2; }
    public void SET5l() { z80._r.b|=0x20; z80._r.m=2; }
    public void SET5a() { z80._r.b|=0x20; z80._r.m=2; }
    public void SET5m() { var i=mMU.rb((z80._r.h<<8)+z80._r.l); i|=0x20; mMU.wb((z80._r.h<<8)+z80._r.l,i); z80._r.m=4; }

    public void BIT6b() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.b&0x40)>0)?0:0x80; z80._r.m=2; }
    public void BIT6c() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.c&0x40)>0)?0:0x80; z80._r.m=2; }
    public void BIT6d() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.d&0x40)>0)?0:0x80; z80._r.m=2; }
    public void BIT6e() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.e&0x40)>0)?0:0x80; z80._r.m=2; }
    public void BIT6h() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.h&0x40)>0)?0:0x80; z80._r.m=2; }
    public void BIT6l() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.l&0x40)>0)?0:0x80; z80._r.m=2; }
    public void BIT6a() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.a&0x40)>0)?0:0x80; z80._r.m=2; }
    public void BIT6m() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((mMU.rb((z80._r.h<<8)+z80._r.l)&0x40)>0)?0:0x80; z80._r.m=3; }

    public void RES6b() { z80._r.b&=0xBF; z80._r.m=2; }
    public void RES6c() { z80._r.c&=0xBF; z80._r.m=2; }
    public void RES6d() { z80._r.d&=0xBF; z80._r.m=2; }
    public void RES6e() { z80._r.e&=0xBF; z80._r.m=2; }
    public void RES6h() { z80._r.h&=0xBF; z80._r.m=2; }
    public void RES6l() { z80._r.l&=0xBF; z80._r.m=2; }
    public void RES6a() { z80._r.a&=0xBF; z80._r.m=2; }
    public void RES6m() { var i=mMU.rb((z80._r.h<<8)+z80._r.l); i&=0xBF; mMU.wb((z80._r.h<<8)+z80._r.l,i); z80._r.m=4; }

    public void SET6b() { z80._r.b|=0x40; z80._r.m=2; }
    public void SET6c() { z80._r.b|=0x40; z80._r.m=2; }
    public void SET6d() { z80._r.b|=0x40; z80._r.m=2; }
    public void SET6e() { z80._r.b|=0x40; z80._r.m=2; }
    public void SET6h() { z80._r.b|=0x40; z80._r.m=2; }
    public void SET6l() { z80._r.b|=0x40; z80._r.m=2; }
    public void SET6a() { z80._r.b|=0x40; z80._r.m=2; }
    public void SET6m() { var i=mMU.rb((z80._r.h<<8)+z80._r.l); i|=0x40; mMU.wb((z80._r.h<<8)+z80._r.l,i); z80._r.m=4; }

    public void BIT7b() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.b&0x80)>0)?0:0x80; z80._r.m=2; }
    public void BIT7c() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.c&0x80)>0)?0:0x80; z80._r.m=2; }
    public void BIT7d() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.d&0x80)>0)?0:0x80; z80._r.m=2; }
    public void BIT7e() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.e&0x80)>0)?0:0x80; z80._r.m=2; }
    public void BIT7h() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.h&0x80)>0)?0:0x80; z80._r.m=2; }
    public void BIT7l() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.l&0x80)>0)?0:0x80; z80._r.m=2; }
    public void BIT7a() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((z80._r.a&0x80)>0)?0:0x80; z80._r.m=2; }
    public void BIT7m() { z80._r.f&=0x1F; z80._r.f|=0x20; z80._r.f=((mMU.rb((z80._r.h<<8)+z80._r.l)&0x80)>0)?0:0x80; z80._r.m=3; }

    public void RES7b() { z80._r.b&=0x7F; z80._r.m=2; }
    public void RES7c() { z80._r.c&=0x7F; z80._r.m=2; }
    public void RES7d() { z80._r.d&=0x7F; z80._r.m=2; }
    public void RES7e() { z80._r.e&=0x7F; z80._r.m=2; }
    public void RES7h() { z80._r.h&=0x7F; z80._r.m=2; }
    public void RES7l() { z80._r.l&=0x7F; z80._r.m=2; }
    public void RES7a() { z80._r.a&=0x7F; z80._r.m=2; }
    public void RES7m() { var i=mMU.rb((z80._r.h<<8)+z80._r.l); i&=0x7F; mMU.wb((z80._r.h<<8)+z80._r.l,i); z80._r.m=4; }

    public void SET7b() { z80._r.b|=0x80; z80._r.m=2; }
    public void SET7c() { z80._r.b|=0x80; z80._r.m=2; }
    public void SET7d() { z80._r.b|=0x80; z80._r.m=2; }
    public void SET7e() { z80._r.b|=0x80; z80._r.m=2; }
    public void SET7h() { z80._r.b|=0x80; z80._r.m=2; }
    public void SET7l() { z80._r.b|=0x80; z80._r.m=2; }
    public void SET7a() { z80._r.b|=0x80; z80._r.m=2; }
    public void SET7m() { var i=mMU.rb((z80._r.h<<8)+z80._r.l); i|=0x80; mMU.wb((z80._r.h<<8)+z80._r.l,i); z80._r.m=4; }

    public void RLA() { var ci=((z80._r.f&0x10)>0)?1:0; var co=((z80._r.a&0x80)>0)?0x10:0; z80._r.a=(z80._r.a<<1)+ci; z80._r.a&=255; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=1; }
    public void RLCA() { var ci=((z80._r.a&0x80)>0)?1:0; var co=((z80._r.a&0x80)>0)?0x10:0; z80._r.a=(z80._r.a<<1)+ci; z80._r.a&=255; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=1; }
    public void RRA() { var ci=((z80._r.f&0x10)>0)?0x80:0; var co=((z80._r.a&1)>0)?0x10:0; z80._r.a=(z80._r.a>>1)+ci; z80._r.a&=255; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=1; }
    public void RRCA() { var ci=((z80._r.a&1)>0)?0x80:0; var co=((z80._r.a&1)>0)?0x10:0; z80._r.a=(z80._r.a>>1)+ci; z80._r.a&=255; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=1; }

    public void RLr_b() { var ci=((z80._r.f&0x10)>0)?1:0; var co=((z80._r.b&0x80)>0)?0x10:0; z80._r.b=(z80._r.b<<1)+ci; z80._r.b&=255; z80._r.f=(z80._r.b>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void RLr_c() { var ci=((z80._r.f&0x10)>0)?1:0; var co=((z80._r.c&0x80)>0)?0x10:0; z80._r.c=(z80._r.c<<1)+ci; z80._r.c&=255; z80._r.f=(z80._r.c>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void RLr_d() { var ci=((z80._r.f&0x10)>0)?1:0; var co=((z80._r.d&0x80)>0)?0x10:0; z80._r.d=(z80._r.d<<1)+ci; z80._r.d&=255; z80._r.f=(z80._r.d>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void RLr_e() { var ci=((z80._r.f&0x10)>0)?1:0; var co=((z80._r.e&0x80)>0)?0x10:0; z80._r.e=(z80._r.e<<1)+ci; z80._r.e&=255; z80._r.f=(z80._r.e>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void RLr_h() { var ci=((z80._r.f&0x10)>0)?1:0; var co=((z80._r.h&0x80)>0)?0x10:0; z80._r.h=(z80._r.h<<1)+ci; z80._r.h&=255; z80._r.f=(z80._r.h>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void RLr_l() { var ci=((z80._r.f&0x10)>0)?1:0; var co=((z80._r.l&0x80)>0)?0x10:0; z80._r.l=(z80._r.l<<1)+ci; z80._r.l&=255; z80._r.f=(z80._r.l>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void RLr_a() { var ci=((z80._r.f&0x10)>0)?1:0; var co=((z80._r.a&0x80)>0)?0x10:0; z80._r.a=(z80._r.a<<1)+ci; z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void RLHL() { var i=mMU.rb((z80._r.h<<8)+z80._r.l); var ci=((z80._r.f&0x10)>0)?1:0; var co=((i&0x80)>0)?0x10:0; i=(i<<1)+ci; i&=255; z80._r.f=(i>0)?0:0x80; mMU.wb((z80._r.h<<8)+z80._r.l,i); z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=4; }

    public void RLCr_b() { var ci=((z80._r.b&0x80)>0)?1:0; var co=((z80._r.b&0x80)>0)?0x10:0; z80._r.b=(z80._r.b<<1)+ci; z80._r.b&=255; z80._r.f=(z80._r.b>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void RLCr_c() { var ci=((z80._r.c&0x80)>0)?1:0; var co=((z80._r.c&0x80)>0)?0x10:0; z80._r.c=(z80._r.c<<1)+ci; z80._r.c&=255; z80._r.f=(z80._r.c>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void RLCr_d() { var ci=((z80._r.d&0x80)>0)?1:0; var co=((z80._r.d&0x80)>0)?0x10:0; z80._r.d=(z80._r.d<<1)+ci; z80._r.d&=255; z80._r.f=(z80._r.d>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void RLCr_e() { var ci=((z80._r.e&0x80)>0)?1:0; var co=((z80._r.e&0x80)>0)?0x10:0; z80._r.e=(z80._r.e<<1)+ci; z80._r.e&=255; z80._r.f=(z80._r.e>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void RLCr_h() { var ci=((z80._r.h&0x80)>0)?1:0; var co=((z80._r.h&0x80)>0)?0x10:0; z80._r.h=(z80._r.h<<1)+ci; z80._r.h&=255; z80._r.f=(z80._r.h>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void RLCr_l() { var ci=((z80._r.l&0x80)>0)?1:0; var co=((z80._r.l&0x80)>0)?0x10:0; z80._r.l=(z80._r.l<<1)+ci; z80._r.l&=255; z80._r.f=(z80._r.l>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void RLCr_a() { var ci=((z80._r.a&0x80)>0)?1:0; var co=((z80._r.a&0x80)>0)?0x10:0; z80._r.a=(z80._r.a<<1)+ci; z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void RLCHL() { var i=mMU.rb((z80._r.h<<8)+z80._r.l); var ci=((i&0x80)>0)?1:0; var co=((i&0x80)>0)?0x10:0; i=(i<<1)+ci; i&=255; z80._r.f=(i>0)?0:0x80; mMU.wb((z80._r.h<<8)+z80._r.l,i); z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=4; }

    public void RRr_b() { var ci=((z80._r.f&0x10)>0)?0x80:0; var co=((z80._r.b&1)>0)?0x10:0; z80._r.b=(z80._r.b>>1)+ci; z80._r.b&=255; z80._r.f=(z80._r.b>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void RRr_c() { var ci=((z80._r.f&0x10)>0)?0x80:0; var co=((z80._r.c&1)>0)?0x10:0; z80._r.c=(z80._r.c>>1)+ci; z80._r.c&=255; z80._r.f=(z80._r.c>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void RRr_d() { var ci=((z80._r.f&0x10)>0)?0x80:0; var co=((z80._r.d&1)>0)?0x10:0; z80._r.d=(z80._r.d>>1)+ci; z80._r.d&=255; z80._r.f=(z80._r.d>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void RRr_e() { var ci=((z80._r.f&0x10)>0)?0x80:0; var co=((z80._r.e&1)>0)?0x10:0; z80._r.e=(z80._r.e>>1)+ci; z80._r.e&=255; z80._r.f=(z80._r.e>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void RRr_h() { var ci=((z80._r.f&0x10)>0)?0x80:0; var co=((z80._r.h&1)>0)?0x10:0; z80._r.h=(z80._r.h>>1)+ci; z80._r.h&=255; z80._r.f=(z80._r.h>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void RRr_l() { var ci=((z80._r.f&0x10)>0)?0x80:0; var co=((z80._r.l&1)>0)?0x10:0; z80._r.l=(z80._r.l>>1)+ci; z80._r.l&=255; z80._r.f=(z80._r.l>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void RRr_a() { var ci=((z80._r.f&0x10)>0)?0x80:0; var co=((z80._r.a&1)>0)?0x10:0; z80._r.a=(z80._r.a>>1)+ci; z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void RRHL() { var i=mMU.rb((z80._r.h<<8)+z80._r.l); var ci=((z80._r.f&0x10)>0)?0x80:0; var co=((i&1)>0)?0x10:0; i=(i>>1)+ci; i&=255; mMU.wb((z80._r.h<<8)+z80._r.l,i); z80._r.f=(i>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=4; }

    public void RRCr_b() { var ci=((z80._r.b&1)>0)?0x80:0; var co=((z80._r.b&1)>0)?0x10:0; z80._r.b=(z80._r.b>>1)+ci; z80._r.b&=255; z80._r.f=(z80._r.b>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void RRCr_c() { var ci=((z80._r.c&1)>0)?0x80:0; var co=((z80._r.c&1)>0)?0x10:0; z80._r.c=(z80._r.c>>1)+ci; z80._r.c&=255; z80._r.f=(z80._r.c>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void RRCr_d() { var ci=((z80._r.d&1)>0)?0x80:0; var co=((z80._r.d&1)>0)?0x10:0; z80._r.d=(z80._r.d>>1)+ci; z80._r.d&=255; z80._r.f=(z80._r.d>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void RRCr_e() { var ci=((z80._r.e&1)>0)?0x80:0; var co=((z80._r.e&1)>0)?0x10:0; z80._r.e=(z80._r.e>>1)+ci; z80._r.e&=255; z80._r.f=(z80._r.e>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void RRCr_h() { var ci=((z80._r.h&1)>0)?0x80:0; var co=((z80._r.h&1)>0)?0x10:0; z80._r.h=(z80._r.h>>1)+ci; z80._r.h&=255; z80._r.f=(z80._r.h>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void RRCr_l() { var ci=((z80._r.l&1)>0)?0x80:0; var co=((z80._r.l&1)>0)?0x10:0; z80._r.l=(z80._r.l>>1)+ci; z80._r.l&=255; z80._r.f=(z80._r.l>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void RRCr_a() { var ci=((z80._r.a&1)>0)?0x80:0; var co=((z80._r.a&1)>0)?0x10:0; z80._r.a=(z80._r.a>>1)+ci; z80._r.a&=255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void RRCHL() { var i=mMU.rb((z80._r.h<<8)+z80._r.l); var ci=((i&1)>0)?0x80:0; var co=((i&1)>0)?0x10:0; i=(i>>1)+ci; i&=255; mMU.wb((z80._r.h<<8)+z80._r.l,i); z80._r.f=(i>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=4; }

    public void SLAr_b() { var co=((z80._r.b&0x80)>0)?0x10:0; z80._r.b=(z80._r.b<<1)&255; z80._r.f=(z80._r.b>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void SLAr_c() { var co=((z80._r.c&0x80)>0)?0x10:0; z80._r.c=(z80._r.c<<1)&255; z80._r.f=(z80._r.c>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void SLAr_d() { var co=((z80._r.d&0x80)>0)?0x10:0; z80._r.d=(z80._r.d<<1)&255; z80._r.f=(z80._r.d>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void SLAr_e() { var co=((z80._r.e&0x80)>0)?0x10:0; z80._r.e=(z80._r.e<<1)&255; z80._r.f=(z80._r.e>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void SLAr_h() { var co=((z80._r.h&0x80)>0)?0x10:0; z80._r.h=(z80._r.h<<1)&255; z80._r.f=(z80._r.h>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void SLAr_l() { var co=((z80._r.l&0x80)>0)?0x10:0; z80._r.l=(z80._r.l<<1)&255; z80._r.f=(z80._r.l>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void SLAr_a() { var co=((z80._r.a&0x80)>0)?0x10:0; z80._r.a=(z80._r.a<<1)&255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }

    public void SLLr_b() { var co=((z80._r.b&0x80)>0)?0x10:0; z80._r.b=(z80._r.b<<1)&255+1; z80._r.f=(z80._r.b>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void SLLr_c() { var co=((z80._r.c&0x80)>0)?0x10:0; z80._r.c=(z80._r.c<<1)&255+1; z80._r.f=(z80._r.c>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void SLLr_d() { var co=((z80._r.d&0x80)>0)?0x10:0; z80._r.d=(z80._r.d<<1)&255+1; z80._r.f=(z80._r.d>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void SLLr_e() { var co=((z80._r.e&0x80)>0)?0x10:0; z80._r.e=(z80._r.e<<1)&255+1; z80._r.f=(z80._r.e>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void SLLr_h() { var co=((z80._r.h&0x80)>0)?0x10:0; z80._r.h=(z80._r.h<<1)&255+1; z80._r.f=(z80._r.h>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void SLLr_l() { var co=((z80._r.l&0x80)>0)?0x10:0; z80._r.l=(z80._r.l<<1)&255+1; z80._r.f=(z80._r.l>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void SLLr_a() { var co=((z80._r.a&0x80)>0)?0x10:0; z80._r.a=(z80._r.a<<1)&255+1; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }

    public void SRAr_b() { var ci=z80._r.b&0x80; var co=((z80._r.b&1)>0)?0x10:0; z80._r.b=((z80._r.b>>1)+ci)&255; z80._r.f=(z80._r.b>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void SRAr_c() { var ci=z80._r.c&0x80; var co=((z80._r.c&1)>0)?0x10:0; z80._r.c=((z80._r.c>>1)+ci)&255; z80._r.f=(z80._r.c>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void SRAr_d() { var ci=z80._r.d&0x80; var co=((z80._r.d&1)>0)?0x10:0; z80._r.d=((z80._r.d>>1)+ci)&255; z80._r.f=(z80._r.d>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void SRAr_e() { var ci=z80._r.e&0x80; var co=((z80._r.e&1)>0)?0x10:0; z80._r.e=((z80._r.e>>1)+ci)&255; z80._r.f=(z80._r.e>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void SRAr_h() { var ci=z80._r.h&0x80; var co=((z80._r.h&1)>0)?0x10:0; z80._r.h=((z80._r.h>>1)+ci)&255; z80._r.f=(z80._r.h>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void SRAr_l() { var ci=z80._r.l&0x80; var co=((z80._r.l&1)>0)?0x10:0; z80._r.l=((z80._r.l>>1)+ci)&255; z80._r.f=(z80._r.l>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void SRAr_a() { var ci=z80._r.a&0x80; var co=((z80._r.a&1)>0)?0x10:0; z80._r.a=((z80._r.a>>1)+ci)&255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }

    public void SRLr_b() { var co=((z80._r.b&1)>0)?0x10:0; z80._r.b=(z80._r.b>>1)&255; z80._r.f=(z80._r.b>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void SRLr_c() { var co=((z80._r.c&1)>0)?0x10:0; z80._r.c=(z80._r.c>>1)&255; z80._r.f=(z80._r.c>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void SRLr_d() { var co=((z80._r.d&1)>0)?0x10:0; z80._r.d=(z80._r.d>>1)&255; z80._r.f=(z80._r.d>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void SRLr_e() { var co=((z80._r.e&1)>0)?0x10:0; z80._r.e=(z80._r.e>>1)&255; z80._r.f=(z80._r.e>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void SRLr_h() { var co=((z80._r.h&1)>0)?0x10:0; z80._r.h=(z80._r.h>>1)&255; z80._r.f=(z80._r.h>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void SRLr_l() { var co=((z80._r.l&1)>0)?0x10:0; z80._r.l=(z80._r.l>>1)&255; z80._r.f=(z80._r.l>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }
    public void SRLr_a() { var co=((z80._r.a&1)>0)?0x10:0; z80._r.a=(z80._r.a>>1)&255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.f=(z80._r.f&0xEF)+co; z80._r.m=2; }

    public void CPL() { z80._r.a ^= 255; z80._r.f=(z80._r.a>0)?0:0x80; z80._r.m=1; }
    public void NEG() { z80._r.a=0-z80._r.a; z80._r.f=(z80._r.a<0)?0x10:0; z80._r.a&=255; if(!(z80._r.a>0)) z80._r.f|=0x80; z80._r.m=2; }

    public void CCF() { var ci=((z80._r.f&0x10)>0)?0:0x10; z80._r.f=(z80._r.f&0xEF)+ci; z80._r.m=1; }
    public void SCF() { z80._r.f|=0x10; z80._r.m=1; }

    /*--- Stack ---*/
    public void PUSHBC() { z80._r.sp--; mMU.wb(z80._r.sp,z80._r.b); z80._r.sp--; mMU.wb(z80._r.sp,z80._r.c); z80._r.m=3; }
    public void PUSHDE() { z80._r.sp--; mMU.wb(z80._r.sp,z80._r.d); z80._r.sp--; mMU.wb(z80._r.sp,z80._r.e); z80._r.m=3; }
    public void PUSHHL() { z80._r.sp--; mMU.wb(z80._r.sp,z80._r.h); z80._r.sp--; mMU.wb(z80._r.sp,z80._r.l); z80._r.m=3; }
    public void PUSHAF() { z80._r.sp--; mMU.wb(z80._r.sp,z80._r.a); z80._r.sp--; mMU.wb(z80._r.sp,z80._r.f); z80._r.m=3; }

    public void POPBC() { z80._r.c=mMU.rb(z80._r.sp); z80._r.sp++; z80._r.b=mMU.rb(z80._r.sp); z80._r.sp++; z80._r.m=3; }
    public void POPDE() { z80._r.e=mMU.rb(z80._r.sp); z80._r.sp++; z80._r.d=mMU.rb(z80._r.sp); z80._r.sp++; z80._r.m=3; }
    public void POPHL() { z80._r.l=mMU.rb(z80._r.sp); z80._r.sp++; z80._r.h=mMU.rb(z80._r.sp); z80._r.sp++; z80._r.m=3; }
    public void POPAF() { z80._r.f=mMU.rb(z80._r.sp); z80._r.sp++; z80._r.a=mMU.rb(z80._r.sp); z80._r.sp++; z80._r.m=3; }

    /*--- Jump ---*/
    public void JPnn() { z80._r.pc = mMU.rw(z80._r.pc); z80._r.m=3; }
    public void JPHL() { z80._r.pc=(z80._r.h<<8)+z80._r.l; z80._r.m=1; }
    public void JPNZnn() { z80._r.m=3; if((z80._r.f&0x80)==0x00) { z80._r.pc=mMU.rw(z80._r.pc); z80._r.m++; } else z80._r.pc+=2; }
    public void JPZnn()  { z80._r.m=3; if((z80._r.f&0x80)==0x80) { z80._r.pc=mMU.rw(z80._r.pc); z80._r.m++; } else z80._r.pc+=2; }
    public void JPNCnn() { z80._r.m=3; if((z80._r.f&0x10)==0x00) { z80._r.pc=mMU.rw(z80._r.pc); z80._r.m++; } else z80._r.pc+=2; }
    public void JPCnn()  { z80._r.m=3; if((z80._r.f&0x10)==0x10) { z80._r.pc=mMU.rw(z80._r.pc); z80._r.m++; } else z80._r.pc+=2; }

    public void JRn() { var i=mMU.rb(z80._r.pc); if(i>127) i=-((~i+1)&255); z80._r.pc++; z80._r.m=2; z80._r.pc+=i; z80._r.m++; }
    public void JRNZn() { var i=mMU.rb(z80._r.pc); if(i>127) i=-((~i+1)&255); z80._r.pc++; z80._r.m=2; if((z80._r.f&0x80)==0x00) { z80._r.pc+=i; z80._r.m++; } }
    public void JRZn()  { var i=mMU.rb(z80._r.pc); if(i>127) i=-((~i+1)&255); z80._r.pc++; z80._r.m=2; if((z80._r.f&0x80)==0x80) { z80._r.pc+=i; z80._r.m++; } }
    public void JRNCn() { var i=mMU.rb(z80._r.pc); if(i>127) i=-((~i+1)&255); z80._r.pc++; z80._r.m=2; if((z80._r.f&0x10)==0x00) { z80._r.pc+=i; z80._r.m++; } }
    public void JRCn()  { var i=mMU.rb(z80._r.pc); if(i>127) i=-((~i+1)&255); z80._r.pc++; z80._r.m=2; if((z80._r.f&0x10)==0x10) { z80._r.pc+=i; z80._r.m++; } }

    public void DJNZn() { var i=mMU.rb(z80._r.pc); if(i>127) i=-((~i+1)&255); z80._r.pc++; z80._r.m=2; z80._r.b--; if(z80._r.b>0) { z80._r.pc+=i; z80._r.m++; } }

    public void CALLnn() { z80._r.sp-=2; mMU.ww(z80._r.sp,z80._r.pc+2); z80._r.pc=mMU.rw(z80._r.pc); z80._r.m=5; }
    public void CALLNZnn() { z80._r.m=3; if((z80._r.f&0x80)==0x00) { z80._r.sp-=2; mMU.ww(z80._r.sp,z80._r.pc+2); z80._r.pc=mMU.rw(z80._r.pc); z80._r.m+=2; } else z80._r.pc+=2; }
    public void CALLZnn() { z80._r.m=3; if((z80._r.f&0x80)==0x80) { z80._r.sp-=2; mMU.ww(z80._r.sp,z80._r.pc+2); z80._r.pc=mMU.rw(z80._r.pc); z80._r.m+=2; } else z80._r.pc+=2; }
    public void CALLNCnn() { z80._r.m=3; if((z80._r.f&0x10)==0x00) { z80._r.sp-=2; mMU.ww(z80._r.sp,z80._r.pc+2); z80._r.pc=mMU.rw(z80._r.pc); z80._r.m+=2; } else z80._r.pc+=2; }
    public void CALLCnn() { z80._r.m=3; if((z80._r.f&0x10)==0x10) { z80._r.sp-=2; mMU.ww(z80._r.sp,z80._r.pc+2); z80._r.pc=mMU.rw(z80._r.pc); z80._r.m+=2; } else z80._r.pc+=2; }

    public void RET() { z80._r.pc=mMU.rw(z80._r.sp); z80._r.sp+=2; z80._r.m=3; }
    public void RETI() { z80._r.ime=1; z80._ops.rrs(); z80._r.pc=mMU.rw(z80._r.sp); z80._r.sp+=2; z80._r.m=3; }
    public void RETNZ() { z80._r.m=1; if((z80._r.f&0x80)==0x00) { z80._r.pc=mMU.rw(z80._r.sp); z80._r.sp+=2; z80._r.m+=2; } }
    public void RETZ() { z80._r.m=1; if((z80._r.f&0x80)==0x80) { z80._r.pc=mMU.rw(z80._r.sp); z80._r.sp+=2; z80._r.m+=2; } }
    public void RETNC() { z80._r.m=1; if((z80._r.f&0x10)==0x00) { z80._r.pc=mMU.rw(z80._r.sp); z80._r.sp+=2; z80._r.m+=2; } }
    public void RETC() { z80._r.m=1; if((z80._r.f&0x10)==0x10) { z80._r.pc=mMU.rw(z80._r.sp); z80._r.sp+=2; z80._r.m+=2; } }

    public void RST00() { z80._ops.rsv(); z80._r.sp-=2; mMU.ww(z80._r.sp,z80._r.pc); z80._r.pc=0x00; z80._r.m=3; }
    public void RST08() { z80._ops.rsv(); z80._r.sp-=2; mMU.ww(z80._r.sp,z80._r.pc); z80._r.pc=0x08; z80._r.m=3; }
    public void RST10() { z80._ops.rsv(); z80._r.sp-=2; mMU.ww(z80._r.sp,z80._r.pc); z80._r.pc=0x10; z80._r.m=3; }
    public void RST18() { z80._ops.rsv(); z80._r.sp-=2; mMU.ww(z80._r.sp,z80._r.pc); z80._r.pc=0x18; z80._r.m=3; }
    public void RST20() { z80._ops.rsv(); z80._r.sp-=2; mMU.ww(z80._r.sp,z80._r.pc); z80._r.pc=0x20; z80._r.m=3; }
    public void RST28() { z80._ops.rsv(); z80._r.sp-=2; mMU.ww(z80._r.sp,z80._r.pc); z80._r.pc=0x28; z80._r.m=3; }
    public void RST30() { z80._ops.rsv(); z80._r.sp-=2; mMU.ww(z80._r.sp,z80._r.pc); z80._r.pc=0x30; z80._r.m=3; }
    public void RST38() { z80._ops.rsv(); z80._r.sp-=2; mMU.ww(z80._r.sp,z80._r.pc); z80._r.pc=0x38; z80._r.m=3; }
    public void RST40() { z80._ops.rsv(); z80._r.sp-=2; mMU.ww(z80._r.sp,z80._r.pc); z80._r.pc=0x40; z80._r.m=3; }
    public void RST48() { z80._ops.rsv(); z80._r.sp-=2; mMU.ww(z80._r.sp,z80._r.pc); z80._r.pc=0x48; z80._r.m=3; }
    public void RST50() { z80._ops.rsv(); z80._r.sp-=2; mMU.ww(z80._r.sp,z80._r.pc); z80._r.pc=0x50; z80._r.m=3; }
    public void RST58() { z80._ops.rsv(); z80._r.sp-=2; mMU.ww(z80._r.sp,z80._r.pc); z80._r.pc=0x58; z80._r.m=3; }
    public void RST60() { z80._ops.rsv(); z80._r.sp-=2; mMU.ww(z80._r.sp,z80._r.pc); z80._r.pc=0x60; z80._r.m=3; }

    public void NOP() { z80._r.m=1; }
    public void HALT() { z80._halt=1; z80._r.m=1; }

    public void DI() { z80._r.ime=0; z80._r.m=1; }
    public void EI() { z80._r.ime=1; z80._r.m=1; }

    /*--- Helper functions ---*/
    public void rsv() {
      z80._rsv.a = z80._r.a; z80._rsv.b = z80._r.b;
      z80._rsv.c = z80._r.c; z80._rsv.d = z80._r.d;
      z80._rsv.e = z80._r.e; z80._rsv.f = z80._r.f;
      z80._rsv.h = z80._r.h; z80._rsv.l = z80._r.l;
    }

    public void rrs() {
      z80._r.a = z80._rsv.a; z80._r.b = z80._rsv.b;
      z80._r.c = z80._rsv.c; z80._r.d = z80._rsv.d;
      z80._r.e = z80._rsv.e; z80._r.f = z80._rsv.f;
      z80._r.h = z80._rsv.h; z80._r.l = z80._rsv.l;
    }

    public void MAPcb() {
      var i=mMU.rb(z80._r.pc); z80._r.pc++;
      z80._r.pc &= 65535;
      if(z80._cbmap[i] != null) z80._cbmap[i]();
      else throw new Exception("Z80: MAPcb i = " + i);
    }

    public void XX() {
      /*Undefined map entry*/
      var opc = z80._r.pc-1;
      throw new Exception("Z80: Unimplemented instruction at $"+opc+", stopping.");
      z80._stop=1;
    }
};


class Z80 
{
  public Z80r _r = new Z80r();

  public Z80rsv _rsv = new Z80rsv();

  public Z80clock _clock = new Z80clock();

  public int _halt = 0;
  public int _stop = 0;
  private MMU mMU;
  
  public Z80()
  {    

    this._map = new Action[256]{
      // 00
      this._ops.NOP,		this._ops.LDBCnn,	this._ops.LDBCmA,	this._ops.INCBC,
      this._ops.INCr_b,	this._ops.DECr_b,	this._ops.LDrn_b,	this._ops.RLCA,
      this._ops.LDmmSP,	this._ops.ADDHLBC,	this._ops.LDABCm,	this._ops.DECBC,
      this._ops.INCr_c,	this._ops.DECr_c,	this._ops.LDrn_c,	this._ops.RRCA,
      // 10
      this._ops.DJNZn,	this._ops.LDDEnn,	this._ops.LDDEmA,	this._ops.INCDE,
      this._ops.INCr_d,	this._ops.DECr_d,	this._ops.LDrn_d,	this._ops.RLA,
      this._ops.JRn,		this._ops.ADDHLDE,	this._ops.LDADEm,	this._ops.DECDE,
      this._ops.INCr_e,	this._ops.DECr_e,	this._ops.LDrn_e,	this._ops.RRA,
      // 20
      this._ops.JRNZn,	this._ops.LDHLnn,	this._ops.LDHLIA,	this._ops.INCHL,
      this._ops.INCr_h,	this._ops.DECr_h,	this._ops.LDrn_h,	this._ops.DAA,
      this._ops.JRZn,	this._ops.ADDHLHL,	this._ops.LDAHLI,	this._ops.DECHL,
      this._ops.INCr_l,	this._ops.DECr_l,	this._ops.LDrn_l,	this._ops.CPL,
      // 30
      this._ops.JRNCn,	this._ops.LDSPnn,	this._ops.LDHLDA,	this._ops.INCSP,
      this._ops.INCHLm,	this._ops.DECHLm,	this._ops.LDHLmn,	this._ops.SCF,
      this._ops.JRCn,	this._ops.ADDHLSP,	this._ops.LDAHLD,	this._ops.DECSP,
      this._ops.INCr_a,	this._ops.DECr_a,	this._ops.LDrn_a,	this._ops.CCF,
      // 40
      this._ops.LDrr_bb,	this._ops.LDrr_bc,	this._ops.LDrr_bd,	this._ops.LDrr_be,
      this._ops.LDrr_bh,	this._ops.LDrr_bl,	this._ops.LDrHLm_b,	this._ops.LDrr_ba,
      this._ops.LDrr_cb,	this._ops.LDrr_cc,	this._ops.LDrr_cd,	this._ops.LDrr_ce,
      this._ops.LDrr_ch,	this._ops.LDrr_cl,	this._ops.LDrHLm_c,	this._ops.LDrr_ca,
      // 50
      this._ops.LDrr_db,	this._ops.LDrr_dc,	this._ops.LDrr_dd,	this._ops.LDrr_de,
      this._ops.LDrr_dh,	this._ops.LDrr_dl,	this._ops.LDrHLm_d,	this._ops.LDrr_da,
      this._ops.LDrr_eb,	this._ops.LDrr_ec,	this._ops.LDrr_ed,	this._ops.LDrr_ee,
      this._ops.LDrr_eh,	this._ops.LDrr_el,	this._ops.LDrHLm_e,	this._ops.LDrr_ea,
      // 60
      this._ops.LDrr_hb,	this._ops.LDrr_hc,	this._ops.LDrr_hd,	this._ops.LDrr_he,
      this._ops.LDrr_hh,	this._ops.LDrr_hl,	this._ops.LDrHLm_h,	this._ops.LDrr_ha,
      this._ops.LDrr_lb,	this._ops.LDrr_lc,	this._ops.LDrr_ld,	this._ops.LDrr_le,
      this._ops.LDrr_lh,	this._ops.LDrr_ll,	this._ops.LDrHLm_l,	this._ops.LDrr_la,
      // 70
      this._ops.LDHLmr_b,	this._ops.LDHLmr_c,	this._ops.LDHLmr_d,	this._ops.LDHLmr_e,
      this._ops.LDHLmr_h,	this._ops.LDHLmr_l,	this._ops.HALT,		this._ops.LDHLmr_a,
      this._ops.LDrr_ab,	this._ops.LDrr_ac,	this._ops.LDrr_ad,	this._ops.LDrr_ae,
      this._ops.LDrr_ah,	this._ops.LDrr_al,	this._ops.LDrHLm_a,	this._ops.LDrr_aa,
      // 80
      this._ops.ADDr_b,	this._ops.ADDr_c,	this._ops.ADDr_d,	this._ops.ADDr_e,
      this._ops.ADDr_h,	this._ops.ADDr_l,	this._ops.ADDHL,		this._ops.ADDr_a,
      this._ops.ADCr_b,	this._ops.ADCr_c,	this._ops.ADCr_d,	this._ops.ADCr_e,
      this._ops.ADCr_h,	this._ops.ADCr_l,	this._ops.ADCHL,		this._ops.ADCr_a,
      // 90
      this._ops.SUBr_b,	this._ops.SUBr_c,	this._ops.SUBr_d,	this._ops.SUBr_e,
      this._ops.SUBr_h,	this._ops.SUBr_l,	this._ops.SUBHL,		this._ops.SUBr_a,
      this._ops.SBCr_b,	this._ops.SBCr_c,	this._ops.SBCr_d,	this._ops.SBCr_e,
      this._ops.SBCr_h,	this._ops.SBCr_l,	this._ops.SBCHL,		this._ops.SBCr_a,
      // A0
      this._ops.ANDr_b,	this._ops.ANDr_c,	this._ops.ANDr_d,	this._ops.ANDr_e,
      this._ops.ANDr_h,	this._ops.ANDr_l,	this._ops.ANDHL,		this._ops.ANDr_a,
      this._ops.XORr_b,	this._ops.XORr_c,	this._ops.XORr_d,	this._ops.XORr_e,
      this._ops.XORr_h,	this._ops.XORr_l,	this._ops.XORHL,		this._ops.XORr_a,
      // B0
      this._ops.ORr_b,	this._ops.ORr_c,		this._ops.ORr_d,		this._ops.ORr_e,
      this._ops.ORr_h,	this._ops.ORr_l,		this._ops.ORHL,		this._ops.ORr_a,
      this._ops.CPr_b,	this._ops.CPr_c,		this._ops.CPr_d,		this._ops.CPr_e,
      this._ops.CPr_h,	this._ops.CPr_l,		this._ops.CPHL,		this._ops.CPr_a,
      // C0
      this._ops.RETNZ,	this._ops.POPBC,		this._ops.JPNZnn,	this._ops.JPnn,
      this._ops.CALLNZnn,	this._ops.PUSHBC,	this._ops.ADDn,		this._ops.RST00,
      this._ops.RETZ,	this._ops.RET,		this._ops.JPZnn,		this._ops.MAPcb,
      this._ops.CALLZnn,	this._ops.CALLnn,	this._ops.ADCn,		this._ops.RST08,
      // D0
      this._ops.RETNC,	this._ops.POPDE,		this._ops.JPNCnn,	this._ops.XX,
      this._ops.CALLNCnn,	this._ops.PUSHDE,	this._ops.SUBn,		this._ops.RST10,
      this._ops.RETC,	this._ops.RETI,		this._ops.JPCnn,		this._ops.XX,
      this._ops.CALLCnn,	this._ops.XX,		this._ops.SBCn,		this._ops.RST18,
      // E0
      this._ops.LDIOnA,	this._ops.POPHL,		this._ops.LDIOCA,	this._ops.XX,
      this._ops.XX,		this._ops.PUSHHL,	this._ops.ANDn,		this._ops.RST20,
      this._ops.ADDSPn,	this._ops.JPHL,		this._ops.LDmmA,		this._ops.XX,
      this._ops.XX,		this._ops.XX,		this._ops.XORn,		this._ops.RST28,
      // F0
      this._ops.LDAIOn,	this._ops.POPAF,		this._ops.LDAIOC,	this._ops.DI,
      this._ops.XX,		this._ops.PUSHAF,	this._ops.ORn,		this._ops.RST30,
      this._ops.LDHLSPn,	this._ops.XX,		this._ops.LDAmm,		this._ops.EI,
      this._ops.XX,		this._ops.XX,		this._ops.CPn,		this._ops.RST38
    };

    this._cbmap = new Action[256]{
      // CB00
      this._ops.RLCr_b,	this._ops.RLCr_c,	this._ops.RLCr_d,	this._ops.RLCr_e,
      this._ops.RLCr_h,	this._ops.RLCr_l,	this._ops.RLCHL,		this._ops.RLCr_a,
      this._ops.RRCr_b,	this._ops.RRCr_c,	this._ops.RRCr_d,	this._ops.RRCr_e,
      this._ops.RRCr_h,	this._ops.RRCr_l,	this._ops.RRCHL,		this._ops.RRCr_a,
      // CB10
      this._ops.RLr_b,	this._ops.RLr_c,		this._ops.RLr_d,		this._ops.RLr_e,
      this._ops.RLr_h,	this._ops.RLr_l,		this._ops.RLHL,		this._ops.RLr_a,
      this._ops.RRr_b,	this._ops.RRr_c,		this._ops.RRr_d,		this._ops.RRr_e,
      this._ops.RRr_h,	this._ops.RRr_l,		this._ops.RRHL,		this._ops.RRr_a,
      // CB20
      this._ops.SLAr_b,	this._ops.SLAr_c,	this._ops.SLAr_d,	this._ops.SLAr_e,
      this._ops.SLAr_h,	this._ops.SLAr_l,	this._ops.XX,		this._ops.SLAr_a,
      this._ops.SRAr_b,	this._ops.SRAr_c,	this._ops.SRAr_d,	this._ops.SRAr_e,
      this._ops.SRAr_h,	this._ops.SRAr_l,	this._ops.XX,		this._ops.SRAr_a,
      // CB30
      this._ops.SWAPr_b,	this._ops.SWAPr_c,	this._ops.SWAPr_d,	this._ops.SWAPr_e,
      this._ops.SWAPr_h,	this._ops.SWAPr_l,	this._ops.XX,		this._ops.SWAPr_a,
      this._ops.SRLr_b,	this._ops.SRLr_c,	this._ops.SRLr_d,	this._ops.SRLr_e,
      this._ops.SRLr_h,	this._ops.SRLr_l,	this._ops.XX,		this._ops.SRLr_a,
      // CB40
      this._ops.BIT0b,	this._ops.BIT0c,		this._ops.BIT0d,		this._ops.BIT0e,
      this._ops.BIT0h,	this._ops.BIT0l,		this._ops.BIT0m,		this._ops.BIT0a,
      this._ops.BIT1b,	this._ops.BIT1c,		this._ops.BIT1d,		this._ops.BIT1e,
      this._ops.BIT1h,	this._ops.BIT1l,		this._ops.BIT1m,		this._ops.BIT1a,
      // CB50
      this._ops.BIT2b,	this._ops.BIT2c,		this._ops.BIT2d,		this._ops.BIT2e,
      this._ops.BIT2h,	this._ops.BIT2l,		this._ops.BIT2m,		this._ops.BIT2a,
      this._ops.BIT3b,	this._ops.BIT3c,		this._ops.BIT3d,		this._ops.BIT3e,
      this._ops.BIT3h,	this._ops.BIT3l,		this._ops.BIT3m,		this._ops.BIT3a,
      // CB60
      this._ops.BIT4b,	this._ops.BIT4c,		this._ops.BIT4d,		this._ops.BIT4e,
      this._ops.BIT4h,	this._ops.BIT4l,		this._ops.BIT4m,		this._ops.BIT4a,
      this._ops.BIT5b,	this._ops.BIT5c,		this._ops.BIT5d,		this._ops.BIT5e,
      this._ops.BIT5h,	this._ops.BIT5l,		this._ops.BIT5m,		this._ops.BIT5a,
      // CB70
      this._ops.BIT6b,	this._ops.BIT6c,		this._ops.BIT6d,		this._ops.BIT6e,
      this._ops.BIT6h,	this._ops.BIT6l,		this._ops.BIT6m,		this._ops.BIT6a,
      this._ops.BIT7b,	this._ops.BIT7c,		this._ops.BIT7d,		this._ops.BIT7e,
      this._ops.BIT7h,	this._ops.BIT7l,		this._ops.BIT7m,		this._ops.BIT7a,
      // CB80
      this._ops.RES0b,	this._ops.RES0c,		this._ops.RES0d,		this._ops.RES0e,
      this._ops.RES0h,	this._ops.RES0l,		this._ops.RES0m,		this._ops.RES0a,
      this._ops.RES1b,	this._ops.RES1c,		this._ops.RES1d,		this._ops.RES1e,
      this._ops.RES1h,	this._ops.RES1l,		this._ops.RES1m,		this._ops.RES1a,
      // CB90
      this._ops.RES2b,	this._ops.RES2c,		this._ops.RES2d,		this._ops.RES2e,
      this._ops.RES2h,	this._ops.RES2l,		this._ops.RES2m,		this._ops.RES2a,
      this._ops.RES3b,	this._ops.RES3c,		this._ops.RES3d,		this._ops.RES3e,
      this._ops.RES3h,	this._ops.RES3l,		this._ops.RES3m,		this._ops.RES3a,
      // CBA0
      this._ops.RES4b,	this._ops.RES4c,		this._ops.RES4d,		this._ops.RES4e,
      this._ops.RES4h,	this._ops.RES4l,		this._ops.RES4m,		this._ops.RES4a,
      this._ops.RES5b,	this._ops.RES5c,		this._ops.RES5d,		this._ops.RES5e,
      this._ops.RES5h,	this._ops.RES5l,		this._ops.RES5m,		this._ops.RES5a,
      // CBB0
      this._ops.RES6b,	this._ops.RES6c,		this._ops.RES6d,		this._ops.RES6e,
      this._ops.RES6h,	this._ops.RES6l,		this._ops.RES6m,		this._ops.RES6a,
      this._ops.RES7b,	this._ops.RES7c,		this._ops.RES7d,		this._ops.RES7e,
      this._ops.RES7h,	this._ops.RES7l,		this._ops.RES7m,		this._ops.RES7a,
      // CBC0
      this._ops.SET0b,	this._ops.SET0c,		this._ops.SET0d,		this._ops.SET0e,
      this._ops.SET0h,	this._ops.SET0l,		this._ops.SET0m,		this._ops.SET0a,
      this._ops.SET1b,	this._ops.SET1c,		this._ops.SET1d,		this._ops.SET1e,
      this._ops.SET1h,	this._ops.SET1l,		this._ops.SET1m,		this._ops.SET1a,
      // CBD0
      this._ops.SET2b,	this._ops.SET2c,		this._ops.SET2d,		this._ops.SET2e,
      this._ops.SET2h,	this._ops.SET2l,		this._ops.SET2m,		this._ops.SET2a,
      this._ops.SET3b,	this._ops.SET3c,		this._ops.SET3d,		this._ops.SET3e,
      this._ops.SET3h,	this._ops.SET3l,		this._ops.SET3m,		this._ops.SET3a,
      // CBE0
      this._ops.SET4b,	this._ops.SET4c,		this._ops.SET4d,		this._ops.SET4e,
      this._ops.SET4h,	this._ops.SET4l,		this._ops.SET4m,		this._ops.SET4a,
      this._ops.SET5b,	this._ops.SET5c,		this._ops.SET5d,		this._ops.SET5e,
      this._ops.SET5h,	this._ops.SET5l,		this._ops.SET5m,		this._ops.SET5a,
      // CBF0
      this._ops.SET6b,	this._ops.SET6c,		this._ops.SET6d,		this._ops.SET6e,
      this._ops.SET6h,	this._ops.SET6l,		this._ops.SET6m,		this._ops.SET6a,
      this._ops.SET7b,	this._ops.SET7c,		this._ops.SET7d,		this._ops.SET7e,
      this._ops.SET7h,	this._ops.SET7l,		this._ops.SET7m,		this._ops.SET7a,
    };
  }
  public void reset(MMU mMU) {
    this.mMU = mMU;
    this._ops = new Z80ops(this, mMU);
    this._r.a=0; this._r.b=0; this._r.c=0; this._r.d=0; this._r.e=0; this._r.h=0; this._r.l=0; this._r.f=0;
    this._r.sp=0; this._r.pc=0; this._r.i=0; this._r.r=0;
    this._r.m=0;
    this._halt=0; this._stop=0;
    this._clock.m=0;
    this._r.ime=1;
    //Echo("Z80: Reset.");
  }

  public void exec() {
    this._r.r = (this._r.r+1) & 127;
    this._map[mMU.rb(this._r.pc++)]();
    this._r.pc &= 65535;
    this._clock.m += this._r.m;
  }

  public Z80ops _ops;

  public Action[] _map;
  public Action[] _cbmap;
}



