.syntax unified
.arm
.text
.align 2
.global telemetry_hook_v8
.type telemetry_hook_v8,%function
.extern telemetry_send_v8
telemetry_hook_v8:
    movw    r0, #0xa6c0
    ldr     r7, [sp, #0x20]
    cmp     r7, #0
    beq     1f
    ubfx    r7, r7, #0, #10
    cmp     r7, #0
    bne     2f
1:
    stmdb   sp!, {r0-r12, lr}
    add     r0, sp, #56
    bl      telemetry_send_v8
    ldmia   sp!, {r0-r12, lr}
2:
    b       RETURN_VA
.size telemetry_hook_v8, .-telemetry_hook_v8
