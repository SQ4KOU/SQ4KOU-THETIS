typedef unsigned int u32;
typedef signed int s32;
typedef unsigned long long u64;
typedef signed long long s64;
typedef unsigned long usize;

struct tspec { s32 tv_sec; s32 tv_nsec; };

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
static inline long sc_nanosleep(const struct tspec *req) {
  register long r0 asm("r0")=(long)req; register long r1 asm("r1")=0; register long r7 asm("r7")=162;
  asm volatile("svc 0" : "+r"(r0) : "r"(r1),"r"(r7) : "memory","cc"); return r0;
}

static const char devmem[]="/dev/mem";
static const char msg_ready[]="[GPS] thread ready; waiting PPS on E1 DIO3_N\n";
static const char msg_lock[]="[GPS] LOCK; frequency discipline active\n";
static const char msg_hold[]="[GPS] HOLDOVER; keeping last correction\n";

#define WB_ID      (0x8004u/4u)
#define PPS_COUNT  (0x8008u/4u)
#define PPS_SEQ    (0x800cu/4u)
#define PPS_HWSTAT (0x8010u/4u)
#define GPS_USED   (0x8014u/4u)
#define GPS_SWSTAT (0x8018u/4u)
#define WB_MAGIC 0x57423131u
#define NOMCLK 125000000u

static u32 correct_word(u32 nominal,u32 measured) {
  s32 delta=(s32)measured-(s32)NOMCLK;
  s64 q=(s64)((u64)nominal) * (s64)delta;
  q *= (s64)8796;
  s64 d;
  if(q>=0) d=(q + (((s64)1)<<39)) >> 40;
  else d=-(((-q) + (((s64)1)<<39)) >> 40);
  return (u32)((s64)(u64)nominal-d);
}

__attribute__((used,visibility("default"))) void *gps_thread(void *arg) {
  (void)arg;
  long fd=sc_open(devmem,2,0);
  if(fd<0) return 0;
  volatile u32 *cfg=(volatile u32*)sc_mmap2(0,4096,3,1,fd,0x40001);
  volatile u32 *wb=(volatile u32*)sc_mmap2(0,36864,3,1,fd,0x48000);
  if((u32)(usize)cfg >= 0xfffff001u || (u32)(usize)wb >= 0xfffff001u || wb[WB_ID]!=WB_MAGIC) return 0;
  sc_write(2,msg_ready,sizeof(msg_ready)-1);

  struct tspec one_ms={0,1000000};
  u32 last_seq=wb[PPS_SEQ], age=0, n_acq=0, sum=0, filt=0, locked=0, hold_reported=0;
  u32 nominal[4]={0,0,0,0};
  u32 applied[4]={0,0,0,0};
  u32 init[4]={0,0,0,0};
  const u32 offs[4]={2,3,4,5};

  wb[GPS_USED]=0; wb[GPS_SWSTAT]=1;
  for(;;) {
    u32 seq=wb[PPS_SEQ];
    if(seq!=last_seq) {
      u32 c=wb[PPS_COUNT];
      last_seq=seq; age=0; hold_reported=0;
      if(c>=124000000u && c<=126000000u) {
        if(!locked) {
          sum += c; n_acq++;
          if(n_acq>=4) {
            filt=(sum+2u)>>2; locked=1;
            wb[GPS_USED]=filt; wb[GPS_SWSTAT]=1u|2u|(n_acq<<8);
            sc_write(2,msg_lock,sizeof(msg_lock)-1);
          }
        } else {
          u32 diff=(c>filt)?(c-filt):(filt-c);
          if(diff<=5000u) filt=(u32)(((u64)filt*7u + c + 4u)>>3);
          wb[GPS_USED]=filt; wb[GPS_SWSTAT]=1u|2u|(n_acq<<8);
        }
      } else if(!locked) { n_acq=0; sum=0; }
    } else if(age<0xffffffffu) age++;

    if(locked && age>2500u) {
      wb[GPS_SWSTAT]=1u|2u|4u|(n_acq<<8);
      if(!hold_reported) { sc_write(2,msg_hold,sizeof(msg_hold)-1); hold_reported=1; }
    }

    for(u32 i=0;i<4;i++) {
      volatile u32 *r=cfg+offs[i];
      u32 raw=*r;
      if(!init[i]) { nominal[i]=raw; applied[i]=raw; init[i]=1; }
      else if(raw!=applied[i]) nominal[i]=raw;
      if(locked && nominal[i]!=0) {
        u32 cor=correct_word(nominal[i],filt);
        if(raw!=cor) {
          u32 verify=*r;
          if(verify==raw) {
            *r=cor;
            applied[i]=cor;
          } else {
            nominal[i]=verify;
            applied[i]=verify;
          }
        } else applied[i]=cor;
      } else applied[i]=raw;
    }
    sc_nanosleep(&one_ms);
  }
  return 0;
}
