#!/usr/bin/env python3
from pathlib import Path
import hashlib, struct, sys

BASE_SHA = '8868bc6a9af2698cc556af012f707a83d6a3eba679c1ed849de33ac5b8cb1407'
EXPECTED_OUT_SHA = '9384231994c309d0c26777cf6db0c742ae38950bd261c52e07ec3720aa19a41f'
HOOK_VA = 0x2338
HOOK_ORIGINAL = bytes.fromhex('00220092')  # movs r2,#0 ; str r2,[sp]
HELPER = bytes.fromhex('06980078010911f001011a790a431a71410911f001015a790a435a71002200927047')
HELPER_SHA = hashlib.sha256(HELPER).hexdigest()

# Current verified ELF contract: segment 6 is the existing RX post-link segment.
EXPECTED_SEG6 = (1, 0x10000, 0x20000, 0x20000, 0x0F83, 0x0F83, 5, 0x10000)

def sha(data): return hashlib.sha256(data).hexdigest()

def thumb_bl(src, target):
    off = target - (src + 4)
    if off & 1 or not (-(1<<24) <= off < (1<<24)):
        raise ValueError('Thumb BL target out of range/alignment')
    val = off & ((1<<25)-1)
    s=(val>>24)&1; i1=(val>>23)&1; i2=(val>>22)&1
    imm10=(val>>12)&0x3ff; imm11=(val>>1)&0x7ff
    j1=((~i1)&1)^s; j2=((~i2)&1)^s
    return struct.pack('<HH', 0xF000|(s<<10)|imm10,
                                0xD000|(j1<<13)|(j2<<11)|imm11)

def patch(inp: Path, out: Path):
    original = inp.read_bytes()
    if sha(original) != BASE_SHA:
        raise RuntimeError('BASE SHA mismatch: wymagany dzialajacy ARM 8868bc6a...')
    b = bytearray(original)
    if b[:4] != b'\x7fELF' or b[4] != 1 or b[5] != 1:
        raise RuntimeError('Nieoczekiwany ELF: wymagany ELF32 little-endian ARM')
    phoff=struct.unpack_from('<I',b,28)[0]
    phentsize=struct.unpack_from('<H',b,42)[0]
    phnum=struct.unpack_from('<H',b,44)[0]
    if (phoff,phentsize,phnum)!=(52,32,7):
        raise RuntimeError('ELF program-header contract mismatch')
    seg6_off=phoff+6*phentsize
    seg6=struct.unpack_from('<IIIIIIII',b,seg6_off)
    if seg6 != EXPECTED_SEG6:
        raise RuntimeError('Existing injected RX PT_LOAD segment mismatch')
    if b[HOOK_VA:HOOK_VA+4] != HOOK_ORIGINAL:
        raise RuntimeError('handler_ep6 hook bytes mismatch')
    if b[0x2334:0x2338] != bytes.fromhex('1f2d5edd'):
        raise RuntimeError('handler_ep6 C0=0x20 path anchor mismatch')
    if b[0x233c:0x2346] != bytes.fromhex('08334ff4fc7218460021'):
        raise RuntimeError('handler_ep6 continuation anchor mismatch')

    pad=(-len(b)) & 3
    b.extend(b'\x00'*pad)
    helper_file_off=len(b)
    helper_va=seg6[2] + (helper_file_off-seg6[1])
    branch=thumb_bl(HOOK_VA,helper_va)
    b[HOOK_VA:HOOK_VA+4]=branch
    b.extend(HELPER)
    new_seg_size=len(b)-seg6[1]
    struct.pack_into('<I',b,seg6_off+16,new_seg_size) # p_filesz
    struct.pack_into('<I',b,seg6_off+20,new_seg_size) # p_memsz

    allowed=set(range(HOOK_VA,HOOK_VA+4)) | set(range(seg6_off+16,seg6_off+24))
    unexpected=[i for i,(x,y) in enumerate(zip(original,b[:len(original)])) if x!=y and i not in allowed]
    if unexpected:
        raise RuntimeError(f'Unexpected mutation in existing ELF at 0x{unexpected[0]:X}')
    if bytes(b[helper_file_off:helper_file_off+len(HELPER)]) != HELPER:
        raise RuntimeError('Appended helper mismatch')
    newseg=struct.unpack_from('<IIIIIIII',b,seg6_off)
    if newseg[:4] != EXPECTED_SEG6[:4] or newseg[6:] != EXPECTED_SEG6[6:]:
        raise RuntimeError('PT_LOAD geometry/flags were altered')
    if newseg[4] != newseg[5] or newseg[4] != new_seg_size:
        raise RuntimeError('PT_LOAD size mismatch')
    if not (newseg[2] <= helper_va and helper_va+len(HELPER) <= newseg[2]+newseg[5]):
        raise RuntimeError('Helper not covered by executable PT_LOAD')

    out.write_bytes(b)
    try: out.chmod(0o755)
    except OSError: pass
    digest=sha(bytes(b))
    if digest != EXPECTED_OUT_SHA:
        raise RuntimeError('Deterministic output SHA mismatch: '+digest)
    return helper_va, branch.hex(), digest

def main():
    here=Path(__file__).resolve().parent
    inp=Path(sys.argv[1]) if len(sys.argv)>1 else here/'sdr-transceiver-hpsdr-ananxd.BASE_8868'
    out=Path(sys.argv[2]) if len(sys.argv)>2 else here/'sdr-transceiver-hpsdr-ananxd-autoatt-rebuilt'
    va,branch,digest=patch(inp,out)
    print('ARM_AUTOATT_PATCH=PASS')
    print('BASE_SHA256='+BASE_SHA)
    print('HELPER_SHA256='+HELPER_SHA)
    print(f'HELPER_VA=0x{va:08X}')
    print('HOOK_BL_BYTES='+branch)
    print('OUTPUT_SHA256='+digest)
    print('OUTPUT='+str(out))

if __name__=='__main__':
    main()
