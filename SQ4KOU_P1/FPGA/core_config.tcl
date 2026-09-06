set display_name {AXI WideBand IN2 Snapshot Capture}

set core [ipx::current_core]
set_property DISPLAY_NAME $display_name $core
set_property DESCRIPTION {Passive 16384-sample ADC-B snapshot for HPSDR Protocol-1 endpoint 4} $core

core_parameter AXI_DATA_WIDTH {AXI DATA WIDTH} {Width of the AXI data bus.}
core_parameter AXI_ADDR_WIDTH {AXI ADDR WIDTH} {Width of the AXI address bus.}

set bus [ipx::get_bus_interfaces -of_objects $core s_axi]
set_property NAME S_AXI $bus
set_property INTERFACE_MODE slave $bus

# AXI clock/reset metadata.
set bus [ipx::get_bus_interfaces aclk]
set parameter [ipx::get_bus_parameters -of_objects $bus ASSOCIATED_BUSIF]
if {[llength $parameter] == 0} { set parameter [ipx::add_bus_parameter ASSOCIATED_BUSIF $bus] }
set_property VALUE S_AXI $parameter
set parameter [ipx::get_bus_parameters -of_objects $bus ASSOCIATED_RESET]
if {[llength $parameter] == 0} { set parameter [ipx::add_bus_parameter ASSOCIATED_RESET $bus] }
set_property VALUE aresetn $parameter

# Raw ADC stream belongs to adc_clk, not to the AXI clock.
set bus [ipx::get_bus_interfaces adc_clk]
set parameter [ipx::get_bus_parameters -of_objects $bus ASSOCIATED_BUSIF]
if {[llength $parameter] == 0} { set parameter [ipx::add_bus_parameter ASSOCIATED_BUSIF $bus] }
set_property VALUE adc $parameter
