RETURN_VA = 0x20904;
SECTIONS {
  . = 0x20bd8;
  .text : AT(0) { *(.text*) }
  .rodata : AT(LOADADDR(.text) + SIZEOF(.text)) { *(.rodata*) *(.rodata.*) }
  /DISCARD/ : { *(.comment*) *(.ARM.attributes*) *(.note*) *(.eh_frame*) *(.ARM.exidx*) *(.ARM.extab*) }
}
