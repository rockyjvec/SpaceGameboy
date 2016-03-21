class GPUData //: IComparable<GPUData>
{
    public int y, x, tile, palette, yflip, xflip, prio, num;
    public GPUData(int num)
    {
        this.num = num;
        y = -16;
        x = -8;
        tile = 0;
        palette = 0;
        yflip = 0;
        xflip = 0;
        prio = 0;
        num = 0;
    }
}

class GPUPalette {
    public byte[] bg = new byte[4];
    public byte[] obj0 = new byte[4];
    public byte[] obj1 = new byte[4];
}

class GPU 
{
  public char[] data = new char[161*144];
  public byte[] _vram = new byte[8192];
  public byte[] _oam = new byte[160];
  byte[] _reg = new byte[256];
  byte[][][] _tilemap = new byte[512][][];
  GPUData[] _objdata = new GPUData[40];
//  List<GPUData> _objdatasorted;
  GPUPalette _palette = new GPUPalette();
  byte[] _scanrow = new byte[160];

  byte _curline = 0;
  int _curscan = 0;
  int _linemode = 0;
  int _modeclocks = 0;

  byte _yscrl = 0;
  byte _xscrl = 0;
  byte _raster = 0;
  
  int _lcdon = 0;
  int _bgon = 0;
  int _objon = 0;

  int _objsize = 0;

  int _bgtilebase = 0x0000;
  int _bgmapbase = 0x1800;
  
  long lastRender = 0, currentTime;
  
  char[] colors = new char[256];
  
  IMyTextPanel screen;
  
  Z80 z80;
  MMU mMU;
  
  public GPU(IMyTextPanel screen)
  {
//      SpaceGameboy.Echo("GPU Constructor");
      this.screen = screen;
      for(int n = 0; n < 256; n++)
      {
            if(n == 0)
            {
                colors[n] = '\uE00F';                
            }
            else if (n == 96)
            {
                colors[n] = '\uE00E';                
            }
            else if(n == 192)
            {
                colors[n] = '\uE00D';                
            }
            else if(n == 255)
            {
                colors[n] = '\uE006';
            }
      }
  }

  public void update()
  {
    currentTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    
    if(currentTime-lastRender > 50)
    {
        screen.ShowTextureOnScreen();
        screen.ShowPublicTextOnScreen();
        
        screen.WritePublicText(new String(data), false);    
        lastRender = currentTime;        
    }
  }

  public void reset(Z80 z80, MMU mMU)
  {
    this.z80 = z80;
    this.mMU = mMU;

    Array.Clear(_vram, 0x00, _vram.Length);
    Array.Clear(_oam, 0x00, _oam.Length);
    
   // SpaceGameboy.Echo("Clearing palettes");
    
    for(int i=0; i<4; i++) 
    {
      _palette.bg[i] = 0xFF;
      _palette.obj0[i] = 0xFF;
      _palette.obj1[i] = 0xFF;

    }
    
   // SpaceGameboy.Echo("Clearing tilemaps");
    for(int i=0;i<512;i++)
    {
      _tilemap[i] = new byte[8][];
      for(int j=0;j<8;j++)
      {
        _tilemap[i][j] = new byte[9];
        for(int k=0;k<8;k++)
        {
          _tilemap[i][j][k] = 0x00;
        }
      }
    }
  }
  public void reset2()
  {
//    Echo("GPU: Initialising screen.");
 
    //SpaceGameboy.Echo("Clearing screen data 1/2");
    for(int i = 0; i < data.Length / 2; i++) data[i] = colors[0xFF];
  }
  public void reset3()
  {
   // SpaceGameboy.Echo("Clearing screen data 2/2");
    for(int i = data.Length / 2; i < data.Length; i++) data[i] = colors[0xFF];
  }
  public void reset4()
  {
    for(int n = 160; n < data.Length; n+= 161) data[n] = "\n"[0];

    update();
    
    _curline=0;
    _curscan=0;
    _linemode=2;
    _modeclocks=0;
    _yscrl=0;
    _xscrl=0;
    _raster=0;

    _lcdon = 0;
    _bgon = 0;
    _objon = 0;

    _objsize = 0;
    for(int i=0; i<160; i++) _scanrow[i] = 0;

    for(int i=0; i<40; i++)
    {
      _objdata[i] = new GPUData(i);
    }

    // Set to values expected by BIOS, to start
    _bgtilebase = 0x0000;
    _bgmapbase = 0x1800;

//    Echo("GPU: Reset.");
  }

  public void checkline() {
    _modeclocks += z80.r.m;
    if(_linemode == 0) // In hblank
    {
        if(_modeclocks >= 51)
        {
          // End of hblank for last scanline; render screen
          if(_curline == 143)
          {
            _linemode = 1;
            update();
            mMU._if |= 1;
          }
          else
          {
            _linemode = 2;
          }
          _curline++;
          _curscan += 161;
          _modeclocks=0;
        }
    }
    else if(_linemode == 1) // In vblank
    {
        if(_modeclocks >= 114)
        {
          _modeclocks = 0;
          _curline++;
          if(_curline > 153)
          {
            _curline = 0;
	        _curscan = 0;
            _linemode = 2;
          }
        }
    }
    else if(_linemode == 2) // In OAM-read mode
    {
        if(_modeclocks >= 20)
        {
          _modeclocks = 0;
          _linemode = 3;
        }
    }
    else if(_linemode == 3) // In VRAM-read mode
    {
        // Render scanline at end of allotted time
        if(_modeclocks >= 43)
        {
          _modeclocks = 0;
          _linemode = 0;
          if(_lcdon > 0)
          {
            if(_bgon > 0)
            {
              int linebase = _curscan;
              int mapbase = _bgmapbase + ((((_curline+_yscrl)&255)>>3)<<5);
              int y = (_curline+_yscrl)&7;
              int x = _xscrl&7;
              int t = (_xscrl>>3)&31;
//              var pixel;
              int w=160;

              if(_bgtilebase > 0)
              {
	            int tile = _vram[mapbase+t];
		        if(tile<128) tile=(256+tile);
                var tilerow = _tilemap[tile][y];
                for(int i = 0; i < 99999; i++)
                {
		          _scanrow[159-x] = tilerow[x];
                  data[linebase] = colors[_palette.bg[tilerow[x]]];
                  x++;
                  if(x==8) 
                  { 
                    t=(t+1)&31;
                    x=0;
                    tile=_vram[mapbase+t];
                    if(tile<128)
                    {
                        tile=(256+tile);                        
                    }
                    tilerow = _tilemap[tile][y]; 
                  }
                  linebase++;
                  if(!(--w > 0)) break;
                }
              }
              else
              {
                var tilerow=_tilemap[_vram[mapbase+t]][y];
                for(int i = 0; i < 99999; i++)
                {
		          _scanrow[159-x] = tilerow[x];
                  data[linebase] = colors[_palette.bg[tilerow[x]]];
                  x++;
                  if(x==8) { t=(t+1)&31; x=0; tilerow=_tilemap[_vram[mapbase+t]][y]; }
                  linebase++;
                  if(!(--w > 0)) break;
                }
	          }
            }
            if(_objon > 0)
            {
              var cnt = 0;
              if(_objsize > 0)
              {
/*                for(var i=0; i<40; i++)
                {
                }*/
              }
              else
              {
                byte[] tilerow;
                GPUData obj;
                byte[] pal;
//                var pixel;
                int x = 0;
                int linebase = _curscan;
                int curline161 = _curline*161;
                for(var i=0; i<40; i++)
                {
                  obj = _objdata[i];
                  if(obj.y <= _curline && (obj.y+8) > _curline)
                  {
                    if(obj.yflip > 0)
                      tilerow = _tilemap[obj.tile][7-(_curline-obj.y)];
                    else
                      tilerow = _tilemap[obj.tile][_curline-obj.y];

                    if(obj.palette > 0) pal=_palette.obj1;
                    else pal=_palette.obj0;

                    linebase = (curline161+obj.x);
                    if(obj.xflip > 0)
                    {
                      for(x=0; x<8; x++)
                      {
                        if(obj.x+x >=0 && obj.x+x < 160)
                        {
                          if(tilerow[7-x] > 0 && (obj.prio > 0 || !(_scanrow[x] > 0)))
                          {
                            data[linebase] = colors[pal[tilerow[7-x]]];
                          }
                        }
                        linebase++;
                      }
                    }
                    else
                    {
                      for(x=0; x<8; x++)
                      {
                        if(obj.x+x >=0 && obj.x+x < 160)
                        {
                          if(tilerow[x] > 0 && (obj.prio > 0 || !(_scanrow[x] > 0)))
                          {
                            data[linebase] = colors[pal[tilerow[x]]];
                          }
                        }
                        linebase++;
                      }
                    }
                    cnt++; if(cnt>10) break;
                  }
                }
              }
            }
          }
        }
    }
  }

  int gaddr, i;
  byte v;

  public void updatetile(int addr,byte val) {
    var saddr = addr;
    if((addr&1) > 0) { saddr--; addr--; }
    var tile = (addr>>4)&511;
    var y = (addr>>1)&7;
    int sx;
    for(var x=0;x<8;x++)
    {
      sx=1<<(7-x);
      _tilemap[tile][y][x] = (byte)((((_vram[saddr]&sx)>0)?0x01:0x00) | (((_vram[saddr+1]&sx)>0)?0x02:0x00));
    }
  }

  public void updateoam(int addr,byte val) {
    addr-=0xFE00;
    var obj=addr>>2;
    if(obj<40)
    {
      switch(addr&3)
      {
        case 0: _objdata[obj].y=val-16; break;
        case 1: _objdata[obj].x=val-8; break;
        case 2:
          if(_objsize>0) _objdata[obj].tile = (val&0xFE);
          else _objdata[obj].tile = val;
          break;
        case 3:
          _objdata[obj].palette = ((val&0x10)>0)?1:0;
          _objdata[obj].xflip = ((val&0x20)>0)?1:0;
          _objdata[obj].yflip = ((val&0x40)>0)?1:0;
          _objdata[obj].prio = ((val&0x80)>0)?1:0;
          break;
     }
    }
//    _objdatasorted = new List<GPUData>(_objdata);
   // _objdatasorted.Sort();
   
  }

  public byte rb(int addr) {
    gaddr = addr-0xFF40;
    switch(gaddr)
    {
      case 0:
        return (byte)(((_lcdon>0)?0x80:0)|
               ((_bgtilebase==0x0000)?0x10:0)|
               ((_bgmapbase==0x1C00)?0x08:0)|
               ((_objsize>0)?0x04:0)|
               ((_objon>0)?0x02:0)|
               ((_bgon>0)?0x01:0));

      case 1:
        return (byte)((_curline==_raster?4:0)|_linemode);

      case 2:
        return _yscrl;

      case 3:
        return _xscrl;

      case 4:
        return _curline;

      case 5:
        return _raster;

      default:
        return _reg[gaddr];
    }
  }

  public void wb(int addr,byte val) {
    gaddr = addr-0xFF40;
    _reg[gaddr] = val;
    switch(gaddr)
    {
      case 0:
        _lcdon = ((val&0x80)>0)?1:0;
        _bgtilebase = ((val&0x10)>0)?0x0000:0x0800;
        _bgmapbase = ((val&0x08)>0)?0x1C00:0x1800;
        _objsize = ((val&0x04)>0)?1:0;
        _objon = ((val&0x02)>0)?1:0;
        _bgon = ((val&0x01)>0)?1:0;
        break;

      case 2:
        _yscrl = val;
        break;

      case 3:
        _xscrl = val;
        break;

      case 5:
        _raster = val;
        break; // this was missing, should it be?
      // OAM DMA
      case 6:
        for(i=0; i<160; i++)
        {
          v = mMU.rb((val<<8)+i);
          _oam[i] = v;
          updateoam(0xFE00+i, v);
        }
        break;

      // BG palette mapping
      case 7:
        for(i=0;i<4;i++)
        {
          switch((val>>(i*2))&3)
          {
            case 0: _palette.bg[i] = 255; break;
            case 1: _palette.bg[i] = 192; break;
            case 2: _palette.bg[i] = 96; break;
            case 3: _palette.bg[i] = 0; break;
          }
        }
        break;

      // OBJ0 palette mapping
      case 8:
        for(i=0;i<4;i++)
        {
          switch((val>>(i*2))&3)
          {
            case 0: _palette.obj0[i] = 255; break;
            case 1: _palette.obj0[i] = 192; break;
            case 2: _palette.obj0[i] = 96; break;
            case 3: _palette.obj0[i] = 0; break;
          }
        }
        break;

      // OBJ1 palette mapping
      case 9:
        for(i=0;i<4;i++)
        {
          switch((val>>(i*2))&3)
          {
            case 0: _palette.obj1[i] = 255; break;
            case 1: _palette.obj1[i] = 192; break;
            case 2: _palette.obj1[i] = 96; break;
            case 3: _palette.obj1[i] = 0; break;
          }
        }
        break;
    }
  }
}
