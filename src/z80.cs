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



