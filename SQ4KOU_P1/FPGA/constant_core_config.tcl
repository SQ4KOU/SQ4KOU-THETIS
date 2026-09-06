set display_name {Constant}

set core [ipx::current_core]

set_property DISPLAY_NAME $display_name $core
set_property DESCRIPTION {Parameterized constant value} $core

core_parameter CONST_WIDTH {CONSTANT WIDTH} {Width of the output port.}
core_parameter CONST_VALUE {CONSTANT VALUE} {Value driven on the output port.}
