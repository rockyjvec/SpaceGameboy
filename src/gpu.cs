class GPUData //: IComparable<GPUData>
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

class GPUPalette {
	public int[] bg = new int[4];
	public int[] obj0 = new int[4];
	public int[] obj1 = new int[4];
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
	GPUPalette _palette = new GPUPalette();
	int[] _scanrow = new int[160];

	int _curline = 0;
	int _curscan = 0;
	int _linemode = 0;
	int _modeclocks = 0;

	int _yscrl = 0;
	int _xscrl = 0;
	int _raster = 0;

	bool _lcdon = false;
	bool _bgon = false;
	bool _objon = false;

	bool _objsize = false;

	int _bgtilebase = 0x0000;
	int _bgmapbase = 0x1800;

	char[] colors = new char[256];
	
	int wpixel = 0;

	IMyTextPanel screen;
	bool draw = true, startDraw = false;
	Z80 z80;
	MMU mMU;

	public GPU(IMyTextPanel screen)
	{
		//      SpaceGameboy.Echo("GPU Constructor");
		this.screen = screen;
        for(int i = 0; i < 256; i++) colors[i] = '\uE00F';
		colors[0] = '\uE00F';                
		colors[96] = '\uE00E';                
		colors[192] = '\uE00D';                
		colors[255] = '\uE006';
	}

	public void drawNow()
	{
		if (startDraw == false) {
			startDraw = true;
		}
	}

	public void update()
	{
		screen.WritePublicText(new String(data), false);
		screen.ShowTextureOnScreen();
		screen.ShowPublicTextOnScreen();
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

		_lcdon = false;
		_bgon = false;
		_objon = false;

		_objsize = false;
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
	int[] tilerow, pal;
	GPUData obj;
	public void checkline() {
		if (!this.draw && !this.startDraw)
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
							} else if (this.startDraw) {
								this.startDraw = false;
								this.draw = true;
							}
							mMU._if |= 1;
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

								if (_bgtilebase > 0) {
									tile = _vram [mapbase + t];
									if (tile < 128)
										tile = (256 + tile);
									tilerow = _tilemap [tile] [y];
									for (wpixel = 160; wpixel > 0; wpixel--) {
										_scanrow [159 - x] = tilerow [x];
										data [linebase] = colors [_palette.bg [tilerow [x]]];
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
										data [linebase] = colors [_palette.bg [tilerow [x]]];
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
							if (_objon) {
								cnt = 0;
								if (_objsize) {
									/*                for(var i=0; i<40; i++)
                {
                }*/
								} else {
									//                var pixel;
									linebase = _curscan;
									curline161 = _curline * 161;
									for (i = 0; i < 40; i++) {
										obj = _objdata [i];
										if (obj.y <= _curline && (obj.y + 8) > _curline) {
											if (obj.yflip > 0)
												tilerow = _tilemap [obj.tile] [7 - (_curline - obj.y)];
											else
												tilerow = _tilemap [obj.tile] [_curline - obj.y];

											if (obj.palette > 0)
												pal = _palette.obj1;
											else
												pal = _palette.obj0;

											linebase = (curline161 + obj.x);
											if (obj.xflip > 0) {
												for (x = 0; x < 8; x++) {
													if (obj.x + x >= 0 && obj.x + x < 160) {
														if (tilerow [7 - x] > 0 && (obj.prio > 0 || !(_scanrow [x] > 0))) {
															data [linebase] = colors [pal [tilerow [7 - x]]];
														}
													}
													linebase++;
												}
											} else {
												for (x = 0; x < 8; x++) {
													if (obj.x + x >= 0 && obj.x + x < 160) {
														if (tilerow [x] > 0 && (obj.prio > 0 || !(_scanrow [x] > 0))) {
															data [linebase] = colors [pal [tilerow [x]]];
														}
													}
													linebase++;
												}
											}
											cnt++;
											if (cnt > 10)
												break;
										}
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
		var obj=addr>>2;
		if(obj<40)
		{
			switch(addr&3)
			{
			case 0: _objdata[obj].y=val-16; break;
			case 1: _objdata[obj].x=val-8; break;
			case 2:
				if(_objsize) _objdata[obj].tile = (val&0xFE);
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

	public int rb(int addr) {
		gaddr = addr-0xFF40;
		switch(gaddr)
		{
		case 0:
			return ((_lcdon?0x80:0)|
				((_bgtilebase==0x0000)?0x10:0)|
				((_bgmapbase==0x1C00)?0x08:0)|
				(_objsize?0x04:0)|
				(_objon?0x02:0x00)|
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
			_bgtilebase = ((val&0x10)>0)?0x0000:0x0800;
			_bgmapbase = ((val&0x08)>0)?0x1C00:0x1800;
			_objsize = ((val&0x04)>0)?true:false;
			_objon = ((val&0x02)>0)?true:false;
			_bgon = ((val&0x01)>0)?true:false;
			break;

		case 2:
			_yscrl = val;
			break;

		case 3:
			_xscrl = val;
			break;

		case 5: // this used to have _raster = val; but I moved it below
			// OAM DMA
		case 6:
			if(gaddr == 5) _raster = val;

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