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
