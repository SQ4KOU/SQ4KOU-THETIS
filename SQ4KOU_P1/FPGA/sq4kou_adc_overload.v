`timescale 1 ns / 1 ps

// SQ4KOU Red Pitaya STEMlab 125-14 / HPSDR Protocol 1
// Physical ADC overload telemetry for Ramdor/Thetis Auto Attenuate RX.
//
// The detector is a PASSIVE tap of adc_0/m_axis_tdata.  It never modifies,
// delays or gates the RX/DDC/PureSignal/WideBand sample stream.
//
// axis_red_pitaya_adc presents ADC-A/IN1 in bits 15:0 and ADC-B/IN2 in
// bits 31:16 as signed 16-bit values carrying the 14-bit ADC sample.
// Full-scale magnitude is 8192 counts.  The default 8064 threshold leaves
// 128 counts (~1.56 %, ~0.14 dB) headroom before mathematical full scale.
//
// Each hit is stretched for HOLD_CYCLES.  At 125 MHz, 1,250,000 cycles =
// 10 ms.  A sustained near-clipping RF waveform continuously retriggers the
// hold, while an isolated transient naturally disappears well before the
// ~400 ms overload qualification already implemented in Thetis.

module sq4kou_adc_overload #(
  parameter integer THRESHOLD   = 8064,
  parameter integer HOLD_CYCLES = 1250000
)(
  input  wire        aclk,
  input  wire        aresetn,
  input  wire        adc_tvalid,
  input  wire [31:0] adc_tdata,
  output wire [1:0]  overload
);

  localparam integer HOLD_BITS = $clog2(HOLD_CYCLES + 1);
  localparam signed [15:0] POS_THRESHOLD = THRESHOLD;
  localparam signed [15:0] NEG_THRESHOLD = -THRESHOLD;

  wire signed [15:0] adc_a = adc_tdata[15:0];
  wire signed [15:0] adc_b = adc_tdata[31:16];

  wire hit_a = adc_tvalid &&
               ((adc_a >= POS_THRESHOLD) || (adc_a <= NEG_THRESHOLD));
  wire hit_b = adc_tvalid &&
               ((adc_b >= POS_THRESHOLD) || (adc_b <= NEG_THRESHOLD));

  reg [HOLD_BITS-1:0] hold_a = {HOLD_BITS{1'b0}};
  reg [HOLD_BITS-1:0] hold_b = {HOLD_BITS{1'b0}};

  always @(posedge aclk) begin
    if (!aresetn) begin
      hold_a <= {HOLD_BITS{1'b0}};
      hold_b <= {HOLD_BITS{1'b0}};
    end else begin
      if (hit_a)
        hold_a <= HOLD_CYCLES;
      else if (hold_a != 0)
        hold_a <= hold_a - 1'b1;

      if (hit_b)
        hold_b <= HOLD_CYCLES;
      else if (hold_b != 0)
        hold_b <= hold_b - 1'b1;
    end
  end

  assign overload[0] = hit_a || (hold_a != 0); // ADC0 / physical IN1
  assign overload[1] = hit_b || (hold_b != 0); // ADC1 / physical IN2

endmodule
