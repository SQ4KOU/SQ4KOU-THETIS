typedef unsigned int u32;
typedef unsigned long usize;

static inline long sc_open(const char *p, long flags, long mode) {
  register long r0 asm("r0")=(long)p; register long r1 asm("r1")=flags; register long r2 asm("r2")=mode; register long r7 asm("r7")=5;
  asm volatile("svc 0" : "+r"(r0) : "r"(r1),"r"(r2),"r"(r7) : "memory","cc"); return r0;
}
static inline long sc_mmap2(long addr,long len,long prot,long flags,long fd,long pgoff) {
  register long r0 asm("r0")=addr; register long r1 asm("r1")=len; register long r2 asm("r2")=prot; register long r3 asm("r3")=flags; register long r4 asm("r4")=fd; register long r5 asm("r5")=pgoff; register long r7 asm("r7")=192;
  asm volatile("svc 0" : "+r"(r0) : "r"(r1),"r"(r2),"r"(r3),"r"(r4),"r"(r5),"r"(r7) : "memory","cc"); return r0;
}
static inline long sc_write(long fd,const void *p,long n) {
  register long r0 asm("r0")=fd; register long r1 asm("r1")=(long)p; register long r2 asm("r2")=n; register long r7 asm("r7")=4;
  asm volatile("svc 0" : "+r"(r0) : "r"(r1),"r"(r2),"r"(r7) : "memory","cc"); return r0;
}
static inline void sc_exit(long code) {
  register long r0 asm("r0")=code; register long r7 asm("r7")=1;
  asm volatile("svc 0" :: "r"(r0),"r"(r7) : "memory","cc"); for(;;){}
}
static int is_err(long x) { return (u32)(usize)x >= 0xfffff001u; }
static long slen(const char *s) { long n=0; while(s[n]) n++; return n; }
static void out(const char *s) { sc_write(1,s,slen(s)); }
static const char hex[]="0123456789ABCDEF";
static void outhex(u32 v) {
  char b[11]; b[0]='0'; b[1]='x';
  for(int i=0;i<8;i++) b[2+i]=hex[(v>>(28-4*i))&15u];
  b[10]='\n'; sc_write(1,b,11);
}
static const char devmem[]="/dev/mem";
#define WB_ID      (0x8004u/4u)
#define PPS_COUNT  (0x8008u/4u)
#define PPS_SEQ    (0x800cu/4u)
#define PPS_HWSTAT (0x8010u/4u)
#define GPS_USED   (0x8014u/4u)
#define GPS_SWSTAT (0x8018u/4u)
#define WB_MAGIC 0x57423131u

void _start(void) {
  long fd=sc_open(devmem,2,0);
  if(fd<0) { out("GPS_PROBE_OPEN=FAIL\n"); sc_exit(2); }
  long m=sc_mmap2(0,36864,3,1,fd,0x48000);
  if(is_err(m)) { out("GPS_PROBE_MAP=FAIL\n"); sc_exit(3); }
  volatile u32 *wb=(volatile u32*)(usize)m;
  if(wb[WB_ID]!=WB_MAGIC) { out("GPS_PROBE_ID=FAIL\n"); sc_exit(4); }
  out("GPS_PROBE_ID=PASS\n");
  out("PPS_HW_STATUS="); outhex(wb[PPS_HWSTAT]);
  out("PPS_SEQ="); outhex(wb[PPS_SEQ]);
  out("PPS_COUNT="); outhex(wb[PPS_COUNT]);
  out("GPS_USED_COUNT="); outhex(wb[GPS_USED]);
  out("GPS_SW_STATUS="); outhex(wb[GPS_SWSTAT]);
  sc_exit(0);
}
