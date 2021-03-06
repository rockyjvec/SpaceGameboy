// You need a timer set to TRIGGER itself and call this script.
// Paste your rom here AFTER converting it at http://jpillora.com/base64-encoder
// DONT PASTE THE ROM IN GAME, COPY THE SCRIPT TO A TEXT EDITOR FIRST. 
//   IT WILL FREEZE THE GAME IF YOU EVEN CLICK ON THE FOLLOWING LINE IN GAME!
string rom="";
//rom source: https://github.com/Skillath/GameBoyFlappyBird

// Adjust the throttle/frameSkips to eliminate complexity errors.
static int throttle = 1000;
static int frameSkips = 4;
static string lcdName = "SpaceGameboy LCD";
static int tapLength = 4;

int stage = 0;
SpaceGameboy gb;
Dictionary<string, bool> tgls = new Dictionary<string, bool>() {
    {"up", false},
    {"down", false},
    {"right", false},
    {"left", false},
    {"a", false},
    {"b", false},
    {"start", false},
    {"select", false}
};

Dictionary<string, int> taps = new Dictionary<string, int>() {
    {"up", 0},
    {"down", 0},
    {"right", 0},
    {"left", 0},
    {"a", 0},
    {"b", 0},
    {"start", 0},
    {"select", 0}
};

Dictionary<string, bool> tgl = new Dictionary<string, bool>() {
    {"up", false},
    {"down", false},
    {"right", false},
    {"left", false},
};

public Program()
{
	Runtime.UpdateFrequency = UpdateFrequency.Update1;
}

public void Main(string arg)
{
    switch(stage)
    {
        case 0:
            if(arg == "storage" || rom == "") rom = Storage;
            var lcd = GridTerminalSystem.GetBlockWithName(lcdName) as IMyTextPanel;
            if(lcd != null)
            {
                gb = new SpaceGameboy(lcd,Echo);
            }
            else
            {
                throw new Exception(lcdName + " not found!");
            }
            break;
        case 1:
        case 2:
        case 3:
        case 4:
        case 5:
        case 6:
            gb.reset(rom, stage);
            break;
        default:
            foreach(var b in new string[]{"up", "down", "left", "right", "a", "b", "start", "select"})
            {
                if(taps[b] > 0)
                {
                    taps[b]--;
                    if(taps[b] == 0) gb.keyup(b);
                }
            }
            if(arg.EndsWith("On")) gb.keydown(arg.Remove(arg.Length - 2));
            else if(arg.EndsWith("Off")) gb.keyup(arg.Remove(arg.Length - 3));
            else if(arg.EndsWith("Toggle"))
            {
                string btn = arg.Remove(arg.Length - 6);
                if(tgls[btn])
                {
                    gb.keyup(btn);
                    tgls[btn] = false;
                }
                else
                {
                    gb.keydown(btn);
                    tgls[btn] = true;                        
                }
            }
            else if(arg.EndsWith("Arrow"))
            {
                string btn = arg.Remove(arg.Length - 5);
                gb.keydown(btn);
                foreach(var b in new string[]{"up", "down", "right", "left"})
                {
                    if(tgl[b])
                    {
                        gb.keyup(b);
                        tgl[b] = false;
                    }
                    else if(btn == b)
                    {
                        tgl[btn] = true;        
                    }
                }                    
            }
            else
            {
                gb.keydown(arg);
                taps[arg] = tapLength;
            }
            gb.frame(throttle, frameSkips, stage);
            if((stage % 2) == 0) gb.update();
            break;
    }
    stage++;
}class GPUData
{
	public int tile, palette, yflip, xflip, prio, num, y, x;
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

class GPU 
{
	public char[] data = new char[161*144];
	public int[] _vram = new int[8192];
	public int[] _oam = new int[160];
	int[] _reg = new int[256];
	int[][][] _tilemap = new int[512][][];
	GPUData[] _objdata = new GPUData[40];
	//  List<GPUData> _objdatasorted;
	int[] _scanrow = new int[160];
    
    char[] bgPalette = new char[4];
    char[] obj0Palette = new char[4];
    char[] obj1Palette = new char[4];

	int _curline = 0;
	int _curscan = 0;
	int _linemode = 0;
	int _modeclocks = 0;

	int _yscrl = 0;
	int _xscrl = 0;
	int _raster = 0;

	bool _lcdon = false;
	bool _bgon = false;
	bool _winTransparent = false;
	bool _winon = false;
    int winx = 7;
    int winy = 0;

	int _objsize = 8;

    int _winbase = 0x1800;
	int _bgtilebase = 0x0000;
	int _bgmapbase = 0x1800;

	int wpixel = 0;

	IMyTextPanel screen;
	bool draw = true, startDraw = false, ready = false;
	Z80 z80;
	MMU mMU;

	public GPU(IMyTextPanel screen)
	{
		this.screen = screen;
        screen.ShowPublicTextOnScreen();
	}

	public void drawNow()
	{
		if (startDraw == false) {
			startDraw = true;
		}
	}

	public void update()
	{
        if(ready)
        {
            screen.WritePublicText(new String(data), false);
        }
	}

	public void reset(Z80 z80, MMU mMU)
	{
		this.z80 = z80;
		this.mMU = mMU;

		Array.Clear(_vram, 0x00, _vram.Length);
		Array.Clear(_oam, 0x00, _oam.Length);

		// SpaceGameboy.Echo("Clearing palettes");

        bgPalette[0] = '\uE00F';
        bgPalette[1] = '\uE00E';
        bgPalette[2] = '\uE00D';
        bgPalette[3] = '\uE006';

        obj0Palette[0] = '\uE00F';
        obj0Palette[1] = '\uE00E';
        obj0Palette[2] = '\uE00D';
        obj0Palette[3] = '\uE006';

        obj1Palette[0] = '\uE00F';
        obj1Palette[1] = '\uE00E';
        obj1Palette[2] = '\uE00D';
        obj1Palette[3] = '\uE006';
        
		// SpaceGameboy.Echo("Clearing tilemaps");
		for(int i=0;i<512;i++)
		{
			_tilemap[i] = new int[8][];
			for(int j=0;j<8;j++)
			{
				_tilemap[i][j] = new int[9];
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
		for(int i = 0; i < data.Length / 2; i++) data[i] = bgPalette[3];
	}
	public void reset3()
	{
		// SpaceGameboy.Echo("Clearing screen data 2/2");
		for(int i = data.Length / 2; i < data.Length; i++) data[i] = bgPalette[3];
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

		_lcdon = false;
		_bgon = false;
		_winTransparent = true;
        

		_objsize = 8;
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

	int t, x, y, mapbase, linebase, tile, cnt, curline161, i;
	int[] tilerow;
    char[] pal;
	GPUData obj;
	public void checkline() {
		if (!this.draw && !this.startDraw && !this.ready)
			return;
		_modeclocks += z80.r.m;
			switch (_linemode) {
			case 0: // In hblank
				{
					if (_modeclocks >= 51) {
						// End of hblank for last scanline; render screen
						if (_curline == 143) {
							_linemode = 1;
							if (this.draw) {
								this.draw = false;
                                this.ready = true;
							} else if (this.startDraw) {
								this.startDraw = false;
								this.draw = true;
							}
//							mMU._if |= 1;
						} else {
							_linemode = 2;
						}
						_curline++;
						_curscan += 161;
						_modeclocks = 0;
					}
					break;
				}
			case 1: // In vblank
				{
					if (_modeclocks >= 114) {
						_modeclocks = 0;
						_curline++;
						if (_curline > 153) {
							_curline = 0;
							_curscan = 0;
							_linemode = 2;
						}
					}
					break;
				}
			case 2: // In OAM-read mode
				{
					if (_modeclocks >= 20) {
						_modeclocks = 0;
						_linemode = 3;
					}
					break;
				}
			case 3: // In VRAM-read mode
				{
					// Render scanline at end of allotted time
					if (_modeclocks >= 43) {
						_modeclocks = 0;
						_linemode = 0;
						if (_lcdon) {
							if (_bgon) {
								linebase = _curscan;
								mapbase = _bgmapbase + ((((_curline + _yscrl) & 0xFF) >> 0x03) << 0x05);
								y = (_curline + _yscrl) & 7;
								x = _xscrl & 7;
								t = (_xscrl >> 3) & 31;

								if (_bgtilebase != 0) {
									tile = _vram [mapbase + t];
									if (tile < 128)
										tile = (256 + tile);
									tilerow = _tilemap [tile] [y];
									for (wpixel = 160; wpixel > 0; wpixel--) {
										_scanrow [159 - x] = tilerow [x];
										data [linebase] = bgPalette [tilerow [x]];
										x++;
										if (x == 8) { 
											t = (t + 1) & 31;
											x = 0;
											tile = _vram [mapbase + t];
											if (tile < 128) {
												tile = (256 + tile);                        
											}
											tilerow = _tilemap [tile] [y]; 
										}
										linebase++;
									}
								} else {
									tilerow = _tilemap [_vram [mapbase + t]] [y];
									for (wpixel = 160; wpixel > 0; wpixel--) {
										_scanrow [159 - x] = tilerow [x];
										data [linebase] = bgPalette [tilerow [x]];
										x++;
										if (x == 8) {
											t = (t + 1) & 31; 
											x = 0;
											tilerow = _tilemap [_vram [mapbase + t]] [y];
										}
										linebase++;
									}
								}
							}
                            if (_winon && false) { // windows disabled for now until they are fixed
								linebase = _curscan;
								mapbase = _winbase + ((((_curline + winy) & 0xFF) >> 0x03) << 0x05);
								y = (_curline + winy) & 7;
								x = winx & 7;
								t = (winx >> 3) & 31;

								if (_winbase != 0) {
									tile = _vram [mapbase + t];
									if (tile < 128)
										tile = (256 + tile);
									tilerow = _tilemap [tile] [y];
									for (wpixel = 160; wpixel > 0; wpixel--) {
										_scanrow [159 - x] = tilerow [x];
                                        if(!_winTransparent || bgPalette [tilerow [x]] != '\uE00F')
                                            data [linebase] = bgPalette [tilerow [x]];
										x++;
										if (x == 8) { 
											t = (t + 1) & 31;
											x = 0;
											tile = _vram [mapbase + t];
											if (tile < 128) {
												tile = (256 + tile);                        
											}
											tilerow = _tilemap [tile] [y]; 
										}
										linebase++;
									}
								} else {
									tilerow = _tilemap [_vram [mapbase + t]] [y];
									for (wpixel = 160; wpixel > 0; wpixel--) {
										_scanrow [159 - x] = tilerow [x];
                                        if(!_winTransparent || bgPalette [tilerow [x]] != '\uE00F')
                                            data [linebase] = bgPalette [tilerow [x]];
										x++;
										if (x == 8) {
											t = (t + 1) & 31; 
											x = 0;
											tilerow = _tilemap [_vram [mapbase + t]] [y];
										}
										linebase++;
									}
								}
                                
                            }                                
							//if (_objon) 
                            {
						//		cnt = 0;
                                linebase = _curscan;
                                curline161 = _curline * 161;
                                for (i = 0; i < 40; i++) {
                                    obj = _objdata [i];
                                    if (obj.y <= _curline && (obj.y + _objsize) > _curline) {
                                        if (obj.yflip > 0)
                                            tilerow = _tilemap [obj.tile+((((_objsize - 1 - (_curline - obj.y)))>7)?1:0)] [(_objsize - 1 - (_curline - obj.y)) % 8];
                                        else
                                            tilerow = _tilemap [obj.tile+(((_curline - obj.y)>7)?1:0)] [(_curline - obj.y) % 8];

                                        
                                        if (obj.palette > 0)
                                            pal = obj1Palette;
                                        else
                                            pal = obj0Palette;

                                        linebase = (curline161 + obj.x);
                                        if (obj.xflip > 0) {
                                            for (x = 0; x < 8; x++) {
                                                if (obj.x + x >= 0 && obj.x + x < 160) {
                                                    if (tilerow [7 - x] > 0 && (obj.prio > 0 || !(_scanrow [x] > 0))) {
                                                        data [linebase] = pal [tilerow [7 - x]];
                                                    }
                                                }
                                                linebase++;
                                            }
                                        } else {
                                            for (x = 0; x < 8; x++) {
                                                if (obj.x + x >= 0 && obj.x + x < 160) {
                                                    if (tilerow [x] > 0 && (obj.prio > 0 || !(_scanrow [x] > 0))) {
                                                        data [linebase] = pal [tilerow [x]];
                                                    }
                                                }
                                                linebase++;
                                            }
                                        }
                                  //      cnt++;
                                     //   if (cnt > 10)
                                     //       break;
                                    }
                                }
                            }
						}
					}
					break;
				}
			}
	}

	int gaddr;
	int v;

	public void updatetile(int addr,int val) {
		var saddr = addr;
		if((addr&1) > 0) { saddr--; addr--; }
		var tile = (addr>>4)&511;
		var y = (addr>>1)&7;
		int sx;
		for(var x=0;x<8;x++)
		{
			sx=1<<(7-x);
			_tilemap[tile][y][x] = ((((_vram[saddr]&sx)>0)?0x01:0x00) | (((_vram[saddr+1]&sx)>0)?0x02:0x00));
		}
	}

	public void updateoam(int addr,int val) {
		addr-=0xFE00;
		var o=addr>>2;
      //  var sorted = new SortedDictionary<int, GPUData>();
		if(o<40&&o>=0)
		{
			switch(addr&3)
			{
			case 0: _objdata[o].y=val-16; break;
			case 1: _objdata[o].x=val-8; break;
			case 2:
				if(_objsize != 8) _objdata[o].tile = (val&0xFE);
				else _objdata[o].tile = val;
				break;
			case 3:
				_objdata[o].palette = ((val&0x10)>0)?1:0;
				_objdata[o].xflip = ((val&0x20)>0)?1:0;
				_objdata[o].yflip = ((val&0x40)>0)?1:0;
				_objdata[o].prio = ((val&0x80)>0)?1:0;
				break;
			}
          //  sorted.Add((-_objdata[o].x * 100) - _objdata[o].num, _objdata[o]);
		}
/*        int i = 0;
        foreach(var q in sorted)
        {
            _objdata[i] = q.Value;
            i++;
        }*/
		//    _objdatasorted = new List<GPUData>(_objdata);
//		Array.Sort(_objdata);

	}

	public int rb(int addr) {
		gaddr = addr-0xFF40;
		switch(gaddr)
		{
		case 0:
			return ((_lcdon?0x80:0)|
				((_winbase==0x1C00)?0x40:0)|
                (_winon?0x20:0x00)|
				((_bgtilebase==0x0000)?0x10:0)|
				((_bgmapbase==0x1C00)?0x08:0)|
				((_objsize == 16)?0x04:0)|
				(_winTransparent?0x00:0x02)|
				(_bgon?0x01:0x00));

		case 1:
			return ((_curline==_raster?4:0)|_linemode);

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

	public void wb(int addr,int val) {
		gaddr = addr-0xFF40;
		_reg[gaddr] = val;
		switch(gaddr)
		{
		case 0:
			_lcdon = ((val&0x80)>0)?true:false;
            _winbase = ((val&0x40)>0)?0x1C00:0x1800;
            _winon = ((val&0x20)>0)?true:false;
			_bgtilebase = ((val&0x10)>0)?0x0000:0x0800;
			_bgmapbase = ((val&0x08)>0)?0x1C00:0x1800;
			_objsize = ((val&0x04)>0)?16:8;
			_winTransparent = ((val&0x02)>0)?false:true;
			_bgon = ((val&0x01)>0)?true:false;
			break;

		case 2:
			_yscrl = val;
			break;

		case 3:
			_xscrl = val;
			break;

		case 5: 
			_raster = val;
            break;
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
				case 3: bgPalette[i] = '\uE00F'; break;
				case 2: bgPalette[i] = '\uE00E'; break;
				case 1: bgPalette[i] = '\uE00D'; break;
				case 0: bgPalette[i] = '\uE006'; break;
				}
			}
			break;

			// OBJ0 palette mapping
		case 8:
			for(i=0;i<4;i++)
			{
				switch((val>>(i*2))&3)
				{
				case 3: obj0Palette[i] = '\uE00F'; break;
				case 2: obj0Palette[i] = '\uE00E'; break;
				case 1: obj0Palette[i] = '\uE00D'; break;
				case 0: obj0Palette[i] = '\uE006'; break;
				}
			}
			break;

			// OBJ1 palette mapping
		case 9:
			for(i=0;i<4;i++)
			{
				switch((val>>(i*2))&3)
				{
				case 3: obj1Palette[i] = '\uE00F'; break;
				case 2: obj1Palette[i] = '\uE00E'; break;
				case 1: obj1Palette[i] = '\uE00D'; break;
				case 0: obj1Palette[i] = '\uE006'; break;
				}
			}

			break;
        case 10: // WNDPOSY
            winy = val;
            break;
        case 11:
            if(val > 166) _winon = false;
            winx = val;
            break;
		}
	}
}class KEY 
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
	class MMUmbc {
		public int rombank=0, rambank=0, ramon=0, mode=0;
	}
	class MMU 
	{
		public byte[] _rom;
		int _carttype = 0;
		MMUmbc _mbc0 = new MMUmbc();
		MMUmbc _mbc1 = new MMUmbc();
		int _romoffs = 0x4000;
		int _ramoffs = 0;

		public byte[] mem = new byte[65536];
		int[] _wram = new int[8192];
		int[] _eram = new int[32768];
		int[] _zram = new int[128];
		public int[] _timer = new int[4];

		public int _ie = 0;
		public int _if = 0;

		GPU gPU;
//		TIMER tIMER;
		KEY kEY;
		Z80 z80;

		int v, w;

		public void reset(GPU gPU,/* TIMER tIMER,*/ KEY kEY, Z80 z80) {

			this.gPU = gPU;
		//	this.tIMER = tIMER;
			this.kEY = kEY;
			this.z80 = z80;

			_timer = new int[4]{ 0, 0, 0, 0 };
				
			int i;
			//SpaceGameboy.Echo("Clearing wram");
			for(i=0; i<8192; i++) _wram[i] = 0;
			//SpaceGameboy.Echo("Clearing eram");
			for(i=0; i<32768; i++) _eram[i] = 0;
			//SpaceGameboy.Echo("Clearing zram");
			for(i=0; i<127; i++) _zram[i] = 0;

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
			_rom = rom;
			_carttype = _rom[0x0147];
		}
		public int rb(int addr) {
			if(addr <= 0x3FFF)
			{
				return _rom[addr];
			}
			switch(addr&0xF000)
			{
			case 0xF000:
				{
					// Everything else
					if(addr <= 0xFDFF)
					{
						// Echo RAM
						return _wram[addr&0x1FFF];
					}
					else if(addr <= 0xFEFF)
					{
						// OAM
						return (((addr&0xFF)<0xA0) ? gPU._oam[addr&0xFF] : 0x00);
					}
					// Zeropage RAM, I/O, interrupts                        
					else if(addr <= 0xFF0F)
						switch(addr&0xF)
					{
					case 0: return kEY.rb();    // JOYP
						case 4:
						case 5:
						case 6:
						case 7:
						return _timer[addr&0xF - 4];//tIMER.rb(addr);
					case 15: return _if;    // Interrupt flags
					default: return 0x00;
					}
					else if(addr <= 0xFF3F)
						return 0x00;

					else if(addr <= 0xFF7F)
						return gPU.rb(addr);
					else if(addr == 0xFFFF) { return _ie; }
					else { return _zram[addr&0x7F]; }
				}
			case 0x0000:
			case 0x1000:
			case 0x2000:
			case 0x3000:
				{
					return _rom[addr];
				}
			case 0x4000:
			case 0x5000:
			case 0x6000:
			case 0x7000:
				{
					// ROM bank 1
					return _rom[_romoffs+(addr&0x3FFF)];
				}
			case 0x8000:
			case 0x9000:
				{
					// VRAM
					return gPU._vram[addr&0x1FFF];
				}
			case 0xA000:
			case 0xB000:
				{
					// External RAM
					return _eram[_ramoffs+(addr&0x1FFF)];
				}
			case 0xC000:
			case 0xD000:
			case 0xE000:
				{
					// Work RAM and echo
					return _wram[addr&0x1FFF];
				}
			}

			return 0x00;
		}

		public int rw(int addr) { return rb(addr)+(rb(addr+1)<<8); }

		public void wb(int addr, int val) {
			v = addr&0xF000;
			if(v == 0x0000 || v == 0x1000)
			{
				// ROM bank 0
				// MBC1: Turn external RAM on
				switch(_carttype)
				{
				case 1:
				case 2:
				case 3:
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
				case 2:
				case 3:
					_mbc1.rombank &= 0x60;
					val &= 0x1F;
					if(!(val!=0)) val=1;
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
				case 2:
				case 3:
					if(_mbc1.mode != 0)
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
				w = addr&0x0F00;
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
							case 4:
								_timer [addr & 0xF - 4] = 0x00;
								break;
							case 5:
							case 6:
							case 7: 
								_timer [addr & 0xF - 4] = val;
							break;
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

		public void ww(int addr,int val) { wb(addr,val&0xFF); wb(addr+1,val>>8); }
	}public class SpaceGameboy
{
	private GPU gPU;
	private MMU mMU;
	private Z80 z80;
	private KEY kEY;
//	private TIMER tIMER;
	static public Action<string> Echo;

	public SpaceGameboy(IMyTextPanel screen, Action<string> Echo)
	{
		SpaceGameboy.Echo = Echo;
		//      Echo("Loading GPU");
		this.gPU = new GPU(screen);
		//Echo("Loading MMU");
		this.mMU = new MMU();
		//Echo("Loading Z80");
		this.z80 = new Z80();
		//Echo("Loading KEY");
		this.kEY = new KEY();
		//Echo("Loading TIMER");
	//	this.tIMER = new TIMER();
	}

	long startTime = 0, lastTime = 0, hblankTime, currentTime = 0;

    long vblanks = 0;
	int fclock, ifired;
	public void frame(int throttle, int frameSkip, int stage) {
		fclock = z80._clock.m+17556;
		//Echo every 100 frames
		//    if(stage % 100 == 0) Echo("Stepping Gameboy (PC = " + z80.r.pc + ")"); 
		//var brk = document.getElementById('breakpoint').value;
		if((stage % frameSkip) == 0)gPU.drawNow ();

			currentTime = System.DateTime.Now.Ticks;
			mMU._timer [0] = (int)((currentTime - startTime) / 610.3515625);
			switch(mMU._timer[3] & 0x3)
			{
			case 0:
				mMU._timer [1] += (int)(((currentTime - lastTime) / 610.3515625) / 1024);
				break;
			case 1:
				mMU._timer [1] += (int)(((currentTime - lastTime) / 610.3515625) / 16);
				break;
			case 2:
				mMU._timer [1] += (int)(((currentTime - lastTime) / 610.3515625) / 64);
				break;
			case 3:
				mMU._timer [1] += (int)(((currentTime - lastTime) / 610.3515625) / 256);
				break;				
			}
			if (mMU._timer [1] > 255) {
				mMU._if |= 0x04;
				lastTime = currentTime;
			}
            if(currentTime - hblankTime > 283813) //handle hblank
            {
				mMU._if |= 0x01;
                hblankTime = currentTime;
                vblanks++;
//                Echo("vblank " + vblanks);
            }

			lastTime = currentTime;
		
		for(;z80._clock.m < fclock && throttle > 0;throttle--)
		{
			//if(z80._halt!=0) z80.r.m=1;
			//else
			//{
			//  z80.r.r = (z80.r.r+1) & 127;
			z80._map[mMU.rb(z80.r.pc++)]();
			z80.r.pc &= 65535;
			//}
			if(z80.r.ime !=0 && mMU._ie!=0 && mMU._if!=0)
			{
				z80._halt=0; z80.r.ime=0;
				ifired = mMU._ie & mMU._if;
				if((ifired&1)!=0) { mMU._if &= 0xFE; z80.RST40(); }
				else if((ifired&2)!=0) { mMU._if &= 0xFD; z80.RST48(); }
				else if((ifired&4)!=0) { mMU._if &= 0xFB; z80.RST50(); }
				else if((ifired&8)!=0) { mMU._if &= 0xF7; z80.RST58(); }
				else if((ifired&16)!=0) { mMU._if &= 0xEF; z80.RST60(); }
				else { z80.r.ime=1; }
			}
			z80._clock.m += z80.r.m;
			gPU.checkline();
	//		tIMER.inc();
		}
	}

	public void update()
	{
		gPU.update();
	}

	public void reset(string rom, int stage) {
		switch(stage)
		{
		case 1:
			//SpaceGameboy.Echo("Resetting gpu");
			gPU.reset(this.z80, this.mMU);
			break;
		case 2:
			gPU.reset2();            
			break;
		case 3:
			gPU.reset3();            
			break;
		case 4:
			gPU.reset4();            
			break;
		case 5:
			// SpaceGameboy.Echo("Resetting mmu");
			mMU.reset(gPU, /*tIMER,*/ kEY, z80); 
			//   SpaceGameboy.Echo("Resetting z80");
			z80.reset(mMU); 
			//   SpaceGameboy.Echo("Resetting key");
			kEY.reset(); 
			//     SpaceGameboy.Echo("Resetting timer");
		//	tIMER.reset(mMU, z80);
			z80.r.pc=0x100;z80.r.sp=0xFFFE;/*z80.r.hl=0x014D;*/z80.r.c=0x13;z80.r.e=0xD8;z80.r.a=1;
			//TODO:                                              ^ this was missing, I don't know if it is supposed to be set
			break;
		case 6:
			// SpaceGameboy.Echo("Loading ROM!");
            byte[] r;
			if (rom.Substring (0, 13) == "data:;base64,")  
				r = Convert.FromBase64String (rom.Substring (13));  
			else  
				r = Convert.FromBase64String (rom);  
			mMU.load(r);
			this.run();
			break;
		}

        startTime = hblankTime = System.DateTime.Now.Ticks;

		//    Echo("MAIN: Reset.");
	}

	public void run() {
		z80._stop = 0;
	}

	public void keydown(string key)
	{
		kEY.keydown(key);
	}

	public void keyup(string key)
	{
		kEY.keyup(key);        
	}
}/*class TIMER 
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

		if((_tac & 4)!=0)
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
}*/	/**
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

	class Z80 
	{
		public Z80r r = new Z80r();

		public Z80rsv rv = new Z80rsv();

		public Z80clock _clock = new Z80clock();

		public int _halt = 0;
		public int _stop = 0;
		MMU mMU;

		int ci, co, i, tr, a, hl, m;

		public Z80()
		{    

			this._map = new Action[256]{
				// 00
				NOP,		LDBCnn,	LDBCmA,	INCBC,
				INCr_b,	DECr_b,	LDrn_b,	RLCA,
				LDmmSP,	ADDHLBC,	LDABCm,	DECBC,
				INCr_c,	DECr_c,	LDrn_c,	RRCA,
				// 10
				DJNZn,	LDDEnn,	LDDEmA,	INCDE,
				INCr_d,	DECr_d,	LDrn_d,	RLA,
				JRn,		ADDHLDE,	LDADEm,	DECDE,
				INCr_e,	DECr_e,	LDrn_e,	RRA,
				// 20
				JRNZn,	LDHLnn,	LDHLIA,	INCHL,
				INCr_h,	DECr_h,	LDrn_h,	DAA,
				JRZn,	ADDHLHL,	LDAHLI,	DECHL,
				INCr_l,	DECr_l,	LDrn_l,	CPL,
				// 30
				JRNCn,	LDSPnn,	LDHLDA,	INCSP,
				INCHLm,	DECHLm,	LDHLmn,	SCF,
				JRCn,	ADDHLSP,	LDAHLD,	DECSP,
				INCr_a,	DECr_a,	LDrn_a,	CCF,
				// 40
				LDrr_bb,	LDrr_bc,	LDrr_bd,	LDrr_be,
				LDrr_bh,	LDrr_bl,	LDrHLm_b,	LDrr_ba,
				LDrr_cb,	LDrr_cc,	LDrr_cd,	LDrr_ce,
				LDrr_ch,	LDrr_cl,	LDrHLm_c,	LDrr_ca,
				// 50
				LDrr_db,	LDrr_dc,	LDrr_dd,	LDrr_de,
				LDrr_dh,	LDrr_dl,	LDrHLm_d,	LDrr_da,
				LDrr_eb,	LDrr_ec,	LDrr_ed,	LDrr_ee,
				LDrr_eh,	LDrr_el,	LDrHLm_e,	LDrr_ea,
				// 60
				LDrr_hb,	LDrr_hc,	LDrr_hd,	LDrr_he,
				LDrr_hh,	LDrr_hl,	LDrHLm_h,	LDrr_ha,
				LDrr_lb,	LDrr_lc,	LDrr_ld,	LDrr_le,
				LDrr_lh,	LDrr_ll,	LDrHLm_l,	LDrr_la,
				// 70
				LDHLmr_b,	LDHLmr_c,	LDHLmr_d,	LDHLmr_e,
				LDHLmr_h,	LDHLmr_l,	HALT,		LDHLmr_a,
				LDrr_ab,	LDrr_ac,	LDrr_ad,	LDrr_ae,
				LDrr_ah,	LDrr_al,	LDrHLm_a,	LDrr_aa,
				// 80
				ADDr_b,	ADDr_c,	ADDr_d,	ADDr_e,
				ADDr_h,	ADDr_l,	ADDHL,		ADDr_a,
				ADCr_b,	ADCr_c,	ADCr_d,	ADCr_e,
				ADCr_h,	ADCr_l,	ADCHL,		ADCr_a,
				// 90
				SUBr_b,	SUBr_c,	SUBr_d,	SUBr_e,
				SUBr_h,	SUBr_l,	SUBHL,		SUBr_a,
				SBCr_b,	SBCr_c,	SBCr_d,	SBCr_e,
				SBCr_h,	SBCr_l,	SBCHL,		SBCr_a,
				// A0
				ANDr_b,	ANDr_c,	ANDr_d,	ANDr_e,
				ANDr_h,	ANDr_l,	ANDHL,		ANDr_a,
				XORr_b,	XORr_c,	XORr_d,	XORr_e,
				XORr_h,	XORr_l,	XORHL,		XORr_a,
				// B0
				ORr_b,	ORr_c,		ORr_d,		ORr_e,
				ORr_h,	ORr_l,		ORHL,		ORr_a,
				CPr_b,	CPr_c,		CPr_d,		CPr_e,
				CPr_h,	CPr_l,		CPHL,		CPr_a,
				// C0
				RETNZ,	POPBC,		JPNZnn,	JPnn,
				CALLNZnn,	PUSHBC,	ADDn,		RST00,
				RETZ,	RET,		JPZnn,		MAPcb,
				CALLZnn,	CALLnn,	ADCn,		RST08,
				// D0
				RETNC,	POPDE,		JPNCnn,	XX,
				CALLNCnn,	PUSHDE,	SUBn,		RST10,
				RETC,	RETI,		JPCnn,		XX,
				CALLCnn,	XX,		SBCn,		RST18,
				// E0
				LDIOnA,	POPHL,		LDIOCA,	XX,
				XX,		PUSHHL,	ANDn,		RST20,
				ADDSPn,	JPHL,		LDmmA,		XX,
				XX,		XX,		XORn,		RST28,
				// F0
				LDAIOn,	POPAF,		LDAIOC,	DI,
				XX,		PUSHAF,	ORn,		RST30,
				LDHLSPn,	XX,		LDAmm,		EI,
				XX,		XX,		CPn,		RST38
			};

			this._cbmap = new Action[256]{
				// CB00
				RLCr_b,	RLCr_c,	RLCr_d,	RLCr_e,
				RLCr_h,	RLCr_l,	RLCHL,		RLCr_a,
				RRCr_b,	RRCr_c,	RRCr_d,	RRCr_e,
				RRCr_h,	RRCr_l,	RRCHL,		RRCr_a,
				// CB10
				RLr_b,	RLr_c,		RLr_d,		RLr_e,
				RLr_h,	RLr_l,		RLHL,		RLr_a,
				RRr_b,	RRr_c,		RRr_d,		RRr_e,
				RRr_h,	RRr_l,		RRHL,		RRr_a,
				// CB20
				SLAr_b,	SLAr_c,	SLAr_d,	SLAr_e,
				SLAr_h,	SLAr_l,	XX,		SLAr_a,
				SRAr_b,	SRAr_c,	SRAr_d,	SRAr_e,
				SRAr_h,	SRAr_l,	XX,		SRAr_a,
				// CB30
				SWAPr_b,	SWAPr_c,	SWAPr_d,	SWAPr_e,
				SWAPr_h,	SWAPr_l,	XX,		SWAPr_a,
				SRLr_b,	SRLr_c,	SRLr_d,	SRLr_e,
				SRLr_h,	SRLr_l,	XX,		SRLr_a,
				// CB40
				BIT0b,	BIT0c,		BIT0d,		BIT0e,
				BIT0h,	BIT0l,		BIT0m,		BIT0a,
				BIT1b,	BIT1c,		BIT1d,		BIT1e,
				BIT1h,	BIT1l,		BIT1m,		BIT1a,
				// CB50
				BIT2b,	BIT2c,		BIT2d,		BIT2e,
				BIT2h,	BIT2l,		BIT2m,		BIT2a,
				BIT3b,	BIT3c,		BIT3d,		BIT3e,
				BIT3h,	BIT3l,		BIT3m,		BIT3a,
				// CB60
				BIT4b,	BIT4c,		BIT4d,		BIT4e,
				BIT4h,	BIT4l,		BIT4m,		BIT4a,
				BIT5b,	BIT5c,		BIT5d,		BIT5e,
				BIT5h,	BIT5l,		BIT5m,		BIT5a,
				// CB70
				BIT6b,	BIT6c,		BIT6d,		BIT6e,
				BIT6h,	BIT6l,		BIT6m,		BIT6a,
				BIT7b,	BIT7c,		BIT7d,		BIT7e,
				BIT7h,	BIT7l,		BIT7m,		BIT7a,
				// CB80
				RES0b,	RES0c,		RES0d,		RES0e,
				RES0h,	RES0l,		RES0m,		RES0a,
				RES1b,	RES1c,		RES1d,		RES1e,
				RES1h,	RES1l,		RES1m,		RES1a,
				// CB90
				RES2b,	RES2c,		RES2d,		RES2e,
				RES2h,	RES2l,		RES2m,		RES2a,
				RES3b,	RES3c,		RES3d,		RES3e,
				RES3h,	RES3l,		RES3m,		RES3a,
				// CBA0
				RES4b,	RES4c,		RES4d,		RES4e,
				RES4h,	RES4l,		RES4m,		RES4a,
				RES5b,	RES5c,		RES5d,		RES5e,
				RES5h,	RES5l,		RES5m,		RES5a,
				// CBB0
				RES6b,	RES6c,		RES6d,		RES6e,
				RES6h,	RES6l,		RES6m,		RES6a,
				RES7b,	RES7c,		RES7d,		RES7e,
				RES7h,	RES7l,		RES7m,		RES7a,
				// CBC0
				SET0b,	SET0c,		SET0d,		SET0e,
				SET0h,	SET0l,		SET0m,		SET0a,
				SET1b,	SET1c,		SET1d,		SET1e,
				SET1h,	SET1l,		SET1m,		SET1a,
				// CBD0
				SET2b,	SET2c,		SET2d,		SET2e,
				SET2h,	SET2l,		SET2m,		SET2a,
				SET3b,	SET3c,		SET3d,		SET3e,
				SET3h,	SET3l,		SET3m,		SET3a,
				// CBE0
				SET4b,	SET4c,		SET4d,		SET4e,
				SET4h,	SET4l,		SET4m,		SET4a,
				SET5b,	SET5c,		SET5d,		SET5e,
				SET5h,	SET5l,		SET5m,		SET5a,
				// CBF0
				SET6b,	SET6c,		SET6d,		SET6e,
				SET6h,	SET6l,		SET6m,		SET6a,
				SET7b,	SET7c,		SET7d,		SET7e,
				SET7h,	SET7l,		SET7m,		SET7a,
			};
		}
		public void reset(MMU mMU) {
			this.mMU = mMU;
			r.a=0; r.b=0; r.c=0; r.d=0; r.e=0; r.h=0; r.l=0; r.f=0;
			r.sp=0; r.pc=0; r.i=0; r.r=0;
			r.m=0;
			_halt=0; _stop=0;
			_clock.m=0;
			r.ime=1;
			//Echo("Z80: Reset.");
		}

		public void exec() {
			r.r = (r.r+1) & 127;
			_map[mMU.rb(r.pc++)]();
			r.pc &= 65535;
			_clock.m += r.m;
		}

		public Action[] _map;
		public Action[] _cbmap;


		/*--- Load/store ---*/
		public void LDrr_bb() { /*r.b=r.b;*/ r.m=1; }
		public void LDrr_bc() { r.b=r.c; r.m=1; }
		public void LDrr_bd() { r.b=r.d; r.m=1; }
		public void LDrr_be() { r.b=r.e; r.m=1; }
		public void LDrr_bh() { r.b=r.h; r.m=1; }
		public void LDrr_bl() { r.b=r.l; r.m=1; }
		public void LDrr_ba() { r.b=r.a; r.m=1; }
		public void LDrr_cb() { r.c=r.b; r.m=1; }
		public void LDrr_cc() { /*r.c=r.c;*/ r.m=1; }
		public void LDrr_cd() { r.c=r.d; r.m=1; }
		public void LDrr_ce() { r.c=r.e; r.m=1; }
		public void LDrr_ch() { r.c=r.h; r.m=1; }
		public void LDrr_cl() { r.c=r.l; r.m=1; }
		public void LDrr_ca() { r.c=r.a; r.m=1; }
		public void LDrr_db() { r.d=r.b; r.m=1; }
		public void LDrr_dc() { r.d=r.c; r.m=1; }
		public void LDrr_dd() {/* r.d=r.d;*/ r.m=1; }
		public void LDrr_de() { r.d=r.e; r.m=1; }
		public void LDrr_dh() { r.d=r.h; r.m=1; }
		public void LDrr_dl() { r.d=r.l; r.m=1; }
		public void LDrr_da() { r.d=r.a; r.m=1; }
		public void LDrr_eb() { r.e=r.b; r.m=1; }
		public void LDrr_ec() { r.e=r.c; r.m=1; }
		public void LDrr_ed() { r.e=r.d; r.m=1; }
		public void LDrr_ee() {/* r.e=r.e;*/ r.m=1; }
		public void LDrr_eh() { r.e=r.h; r.m=1; }
		public void LDrr_el() { r.e=r.l; r.m=1; }
		public void LDrr_ea() { r.e=r.a; r.m=1; }
		public void LDrr_hb() { r.h=r.b; r.m=1; }
		public void LDrr_hc() { r.h=r.c; r.m=1; }
		public void LDrr_hd() { r.h=r.d; r.m=1; }
		public void LDrr_he() { r.h=r.e; r.m=1; }
		public void LDrr_hh() {/* r.h=r.h;*/ r.m=1; }
		public void LDrr_hl() { r.h=r.l; r.m=1; }
		public void LDrr_ha() { r.h=r.a; r.m=1; }
		public void LDrr_lb() { r.l=r.b; r.m=1; }
		public void LDrr_lc() { r.l=r.c; r.m=1; }
		public void LDrr_ld() { r.l=r.d; r.m=1; }
		public void LDrr_le() { r.l=r.e; r.m=1; }
		public void LDrr_lh() { r.l=r.h; r.m=1; }
		public void LDrr_ll() { /*r.l=r.l;*/ r.m=1; }
		public void LDrr_la() { r.l=r.a; r.m=1; }
		public void LDrr_ab() { r.a=r.b; r.m=1; }
		public void LDrr_ac() { r.a=r.c; r.m=1; }
		public void LDrr_ad() { r.a=r.d; r.m=1; }
		public void LDrr_ae() { r.a=r.e; r.m=1; }
		public void LDrr_ah() { r.a=r.h; r.m=1; }
		public void LDrr_al() { r.a=r.l; r.m=1; }
		public void LDrr_aa() { /*r.a=r.a;*/ r.m=1; }

		public void LDrHLm_b() { r.b=mMU.rb((r.h<<8)+r.l); r.m=2; }
		public void LDrHLm_c() { r.c=mMU.rb((r.h<<8)+r.l); r.m=2; }
		public void LDrHLm_d() { r.d=mMU.rb((r.h<<8)+r.l); r.m=2; }
		public void LDrHLm_e() { r.e=mMU.rb((r.h<<8)+r.l); r.m=2; }
		public void LDrHLm_h() { r.h=mMU.rb((r.h<<8)+r.l); r.m=2; }
		public void LDrHLm_l() { r.l=mMU.rb((r.h<<8)+r.l); r.m=2; }
		public void LDrHLm_a() { r.a=mMU.rb((r.h<<8)+r.l); r.m=2; }

		public void LDHLmr_b() { mMU.wb((r.h<<8)+r.l,(byte)r.b); r.m=2; }
		public void LDHLmr_c() { mMU.wb((r.h<<8)+r.l,(byte)r.c); r.m=2; }
		public void LDHLmr_d() { mMU.wb((r.h<<8)+r.l,(byte)r.d); r.m=2; }
		public void LDHLmr_e() { mMU.wb((r.h<<8)+r.l,(byte)r.e); r.m=2; }
		public void LDHLmr_h() { mMU.wb((r.h<<8)+r.l,(byte)r.h); r.m=2; }
		public void LDHLmr_l() { mMU.wb((r.h<<8)+r.l,(byte)r.l); r.m=2; }
		public void LDHLmr_a() { mMU.wb((r.h<<8)+r.l,(byte)r.a); r.m=2; }

		public void LDrn_b() { r.b=mMU.rb(r.pc); r.pc++; r.m=2; }
		public void LDrn_c() { r.c=mMU.rb(r.pc); r.pc++; r.m=2; }
		public void LDrn_d() { r.d=mMU.rb(r.pc); r.pc++; r.m=2; }
		public void LDrn_e() { r.e=mMU.rb(r.pc); r.pc++; r.m=2; }
		public void LDrn_h() { r.h=mMU.rb(r.pc); r.pc++; r.m=2; }
		public void LDrn_l() { r.l=mMU.rb(r.pc); r.pc++; r.m=2; }
		public void LDrn_a() { r.a=mMU.rb(r.pc); r.pc++; r.m=2; }

		public void LDHLmn() { mMU.wb((r.h<<8)+r.l, (byte)mMU.rb(r.pc)); r.pc++; r.m=3; }

		public void LDBCmA() { mMU.wb((r.b<<8)+r.c, (byte)r.a); r.m=2; }
		public void LDDEmA() { mMU.wb((r.d<<8)+r.e, (byte)r.a); r.m=2; }

		public void LDmmA() { mMU.wb(mMU.rw(r.pc), (byte)r.a); r.pc+=2; r.m=4; }

		public void LDmmSP() { /*throw new Exception("Z80: LDmmSP not implemented");*/ }

		public void LDABCm() { r.a=mMU.rb((r.b<<8)+r.c); r.m=2; }
		public void LDADEm() { r.a=mMU.rb((r.d<<8)+r.e); r.m=2; }

		public void LDAmm() { i = mMU.rw (r.pc);r.a=(i<0x4000)?mMU._rom[i]:mMU.rb(i);r.pc+=2; r.m=4; }

		public void LDBCnn() { r.c=mMU.rb(r.pc); r.b=mMU.rb(r.pc+1); r.pc+=2; r.m=3; }
		public void LDDEnn() { r.e=mMU.rb(r.pc); r.d=mMU.rb(r.pc+1); r.pc+=2; r.m=3; }
		public void LDHLnn() { r.l=mMU.rb(r.pc); r.h=mMU.rb(r.pc+1); r.pc+=2; r.m=3; }
		public void LDSPnn() { r.sp=mMU.rw(r.pc); r.pc+=2; r.m=3; }

		public void LDHLmm() { i=mMU.rw(r.pc); r.pc+=2; r.l=mMU.rb(i); r.h=mMU.rb(i+1); r.m=5; }
		public void LDmmHL() { i=mMU.rw(r.pc); r.pc+=2; mMU.ww(i,(r.h<<8)+r.l); r.m=5; }

		public void LDHLIA() { mMU.wb((r.h<<8)+r.l, (byte)r.a); r.l=(r.l+1)&0xFF; if(!(r.l>0)) r.h=(r.h+1)&0xFF; r.m=2; }
		public void LDAHLI() { r.a=mMU.rb((r.h<<8)+r.l); r.l=(r.l+1)&0xFF; if(!(r.l>0)) r.h=(r.h+1)&0xFF; r.m=2; }

		public void LDHLDA() { mMU.wb((r.h<<8)+r.l, (byte)r.a); r.l=(r.l-1)&0xFF; if(r.l==0xFF) r.h=(r.h-1)&0xFF; r.m=2; }
		public void LDAHLD() { r.a=mMU.rb((r.h<<8)+r.l); r.l=(r.l-1)&0xFF; if(r.l==0xFF) r.h=(r.h-1)&0xFF; r.m=2; }

		public void LDAIOn() { r.a=mMU.rb(0xFF00+mMU.rb(r.pc)); r.pc++; r.m=3; }
		public void LDIOnA() { mMU.wb(0xFF00+mMU.rb(r.pc), (byte)r.a); r.pc++; r.m=3; }
		public void LDAIOC() { r.a=mMU.rb(0xFF00+r.c); r.m=2; }
		public void LDIOCA() { mMU.wb(0xFF00+r.c, (byte)r.a); r.m=2; }

		public void LDHLSPn() { i=mMU.rb(r.pc); if(i>127) i=-((~i+1)&0xFF); r.pc++; i+=r.sp; r.h=(i>>8)&0xFF; r.l=i&0xFF; r.m=3; }

		public void SWAPr_b() { tr=r.b; r.b=((tr&0xF)<<4)|((tr&0xF0)>>4); r.f=(r.b>0)?0:0x80; r.m=1; }
		public void SWAPr_c() { tr=r.c; r.c=((tr&0xF)<<4)|((tr&0xF0)>>4); r.f=(r.c>0)?0:0x80; r.m=1; }
		public void SWAPr_d() { tr=r.d; r.d=((tr&0xF)<<4)|((tr&0xF0)>>4); r.f=(r.d>0)?0:0x80; r.m=1; }
		public void SWAPr_e() { tr=r.e; r.e=((tr&0xF)<<4)|((tr&0xF0)>>4); r.f=(r.e>0)?0:0x80; r.m=1; }
		public void SWAPr_h() { tr=r.h; r.h=((tr&0xF)<<4)|((tr&0xF0)>>4); r.f=(r.h>0)?0:0x80; r.m=1; }
		public void SWAPr_l() { tr=r.l; r.l=((tr&0xF)<<4)|((tr&0xF0)>>4); r.f=(r.l>0)?0:0x80; r.m=1; }
		public void SWAPr_a() { tr=r.a; r.a=((tr&0xF)<<4)|((tr&0xF0)>>4); r.f=(r.a>0)?0:0x80; r.m=1; }

		/*--- Data processing ---*/
		public void ADDr_b() { a=r.a; r.a+=r.b; r.f=(r.a>0xFF)?0x10:0; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^r.b^a)&0x10)>0) r.f|=0x20; r.m=1; }
		public void ADDr_c() { a=r.a; r.a+=r.c; r.f=(r.a>0xFF)?0x10:0; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^r.c^a)&0x10)>0) r.f|=0x20; r.m=1; }
		public void ADDr_d() { a=r.a; r.a+=r.d; r.f=(r.a>0xFF)?0x10:0; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^r.d^a)&0x10)>0) r.f|=0x20; r.m=1; }
		public void ADDr_e() { a=r.a; r.a+=r.e; r.f=(r.a>0xFF)?0x10:0; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^r.e^a)&0x10)>0) r.f|=0x20; r.m=1; }
		public void ADDr_h() { a=r.a; r.a+=r.h; r.f=(r.a>0xFF)?0x10:0; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^r.h^a)&0x10)>0) r.f|=0x20; r.m=1; }
		public void ADDr_l() { a=r.a; r.a+=r.l; r.f=(r.a>0xFF)?0x10:0; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^r.l^a)&0x10)>0) r.f|=0x20; r.m=1; }
		public void ADDr_a() { a=r.a; r.a+=r.a; r.f=(r.a>0xFF)?0x10:0; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^r.a^a)&0x10)>0) r.f|=0x20; r.m=1; }
		public void ADDHL() { a=r.a; m=mMU.rb((r.h<<8)+r.l); r.a+=m; r.f=(r.a>0xFF)?0x10:0; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^a^m)&0x10)>0) r.f|=0x20; r.m=2; }
		public void ADDn() { a=r.a; m=mMU.rb(r.pc); r.a+=m; r.pc++; r.f=(r.a>0xFF)?0x10:0; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^a^m)&0x10)>0) r.f|=0x20; r.m=2; }
		public void ADDHLBC() { hl=(r.h<<8)+r.l; hl+=(r.b<<8)+r.c; if(hl>65535) r.f|=0x10; else r.f&=0xEF; r.h=(hl>>8)&0xFF; r.l=hl&0xFF; r.m=3; }
		public void ADDHLDE() { hl=(r.h<<8)+r.l; hl+=(r.d<<8)+r.e; if(hl>65535) r.f|=0x10; else r.f&=0xEF; r.h=(hl>>8)&0xFF; r.l=hl&0xFF; r.m=3; }
		public void ADDHLHL() { hl=(r.h<<8)+r.l; hl+=(r.h<<8)+r.l; if(hl>65535) r.f|=0x10; else r.f&=0xEF; r.h=(hl>>8)&0xFF; r.l=hl&0xFF; r.m=3; }
		public void ADDHLSP() { hl=(r.h<<8)+r.l; hl+=r.sp; if(hl>65535) r.f|=0x10; else r.f&=0xEF; r.h=(hl>>8)&0xFF; r.l=hl&0xFF; r.m=3; }
		public void ADDSPn() { i=mMU.rb(r.pc); if(i>127) i=-((~i+1)&0xFF); r.pc++; r.sp+=i; r.m=4; }

		public void ADCr_b() { a=r.a; r.a+=r.b; r.a+=((r.f&0x10)>0)?1:0; r.f=(r.a>0xFF)?0x10:0; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^r.b^a)&0x10)>0) r.f|=0x20; r.m=1; }
		public void ADCr_c() { a=r.a; r.a+=r.c; r.a+=((r.f&0x10)>0)?1:0; r.f=(r.a>0xFF)?0x10:0; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^r.c^a)&0x10)>0) r.f|=0x20; r.m=1; }
		public void ADCr_d() { a=r.a; r.a+=r.d; r.a+=((r.f&0x10)>0)?1:0; r.f=(r.a>0xFF)?0x10:0; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^r.d^a)&0x10)>0) r.f|=0x20; r.m=1; }
		public void ADCr_e() { a=r.a; r.a+=r.e; r.a+=((r.f&0x10)>0)?1:0; r.f=(r.a>0xFF)?0x10:0; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^r.e^a)&0x10)>0) r.f|=0x20; r.m=1; }
		public void ADCr_h() { a=r.a; r.a+=r.h; r.a+=((r.f&0x10)>0)?1:0; r.f=(r.a>0xFF)?0x10:0; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^r.h^a)&0x10)>0) r.f|=0x20; r.m=1; }
		public void ADCr_l() { a=r.a; r.a+=r.l; r.a+=((r.f&0x10)>0)?1:0; r.f=(r.a>0xFF)?0x10:0; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^r.l^a)&0x10)>0) r.f|=0x20; r.m=1; }
		public void ADCr_a() { a=r.a; r.a+=r.a; r.a+=((r.f&0x10)>0)?1:0; r.f=(r.a>0xFF)?0x10:0; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^r.a^a)&0x10)>0) r.f|=0x20; r.m=1; }
		public void ADCHL() { a=r.a; m=mMU.rb((r.h<<8)+r.l); r.a+=m; r.a+=((r.f&0x10)>0)?1:0; r.f=(r.a>0xFF)?0x10:0; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^m^a)&0x10)>0) r.f|=0x20; r.m=2; }
		public void ADCn() { a=r.a; m=mMU.rb(r.pc); r.a+=m; r.pc++; r.a+=((r.f&0x10)>0)?1:0; r.f=(r.a>0xFF)?0x10:0; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^m^a)&0x10)>0) r.f|=0x20; r.m=2; }

		public void SUBr_b() { a=r.a; r.a-=r.b; r.f=(r.a<0)?0x50:0x40; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^r.b^a)&0x10)>0) r.f|=0x20; r.m=1; }
		public void SUBr_c() { a=r.a; r.a-=r.c; r.f=(r.a<0)?0x50:0x40; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^r.c^a)&0x10)>0) r.f|=0x20; r.m=1; }
		public void SUBr_d() { a=r.a; r.a-=r.d; r.f=(r.a<0)?0x50:0x40; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^r.d^a)&0x10)>0) r.f|=0x20; r.m=1; }
		public void SUBr_e() { a=r.a; r.a-=r.e; r.f=(r.a<0)?0x50:0x40; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^r.e^a)&0x10)>0) r.f|=0x20; r.m=1; }
		public void SUBr_h() { a=r.a; r.a-=r.h; r.f=(r.a<0)?0x50:0x40; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^r.h^a)&0x10)>0) r.f|=0x20; r.m=1; }
		public void SUBr_l() { a=r.a; r.a-=r.l; r.f=(r.a<0)?0x50:0x40; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^r.l^a)&0x10)>0) r.f|=0x20; r.m=1; }
		public void SUBr_a() { a=r.a; r.a-=r.a; r.f=(r.a<0)?0x50:0x40; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^r.a^a)&0x10)>0) r.f|=0x20; r.m=1; }
		public void SUBHL() { a=r.a; m=mMU.rb((r.h<<8)+r.l); r.a-=m; r.f=(r.a<0)?0x50:0x40; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^m^a)&0x10)>0) r.f|=0x20; r.m=2; }
		public void SUBn() { a=r.a; m=mMU.rb(r.pc); r.a-=m; r.pc++; r.f=(r.a<0)?0x50:0x40; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^m^a)&0x10)>0) r.f|=0x20; r.m=2; }

		public void SBCr_b() { a=r.a; r.a-=r.b; r.a-=((r.f&0x10)>0)?1:0; r.f=(r.a<0)?0x50:0x40; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^r.b^a)&0x10)>0) r.f|=0x20; r.m=1; }
		public void SBCr_c() { a=r.a; r.a-=r.c; r.a-=((r.f&0x10)>0)?1:0; r.f=(r.a<0)?0x50:0x40; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^r.c^a)&0x10)>0) r.f|=0x20; r.m=1; }
		public void SBCr_d() { a=r.a; r.a-=r.d; r.a-=((r.f&0x10)>0)?1:0; r.f=(r.a<0)?0x50:0x40; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^r.d^a)&0x10)>0) r.f|=0x20; r.m=1; }
		public void SBCr_e() { a=r.a; r.a-=r.e; r.a-=((r.f&0x10)>0)?1:0; r.f=(r.a<0)?0x50:0x40; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^r.e^a)&0x10)>0) r.f|=0x20; r.m=1; }
		public void SBCr_h() { a=r.a; r.a-=r.h; r.a-=((r.f&0x10)>0)?1:0; r.f=(r.a<0)?0x50:0x40; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^r.h^a)&0x10)>0) r.f|=0x20; r.m=1; }
		public void SBCr_l() { a=r.a; r.a-=r.l; r.a-=((r.f&0x10)>0)?1:0; r.f=(r.a<0)?0x50:0x40; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^r.l^a)&0x10)>0) r.f|=0x20; r.m=1; }
		public void SBCr_a() { a=r.a; r.a-=r.a; r.a-=((r.f&0x10)>0)?1:0; r.f=(r.a<0)?0x50:0x40; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^r.a^a)&0x10)>0) r.f|=0x20; r.m=1; }
		public void SBCHL() { a=r.a; m=mMU.rb((r.h<<8)+r.l); r.a-=m; r.a-=((r.f&0x10)>0)?1:0; r.f=(r.a<0)?0x50:0x40; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^m^a)&0x10)>0) r.f|=0x20; r.m=2; }
		public void SBCn() { a=r.a; m=mMU.rb(r.pc); r.a-=m; r.pc++; r.a-=((r.f&0x10)>0)?1:0; r.f=(r.a<0)?0x50:0x40; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; if(((r.a^m^a)&0x10)>0) r.f|=0x20; r.m=2; }

		public void CPr_b() { int i=r.a; i-=r.b; r.f=(i<0)?0x50:0x40; i&=0xFF; if(!(i>0)) r.f|=0x80; if(((r.a^r.b^i)&0x10)>0) r.f|=0x20; r.m=1; }
		public void CPr_c() { int i=r.a; i-=r.c; r.f=(i<0)?0x50:0x40; i&=0xFF; if(!(i>0)) r.f|=0x80; if(((r.a^r.c^i)&0x10)>0) r.f|=0x20; r.m=1; }
		public void CPr_d() { int i=r.a; i-=r.d; r.f=(i<0)?0x50:0x40; i&=0xFF; if(!(i>0)) r.f|=0x80; if(((r.a^r.d^i)&0x10)>0) r.f|=0x20; r.m=1; }
		public void CPr_e() { int i=r.a; i-=r.e; r.f=(i<0)?0x50:0x40; i&=0xFF; if(!(i>0)) r.f|=0x80; if(((r.a^r.e^i)&0x10)>0) r.f|=0x20; r.m=1; }
		public void CPr_h() { int i=r.a; i-=r.h; r.f=(i<0)?0x50:0x40; i&=0xFF; if(!(i>0)) r.f|=0x80; if(((r.a^r.h^i)&0x10)>0) r.f|=0x20; r.m=1; }
		public void CPr_l() { int i=r.a; i-=r.l; r.f=(i<0)?0x50:0x40; i&=0xFF; if(!(i>0)) r.f|=0x80; if(((r.a^r.l^i)&0x10)>0) r.f|=0x20; r.m=1; }
		public void CPr_a() { int i=r.a; i-=r.a; r.f=(i<0)?0x50:0x40; i&=0xFF; if(!(i>0)) r.f|=0x80; if(((r.a^r.a^i)&0x10)>0) r.f|=0x20; r.m=1; }
		public void CPHL() { int i=r.a; m=mMU.rb((r.h<<8)+r.l); i-=m; r.f=(i<0)?0x50:0x40; i&=0xFF; if(!(i>0)) r.f|=0x80; if(((r.a^i^m)&0x10)>0) r.f|=0x20; r.m=2; }
		public void CPn() { int i=r.a; m=mMU.rb(r.pc); i-=m; r.pc++; r.f=(i<0)?0x50:0x40; i&=0xFF; if(!(i>0)) r.f|=0x80; if(((r.a^i^m)&0x10)>0) r.f|=0x20; r.m=2; }

		public void DAA() { a=r.a; if((r.f&0x20)>0||((r.a&15)>9)) r.a+=6; r.f&=0xEF; if(((r.f&0x20) > 0)||(a>0x99)) { r.a+=0x60; r.f|=0x10; } r.m=1; }

		public void ANDr_b() { r.a&=r.b; r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.m=1; }
		public void ANDr_c() { r.a&=r.c; r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.m=1; }
		public void ANDr_d() { r.a&=r.d; r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.m=1; }
		public void ANDr_e() { r.a&=r.e; r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.m=1; }
		public void ANDr_h() { r.a&=r.h; r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.m=1; }
		public void ANDr_l() { r.a&=r.l; r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.m=1; }
		public void ANDr_a() { r.a&=r.a; r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.m=1; }
		public void ANDHL() { r.a&=mMU.rb((r.h<<8)+r.l); r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.m=2; }
		public void ANDn() { r.a&=mMU.rb(r.pc); r.pc++; r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.m=2; }

		public void ORr_b() { r.a|=r.b; r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.m=1; }
		public void ORr_c() { r.a|=r.c; r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.m=1; }
		public void ORr_d() { r.a|=r.d; r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.m=1; }
		public void ORr_e() { r.a|=r.e; r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.m=1; }
		public void ORr_h() { r.a|=r.h; r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.m=1; }
		public void ORr_l() { r.a|=r.l; r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.m=1; }
		public void ORr_a() { r.a|=r.a; r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.m=1; }
		public void ORHL() { r.a|=mMU.rb((r.h<<8)+r.l); r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.m=2; }
		public void ORn() { r.a|=mMU.rb(r.pc); r.pc++; r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.m=2; }

		public void XORr_b() { r.a^=r.b; r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.m=1; }
		public void XORr_c() { r.a^=r.c; r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.m=1; }
		public void XORr_d() { r.a^=r.d; r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.m=1; }
		public void XORr_e() { r.a^=r.e; r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.m=1; }
		public void XORr_h() { r.a^=r.h; r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.m=1; }
		public void XORr_l() { r.a^=r.l; r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.m=1; }
		public void XORr_a() { r.a^=r.a; r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.m=1; }
		public void XORHL() { r.a^=mMU.rb((r.h<<8)+r.l); r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.m=2; }
		public void XORn() { r.a^=mMU.rb(r.pc); r.pc++; r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.m=2; }

		public void INCr_b() { r.b++; r.b&=0xFF; r.f=(r.b>0)?0:0x80; r.m=1; }
		public void INCr_c() { r.c++; r.c&=0xFF; r.f=(r.c>0)?0:0x80; r.m=1; }
		public void INCr_d() { r.d++; r.d&=0xFF; r.f=(r.d>0)?0:0x80; r.m=1; }
		public void INCr_e() { r.e++; r.e&=0xFF; r.f=(r.e>0)?0:0x80; r.m=1; }
		public void INCr_h() { r.h++; r.h&=0xFF; r.f=(r.h>0)?0:0x80; r.m=1; }
		public void INCr_l() { r.l++; r.l&=0xFF; r.f=(r.l>0)?0:0x80; r.m=1; }
		public void INCr_a() { r.a++; r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.m=1; }
		public void INCHLm() { int i=mMU.rb((r.h<<8)+r.l)+1; i&=0xFF; mMU.wb((r.h<<8)+r.l,(byte)i); r.f=(i>0)?0:0x80; r.m=3; }

		public void DECr_b() { r.b--; r.b&=0xFF; r.f=(r.b>0)?0:0x80; r.m=1; }
		public void DECr_c() { r.c--; r.c&=0xFF; r.f=(r.c>0)?0:0x80; r.m=1; }
		public void DECr_d() { r.d--; r.d&=0xFF; r.f=(r.d>0)?0:0x80; r.m=1; }
		public void DECr_e() { r.e--; r.e&=0xFF; r.f=(r.e>0)?0:0x80; r.m=1; }
		public void DECr_h() { r.h--; r.h&=0xFF; r.f=(r.h>0)?0:0x80; r.m=1; }
		public void DECr_l() { r.l--; r.l&=0xFF; r.f=(r.l>0)?0:0x80; r.m=1; }
		public void DECr_a() { r.a--; r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.m=1; }
		public void DECHLm() { int i=mMU.rb((r.h<<8)+r.l)-1; i&=0xFF; mMU.wb((r.h<<8)+r.l,(byte)i); r.f=(i>0)?0:0x80; r.m=3; }

		public void INCBC() { r.c=(r.c+1)&0xFF; if(!(r.c>0)) r.b=(r.b+1)&0xFF; r.m=1; }
		public void INCDE() { r.e=(r.e+1)&0xFF; if(!(r.e>0)) r.d=(r.d+1)&0xFF; r.m=1; }
		public void INCHL() { r.l=(r.l+1)&0xFF; if(!(r.l>0)) r.h=(r.h+1)&0xFF; r.m=1; }
		public void INCSP() { r.sp=(r.sp+1)&65535; r.m=1; }

		public void DECBC() { r.c=(r.c-1)&0xFF; if(r.c==0xFF) r.b=(r.b-1)&0xFF; r.m=1; }
		public void DECDE() { r.e=(r.e-1)&0xFF; if(r.e==0xFF) r.d=(r.d-1)&0xFF; r.m=1; }
		public void DECHL() { r.l=(r.l-1)&0xFF; if(r.l==0xFF) r.h=(r.h-1)&0xFF; r.m=1; }
		public void DECSP() { r.sp=(r.sp-1)&65535; r.m=1; }

		/*--- Bit manipulation ---*/
		public void BIT0b() { r.f&=0x1F; r.f|=0x20; r.f=((r.b&0x01)>0)?0:0x80; r.m=2; }
		public void BIT0c() { r.f&=0x1F; r.f|=0x20; r.f=((r.c&0x01)>0)?0:0x80; r.m=2; }
		public void BIT0d() { r.f&=0x1F; r.f|=0x20; r.f=((r.d&0x01)>0)?0:0x80; r.m=2; }
		public void BIT0e() { r.f&=0x1F; r.f|=0x20; r.f=((r.e&0x01)>0)?0:0x80; r.m=2; }
		public void BIT0h() { r.f&=0x1F; r.f|=0x20; r.f=((r.h&0x01)>0)?0:0x80; r.m=2; }
		public void BIT0l() { r.f&=0x1F; r.f|=0x20; r.f=((r.l&0x01)>0)?0:0x80; r.m=2; }
		public void BIT0a() { r.f&=0x1F; r.f|=0x20; r.f=((r.a&0x01)>0)?0:0x80; r.m=2; }
		public void BIT0m() { r.f&=0x1F; r.f|=0x20; r.f=((mMU.rb((r.h<<8)+r.l)&0x01)>0)?0:0x80; r.m=3; }

		public void RES0b() { r.b&=0xFE; r.m=2; }
		public void RES0c() { r.c&=0xFE; r.m=2; }
		public void RES0d() { r.d&=0xFE; r.m=2; }
		public void RES0e() { r.e&=0xFE; r.m=2; }
		public void RES0h() { r.h&=0xFE; r.m=2; }
		public void RES0l() { r.l&=0xFE; r.m=2; }
		public void RES0a() { r.a&=0xFE; r.m=2; }
		public void RES0m() { int i=mMU.rb((r.h<<8)+r.l); i&=0xFE; mMU.wb((r.h<<8)+r.l,(byte)i); r.m=4; }

		public void SET0b() { r.b|=0x01; r.m=2; }
		public void SET0c() { r.b|=0x01; r.m=2; }
		public void SET0d() { r.b|=0x01; r.m=2; }
		public void SET0e() { r.b|=0x01; r.m=2; }
		public void SET0h() { r.b|=0x01; r.m=2; }
		public void SET0l() { r.b|=0x01; r.m=2; }
		public void SET0a() { r.b|=0x01; r.m=2; }
		public void SET0m() { int i=mMU.rb((r.h<<8)+r.l); i|=0x01; mMU.wb((r.h<<8)+r.l,(byte)i); r.m=4; }

		public void BIT1b() { r.f&=0x1F; r.f|=0x20; r.f=((r.b&0x02)>0)?0:0x80; r.m=2; }
		public void BIT1c() { r.f&=0x1F; r.f|=0x20; r.f=((r.c&0x02)>0)?0:0x80; r.m=2; }
		public void BIT1d() { r.f&=0x1F; r.f|=0x20; r.f=((r.d&0x02)>0)?0:0x80; r.m=2; }
		public void BIT1e() { r.f&=0x1F; r.f|=0x20; r.f=((r.e&0x02)>0)?0:0x80; r.m=2; }
		public void BIT1h() { r.f&=0x1F; r.f|=0x20; r.f=((r.h&0x02)>0)?0:0x80; r.m=2; }
		public void BIT1l() { r.f&=0x1F; r.f|=0x20; r.f=((r.l&0x02)>0)?0:0x80; r.m=2; }
		public void BIT1a() { r.f&=0x1F; r.f|=0x20; r.f=((r.a&0x02)>0)?0:0x80; r.m=2; }
		public void BIT1m() { r.f&=0x1F; r.f|=0x20; r.f=((mMU.rb((r.h<<8)+r.l)&0x02)>0)?0:0x80; r.m=3; }

		public void RES1b() { r.b&=0xFD; r.m=2; }
		public void RES1c() { r.c&=0xFD; r.m=2; }
		public void RES1d() { r.d&=0xFD; r.m=2; }
		public void RES1e() { r.e&=0xFD; r.m=2; }
		public void RES1h() { r.h&=0xFD; r.m=2; }
		public void RES1l() { r.l&=0xFD; r.m=2; }
		public void RES1a() { r.a&=0xFD; r.m=2; }
		public void RES1m() { int i=mMU.rb((r.h<<8)+r.l); i&=0xFD; mMU.wb((r.h<<8)+r.l,(byte)i); r.m=4; }

		public void SET1b() { r.b|=0x02; r.m=2; }
		public void SET1c() { r.b|=0x02; r.m=2; }
		public void SET1d() { r.b|=0x02; r.m=2; }
		public void SET1e() { r.b|=0x02; r.m=2; }
		public void SET1h() { r.b|=0x02; r.m=2; }
		public void SET1l() { r.b|=0x02; r.m=2; }
		public void SET1a() { r.b|=0x02; r.m=2; }
		public void SET1m() { int i=mMU.rb((r.h<<8)+r.l); i|=0x02; mMU.wb((r.h<<8)+r.l,(byte)i); r.m=4; }

		public void BIT2b() { r.f&=0x1F; r.f|=0x20; r.f=((r.b&0x04)>0)?0:0x80; r.m=2; }
		public void BIT2c() { r.f&=0x1F; r.f|=0x20; r.f=((r.c&0x04)>0)?0:0x80; r.m=2; }
		public void BIT2d() { r.f&=0x1F; r.f|=0x20; r.f=((r.d&0x04)>0)?0:0x80; r.m=2; }
		public void BIT2e() { r.f&=0x1F; r.f|=0x20; r.f=((r.e&0x04)>0)?0:0x80; r.m=2; }
		public void BIT2h() { r.f&=0x1F; r.f|=0x20; r.f=((r.h&0x04)>0)?0:0x80; r.m=2; }
		public void BIT2l() { r.f&=0x1F; r.f|=0x20; r.f=((r.l&0x04)>0)?0:0x80; r.m=2; }
		public void BIT2a() { r.f&=0x1F; r.f|=0x20; r.f=((r.a&0x04)>0)?0:0x80; r.m=2; }
		public void BIT2m() { r.f&=0x1F; r.f|=0x20; r.f=((mMU.rb((r.h<<8)+r.l)&0x04)>0)?0:0x80; r.m=3; }

		public void RES2b() { r.b&=0xFB; r.m=2; }
		public void RES2c() { r.c&=0xFB; r.m=2; }
		public void RES2d() { r.d&=0xFB; r.m=2; }
		public void RES2e() { r.e&=0xFB; r.m=2; }
		public void RES2h() { r.h&=0xFB; r.m=2; }
		public void RES2l() { r.l&=0xFB; r.m=2; }
		public void RES2a() { r.a&=0xFB; r.m=2; }
		public void RES2m() { int i=mMU.rb((r.h<<8)+r.l); i&=0xFB; mMU.wb((r.h<<8)+r.l,(byte)i); r.m=4; }

		public void SET2b() { r.b|=0x04; r.m=2; }
		public void SET2c() { r.b|=0x04; r.m=2; }
		public void SET2d() { r.b|=0x04; r.m=2; }
		public void SET2e() { r.b|=0x04; r.m=2; }
		public void SET2h() { r.b|=0x04; r.m=2; }
		public void SET2l() { r.b|=0x04; r.m=2; }
		public void SET2a() { r.b|=0x04; r.m=2; }
		public void SET2m() { int i=mMU.rb((r.h<<8)+r.l); i|=0x04; mMU.wb((r.h<<8)+r.l,(byte)i); r.m=4; }

		public void BIT3b() { r.f&=0x1F; r.f|=0x20; r.f=((r.b&0x08)>0)?0:0x80; r.m=2; }
		public void BIT3c() { r.f&=0x1F; r.f|=0x20; r.f=((r.c&0x08)>0)?0:0x80; r.m=2; }
		public void BIT3d() { r.f&=0x1F; r.f|=0x20; r.f=((r.d&0x08)>0)?0:0x80; r.m=2; }
		public void BIT3e() { r.f&=0x1F; r.f|=0x20; r.f=((r.e&0x08)>0)?0:0x80; r.m=2; }
		public void BIT3h() { r.f&=0x1F; r.f|=0x20; r.f=((r.h&0x08)>0)?0:0x80; r.m=2; }
		public void BIT3l() { r.f&=0x1F; r.f|=0x20; r.f=((r.l&0x08)>0)?0:0x80; r.m=2; }
		public void BIT3a() { r.f&=0x1F; r.f|=0x20; r.f=((r.a&0x08)>0)?0:0x80; r.m=2; }
		public void BIT3m() { r.f&=0x1F; r.f|=0x20; r.f=((mMU.rb((r.h<<8)+r.l)&0x08)>0)?0:0x80; r.m=3; }

		public void RES3b() { r.b&=0xF7; r.m=2; }
		public void RES3c() { r.c&=0xF7; r.m=2; }
		public void RES3d() { r.d&=0xF7; r.m=2; }
		public void RES3e() { r.e&=0xF7; r.m=2; }
		public void RES3h() { r.h&=0xF7; r.m=2; }
		public void RES3l() { r.l&=0xF7; r.m=2; }
		public void RES3a() { r.a&=0xF7; r.m=2; }
		public void RES3m() { int i=mMU.rb((r.h<<8)+r.l); i&=0xF7; mMU.wb((r.h<<8)+r.l,(byte)i); r.m=4; }

		public void SET3b() { r.b|=0x08; r.m=2; }
		public void SET3c() { r.b|=0x08; r.m=2; }
		public void SET3d() { r.b|=0x08; r.m=2; }
		public void SET3e() { r.b|=0x08; r.m=2; }
		public void SET3h() { r.b|=0x08; r.m=2; }
		public void SET3l() { r.b|=0x08; r.m=2; }
		public void SET3a() { r.b|=0x08; r.m=2; }
		public void SET3m() { int i=mMU.rb((r.h<<8)+r.l); i|=0x08; mMU.wb((r.h<<8)+r.l,(byte)i); r.m=4; }

		public void BIT4b() { r.f&=0x1F; r.f|=0x20; r.f=((r.b&0x10)>0)?0:0x80; r.m=2; }
		public void BIT4c() { r.f&=0x1F; r.f|=0x20; r.f=((r.c&0x10)>0)?0:0x80; r.m=2; }
		public void BIT4d() { r.f&=0x1F; r.f|=0x20; r.f=((r.d&0x10)>0)?0:0x80; r.m=2; }
		public void BIT4e() { r.f&=0x1F; r.f|=0x20; r.f=((r.e&0x10)>0)?0:0x80; r.m=2; }
		public void BIT4h() { r.f&=0x1F; r.f|=0x20; r.f=((r.h&0x10)>0)?0:0x80; r.m=2; }
		public void BIT4l() { r.f&=0x1F; r.f|=0x20; r.f=((r.l&0x10)>0)?0:0x80; r.m=2; }
		public void BIT4a() { r.f&=0x1F; r.f|=0x20; r.f=((r.a&0x10)>0)?0:0x80; r.m=2; }
		public void BIT4m() { r.f&=0x1F; r.f|=0x20; r.f=((mMU.rb((r.h<<8)+r.l)&0x10)>0)?0:0x80; r.m=3; }

		public void RES4b() { r.b&=0xEF; r.m=2; }
		public void RES4c() { r.c&=0xEF; r.m=2; }
		public void RES4d() { r.d&=0xEF; r.m=2; }
		public void RES4e() { r.e&=0xEF; r.m=2; }
		public void RES4h() { r.h&=0xEF; r.m=2; }
		public void RES4l() { r.l&=0xEF; r.m=2; }
		public void RES4a() { r.a&=0xEF; r.m=2; }
		public void RES4m() { int i=mMU.rb((r.h<<8)+r.l); i&=0xEF; mMU.wb((r.h<<8)+r.l,(byte)i); r.m=4; }

		public void SET4b() { r.b|=0x10; r.m=2; }
		public void SET4c() { r.b|=0x10; r.m=2; }
		public void SET4d() { r.b|=0x10; r.m=2; }
		public void SET4e() { r.b|=0x10; r.m=2; }
		public void SET4h() { r.b|=0x10; r.m=2; }
		public void SET4l() { r.b|=0x10; r.m=2; }
		public void SET4a() { r.b|=0x10; r.m=2; }
		public void SET4m() { int i=mMU.rb((r.h<<8)+r.l); i|=0x10; mMU.wb((r.h<<8)+r.l,(byte)i); r.m=4; }

		public void BIT5b() { r.f&=0x1F; r.f|=0x20; r.f=((r.b&0x20)>0)?0:0x80; r.m=2; }
		public void BIT5c() { r.f&=0x1F; r.f|=0x20; r.f=((r.c&0x20)>0)?0:0x80; r.m=2; }
		public void BIT5d() { r.f&=0x1F; r.f|=0x20; r.f=((r.d&0x20)>0)?0:0x80; r.m=2; }
		public void BIT5e() { r.f&=0x1F; r.f|=0x20; r.f=((r.e&0x20)>0)?0:0x80; r.m=2; }
		public void BIT5h() { r.f&=0x1F; r.f|=0x20; r.f=((r.h&0x20)>0)?0:0x80; r.m=2; }
		public void BIT5l() { r.f&=0x1F; r.f|=0x20; r.f=((r.l&0x20)>0)?0:0x80; r.m=2; }
		public void BIT5a() { r.f&=0x1F; r.f|=0x20; r.f=((r.a&0x20)>0)?0:0x80; r.m=2; }
		public void BIT5m() { r.f&=0x1F; r.f|=0x20; r.f=((mMU.rb((r.h<<8)+r.l)&0x20)>0)?0:0x80; r.m=3; }

		public void RES5b() { r.b&=0xDF; r.m=2; }
		public void RES5c() { r.c&=0xDF; r.m=2; }
		public void RES5d() { r.d&=0xDF; r.m=2; }
		public void RES5e() { r.e&=0xDF; r.m=2; }
		public void RES5h() { r.h&=0xDF; r.m=2; }
		public void RES5l() { r.l&=0xDF; r.m=2; }
		public void RES5a() { r.a&=0xDF; r.m=2; }
		public void RES5m() { int i=mMU.rb((r.h<<8)+r.l); i&=0xDF; mMU.wb((r.h<<8)+r.l,(byte)i); r.m=4; }

		public void SET5b() { r.b|=0x20; r.m=2; }
		public void SET5c() { r.b|=0x20; r.m=2; }
		public void SET5d() { r.b|=0x20; r.m=2; }
		public void SET5e() { r.b|=0x20; r.m=2; }
		public void SET5h() { r.b|=0x20; r.m=2; }
		public void SET5l() { r.b|=0x20; r.m=2; }
		public void SET5a() { r.b|=0x20; r.m=2; }
		public void SET5m() { int i=mMU.rb((r.h<<8)+r.l); i|=0x20; mMU.wb((r.h<<8)+r.l,(byte)i); r.m=4; }

		public void BIT6b() { r.f&=0x1F; r.f|=0x20; r.f=((r.b&0x40)>0)?0:0x80; r.m=2; }
		public void BIT6c() { r.f&=0x1F; r.f|=0x20; r.f=((r.c&0x40)>0)?0:0x80; r.m=2; }
		public void BIT6d() { r.f&=0x1F; r.f|=0x20; r.f=((r.d&0x40)>0)?0:0x80; r.m=2; }
		public void BIT6e() { r.f&=0x1F; r.f|=0x20; r.f=((r.e&0x40)>0)?0:0x80; r.m=2; }
		public void BIT6h() { r.f&=0x1F; r.f|=0x20; r.f=((r.h&0x40)>0)?0:0x80; r.m=2; }
		public void BIT6l() { r.f&=0x1F; r.f|=0x20; r.f=((r.l&0x40)>0)?0:0x80; r.m=2; }
		public void BIT6a() { r.f&=0x1F; r.f|=0x20; r.f=((r.a&0x40)>0)?0:0x80; r.m=2; }
		public void BIT6m() { r.f&=0x1F; r.f|=0x20; r.f=((mMU.rb((r.h<<8)+r.l)&0x40)>0)?0:0x80; r.m=3; }

		public void RES6b() { r.b&=0xBF; r.m=2; }
		public void RES6c() { r.c&=0xBF; r.m=2; }
		public void RES6d() { r.d&=0xBF; r.m=2; }
		public void RES6e() { r.e&=0xBF; r.m=2; }
		public void RES6h() { r.h&=0xBF; r.m=2; }
		public void RES6l() { r.l&=0xBF; r.m=2; }
		public void RES6a() { r.a&=0xBF; r.m=2; }
		public void RES6m() { int i=mMU.rb((r.h<<8)+r.l); i&=0xBF; mMU.wb((r.h<<8)+r.l,(byte)i); r.m=4; }

		public void SET6b() { r.b|=0x40; r.m=2; }
		public void SET6c() { r.b|=0x40; r.m=2; }
		public void SET6d() { r.b|=0x40; r.m=2; }
		public void SET6e() { r.b|=0x40; r.m=2; }
		public void SET6h() { r.b|=0x40; r.m=2; }
		public void SET6l() { r.b|=0x40; r.m=2; }
		public void SET6a() { r.b|=0x40; r.m=2; }
		public void SET6m() { int i=mMU.rb((r.h<<8)+r.l); i|=0x40; mMU.wb((r.h<<8)+r.l,(byte)i); r.m=4; }

		public void BIT7b() { r.f&=0x1F; r.f|=0x20; r.f=((r.b&0x80)>0)?0:0x80; r.m=2; }
		public void BIT7c() { r.f&=0x1F; r.f|=0x20; r.f=((r.c&0x80)>0)?0:0x80; r.m=2; }
		public void BIT7d() { r.f&=0x1F; r.f|=0x20; r.f=((r.d&0x80)>0)?0:0x80; r.m=2; }
		public void BIT7e() { r.f&=0x1F; r.f|=0x20; r.f=((r.e&0x80)>0)?0:0x80; r.m=2; }
		public void BIT7h() { r.f&=0x1F; r.f|=0x20; r.f=((r.h&0x80)>0)?0:0x80; r.m=2; }
		public void BIT7l() { r.f&=0x1F; r.f|=0x20; r.f=((r.l&0x80)>0)?0:0x80; r.m=2; }
		public void BIT7a() { r.f&=0x1F; r.f|=0x20; r.f=((r.a&0x80)>0)?0:0x80; r.m=2; }
		public void BIT7m() { r.f&=0x1F; r.f|=0x20; r.f=((mMU.rb((r.h<<8)+r.l)&0x80)>0)?0:0x80; r.m=3; }

		public void RES7b() { r.b&=0x7F; r.m=2; }
		public void RES7c() { r.c&=0x7F; r.m=2; }
		public void RES7d() { r.d&=0x7F; r.m=2; }
		public void RES7e() { r.e&=0x7F; r.m=2; }
		public void RES7h() { r.h&=0x7F; r.m=2; }
		public void RES7l() { r.l&=0x7F; r.m=2; }
		public void RES7a() { r.a&=0x7F; r.m=2; }
		public void RES7m() { int i=mMU.rb((r.h<<8)+r.l); i&=0x7F; mMU.wb((r.h<<8)+r.l,(byte)i); r.m=4; }

		public void SET7b() { r.b|=0x80; r.m=2; }
		public void SET7c() { r.b|=0x80; r.m=2; }
		public void SET7d() { r.b|=0x80; r.m=2; }
		public void SET7e() { r.b|=0x80; r.m=2; }
		public void SET7h() { r.b|=0x80; r.m=2; }
		public void SET7l() { r.b|=0x80; r.m=2; }
		public void SET7a() { r.b|=0x80; r.m=2; }
		public void SET7m() { int i=mMU.rb((r.h<<8)+r.l); i|=0x80; mMU.wb((r.h<<8)+r.l,(byte)i); r.m=4; }

		public void RLA() { ci=((r.f&0x10)>0)?1:0; co=((r.a&0x80)>0)?0x10:0; r.a=(r.a<<1)+ci; r.a&=0xFF; r.f=(r.f&0xEF)+co; r.m=1; }
		public void RLCA() { ci=((r.a&0x80)>0)?1:0; co=((r.a&0x80)>0)?0x10:0; r.a=(r.a<<1)+ci; r.a&=0xFF; r.f=(r.f&0xEF)+co; r.m=1; }
		public void RRA() { ci=((r.f&0x10)>0)?0x80:0; co=((r.a&1)>0)?0x10:0; r.a=(r.a>>1)+ci; r.a&=0xFF; r.f=(r.f&0xEF)+co; r.m=1; }
		public void RRCA() { ci=((r.a&1)>0)?0x80:0; co=((r.a&1)>0)?0x10:0; r.a=(r.a>>1)+ci; r.a&=0xFF; r.f=(r.f&0xEF)+co; r.m=1; }

		public void RLr_b() { ci=((r.f&0x10)>0)?1:0; co=((r.b&0x80)>0)?0x10:0; r.b=(r.b<<1)+ci; r.b&=0xFF; r.f=(r.b>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void RLr_c() { ci=((r.f&0x10)>0)?1:0; co=((r.c&0x80)>0)?0x10:0; r.c=(r.c<<1)+ci; r.c&=0xFF; r.f=(r.c>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void RLr_d() { ci=((r.f&0x10)>0)?1:0; co=((r.d&0x80)>0)?0x10:0; r.d=(r.d<<1)+ci; r.d&=0xFF; r.f=(r.d>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void RLr_e() { ci=((r.f&0x10)>0)?1:0; co=((r.e&0x80)>0)?0x10:0; r.e=(r.e<<1)+ci; r.e&=0xFF; r.f=(r.e>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void RLr_h() { ci=((r.f&0x10)>0)?1:0; co=((r.h&0x80)>0)?0x10:0; r.h=(r.h<<1)+ci; r.h&=0xFF; r.f=(r.h>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void RLr_l() { ci=((r.f&0x10)>0)?1:0; co=((r.l&0x80)>0)?0x10:0; r.l=(r.l<<1)+ci; r.l&=0xFF; r.f=(r.l>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void RLr_a() { ci=((r.f&0x10)>0)?1:0; co=((r.a&0x80)>0)?0x10:0; r.a=(r.a<<1)+ci; r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void RLHL() { int i=mMU.rb((r.h<<8)+r.l); ci=((r.f&0x10)>0)?1:0; co=((i&0x80)>0)?0x10:0; i=(i<<1)+ci; i&=0xFF; r.f=(i>0)?0:0x80; mMU.wb((r.h<<8)+r.l,(byte)i); r.f=(r.f&0xEF)+co; r.m=4; }

		public void RLCr_b() { ci=((r.b&0x80)>0)?1:0; co=((r.b&0x80)>0)?0x10:0; r.b=(r.b<<1)+ci; r.b&=0xFF; r.f=(r.b>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void RLCr_c() { ci=((r.c&0x80)>0)?1:0; co=((r.c&0x80)>0)?0x10:0; r.c=(r.c<<1)+ci; r.c&=0xFF; r.f=(r.c>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void RLCr_d() { ci=((r.d&0x80)>0)?1:0; co=((r.d&0x80)>0)?0x10:0; r.d=(r.d<<1)+ci; r.d&=0xFF; r.f=(r.d>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void RLCr_e() { ci=((r.e&0x80)>0)?1:0; co=((r.e&0x80)>0)?0x10:0; r.e=(r.e<<1)+ci; r.e&=0xFF; r.f=(r.e>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void RLCr_h() { ci=((r.h&0x80)>0)?1:0; co=((r.h&0x80)>0)?0x10:0; r.h=(r.h<<1)+ci; r.h&=0xFF; r.f=(r.h>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void RLCr_l() { ci=((r.l&0x80)>0)?1:0; co=((r.l&0x80)>0)?0x10:0; r.l=(r.l<<1)+ci; r.l&=0xFF; r.f=(r.l>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void RLCr_a() { ci=((r.a&0x80)>0)?1:0; co=((r.a&0x80)>0)?0x10:0; r.a=(r.a<<1)+ci; r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void RLCHL() { int i=mMU.rb((r.h<<8)+r.l); ci=((i&0x80)>0)?1:0; co=((i&0x80)>0)?0x10:0; i=(i<<1)+ci; i&=0xFF; r.f=(i>0)?0:0x80; mMU.wb((r.h<<8)+r.l,(byte)i); r.f=(r.f&0xEF)+co; r.m=4; }

		public void RRr_b() { ci=((r.f&0x10)>0)?0x80:0; co=((r.b&1)>0)?0x10:0; r.b=(r.b>>1)+ci; r.b&=0xFF; r.f=(r.b>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void RRr_c() { ci=((r.f&0x10)>0)?0x80:0; co=((r.c&1)>0)?0x10:0; r.c=(r.c>>1)+ci; r.c&=0xFF; r.f=(r.c>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void RRr_d() { ci=((r.f&0x10)>0)?0x80:0; co=((r.d&1)>0)?0x10:0; r.d=(r.d>>1)+ci; r.d&=0xFF; r.f=(r.d>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void RRr_e() { ci=((r.f&0x10)>0)?0x80:0; co=((r.e&1)>0)?0x10:0; r.e=(r.e>>1)+ci; r.e&=0xFF; r.f=(r.e>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void RRr_h() { ci=((r.f&0x10)>0)?0x80:0; co=((r.h&1)>0)?0x10:0; r.h=(r.h>>1)+ci; r.h&=0xFF; r.f=(r.h>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void RRr_l() { ci=((r.f&0x10)>0)?0x80:0; co=((r.l&1)>0)?0x10:0; r.l=(r.l>>1)+ci; r.l&=0xFF; r.f=(r.l>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void RRr_a() { ci=((r.f&0x10)>0)?0x80:0; co=((r.a&1)>0)?0x10:0; r.a=(r.a>>1)+ci; r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void RRHL() { int i=mMU.rb((r.h<<8)+r.l); ci=((r.f&0x10)>0)?0x80:0; co=((i&1)>0)?0x10:0; i=(i>>1)+ci; i&=0xFF; mMU.wb((r.h<<8)+r.l,(byte)i); r.f=(i>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=4; }

		public void RRCr_b() { ci=((r.b&1)>0)?0x80:0; co=((r.b&1)>0)?0x10:0; r.b=(r.b>>1)+ci; r.b&=0xFF; r.f=(r.b>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void RRCr_c() { ci=((r.c&1)>0)?0x80:0; co=((r.c&1)>0)?0x10:0; r.c=(r.c>>1)+ci; r.c&=0xFF; r.f=(r.c>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void RRCr_d() { ci=((r.d&1)>0)?0x80:0; co=((r.d&1)>0)?0x10:0; r.d=(r.d>>1)+ci; r.d&=0xFF; r.f=(r.d>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void RRCr_e() { ci=((r.e&1)>0)?0x80:0; co=((r.e&1)>0)?0x10:0; r.e=(r.e>>1)+ci; r.e&=0xFF; r.f=(r.e>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void RRCr_h() { ci=((r.h&1)>0)?0x80:0; co=((r.h&1)>0)?0x10:0; r.h=(r.h>>1)+ci; r.h&=0xFF; r.f=(r.h>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void RRCr_l() { ci=((r.l&1)>0)?0x80:0; co=((r.l&1)>0)?0x10:0; r.l=(r.l>>1)+ci; r.l&=0xFF; r.f=(r.l>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void RRCr_a() { ci=((r.a&1)>0)?0x80:0; co=((r.a&1)>0)?0x10:0; r.a=(r.a>>1)+ci; r.a&=0xFF; r.f=(r.a>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void RRCHL() { int i=mMU.rb((r.h<<8)+r.l); ci=((i&1)>0)?0x80:0; co=((i&1)>0)?0x10:0; i=(i>>1)+ci; i&=0xFF; mMU.wb((r.h<<8)+r.l,(byte)i); r.f=(i>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=4; }

		public void SLAr_b() { co=((r.b&0x80)>0)?0x10:0; r.b=(r.b<<1)&0xFF; r.f=(r.b>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void SLAr_c() { co=((r.c&0x80)>0)?0x10:0; r.c=(r.c<<1)&0xFF; r.f=(r.c>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void SLAr_d() { co=((r.d&0x80)>0)?0x10:0; r.d=(r.d<<1)&0xFF; r.f=(r.d>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void SLAr_e() { co=((r.e&0x80)>0)?0x10:0; r.e=(r.e<<1)&0xFF; r.f=(r.e>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void SLAr_h() { co=((r.h&0x80)>0)?0x10:0; r.h=(r.h<<1)&0xFF; r.f=(r.h>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void SLAr_l() { co=((r.l&0x80)>0)?0x10:0; r.l=(r.l<<1)&0xFF; r.f=(r.l>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void SLAr_a() { co=((r.a&0x80)>0)?0x10:0; r.a=(r.a<<1)&0xFF; r.f=(r.a>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }

		public void SLLr_b() { co=((r.b&0x80)>0)?0x10:0; r.b=(r.b<<1)&0xFF+1; r.f=(r.b>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void SLLr_c() { co=((r.c&0x80)>0)?0x10:0; r.c=(r.c<<1)&0xFF+1; r.f=(r.c>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void SLLr_d() { co=((r.d&0x80)>0)?0x10:0; r.d=(r.d<<1)&0xFF+1; r.f=(r.d>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void SLLr_e() { co=((r.e&0x80)>0)?0x10:0; r.e=(r.e<<1)&0xFF+1; r.f=(r.e>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void SLLr_h() { co=((r.h&0x80)>0)?0x10:0; r.h=(r.h<<1)&0xFF+1; r.f=(r.h>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void SLLr_l() { co=((r.l&0x80)>0)?0x10:0; r.l=(r.l<<1)&0xFF+1; r.f=(r.l>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void SLLr_a() { co=((r.a&0x80)>0)?0x10:0; r.a=(r.a<<1)&0xFF+1; r.f=(r.a>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }

		public void SRAr_b() { ci=r.b&0x80; co=((r.b&1)>0)?0x10:0; r.b=((r.b>>1)+ci)&0xFF; r.f=(r.b>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void SRAr_c() { ci=r.c&0x80; co=((r.c&1)>0)?0x10:0; r.c=((r.c>>1)+ci)&0xFF; r.f=(r.c>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void SRAr_d() { ci=r.d&0x80; co=((r.d&1)>0)?0x10:0; r.d=((r.d>>1)+ci)&0xFF; r.f=(r.d>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void SRAr_e() { ci=r.e&0x80; co=((r.e&1)>0)?0x10:0; r.e=((r.e>>1)+ci)&0xFF; r.f=(r.e>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void SRAr_h() { ci=r.h&0x80; co=((r.h&1)>0)?0x10:0; r.h=((r.h>>1)+ci)&0xFF; r.f=(r.h>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void SRAr_l() { ci=r.l&0x80; co=((r.l&1)>0)?0x10:0; r.l=((r.l>>1)+ci)&0xFF; r.f=(r.l>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void SRAr_a() { ci=r.a&0x80; co=((r.a&1)>0)?0x10:0; r.a=((r.a>>1)+ci)&0xFF; r.f=(r.a>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }

		public void SRLr_b() { co=((r.b&1)>0)?0x10:0; r.b=(r.b>>1)&0xFF; r.f=(r.b>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void SRLr_c() { co=((r.c&1)>0)?0x10:0; r.c=(r.c>>1)&0xFF; r.f=(r.c>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void SRLr_d() { co=((r.d&1)>0)?0x10:0; r.d=(r.d>>1)&0xFF; r.f=(r.d>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void SRLr_e() { co=((r.e&1)>0)?0x10:0; r.e=(r.e>>1)&0xFF; r.f=(r.e>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void SRLr_h() { co=((r.h&1)>0)?0x10:0; r.h=(r.h>>1)&0xFF; r.f=(r.h>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void SRLr_l() { co=((r.l&1)>0)?0x10:0; r.l=(r.l>>1)&0xFF; r.f=(r.l>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }
		public void SRLr_a() { co=((r.a&1)>0)?0x10:0; r.a=(r.a>>1)&0xFF; r.f=(r.a>0)?0:0x80; r.f=(r.f&0xEF)+co; r.m=2; }

		public void CPL() { r.a ^= 0xFF; r.f=(r.a>0)?0:0x80; r.m=1; }
		public void NEG() { r.a=0-r.a; r.f=(r.a<0)?0x10:0; r.a&=0xFF; if(!(r.a>0)) r.f|=0x80; r.m=2; }

		public void CCF() { ci=((r.f&0x10)>0)?0:0x10; r.f=(r.f&0xEF)+ci; r.m=1; }
		public void SCF() { r.f|=0x10; r.m=1; }

		/*--- Stack ---*/
		public void PUSHBC() { r.sp--; mMU.wb(r.sp,(byte)r.b); r.sp--; mMU.wb(r.sp,(byte)r.c); r.m=3; }
		public void PUSHDE() { r.sp--; mMU.wb(r.sp,(byte)r.d); r.sp--; mMU.wb(r.sp,(byte)r.e); r.m=3; }
		public void PUSHHL() { r.sp--; mMU.wb(r.sp,(byte)r.h); r.sp--; mMU.wb(r.sp,(byte)r.l); r.m=3; }
		public void PUSHAF() { r.sp--; mMU.wb(r.sp,(byte)r.a); r.sp--; mMU.wb(r.sp,(byte)r.f); r.m=3; }

		public void POPBC() { r.c=mMU.rb(r.sp); r.sp++; r.b=mMU.rb(r.sp); r.sp++; r.m=3; }
		public void POPDE() { r.e=mMU.rb(r.sp); r.sp++; r.d=mMU.rb(r.sp); r.sp++; r.m=3; }
		public void POPHL() { r.l=mMU.rb(r.sp); r.sp++; r.h=mMU.rb(r.sp); r.sp++; r.m=3; }
		public void POPAF() { r.f=mMU.rb(r.sp); r.sp++; r.a=mMU.rb(r.sp); r.sp++; r.m=3; }

		/*--- Jump ---*/
		public void JPnn() { r.pc = mMU.rw(r.pc); r.m=3; }
		public void JPHL() { r.pc=(r.h<<8)+r.l; r.m=1; }
		public void JPNZnn() { r.m=3; if((r.f&0x80)==0x00) { r.pc=mMU.rw(r.pc); r.m++; } else r.pc+=2; }
		public void JPZnn()  { r.m=3; if((r.f&0x80)==0x80) { r.pc=mMU.rw(r.pc); r.m++; } else r.pc+=2; }
		public void JPNCnn() { r.m=3; if((r.f&0x10)==0x00) { r.pc=mMU.rw(r.pc); r.m++; } else r.pc+=2; }
		public void JPCnn()  { r.m=3; if((r.f&0x10)==0x10) { r.pc=mMU.rw(r.pc); r.m++; } else r.pc+=2; }

		public void JRn() { i=mMU.rb(r.pc); if(i>127) i=-((~i+1)&0xFF); r.pc++; r.m=2; r.pc+=i; r.m++; }
		public void JRNZn() { i=mMU.rb(r.pc); if(i>127) i=-((~i+1)&0xFF); r.pc++; r.m=2; if((r.f&0x80)==0x00) { r.pc+=i; r.m++; } }
		public void JRZn()  { i=mMU.rb(r.pc); if(i>127) i=-((~i+1)&0xFF); r.pc++; r.m=2; if((r.f&0x80)==0x80) { r.pc+=i; r.m++; } }
		public void JRNCn() { i=mMU.rb(r.pc); if(i>127) i=-((~i+1)&0xFF); r.pc++; r.m=2; if((r.f&0x10)==0x00) { r.pc+=i; r.m++; } }
		public void JRCn()  { i=mMU.rb(r.pc); if(i>127) i=-((~i+1)&0xFF); r.pc++; r.m=2; if((r.f&0x10)==0x10) { r.pc+=i; r.m++; } }

		public void DJNZn() { i=mMU.rb(r.pc); if(i>127) i=-((~i+1)&0xFF); r.pc++; r.m=2; r.b--; if(r.b>0) { r.pc+=i; r.m++; } }

		public void CALLnn() { r.sp-=2; mMU.ww(r.sp,r.pc+2); r.pc=mMU.rw(r.pc); r.m=5; }
		public void CALLNZnn() { r.m=3; if((r.f&0x80)==0x00) { r.sp-=2; mMU.ww(r.sp,r.pc+2); r.pc=mMU.rw(r.pc); r.m+=2; } else r.pc+=2; }
		public void CALLZnn() { r.m=3; if((r.f&0x80)==0x80) { r.sp-=2; mMU.ww(r.sp,r.pc+2); r.pc=mMU.rw(r.pc); r.m+=2; } else r.pc+=2; }
		public void CALLNCnn() { r.m=3; if((r.f&0x10)==0x00) { r.sp-=2; mMU.ww(r.sp,r.pc+2); r.pc=mMU.rw(r.pc); r.m+=2; } else r.pc+=2; }
		public void CALLCnn() { r.m=3; if((r.f&0x10)==0x10) { r.sp-=2; mMU.ww(r.sp,r.pc+2); r.pc=mMU.rw(r.pc); r.m+=2; } else r.pc+=2; }

		public void RET() { r.pc=mMU.rw(r.sp); r.sp+=2; r.m=3; }
		public void RETI() { r.ime=1; rrs(); r.pc=mMU.rw(r.sp); r.sp+=2; r.m=3; }
		public void RETNZ() { r.m=1; if((r.f&0x80)==0x00) { r.pc=mMU.rw(r.sp); r.sp+=2; r.m+=2; } }
		public void RETZ() { r.m=1; if((r.f&0x80)==0x80) { r.pc=mMU.rw(r.sp); r.sp+=2; r.m+=2; } }
		public void RETNC() { r.m=1; if((r.f&0x10)==0x00) { r.pc=mMU.rw(r.sp); r.sp+=2; r.m+=2; } }
		public void RETC() { r.m=1; if((r.f&0x10)==0x10) { r.pc=mMU.rw(r.sp); r.sp+=2; r.m+=2; } }

		public void RST00() { rsv(); r.sp-=2; mMU.ww(r.sp,r.pc); r.pc=0x00; r.m=3; }
		public void RST08() { rsv(); r.sp-=2; mMU.ww(r.sp,r.pc); r.pc=0x08; r.m=3; }
		public void RST10() { rsv(); r.sp-=2; mMU.ww(r.sp,r.pc); r.pc=0x10; r.m=3; }
		public void RST18() { rsv(); r.sp-=2; mMU.ww(r.sp,r.pc); r.pc=0x18; r.m=3; }
		public void RST20() { rsv(); r.sp-=2; mMU.ww(r.sp,r.pc); r.pc=0x20; r.m=3; }
		public void RST28() { rsv(); r.sp-=2; mMU.ww(r.sp,r.pc); r.pc=0x28; r.m=3; }
		public void RST30() { rsv(); r.sp-=2; mMU.ww(r.sp,r.pc); r.pc=0x30; r.m=3; }
		public void RST38() { rsv(); r.sp-=2; mMU.ww(r.sp,r.pc); r.pc=0x38; r.m=3; }
		public void RST40() { rsv(); r.sp-=2; mMU.ww(r.sp,r.pc); r.pc=0x40; r.m=3; }
		public void RST48() { rsv(); r.sp-=2; mMU.ww(r.sp,r.pc); r.pc=0x48; r.m=3; }
		public void RST50() { rsv(); r.sp-=2; mMU.ww(r.sp,r.pc); r.pc=0x50; r.m=3; }
		public void RST58() { rsv(); r.sp-=2; mMU.ww(r.sp,r.pc); r.pc=0x58; r.m=3; }
		public void RST60() { rsv(); r.sp-=2; mMU.ww(r.sp,r.pc); r.pc=0x60; r.m=3; }

		public void NOP() { r.m=1; }
		public void HALT() { _halt=1; r.m=1; }

		public void DI() { r.ime=0; r.m=1; }
		public void EI() { r.ime=1; r.m=1; }

		/*--- Helper functions ---*/
		public void rsv() {
			rv.a = r.a; rv.b = r.b;
			rv.c = r.c; rv.d = r.d;
			rv.e = r.e; rv.f = r.f;
			rv.h = r.h; rv.l = r.l;
		}

		public void rrs() {
			r.a = rv.a; r.b = rv.b;
			r.c = rv.c; r.d = rv.d;
			r.e = rv.e; r.f = rv.f;
			r.h = rv.h; r.l = rv.l;
		}

		public void MAPcb() {
			int i=mMU.rb(r.pc); r.pc++;
			r.pc &= 65535;
			if(_cbmap[i] != null) _cbmap[i]();
			//      else throw new Exception("Z80: MAPcb i = " + i);
		}

		public void XX() {
			/*Undefined map entry*/
			//  var opc = r.pc-1;
			//    throw new Exception("Z80: Unimplemented instruction at $"+opc+", stopping.");
			_stop=1;
		}  
	}