#!/usr/bin/env python3
from pathlib import Path
import hashlib, struct, sys

V8_SHA='2afc54a4f3415b22d1fe1b126f8deb2e0842c24a1f6d6bc14dc96d747180d71f'
OUT_SHA='11f0f28332d5dcbdcf19a5c4cbb72a9eb98e2d20e346185c287b0363e79820f6'
HOOK_VA=0x2338
HOOK_ORIGINAL=bytes.fromhex('00220092')
ANCHOR_BEFORE=bytes.fromhex('1f2d5edd')
ANCHOR_AFTER=bytes.fromhex('08334ff4fc7218460021')
HELPER=bytes.fromhex('06980078010911f001011a790a431a71410911f001015a790a435a71002200927047')
EXPECTED_SEG6_PREFIX=(1,0x10000,0x20000,0x20000)
EXPECTED_SEG6_FLAGS_ALIGN=(5,0x10000)

def sha(b): return hashlib.sha256(b).hexdigest()

def thumb_bl(src,target):
    off=target-(src+4)
    if off & 1 or not (-(1<<24) <= off < (1<<24)):
        raise RuntimeError('Thumb BL range/alignment')
    val=off & ((1<<25)-1)
    s=(val>>24)&1; i1=(val>>23)&1; i2=(val>>22)&1
    imm10=(val>>12)&0x3ff; imm11=(val>>1)&0x7ff
    j1=((~i1)&1)^s; j2=((~i2)&1)^s
    return struct.pack('<HH',0xF000|(s<<10)|imm10,0xD000|(j1<<13)|(j2<<11)|imm11)

def build(inp,out):
    original=Path(inp).read_bytes()
    if sha(original)!=V8_SHA:
        raise RuntimeError('V8 SHA mismatch: '+sha(original))
    b=bytearray(original)
    if b[:4]!=b'\x7fELF' or b[4]!=1 or b[5]!=1:
        raise RuntimeError('unexpected ELF')
    phoff=struct.unpack_from('<I',b,28)[0]
    phentsize=struct.unpack_from('<H',b,42)[0]
    phnum=struct.unpack_from('<H',b,44)[0]
    if (phoff,phentsize,phnum)!=(52,32,7):
        raise RuntimeError('program header contract mismatch')
    seg6_off=phoff+6*phentsize
    seg6=struct.unpack_from('<IIIIIIII',b,seg6_off)
    if seg6[:4]!=EXPECTED_SEG6_PREFIX or seg6[6:]!=EXPECTED_SEG6_FLAGS_ALIGN:
        raise RuntimeError('V8 executable PT_LOAD geometry mismatch')
    if b[HOOK_VA-4:HOOK_VA]!=ANCHOR_BEFORE or b[HOOK_VA:HOOK_VA+4]!=HOOK_ORIGINAL or b[HOOK_VA+4:HOOK_VA+14]!=ANCHOR_AFTER:
        raise RuntimeError('AutoATT hook anchor mismatch')
    pad=(-len(b)) & 3
    b.extend(b'\x00'*pad)
    helper_file_off=len(b)
    helper_va=seg6[2]+(helper_file_off-seg6[1])
    branch=thumb_bl(HOOK_VA,helper_va)
    b[HOOK_VA:HOOK_VA+4]=branch
    b.extend(HELPER)
    newsize=len(b)-seg6[1]
    struct.pack_into('<I',b,seg6_off+16,newsize)
    struct.pack_into('<I',b,seg6_off+20,newsize)
    allowed=set(range(HOOK_VA,HOOK_VA+4))|set(range(seg6_off+16,seg6_off+24))
    bad=[i for i,(x,y) in enumerate(zip(original,b[:len(original)])) if x!=y and i not in allowed]
    if bad:
        raise RuntimeError('unexpected mutation at 0x%X'%bad[0])
    if b[helper_file_off:helper_file_off+len(HELPER)]!=HELPER:
        raise RuntimeError('AutoATT helper mismatch')
    digest=sha(bytes(b))
    if digest!=OUT_SHA:
        raise RuntimeError('output SHA mismatch: '+digest)
    Path(out).write_bytes(b)
    try: Path(out).chmod(0o755)
    except OSError: pass
    print('FINAL_ARM_BUILD=PASS')
    print('V8_BASE_SHA256='+V8_SHA)
    print('AUTOATT_HELPER_SHA256='+sha(HELPER))
    print('AUTOATT_HELPER_FILE_OFF=0x%X'%helper_file_off)
    print('AUTOATT_HELPER_VA=0x%X'%helper_va)
    print('AUTOATT_HOOK_BL='+branch.hex())
    print('PT_LOAD_SIZE=0x%X'%newsize)
    print('FINAL_ARM_SHA256='+digest)
    print('FINAL_ARM_SIZE='+str(len(b)))

if __name__=='__main__':
    if len(sys.argv)!=3:
        raise SystemExit('usage: build_final_arm.py V8_ARM OUTPUT')
    build(sys.argv[1],sys.argv[2])
