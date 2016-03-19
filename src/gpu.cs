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
