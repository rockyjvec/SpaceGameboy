public class SpaceGameboy
{
	private GPU gPU;
	private MMU mMU;
	private Z80 z80;
	private KEY kEY;
	private TIMER tIMER;
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
		this.tIMER = new TIMER();
	}

	int fclock, ifired;
	public void frame(int throttle, int stage) {
		fclock = z80._clock.m+17556;
		//Echo every 100 frames
		//    if(stage % 100 == 0) Echo("Stepping Gameboy (PC = " + z80.r.pc + ")"); 
		//var brk = document.getElementById('breakpoint').value;
        if(((stage % 2) == 1)gPU.drawNow ();
		for(;z80._clock.m < fclock && throttle > 0;throttle--)
		{
			//if(z80._halt>0) z80.r.m=1;
			//else
			//{
			//  z80.r.r = (z80.r.r+1) & 127;
			z80._map[mMU.rb(z80.r.pc++)]();
			z80.r.pc &= 65535;
			//}
			if(z80.r.ime >0 && mMU._ie>0 && mMU._if>0)
			{
				z80._halt=0; z80.r.ime=0;
				ifired = mMU._ie & mMU._if;
				if((ifired&1)>0) { mMU._if &= 0xFE; z80.RST40(); }
				else if((ifired&2)>0) { mMU._if &= 0xFD; z80.RST48(); }
				else if((ifired&4)>0) { mMU._if &= 0xFB; z80.RST50(); }
				else if((ifired&8)>0) { mMU._if &= 0xF7; z80.RST58(); }
				else if((ifired&16)>0) { mMU._if &= 0xEF; z80.RST60(); }
				else { z80.r.ime=1; }
			}
			z80._clock.m += z80.r.m;
			gPU.checkline();
			tIMER.inc();
		}
	}

    public void update()
    {
        gPU.update();
    }
    
	public void reset(byte[] rom, int stage) {
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
			mMU.reset(gPU, tIMER, kEY, z80); 
			//   SpaceGameboy.Echo("Resetting z80");
			z80.reset(mMU); 
			//   SpaceGameboy.Echo("Resetting key");
			kEY.reset(); 
			//     SpaceGameboy.Echo("Resetting timer");
			tIMER.reset(mMU, z80);
			z80.r.pc=0x100;z80.r.sp=0xFFFE;/*z80.r.hl=0x014D;*/z80.r.c=0x13;z80.r.e=0xD8;z80.r.a=1;
			//TODO:                                              ^ this was missing, I don't know if it is supposed to be set
			break;
		case 6:
			// SpaceGameboy.Echo("Loading ROM!");
			mMU.load(rom);
			this.run();
			break;
		}


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
}