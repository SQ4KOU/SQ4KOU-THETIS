#!/usr/bin/env python3
import hashlib,struct,pathlib,sys
BASE_EXPECT='14149a1679ff5ce907b5c65dca903881a782fb8928834e9c5fa5ed90da20a71a'
BASE_LOAD_OFF=0x10000
BASE_LOAD_VA=0x20000
BASE_LOAD_SIZE=0x0bd8
HOOK_VA=0x20900
HOOK_FILE=BASE_LOAD_OFF+(HOOK_VA-BASE_LOAD_VA)
HELPER_VA=BASE_LOAD_VA+BASE_LOAD_SIZE
INIT_PTR_OFF=0x4dd4
INIT_EXPECT=0x20ba0
PH_FILESZ_OFF=0x104
PH_MEMSZ_OFF=0x108
OLD_INSN=0xe30a06c0

def sha(b): return hashlib.sha256(b).hexdigest()
def arm_b(src,target):
    d=target-(src+8)
    if d%4: raise ValueError('unaligned branch')
    imm=d//4
    if not (-(1<<23) <= imm < (1<<23)): raise ValueError('branch range')
    return 0xea000000 | (imm & 0x00ffffff)

def main(base,helper,out):
    b=bytearray(pathlib.Path(base).read_bytes())
    h=pathlib.Path(helper).read_bytes()
    assert sha(b)==BASE_EXPECT,(sha(b),'base sha mismatch')
    assert len(b)==BASE_LOAD_OFF+BASE_LOAD_SIZE,hex(len(b))
    assert struct.unpack_from('<I',b,PH_FILESZ_OFF)[0]==BASE_LOAD_SIZE
    assert struct.unpack_from('<I',b,PH_MEMSZ_OFF)[0]==BASE_LOAD_SIZE
    assert struct.unpack_from('<I',b,INIT_PTR_OFF)[0]==INIT_EXPECT
    assert struct.unpack_from('<I',b,HOOK_FILE)[0]==OLD_INSN,hex(struct.unpack_from('<I',b,HOOK_FILE)[0])
    br=arm_b(HOOK_VA,HELPER_VA)
    struct.pack_into('<I',b,HOOK_FILE,br)
    b.extend(h)
    newsize=BASE_LOAD_SIZE+len(h)
    struct.pack_into('<I',b,PH_FILESZ_OFF,newsize)
    struct.pack_into('<I',b,PH_MEMSZ_OFF,newsize)
    pathlib.Path(out).write_bytes(b)
    print('BASE_SHA='+BASE_EXPECT)
    print('HELPER_SHA='+sha(h))
    print('HELPER_SIZE='+str(len(h)))
    print('HOOK_VA='+hex(HOOK_VA))
    print('HELPER_VA='+hex(HELPER_VA))
    print('HOOK_OLD='+hex(OLD_INSN))
    print('HOOK_NEW='+hex(br))
    print('NEW_LOAD_SIZE='+hex(newsize))
    print('FINAL_SIZE='+str(len(b)))
    print('FINAL_SHA='+sha(b))
if __name__=='__main__': main(*sys.argv[1:4])
