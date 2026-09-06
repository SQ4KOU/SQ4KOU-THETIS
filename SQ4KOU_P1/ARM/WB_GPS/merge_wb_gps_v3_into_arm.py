#!/usr/bin/env python3
import struct, hashlib, pathlib, sys
ARM_EXPECT='2f7bbb5beac20ea51775b943bce2cbad3df0e0065118e5b4034ec4f5c91dfe27'
WB_EXPECT='91a2332f66942b97c18d37a604a25ed2fcfdfc1163f1b552ca3ba7399ee1f1c3'
GPS_EXPECT='244b85bd4eb9758f261d2e40f64be52c13fa01e1be8553c104a47e837577e048'
LOAD_OFF=0x10000
LOAD_VA=0x20000
PTHREAD_CREATE_PLT=0x95c
PT_LOAD=1
PT_GNU_RELRO=0x6474e552

def h(b): return hashlib.sha256(b).hexdigest()
def align4(x): return (x+3)&~3
def u32(b,o): return struct.unpack_from('<I',b,o)[0]

def patch(arm_path,wb_path,gps_path,out_path):
    arm=bytearray(pathlib.Path(arm_path).read_bytes())
    wb=pathlib.Path(wb_path).read_bytes()
    gps=pathlib.Path(gps_path).read_bytes()
    if h(arm)!=ARM_EXPECT: raise RuntimeError('ARM SHA mismatch')
    if h(wb)!=WB_EXPECT: raise RuntimeError('WB SHA mismatch')
    if h(gps)!=GPS_EXPECT: raise RuntimeError('GPS SHA mismatch')

    ro=wb[0xEC:0xEC+0xB5]
    text=bytearray(wb[0x1A4:0x1A4+0x538])
    wb_ro_off=0
    wb_text_off=align4(wb_ro_off+len(ro))
    gps_off=align4(wb_text_off+len(text))
    supervisor_off=align4(gps_off+len(gps))

    wb_ro_va=LOAD_VA+wb_ro_off
    wb_text_va=LOAD_VA+wb_text_off
    gps_va=LOAD_VA+gps_off
    supervisor_va=LOAD_VA+supervisor_off

    refs=[
        (0x408,0x024,0x000),(0x40C,0x370,0x009),(0x410,0x390,0x027),
        (0x414,0x3B0,0x041),(0x418,0x3D0,0x05F),(0x41C,0x108,0x099),
        (0x420,0x3F0,0x07E),
    ]
    for lit_off,add_off,ro_off in refs:
        pc=wb_text_va+add_off+8
        target=wb_ro_va+ro_off
        struct.pack_into('<I',text,lit_off,(target-pc)&0xffffffff)

    supervisor=bytearray(bytes.fromhex(
        '10402de9' '08d04de2' '0d00a0e1' '0010a0e3'
        '24209fe5' '02208fe0' '0030a0e3' '1cc09fe5'
        '0cc08fe0' '3cff2fe1' '08d08de2' '1040bde8'
        '0cc09fe5' '0cc08fe0' '1cff2fe1'
        '11111111' '22222222' '33333333'
    ))
    assert len(supervisor)==72
    struct.pack_into('<I',supervisor,0x3c,(wb_text_va-(supervisor_va+0x14+8))&0xffffffff)
    struct.pack_into('<I',supervisor,0x40,(PTHREAD_CREATE_PLT-(supervisor_va+0x20+8))&0xffffffff)
    struct.pack_into('<I',supervisor,0x44,(gps_va-(supervisor_va+0x34+8))&0xffffffff)

    boot_off=align4(supervisor_off+len(supervisor))
    boot_va=LOAD_VA+boot_off
    boot=bytearray(bytes.fromhex(
        '10402de9' '08d04de2' '0d00a0e1' '0010a0e3'
        '18209fe5' '02208fe0' '0030a0e3' '10c09fe5'
        '0cc08fe0' '3cff2fe1' '08d08de2' '1080bde8'
        '11111111' '22222222'
    ))
    assert len(boot)==56
    struct.pack_into('<I',boot,0x30,(supervisor_va-(boot_va+0x14+8))&0xffffffff)
    struct.pack_into('<I',boot,0x34,(PTHREAD_CREATE_PLT-(boot_va+0x20+8))&0xffffffff)

    payload=bytearray(boot_off+len(boot))
    payload[wb_ro_off:wb_ro_off+len(ro)]=ro
    payload[wb_text_off:wb_text_off+len(text)]=text
    payload[gps_off:gps_off+len(gps)]=gps
    payload[supervisor_off:supervisor_off+len(supervisor)]=supervisor
    payload[boot_off:boot_off+len(boot)]=boot

    if arm[:4]!=b'\x7fELF' or arm[4]!=1 or arm[5]!=1: raise RuntimeError('unexpected ELF')
    if u32(arm,0x84)!=0x46B4 or u32(arm,0x88)!=0x46B4: raise RuntimeError('base LOAD mismatch')
    if u32(arm,0x4DD4)!=0x203D: raise RuntimeError('init_array mismatch')
    if u32(arm,0xF4)!=PT_GNU_RELRO: raise RuntimeError('GNU_RELRO mismatch')
    if arm[PTHREAD_CREATE_PLT:PTHREAD_CREATE_PLT+4]!=bytes.fromhex('00c68fe2'): raise RuntimeError('pthread PLT mismatch')
    if len(arm)>LOAD_OFF: raise RuntimeError('base ELF too large')
    arm.extend(b'\0'*(LOAD_OFF-len(arm)))
    arm.extend(payload)
    vals=(PT_LOAD,LOAD_OFF,LOAD_VA,LOAD_VA,len(payload),len(payload),5,0x10000)
    for i,v in enumerate(vals): struct.pack_into('<I',arm,0xF4+4*i,v)
    struct.pack_into('<I',arm,0x4DD4,boot_va)
    pathlib.Path(out_path).write_bytes(arm)
    return {
      'base_arm_sha':ARM_EXPECT,'wb_sha':WB_EXPECT,'gps_sha':GPS_EXPECT,
      'merged_sha':h(arm),'wb_text_va':hex(wb_text_va),'gps_va':hex(gps_va),
      'supervisor_va':hex(supervisor_va),'bootstrap_va':hex(boot_va),
      'payload_size':len(payload),'file_size':len(arm)
    }

if __name__=='__main__':
    if len(sys.argv)!=5: raise SystemExit('usage: merge_v2.py ARM WB GPS OUT')
    for k,v in patch(*sys.argv[1:]).items(): print(f'{k}={v}')
