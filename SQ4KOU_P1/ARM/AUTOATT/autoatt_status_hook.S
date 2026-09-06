.syntax unified
.thumb
.text
.align 2
.global autoatt_status_hook
.type autoatt_status_hook,%function
.thumb_func
autoatt_status_hook:
    // handler_ep6 saved the live MMIO gpio_in pointer at [sp,#0x18].
    // FPGA appends ADC0/ADC1 overload as bits 4/5 of that same status byte.
    ldr     r0, [sp, #0x18]
    ldrb    r0, [r0]

    // HPSDR P1 C0=0x20: C1 bit0 = ADC0 overload.
    lsrs    r1, r0, #4
    ands    r1, r1, #1
    ldrb    r2, [r3, #4]
    orrs    r2, r2, r1
    strb    r2, [r3, #4]

    // HPSDR P1 C0=0x20: C2 bit0 = ADC1 overload.
    lsrs    r1, r0, #5
    ands    r1, r1, #1
    ldrb    r2, [r3, #5]
    orrs    r2, r2, r1
    strb    r2, [r3, #5]

    // Reproduce the two displaced instructions at handler_ep6+0x2c8.
    movs    r2, #0
    str     r2, [sp]
    bx      lr
.size autoatt_status_hook, .-autoatt_status_hook
