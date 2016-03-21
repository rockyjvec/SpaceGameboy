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
    public void LDrr_bb() { r.b=r.b; r.m=1; }
    public void LDrr_bc() { r.b=r.c; r.m=1; }
    public void LDrr_bd() { r.b=r.d; r.m=1; }
    public void LDrr_be() { r.b=r.e; r.m=1; }
    public void LDrr_bh() { r.b=r.h; r.m=1; }
    public void LDrr_bl() { r.b=r.l; r.m=1; }
    public void LDrr_ba() { r.b=r.a; r.m=1; }
    public void LDrr_cb() { r.c=r.b; r.m=1; }
    public void LDrr_cc() { r.c=r.c; r.m=1; }
    public void LDrr_cd() { r.c=r.d; r.m=1; }
    public void LDrr_ce() { r.c=r.e; r.m=1; }
    public void LDrr_ch() { r.c=r.h; r.m=1; }
    public void LDrr_cl() { r.c=r.l; r.m=1; }
    public void LDrr_ca() { r.c=r.a; r.m=1; }
    public void LDrr_db() { r.d=r.b; r.m=1; }
    public void LDrr_dc() { r.d=r.c; r.m=1; }
    public void LDrr_dd() { r.d=r.d; r.m=1; }
    public void LDrr_de() { r.d=r.e; r.m=1; }
    public void LDrr_dh() { r.d=r.h; r.m=1; }
    public void LDrr_dl() { r.d=r.l; r.m=1; }
    public void LDrr_da() { r.d=r.a; r.m=1; }
    public void LDrr_eb() { r.e=r.b; r.m=1; }
    public void LDrr_ec() { r.e=r.c; r.m=1; }
    public void LDrr_ed() { r.e=r.d; r.m=1; }
    public void LDrr_ee() { r.e=r.e; r.m=1; }
    public void LDrr_eh() { r.e=r.h; r.m=1; }
    public void LDrr_el() { r.e=r.l; r.m=1; }
    public void LDrr_ea() { r.e=r.a; r.m=1; }
    public void LDrr_hb() { r.h=r.b; r.m=1; }
    public void LDrr_hc() { r.h=r.c; r.m=1; }
    public void LDrr_hd() { r.h=r.d; r.m=1; }
    public void LDrr_he() { r.h=r.e; r.m=1; }
    public void LDrr_hh() { r.h=r.h; r.m=1; }
    public void LDrr_hl() { r.h=r.l; r.m=1; }
    public void LDrr_ha() { r.h=r.a; r.m=1; }
    public void LDrr_lb() { r.l=r.b; r.m=1; }
    public void LDrr_lc() { r.l=r.c; r.m=1; }
    public void LDrr_ld() { r.l=r.d; r.m=1; }
    public void LDrr_le() { r.l=r.e; r.m=1; }
    public void LDrr_lh() { r.l=r.h; r.m=1; }
    public void LDrr_ll() { r.l=r.l; r.m=1; }
    public void LDrr_la() { r.l=r.a; r.m=1; }
    public void LDrr_ab() { r.a=r.b; r.m=1; }
    public void LDrr_ac() { r.a=r.c; r.m=1; }
    public void LDrr_ad() { r.a=r.d; r.m=1; }
    public void LDrr_ae() { r.a=r.e; r.m=1; }
    public void LDrr_ah() { r.a=r.h; r.m=1; }
    public void LDrr_al() { r.a=r.l; r.m=1; }
    public void LDrr_aa() { r.a=r.a; r.m=1; }

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

    public void LDAmm() { r.a=mMU.rb(mMU.rw(r.pc)); r.pc+=2; r.m=4; }

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

    public void JRn() { int i=mMU.rb(r.pc); if(i>127) i=-((~i+1)&0xFF); r.pc++; r.m=2; r.pc+=i; r.m++; }
    public void JRNZn() { int i=mMU.rb(r.pc); if(i>127) i=-((~i+1)&0xFF); r.pc++; r.m=2; if((r.f&0x80)==0x00) { r.pc+=i; r.m++; } }
    public void JRZn()  { int i=mMU.rb(r.pc); if(i>127) i=-((~i+1)&0xFF); r.pc++; r.m=2; if((r.f&0x80)==0x80) { r.pc+=i; r.m++; } }
    public void JRNCn() { int i=mMU.rb(r.pc); if(i>127) i=-((~i+1)&0xFF); r.pc++; r.m=2; if((r.f&0x10)==0x00) { r.pc+=i; r.m++; } }
    public void JRCn()  { int i=mMU.rb(r.pc); if(i>127) i=-((~i+1)&0xFF); r.pc++; r.m=2; if((r.f&0x10)==0x10) { r.pc+=i; r.m++; } }

    public void DJNZn() { int i=mMU.rb(r.pc); if(i>127) i=-((~i+1)&0xFF); r.pc++; r.m=2; r.b--; if(r.b>0) { r.pc+=i; r.m++; } }

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



