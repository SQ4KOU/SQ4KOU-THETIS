set display_name {SQ4KOU ADC Overload Detector}
set core [ipx::current_core]
set_property DISPLAY_NAME $display_name $core
set_property DESCRIPTION {Passive Red Pitaya ADC0/ADC1 near-full-scale detector for HPSDR P1 Auto Attenuate telemetry} $core
core_parameter THRESHOLD {ADC threshold} {Absolute signed ADC count at which overload telemetry is asserted.}
core_parameter HOLD_CYCLES {Hold cycles} {Retriggerable overload pulse-stretch duration in ADC clock cycles.}
