`timescale 1 ns / 1 ps

// SQ4KOU RP125-14 / HPSDR Protocol 1 WideBand snapshot capture.
//
// This core is deliberately independent of every existing HPSDR register and
// stream.  It passively taps physical IN2/ADC-B from adc_tdata[31:16] and adds
// one new AXI4-Lite slave at 0x48000000.
//
// CPU map (64 KiB):
//   0x0000..0x7fff  8192 words, two chronological signed-16 samples per word
//   0x8000          W bit0: trigger; R bit0: busy, bit1: ready
//   0x8004          R: 0x57423131 ("WB11")
//   0x8008          R: measured 125 MHz clock cycles between PPS edges
//   0x800c          R: PPS interval sequence counter
//   0x8010          R: PPS status (seen/valid/recent)
//   0x8014          R/W: filtered clock count used by GPS discipline
//   0x8018          R/W: software GPS state (alive/locked/holdover)
//
// The resulting 16384-sample snapshot is the exact payload required for one
// 32-packet Protocol-1 endpoint-4 WideBand frame.

module axi_wb_capture #
(
  parameter integer AXI_DATA_WIDTH = 32,
  parameter integer AXI_ADDR_WIDTH = 32
)
(
  input  wire                        aclk,
  input  wire                        aresetn,

  input  wire                        adc_clk,
  input  wire                        adc_tvalid,
  input  wire [31:0]                 adc_tdata,

  // GPS 1 PPS input. Connected to E1 DIO3_N through the existing GPIO
  // debouncer; it is only observed here and does not alter the HPSDR GPIO path.
  input  wire                        pps,

  input  wire [AXI_ADDR_WIDTH-1:0]   s_axi_awaddr,
  input  wire                        s_axi_awvalid,
  output wire                        s_axi_awready,
  input  wire [AXI_DATA_WIDTH-1:0]   s_axi_wdata,
  input  wire [AXI_DATA_WIDTH/8-1:0] s_axi_wstrb,
  input  wire                        s_axi_wvalid,
  output wire                        s_axi_wready,
  output wire [1:0]                  s_axi_bresp,
  output wire                        s_axi_bvalid,
  input  wire                        s_axi_bready,
  input  wire [AXI_ADDR_WIDTH-1:0]   s_axi_araddr,
  input  wire                        s_axi_arvalid,
  output wire                        s_axi_arready,
  output wire [AXI_DATA_WIDTH-1:0]   s_axi_rdata,
  output wire [1:0]                  s_axi_rresp,
  output wire                        s_axi_rvalid,
  input  wire                        s_axi_rready
);

  localparam [15:0] CTRL_OFFSET = 16'h8000;
  localparam [15:0] ID_OFFSET          = 16'h8004;
  localparam [15:0] PPS_COUNT_OFFSET   = 16'h8008;
  localparam [15:0] PPS_SEQ_OFFSET     = 16'h800c;
  localparam [15:0] PPS_STATUS_OFFSET  = 16'h8010;
  localparam [15:0] GPS_USED_OFFSET    = 16'h8014;
  localparam [15:0] GPS_SWSTAT_OFFSET  = 16'h8018;
  localparam [31:0] ID_VALUE           = 32'h57423131;
  localparam [12:0] LAST_WORD   = 13'd8191;

  // AXI write response.  As in the original 2019 Pavel cores, AW and W are
  // accepted together; the Zynq GP0 AXI-Lite interconnect provides this form.
  reg bvalid_reg = 1'b0;
  wire write_accept = s_axi_awvalid && s_axi_wvalid && !bvalid_reg;
  assign s_axi_awready = s_axi_wvalid && !bvalid_reg;
  assign s_axi_wready  = s_axi_awvalid && !bvalid_reg;
  assign s_axi_bresp   = 2'b00;
  assign s_axi_bvalid  = bvalid_reg;

  // Capture request/completion toggle crossing.  The two clocks are normally
  // the same 125 MHz clock in this project, but the synchronizers make that
  // implementation detail non-critical.
  reg req_toggle = 1'b0;
  (* ASYNC_REG = "TRUE" *) reg done_meta = 1'b0;
  (* ASYNC_REG = "TRUE" *) reg done_sync = 1'b0;
  reg done_seen = 1'b0;
  reg done_latched = 1'b0;
  reg [15:0] generation = 16'd0;
  // Software-visible GPS discipline status. The injected ARM thread writes
  // these two words; FPGA only stores/returns them.
  reg [31:0] gps_used_count = 32'd0;
  reg [31:0] gps_sw_status  = 32'd0;
  wire busy = req_toggle ^ done_sync;

  wire trigger = write_accept && s_axi_wstrb[0] && s_axi_wdata[0] &&
                 (s_axi_awaddr[15:0] == CTRL_OFFSET) && !busy;

  // AXI read response.  The memory and special-register paths both have one
  // clock of latency, so rvalid and the selected data are naturally aligned.
  reg rvalid_reg = 1'b0;
  reg read_is_memory = 1'b0;
  reg [31:0] special_rdata = 32'd0;
  wire read_accept = s_axi_arvalid && s_axi_arready;
  wire read_memory = read_accept && (s_axi_araddr[15:0] < CTRL_OFFSET);
  wire [12:0] read_word_addr = s_axi_araddr[14:2];
  (* ram_style = "block" *) reg [31:0] wb_memory [0:8191];
  reg [31:0] memory_rdata = 32'd0;

  assign s_axi_arready = !rvalid_reg || s_axi_rready;
  assign s_axi_rdata   = read_is_memory ? memory_rdata : special_rdata;
  assign s_axi_rresp   = 2'b00;
  assign s_axi_rvalid  = rvalid_reg;

  // Port B: synchronous CPU read.  Vivado infers the read port of a true
  // dual-port block RAM; no version-specific XPM parameters are required.
  always @(posedge aclk)
  begin
    if (read_memory)
      memory_rdata <= wb_memory[read_word_addr];
  end

  always @(posedge aclk)
  begin
    if (!aresetn)
    begin
      bvalid_reg    <= 1'b0;
      rvalid_reg    <= 1'b0;
      read_is_memory <= 1'b0;
      special_rdata <= 32'd0;
      req_toggle    <= 1'b0;
      done_meta     <= 1'b0;
      done_sync     <= 1'b0;
      done_seen     <= 1'b0;
      done_latched  <= 1'b0;
      generation    <= 16'd0;
      gps_used_count <= 32'd0;
      gps_sw_status  <= 32'd0;
    end
    else
    begin
      done_meta <= done_toggle;
      done_sync <= done_meta;

      if (write_accept)
        bvalid_reg <= 1'b1;
      else if (bvalid_reg && s_axi_bready)
        bvalid_reg <= 1'b0;

      if (trigger)
      begin
        req_toggle   <= ~req_toggle;
        done_latched <= 1'b0;
      end

      if (write_accept && (s_axi_wstrb == 4'b1111))
      begin
        if (s_axi_awaddr[15:0] == GPS_USED_OFFSET)
          gps_used_count <= s_axi_wdata;
        else if (s_axi_awaddr[15:0] == GPS_SWSTAT_OFFSET)
          gps_sw_status <= s_axi_wdata;
      end

      if (done_sync != done_seen)
      begin
        done_seen    <= done_sync;
        done_latched <= 1'b1;
        generation   <= generation + 1'b1;
      end

      if (read_accept)
      begin
        rvalid_reg     <= 1'b1;
        read_is_memory <= (s_axi_araddr[15:0] < CTRL_OFFSET);
        if (s_axi_araddr[15:0] == CTRL_OFFSET)
          special_rdata <= {generation, 14'd0, done_latched, busy};
        else if (s_axi_araddr[15:0] == ID_OFFSET)
          special_rdata <= ID_VALUE;
        else if (s_axi_araddr[15:0] == PPS_COUNT_OFFSET)
          special_rdata <= pps_count_latched;
        else if (s_axi_araddr[15:0] == PPS_SEQ_OFFSET)
          special_rdata <= pps_seq;
        else if (s_axi_araddr[15:0] == PPS_STATUS_OFFSET)
          special_rdata <= {29'd0, pps_recent, pps_valid, pps_seen};
        else if (s_axi_araddr[15:0] == GPS_USED_OFFSET)
          special_rdata <= gps_used_count;
        else if (s_axi_araddr[15:0] == GPS_SWSTAT_OFFSET)
          special_rdata <= gps_sw_status;
        else
          special_rdata <= 32'd0;
      end
      else if (rvalid_reg && s_axi_rready)
        rvalid_reg <= 1'b0;
    end
  end

  // ADC-domain snapshot writer: two consecutive ADC-B samples per BRAM word.
  wire [15:0] adc_b_sample = adc_tdata[31:16];
  // Explicit FPGA INIT values make the ADC side deterministic even when the
  // processor reset is already deasserted immediately after configuration.
  // Reset still has synchronous priority below; these initializers are not a
  // substitute for reset and do not initialize the BRAM contents.
  (* ASYNC_REG = "TRUE" *) reg rst_meta_adc = 1'b1;
  (* ASYNC_REG = "TRUE" *) reg rst_sync_adc = 1'b1;
  (* ASYNC_REG = "TRUE" *) reg req_meta_adc = 1'b0;
  (* ASYNC_REG = "TRUE" *) reg req_sync_adc = 1'b0;

  // PPS synchronizer and reciprocal-second counter. adc_clk and aclk are both
  // connected to pll_0/clk_out1 in this project. The first PPS arms the
  // measurement; each subsequent rising edge latches the number of 125 MHz
  // clock periods since the previous PPS.
  (* ASYNC_REG = "TRUE" *) reg pps_meta = 1'b0;
  (* ASYNC_REG = "TRUE" *) reg pps_sync = 1'b0;
  reg pps_prev = 1'b0;
  reg pps_seen = 1'b0;
  reg [31:0] pps_interval = 32'd0;
  reg [31:0] pps_count_latched = 32'd0;
  reg [31:0] pps_seq = 32'd0;
  reg [31:0] pps_age = 32'hffffffff;
  wire pps_rise = pps_sync && !pps_prev;
  wire pps_valid = (pps_seq != 32'd0);
  wire pps_recent = pps_seen && (pps_age < 32'd250000000);

  reg done_toggle = 1'b0;
  reg capturing = 1'b0;
  reg half = 1'b0;
  reg [15:0] sample_lo = 16'd0;
  reg [12:0] write_word = 13'd0;

  wire memory_write = capturing && adc_tvalid && half;
  wire [31:0] memory_wdata = {adc_b_sample, sample_lo};

  // Port A: ADC-domain write.
  always @(posedge adc_clk)
  begin
    if (memory_write)
      wb_memory[write_word] <= memory_wdata;
  end

  always @(posedge adc_clk)
  begin
    rst_meta_adc <= !aresetn;
    rst_sync_adc <= rst_meta_adc;

    if (rst_sync_adc)
    begin
      req_meta_adc <= 1'b0;
      req_sync_adc <= 1'b0;
      pps_meta     <= 1'b0;
      pps_sync     <= 1'b0;
      pps_prev     <= 1'b0;
      pps_seen     <= 1'b0;
      pps_interval <= 32'd0;
      pps_count_latched <= 32'd0;
      pps_seq      <= 32'd0;
      pps_age      <= 32'hffffffff;
      done_toggle  <= 1'b0;
      capturing    <= 1'b0;
      half         <= 1'b0;
      sample_lo    <= 16'd0;
      write_word   <= 13'd0;
    end
    else
    begin
      req_meta_adc <= req_toggle;
      req_sync_adc <= req_meta_adc;

      pps_meta <= pps;
      pps_sync <= pps_meta;
      pps_prev <= pps_sync;
      if (pps_rise)
      begin
        if (pps_seen)
        begin
          pps_count_latched <= pps_interval + 1'b1;
          pps_seq <= pps_seq + 1'b1;
        end
        else
          pps_seen <= 1'b1;
        pps_interval <= 32'd0;
        pps_age <= 32'd0;
      end
      else
      begin
        if (pps_interval != 32'hffffffff)
          pps_interval <= pps_interval + 1'b1;
        if (pps_seen && (pps_age != 32'hffffffff))
          pps_age <= pps_age + 1'b1;
      end

      if (!capturing && (req_sync_adc != done_toggle))
      begin
        capturing  <= 1'b1;
        half       <= 1'b0;
        write_word <= 13'd0;
      end
      else if (capturing && adc_tvalid)
      begin
        if (!half)
        begin
          sample_lo <= adc_b_sample;
          half      <= 1'b1;
        end
        else
        begin
          half <= 1'b0;
          if (write_word == LAST_WORD)
          begin
            capturing   <= 1'b0;
            done_toggle <= req_sync_adc;
          end
          else
            write_word <= write_word + 1'b1;
        end
      end
    end
  end

endmodule
