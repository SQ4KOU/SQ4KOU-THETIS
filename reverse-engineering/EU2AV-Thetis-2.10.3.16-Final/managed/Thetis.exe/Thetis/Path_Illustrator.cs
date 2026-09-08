using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis;

public class Path_Illustrator : Form
{
	private Console console;

	private bool bool_HPSDR;

	private bool bool_HERMES;

	private bool bool_ANAN_10E;

	private bool bool_ANAN_100_PA_rev15;

	private bool bool_ANAN_100_PA_rev24;

	private bool bool_ANAN_100D_PA_rev15;

	private bool bool_ANAN_100D_PA_rev24;

	private bool bool_rx;

	private bool bool_ANT1;

	private bool bool_ANT1_TX;

	private bool bool_ANT2;

	private bool bool_ANT2_TX;

	private bool bool_ANT3;

	private bool bool_ANT3_TX;

	private bool bool_DUAL_MERCURY_ALEX;

	private bool bool_MON;

	private bool bool_RX1_MUTE;

	private bool bool_RX2_MUTE;

	private bool bool_duplex;

	private bool bool_PureSignal;

	private bool bool_diversity;

	private bool bool_XVTR;

	private bool bool_EXT1;

	private bool bool_EXT1_on_TX;

	private bool bool_EXT2;

	private bool bool_EXT2_on_TX;

	private bool bool_BYPASS;

	private bool bool_disable_BYPASS;

	private bool bool_HPF_BYPASS;

	private bool bool_DisableHPFOnTx;

	private bool bool_BYPASS_on_TX;

	private bool bool_Rx0_0 = true;

	private bool bool_Rx0_1;

	private bool bool_Rx1_0;

	private bool bool_Rx1_1;

	private bool bool_Rx2_0;

	private bool bool_Rx2_1;

	private bool bool_Rx3_0;

	private bool bool_Rx3_1;

	private bool bool_Rx4_0;

	private bool bool_Rx4_1;

	private bool bool_Rx5_0;

	private bool bool_Rx5_1;

	private bool bool_Rx6_0;

	private bool bool_Rx6_1;

	private bool bool_RX1_OUT_on_TX;

	private bool bool_RX1_IN_on_TX;

	private bool bool_RX2_IN_on_TX;

	private int int_RxAnt_switch;

	private int int_TxAnt_switch;

	private Pen blackPen = new Pen(Color.Black);

	private Pen blackPen2 = new Pen(Color.Black);

	private Pen bluePen = new Pen(Color.Blue);

	private Pen indianredPen = new Pen(Color.IndianRed);

	private Pen redPen = new Pen(Color.Red);

	private Pen myPen = new Pen(Color.Black);

	private Graphics g;

	private static bool update_diagram = true;

	private static int std_size = 50;

	private static int SDR_width = 665;

	private static int SDR_height = 625;

	private static int FPGA_width = 230;

	private static int FPGA_height = 600;

	private static int PC_width = 390;

	private static int PC_height = 350;

	private static int DSP_width = 50;

	private static int DSP_height = 260;

	private static int RX_Display_width = 200;

	private static int RX_Display_height = 80;

	private static int ALEX_offset_x = 65;

	private static int ALEX_offset_y = 35;

	private static Rectangle ALEX = new Rectangle(ALEX_offset_x, ALEX_offset_y, 100, 275);

	private static int ALEX_HPF_offset_x = ALEX.X + 10;

	private static int ALEX_HPF_offset_y = ALEX.Y + 25;

	private static Rectangle ALEX_HPF = new Rectangle(ALEX_HPF_offset_x, ALEX_HPF_offset_y, 80, 115);

	private static int ALEX_LPF_offset_x = ALEX.X + 10;

	private static int ALEX_LPF_offset_y = ALEX.Y + 150;

	private static Rectangle ALEX_LPF = new Rectangle(ALEX_LPF_offset_x, ALEX_LPF_offset_y, 80, 115);

	private static int ALEX_2_offset_x = 65;

	private static int ALEX_2_offset_y = 390;

	private static Rectangle ALEX_2 = new Rectangle(ALEX_2_offset_x, ALEX_2_offset_y, 100, 275);

	private static int ALEX_2_HPF_offset_x = ALEX_2.X + 10;

	private static int ALEX_2_HPF_offset_y = ALEX_2.Y + 25;

	private static Rectangle ALEX_2_HPF = new Rectangle(ALEX_2_HPF_offset_x, ALEX_2_HPF_offset_y, 80, 115);

	private static int ALEX_2_LPF_offset_x = ALEX_2.X + 10;

	private static int ALEX_2_LPF_offset_y = ALEX_2.Y + 150;

	private static Rectangle ALEX_2_LPF = new Rectangle(ALEX_2_LPF_offset_x, ALEX_2_LPF_offset_y, 80, 115);

	private static int SDR_offset_x = 80;

	private static int SDR_offset_y = 30;

	private static Rectangle SDR = new Rectangle(SDR_offset_x, SDR_offset_y, SDR_width, SDR_height);

	private static int FPGA_offset_x = 500;

	private static int FPGA_offset_y = 40;

	private static Rectangle FPGA = new Rectangle(FPGA_offset_x, FPGA_offset_y, FPGA_width, FPGA_height);

	private static int PC_offset_x = 760;

	private static int PC_offset_y = 30;

	private static Rectangle PC = new Rectangle(PC_offset_x, PC_offset_y, PC_width, PC_height);

	private static int DSP_offset_x = PC.X + 30;

	private static int DSP_offset_y = PC.Y + 80;

	private static Rectangle DSP = new Rectangle(DSP_offset_x, DSP_offset_y, DSP_width, DSP_height);

	private static int DSP_HPSDR_offset_x = PC.X + 30;

	private static int DSP_HPSDR_offset_y = PC.Y + 30;

	private static Rectangle DSP_HPSDR = new Rectangle(DSP_HPSDR_offset_x, DSP_HPSDR_offset_y, 50, 260);

	private static Rectangle DSP_HERMES = new Rectangle(DSP_HPSDR_offset_x, DSP_HPSDR_offset_y, 50, 300);

	private static int HPF_offset_x = 230;

	private static int HPF_offset_y = 180;

	private static Rectangle HPF = new Rectangle(HPF_offset_x, HPF_offset_y, std_size, std_size);

	private static int HPF2_offset_x = 230;

	private static int HPF2_offset_y = 530;

	private static Rectangle HPF2 = new Rectangle(HPF2_offset_x, HPF2_offset_y, std_size, std_size);

	private static int LPF_offset_x = 310;

	private static int LPF_offset_y = 280;

	private static Rectangle LPF = new Rectangle(LPF_offset_x, LPF_offset_y, std_size, std_size);

	private static int LPF2_offset_x = 230;

	private static int LPF2_offset_y = 605;

	private static Rectangle LPF2 = new Rectangle(LPF2_offset_x, LPF2_offset_y, std_size, std_size);

	private static int MERCURY_offset_x = 275;

	private static int MERCURY_offset_y = 35;

	private static Rectangle MERCURY = new Rectangle(MERCURY_offset_x, MERCURY_offset_y, 160, 235);

	private static int MERCURY_ADC_offset_x = MERCURY.X + 10;

	private static int MERCURY_ADC_offset_y = MERCURY.Y + 40;

	private static Rectangle MERCURY_ADC = new Rectangle(MERCURY_ADC_offset_x, MERCURY_ADC_offset_y, 50, 50);

	private static int MERCURY_CODEC_offset_x = MERCURY.X + 10;

	private static int MERCURY_CODEC_offset_y = MERCURY.Y + 180;

	private static Rectangle MERCURY_CODEC = new Rectangle(MERCURY_CODEC_offset_x, MERCURY_CODEC_offset_y, 50, 50);

	private static int MERCURY_DDC0_offset_x = MERCURY.X + 95;

	private static int MERCURY_DDC0_offset_y = MERCURY.Y + 40;

	private static Rectangle MERCURY_DDC0 = new Rectangle(MERCURY_DDC0_offset_x, MERCURY_DDC0_offset_y, 50, 50);

	private static int MERCURY_DDC1_offset_x = MERCURY.X + 95;

	private static int MERCURY_DDC1_offset_y = MERCURY.Y + 100;

	private static Rectangle MERCURY_DDC1 = new Rectangle(MERCURY_DDC1_offset_x, MERCURY_DDC1_offset_y, 50, 50);

	private static int MERCURY_DDC2_offset_x = MERCURY.X + 95;

	private static int MERCURY_DDC2_offset_y = MERCURY.Y + 160;

	private static Rectangle MERCURY_DDC2 = new Rectangle(MERCURY_DDC2_offset_x, MERCURY_DDC2_offset_y, 50, 50);

	private static int MERCURY_FPGA_offset_x = MERCURY.X + 75;

	private static int MERCURY_FPGA_offset_y = MERCURY.Y + 20;

	private static Rectangle MERCURY_FPGA = new Rectangle(MERCURY_FPGA_offset_x, MERCURY_FPGA_offset_y, 75, 207);

	private static int MERCURY_2_offset_x = 275;

	private static int MERCURY_2_offset_y = 430;

	private static Rectangle MERCURY_2 = new Rectangle(MERCURY_2_offset_x, MERCURY_2_offset_y, 160, 235);

	private static int MERCURY_2_ADC_offset_x = MERCURY_2.X + 10;

	private static int MERCURY_2_ADC_offset_y = MERCURY_2.Y + 40;

	private static Rectangle MERCURY_2_ADC = new Rectangle(MERCURY_2_ADC_offset_x, MERCURY_2_ADC_offset_y, 50, 50);

	private static int MERCURY_2_CODEC_offset_x = MERCURY_2.X + 10;

	private static int MERCURY_2_CODEC_offset_y = MERCURY_2.Y + 180;

	private static Rectangle MERCURY_2_CODEC = new Rectangle(MERCURY_2_CODEC_offset_x, MERCURY_2_CODEC_offset_y, 50, 50);

	private static int MERCURY_2_DDC0_offset_x = MERCURY_2.X + 95;

	private static int MERCURY_2_DDC0_offset_y = MERCURY_2.Y + 40;

	private static Rectangle MERCURY_2_DDC0 = new Rectangle(MERCURY_2_DDC0_offset_x, MERCURY_2_DDC0_offset_y, 50, 50);

	private static int MERCURY_2_DDC1_offset_x = MERCURY_2.X + 95;

	private static int MERCURY_2_DDC1_offset_y = MERCURY_2.Y + 100;

	private static Rectangle MERCURY_2_DDC1 = new Rectangle(MERCURY_2_DDC1_offset_x, MERCURY_2_DDC1_offset_y, 50, 50);

	private static int MERCURY_2_DDC2_offset_x = MERCURY_2.X + 95;

	private static int MERCURY_2_DDC2_offset_y = MERCURY_2.Y + 160;

	private static Rectangle MERCURY_2_DDC2 = new Rectangle(MERCURY_2_DDC2_offset_x, MERCURY_2_DDC2_offset_y, 50, 50);

	private static int MERCURY_2_FPGA_offset_x = MERCURY_2.X + 75;

	private static int MERCURY_2_FPGA_offset_y = MERCURY_2.Y + 20;

	private static Rectangle MERCURY_2_FPGA = new Rectangle(MERCURY_2_FPGA_offset_x, MERCURY_2_FPGA_offset_y, 75, 207);

	private static int METIS_offset_x = 620;

	private static int METIS_offset_y = 30;

	private static Rectangle METIS = new Rectangle(METIS_offset_x, METIS_offset_y, 80, 600);

	private static int METIS_FPGA_offset_x = METIS.X + 15;

	private static int METIS_FPGA_offset_y = METIS.Y + 30;

	private static Rectangle METIS_FPGA = new Rectangle(METIS_FPGA_offset_x, METIS_FPGA_offset_y, 50, 550);

	private static int PA_offset_x = 230;

	private static int PA_offset_y = 380;

	private static Rectangle PA = new Rectangle(PA_offset_x, PA_offset_y, std_size, std_size);

	private static int PENELOPE_offset_x = 275;

	private static int PENELOPE_offset_y = 280;

	private static Rectangle PENELOPE = new Rectangle(PENELOPE_offset_x, PENELOPE_offset_y, 295, 142);

	private static int PENELOPE_FPGA_offset_x = PENELOPE.X + 210;

	private static int PENELOPE_FPGA_offset_y = PENELOPE.Y + 10;

	private static Rectangle PENELOPE_FPGA = new Rectangle(PENELOPE_FPGA_offset_x, PENELOPE_FPGA_offset_y, 70, 120);

	private static int PENELOPE_PA_offset_x = PENELOPE.X + 10;

	private static int PENELOPE_PA_offset_y = PENELOPE.Y + 23;

	private static Rectangle PENELOPE_PA = new Rectangle(PENELOPE_PA_offset_x, PENELOPE_PA_offset_y, 50, 50);

	private static int PENELOPE_AMPF_offset_x = PENELOPE.X + 80;

	private static int PENELOPE_AMPF_offset_y = PENELOPE.Y + 23;

	private static Rectangle PENELOPE_AMPF = new Rectangle(PENELOPE_AMPF_offset_x, PENELOPE_AMPF_offset_y, 50, 50);

	private static int PENELOPE_DAC_offset_x = PENELOPE.X + 145;

	private static int PENELOPE_DAC_offset_y = PENELOPE.Y + 23;

	private static Rectangle PENELOPE_DAC = new Rectangle(PENELOPE_DAC_offset_x, PENELOPE_DAC_offset_y, 50, 50);

	private static int PENELOPE_DUC_offset_x = PENELOPE.X + 220;

	private static int PENELOPE_DUC_offset_y = PENELOPE.Y + 30;

	private static Rectangle PENELOPE_DUC = new Rectangle(PENELOPE_DUC_offset_x, PENELOPE_DUC_offset_y, 50, 50);

	private static int PENELOPE_CODEC_offset_x = PENELOPE.X + 10;

	private static int PENELOPE_CODEC_offset_y = PENELOPE.Y + 86;

	private static Rectangle PENELOPE_CODEC = new Rectangle(PENELOPE_CODEC_offset_x, PENELOPE_CODEC_offset_y, 50, 50);

	private static int AMP_offset_x = 230;

	private static int AMP_offset_y = 480;

	private static Rectangle AMP = new Rectangle(AMP_offset_x, AMP_offset_y, std_size, std_size);

	private static int AUDIO_AMP_offset_x = 200;

	private static int AUDIO_AMP_offset_y = 540;

	private static Rectangle AUDIO_AMP = new Rectangle(AUDIO_AMP_offset_x, AUDIO_AMP_offset_y, std_size, std_size);

	private static int AMPF_offset_x = 315;

	private static int AMPF_offset_y = 480;

	private static Rectangle AMPF = new Rectangle(AMPF_offset_x, AMPF_offset_y, std_size, std_size);

	private static int AUDIO_MIXER_offset_x = 935;

	private static int AUDIO_MIXER_offset_y = 235;

	private static Rectangle AUDIO_MIXER = new Rectangle(AUDIO_MIXER_offset_x, AUDIO_MIXER_offset_y, 200, 50);

	private static int DAC0_offset_x = 400;

	private static int DAC0_offset_y = 480;

	private static Rectangle DAC0 = new Rectangle(DAC0_offset_x, DAC0_offset_y, std_size, std_size);

	private static int CODEC_offset_x = 400;

	private static int CODEC_offset_y = 600;

	private static Rectangle CODEC = new Rectangle(CODEC_offset_x, CODEC_offset_y, std_size, std_size);

	private static int CODEC2_offset_x = 400;

	private static int CODEC2_offset_y = 540;

	private static Rectangle CODEC2 = new Rectangle(CODEC2_offset_x, CODEC2_offset_y, std_size, std_size);

	private static int DUC0_offset_x = 550;

	private static int DUC0_offset_y = 480;

	private static Rectangle DUC0 = new Rectangle(DUC0_offset_x, DUC0_offset_y, std_size, std_size);

	private static int ADC0_offset_x = 400;

	private static int ADC0_offset_y = 50;

	private static Rectangle ADC0 = new Rectangle(ADC0_offset_x, ADC0_offset_y, std_size, std_size);

	private static int ADC1_offset_x = 400;

	private static int ADC1_offset_y = 138;

	private static Rectangle ADC1 = new Rectangle(ADC1_offset_x, ADC1_offset_y, std_size, std_size);

	private static int ADC2_offset_x = 400;

	private static int ADC2_offset_y = 250;

	private static Rectangle ADC2 = new Rectangle(ADC2_offset_x, ADC2_offset_y, std_size, std_size);

	private static int ADC3_offset_x = 400;

	private static int ADC3_offset_y = 350;

	private static Rectangle ADC3 = new Rectangle(ADC3_offset_x, ADC3_offset_y, std_size, std_size);

	private static int Rx0_offset_x = 650;

	private static int Rx0_offset_y = 50;

	private static Rectangle Rx0 = new Rectangle(Rx0_offset_x, Rx0_offset_y, std_size, std_size);

	private static int Rx1_offset_x = 650;

	private static int Rx1_offset_y = 125;

	private static Rectangle Rx1 = new Rectangle(Rx1_offset_x, Rx1_offset_y, std_size, std_size);

	private static int Rx2_offset_x = 650;

	private static int Rx2_offset_y = 200;

	private static Rectangle Rx2 = new Rectangle(Rx2_offset_x, Rx2_offset_y, std_size, std_size);

	private static int Rx3_offset_x = 650;

	private static int Rx3_offset_y = 275;

	private static Rectangle Rx3 = new Rectangle(Rx3_offset_x, Rx3_offset_y, std_size, std_size);

	private static int Rx4_offset_x = 650;

	private static int Rx4_offset_y = 350;

	private static Rectangle Rx4 = new Rectangle(Rx4_offset_x, Rx4_offset_y, std_size, std_size);

	private static int Rx5_offset_x = 650;

	private static int Rx5_offset_y = 429;

	private static Rectangle Rx5 = new Rectangle(Rx5_offset_x, Rx5_offset_y, std_size, std_size);

	private static int Rx6_offset_x = 650;

	private static int Rx6_offset_y = 509;

	private static Rectangle Rx6 = new Rectangle(Rx6_offset_x, Rx6_offset_y, std_size, std_size);

	private static int RXn_Display_offset_x = PC.X + 175;

	private static int RXn_Display_offset_y = PC.Y + 15;

	private static int RX1_DISPLAY_x = RXn_Display_offset_x;

	private static int RX1_DISPLAY_y = RXn_Display_offset_y;

	private static int RX2_DISPLAY_x = RX1_DISPLAY_x;

	private static int RX2_DISPLAY_y = RX1_DISPLAY_y + 95;

	private static Rectangle RX1_DISPLAY = new Rectangle(RX1_DISPLAY_x, RX1_DISPLAY_y, RX_Display_width, RX_Display_height);

	private static Rectangle RX2_DISPLAY = new Rectangle(RX2_DISPLAY_x, RX2_DISPLAY_y, RX_Display_width, RX_Display_height);

	private static int SWR_offset_x = LPF.X - 80;

	private static int SWR_offset_y = LPF.Y;

	private static Rectangle SWR = new Rectangle(SWR_offset_x, SWR_offset_y, 50, 50);

	private static Point ADC0_L = new Point(ADC0.X, ADC0.Y + 25);

	private static Point ADC0_L_c = new Point(ADC0.X - 20, ADC0.Y + 25);

	private static Point ADC0_R = new Point(ADC0.X + 50, ADC0.Y + 25);

	private static Point ADC0_R_c = new Point(ADC0.X + 70, ADC0.Y + 25);

	private static Point ADC0_corner1_Rx1 = new Point(FPGA.X + 20, ADC0_R.Y);

	private static Point ADC0_corner2_Rx1 = new Point(FPGA.X + 20, Rx1.Y + 25);

	private static Point ADC0_corner1_Rx2 = new Point(FPGA.X + 40, ADC0_R.Y);

	private static Point ADC0_corner2_Rx2 = new Point(FPGA.X + 40, Rx2.Y + 25);

	private static Point ADC0_corner1_Rx3 = new Point(FPGA.X + 60, ADC0_R.Y);

	private static Point ADC0_corner2_Rx3 = new Point(FPGA.X + 60, Rx3.Y + 25);

	private static Point ADC0_corner1_Rx4 = new Point(FPGA.X + 80, ADC0_R.Y);

	private static Point ADC0_corner2_Rx4 = new Point(FPGA.X + 80, Rx4.Y + 25);

	private static Point ADC0_corner1_Rx5 = new Point(FPGA.X + 100, ADC0_R.Y);

	private static Point ADC0_corner2_Rx5 = new Point(FPGA.X + 100, Rx5.Y + 25);

	private static Point ADC0_corner1_Rx6 = new Point(FPGA.X + 120, ADC0.Y + 25);

	private static Point ADC0_corner2_Rx6 = new Point(FPGA.X + 120, Rx6.Y + 25);

	private static Point ADC0_R_corner = new Point(DSP.X - 20, ADC0_R.Y);

	private static Point ADC1_L = new Point(ADC1.X, ADC1.Y + 25);

	private static Point ADC1_L_c = new Point(ADC1.X - 20, ADC1.Y + 25);

	private static Point ADC1_R = new Point(ADC1.X + 50, ADC1.Y + 25);

	private static Point ADC1_R_c = new Point(ADC1.X + 70, ADC1.Y + 25);

	private static Point ADC1_L_corner1 = new Point(ADC1.X - 255, ADC1.Y + 25);

	private static Point ADC1_L_corner2 = new Point(ADC1.X - 255, SDR.Y + 45);

	private static Point ADC1_L_corner3 = new Point(ADC1.X - 220, ADC1.Y + 25);

	private static Point ADC1_corner1_Rx0 = new Point(FPGA.X + 10, ADC1_R.Y);

	private static Point ADC1_corner2_Rx0 = new Point(FPGA.X + 10, Rx0.Y + 35);

	private static Point ADC1_corner1_Rx1 = new Point(FPGA.X + 30, ADC1_R.Y);

	private static Point ADC1_corner2_Rx1 = new Point(FPGA.X + 30, Rx1.Y + 25);

	private static Point ADC1_corner1_Rx2 = new Point(FPGA.X + 50, ADC1_R.Y);

	private static Point ADC1_corner2_Rx2 = new Point(FPGA.X + 50, Rx2.Y + 25);

	private static Point ADC1_corner1_Rx3 = new Point(FPGA.X + 70, ADC1_R.Y);

	private static Point ADC1_corner2_Rx3 = new Point(FPGA.X + 70, Rx3.Y + 25);

	private static Point ADC1_corner1_Rx4 = new Point(FPGA.X + 90, ADC1_R.Y);

	private static Point ADC1_corner2_Rx4 = new Point(FPGA.X + 90, Rx4.Y + 25);

	private static Point ADC1_corner1_Rx5 = new Point(FPGA.X + 110, ADC1_R.Y);

	private static Point ADC1_corner2_Rx5 = new Point(FPGA.X + 110, Rx5.Y + 25);

	private static Point ADC1_corner1_Rx6 = new Point(FPGA.X + 130, ADC1.Y + 25);

	private static Point ADC1_corner2_Rx6 = new Point(FPGA.X + 130, Rx6.Y + 25);

	private static Point ADC2_L = new Point(ADC2.X, ADC2.Y + 25);

	private static Point ADC2_L_c = new Point(ADC2.X - 20, ADC2.Y + 25);

	private static Point ADC2_R = new Point(ADC2.X + 50, ADC2.Y + 25);

	private static Point ADC2_R_c = new Point(ADC2.X + 70, ADC2.Y + 25);

	private static Point ALEX_label = new Point(ALEX.X + 5, ALEX.Y + 5);

	private static Point ALEX_HPF_label = new Point(ALEX_HPF.X + 5, ALEX_HPF.Y + 5);

	private static Point ALEX_HPF_corner1 = new Point(ALEX_HPF.X + 40, ALEX.Y + 65);

	private static Point ALEX_HPF_B = new Point(ALEX.X + 50, ALEX.Y + 140);

	private static Point ALEX_HPF_corner2 = new Point(ALEX_HPF.X + 40, ALEX.Y + 125);

	private static Point ALEX_RX1_out = new Point(ALEX.X, ALEX.Y + 125);

	private static Point ALEX_HPF_corner3 = new Point(ALEX_HPF.X + 40, ALEX.Y + 50);

	private static Point ALEX_XV_RX_IN = new Point(ALEX.X, ALEX.Y + 50);

	private static Point ALEX_HPF_corner4 = new Point(ALEX_HPF.X + 40, ALEX.Y + 80);

	private static Point ALEX_RX_2_IN = new Point(ALEX.X, ALEX.Y + 80);

	private static Point ALEX_HPF_corner5 = new Point(ALEX_HPF.X + 40, ALEX.Y + 105);

	private static Point ALEX_RX_1_IN = new Point(ALEX.X, ALEX.Y + 105);

	private static Point ALEX_LPF_label = new Point(ALEX_LPF.X + 5, ALEX_LPF.Y + 5);

	private static Point ALEX_LPF_corner1 = new Point(ALEX_LPF.X + 40, ALEX_LPF.Y + 27);

	private static Point ALEX_LPF_ANT1 = new Point(ALEX.X, ALEX_LPF.Y + 27);

	private static Point ALEX_LPF_corner2 = new Point(ALEX_LPF.X + 40, ALEX_LPF.Y + 52);

	private static Point ALEX_LPF_ANT2 = new Point(ALEX.X, ALEX_LPF.Y + 52);

	private static Point ALEX_LPF_corner3 = new Point(ALEX_LPF.X + 40, ALEX_LPF.Y + 78);

	private static Point ALEX_LPF_ANT3 = new Point(ALEX.X, ALEX_LPF.Y + 78);

	private static Point ALEX_To_RX_label = new Point(ALEX.X + 103, ALEX.Y + 45);

	private static Point ALEX_RX_out = new Point(ALEX.X + 100, ALEX.Y + 65);

	private static Point ALEX_ANT1 = new Point(ALEX.X, ALEX.Y + 175);

	private static Point ALEX_ANT1_corner = new Point(ALEX.X + 50, ALEX.Y + 175);

	private static Point ALEX_ANT2 = new Point(ALEX.X, ALEX.Y + 200);

	private static Point ALEX_ANT2_corner = new Point(ALEX.X + 50, ALEX.Y + 200);

	private static Point ALEX_ANT3 = new Point(ALEX.X, ALEX.Y + 225);

	private static Point ALEX_ANT3_corner = new Point(ALEX.X + 50, ALEX.Y + 225);

	private static Point ALEX_2_label = new Point(ALEX_2.X + 5, ALEX_2.Y + 5);

	private static Point ALEX_2_HPF_label = new Point(ALEX_2_HPF.X + 5, ALEX_2_HPF.Y + 5);

	private static Point ALEX_2_HPF_corner1 = new Point(ALEX_2_HPF.X + 40, ALEX_2.Y + 105);

	private static Point ALEX_2_HPF_B = new Point(ALEX_2.X + 50, ALEX_2.Y + 140);

	private static Point ALEX_2_HPF_corner2 = new Point(ALEX_2_HPF.X + 40, ALEX_2.Y + 125);

	private static Point ALEX_2_RX1_out = new Point(ALEX_2.X, ALEX_2.Y + 125);

	private static Point ALEX_2_HPF_corner3 = new Point(ALEX_2_HPF.X + 40, ALEX_2.Y + 50);

	private static Point ALEX_2_XV_RX_IN = new Point(ALEX_2.X, ALEX_2.Y + 50);

	private static Point ALEX_2_HPF_corner4 = new Point(ALEX_2_HPF.X + 40, ALEX_2.Y + 80);

	private static Point ALEX_2_RX_2_IN = new Point(ALEX_2.X, ALEX_2.Y + 80);

	private static Point ALEX_2_HPF_corner5 = new Point(ALEX_2_HPF.X + 40, ALEX_2.Y + 105);

	private static Point ALEX_2_RX_1_IN = new Point(ALEX_2.X, ALEX_2.Y + 105);

	private static Point ALEX_2_LPF_label = new Point(ALEX_2_LPF.X + 5, ALEX_2_LPF.Y + 5);

	private static Point ALEX_2_To_RX_label = new Point(ALEX_2.X + 105, ALEX_2.Y + 85);

	private static Point ALEX_2_RX_out = new Point(ALEX_2.X + 100, ALEX_2.Y + 105);

	private static Point ALEX_2_RX_out_corner1 = new Point(ALEX_2.X + 150, ALEX_2.Y + 105);

	private static Point ALEX_2_ANT1 = new Point(ALEX_2.X, ALEX_2.Y + 175);

	private static Point ALEX_2_ANT1_corner = new Point(ALEX_2.X + 50, ALEX_2.Y + 175);

	private static Point ALEX_2_ANT2 = new Point(ALEX_2.X, ALEX_2.Y + 200);

	private static Point ALEX_2_ANT2_corner = new Point(ALEX_2.X + 50, ALEX_2.Y + 200);

	private static Point ALEX_2_ANT3 = new Point(ALEX_2.X, ALEX_2.Y + 225);

	private static Point ALEX_2_ANT3_corner = new Point(ALEX_2.X + 50, ALEX_2.Y + 225);

	private static Point AMPF_R = new Point(AMPF.X + 50, AMPF.Y + 25);

	private static Point AMPF_L = new Point(AMPF.X, AMPF.Y + 25);

	private static Point AMPF_L_PA15 = new Point(AMPF.X - 60, AMPF_L.Y);

	private static Point AUDIO_AMP_L = new Point(AUDIO_AMP.X, AUDIO_AMP.Y + 25);

	private static Point AUDIO_AMP_L_c = new Point(AUDIO_AMP.X - 20, AUDIO_AMP.Y + 25);

	private static Point AUDIO_AMP_R = new Point(AUDIO_AMP.X + 50, AUDIO_AMP.Y + 25);

	private static Point AUDIO_AMP_R_c = new Point(AUDIO_AMP.X + 70, AUDIO_AMP.Y + 25);

	private static Point AUDIO_MIXER_L_1 = new Point(AUDIO_MIXER.X, AUDIO_MIXER.Y + 20);

	private static Point AUDIO_MIXER_L_2 = new Point(AUDIO_MIXER.X, AUDIO_MIXER.Y + 30);

	private static Point AUDIO_MIXER_B = new Point(AUDIO_MIXER.X + 100, AUDIO_MIXER.Y + 50);

	private static Point AUDIO_MIXER_external_corner = new Point(AUDIO_MIXER.X + 100, CODEC2_offset_y + 25);

	private static Point AUDIO_MIXER_internal_corner1 = new Point(AUDIO_MIXER.X + 100, AUDIO_MIXER_L_1.Y);

	private static Point AUDIO_MIXER_internal_corner2 = new Point(AUDIO_MIXER.X + 100, AUDIO_MIXER_L_2.Y);

	private static Point AUDIO_MIXER_external_corner2 = new Point(AUDIO_MIXER.X + 100, AUDIO_MIXER.Y + 130);

	private static Point BYPASS_corner1 = new Point(SDR.X + 200, SDR.Y + 170);

	private static Point BYPASS_corner2 = new Point(SDR.X + 200, SDR.Y + 45);

	private static Point C1 = new Point(SDR.X, SDR.Y + 20);

	private static Point C1_c = new Point(SDR.X + 20, SDR.Y + 20);

	private static Point C1_label = new Point(C1.X - 50, C1.Y + 5);

	private static Point C2 = new Point(SDR.X, SDR.Y + 45);

	private static Point C2_c = new Point(SDR.X + 20, SDR.Y + 45);

	private static Point C2_corner = new Point(SDR.X + 120, C2.Y);

	private static Point C2_label = new Point(C2.X - 50, C2.Y + 5);

	private static Point C2_label_HPSDR = new Point(C2.X - 73, C2.Y + 5);

	private static Point C3 = new Point(SDR.X, SDR.Y + 70);

	private static Point C3_c = new Point(SDR.X + 20, SDR.Y + 70);

	private static Point C3_corner = new Point(SDR.X + 120, C3.Y);

	private static Point C3_corner2 = new Point(SDR.X + 220, C3.Y);

	private static Point C3_label = new Point(C3.X - 50, C3.Y + 5);

	private static Point C3_corner3 = new Point(SDR.X + 220, ADC0_L.Y);

	private static Point C3_label_HPSDR = new Point(C3.X - 65, C3.Y + 5);

	private static Point C4 = new Point(SDR.X, SDR.Y + 95);

	private static Point C4_c = new Point(SDR.X + 20, SDR.Y + 95);

	private static Point C4_corner = new Point(SDR.X + 120, C4.Y);

	private static Point C4_corner2 = new Point(SDR.X + 220, C4.Y);

	private static Point C4_corner3 = new Point(SDR.X + 220, ADC0_L.Y);

	private static Point C4_label = new Point(C4.X - 50, C4.Y + 5);

	private static Point C4_label_HPSDR = new Point(C4.X - 65, C4.Y + 5);

	private static Point C5 = new Point(SDR.X, SDR.Y + 120);

	private static Point C5_c = new Point(SDR.X + 20, SDR.Y + 120);

	private static Point C5_corner = new Point(LPF.X + 70, C5.Y);

	private static Point C5_riser = new Point(LPF.X + 70, ADC0_L.Y);

	private static Point C5_label = new Point(C5.X - 50, C5.Y + 5);

	private static Point C5_label_HPSDR = new Point(C5.X - 79, C5.Y + 5);

	private static Point C6 = new Point(SDR.X, SDR.Y + 145);

	private static Point C6_c = new Point(SDR.X + 20, SDR.Y + 145);

	private static Point C6_label = new Point(C6.X - 50, C6.Y + 5);

	private static Point C7 = new Point(SDR.X, SDR.Y + 170);

	private static Point C7_c = new Point(SDR.X + 20, SDR.Y + 170);

	private static Point C7_label = new Point(C7.X - 50, C7.Y + 5);

	private static Point C7_label_HPSDR = new Point(C7.X - 65, C7.Y + 5);

	private static Point C8 = new Point(SDR.X, SDR.Y + 195);

	private static Point C8_c = new Point(SDR.X + 20, SDR.Y + 195);

	private static Point C8_label = new Point(C8.X - 50, C8.Y + 5);

	private static Point C8_label_HPSDR = new Point(C8.X - 65, C8.Y + 5);

	private static Point C9 = new Point(SDR.X, SDR.Y + 220);

	private static Point C9_c = new Point(SDR.X + 20, SDR.Y + 220);

	private static Point C9_label = new Point(C9.X - 50, C9.Y + 5);

	private static Point C9_label_HPSDR = new Point(C9.X - 65, C9.Y + 5);

	private static Point C10 = new Point(SDR.X, SDR.Y + 245);

	private static Point C10_c = new Point(SDR.X + 20, SDR.Y + 245);

	private static Point C10_label = new Point(C10.X - 50, C10.Y + 5);

	private static Point C10_label_HPSDR = new Point(C10.X - 78, C10.Y + 5);

	private static Point C10_label_ALEX_TX_IN = new Point(C10.X - 78, C10.Y - 1);

	private static Point C11 = new Point(SDR.X, SDR.Y + 275);

	private static Point C11_c = new Point(SDR.X + 20, SDR.Y + 275);

	private static Point C11_label = new Point(C11.X - 50, C11.Y + 5);

	private static Point C12 = new Point(SDR.X, SDR.Y + 295);

	private static Point C12_c = new Point(SDR.X + 20, SDR.Y + 295);

	private static Point C12_label = new Point(C12.X - 50, C12.Y + 5);

	private static Point C13 = new Point(SDR.X, SDR.Y + 320);

	private static Point C13_c = new Point(SDR.X + 20, SDR.Y + 320);

	private static Point C13_label = new Point(C13.X - 50, C13.Y + 5);

	private static Point C14 = new Point(SDR.X, SDR.Y + 345);

	private static Point C14_c = new Point(SDR.X + 20, SDR.Y + 345);

	private static Point C14_label = new Point(C14.X - 50, C14.Y + 5);

	private static Point C15 = new Point(SDR.X, SDR.Y + 370);

	private static Point C15_c = new Point(SDR.X + 20, SDR.Y + 370);

	private static Point C15_label = new Point(C15.X - 50, C15.Y + 5);

	private static Point C16 = new Point(SDR.X, SDR.Y + 395);

	private static Point C16_c = new Point(SDR.X + 20, SDR.Y + 395);

	private static Point C16_label = new Point(C16.X - 50, C16.Y + 5);

	private static Point C16_ALEX_2_label = new Point(5, 435);

	private static Point C17 = new Point(SDR.X, SDR.Y + 420);

	private static Point C17_c = new Point(SDR.X + 20, SDR.Y + 420);

	private static Point C17_label = new Point(C17.X - 50, C17.Y + 5);

	private static Point C17_ALEX_2_label = new Point(15, 460);

	private static Point C18 = new Point(SDR.X, SDR.Y + 445);

	private static Point C18_c = new Point(SDR.X + 20, SDR.Y + 445);

	private static Point C18_label = new Point(C18.X - 50, C18.Y + 5);

	private static Point C18_ALEX_2_label = new Point(15, 485);

	private static Point C19 = new Point(SDR.X, SDR.Y + 470);

	private static Point C19_c = new Point(SDR.X + 20, SDR.Y + 470);

	private static Point C19_label = new Point(C19.X - 50, C19.Y + 5);

	private static Point C19_ALEX_2_label = new Point(3, 510);

	private static Point C20 = new Point(SDR.X, SDR.Y + 495);

	private static Point C20_c = new Point(SDR.X + 20, SDR.Y + 495);

	private static Point C20_label = new Point(C20.X - 50, C20.Y + 5);

	private static Point C20_ALEX_2_label = new Point(20, 560);

	private static Point C24 = new Point(SDR.X, SDR.Y + 535);

	private static Point C24_c = new Point(SDR.X + 20, CODEC.Y + 25);

	private static Point C24_label = new Point(C24.X - 50, C24.Y + 5);

	private static Point C24_ALEX_2_label = new Point(20, 585);

	private static Point C25 = new Point(SDR.X, SDR.Y + 575);

	private static Point C25_c = new Point(SDR.X + 20, CODEC2.Y + 25);

	private static Point C25_label = new Point(C25.X - 95, C25.Y + 5);

	private static Point C25_ALEX_2_label = new Point(20, 610);

	private static Point C26 = new Point(SDR.X, CODEC.Y + 25);

	private static Point C26_c = new Point(SDR.X + 20, CODEC.Y + 25);

	private static Point C26_label = new Point(C26.X - 50, C26.Y + 5);

	private static Point C26_ALEX_2_label = new Point(5, 630);

	private static Point CODEC_L = new Point(CODEC.X, CODEC.Y + 25);

	private static Point CODEC_L_c = new Point(CODEC.X - 20, CODEC.Y + 25);

	private static Point CODEC_R = new Point(CODEC.X + 50, CODEC.Y + 25);

	private static Point CODEC_R_c = new Point(CODEC.X + 70, CODEC.Y + 25);

	private static Point CODEC2_L = new Point(CODEC2.X, CODEC2.Y + 25);

	private static Point CODEC2_L_c = new Point(CODEC2.X - 20, CODEC2.Y + 25);

	private static Point CODEC2_R = new Point(CODEC2.X + 50, CODEC2.Y + 25);

	private static Point CODEC2_R_c = new Point(CODEC2.X + 70, CODEC2.Y + 25);

	private static Point DAC0_L = new Point(DAC0.X, DAC0.Y + 25);

	private static Point DAC0_L_c = new Point(DAC0.X - 20, DAC0.Y + 25);

	private static Point DAC0_R = new Point(DAC0.X + 50, DAC0.Y + 25);

	private static Point DAC0_R_c = new Point(DAC0.X + 70, DAC0.Y + 25);

	private static Point DSP_B_1 = new Point(DSP.X + 10, DSP.Y + 220);

	private static Point DSP_B_1_c = new Point(DSP.X + 10, DSP.Y + 135);

	private static Point DSP_B_2 = new Point(DSP.X + 40, DSP.Y + 220);

	private static Point DSP_B_2_c = new Point(DSP.X + 40, DSP.Y + 135);

	private static Point DSP_L_1 = new Point(DSP.X, DSP.Y + 20);

	private static Point DSP_L_1_c = new Point(DSP.X - 20, DSP.Y + 20);

	private static Point DSP_R_1 = new Point(DSP.X + 50, DSP.Y + 20);

	private static Point DSP_R_1_c = new Point(DSP.X + 70, DSP.Y + 20);

	private static Point DSP_L_2 = new Point(DSP.X, DSP.Y + 40);

	private static Point DSP_L_2_c = new Point(DSP.X - 20, DSP.Y + 40);

	private static Point DSP_R_2 = new Point(DSP.X + 50, DSP.Y + 40);

	private static Point DSP_R_2_c = new Point(DSP.X + 70, DSP.Y + 40);

	private static Point DSP_L_3 = new Point(DSP.X, DSP.Y + 115);

	private static Point DSP_R_3 = new Point(DSP.X + 70, DSP.Y + 115);

	private static Point DSP_L_4 = new Point(DSP.X, DSP.Y + 190);

	private static Point DSP_R_4 = new Point(DSP.X + 90, DSP.Y + 190);

	private static Point DSP_R_3_RX1_corner = new Point(DSP_R_3.X, DSP.Y - 40);

	private static Point DSP_R_4_RX2_corner = new Point(DSP_R_4.X, DSP.Y + 55);

	private static Point DSP_bottom_corner = new Point(DSP.X + 40, CODEC_R.Y);

	private static Point DSP_loopback_1 = new Point(DSP_B_2.X, DSP.Y + 220);

	private static Point DSP_loopback_2 = new Point(DSP_B_1.X, DSP.Y + 220);

	private static Point DSP_looback_center = new Point(DSP.X + 25, DSP.Y + 270);

	private static Point DSP_L_corner_Rx0 = new Point(DSP_L_1_c.X, DSP.Y - 35);

	private static Point DSP_L_corner2_Rx0 = new Point(DSP_L_1_c.X, DSP_L_1_c.Y);

	private static Point DSP_MIXER_1 = new Point(DSP.X + 50, AUDIO_MIXER_L_1.Y);

	private static Point DSP_MIXER_2 = new Point(DSP.X + 50, AUDIO_MIXER_L_2.Y);

	private static Point DSP_HPSDR_label = new Point(DSP_HPSDR.X + 10, DSP_HPSDR.Y + 5);

	private static Point DSP_internal_MIXER1_1 = new Point(DSP.X + 40, AUDIO_MIXER_L_1.Y);

	private static Point DSP_internal_MIXER1_2 = new Point(DSP.X + 40, DSP_R_1.Y);

	private static Point DSP_internal_MIXER1_3 = new Point(DSP.X + 40, DSP_internal_MIXER1_2.Y - 30);

	private static Point DSP_internal_MIXER2_1 = new Point(DSP.X + 10, AUDIO_MIXER_L_2.Y);

	private static Point DSP_internal_MIXER2_2 = new Point(DSP.X + 10, DSP_R_2.Y);

	private static Point DSP_internal_MIXER2_3 = new Point(DSP.X + 10, DSP_internal_MIXER2_2.Y + 10);

	private static Point DSP_internal_Rx2_audio_connection = new Point(DSP_internal_MIXER1_1.X, DSP_L_3.Y);

	private static Point DSP_internal_Rx3_audio_connection = new Point(DSP_internal_MIXER2_1.X, DSP_L_4.Y);

	private static Point DSP_internal_diversity_corner1 = new Point(DSP_L_2.X + 10, DSP_L_2.Y);

	private static Point DSP_internal_diversity_corner2 = new Point(DSP_L_2.X + 10, DSP_L_1.Y);

	private static Point DUC0_T = new Point(DUC0.X + 25, DUC0.Y);

	private static Point DUC0_T_c = new Point(DUC0.X + 25, DUC0.Y - 20);

	private static Point DUC0_L = new Point(DUC0.X, DUC0.Y + 25);

	private static Point DUC0_L_c = new Point(DUC0.X - 20, DUC0.Y + 25);

	private static Point DUC0_R = new Point(DUC0.X + 50, DUC0.Y + 25);

	private static Point DUC0_R_c = new Point(DUC0.X + 70, DUC0.Y + 25);

	private static Point DUC0_R_corner = new Point(DSP_B_1.X, DUC0_R.Y);

	private static Point DUC0_L_corner = new Point(FPGA.X + 20, Rx1.Y + 25);

	private static Point DUC0_L_corner_lower_pt = new Point(DUC0_L_corner.X, DUC0_L.Y);

	private static Point EXT1_HPF_corner = new Point(HPF_B.X, C5.Y);

	private static Point ext_amp_label1 = new Point(ALEX.X - 40, ALEX.Y + 300);

	private static Point ext_amp_label2 = new Point(ALEX.X - 40, ALEX.Y + 315);

	private static Point EXT2_HPF_corner = new Point(HPF_B.X, C6.Y);

	private static Point HPF_GROUND1 = new Point(SDR.X + 100, SDR.Y + 45);

	private static Point HPF_GROUND2 = new Point(SDR.X + 100, SDR.Y + 195);

	private static Point HPF_GROUND3 = new Point(SDR.X + 85, SDR.Y + 195);

	private static Point HPF_GROUND4 = new Point(SDR.X + 115, SDR.Y + 195);

	private static Point HPF_GROUND5 = new Point(SDR.X + 90, SDR.Y + 200);

	private static Point HPF_GROUND6 = new Point(SDR.X + 110, SDR.Y + 200);

	private static Point HPF_GROUND7 = new Point(SDR.X + 95, SDR.Y + 205);

	private static Point HPF_GROUND8 = new Point(SDR.X + 105, SDR.Y + 205);

	private static Point HPF_GROUND9 = new Point(SDR.X + 100, SDR.Y + 133);

	private static Point HPF_GROUND10 = new Point(SDR.X + 100, C7.Y);

	private static Point GROUND1 = new Point(SDR.X + 100, SDR.Y + 120);

	private static Point GROUND2 = new Point(SDR.X + 100, SDR.Y + 185);

	private static Point GROUND3 = new Point(SDR.X + 85, SDR.Y + 185);

	private static Point GROUND4 = new Point(SDR.X + 115, SDR.Y + 185);

	private static Point GROUND5 = new Point(SDR.X + 90, SDR.Y + 190);

	private static Point GROUND6 = new Point(SDR.X + 110, SDR.Y + 190);

	private static Point GROUND7 = new Point(SDR.X + 95, SDR.Y + 195);

	private static Point GROUND8 = new Point(SDR.X + 105, SDR.Y + 195);

	private static Point HEADPHONES = new Point(C25.X, C25.Y);

	private static Point HEADPHONES_1 = new Point(C25.X + 240, C25.Y);

	private static Point HEADPHONES_2 = new Point(C25.X + 240, AUDIO_AMP_L.Y);

	private static Point HERMES1 = new Point(80, 660);

	private static Point HERMES2 = new Point(80, 370);

	private static Point HERMES3 = new Point(230, 370);

	private static Point HERMES4 = new Point(230, 30);

	private static Point HERMES5 = new Point(740, 30);

	private static Point HERMES6 = new Point(740, 660);

	private static Point HERMES_corner1 = new Point(ALEX_RX_out.X + 150, ALEX_RX_out.Y);

	private static Point HERMES_corner2 = new Point(ALEX_RX_out.X + 150, ADC0_L.Y);

	private static Point HERMES_label = new Point(HERMES4.X + 5, HERMES4.Y + 5);

	private static Point HERMES_corner3 = new Point(PA.X + 25, PA.Y - 50);

	private static Point HERMES_RX_IN_label = new Point(HERMES4.X - 38, HERMES4.Y + 75);

	private static Point HERMES_J5_label = new Point(HERMES4.X - 33, HERMES4.Y + 88);

	private static Point HERMES_TX_OUT_label = new Point(HERMES4.X - 51, HERMES4.Y + 263);

	private static Point HERMES_J3_label = new Point(HERMES4.X - 31, HERMES4.Y + 278);

	private static Point HERMES_XVTR_TX_label = new Point(18, 490);

	private static Point HERMES_J1_label = new Point(45, 502);

	private static Point HERMES_XVTR_TX = new Point(80, 505);

	private static Point HPF_L = new Point(HPF.X, HPF.Y + 25);

	private static Point HPF_L_c = new Point(HPF.X - 20, HPF.Y + 25);

	private static Point HPF_R = new Point(HPF.X + 50, HPF.Y + 25);

	private static Point HPF_R_c = new Point(HPF.X + 70, HPF.Y + 25);

	private static Point HPF_label = new Point(HPF.X + 10, HPF.Y + 17);

	private static Point HPF_B = new Point(HPF.X + 25, HPF.Y + 50);

	private static Point HPF_center = new Point(HPF.X + 25, ADC0.Y + 25);

	private static Point HPF_TX_corner1 = new Point(SDR.X + 100, SDR.Y + 95);

	private static Point HPF_TX_corner2 = new Point(SDR.X + 100, SDR.Y + 45);

	private static Point HPF_TX_corner3 = new Point(SDR.X + 100, SDR.Y + 120);

	private static Point HPF_TX_corner4 = new Point(SDR.X + 100, SDR.Y + 145);

	private static Point HPF_TX_corner5 = new Point(SDR.X + 50, SDR.Y + 45);

	private static Point HPF_TX_corner6 = new Point(SDR.X + 50, SDR.Y + 145);

	private static Point HPF2_L = new Point(HPF2.X, HPF2.Y + 25);

	private static Point HPF2_L_c = new Point(HPF2.X - 20, HPF2.Y + 25);

	private static Point HPF2_R = new Point(HPF2.X + 50, HPF2.Y + 25);

	private static Point HPF2_R_c = new Point(HPF2.X + 70, HPF2.Y + 25);

	private static Point loopback_center = new Point(DSP.X + 25, DSP.Y + 220);

	private static Point loopback_center2 = new Point(DSP.X + 25, DSP_R_1.Y);

	private static Point LPF_L = new Point(LPF.X, LPF.Y + 25);

	private static Point LPF_L_c = new Point(LPF.X - 20, LPF.Y + 25);

	private static Point LPF_R = new Point(LPF.X + 50, LPF.Y + 25);

	private static Point LPF_R_c = new Point(LPF.X + 70, LPF.Y + 25);

	private static Point LPF_B = new Point(LPF.X + 25, LPF.Y + 50);

	private static Point LPF_T = new Point(LPF.X + 25, LPF.Y);

	private static Point LPF_in_corner = new Point(SDR.X + 120, LPF_L.Y);

	private static Point LPF_R_corner = new Point(PA_T.X, LPF_R.Y);

	private static Point LPF_corner_C2 = new Point(LPF.X + 25, C2.Y);

	private static Point LPF_corner_C3 = new Point(LPF.X + 25, C3.Y);

	private static Point LPF_corner_C4 = new Point(LPF.X + 25, C4.Y);

	private static Point LPF_label = new Point(LPF.X + 10, LPF.Y + 17);

	private static Point LPF_BYPASS_corner = new Point(LPF.X + 25, C7.Y);

	private static Point LPF_HPF_corner = new Point(HPF.X + 25, LPF.Y + 25);

	private static Point LPF2_L = new Point(LPF2.X, LPF2.Y + 25);

	private static Point LPF2_L_c = new Point(LPF2.X - 20, LPF2.Y + 25);

	private static Point LPF2_R = new Point(LPF2.X + 50, LPF2.Y + 25);

	private static Point LPF2_R_c = new Point(LPF2.X + 70, LPF2.Y + 25);

	private static Point MERCURY_label = new Point(MERCURY.X + 5, MERCURY.Y + 5);

	private static Point MERCURY_ADC_label = new Point(MERCURY_ADC.X + 10, MERCURY_ADC.Y + 17);

	private static Point MERCURY_ADC_in = new Point(MERCURY_ADC.X, MERCURY_ADC.Y + 25);

	private static Point MERCURY_ADC_out = new Point(MERCURY_ADC.X + 50, MERCURY_ADC.Y + 25);

	private static Point MERCURY_DDC0_label = new Point(MERCURY_DDC0.X + 5, MERCURY_DDC0.Y + 10);

	private static Point MERCURY_DDC0_Rx0_label = new Point(MERCURY_DDC0.X + 5, MERCURY_DDC0.Y + 27);

	private static Point MERCURY_DDC0_in = new Point(MERCURY_DDC0.X, MERCURY_DDC0.Y + 25);

	private static Point MERCURY_DDC0_out = new Point(MERCURY_DDC0.X + 50, MERCURY_DDC0.Y + 25);

	private static Point MERCURY_DDC1_label = new Point(MERCURY_DDC1.X + 5, MERCURY_DDC1.Y + 10);

	private static Point MERCURY_DDC1_in = new Point(MERCURY_DDC1.X, MERCURY_DDC1.Y + 25);

	private static Point MERCURY_DDC1_Rx1_label = new Point(MERCURY_DDC1.X + 5, MERCURY_DDC1.Y + 27);

	private static Point MERCURY_DDC1_out = new Point(MERCURY_DDC1.X + 50, MERCURY_DDC1.Y + 25);

	private static Point MERCURY_DDC2_label = new Point(MERCURY_DDC2.X + 5, MERCURY_DDC2.Y + 10);

	private static Point MERCURY_DDC2_in = new Point(MERCURY_DDC2.X, MERCURY_DDC2.Y + 25);

	private static Point MERCURY_DDC2_Rx2_label = new Point(MERCURY_DDC2.X + 5, MERCURY_DDC2.Y + 27);

	private static Point MERCURY_CODEC_label = new Point(MERCURY_CODEC.X + 3, MERCURY_CODEC.Y + 17);

	private static Point MERCURY_CODEC_LINEOUT = new Point(MERCURY.X, MERCURY_CODEC.Y + 10);

	private static Point MERCURY_CODEC_OUT1 = new Point(MERCURY_CODEC.X, MERCURY_CODEC.Y + 10);

	private static Point MERCURY_CODEC_PHONES = new Point(MERCURY.X, MERCURY_CODEC.Y + 40);

	private static Point MERCURY_CODEC_OUT2 = new Point(MERCURY_CODEC.X, MERCURY_CODEC.Y + 40);

	private static Point MERCURY_CODEC_IN = new Point(MERCURY_CODEC.X + 50, MERCURY_CODEC.Y + 40);

	private static Point MERCURY_CODEC_corner1 = new Point(PC.X + 15, MERCURY_CODEC.Y + 40);

	private static Point MERCURY_CODEC_corner2 = new Point(PC.X + 15, MERCURY_CODEC.Y + 150);

	private static Point MERCURY_FPGA_label = new Point(MERCURY_FPGA.X + 5, MERCURY_FPGA.Y + 3);

	private static Point MERCURY_FPGA_corner1 = new Point(MERCURY_FPGA.X + 5, MERCURY_DDC0_in.Y);

	private static Point MERCURY_FPGA_corner2 = new Point(MERCURY_FPGA.X + 5, MERCURY_DDC1_in.Y);

	private static Point MERCURY_FPGA_corner3 = new Point(MERCURY_FPGA.X + 5, MERCURY_DDC2_in.Y);

	private static Point MERCURY_PHONES_label = new Point(MERCURY.X - 59, MERCURY.Y + 210);

	private static Point MERCURY_P_OUT_label = new Point(MERCURY.X - 35, MERCURY.Y + 222);

	private static Point MERCURY_LINE_label = new Point(MERCURY.X - 35, MERCURY.Y + 178);

	private static Point MERCURY_OUT_label = new Point(MERCURY.X - 35, MERCURY.Y + 190);

	private static Point MERCURY_ANT_label = new Point(MERCURY.X - 35, MERCURY.Y + 45);

	private static Point MERCURY_2_label = new Point(MERCURY_2.X + 5, MERCURY_2.Y + 2);

	private static Point MERCURY_2_ADC_label = new Point(MERCURY_2_ADC.X + 10, MERCURY_2_ADC.Y + 17);

	private static Point MERCURY_2_ADC_in = new Point(MERCURY_2_ADC.X, MERCURY_2_ADC.Y + 25);

	private static Point MERCURY_2_ADC_in_corner1 = new Point(MERCURY_2_ADC.X - 70, MERCURY_2_ADC.Y + 25);

	private static Point MERCURY_2_ADC_out = new Point(MERCURY_2_ADC.X + 50, MERCURY_2_ADC.Y + 25);

	private static Point MERCURY_2_DDC0_label = new Point(MERCURY_2_DDC0.X + 10, MERCURY_2_DDC0.Y + 10);

	private static Point MERCURY_2_DDC0_Rx0_label = new Point(MERCURY_2_DDC0.X + 5, MERCURY_2_DDC0.Y + 27);

	private static Point MERCURY_2_DDC0_in = new Point(MERCURY_2_DDC0.X, MERCURY_2_DDC0.Y + 25);

	private static Point MERCURY_2_DDC0_out = new Point(MERCURY_2_DDC0.X + 50, MERCURY_2_DDC0.Y + 25);

	private static Point MERCURY_2_DDC0_out_corner1 = new Point(MERCURY_2_DDC0.X + 210, MERCURY_2_DDC0.Y + 25);

	private static Point MERCURY_2_DDC0_out_corner2 = new Point(MERCURY_2_DDC0.X + 210, DSP_internal_MIXER2_3.Y);

	private static Point MERCURY_2_DDC1_label = new Point(MERCURY_2_DDC1.X + 10, MERCURY_2_DDC1.Y + 10);

	private static Point MERCURY_2_DDC1_Rx1_label = new Point(MERCURY_2_DDC1.X + 5, MERCURY_2_DDC1.Y + 27);

	private static Point MERCURY_2_DDC1_in = new Point(MERCURY_2_DDC1.X, MERCURY_2_DDC1.Y + 25);

	private static Point MERCURY_2_DDC2_label = new Point(MERCURY_2_DDC2.X + 10, MERCURY_2_DDC2.Y + 10);

	private static Point MERCURY_2_DDC2_in = new Point(MERCURY_2_DDC2.X, MERCURY_2_DDC2.Y + 25);

	private static Point MERCURY_2_DDC2_Rx2_label = new Point(MERCURY_2_DDC2.X + 5, MERCURY_2_DDC2.Y + 27);

	private static Point MERCURY_2_CODEC_label = new Point(MERCURY_2_CODEC.X + 3, MERCURY_2_CODEC.Y + 17);

	private static Point MERCURY_2_CODEC_LINEOUT = new Point(MERCURY_2.X, MERCURY_2_CODEC.Y + 10);

	private static Point MERCURY_2_CODEC_OUT1 = new Point(MERCURY_CODEC.X, MERCURY_2_CODEC.Y + 10);

	private static Point MERCURY_2_CODEC_PHONES = new Point(MERCURY_2.X, MERCURY_2_CODEC.Y + 40);

	private static Point MERCURY_2_CODEC_OUT2 = new Point(MERCURY_2_CODEC.X, MERCURY_2_CODEC.Y + 40);

	private static Point MERCURY_2_CODEC_out = new Point(MERCURY_2_CODEC.X + 50, MERCURY_2_CODEC.Y + 38);

	private static Point MERCURY_2_CODEC_corner1 = new Point(MERCURY_2_CODEC.X + 310, MERCURY_2_CODEC.Y + 38);

	private static Point MERCURY_2_CODEC_corner2 = new Point(MERCURY_2_CODEC.X + 310, MERCURY_CODEC_IN.Y);

	private static Point MERCURY_2_FPGA_label = new Point(MERCURY_2_FPGA.X + 5, MERCURY_2_FPGA.Y + 3);

	private static Point MERCURY_2_FPGA_corner1 = new Point(MERCURY_2_FPGA.X + 5, MERCURY_2_DDC0_in.Y);

	private static Point MERCURY_2_FPGA_corner2 = new Point(MERCURY_2_FPGA.X + 5, MERCURY_2_DDC1_in.Y);

	private static Point MERCURY_2_FPGA_corner3 = new Point(MERCURY_2_FPGA.X + 5, MERCURY_2_DDC2_in.Y);

	private static Point MERCURY_2_PHONES_label = new Point(MERCURY_2.X - 59, MERCURY_2.Y + 210);

	private static Point MERCURY_2_P_OUT_label = new Point(MERCURY_2.X - 35, MERCURY_2.Y + 222);

	private static Point MERCURY_2_LINE_label = new Point(MERCURY_2.X - 35, MERCURY_2.Y + 178);

	private static Point MERCURY_2_OUT_label = new Point(MERCURY_2.X - 35, MERCURY_2.Y + 190);

	private static Point MERCURY_2_ANT_label = new Point(MERCURY_2.X - 35, MERCURY_2.Y + 45);

	private static Point METIS_label = new Point(METIS.X + 5, METIS.Y + 5);

	private static Point METIS_FPGA_label = new Point(METIS_FPGA.X + 5, METIS_FPGA.Y + 3);

	private static Point METIS_corner1 = new Point(METIS.X + 40, MERCURY_2_DDC0_out_corner2.Y);

	private static Point METIS_corner2 = new Point(METIS.X + 40, METIS.Y + 70);

	private static Point MIC_R = new Point(SDR.X, CODEC.Y + 25);

	private static Point MIC_R_c = new Point(SDR.X + 20, CODEC.Y + 25);

	private static Point PA_B = new Point(PA.X + 25, PA.Y + 50);

	private static Point PA_T = new Point(PA.X + 25, PA.Y);

	private static Point PA_corner = new Point(PA.X + 25, AMPF_L.Y);

	private static Point PA15_corner = new Point(PA_T.X, LPF_R.Y);

	private static Point PA15_corner2 = new Point(PA_B.X, AMPF_L.Y);

	private static Point PA15_corner3 = new Point(PA_T.X, LPF_R.Y);

	private static Point PC_PENELOPE_corner1 = new Point(PC.X + 40, PC.Y + 305);

	private static Point PC_PENELOPE_corner2 = new Point(PC.X + 40, PC.Y + 275);

	private static Point PC_PENELOPE_corner3 = new Point(PC.X + 70, PC.Y + 275);

	private static Point PC_PENELOPE_corner4 = new Point(PC.X + 70, PC.Y + 361);

	private static Point PENELOPE_label = new Point(PENELOPE.X + 5, PENELOPE.Y + 5);

	private static Point PENELOPE_FPGA_label = new Point(PENELOPE_FPGA.X + 5, PENELOPE_FPGA.Y + 3);

	private static Point PENELOPE_DAC_label = new Point(PENELOPE_DAC.X + 10, PENELOPE_DAC.Y + 17);

	private static Point PENELOPE_DUC_label = new Point(PENELOPE_DUC.X + 10, PENELOPE_DUC.Y + 17);

	private static Point PENELOPE_AMPF_label = new Point(PENELOPE_AMPF.X + 5, PENELOPE_AMPF.Y + 10);

	private static Point PENELOPE_FILTER_label = new Point(PENELOPE_AMPF.X + 5, PENELOPE_AMPF.Y + 27);

	private static Point PENELOPE_PA_label = new Point(PENELOPE_PA.X + 15, PENELOPE_PA.Y + 17);

	private static Point PENELOPE_CODEC_label = new Point(PENELOPE_CODEC.X + 3, PENELOPE_CODEC.Y + 17);

	private static Point PENELOPE_CODEC_LINE_IN_1 = new Point(PENELOPE_CODEC.X, PENELOPE_CODEC.Y + 15);

	private static Point PENELOPE_CODEC_LINE_IN_2 = new Point(PENELOPE.X, PENELOPE_CODEC.Y + 15);

	private static Point PENELOPE_CODEC_MIC_1 = new Point(PENELOPE_CODEC.X, PENELOPE_CODEC.Y + 40);

	private static Point PENELOPE_CODEC_MIC_2 = new Point(PENELOPE.X, PENELOPE_CODEC.Y + 40);

	private static Point PENELOPE_CODEC_R = new Point(PENELOPE_CODEC.X + 50, PENELOPE_CODEC.Y + 25);

	private static Point PENELOPE_CODEC_L = new Point(PENELOPE_CODEC.X, PENELOPE_CODEC.Y + 25);

	private static Point PENELOPE_ANT_label = new Point(PENELOPE.X - 33, PENELOPE.Y + 28);

	private static Point PENELOPE_LINE_label = new Point(PENELOPE.X - 35, PENELOPE.Y + 90);

	private static Point PENELOPE_IN_label = new Point(PENELOPE.X - 23, PENELOPE.Y + 102);

	private static Point PENELOPE_MIC_label = new Point(PENELOPE.X - 30, PENELOPE.Y + 120);

	private static Point PENELOPE_XVTR_label = new Point(PENELOPE.X - 40, PENELOPE.Y + 70);

	private static Point PENELOPE_PA_out = new Point(PENELOPE_PA.X, PENELOPE_PA.Y + 25);

	private static Point PENELOPE_PA_out_corner1 = new Point(PENELOPE.X - 230, PENELOPE_PA.Y + 25);

	private static Point PENELOPE_PA_out_corner2 = new Point(PENELOPE.X - 230, PENELOPE_PA.Y - 10);

	private static Point PENELOPE_PA_out_corner3 = new Point(ALEX_LPF.X + 40, PENELOPE_PA.Y - 10);

	private static Point PENELOPE_PA_R = new Point(PENELOPE_PA.X + 50, PENELOPE_PA.Y + 25);

	private static Point PENELOPE_PA_XVTR_1 = new Point(PENELOPE_PA.X + 60, PENELOPE_PA.Y + 25);

	private static Point PENELOPE_PA_XVTR_2 = new Point(PENELOPE_PA.X + 60, PENELOPE_PA.Y + 56);

	private static Point PENELOPE_PA_XVTR_3 = new Point(PENELOPE.X, PENELOPE_PA.Y + 56);

	private static Point PENELOPE_AMPF_L = new Point(PENELOPE_AMPF.X, PENELOPE_AMPF.Y + 25);

	private static Point PENELOPE_AMPF_R = new Point(PENELOPE_AMPF.X + 50, PENELOPE_AMPF.Y + 25);

	private static Point PENELOPE_DAC_L = new Point(PENELOPE_DAC.X, PENELOPE_DAC.Y + 25);

	private static Point PENELOPE_DAC_R = new Point(PENELOPE_DAC.X + 50, PENELOPE_DUC.Y + 25);

	private static Point PENELOPE_DUC_L = new Point(PENELOPE_DUC.X, PENELOPE_DUC.Y + 25);

	private static Point PENELOPE_DUC_R = new Point(PENELOPE_DUC.X + 50, PENELOPE_DUC.Y + 25);

	private static Point PENELOPE_DSP_mid_1 = new Point(PC.X + 55, PENELOPE_DUC_R.Y - 30);

	private static Point PENELOPE_DSP_mid_2 = new Point(PC.X + 55, MERCURY_DDC0_out.Y);

	private static Point HERMES_PA_corner1 = new Point(PA_T.X, PENELOPE_PA_out_corner1.Y);

	private static Point RX1_DISPLAY_L = new Point(RX1_DISPLAY.X, RX1_DISPLAY.Y + 25);

	private static Point RX1_DISPLAY_L_c = new Point(RX1_DISPLAY.X - 20, RX1_DISPLAY.Y + 25);

	private static Point RX1_DISPLAY_corner1 = new Point(DSP_R_1_c.X, RX1_DISPLAY_L.Y);

	private static Point RX1_DISPLAY_HPSDR_DDC0 = new Point(RX1_DISPLAY.X, MERCURY_DDC0_out.Y);

	private static Point RX2_DISPLAY_L = new Point(RX2_DISPLAY.X, RX2_DISPLAY.Y + 25);

	private static Point RX2_DISPLAY_L_c = new Point(RX2_DISPLAY.X - 40, RX2_DISPLAY.Y + 25);

	private static Point RX2_L_DSP_2 = new Point(RX2_DISPLAY.X, DSP_R_2.Y);

	private static Point RX2_DISPLAY_HPSDR_DDC1 = new Point(RX2_DISPLAY.X, MERCURY_DDC1_out.Y);

	private static Point Rx0_L = new Point(Rx0.X, Rx0.Y + 25);

	private static Point Rx0_L_c = new Point(Rx0.X - 20, Rx0.Y + 25);

	private static Point Rx0_R = new Point(Rx0.X + 50, Rx0.Y + 25);

	private static Point Rx0_R_c = new Point(Rx0.X + 70, Rx0.Y + 25);

	private static Point Rx0_ADC1_input = new Point(Rx0.X, Rx0.Y + 35);

	private static Point Rx1_L = new Point(Rx1.X, Rx1.Y + 25);

	private static Point Rx1_L_c = new Point(Rx1.X - 20, Rx1.Y + 25);

	private static Point Rx1_R = new Point(Rx1.X + 50, Rx1.Y + 25);

	private static Point Rx1_R_c = new Point(Rx1.X + 70, Rx1.Y + 25);

	private static Point Rx2_L = new Point(Rx2.X, Rx2.Y + 25);

	private static Point Rx2_L_c = new Point(Rx2.X - 20, Rx2.Y + 25);

	private static Point Rx2_R = new Point(Rx2.X + 50, Rx2.Y + 25);

	private static Point Rx2_R_c = new Point(Rx2.X + 70, Rx2.Y + 25);

	private static Point Rx3_L = new Point(Rx3.X, Rx3.Y + 25);

	private static Point Rx3_L_c = new Point(Rx3.X - 20, Rx3.Y + 25);

	private static Point Rx3_R = new Point(Rx3.X + 50, Rx3.Y + 25);

	private static Point Rx3_R_c = new Point(Rx3.X + 70, Rx3.Y + 25);

	private static Point Rx4_L = new Point(Rx4.X, Rx4.Y + 25);

	private static Point Rx4_L_c = new Point(Rx4.X - 20, Rx4.Y + 25);

	private static Point Rx4_R = new Point(Rx4.X + 50, Rx4.Y + 25);

	private static Point Rx4_R_c = new Point(Rx4.X + 70, Rx4.Y + 25);

	private static Point Rx5_L = new Point(Rx5.X, Rx5.Y + 25);

	private static Point Rx5_L_c = new Point(Rx5.X - 20, Rx5.Y + 25);

	private static Point Rx5_R = new Point(Rx5.X + 50, Rx5.Y + 25);

	private static Point Rx5_R_c = new Point(Rx5.X + 70, Rx5.Y + 25);

	private static Point Rx6_L = new Point(Rx6.X, Rx6.Y + 25);

	private static Point Rx6_L_c = new Point(Rx6.X - 20, Rx6.Y + 25);

	private static Point Rx6_R = new Point(Rx6.X + 50, Rx6.Y + 25);

	private static Point Rx6_R_c = new Point(Rx6.X + 70, Rx6.Y + 25);

	private static Point TX_AMP_corner = new Point(C6.X + 20, AMPF_L.Y);

	private static Point TX_corner_PA10 = new Point(C6.X + 20, C6.Y);

	private static Point TX_corner_PA15 = new Point(C13.X + 20, C13.Y);

	private static Point SPKR_corner1 = new Point(RX1_DISPLAY_L_c.X, C24.Y);

	private static Point SPKR_corner2 = new Point(RX2_DISPLAY_L_c.X, C24.Y);

	private static Point SPKR_DSP1 = new Point(SPKR_corner1.X, DSP_L_1.Y);

	private static Point SPKR_DSP2 = new Point(SPKR_corner1.X, DSP_L_2.Y);

	private static Point SWR_L = new Point(SWR.X, SWR.Y + 25);

	private static Point SWR_R = new Point(SWR.X + 50, SWR.Y + 25);

	private static Point SWR_T = new Point(SWR.X + 25, SWR.Y);

	private static Point SWR_corner_ADC0 = new Point(SDR.X + 75, ADC0_L.Y);

	private static Point XVTR_HPF_corner = new Point(HPF_B.X, C4.Y);

	private IContainer components;

	private Panel panel1;

	private Panel canvas;

	private Label label_XVTR_VHF;

	private Label label_DUAL_MERCURY;

	private CheckBox cb_DUAL_MERCURY_ALEX;

	private Label label_UNCHECK;

	private Label label_NOTE;

	private GroupBox groupBox10;

	private RadioButton rb_tx;

	private RadioButton rb_rx;

	private Label label_HERMES_J1;

	private Label label_HERMES_J3;

	private Label label_HERMES_XVTR_TX;

	private Label label_HERMES_TX_OUT;

	private Label label_HERMES_J5;

	private Label label_HERMES_RX_IN;

	private Label label_HERMES;

	private Label label_ext_amp2;

	private Label label_ext_amp1;

	private Label label_PENELOPE_MIC;

	private Label label_DSP_HPSDR;

	private Label label_PENELOPE_XVTR;

	private Label label_PENELOPE_IN;

	private Label label_PENELOPE_LINE;

	private Label label_PENELOPE_ANT;

	private Label label_MERCURY_2_ANT;

	private Label label_MERCURY_ANT;

	private Label label_MERCURY_2_OUT;

	private Label label_MERCURY_2_LINE;

	private Label label_MERCURY_OUT;

	private Label label_MERCURY_LINE;

	private Label label_MERCURY_2_P_OUT;

	private Label label_MERCURY_2_PHONES;

	private Label label_MERCURY_P_OUT;

	private Label label_MERCURY_PHONES;

	private Label label_MERCURY_2_CODEC;

	private Label label_MERCURY_2_Rx2;

	private Label label_MERCURY_2_DDC2;

	private Label label_MERCURY_2_Rx1;

	private Label label_MERCURY_2_DDC1;

	private Label label_MERCURY_2_Rx0;

	private Label label_MERCURY_2_DDC0;

	private Label label_MERCURY_2_ADC;

	private Label label12;

	private Label label13;

	private Label label14;

	private Label label_MERCURY_CODEC;

	private Label label_MERCURY_Rx2;

	private Label label_MERCURY_DDC2;

	private Label label_MERCURY_Rx1;

	private Label label_MERCURY_DDC1;

	private Label label_MERCURY_Rx0;

	private Label label_MERCURY_DDC0;

	private Label label_MERCURY_ADC;

	private Label label_PENELOPE_CODEC;

	private Label label_PENELOPE_FILTER;

	private Label label_PENELOPE_AMPF;

	private Label label_PENELOPE_PA;

	private Label label_PENELOPE_DUC;

	private Label label_PENELOPE_DAC;

	private Label label_METIS_FPGA;

	private Label label_METIS;

	private Label label_ALEX_2_To_RX;

	private Label label_ALEX_2_LPF;

	private Label label_ALEX_2_HPF;

	private Label label_ALEX_2;

	private Label label_PENELOPE_FPGA;

	private Label label_PENELOPE;

	private Label label_MERCURY_2_FPGA;

	private Label label_MERCURY_2;

	private Label label_ALEX_To_RX;

	private Label label_MERCURY_FPGA;

	private Label label_MERCURY;

	private Label label_ALEX_LPF;

	private Label label_ALEX_HPF;

	private Label label_ALEX;

	private Label label_DDC6;

	private Label label_DDC5;

	private Label label_DDC4;

	private Label label_DDC3;

	private Label label_DDC2;

	private Label label_DDC1;

	private Label label_DDC0;

	private Label label_SWR;

	private Label label_RX2_LR_audio;

	private Label label_RX1_LR_audio;

	private Label label_AUDIO_MIXER;

	private Label label_AUDIO_AMP;

	private Label label_LR_audio;

	private Label label_L_audio_only;

	private Label label_C25;

	private Label label_CODEC2;

	private Label label_C26;

	private Label label_SMA;

	private Label label_front_panel;

	private Label label_rear_panel;

	private Label label4;

	private Label label3;

	private Label label2;

	private Label label_CODEC;

	private Label label_C24;

	private Label label_LPF2;

	private Label label_HPF2;

	private Label label_C20;

	private Label label_C19;

	private Label label_C18;

	private Label label_C17;

	private Label label_DUC0;

	private Label label_FILTER;

	private Label label_AMP;

	private Label label_Rx4;

	private Label label_DSP;

	private Label label_PC;

	private Label label_FPGA;

	private Label label_SDR_Hardware;

	private Label label_hardware_selected;

	private Label label_ADC2_atten;

	private Label label_ADC1_atten;

	private Label label_ADC0_atten;

	private Label label_C16;

	private Label label_C15;

	private Label label_C14;

	private Label label_C13;

	private Label label_C12;

	private Label label_C3;

	private Label label_C2;

	private Label label_C1;

	private Label label_HPF;

	private Label label_C4;

	private Label label_C11;

	private Label label_C10;

	private Label label_C9;

	private Label label_C8;

	private Label label_PA;

	private Label label_DAC0;

	private Label label_LPF;

	private Label label_RX2_DISPLAY;

	private Label label_RX1_DISPLAY;

	private Label label_Rx6;

	private Label label_Rx5;

	private Label label_Rx3;

	private Label label_Rx2;

	private Label label_Rx1;

	private Label label_Rx0;

	private Label label_ADC2;

	private Label label_ADC1;

	private Label label_ADC0;

	private Label label_C7;

	private Label label_C6;

	private Label label_C5;

	public Path_Illustrator(Console c)
	{
		console = c;
		base.Resize += PI_Resize;
		base.Disposed += PI_Disposed;
		InitializeComponent();
		g = canvas.CreateGraphics();
		redPen.Width = 3f;
		bluePen.Width = 3f;
		blackPen2.Width = 2f;
		label_LPF.Location = new Point(LPF.X + 12, LPF.Y + 15);
		label_LPF2.Location = new Point(LPF2.X + 12, LPF2.Y + 15);
		label_HPF.Location = new Point(HPF.X + 12, HPF.Y + 15);
		label_HPF2.Location = new Point(HPF2.X + 12, HPF2.Y + 15);
		label_ADC0.Location = new Point(ADC0.X + 5, ADC0.Y + 25);
		label_ADC0_atten.Location = new Point(ADC0.X + 1, ADC0.Y + 7);
		label_ADC1.Location = new Point(ADC1.X + 5, ADC1.Y + 25);
		label_ADC1_atten.Location = new Point(ADC1.X + 1, ADC1.Y + 7);
		label_ADC2.Location = new Point(ADC2.X + 5, ADC2.Y + 25);
		label_ADC2_atten.Location = new Point(ADC2.X + 1, ADC2.Y + 7);
		label_AUDIO_MIXER.Location = new Point(AUDIO_MIXER.X + 110, AUDIO_MIXER.Y + 18);
		label_RX1_LR_audio.Location = new Point(AUDIO_MIXER.X + 2, AUDIO_MIXER.Y + 2);
		label_RX2_LR_audio.Location = new Point(AUDIO_MIXER.X + 2, AUDIO_MIXER.Y + 33);
		label_Rx0.Location = new Point(Rx0.X + 6, Rx0.Y + 28);
		label_DDC0.Location = new Point(Rx0.X + 8, Rx0.Y + 11);
		label_Rx1.Location = new Point(Rx1.X + 6, Rx1.Y + 28);
		label_DDC1.Location = new Point(Rx1.X + 8, Rx1.Y + 11);
		label_Rx2.Location = new Point(Rx2.X + 6, Rx2.Y + 28);
		label_DDC2.Location = new Point(Rx2.X + 8, Rx2.Y + 11);
		label_Rx3.Location = new Point(Rx3.X + 6, Rx3.Y + 28);
		label_DDC3.Location = new Point(Rx3.X + 8, Rx3.Y + 11);
		label_Rx4.Location = new Point(Rx4.X + 6, Rx4.Y + 28);
		label_DDC4.Location = new Point(Rx4.X + 8, Rx4.Y + 11);
		label_Rx5.Location = new Point(Rx5.X + 6, Rx5.Y + 28);
		label_DDC5.Location = new Point(Rx5.X + 8, Rx5.Y + 11);
		label_Rx6.Location = new Point(Rx6.X + 6, Rx6.Y + 28);
		label_DDC6.Location = new Point(Rx6.X + 8, Rx6.Y + 11);
		label_RX1_DISPLAY.Location = new Point(RX1_DISPLAY_x + 70, RX1_DISPLAY_y + 35);
		label_RX2_DISPLAY.Location = new Point(RX2_DISPLAY_x + 70, RX2_DISPLAY_y + 35);
		label_DAC0.Location = new Point(DAC0.X + 9, DAC0.Y + 18);
		label_DUC0.Location = new Point(DUC0.X + 9, DUC0.Y + 18);
		label_PA.Location = new Point(PA.X + 15, PA.Y + 17);
		label_AMP.Location = new Point(AMPF.X + 5, AMPF.Y + 7);
		label_FILTER.Location = new Point(AMPF.X + 5, AMPF.Y + 22);
		label_SDR_Hardware.Location = new Point(SDR.X + 5, SDR.Y + 5);
		label_CODEC.Location = new Point(CODEC.X + 3, CODEC.Y + 18);
		label_CODEC2.Location = new Point(CODEC2.X + 3, CODEC2.Y + 18);
		label_FPGA.Location = new Point(FPGA.X + 5, FPGA.Y + 5);
		label_FPGA.ForeColor = Color.IndianRed;
		label_PC.Location = new Point(PC.X + 5, PC.Y + 5);
		label_DSP.Location = new Point(DSP.X + 10, DSP.Y + 1);
		label_front_panel.Location = new Point(9, 580);
		do_platform_prep();
		update_diagram = true;
		canvas.Invalidate();
	}

	private void canvas_Paint(object sender, PaintEventArgs e)
	{
		Update_control_settings();
		if (bool_HPSDR & update_diagram)
		{
			draw_HPSDR();
		}
		if (bool_HERMES & update_diagram)
		{
			draw_HERMES();
		}
		if (bool_ANAN_10E & update_diagram)
		{
			draw_ANAN_10E();
		}
		if (bool_ANAN_100_PA_rev15 & update_diagram)
		{
			draw_ANAN_100_PA_rev15();
		}
		if (bool_ANAN_100_PA_rev24 & update_diagram)
		{
			draw_ANAN_100_PA_rev24();
		}
		if (bool_ANAN_100D_PA_rev15 & update_diagram)
		{
			draw_ANAN_100D_PA_rev15();
		}
		if (bool_ANAN_100D_PA_rev24 & update_diagram)
		{
			draw_ANAN_100D_PA_rev24();
		}
	}

	private void Update_control_settings()
	{
		bool_HPSDR = HardwareSpecific.Model == HPSDRModel.HPSDR;
		bool_HERMES = HardwareSpecific.Model == HPSDRModel.HERMES;
		bool_disable_BYPASS = console.SetupForm.ChkDisableRXOut;
		bool_ANAN_10E = HardwareSpecific.Model == HPSDRModel.ANAN10 || HardwareSpecific.Model == HPSDRModel.ANAN10E;
		bool_ANAN_100_PA_rev15 = (HardwareSpecific.Model == HPSDRModel.ANAN100 || HardwareSpecific.Model == HPSDRModel.ANAN100B) & !bool_disable_BYPASS;
		bool_ANAN_100_PA_rev24 = (HardwareSpecific.Model == HPSDRModel.ANAN100 || HardwareSpecific.Model == HPSDRModel.ANAN100B) & bool_disable_BYPASS;
		bool_ANAN_100D_PA_rev15 = (HardwareSpecific.Model == HPSDRModel.ANAN100D || HardwareSpecific.Model == HPSDRModel.ANAN200D) & !bool_disable_BYPASS;
		bool_ANAN_100D_PA_rev24 = (HardwareSpecific.Model == HPSDRModel.ANAN100D || HardwareSpecific.Model == HPSDRModel.ANAN200D) & bool_disable_BYPASS;
		bool_rx = rb_rx.Checked;
		int_RxAnt_switch = console.SetupForm.PI_RxAnt;
		int_TxAnt_switch = console.SetupForm.PI_TxAnt;
		if (bool_rx)
		{
			switch (int_RxAnt_switch)
			{
			case 0:
			case 1:
				bool_ANT1 = true;
				bool_ANT2 = false;
				bool_ANT3 = false;
				break;
			case 2:
				bool_ANT1 = false;
				bool_ANT2 = true;
				bool_ANT3 = false;
				break;
			case 3:
				bool_ANT1 = false;
				bool_ANT2 = false;
				bool_ANT3 = true;
				break;
			default:
				bool_ANT1 = true;
				bool_ANT2 = false;
				bool_ANT3 = false;
				break;
			}
		}
		else
		{
			switch (int_TxAnt_switch)
			{
			case 0:
			case 1:
				bool_ANT1_TX = true;
				bool_ANT2_TX = false;
				bool_ANT3_TX = false;
				break;
			case 2:
				bool_ANT1_TX = false;
				bool_ANT2_TX = true;
				bool_ANT3_TX = false;
				break;
			case 3:
				bool_ANT1_TX = false;
				bool_ANT2_TX = false;
				bool_ANT3_TX = true;
				break;
			default:
				bool_ANT1_TX = true;
				bool_ANT2_TX = false;
				bool_ANT3_TX = false;
				break;
			}
		}
		bool_MON = console.MON;
		bool_RX1_MUTE = console.MUT;
		bool_RX2_MUTE = console.MUT2;
		bool_duplex = Display.DisplayDuplex;
		bool_PureSignal = console.psform.PSEnabled;
		bool_diversity = console.Diversity2;
		bool_DUAL_MERCURY_ALEX = cb_DUAL_MERCURY_ALEX.Checked;
		int alex_EXT2EXT1XVTR = console.SetupForm.Alex_EXT2EXT1XVTR;
		if (alex_EXT2EXT1XVTR == 0)
		{
			bool_XVTR = false;
			bool_EXT1 = false;
			bool_EXT2 = false;
		}
		if (alex_EXT2EXT1XVTR == 1)
		{
			bool_XVTR = false;
			bool_EXT1 = false;
			bool_EXT2 = true;
		}
		if (alex_EXT2EXT1XVTR == 2)
		{
			bool_XVTR = false;
			bool_EXT1 = true;
			bool_EXT2 = false;
		}
		if (alex_EXT2EXT1XVTR == 3)
		{
			bool_XVTR = true;
			bool_EXT1 = false;
			bool_EXT2 = false;
		}
		bool_RX1_OUT_on_TX = console.SetupForm.ChkRxOutOnTx;
		bool_RX1_IN_on_TX = console.SetupForm.ChkRx1InOnTx;
		bool_RX2_IN_on_TX = console.SetupForm.ChkRx2InOnTx;
		bool_HPF_BYPASS = console.SetupForm.RadBPHPFled | console.AlexHPFBypass;
		bool_DisableHPFOnTx = console.SetupForm.ChkDisableHPFOnTx;
		if (!bool_rx)
		{
			if (bool_RX1_OUT_on_TX)
			{
				bool_BYPASS_on_TX = true;
				bool_EXT1_on_TX = false;
				bool_EXT2_on_TX = false;
			}
			if (bool_RX1_IN_on_TX)
			{
				bool_BYPASS_on_TX = false;
				bool_EXT1_on_TX = true;
				bool_EXT2_on_TX = false;
			}
			if (bool_RX2_IN_on_TX)
			{
				bool_BYPASS_on_TX = false;
				bool_EXT1_on_TX = false;
				bool_EXT2_on_TX = true;
			}
			if (!bool_RX1_OUT_on_TX & !bool_RX1_IN_on_TX & !bool_RX2_IN_on_TX)
			{
				bool_BYPASS_on_TX = false;
				bool_EXT1_on_TX = false;
				bool_EXT2_on_TX = false;
			}
		}
		bool_Rx0_0 = console.SetupForm.RadRX1ADC1;
		bool_Rx0_1 = console.SetupForm.RadRX1ADC2;
		bool_Rx1_0 = console.SetupForm.RadRX2ADC1;
		bool_Rx1_1 = console.SetupForm.RadRX2ADC2;
		bool_Rx2_0 = console.SetupForm.RadRX3ADC1;
		bool_Rx2_1 = console.SetupForm.RadRX3ADC2;
		bool_Rx3_0 = console.SetupForm.RadRX4ADC1;
		bool_Rx3_1 = console.SetupForm.RadRX4ADC2;
		bool_Rx4_0 = console.SetupForm.RadRX5ADC1;
		bool_Rx4_1 = console.SetupForm.RadRX5ADC2;
		bool_Rx5_0 = console.SetupForm.RadRX6ADC1;
		bool_Rx5_1 = console.SetupForm.RadRX6ADC2;
		bool_Rx6_0 = console.SetupForm.RadRX7ADC1;
		bool_Rx6_1 = console.SetupForm.RadRX7ADC2;
	}

	private void draw_HPSDR()
	{
		hide_controls();
		cb_DUAL_MERCURY_ALEX.Visible = true;
		cb_DUAL_MERCURY_ALEX.Text = "DUAL MERCURY/ALEX";
		Update_control_settings();
		bool flag = bool_EXT1;
		bool_EXT1 = bool_EXT2;
		bool_EXT2 = flag;
		hide_all_labels();
		label_hardware_selected.Text = "Routing for HPSDR";
		label_PC.Visible = true;
		label_RX1_DISPLAY.Visible = true;
		if (bool_XVTR)
		{
			label_XVTR_VHF.Visible = true;
		}
		label_C2.Text = "XV RX IN";
		label_C2.Visible = true;
		label_C2.Location = C2_label_HPSDR;
		label_C3.Text = "RX 2 IN";
		label_C3.Visible = true;
		label_C3.Location = C3_label_HPSDR;
		label_C4.Text = "RX 1 IN";
		label_C4.Visible = true;
		label_C4.Location = C4_label_HPSDR;
		label_C5.Text = "RX 1 OUT";
		label_C5.Visible = true;
		label_C5.Location = C5_label_HPSDR;
		label_C7.Text = "ANT 1";
		label_C7.Visible = true;
		label_C7.Location = C7_label_HPSDR;
		label_C8.Text = "ANT 2";
		label_C8.Visible = true;
		label_C8.Location = C8_label_HPSDR;
		label_C9.Text = "ANT 3";
		label_C9.Visible = true;
		label_C9.Location = C9_label_HPSDR;
		label_C10.Text = "From TX*";
		label_C10.Visible = true;
		label_C10.Location = C10_label_ALEX_TX_IN;
		label_DSP_HPSDR.Visible = true;
		label_DSP_HPSDR.Location = DSP_HPSDR_label;
		if (!bool_rx)
		{
			label_ext_amp1.Visible = true;
			label_ext_amp1.Location = ext_amp_label1;
			label_ext_amp2.Visible = true;
			label_ext_amp2.Location = ext_amp_label2;
		}
		label_ALEX.Visible = true;
		label_ALEX.Location = ALEX_label;
		if (!bool_HPF_BYPASS)
		{
			label_ALEX_HPF.Visible = true;
		}
		label_ALEX_HPF.Location = ALEX_HPF_label;
		label_ALEX_LPF.Visible = true;
		label_ALEX_LPF.Location = ALEX_LPF_label;
		label_ALEX_To_RX.Visible = true;
		label_ALEX_To_RX.Location = ALEX_To_RX_label;
		if (cb_DUAL_MERCURY_ALEX.Checked)
		{
			label_C16.Text = "XV RX IN";
			label_C16.Visible = true;
			label_C16.Location = C16_ALEX_2_label;
			label_C17.Text = "RX 2 IN";
			label_C17.Visible = true;
			label_C17.Location = C17_ALEX_2_label;
			label_C18.Text = "RX 1 IN";
			label_C18.Visible = true;
			label_C18.Location = C18_ALEX_2_label;
			label_C19.Text = "RX 1 OUT";
			label_C19.Visible = true;
			label_C19.Location = C19_ALEX_2_label;
			label_C20.Text = "ANT 1";
			label_C20.Visible = true;
			label_C20.Location = C20_ALEX_2_label;
			label_C24.Text = "ANT 2";
			label_C24.Visible = true;
			label_C24.Location = C24_ALEX_2_label;
			label_C25.Text = "ANT 3";
			label_C25.Visible = true;
			label_C25.Location = C25_ALEX_2_label;
			label_C26.Text = "From Tx";
			label_C26.Visible = true;
			label_C26.Location = C26_ALEX_2_label;
			label_ALEX_2.Visible = true;
			label_ALEX_2.Location = ALEX_2_label;
			if (!bool_HPF_BYPASS)
			{
				label_ALEX_2_HPF.Visible = true;
			}
			label_ALEX_2_HPF.Location = ALEX_2_HPF_label;
			label_ALEX_2_LPF.Visible = true;
			label_ALEX_2_LPF.Location = ALEX_2_LPF_label;
			label_ALEX_2_To_RX.Visible = true;
			label_ALEX_2_To_RX.Location = ALEX_2_To_RX_label;
			cb_DUAL_MERCURY_ALEX.Text = "DUAL MERCURY/ALEX *";
			label_DUAL_MERCURY.Visible = true;
		}
		label_MERCURY.Visible = true;
		label_MERCURY.Location = MERCURY_label;
		label_MERCURY_FPGA.Visible = true;
		label_MERCURY_FPGA.Visible = true;
		label_MERCURY_FPGA.ForeColor = Color.IndianRed;
		label_MERCURY_FPGA.Location = MERCURY_FPGA_label;
		label_MERCURY_ADC.Visible = true;
		label_MERCURY_ADC.Location = MERCURY_ADC_label;
		label_MERCURY_DDC0.Visible = true;
		label_MERCURY_DDC0.Location = MERCURY_DDC0_label;
		label_MERCURY_Rx0.Visible = true;
		label_MERCURY_Rx0.Location = MERCURY_DDC0_Rx0_label;
		label_MERCURY_DDC1.Visible = true;
		label_MERCURY_DDC1.Location = MERCURY_DDC1_label;
		label_MERCURY_Rx1.Visible = true;
		label_MERCURY_Rx1.Location = MERCURY_DDC1_Rx1_label;
		label_MERCURY_DDC2.Visible = true;
		label_MERCURY_DDC2.Location = MERCURY_DDC2_label;
		label_MERCURY_Rx2.Visible = true;
		label_MERCURY_Rx2.Location = MERCURY_DDC2_Rx2_label;
		label_MERCURY_CODEC.Visible = true;
		label_MERCURY_CODEC.Location = MERCURY_CODEC_label;
		label_MERCURY_PHONES.Visible = true;
		label_MERCURY_PHONES.Location = MERCURY_PHONES_label;
		label_MERCURY_P_OUT.Visible = true;
		label_MERCURY_P_OUT.Location = MERCURY_P_OUT_label;
		label_MERCURY_LINE.Visible = true;
		label_MERCURY_LINE.Location = MERCURY_LINE_label;
		label_MERCURY_OUT.Visible = true;
		label_MERCURY_OUT.Location = MERCURY_OUT_label;
		label_MERCURY_ANT.Visible = true;
		label_MERCURY_ANT.Location = MERCURY_ANT_label;
		if (cb_DUAL_MERCURY_ALEX.Checked)
		{
			label_MERCURY_2.Visible = true;
			label_MERCURY_2.Location = MERCURY_2_label;
			label_MERCURY_2_FPGA.Visible = true;
			label_MERCURY_2_FPGA.ForeColor = Color.IndianRed;
			label_MERCURY_2_FPGA.Location = MERCURY_2_FPGA_label;
			label_MERCURY_2_ADC.Visible = true;
			label_MERCURY_2_ADC.Location = MERCURY_2_ADC_label;
			label_MERCURY_2_DDC0.Visible = true;
			label_MERCURY_2_DDC0.Location = MERCURY_2_DDC0_label;
			label_MERCURY_2_Rx0.Visible = true;
			label_MERCURY_2_Rx0.Location = MERCURY_2_DDC0_Rx0_label;
			label_MERCURY_2_DDC1.Visible = true;
			label_MERCURY_2_DDC1.Location = MERCURY_2_DDC1_label;
			label_MERCURY_2_Rx1.Visible = true;
			label_MERCURY_2_Rx1.Location = MERCURY_2_DDC1_Rx1_label;
			label_MERCURY_2_DDC2.Visible = true;
			label_MERCURY_2_DDC2.Location = MERCURY_2_DDC2_label;
			label_MERCURY_2_Rx2.Visible = true;
			label_MERCURY_2_Rx2.Location = MERCURY_2_DDC2_Rx2_label;
			label_MERCURY_2_CODEC.Visible = true;
			label_MERCURY_2_CODEC.Location = MERCURY_2_CODEC_label;
			label_MERCURY_2_PHONES.Visible = true;
			label_MERCURY_2_PHONES.Location = MERCURY_2_PHONES_label;
			label_MERCURY_2_P_OUT.Visible = true;
			label_MERCURY_2_P_OUT.Location = MERCURY_2_P_OUT_label;
			label_MERCURY_2_LINE.Visible = true;
			label_MERCURY_2_LINE.Location = MERCURY_2_LINE_label;
			label_MERCURY_2_OUT.Visible = true;
			label_MERCURY_2_OUT.Location = MERCURY_2_OUT_label;
			label_MERCURY_2_ANT.Visible = true;
			label_MERCURY_2_ANT.Location = MERCURY_2_ANT_label;
		}
		label_METIS.Visible = true;
		label_METIS.Location = METIS_label;
		label_METIS_FPGA.Visible = true;
		label_METIS_FPGA.ForeColor = Color.IndianRed;
		label_METIS_FPGA.Location = METIS_FPGA_label;
		label_PENELOPE.Visible = true;
		label_PENELOPE.Location = PENELOPE_label;
		label_PENELOPE_FPGA.Visible = true;
		label_PENELOPE_FPGA.ForeColor = Color.IndianRed;
		label_PENELOPE_FPGA.Location = PENELOPE_FPGA_label;
		label_PENELOPE_DAC.Visible = true;
		label_PENELOPE_DAC.Location = PENELOPE_DAC_label;
		label_PENELOPE_DUC.Visible = true;
		label_PENELOPE_DUC.Location = PENELOPE_DUC_label;
		label_PENELOPE_AMPF.Visible = true;
		label_PENELOPE_AMPF.Location = PENELOPE_AMPF_label;
		label_PENELOPE_FILTER.Visible = true;
		label_PENELOPE_FILTER.Location = PENELOPE_FILTER_label;
		label_PENELOPE_PA.Visible = true;
		label_PENELOPE_PA.Location = PENELOPE_PA_label;
		label_PENELOPE_CODEC.Visible = true;
		label_PENELOPE_CODEC.Location = PENELOPE_CODEC_label;
		label_PENELOPE_ANT.Visible = true;
		label_PENELOPE_ANT.Location = PENELOPE_ANT_label;
		label_PENELOPE_LINE.Visible = true;
		label_PENELOPE_LINE.Location = PENELOPE_LINE_label;
		label_PENELOPE_IN.Visible = true;
		label_PENELOPE_IN.Location = PENELOPE_IN_label;
		label_PENELOPE_MIC.Visible = true;
		label_PENELOPE_MIC.Location = PENELOPE_MIC_label;
		label_PENELOPE_XVTR.Visible = true;
		label_PENELOPE_XVTR.Location = PENELOPE_XVTR_label;
		g.DrawRectangle(blackPen2, ALEX);
		if (!bool_HPF_BYPASS)
		{
			g.DrawRectangle(blackPen, ALEX_HPF);
		}
		g.DrawRectangle(blackPen, ALEX_LPF);
		g.DrawRectangle(blackPen2, MERCURY);
		g.DrawRectangle(indianredPen, MERCURY_FPGA);
		g.DrawRectangle(blackPen, MERCURY_DDC0);
		g.DrawRectangle(blackPen, MERCURY_DDC1);
		g.DrawRectangle(blackPen, MERCURY_DDC2);
		g.DrawRectangle(blackPen, MERCURY_ADC);
		g.DrawRectangle(blackPen, MERCURY_CODEC);
		g.DrawRectangle(blackPen2, PENELOPE);
		g.DrawRectangle(indianredPen, PENELOPE_FPGA);
		g.DrawRectangle(blackPen, PENELOPE_PA);
		g.DrawRectangle(blackPen, PENELOPE_AMPF);
		g.DrawRectangle(blackPen, PENELOPE_DAC);
		g.DrawRectangle(blackPen, PENELOPE_DUC);
		g.DrawRectangle(blackPen, PENELOPE_CODEC);
		if (cb_DUAL_MERCURY_ALEX.Checked)
		{
			g.DrawRectangle(blackPen2, ALEX_2);
			if (!bool_HPF_BYPASS)
			{
				g.DrawRectangle(blackPen, ALEX_2_HPF);
			}
			g.DrawRectangle(blackPen, ALEX_2_LPF);
			g.DrawRectangle(blackPen2, MERCURY_2);
			g.DrawRectangle(indianredPen, MERCURY_2_FPGA);
			g.DrawRectangle(blackPen, MERCURY_2_DDC0);
			g.DrawRectangle(blackPen, MERCURY_2_DDC1);
			g.DrawRectangle(blackPen, MERCURY_2_DDC2);
			g.DrawRectangle(blackPen, MERCURY_2_ADC);
			g.DrawRectangle(blackPen, MERCURY_2_CODEC);
		}
		g.DrawRectangle(blackPen2, PC);
		g.DrawRectangle(blackPen, DSP_HPSDR);
		g.DrawRectangle(blackPen, RX1_DISPLAY);
		g.DrawRectangle(blackPen2, METIS);
		g.DrawRectangle(indianredPen, METIS_FPGA);
		if (bool_rx)
		{
			ALEX_ANT_to_HPF_B(bluePen);
			g.DrawLine(bluePen, ALEX_RX_out, MERCURY_ADC_in);
			MERCURY_ADC_to_DDCs(bluePen);
			MERCURY_DDC0_to_RX1_DISPLAY(bluePen);
			if (bool_DUAL_MERCURY_ALEX)
			{
				ALEX_2_ANT_to_HPF_B(bluePen);
				ALEX_2_RX_out_to_ADC(bluePen);
				MERCURY_2_ADC_to_DDCs(bluePen);
				MERCURY_2_AUDIO_INPUT(bluePen);
				if (bool_diversity)
				{
					MERCURY_2_DDC0_to_METIS_diversity(bluePen);
				}
				else
				{
					g.DrawRectangle(blackPen, RX2_DISPLAY);
					label_RX2_DISPLAY.Visible = true;
					MERCURY_2_DDC0_to_RX2_DISPLAY(bluePen);
				}
			}
			else
			{
				MERCURY_DDC1_to_RX2_DISPLAY(bluePen);
				g.DrawRectangle(blackPen, RX2_DISPLAY);
				label_RX2_DISPLAY.Visible = true;
			}
			if (!bool_RX1_MUTE)
			{
				label_RX1_LR_audio.Visible = true;
				MERCURY_RX1_to_AUDIO_MIXER(bluePen);
				HPSDR_DSP_to_AUDIO_MIXER_input_1(bluePen);
			}
			else
			{
				label_RX1_LR_audio.Visible = false;
			}
			if (!bool_RX2_MUTE)
			{
				if (!bool_diversity)
				{
					label_RX2_LR_audio.Visible = true;
					MERCURY_RX2_to_AUDIO_MIXER(bluePen);
					HPSDR_DSP_to_AUDIO_MIXER_input_2(bluePen);
				}
			}
			else
			{
				label_RX2_LR_audio.Visible = false;
			}
			if (bool_RX1_MUTE & bool_RX2_MUTE)
			{
				label_AUDIO_MIXER.Visible = false;
				label_RX1_LR_audio.Visible = false;
				label_RX2_LR_audio.Visible = false;
			}
			else
			{
				g.DrawRectangle(blackPen, AUDIO_MIXER);
				label_AUDIO_MIXER.Visible = true;
			}
			if (!bool_XVTR & !bool_EXT1 & !bool_EXT2)
			{
				g.DrawLine(bluePen, ALEX_HPF_corner1, ALEX_RX_out);
				g.DrawLine(bluePen, ALEX_HPF_corner1, ALEX_HPF_B);
				if (bool_DUAL_MERCURY_ALEX)
				{
					g.DrawLine(bluePen, ALEX_2_HPF_corner1, ALEX_2_RX_out);
					g.DrawLine(bluePen, ALEX_2_HPF_corner1, ALEX_2_HPF_B);
				}
			}
			if (bool_XVTR & !bool_EXT1 & !bool_EXT2)
			{
				if (bool_disable_BYPASS)
				{
					g.DrawLine(bluePen, ALEX_HPF_B, ALEX_HPF_corner1);
				}
				else
				{
					g.DrawLine(bluePen, ALEX_HPF_B, ALEX_HPF_corner2);
					g.DrawLine(bluePen, ALEX_HPF_corner2, ALEX_RX1_out);
				}
				g.DrawLine(bluePen, ALEX_HPF_corner3, ALEX_HPF_corner1);
				g.DrawLine(bluePen, ALEX_HPF_corner1, ALEX_RX_out);
				g.DrawLine(bluePen, ALEX_XV_RX_IN, ALEX_HPF_corner3);
				if (bool_DUAL_MERCURY_ALEX)
				{
					if (bool_disable_BYPASS)
					{
						g.DrawLine(bluePen, ALEX_2_HPF_B, ALEX_2_HPF_corner1);
					}
					else
					{
						g.DrawLine(bluePen, ALEX_2_HPF_B, ALEX_2_HPF_corner2);
						g.DrawLine(bluePen, ALEX_2_HPF_corner2, ALEX_2_RX1_out);
					}
					g.DrawLine(bluePen, ALEX_2_HPF_corner3, ALEX_2_HPF_corner1);
					g.DrawLine(bluePen, ALEX_2_HPF_corner1, ALEX_2_RX_out);
					g.DrawLine(bluePen, ALEX_2_XV_RX_IN, ALEX_2_HPF_corner3);
				}
			}
			if (!bool_XVTR & bool_EXT1 & !bool_EXT2)
			{
				if (bool_disable_BYPASS)
				{
					g.DrawLine(bluePen, ALEX_HPF_B, ALEX_HPF_corner1);
				}
				else
				{
					g.DrawLine(bluePen, ALEX_HPF_B, ALEX_HPF_corner2);
					g.DrawLine(bluePen, ALEX_HPF_corner2, ALEX_RX1_out);
				}
				g.DrawLine(bluePen, ALEX_HPF_corner5, ALEX_HPF_corner1);
				g.DrawLine(bluePen, ALEX_HPF_corner1, ALEX_RX_out);
				g.DrawLine(bluePen, ALEX_RX_1_IN, ALEX_HPF_corner5);
				if (bool_DUAL_MERCURY_ALEX)
				{
					if (bool_disable_BYPASS)
					{
						g.DrawLine(bluePen, ALEX_2_HPF_B, ALEX_2_HPF_corner1);
					}
					else
					{
						g.DrawLine(bluePen, ALEX_2_HPF_B, ALEX_2_HPF_corner2);
						g.DrawLine(bluePen, ALEX_2_HPF_corner2, ALEX_2_RX1_out);
					}
					g.DrawLine(bluePen, ALEX_2_HPF_corner5, ALEX_2_HPF_corner1);
					g.DrawLine(bluePen, ALEX_2_HPF_corner1, ALEX_2_RX_out);
					g.DrawLine(bluePen, ALEX_2_RX_1_IN, ALEX_2_HPF_corner5);
				}
			}
			if (!bool_XVTR & !bool_EXT1 & bool_EXT2)
			{
				if (bool_disable_BYPASS)
				{
					g.DrawLine(bluePen, ALEX_HPF_B, ALEX_HPF_corner1);
				}
				else
				{
					g.DrawLine(bluePen, ALEX_HPF_B, ALEX_HPF_corner2);
					g.DrawLine(bluePen, ALEX_HPF_corner2, ALEX_RX1_out);
				}
				g.DrawLine(bluePen, ALEX_HPF_corner4, ALEX_HPF_corner1);
				g.DrawLine(bluePen, ALEX_HPF_corner1, ALEX_RX_out);
				g.DrawLine(bluePen, ALEX_RX_2_IN, ALEX_HPF_corner4);
				if (bool_DUAL_MERCURY_ALEX)
				{
					if (bool_disable_BYPASS)
					{
						g.DrawLine(bluePen, ALEX_2_HPF_B, ALEX_2_HPF_corner1);
					}
					else
					{
						g.DrawLine(bluePen, ALEX_2_HPF_B, ALEX_2_HPF_corner2);
						g.DrawLine(bluePen, ALEX_2_HPF_corner2, ALEX_2_RX1_out);
					}
					g.DrawLine(bluePen, ALEX_2_HPF_corner4, ALEX_2_HPF_corner1);
					g.DrawLine(bluePen, ALEX_2_HPF_corner1, ALEX_2_RX_out);
					g.DrawLine(bluePen, ALEX_2_RX_2_IN, ALEX_2_HPF_corner4);
				}
			}
		}
		else
		{
			PENELOPE_PA_to_ALEX_LPF(redPen);
			ALEX_TX_ANT(redPen);
			PENELOPE_PA_to_DSP(redPen);
			PENELOPE_DSP_to_CODEC(redPen);
			if (bool_MON)
			{
				if (!bool_RX1_MUTE)
				{
					label_RX1_LR_audio.Visible = true;
					MERCURY_RX1_to_AUDIO_MIXER(bluePen);
					HPSDR_DSP_to_AUDIO_MIXER_input_1(bluePen);
				}
				else
				{
					label_RX1_LR_audio.Visible = false;
				}
				if (bool_RX1_MUTE & bool_RX2_MUTE)
				{
					label_AUDIO_MIXER.Visible = false;
					label_RX1_LR_audio.Visible = false;
					label_RX2_LR_audio.Visible = false;
				}
				else if (!bool_RX1_MUTE)
				{
					g.DrawRectangle(blackPen, AUDIO_MIXER);
					label_AUDIO_MIXER.Visible = true;
				}
				if (bool_DUAL_MERCURY_ALEX & !bool_RX1_MUTE)
				{
					MERCURY_2_AUDIO_INPUT(bluePen);
				}
			}
			if (bool_EXT1_on_TX)
			{
				g.DrawLine(bluePen, ALEX_HPF_corner5, ALEX_HPF_corner1);
				g.DrawLine(bluePen, ALEX_HPF_corner1, ALEX_RX_out);
				g.DrawLine(bluePen, ALEX_RX_1_IN, ALEX_HPF_corner5);
				if (bool_DUAL_MERCURY_ALEX)
				{
					g.DrawLine(bluePen, ALEX_2_HPF_corner5, ALEX_2_HPF_corner1);
					g.DrawLine(bluePen, ALEX_2_HPF_corner1, ALEX_2_RX_out);
					g.DrawLine(bluePen, ALEX_2_RX_1_IN, ALEX_2_HPF_corner5);
				}
			}
			if (bool_EXT2_on_TX)
			{
				g.DrawLine(bluePen, ALEX_HPF_corner4, ALEX_HPF_corner1);
				g.DrawLine(bluePen, ALEX_HPF_corner1, ALEX_RX_out);
				g.DrawLine(bluePen, ALEX_RX_2_IN, ALEX_HPF_corner4);
				if (bool_DUAL_MERCURY_ALEX)
				{
					g.DrawLine(bluePen, ALEX_2_HPF_corner4, ALEX_2_HPF_corner1);
					g.DrawLine(bluePen, ALEX_2_HPF_corner1, ALEX_2_RX_out);
					g.DrawLine(bluePen, ALEX_2_RX_2_IN, ALEX_2_HPF_corner4);
				}
			}
			if (bool_duplex)
			{
				g.DrawLine(bluePen, ALEX_RX_out, MERCURY_ADC_in);
				MERCURY_ADC_to_DDCs(bluePen);
				MERCURY_DDC0_to_RX1_DISPLAY(bluePen);
				if (bool_DUAL_MERCURY_ALEX)
				{
					ALEX_2_RX_out_to_ADC(bluePen);
					MERCURY_2_ADC_to_DDCs(bluePen);
					MERCURY_2_AUDIO_INPUT(bluePen);
					if (bool_diversity)
					{
						MERCURY_2_DDC0_to_METIS_diversity(bluePen);
					}
					else
					{
						g.DrawRectangle(blackPen, RX2_DISPLAY);
						label_RX2_DISPLAY.Visible = true;
						MERCURY_2_DDC0_to_RX2_DISPLAY(bluePen);
					}
				}
				else
				{
					MERCURY_DDC1_to_RX2_DISPLAY(bluePen);
					g.DrawRectangle(blackPen, RX2_DISPLAY);
					label_RX2_DISPLAY.Visible = true;
				}
				if (!bool_RX1_MUTE)
				{
					label_RX1_LR_audio.Visible = true;
					MERCURY_RX1_to_AUDIO_MIXER(bluePen);
					HPSDR_DSP_to_AUDIO_MIXER_input_1(bluePen);
				}
				else
				{
					label_RX1_LR_audio.Visible = false;
				}
				if (!bool_RX2_MUTE)
				{
					if (!bool_diversity)
					{
						label_RX2_LR_audio.Visible = true;
						MERCURY_RX2_to_AUDIO_MIXER(bluePen);
						HPSDR_DSP_to_AUDIO_MIXER_input_2(bluePen);
					}
				}
				else
				{
					label_RX2_LR_audio.Visible = false;
				}
				if (bool_RX1_MUTE & bool_RX2_MUTE)
				{
					label_AUDIO_MIXER.Visible = false;
					label_RX1_LR_audio.Visible = false;
					label_RX2_LR_audio.Visible = false;
				}
				else
				{
					g.DrawRectangle(blackPen, AUDIO_MIXER);
					label_AUDIO_MIXER.Visible = true;
				}
			}
			else
			{
				g.DrawLine(redPen, PENELOPE_DSP_mid_1, PENELOPE_DSP_mid_2);
				g.DrawLine(redPen, PENELOPE_DSP_mid_2, RX1_DISPLAY_HPSDR_DDC0);
			}
		}
		update_diagram = false;
	}

	private void draw_HERMES()
	{
		hide_controls();
		Update_control_settings();
		bool flag = bool_EXT1;
		bool_EXT1 = bool_EXT2;
		bool_EXT2 = flag;
		hide_all_labels();
		label_hardware_selected.Text = "Routing for HERMES";
		label_PC.Visible = true;
		label_RX1_DISPLAY.Visible = true;
		if (bool_XVTR)
		{
			label_XVTR_VHF.Visible = true;
		}
		label_ADC0.Visible = true;
		label_ADC0_atten.Visible = true;
		label_C26.Visible = true;
		label_C25.Visible = true;
		label_C24.Visible = true;
		label_C2.Text = "XV RX IN";
		label_C2.Visible = true;
		label_C2.Location = C2_label_HPSDR;
		label_C3.Text = "RX 2 IN";
		label_C3.Visible = true;
		label_C3.Location = C3_label_HPSDR;
		label_C4.Text = "RX 1 IN";
		label_C4.Visible = true;
		label_C4.Location = C4_label_HPSDR;
		label_C5.Text = "RX 1 OUT";
		label_C5.Visible = true;
		label_C5.Location = C5_label_HPSDR;
		label_C7.Text = "ANT 1";
		label_C7.Visible = true;
		label_C7.Location = C7_label_HPSDR;
		label_C8.Text = "ANT 2";
		label_C8.Visible = true;
		label_C8.Location = C8_label_HPSDR;
		label_C9.Text = "ANT 3";
		label_C9.Visible = true;
		label_C9.Location = C9_label_HPSDR;
		label_C10.Text = "From TX*";
		label_C10.Visible = true;
		label_C10.Location = C10_label_ALEX_TX_IN;
		label_DSP_HPSDR.Visible = true;
		label_DSP_HPSDR.Location = DSP_HPSDR_label;
		label_ALEX.Visible = true;
		label_ALEX.Location = ALEX_label;
		label_HERMES.Visible = true;
		label_HERMES.Location = HERMES_label;
		label_HERMES_RX_IN.Visible = true;
		label_HERMES_RX_IN.Location = HERMES_RX_IN_label;
		label_HERMES_J5.Visible = true;
		label_HERMES_J5.Location = HERMES_J5_label;
		label_HERMES_TX_OUT.Visible = true;
		label_HERMES_TX_OUT.Location = HERMES_TX_OUT_label;
		label_HERMES_J3.Visible = true;
		label_HERMES_J3.Location = HERMES_J3_label;
		label_HERMES_XVTR_TX.Visible = true;
		label_HERMES_XVTR_TX.Location = HERMES_XVTR_TX_label;
		label_HERMES_J1.Visible = true;
		label_HERMES_J1.Location = HERMES_J1_label;
		if (!bool_HPF_BYPASS)
		{
			label_ALEX_HPF.Visible = true;
		}
		label_ALEX_HPF.Location = ALEX_HPF_label;
		label_ALEX_LPF.Visible = true;
		label_ALEX_LPF.Location = ALEX_LPF_label;
		label_ALEX_To_RX.Visible = true;
		label_ALEX_To_RX.Location = ALEX_To_RX_label;
		label_FPGA.Visible = true;
		label_DDC0.Visible = true;
		label_DDC1.Visible = true;
		label_Rx0.Visible = true;
		label_Rx1.Visible = true;
		if (!bool_rx)
		{
			label_ext_amp1.Visible = true;
			label_ext_amp1.Location = ext_amp_label1;
			label_ext_amp2.Visible = true;
			label_ext_amp2.Location = ext_amp_label2;
		}
		if (bool_diversity)
		{
			bool_diversity = false;
		}
		label_C24.Text = "SPKR";
		C24_label.X = C24.X - 45;
		C24_label.Y = C24.Y - 6;
		label_C24.Location = C24_label;
		label_C26.Text = "MIC";
		C26_label.X = C26.X - 35;
		C26_label.Y = C26.Y - 6;
		label_C26.Location = C26_label;
		label_C25.Text = "HDPHONES";
		C25_label.X = C25.X - 75;
		C25_label.Y = C25.Y - 6;
		label_C25.Location = C25_label;
		blackPen.Width = 2f;
		g.DrawLine(blackPen, HERMES1, HERMES2);
		g.DrawLine(blackPen, HERMES2, HERMES3);
		g.DrawLine(blackPen, HERMES3, HERMES4);
		g.DrawLine(blackPen, HERMES4, HERMES5);
		g.DrawLine(blackPen, HERMES5, HERMES6);
		g.DrawLine(blackPen, HERMES6, HERMES1);
		blackPen.Width = 1f;
		g.DrawRectangle(blackPen2, PC);
		g.DrawRectangle(blackPen, DSP_HERMES);
		g.DrawRectangle(blackPen, RX1_DISPLAY);
		g.DrawRectangle(blackPen2, ALEX);
		if (!bool_HPF_BYPASS)
		{
			g.DrawRectangle(blackPen, ALEX_HPF);
		}
		g.DrawRectangle(blackPen, ALEX_LPF);
		g.DrawRectangle(indianredPen, FPGA);
		g.DrawRectangle(blackPen, Rx0);
		g.DrawRectangle(blackPen, Rx1);
		g.DrawRectangle(blackPen, ADC0);
		if (bool_rx)
		{
			ALEX_ANT_to_HPF_B(bluePen);
			g.DrawLine(bluePen, ALEX_RX_out, HERMES_corner1);
			g.DrawLine(bluePen, HERMES_corner1, HERMES_corner2);
			g.DrawLine(bluePen, HERMES_corner2, ADC0_L);
			g.DrawRectangle(blackPen, RX2_DISPLAY);
			label_RX2_DISPLAY.Visible = true;
			label_PA.Visible = false;
			label_LPF.Visible = false;
			label_AMP.Visible = false;
			label_FILTER.Visible = false;
			label_DAC0.Visible = false;
			label_DUC0.Visible = false;
			ADC0_to_Rx0(bluePen);
			ADC0_to_Rx1(bluePen);
			Rx0_to_DSP(bluePen);
			Rx1_to_DSP(bluePen);
			DSP_in1_to_out1_crossconnect(bluePen);
			DSP_in2_to_out2_crossconnect(bluePen);
			DSP_out1_to_RX1_DISPLAY(bluePen);
			DSP_out2_to_RX2_DISPLAY(bluePen);
			if (!bool_RX1_MUTE)
			{
				SPKR_to_RX1_DISPLAY(bluePen);
			}
			if (!bool_RX2_MUTE)
			{
				SPKR_to_RX2_DISPLAY(bluePen);
			}
			if (!bool_RX1_MUTE)
			{
				label_RX1_LR_audio.Visible = true;
			}
			else
			{
				label_RX1_LR_audio.Visible = false;
			}
			if (!bool_RX2_MUTE)
			{
				label_RX2_LR_audio.Visible = true;
			}
			else
			{
				label_RX2_LR_audio.Visible = false;
			}
			if (bool_RX1_MUTE & bool_RX2_MUTE)
			{
				label_AUDIO_AMP.Visible = false;
				label_CODEC2.Visible = false;
				label_L_audio_only.Visible = false;
				label_LR_audio.Visible = false;
				label_AUDIO_MIXER.Visible = false;
			}
			else
			{
				g.DrawRectangle(blackPen, AUDIO_MIXER);
				label_AUDIO_MIXER.Visible = true;
				g.DrawRectangle(blackPen, CODEC2);
				label_CODEC.Visible = false;
				label_AUDIO_AMP.Visible = true;
				label_CODEC2.Visible = true;
				label_L_audio_only.Visible = true;
				label_LR_audio.Visible = true;
				g.DrawRectangle(blackPen, AUDIO_AMP);
			}
			if (!bool_XVTR & !bool_EXT1 & !bool_EXT2)
			{
				g.DrawLine(bluePen, ALEX_HPF_corner1, ALEX_RX_out);
				g.DrawLine(bluePen, ALEX_HPF_corner1, ALEX_HPF_B);
			}
			if (bool_XVTR & !bool_EXT1 & !bool_EXT2)
			{
				if (bool_disable_BYPASS)
				{
					g.DrawLine(bluePen, ALEX_HPF_B, ALEX_HPF_corner1);
				}
				else
				{
					g.DrawLine(bluePen, ALEX_HPF_B, ALEX_HPF_corner2);
					g.DrawLine(bluePen, ALEX_HPF_corner2, ALEX_RX1_out);
				}
				g.DrawLine(bluePen, ALEX_HPF_corner3, ALEX_HPF_corner1);
				g.DrawLine(bluePen, ALEX_HPF_corner1, ALEX_RX_out);
				g.DrawLine(bluePen, ALEX_XV_RX_IN, ALEX_HPF_corner3);
			}
			if (!bool_XVTR & bool_EXT1 & !bool_EXT2)
			{
				if (bool_disable_BYPASS)
				{
					g.DrawLine(bluePen, ALEX_HPF_B, ALEX_HPF_corner1);
				}
				else
				{
					g.DrawLine(bluePen, ALEX_HPF_B, ALEX_HPF_corner2);
					g.DrawLine(bluePen, ALEX_HPF_corner2, ALEX_RX1_out);
				}
				g.DrawLine(bluePen, ALEX_HPF_corner5, ALEX_HPF_corner1);
				g.DrawLine(bluePen, ALEX_HPF_corner1, ALEX_RX_out);
				g.DrawLine(bluePen, ALEX_RX_1_IN, ALEX_HPF_corner5);
			}
			if (!bool_XVTR & !bool_EXT1 & bool_EXT2)
			{
				if (bool_disable_BYPASS)
				{
					g.DrawLine(bluePen, ALEX_HPF_B, ALEX_HPF_corner1);
				}
				else
				{
					g.DrawLine(bluePen, ALEX_HPF_B, ALEX_HPF_corner2);
					g.DrawLine(bluePen, ALEX_HPF_corner2, ALEX_RX1_out);
				}
				g.DrawLine(bluePen, ALEX_HPF_corner4, ALEX_HPF_corner1);
				g.DrawLine(bluePen, ALEX_HPF_corner1, ALEX_RX_out);
				g.DrawLine(bluePen, ALEX_RX_2_IN, ALEX_HPF_corner4);
			}
		}
		else
		{
			label_FILTER.Visible = true;
			g.DrawRectangle(blackPen, AMPF);
			g.DrawLine(redPen, HERMES_XVTR_TX, AMPF_L_PA15);
			label_DAC0.Visible = true;
			g.DrawRectangle(blackPen, DAC0);
			label_DUC0.Visible = true;
			g.DrawRectangle(blackPen, DUC0);
			label_CODEC.Visible = true;
			g.DrawRectangle(blackPen, CODEC);
			label_L_audio_only.Visible = false;
			label_AUDIO_AMP.Visible = false;
			label_CODEC2.Visible = false;
			label_LR_audio.Visible = false;
			label_RX1_LR_audio.Visible = false;
			label_RX2_LR_audio.Visible = false;
			label_AUDIO_MIXER.Visible = false;
			basic_Tx_path(redPen);
			ADC0_to_Rx0(bluePen);
			Rx0_to_DSP(bluePen);
			TX_AUDIO_OUT_2_RX_MODELS();
			if (!bool_PureSignal & !bool_duplex & !bool_XVTR)
			{
				label_PA.Visible = true;
				g.DrawRectangle(blackPen, PA);
				HERMES_PA_to_ALEX_LPF(redPen);
				ALEX_TX_ANT(redPen);
				label_AMP.Visible = true;
				g.DrawLine(redPen, PA_corner, PA_B);
				loopback_to_RX1_DISPLAY(redPen);
				ADC0_to_Rx1(bluePen);
				Rx1_to_DSP(bluePen);
				DSP_out2_to_RX2_DISPLAY(bluePen);
				DSP_in2_to_out2_crossconnect(bluePen);
			}
			if (!bool_PureSignal & !bool_duplex & bool_XVTR)
			{
				label_PA.Visible = false;
				label_LPF.Visible = false;
				loopback_to_RX1_DISPLAY(bluePen);
				ADC0_to_Rx1(bluePen);
				Rx1_to_DSP(bluePen);
				DSP_out2_to_RX2_DISPLAY(bluePen);
				DSP_in2_to_out2_crossconnect(bluePen);
			}
			if (!bool_PureSignal & bool_duplex & bool_XVTR)
			{
				label_PA.Visible = false;
				label_LPF.Visible = false;
				if (!bool_EXT1_on_TX & !bool_EXT2_on_TX)
				{
					g.DrawLine(bluePen, ALEX_HPF_corner3, ALEX_HPF_corner1);
					g.DrawLine(bluePen, ALEX_HPF_corner1, ALEX_RX_out);
					g.DrawLine(bluePen, ALEX_XV_RX_IN, ALEX_HPF_corner3);
				}
				g.DrawLine(bluePen, ALEX_RX_out, HERMES_corner1);
				g.DrawLine(bluePen, HERMES_corner1, HERMES_corner2);
				g.DrawLine(bluePen, HERMES_corner2, ADC0_L);
				ADC0_to_Rx1(bluePen);
				DSP_in1_to_out1_crossconnect(bluePen);
				DSP_out1_to_RX1_DISPLAY(bluePen);
				Rx1_to_DSP(bluePen);
				DSP_out2_to_RX2_DISPLAY(bluePen);
				DSP_in2_to_out2_crossconnect(bluePen);
				if (!bool_RX1_MUTE)
				{
					SPKR_to_RX1_DISPLAY(bluePen);
				}
				if (!bool_RX2_MUTE)
				{
					SPKR_to_RX2_DISPLAY(bluePen);
				}
			}
			if (!bool_PureSignal & bool_duplex & !bool_XVTR)
			{
				label_PA.Visible = true;
				g.DrawRectangle(blackPen, PA);
				label_AMP.Visible = true;
				HERMES_PA_to_ALEX_LPF(redPen);
				ALEX_TX_ANT(redPen);
				g.DrawLine(redPen, PA_corner, PA_B);
				g.DrawLine(bluePen, ALEX_RX_out, HERMES_corner1);
				g.DrawLine(bluePen, HERMES_corner1, HERMES_corner2);
				g.DrawLine(bluePen, HERMES_corner2, ADC0_L);
				ADC0_to_Rx1(bluePen);
				Rx1_to_DSP(bluePen);
				DSP_in2_to_out2_crossconnect(bluePen);
				DSP_out1_to_RX1_DISPLAY(bluePen);
				DSP_in1_to_out1_crossconnect(bluePen);
				DSP_out2_to_RX2_DISPLAY(bluePen);
			}
			if (bool_PureSignal & !bool_duplex & !bool_XVTR)
			{
				g.DrawLine(bluePen, ALEX_RX_out, HERMES_corner1);
				g.DrawLine(bluePen, HERMES_corner1, HERMES_corner2);
				g.DrawLine(bluePen, HERMES_corner2, ADC0_L);
				loopback_to_RX1_DISPLAY(redPen);
				g.DrawLine(redPen, PA_corner, PA_B);
				Rx1_to_DSP(redPen);
				DUC0_to_Rx1(redPen);
				label_PA.Visible = true;
				g.DrawRectangle(blackPen, PA);
				HERMES_PA_to_ALEX_LPF(redPen);
				ALEX_TX_ANT(redPen);
				label_AMP.Visible = true;
			}
			if (bool_PureSignal & bool_duplex & !bool_XVTR)
			{
				label_PA.Visible = true;
				g.DrawRectangle(blackPen, PA);
				label_LPF.Visible = true;
				g.DrawRectangle(blackPen, LPF);
				label_AMP.Visible = true;
				PA_to_LPF(redPen);
				g.DrawLine(redPen, PA_corner, PA_B);
				Rx1_to_DSP(redPen);
				DUC0_to_Rx1(redPen);
			}
			if (bool_PureSignal & !bool_duplex & bool_XVTR)
			{
				label_PA.Visible = false;
				label_LPF.Visible = false;
				loopback_to_RX1_DISPLAY(redPen);
				Rx1_to_DSP(redPen);
				DUC0_to_Rx1(redPen);
			}
			if (bool_PureSignal & bool_duplex & bool_XVTR)
			{
				label_PA.Visible = false;
				label_LPF.Visible = false;
				DUC0_to_Rx1(redPen);
				Rx1_to_DSP(redPen);
			}
			if (bool_EXT1_on_TX)
			{
				g.DrawLine(bluePen, ALEX_HPF_corner5, ALEX_HPF_corner1);
				g.DrawLine(bluePen, ALEX_HPF_corner1, ALEX_RX_out);
				g.DrawLine(bluePen, ALEX_RX_1_IN, ALEX_HPF_corner5);
				if (bool_DUAL_MERCURY_ALEX)
				{
					g.DrawLine(bluePen, ALEX_2_HPF_corner5, ALEX_2_HPF_corner1);
					g.DrawLine(bluePen, ALEX_2_HPF_corner1, ALEX_2_RX_out);
					g.DrawLine(bluePen, ALEX_2_RX_1_IN, ALEX_2_HPF_corner5);
				}
			}
			if (bool_EXT2_on_TX)
			{
				g.DrawLine(bluePen, ALEX_HPF_corner4, ALEX_HPF_corner1);
				g.DrawLine(bluePen, ALEX_HPF_corner1, ALEX_RX_out);
				g.DrawLine(bluePen, ALEX_RX_2_IN, ALEX_HPF_corner4);
				if (bool_DUAL_MERCURY_ALEX)
				{
					g.DrawLine(bluePen, ALEX_2_HPF_corner4, ALEX_2_HPF_corner1);
					g.DrawLine(bluePen, ALEX_2_HPF_corner1, ALEX_2_RX_out);
					g.DrawLine(bluePen, ALEX_2_RX_2_IN, ALEX_2_HPF_corner4);
				}
			}
		}
		update_diagram = false;
	}

	private void draw_ANAN_10E()
	{
		hide_all_labels();
		label_hardware_selected.Text = "Routing for ANAN-10, ANAN-10E";
		if (bool_XVTR)
		{
			label_XVTR_VHF.Visible = true;
		}
		label_ADC0.Visible = true;
		label_ADC0_atten.Visible = true;
		label_SDR_Hardware.Visible = true;
		label_C26.Visible = true;
		label_C25.Visible = true;
		label_C24.Visible = true;
		label_FPGA.Visible = true;
		label_DDC0.Visible = true;
		label_DDC1.Visible = true;
		label_Rx0.Visible = true;
		label_Rx1.Visible = true;
		label_PC.Visible = true;
		label_RX1_DISPLAY.Visible = true;
		label_DSP.Visible = true;
		hide_controls();
		cb_DUAL_MERCURY_ALEX.Visible = false;
		Update_control_settings();
		g.DrawRectangle(blackPen2, SDR);
		g.DrawRectangle(blackPen2, PC);
		g.DrawRectangle(indianredPen, FPGA);
		g.DrawRectangle(blackPen, DSP);
		g.DrawRectangle(blackPen, RX1_DISPLAY);
		g.DrawRectangle(blackPen, Rx0);
		g.DrawRectangle(blackPen, Rx1);
		g.DrawRectangle(blackPen, ADC0);
		LPF.X = 230;
		LPF.Y = 280;
		LPF_L_c.X = LPF.X - 20;
		LPF_L_c.Y = LPF_L.Y;
		LPF_R.X = LPF.X + 50;
		LPF_R.Y = LPF.Y + 25;
		LPF_R_c.X = LPF.Y + 25;
		LPF_T.X = LPF.X + 25;
		LPF_T.Y = LPF.Y;
		LPF_B.X = LPF.X + 25;
		LPF_B.Y = LPF.Y + 50;
		LPF_corner_C2.X = LPF_T.X;
		LPF_corner_C2.Y = C2.Y;
		LPF_corner_C3.X = LPF.X + 25;
		LPF_corner_C3.Y = C3.Y;
		LPF_corner_C4.X = LPF.X + 25;
		LPF_corner_C4.Y = C4.Y;
		update_labels_PA10();
		if (bool_rx)
		{
			g.DrawRectangle(blackPen, RX2_DISPLAY);
			label_RX2_DISPLAY.Visible = true;
			label_PA.Visible = false;
			label_LPF.Visible = false;
			label_AMP.Visible = false;
			label_FILTER.Visible = false;
			label_DAC0.Visible = false;
			label_DUC0.Visible = false;
			if (bool_ANT1)
			{
				C2_to_Rx0(bluePen);
			}
			if (bool_ANT2)
			{
				C3_to_Rx0(bluePen);
			}
			if (bool_ANT3)
			{
				C4_to_Rx0(bluePen);
			}
			C5_to_ADC0(bluePen);
			ADC0_to_Rx0(bluePen);
			ADC0_to_Rx1(bluePen);
			Rx0_to_DSP(bluePen);
			Rx1_to_DSP(bluePen);
			DSP_in1_to_out1_crossconnect(bluePen);
			DSP_in2_to_out2_crossconnect(bluePen);
			DSP_out1_to_RX1_DISPLAY(bluePen);
			DSP_out2_to_RX2_DISPLAY(bluePen);
			g.DrawRectangle(blackPen2, SDR);
			if (!bool_RX1_MUTE)
			{
				SPKR_to_RX1_DISPLAY(bluePen);
			}
			if (!bool_RX2_MUTE)
			{
				SPKR_to_RX2_DISPLAY(bluePen);
			}
			if (!bool_RX1_MUTE)
			{
				label_RX1_LR_audio.Visible = true;
			}
			else
			{
				label_RX1_LR_audio.Visible = false;
			}
			if (!bool_RX2_MUTE)
			{
				label_RX2_LR_audio.Visible = true;
			}
			else
			{
				label_RX2_LR_audio.Visible = false;
			}
			if (bool_RX1_MUTE & bool_RX2_MUTE)
			{
				label_AUDIO_AMP.Visible = false;
				label_CODEC2.Visible = false;
				label_L_audio_only.Visible = false;
				label_LR_audio.Visible = false;
				label_AUDIO_MIXER.Visible = false;
			}
			else
			{
				g.DrawRectangle(blackPen, AUDIO_MIXER);
				label_AUDIO_MIXER.Visible = true;
				g.DrawRectangle(blackPen, CODEC2);
				label_CODEC.Visible = false;
				label_AUDIO_AMP.Visible = true;
				label_CODEC2.Visible = true;
				label_L_audio_only.Visible = true;
				label_LR_audio.Visible = true;
				g.DrawRectangle(blackPen, AUDIO_AMP);
			}
		}
		else
		{
			label_FILTER.Visible = true;
			g.DrawRectangle(blackPen, AMPF);
			label_DAC0.Visible = true;
			g.DrawRectangle(blackPen, DAC0);
			label_DUC0.Visible = true;
			g.DrawRectangle(blackPen, DUC0);
			label_CODEC.Visible = true;
			g.DrawRectangle(blackPen, CODEC);
			label_L_audio_only.Visible = false;
			label_AUDIO_AMP.Visible = false;
			label_CODEC2.Visible = false;
			label_LR_audio.Visible = false;
			label_RX1_LR_audio.Visible = false;
			label_RX2_LR_audio.Visible = false;
			label_AUDIO_MIXER.Visible = false;
			C5_to_ADC0(bluePen);
			basic_Tx_path(redPen);
			AMPF_TX_path_PA10(redPen);
			ADC0_to_Rx0(bluePen);
			Rx0_to_DSP(bluePen);
			TX_AUDIO_OUT_2_RX_MODELS();
			if (!bool_PureSignal & !bool_duplex & !bool_XVTR)
			{
				label_PA.Visible = true;
				g.DrawRectangle(blackPen, PA);
				label_LPF.Visible = true;
				g.DrawRectangle(blackPen, LPF);
				label_AMP.Visible = true;
				line_to_ground(bluePen);
				PA_to_LPF(redPen);
				g.DrawLine(redPen, PA_corner, PA_B);
				if (bool_ANT1_TX)
				{
					LPF_to_C2(redPen);
				}
				if (bool_ANT2_TX)
				{
					LPF_to_C3(redPen);
				}
				if (bool_ANT3_TX)
				{
					LPF_to_C4(redPen);
				}
				loopback_to_RX1_DISPLAY(redPen);
				ADC0_to_Rx1(bluePen);
				Rx1_to_DSP(bluePen);
				DSP_out2_to_RX2_DISPLAY(bluePen);
				DSP_in2_to_out2_crossconnect(bluePen);
			}
			if (!bool_PureSignal & !bool_duplex & bool_XVTR)
			{
				label_PA.Visible = false;
				label_LPF.Visible = false;
				loopback_to_RX1_DISPLAY(redPen);
				ADC0_to_Rx1(bluePen);
				Rx1_to_DSP(bluePen);
				DSP_out2_to_RX2_DISPLAY(bluePen);
				DSP_in2_to_out2_crossconnect(bluePen);
			}
			if (!bool_PureSignal & bool_duplex & bool_XVTR)
			{
				label_PA.Visible = false;
				label_LPF.Visible = false;
				ADC0_to_Rx1(bluePen);
				DSP_in1_to_out1_crossconnect(bluePen);
				DSP_out1_to_RX1_DISPLAY(bluePen);
				Rx1_to_DSP(bluePen);
				DSP_out2_to_RX2_DISPLAY(bluePen);
				DSP_in2_to_out2_crossconnect(bluePen);
				if (!bool_RX1_MUTE)
				{
					SPKR_to_RX1_DISPLAY(bluePen);
				}
				if (!bool_RX2_MUTE)
				{
					SPKR_to_RX2_DISPLAY(bluePen);
				}
			}
			if (!bool_PureSignal & bool_duplex & !bool_XVTR)
			{
				label_PA.Visible = true;
				g.DrawRectangle(blackPen, PA);
				label_LPF.Visible = true;
				g.DrawRectangle(blackPen, LPF);
				label_AMP.Visible = true;
				line_to_ground(bluePen);
				PA_to_LPF(redPen);
				g.DrawLine(redPen, PA_corner, PA_B);
				if (bool_ANT1_TX)
				{
					LPF_to_C2(redPen);
				}
				if (bool_ANT2_TX)
				{
					LPF_to_C3(redPen);
				}
				if (bool_ANT3_TX)
				{
					LPF_to_C4(redPen);
				}
				ADC0_to_Rx1(bluePen);
				Rx1_to_DSP(bluePen);
				DSP_in2_to_out2_crossconnect(bluePen);
				DSP_out1_to_RX1_DISPLAY(bluePen);
				DSP_in1_to_out1_crossconnect(bluePen);
				DSP_out2_to_RX2_DISPLAY(bluePen);
			}
			if (bool_PureSignal & !bool_duplex & !bool_XVTR)
			{
				loopback_to_RX1_DISPLAY(redPen);
				line_to_ground(bluePen);
				PA_to_LPF(redPen);
				g.DrawLine(redPen, PA_corner, PA_B);
				Rx1_to_DSP(redPen);
				DUC0_to_Rx1(redPen);
				if (bool_ANT1_TX)
				{
					LPF_to_C2(redPen);
				}
				if (bool_ANT2_TX)
				{
					LPF_to_C3(redPen);
				}
				if (bool_ANT3_TX)
				{
					LPF_to_C4(redPen);
				}
				label_PA.Visible = true;
				g.DrawRectangle(blackPen, PA);
				label_LPF.Visible = true;
				g.DrawRectangle(blackPen, LPF);
				label_AMP.Visible = true;
			}
			if (bool_PureSignal & bool_duplex & !bool_XVTR)
			{
				label_PA.Visible = true;
				g.DrawRectangle(blackPen, PA);
				label_LPF.Visible = true;
				g.DrawRectangle(blackPen, LPF);
				label_AMP.Visible = true;
				line_to_ground(bluePen);
				PA_to_LPF(redPen);
				g.DrawLine(redPen, PA_corner, PA_B);
				Rx1_to_DSP(redPen);
				DUC0_to_Rx1(redPen);
				if (bool_ANT1_TX)
				{
					LPF_to_C2(redPen);
				}
				if (bool_ANT2_TX)
				{
					LPF_to_C3(redPen);
				}
				if (bool_ANT3_TX)
				{
					LPF_to_C4(redPen);
				}
			}
			if (bool_PureSignal & !bool_duplex & bool_XVTR)
			{
				label_PA.Visible = false;
				label_LPF.Visible = false;
				loopback_to_RX1_DISPLAY(redPen);
				Rx1_to_DSP(redPen);
				DUC0_to_Rx1(redPen);
			}
			if (bool_PureSignal & bool_duplex & bool_XVTR)
			{
				label_PA.Visible = false;
				label_LPF.Visible = false;
				DUC0_to_Rx1(redPen);
				Rx1_to_DSP(redPen);
			}
		}
		update_diagram = false;
	}

	private void draw_ANAN_100_PA_rev15()
	{
		hide_all_labels();
		label_hardware_selected.Text = "Routing for ANAN-100 (PA rev_15/16)";
		if (bool_XVTR)
		{
			label_XVTR_VHF.Visible = true;
		}
		label_ADC0.Visible = true;
		label_ADC0_atten.Visible = true;
		label_SDR_Hardware.Visible = true;
		label_C26.Visible = true;
		label_C25.Visible = true;
		label_C24.Visible = true;
		label_FPGA.Visible = true;
		label_DDC0.Visible = true;
		label_DDC1.Visible = true;
		label_Rx0.Visible = true;
		label_Rx1.Visible = true;
		label_PC.Visible = true;
		label_RX1_DISPLAY.Visible = true;
		label_DSP.Visible = true;
		label_NOTE.Visible = true;
		label_UNCHECK.Visible = true;
		hide_controls();
		cb_DUAL_MERCURY_ALEX.Visible = false;
		Update_control_settings();
		LPF.X = C10.X + 110;
		LPF.Y = C10.Y - 25;
		LPF_L.X = LPF.X;
		LPF_L.Y = LPF.Y + 25;
		LPF_L_c.X = LPF.X - 20;
		LPF_L_c.Y = LPF_L.Y;
		LPF_R.X = LPF.X + 50;
		LPF_R.Y = LPF.Y + 25;
		LPF_R_c.X = LPF.Y + 25;
		LPF_T.X = LPF.X + 25;
		LPF_T.Y = LPF.Y;
		LPF_BYPASS_corner.X = LPF.X + 25;
		LPF_BYPASS_corner.Y = C7.Y;
		SWR.X = C10.X + 50;
		SWR.Y = C10.Y - 25;
		SWR_L.X = SWR.X;
		SWR_L.Y = SWR.Y + 25;
		SWR_R.X = SWR.X + 50;
		SWR_R.Y = SWR.Y + 25;
		SWR_T.X = SWR.X + 25;
		SWR_T.Y = SWR.Y;
		HPF.X = PA.X;
		HPF.Y = ADC0.Y;
		HPF_R.X = HPF.X + 50;
		HPF_R.Y = HPF.Y + 25;
		HPF_B.X = HPF.X + 25;
		HPF_B.Y = HPF.Y + 50;
		HPF_L.X = HPF.X;
		HPF_L.Y = HPF.Y + 25;
		LPF_HPF_corner.X = HPF.X + 25;
		LPF_HPF_corner.Y = C10.Y;
		XVTR_HPF_corner.X = HPF_B.X;
		XVTR_HPF_corner.Y = C4.Y;
		EXT1_HPF_corner.X = HPF_B.X;
		EXT1_HPF_corner.Y = C5.Y;
		EXT2_HPF_corner.X = HPF_B.X;
		EXT2_HPF_corner.Y = C6.Y;
		update_labels_PA();
		g.DrawRectangle(blackPen2, PC);
		g.DrawRectangle(indianredPen, FPGA);
		g.DrawRectangle(blackPen, DSP);
		g.DrawRectangle(blackPen, RX1_DISPLAY);
		g.DrawRectangle(blackPen, Rx0);
		g.DrawRectangle(blackPen, Rx1);
		g.DrawRectangle(blackPen, ADC0);
		if (bool_rx)
		{
			g.DrawRectangle(blackPen, RX2_DISPLAY);
			label_RX2_DISPLAY.Visible = true;
			label_PA.Visible = false;
			label_AMP.Visible = false;
			label_FILTER.Visible = false;
			label_DAC0.Visible = false;
			label_DUC0.Visible = false;
			if (!bool_HPF_BYPASS)
			{
				g.DrawRectangle(blackPen, HPF);
				label_HPF.Visible = true;
			}
			else
			{
				g.DrawLine(bluePen, HPF_B, HPF_center);
				g.DrawLine(bluePen, HPF_center, HPF_R);
			}
			if (!bool_RX1_MUTE)
			{
				label_RX1_LR_audio.Visible = true;
			}
			else
			{
				label_RX1_LR_audio.Visible = false;
			}
			if (!bool_RX2_MUTE)
			{
				label_RX2_LR_audio.Visible = true;
			}
			else
			{
				label_RX2_LR_audio.Visible = false;
			}
			if (bool_RX1_MUTE & bool_RX2_MUTE)
			{
				label_AUDIO_AMP.Visible = false;
				label_CODEC2.Visible = false;
				label_L_audio_only.Visible = false;
				label_LR_audio.Visible = false;
				label_AUDIO_MIXER.Visible = false;
			}
			else
			{
				g.DrawRectangle(blackPen, AUDIO_MIXER);
				label_AUDIO_MIXER.Visible = true;
				g.DrawRectangle(blackPen, CODEC2);
				label_CODEC.Visible = false;
				label_AUDIO_AMP.Visible = true;
				label_CODEC2.Visible = true;
				label_L_audio_only.Visible = true;
				label_LR_audio.Visible = true;
				g.DrawRectangle(blackPen, AUDIO_AMP);
			}
			g.DrawLine(bluePen, HPF_R, ADC0_L);
			if (!bool_XVTR & !bool_EXT1 & !bool_EXT2 & !bool_BYPASS)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
				LPF_to_HPF_PA15(bluePen);
				if (bool_ANT1)
				{
					C9_to_LPF_L(bluePen);
				}
				if (bool_ANT2)
				{
					C10_to_LPF_L(bluePen);
				}
				if (bool_ANT3)
				{
					C11_to_LPF_L(bluePen);
				}
			}
			if (bool_XVTR & !bool_EXT1 & !bool_EXT2 & !bool_BYPASS)
			{
				XVTR_to_HPF_PA15(bluePen);
				g.DrawRectangle(blackPen, AUDIO_MIXER);
			}
			if (!bool_XVTR & bool_EXT1 & !bool_EXT2 & !bool_BYPASS)
			{
				EXT1_to_HPF_PA15(bluePen);
				g.DrawRectangle(blackPen, AUDIO_MIXER);
			}
			if (!bool_XVTR & !bool_EXT1 & bool_EXT2 & !bool_BYPASS)
			{
				EXT2_to_HPF_PA15(bluePen);
			}
			if (!bool_XVTR & !bool_EXT1 & !bool_EXT2 & bool_BYPASS)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
				LPF_to_BYPASS(bluePen);
				if (bool_ANT1)
				{
					C9_to_LPF_L(bluePen);
				}
				if (bool_ANT2)
				{
					C10_to_LPF_L(bluePen);
				}
				if (bool_ANT3)
				{
					C11_to_LPF_L(bluePen);
				}
			}
			if (bool_XVTR & !bool_EXT1 & !bool_EXT2 & bool_BYPASS)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				XVTR_to_HPF_PA15(bluePen);
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
				LPF_to_BYPASS(bluePen);
				if (bool_ANT1)
				{
					C9_to_LPF_L(bluePen);
				}
				if (bool_ANT2)
				{
					C10_to_LPF_L(bluePen);
				}
				if (bool_ANT3)
				{
					C11_to_LPF_L(bluePen);
				}
			}
			if (!bool_XVTR & bool_EXT1 & !bool_EXT2 & bool_BYPASS)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				EXT1_to_HPF_PA15(bluePen);
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
				LPF_to_BYPASS(bluePen);
				if (bool_ANT1)
				{
					C9_to_LPF_L(bluePen);
				}
				if (bool_ANT2)
				{
					C10_to_LPF_L(bluePen);
				}
				if (bool_ANT3)
				{
					C11_to_LPF_L(bluePen);
				}
			}
			if (!bool_XVTR & !bool_EXT1 & bool_EXT2 & bool_BYPASS)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				EXT2_to_HPF_PA15(bluePen);
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
				LPF_to_BYPASS(bluePen);
				if (bool_ANT1)
				{
					C9_to_LPF_L(bluePen);
				}
				if (bool_ANT2)
				{
					C10_to_LPF_L(bluePen);
				}
				if (bool_ANT3)
				{
					C11_to_LPF_L(bluePen);
				}
			}
			ADC0_to_Rx0(bluePen);
			ADC0_to_Rx1(bluePen);
			Rx0_to_DSP(bluePen);
			Rx1_to_DSP(bluePen);
			DSP_in1_to_out1_crossconnect(bluePen);
			DSP_in2_to_out2_crossconnect(bluePen);
			DSP_out1_to_RX1_DISPLAY(bluePen);
			DSP_out2_to_RX2_DISPLAY(bluePen);
			if (!bool_RX1_MUTE)
			{
				SPKR_to_RX1_DISPLAY(bluePen);
			}
			if (!bool_RX2_MUTE)
			{
				SPKR_to_RX2_DISPLAY(bluePen);
			}
		}
		else
		{
			g.DrawRectangle(blackPen, AMPF);
			g.DrawRectangle(blackPen, DAC0);
			g.DrawRectangle(blackPen, DUC0);
			if (!bool_HPF_BYPASS)
			{
				g.DrawRectangle(blackPen, HPF);
				label_HPF.Visible = true;
			}
			else
			{
				g.DrawLine(bluePen, HPF_L, HPF_center);
				g.DrawLine(bluePen, HPF_center, HPF_R);
			}
			g.DrawLine(bluePen, HPF_R, ADC0_L);
			g.DrawRectangle(blackPen, CODEC);
			label_PA.Visible = true;
			label_AMP.Visible = true;
			label_FILTER.Visible = true;
			label_DAC0.Visible = true;
			label_DUC0.Visible = true;
			label_CODEC.Visible = true;
			label_L_audio_only.Visible = false;
			label_AUDIO_AMP.Visible = false;
			label_CODEC2.Visible = false;
			label_LR_audio.Visible = false;
			label_RX1_LR_audio.Visible = false;
			label_RX2_LR_audio.Visible = false;
			label_AUDIO_MIXER.Visible = false;
			bool flag = bool_EXT1_on_TX;
			bool_EXT1_on_TX = bool_EXT2_on_TX;
			bool_EXT2_on_TX = flag;
			if (!bool_BYPASS_on_TX & !bool_EXT1_on_TX & !bool_EXT2_on_TX)
			{
				HPF_to_ground(bluePen);
			}
			if (bool_BYPASS_on_TX)
			{
				C7_to_ground(bluePen);
			}
			if (bool_EXT1_on_TX)
			{
				C5_to_HPF_PA15_TX(bluePen);
				C7_to_ground(bluePen);
			}
			if (bool_EXT2_on_TX)
			{
				C6_to_HPF_PA15_TX(bluePen);
				C7_to_ground(bluePen);
			}
			if (!bool_XVTR & !bool_EXT1 & !bool_EXT2 & !bool_BYPASS)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
			}
			if (bool_XVTR & !bool_EXT1 & !bool_EXT2 & bool_BYPASS)
			{
				label_LPF.Visible = false;
				label_PA.Visible = false;
			}
			if (!bool_XVTR & bool_EXT1 & !bool_EXT2 & bool_BYPASS)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				g.DrawRectangle(blackPen, PA);
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
				label_PA.Visible = true;
			}
			if (!bool_XVTR & !bool_EXT1 & bool_EXT2 & bool_BYPASS)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				g.DrawRectangle(blackPen, PA);
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
				label_PA.Visible = true;
				label_LPF.Visible = false;
			}
			if (bool_MON)
			{
				if (!bool_RX1_MUTE)
				{
					SPKR_to_RX1_DISPLAY(bluePen);
				}
				if (!bool_RX2_MUTE)
				{
					SPKR_to_RX2_DISPLAY(bluePen);
				}
				if (!bool_RX1_MUTE)
				{
					label_RX1_LR_audio.Visible = true;
				}
				else
				{
					label_RX1_LR_audio.Visible = false;
				}
				if (!bool_RX2_MUTE)
				{
					label_RX2_LR_audio.Visible = true;
				}
				else
				{
					label_RX2_LR_audio.Visible = false;
				}
				if (bool_RX1_MUTE & bool_RX2_MUTE)
				{
					label_AUDIO_AMP.Visible = false;
					label_CODEC2.Visible = false;
					label_L_audio_only.Visible = false;
					label_LR_audio.Visible = false;
					label_AUDIO_MIXER.Visible = false;
				}
				else
				{
					g.DrawRectangle(blackPen, AUDIO_MIXER);
					label_AUDIO_MIXER.Visible = true;
					g.DrawRectangle(blackPen, CODEC2);
					label_AUDIO_AMP.Visible = true;
					label_CODEC2.Visible = true;
					label_L_audio_only.Visible = true;
					label_LR_audio.Visible = true;
					g.DrawRectangle(blackPen, AUDIO_AMP);
				}
			}
			basic_Tx_path(redPen);
			ADC0_to_Rx0(bluePen);
			Rx0_to_DSP(bluePen);
			TX_AUDIO_OUT_2_RX_MODELS();
			if (!bool_PureSignal & !bool_duplex & !bool_XVTR)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				g.DrawRectangle(blackPen, PA);
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
				label_PA.Visible = true;
				PA15_to_LPF(redPen);
				AMPF_XVTR_TX(redPen);
				AMPF_to_PA15(redPen);
				if (bool_ANT1_TX)
				{
					C9_to_LPF_L(redPen);
				}
				if (bool_ANT2_TX)
				{
					C10_to_LPF_L(redPen);
				}
				if (bool_ANT3_TX)
				{
					C11_to_LPF_L(redPen);
				}
				loopback_to_RX1_DISPLAY(redPen);
				ADC0_to_Rx1(bluePen);
				Rx1_to_DSP(bluePen);
				DSP_out2_to_RX2_DISPLAY(bluePen);
				DSP_in2_to_out2_crossconnect(bluePen);
			}
			if (!bool_PureSignal & !bool_duplex & bool_XVTR)
			{
				label_SWR.Visible = false;
				label_LPF.Visible = false;
				label_PA.Visible = false;
				AMPF_XVTR_TX(redPen);
				loopback_to_RX1_DISPLAY(redPen);
				ADC0_to_Rx1(bluePen);
				Rx1_to_DSP(bluePen);
				DSP_out2_to_RX2_DISPLAY(bluePen);
				DSP_in2_to_out2_crossconnect(bluePen);
			}
			if (!bool_PureSignal & bool_duplex & bool_XVTR)
			{
				label_SWR.Visible = false;
				label_LPF.Visible = false;
				label_PA.Visible = false;
				AMPF_XVTR_TX(redPen);
				ADC0_to_Rx1(bluePen);
				DSP_in1_to_out1_crossconnect(bluePen);
				DSP_out1_to_RX1_DISPLAY(bluePen);
				Rx1_to_DSP(bluePen);
				DSP_out2_to_RX2_DISPLAY(bluePen);
				DSP_in2_to_out2_crossconnect(bluePen);
			}
			if (!bool_PureSignal & bool_duplex & !bool_XVTR)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				g.DrawRectangle(blackPen, PA);
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
				label_PA.Visible = true;
				AMPF_XVTR_TX(redPen);
				PA15_to_LPF(redPen);
				g.DrawLine(redPen, PA_corner, PA_B);
				if (bool_ANT1_TX)
				{
					C9_to_LPF_L(redPen);
				}
				if (bool_ANT2_TX)
				{
					C10_to_LPF_L(redPen);
				}
				if (bool_ANT3_TX)
				{
					C11_to_LPF_L(redPen);
				}
				ADC0_to_Rx1(bluePen);
				Rx1_to_DSP(bluePen);
				DSP_in2_to_out2_crossconnect(bluePen);
				DSP_out1_to_RX1_DISPLAY(bluePen);
				DSP_in1_to_out1_crossconnect(bluePen);
				DSP_out2_to_RX2_DISPLAY(bluePen);
			}
			if (bool_PureSignal & !bool_duplex & !bool_XVTR)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				g.DrawRectangle(blackPen, PA);
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
				label_PA.Visible = true;
				loopback_to_RX1_DISPLAY(redPen);
				AMPF_XVTR_TX(redPen);
				PA15_to_LPF(redPen);
				g.DrawLine(redPen, PA_corner, PA_B);
				Rx1_to_DSP(redPen);
				DUC0_to_Rx1(redPen);
				if (bool_ANT1_TX)
				{
					C9_to_LPF_L(redPen);
				}
				if (bool_ANT2_TX)
				{
					C10_to_LPF_L(redPen);
				}
				if (bool_ANT3_TX)
				{
					C11_to_LPF_L(redPen);
				}
			}
			if (bool_PureSignal & bool_duplex & !bool_XVTR)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				g.DrawRectangle(blackPen, PA);
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
				label_PA.Visible = true;
				AMPF_XVTR_TX(redPen);
				PA15_to_LPF(redPen);
				g.DrawLine(redPen, PA_corner, PA_B);
				Rx1_to_DSP(redPen);
				DUC0_to_Rx1(redPen);
				if (bool_ANT1_TX)
				{
					C9_to_LPF_L(redPen);
				}
				if (bool_ANT2_TX)
				{
					C10_to_LPF_L(redPen);
				}
				if (bool_ANT3_TX)
				{
					C11_to_LPF_L(redPen);
				}
			}
			if (bool_PureSignal & !bool_duplex & bool_XVTR)
			{
				label_RX2_DISPLAY.Visible = false;
				label_SWR.Visible = false;
				label_LPF.Visible = false;
				label_PA.Visible = false;
				loopback_to_RX1_DISPLAY(redPen);
				Rx1_to_DSP(redPen);
				DUC0_to_Rx1(redPen);
				AMPF_XVTR_TX(redPen);
			}
			if ((bool_PureSignal & bool_duplex & bool_XVTR) && !(bool_RX1_MUTE & bool_RX2_MUTE))
			{
				label_SWR.Visible = false;
				label_LPF.Visible = false;
				label_PA.Visible = false;
				DUC0_to_Rx1(redPen);
				Rx1_to_DSP(redPen);
				AMPF_XVTR_TX(redPen);
			}
		}
		g.DrawRectangle(blackPen, SDR);
		update_diagram = false;
	}

	private void draw_ANAN_100_PA_rev24()
	{
		hide_all_labels();
		label_hardware_selected.Text = "Routing for ANAN-100, ANAN-100B (PA rev_24)";
		if (bool_XVTR)
		{
			label_XVTR_VHF.Visible = true;
		}
		label_ADC0.Visible = true;
		label_ADC0_atten.Visible = true;
		label_SDR_Hardware.Visible = true;
		label_C26.Visible = true;
		label_C25.Visible = true;
		label_C24.Visible = true;
		label_FPGA.Visible = true;
		label_DDC0.Visible = true;
		label_DDC1.Visible = true;
		label_Rx0.Visible = true;
		label_Rx1.Visible = true;
		label_PC.Visible = true;
		label_RX1_DISPLAY.Visible = true;
		label_DSP.Visible = true;
		label_NOTE.Visible = true;
		label_UNCHECK.Visible = true;
		hide_controls();
		cb_DUAL_MERCURY_ALEX.Visible = false;
		Update_control_settings();
		LPF.X = C10.X + 110;
		LPF.Y = C10.Y - 25;
		LPF_L.X = LPF.X;
		LPF_L.Y = LPF.Y + 25;
		LPF_L_c.X = LPF.X - 20;
		LPF_L_c.Y = LPF_L.Y;
		LPF_R.X = LPF.X + 50;
		LPF_R.Y = LPF.Y + 25;
		LPF_R_c.X = LPF.Y + 25;
		LPF_T.X = LPF.X + 25;
		LPF_T.Y = LPF.Y;
		LPF_BYPASS_corner.X = LPF.X + 25;
		LPF_BYPASS_corner.Y = C7.Y;
		SWR.X = C10.X + 50;
		SWR.Y = C10.Y - 25;
		SWR_L.X = SWR.X;
		SWR_L.Y = SWR.Y + 25;
		SWR_R.X = SWR.X + 50;
		SWR_R.Y = SWR.Y + 25;
		SWR_T.X = SWR.X + 25;
		SWR_T.Y = SWR.Y;
		HPF.X = PA.X;
		HPF.Y = ADC0.Y;
		HPF_R.X = HPF.X + 50;
		HPF_R.Y = HPF.Y + 25;
		HPF_B.X = HPF.X + 25;
		HPF_B.Y = HPF.Y + 50;
		HPF_L.X = HPF.X;
		HPF_L.Y = HPF.Y + 25;
		LPF_HPF_corner.X = HPF.X + 25;
		LPF_HPF_corner.Y = C10.Y;
		XVTR_HPF_corner.X = HPF_B.X;
		XVTR_HPF_corner.Y = C4.Y;
		EXT1_HPF_corner.X = HPF_B.X;
		EXT1_HPF_corner.Y = C5.Y;
		EXT2_HPF_corner.X = HPF_B.X;
		EXT2_HPF_corner.Y = C6.Y;
		update_labels_PA();
		label_HPF.Visible = false;
		g.DrawRectangle(blackPen2, PC);
		g.DrawRectangle(indianredPen, FPGA);
		g.DrawRectangle(blackPen, DSP);
		g.DrawRectangle(blackPen, RX1_DISPLAY);
		g.DrawRectangle(blackPen, Rx0);
		g.DrawRectangle(blackPen, Rx1);
		g.DrawRectangle(blackPen, ADC0);
		Rx0_to_DSP(bluePen);
		if (bool_rx)
		{
			g.DrawRectangle(blackPen, RX2_DISPLAY);
			label_RX2_DISPLAY.Visible = true;
			label_PA.Visible = false;
			label_AMP.Visible = false;
			label_FILTER.Visible = false;
			label_DAC0.Visible = false;
			label_DUC0.Visible = false;
			if (!bool_HPF_BYPASS)
			{
				g.DrawRectangle(blackPen, HPF);
				label_HPF.Visible = true;
			}
			else
			{
				g.DrawLine(bluePen, HPF_B, HPF_center);
				g.DrawLine(bluePen, HPF_center, HPF_R);
			}
			if (!bool_RX1_MUTE)
			{
				label_RX1_LR_audio.Visible = true;
			}
			else
			{
				label_RX1_LR_audio.Visible = false;
			}
			if (!bool_RX2_MUTE)
			{
				label_RX2_LR_audio.Visible = true;
			}
			else
			{
				label_RX2_LR_audio.Visible = false;
			}
			if (bool_RX1_MUTE & bool_RX2_MUTE)
			{
				label_AUDIO_AMP.Visible = false;
				label_CODEC2.Visible = false;
				label_L_audio_only.Visible = false;
				label_LR_audio.Visible = false;
				label_AUDIO_MIXER.Visible = false;
			}
			else
			{
				g.DrawRectangle(blackPen, AUDIO_MIXER);
				label_AUDIO_MIXER.Visible = true;
				g.DrawRectangle(blackPen, CODEC2);
				label_CODEC.Visible = false;
				label_AUDIO_AMP.Visible = true;
				label_CODEC2.Visible = true;
				label_L_audio_only.Visible = true;
				label_LR_audio.Visible = true;
				g.DrawRectangle(blackPen, AUDIO_AMP);
			}
			g.DrawLine(bluePen, HPF_R, ADC0_L);
			if (!bool_XVTR & !bool_EXT1 & !bool_EXT2 & !bool_BYPASS)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
				LPF_to_HPF_PA15(bluePen);
				if (bool_ANT1)
				{
					C9_to_LPF_L(bluePen);
				}
				if (bool_ANT2)
				{
					C10_to_LPF_L(bluePen);
				}
				if (bool_ANT3)
				{
					C11_to_LPF_L(bluePen);
				}
			}
			if (bool_XVTR & !bool_EXT1 & !bool_EXT2 & !bool_BYPASS)
			{
				XVTR_to_HPF_PA15(bluePen);
				g.DrawRectangle(blackPen, AUDIO_MIXER);
			}
			if (!bool_XVTR & bool_EXT1 & !bool_EXT2 & !bool_BYPASS)
			{
				EXT1_to_HPF_PA15(bluePen);
				g.DrawRectangle(blackPen, AUDIO_MIXER);
			}
			if (!bool_XVTR & !bool_EXT1 & bool_EXT2 & !bool_BYPASS)
			{
				EXT2_to_HPF_PA15(bluePen);
			}
			if (!bool_XVTR & !bool_EXT1 & !bool_EXT2 & bool_BYPASS)
			{
				label_SWR.Visible = false;
				label_LPF.Visible = false;
				BYPASS_to_ADC0(bluePen);
			}
			if ((bool_XVTR & !bool_EXT1 & !bool_EXT2 & bool_BYPASS) || (!bool_XVTR & bool_EXT1 & !bool_EXT2 & bool_BYPASS) || (!bool_XVTR & !bool_EXT1 & bool_EXT2 & bool_BYPASS))
			{
				return;
			}
			ADC0_to_Rx0(bluePen);
			ADC0_to_Rx1(bluePen);
			Rx1_to_DSP(bluePen);
			DSP_in1_to_out1_crossconnect(bluePen);
			DSP_in2_to_out2_crossconnect(bluePen);
			DSP_out1_to_RX1_DISPLAY(bluePen);
			DSP_out2_to_RX2_DISPLAY(bluePen);
			if (!bool_RX1_MUTE)
			{
				SPKR_to_RX1_DISPLAY(bluePen);
			}
			if (!bool_RX2_MUTE)
			{
				SPKR_to_RX2_DISPLAY(bluePen);
			}
		}
		else
		{
			if (bool_EXT1_on_TX)
			{
				bool_EXT1_on_TX = false;
			}
			if (bool_EXT2_on_TX)
			{
				bool_EXT2_on_TX = false;
			}
			g.DrawRectangle(blackPen, AMPF);
			g.DrawRectangle(blackPen, DAC0);
			g.DrawRectangle(blackPen, DUC0);
			g.DrawRectangle(blackPen, CODEC);
			label_PA.Visible = true;
			label_AMP.Visible = true;
			label_FILTER.Visible = true;
			label_DAC0.Visible = true;
			label_DUC0.Visible = true;
			label_CODEC.Visible = true;
			label_L_audio_only.Visible = false;
			label_AUDIO_AMP.Visible = false;
			label_CODEC2.Visible = false;
			label_LR_audio.Visible = false;
			label_RX1_LR_audio.Visible = false;
			label_RX2_LR_audio.Visible = false;
			label_AUDIO_MIXER.Visible = false;
			if (bool_EXT1_on_TX || bool_EXT2_on_TX)
			{
				return;
			}
			if (bool_BYPASS_on_TX)
			{
				BYPASS_to_ADC0(bluePen);
			}
			if (!bool_EXT1_on_TX & !bool_EXT2_on_TX & !bool_BYPASS_on_TX)
			{
				SWR_to_ADC0(bluePen);
			}
			if (!bool_XVTR & !bool_EXT1 & !bool_EXT2 & !bool_BYPASS)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
			}
			if (!bool_XVTR & !bool_EXT1 & !bool_EXT2 & bool_BYPASS)
			{
				BYPASS_to_ADC0(bluePen);
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
			}
			if ((bool_XVTR & !bool_EXT1 & !bool_EXT2 & bool_BYPASS) || (!bool_XVTR & bool_EXT1 & !bool_EXT2 & bool_BYPASS) || (!bool_XVTR & !bool_EXT1 & bool_EXT2 & bool_BYPASS))
			{
				return;
			}
			if (bool_MON)
			{
				if (!bool_RX1_MUTE)
				{
					SPKR_to_RX1_DISPLAY(bluePen);
				}
				if (!bool_RX2_MUTE)
				{
					SPKR_to_RX2_DISPLAY(bluePen);
				}
				if (!bool_RX1_MUTE)
				{
					label_RX1_LR_audio.Visible = true;
				}
				else
				{
					label_RX1_LR_audio.Visible = false;
				}
				if (!bool_RX2_MUTE)
				{
					label_RX2_LR_audio.Visible = true;
				}
				else
				{
					label_RX2_LR_audio.Visible = false;
				}
				if (bool_RX1_MUTE & bool_RX2_MUTE)
				{
					label_AUDIO_AMP.Visible = false;
					label_CODEC2.Visible = false;
					label_L_audio_only.Visible = false;
					label_LR_audio.Visible = false;
					label_AUDIO_MIXER.Visible = false;
				}
				else
				{
					g.DrawRectangle(blackPen, AUDIO_MIXER);
					label_AUDIO_MIXER.Visible = true;
					g.DrawRectangle(blackPen, CODEC2);
					label_AUDIO_AMP.Visible = true;
					label_CODEC2.Visible = true;
					label_L_audio_only.Visible = true;
					label_LR_audio.Visible = true;
					g.DrawRectangle(blackPen, AUDIO_AMP);
				}
			}
			basic_Tx_path(redPen);
			ADC0_to_Rx0(bluePen);
			Rx0_to_DSP(bluePen);
			TX_AUDIO_OUT_2_RX_MODELS();
			if (!bool_PureSignal & !bool_duplex & !bool_XVTR)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				g.DrawRectangle(blackPen, PA);
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
				label_PA.Visible = true;
				PA15_to_LPF(redPen);
				AMPF_XVTR_TX(redPen);
				AMPF_to_PA15(redPen);
				if (bool_ANT1_TX)
				{
					C9_to_LPF_L(redPen);
				}
				if (bool_ANT2_TX)
				{
					C10_to_LPF_L(redPen);
				}
				if (bool_ANT3_TX)
				{
					C11_to_LPF_L(redPen);
				}
				loopback_to_RX1_DISPLAY(redPen);
				ADC0_to_Rx1(bluePen);
				Rx1_to_DSP(bluePen);
				DSP_out2_to_RX2_DISPLAY(bluePen);
				DSP_in2_to_out2_crossconnect(bluePen);
			}
			if (!bool_PureSignal & !bool_duplex & bool_XVTR)
			{
				label_SWR.Visible = false;
				label_LPF.Visible = false;
				label_PA.Visible = false;
				AMPF_XVTR_TX(redPen);
				loopback_to_RX1_DISPLAY(redPen);
				ADC0_to_Rx1(bluePen);
				Rx1_to_DSP(bluePen);
				DSP_out2_to_RX2_DISPLAY(bluePen);
				DSP_in2_to_out2_crossconnect(bluePen);
			}
			if (!bool_PureSignal & bool_duplex & bool_XVTR)
			{
				label_SWR.Visible = false;
				label_LPF.Visible = false;
				label_PA.Visible = false;
				AMPF_XVTR_TX(redPen);
				ADC0_to_Rx1(bluePen);
				DSP_in1_to_out1_crossconnect(bluePen);
				DSP_out1_to_RX1_DISPLAY(bluePen);
				Rx1_to_DSP(bluePen);
				DSP_out2_to_RX2_DISPLAY(bluePen);
				DSP_in2_to_out2_crossconnect(bluePen);
			}
			if (!bool_PureSignal & bool_duplex & !bool_XVTR)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				g.DrawRectangle(blackPen, PA);
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
				label_PA.Visible = true;
				AMPF_XVTR_TX(redPen);
				PA15_to_LPF(redPen);
				g.DrawLine(redPen, PA_corner, PA_B);
				if (bool_ANT1_TX)
				{
					C9_to_LPF_L(redPen);
				}
				if (bool_ANT2_TX)
				{
					C10_to_LPF_L(redPen);
				}
				if (bool_ANT3_TX)
				{
					C11_to_LPF_L(redPen);
				}
				ADC0_to_Rx1(bluePen);
				Rx1_to_DSP(bluePen);
				DSP_in2_to_out2_crossconnect(bluePen);
				DSP_out1_to_RX1_DISPLAY(bluePen);
				DSP_in1_to_out1_crossconnect(bluePen);
				DSP_out2_to_RX2_DISPLAY(bluePen);
			}
			if (bool_PureSignal & !bool_duplex & !bool_XVTR)
			{
				SWR_to_ADC0(bluePen);
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				g.DrawRectangle(blackPen, PA);
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
				label_PA.Visible = true;
				loopback_to_RX1_DISPLAY(redPen);
				AMPF_XVTR_TX(redPen);
				PA15_to_LPF(redPen);
				g.DrawLine(redPen, PA_corner, PA_B);
				Rx1_to_DSP(redPen);
				DUC0_to_Rx1(redPen);
				if (bool_ANT1_TX)
				{
					C9_to_LPF_L(redPen);
				}
				if (bool_ANT2_TX)
				{
					C10_to_LPF_L(redPen);
				}
				if (bool_ANT3_TX)
				{
					C11_to_LPF_L(redPen);
				}
			}
			if (bool_PureSignal & bool_duplex & !bool_XVTR)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				g.DrawRectangle(blackPen, PA);
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
				label_PA.Visible = true;
				AMPF_XVTR_TX(redPen);
				PA15_to_LPF(redPen);
				g.DrawLine(redPen, PA_corner, PA_B);
				Rx1_to_DSP(redPen);
				DUC0_to_Rx1(redPen);
				if (bool_ANT1_TX)
				{
					C9_to_LPF_L(redPen);
				}
				if (bool_ANT2_TX)
				{
					C10_to_LPF_L(redPen);
				}
				if (bool_ANT3_TX)
				{
					C11_to_LPF_L(redPen);
				}
			}
			if (bool_PureSignal & !bool_duplex & bool_XVTR)
			{
				label_RX2_DISPLAY.Visible = false;
				label_SWR.Visible = false;
				label_LPF.Visible = false;
				label_PA.Visible = false;
				loopback_to_RX1_DISPLAY(redPen);
				Rx1_to_DSP(redPen);
				DUC0_to_Rx1(redPen);
				AMPF_XVTR_TX(redPen);
			}
			if ((bool_PureSignal & bool_duplex & bool_XVTR) && !(bool_RX1_MUTE & bool_RX2_MUTE))
			{
				label_SWR.Visible = false;
				label_LPF.Visible = false;
				label_PA.Visible = false;
				DUC0_to_Rx1(redPen);
				Rx1_to_DSP(redPen);
				AMPF_XVTR_TX(redPen);
			}
		}
		g.DrawRectangle(blackPen, SDR);
		update_diagram = false;
	}

	private void draw_ANAN_100D_PA_rev15()
	{
		hide_all_labels();
		label_hardware_selected.Text = "Routing for ANAN-100D, ANAN-200D (PA rev_15/16)";
		label_SDR_Hardware.Visible = true;
		label_FPGA.Visible = true;
		label_PC.Visible = true;
		label_DSP.Visible = true;
		label_RX1_DISPLAY.Visible = true;
		if (bool_XVTR)
		{
			label_XVTR_VHF.Visible = true;
		}
		label_NOTE.Visible = true;
		label_UNCHECK.Visible = true;
		hide_controls();
		Update_control_settings();
		LPF.X = C10.X + 110;
		LPF.Y = C10.Y - 25;
		LPF_L.X = LPF.X;
		LPF_L.Y = LPF.Y + 25;
		LPF_L_c.X = LPF.X - 20;
		LPF_L_c.Y = LPF_L.Y;
		LPF_R.X = LPF.X + 50;
		LPF_R.Y = LPF.Y + 25;
		LPF_R_c.X = LPF.Y + 25;
		LPF_T.X = LPF.X + 25;
		LPF_T.Y = LPF.Y;
		LPF_BYPASS_corner.X = LPF.X + 25;
		LPF_BYPASS_corner.Y = C7.Y;
		SWR.X = C10.X + 50;
		SWR.Y = C10.Y - 25;
		SWR_L.X = SWR.X;
		SWR_L.Y = SWR.Y + 25;
		SWR_R.X = SWR.X + 50;
		SWR_R.Y = SWR.Y + 25;
		SWR_T.X = SWR.X + 25;
		SWR_T.Y = SWR.Y;
		HPF.X = PA.X;
		HPF.Y = ADC0.Y;
		HPF_R.X = HPF.X + 50;
		HPF_R.Y = HPF.Y + 25;
		HPF_B.X = HPF.X + 25;
		HPF_B.Y = HPF.Y + 50;
		HPF_L.X = HPF.X;
		HPF_L.Y = HPF.Y + 25;
		LPF_HPF_corner.X = HPF.X + 25;
		LPF_HPF_corner.Y = C10.Y;
		XVTR_HPF_corner.X = HPF_B.X;
		XVTR_HPF_corner.Y = C4.Y;
		EXT1_HPF_corner.X = HPF_B.X;
		EXT1_HPF_corner.Y = C5.Y;
		EXT2_HPF_corner.X = HPF_B.X;
		EXT2_HPF_corner.Y = C6.Y;
		label_HPF.Visible = true;
		label_ADC0_atten.Visible = true;
		label_ADC0.Visible = true;
		label_DDC0.Visible = true;
		label_Rx0.Visible = true;
		label_DDC1.Visible = true;
		label_Rx1.Visible = true;
		g.DrawRectangle(blackPen2, PC);
		g.DrawRectangle(indianredPen, FPGA);
		g.DrawRectangle(blackPen, DSP);
		g.DrawRectangle(blackPen, RX1_DISPLAY);
		g.DrawRectangle(blackPen, RX2_DISPLAY);
		label_RX2_DISPLAY.Visible = true;
		g.DrawRectangle(blackPen, Rx0);
		g.DrawRectangle(blackPen, Rx1);
		g.DrawRectangle(blackPen, Rx2);
		g.DrawRectangle(blackPen, Rx3);
		g.DrawRectangle(blackPen, Rx4);
		g.DrawRectangle(blackPen, Rx5);
		g.DrawRectangle(blackPen, Rx6);
		g.DrawRectangle(blackPen, ADC0);
		g.DrawRectangle(blackPen, ADC1);
		update_labels_PA();
		label_ADC1_atten.Visible = true;
		label_ADC1.Visible = true;
		g.DrawRectangle(blackPen2, PC);
		g.DrawRectangle(indianredPen, FPGA);
		g.DrawRectangle(blackPen, DSP);
		g.DrawRectangle(blackPen, RX1_DISPLAY);
		g.DrawRectangle(blackPen, Rx0);
		g.DrawRectangle(blackPen, Rx1);
		g.DrawRectangle(blackPen, Rx2);
		label_Rx2.Visible = true;
		label_DDC2.Visible = true;
		g.DrawRectangle(blackPen, Rx3);
		label_Rx3.Visible = true;
		label_DDC3.Visible = true;
		g.DrawRectangle(blackPen, Rx4);
		label_Rx4.Visible = true;
		label_DDC4.Visible = true;
		g.DrawRectangle(blackPen, Rx5);
		label_Rx5.Visible = true;
		label_DDC5.Visible = true;
		g.DrawRectangle(blackPen, Rx6);
		label_Rx6.Visible = true;
		label_DDC6.Visible = true;
		g.DrawRectangle(blackPen, ADC0);
		if (bool_Rx0_0)
		{
			ADC0_to_Rx0(bluePen);
		}
		if (bool_Rx0_1)
		{
			ADC1_to_Rx0(bluePen);
		}
		if (bool_Rx3_0)
		{
			ADC0_to_Rx3(bluePen);
		}
		if (bool_Rx3_1)
		{
			ADC1_to_Rx3(bluePen);
		}
		if (bool_Rx4_0)
		{
			ADC0_to_Rx4(bluePen);
		}
		if (bool_Rx4_1)
		{
			ADC1_to_Rx4(bluePen);
		}
		if (bool_Rx5_0)
		{
			ADC0_to_Rx5(bluePen);
		}
		if (bool_Rx5_1)
		{
			ADC1_to_Rx5(bluePen);
		}
		if (bool_Rx6_0)
		{
			ADC0_to_Rx6(bluePen);
		}
		if (bool_Rx6_1)
		{
			ADC1_to_Rx6(bluePen);
		}
		if (bool_rx)
		{
			g.DrawRectangle(blackPen, RX2_DISPLAY);
			label_RX2_DISPLAY.Visible = true;
			label_PA.Visible = false;
			label_AMP.Visible = false;
			label_FILTER.Visible = false;
			label_DAC0.Visible = false;
			label_DUC0.Visible = false;
			if (!bool_HPF_BYPASS)
			{
				g.DrawRectangle(blackPen, HPF);
				label_HPF.Visible = true;
			}
			else
			{
				g.DrawLine(bluePen, HPF_B, HPF_center);
				g.DrawLine(bluePen, HPF_center, HPF_R);
			}
			if (!bool_RX1_MUTE)
			{
				label_RX1_LR_audio.Visible = true;
			}
			else
			{
				label_RX1_LR_audio.Visible = false;
			}
			if (!bool_RX2_MUTE)
			{
				label_RX2_LR_audio.Visible = true;
			}
			else
			{
				label_RX2_LR_audio.Visible = false;
			}
			if (bool_RX1_MUTE & bool_RX2_MUTE)
			{
				label_AUDIO_AMP.Visible = false;
				label_CODEC2.Visible = false;
				label_L_audio_only.Visible = false;
				label_LR_audio.Visible = false;
				label_AUDIO_MIXER.Visible = false;
			}
			else
			{
				g.DrawRectangle(blackPen, AUDIO_MIXER);
				label_AUDIO_MIXER.Visible = true;
				g.DrawRectangle(blackPen, CODEC2);
				label_CODEC.Visible = false;
				label_AUDIO_AMP.Visible = true;
				label_CODEC2.Visible = true;
				label_L_audio_only.Visible = true;
				label_LR_audio.Visible = true;
				g.DrawRectangle(blackPen, AUDIO_AMP);
			}
			g.DrawLine(bluePen, HPF_R, ADC0_L);
			ADC1_to_RX2(bluePen);
			if (bool_diversity)
			{
				ADC1_to_Rx1(bluePen);
			}
			if (!bool_XVTR & !bool_EXT1 & !bool_EXT2 & !bool_BYPASS)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
				LPF_to_HPF_PA15(bluePen);
				if (bool_ANT1)
				{
					C9_to_LPF_L(bluePen);
				}
				if (bool_ANT2)
				{
					C10_to_LPF_L(bluePen);
				}
				if (bool_ANT3)
				{
					C11_to_LPF_L(bluePen);
				}
			}
			if (bool_XVTR & !bool_EXT1 & !bool_EXT2 & !bool_BYPASS)
			{
				label_SWR.Visible = false;
				XVTR_to_HPF_PA15(bluePen);
				label_LPF.Visible = false;
				g.DrawRectangle(blackPen, AUDIO_MIXER);
			}
			if (!bool_XVTR & bool_EXT1 & !bool_EXT2 & !bool_BYPASS)
			{
				label_SWR.Visible = false;
				EXT1_to_HPF_PA15(bluePen);
				label_LPF.Visible = false;
				g.DrawRectangle(blackPen, AUDIO_MIXER);
			}
			if (!bool_XVTR & !bool_EXT1 & bool_EXT2 & !bool_BYPASS)
			{
				label_SWR.Visible = false;
				EXT2_to_HPF_PA15(bluePen);
				label_LPF.Visible = false;
			}
			if (!bool_XVTR & !bool_EXT1 & !bool_EXT2 & bool_BYPASS)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
				LPF_to_BYPASS(bluePen);
				if (bool_ANT1)
				{
					C9_to_LPF_L(bluePen);
				}
				if (bool_ANT2)
				{
					C10_to_LPF_L(bluePen);
				}
				if (bool_ANT3)
				{
					C11_to_LPF_L(bluePen);
				}
			}
			if (bool_XVTR & !bool_EXT1 & !bool_EXT2 & bool_BYPASS)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				XVTR_to_HPF_PA15(bluePen);
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
				LPF_to_BYPASS(bluePen);
				if (bool_ANT1)
				{
					C9_to_LPF_L(bluePen);
				}
				if (bool_ANT2)
				{
					C10_to_LPF_L(bluePen);
				}
				if (bool_ANT3)
				{
					C11_to_LPF_L(bluePen);
				}
			}
			if (!bool_XVTR & bool_EXT1 & !bool_EXT2 & bool_BYPASS)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				EXT1_to_HPF_PA15(bluePen);
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
				LPF_to_BYPASS(bluePen);
				if (bool_ANT1)
				{
					C9_to_LPF_L(bluePen);
				}
				if (bool_ANT2)
				{
					C10_to_LPF_L(bluePen);
				}
				if (bool_ANT3)
				{
					C11_to_LPF_L(bluePen);
				}
			}
			if (!bool_XVTR & !bool_EXT1 & bool_EXT2 & bool_BYPASS)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				EXT2_to_HPF_PA15(bluePen);
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
				LPF_to_BYPASS(bluePen);
				if (bool_ANT1)
				{
					C9_to_LPF_L(bluePen);
				}
				if (bool_ANT2)
				{
					C10_to_LPF_L(bluePen);
				}
				if (bool_ANT3)
				{
					C11_to_LPF_L(bluePen);
				}
			}
			if (bool_Rx1_0)
			{
				ADC0_to_Rx1(bluePen);
			}
			if (bool_Rx1_1)
			{
				ADC1_to_Rx1(bluePen);
			}
			if (bool_Rx2_0)
			{
				ADC0_to_Rx2(bluePen);
			}
			if (bool_Rx2_1)
			{
				ADC1_to_Rx2(bluePen);
			}
			Rx0_to_DSP(bluePen);
			Rx1_to_DSP(bluePen);
			Rx2_to_DSP(bluePen);
			Rx3_to_DSP(bluePen);
			if (!bool_PureSignal & !bool_diversity)
			{
				DSP_Rx2_to_RX1_DISPLAY(bluePen);
				if (!bool_RX1_MUTE)
				{
					SPKR_to_RX1_DISPLAY_2(bluePen);
				}
				if (!bool_RX2_MUTE)
				{
					SPKR_to_RX2_DISPLAY_2(bluePen);
				}
			}
			if (!bool_PureSignal & bool_diversity)
			{
				DSP_out1_to_RX1_DISPLAY(bluePen);
				DSP_in1_to_out1_crossconnect(bluePen);
				draw_diversity_connection(bluePen);
				if (!bool_RX1_MUTE)
				{
					SPKR_to_RX1_DISPLAY(bluePen);
				}
				if (!bool_RX2_MUTE)
				{
					SPKR_to_RX2_DISPLAY_2(bluePen);
				}
			}
			if (bool_PureSignal & !bool_diversity)
			{
				DSP_Rx2_to_RX1_DISPLAY(bluePen);
				if (!bool_RX1_MUTE)
				{
					SPKR_to_RX1_DISPLAY_2(bluePen);
				}
				if (!bool_RX2_MUTE)
				{
					SPKR_to_RX2_DISPLAY_2(bluePen);
				}
			}
			if (bool_PureSignal & bool_diversity)
			{
				DSP_out1_to_RX1_DISPLAY(bluePen);
				DSP_in1_to_out1_crossconnect(bluePen);
				draw_diversity_connection(bluePen);
				if (!bool_RX1_MUTE)
				{
					SPKR_to_RX1_DISPLAY(bluePen);
				}
				if (!bool_RX2_MUTE)
				{
					SPKR_to_RX2_DISPLAY_2(bluePen);
				}
			}
			g.DrawRectangle(blackPen, ADC1);
		}
		else
		{
			g.DrawRectangle(blackPen, AMPF);
			g.DrawRectangle(blackPen, DAC0);
			g.DrawRectangle(blackPen, DUC0);
			if (!bool_HPF_BYPASS)
			{
				g.DrawRectangle(blackPen, HPF);
				label_HPF.Visible = true;
			}
			else
			{
				g.DrawLine(bluePen, HPF_L, HPF_center);
				g.DrawLine(bluePen, HPF_center, HPF_R);
			}
			g.DrawLine(bluePen, HPF_R, ADC0_L);
			g.DrawRectangle(blackPen, CODEC);
			label_PA.Visible = true;
			label_AMP.Visible = true;
			label_FILTER.Visible = true;
			label_DAC0.Visible = true;
			label_DUC0.Visible = true;
			label_CODEC.Visible = true;
			label_L_audio_only.Visible = false;
			label_AUDIO_AMP.Visible = false;
			label_CODEC2.Visible = false;
			label_LR_audio.Visible = false;
			label_RX1_LR_audio.Visible = false;
			label_RX2_LR_audio.Visible = false;
			label_AUDIO_MIXER.Visible = false;
			bool flag = bool_EXT1_on_TX;
			bool_EXT1_on_TX = bool_EXT2_on_TX;
			bool_EXT2_on_TX = flag;
			if (!bool_BYPASS_on_TX & !bool_EXT1_on_TX & !bool_EXT2_on_TX)
			{
				HPF_to_ground(bluePen);
			}
			if (bool_BYPASS_on_TX)
			{
				g.DrawLine(bluePen, C7, HPF_GROUND10);
			}
			if (bool_EXT1_on_TX)
			{
				C5_to_HPF_PA15_TX(bluePen);
				g.DrawLine(bluePen, C7, HPF_GROUND10);
			}
			if (bool_EXT2_on_TX)
			{
				C6_to_HPF_PA15_TX(bluePen);
				g.DrawLine(bluePen, C7, HPF_GROUND10);
			}
			ADC1_to_ground(bluePen);
			if (!bool_XVTR & !bool_EXT1 & !bool_EXT2 & !bool_BYPASS)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
			}
			if (bool_XVTR & !bool_EXT1 & !bool_EXT2 & bool_BYPASS)
			{
				label_LPF.Visible = false;
				label_PA.Visible = false;
			}
			if (!bool_XVTR & bool_EXT1 & !bool_EXT2 & bool_BYPASS)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				g.DrawRectangle(blackPen, PA);
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
				label_PA.Visible = true;
			}
			if (!bool_XVTR & !bool_EXT1 & bool_EXT2 & bool_BYPASS)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				g.DrawRectangle(blackPen, PA);
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
				label_PA.Visible = true;
				label_LPF.Visible = false;
			}
			if (!bool_PureSignal & !bool_diversity)
			{
				DSP_Rx2_to_RX1_DISPLAY(bluePen);
				if (bool_MON)
				{
					if (!bool_RX1_MUTE)
					{
						SPKR_to_RX1_DISPLAY_2(bluePen);
					}
					if (!bool_RX2_MUTE & bool_MON)
					{
						SPKR_to_RX2_DISPLAY_2(bluePen);
					}
					label_L_audio_only.Visible = true;
					label_AUDIO_AMP.Visible = true;
					label_CODEC2.Visible = true;
					label_LR_audio.Visible = true;
					label_RX1_LR_audio.Visible = true;
					label_RX2_LR_audio.Visible = true;
					label_AUDIO_MIXER.Visible = true;
					g.DrawRectangle(blackPen, CODEC2);
					g.DrawRectangle(blackPen, AUDIO_AMP);
				}
			}
			if (!bool_PureSignal & bool_diversity)
			{
				DSP_out1_to_RX1_DISPLAY(bluePen);
				DSP_in1_to_out1_crossconnect(bluePen);
				draw_diversity_connection(bluePen);
				if (bool_MON | bool_duplex)
				{
					if (!bool_RX1_MUTE)
					{
						SPKR_to_RX1_DISPLAY(bluePen);
					}
					if (!bool_RX2_MUTE & bool_MON)
					{
						SPKR_to_RX2_DISPLAY_2(bluePen);
					}
					label_L_audio_only.Visible = true;
					label_AUDIO_AMP.Visible = true;
					label_CODEC2.Visible = true;
					label_LR_audio.Visible = true;
					label_RX1_LR_audio.Visible = true;
					label_RX2_LR_audio.Visible = true;
					label_AUDIO_MIXER.Visible = true;
					g.DrawRectangle(blackPen, CODEC2);
					g.DrawRectangle(blackPen, AUDIO_AMP);
				}
			}
			if (bool_PureSignal & !bool_diversity)
			{
				DSP_Rx2_to_RX1_DISPLAY(bluePen);
				Rx2_to_DSP(bluePen);
				Rx3_to_DSP(bluePen);
				if (bool_MON)
				{
					if (!bool_RX1_MUTE)
					{
						SPKR_to_RX1_DISPLAY_2(bluePen);
					}
					if (!bool_RX2_MUTE & bool_MON)
					{
						SPKR_to_RX2_DISPLAY_2(bluePen);
					}
					label_L_audio_only.Visible = true;
					label_AUDIO_AMP.Visible = true;
					label_CODEC2.Visible = true;
					label_LR_audio.Visible = true;
					label_RX1_LR_audio.Visible = true;
					label_RX2_LR_audio.Visible = true;
					label_AUDIO_MIXER.Visible = true;
					g.DrawRectangle(blackPen, CODEC2);
					g.DrawRectangle(blackPen, AUDIO_AMP);
				}
			}
			if (bool_PureSignal & bool_diversity)
			{
				DSP_Rx2_to_RX1_DISPLAY(bluePen);
				Rx2_to_DSP(bluePen);
				Rx3_to_DSP(bluePen);
				if (bool_MON | bool_duplex)
				{
					if (!bool_RX1_MUTE)
					{
						SPKR_to_RX1_DISPLAY_2(bluePen);
					}
					if (!bool_RX2_MUTE & bool_MON)
					{
						SPKR_to_RX2_DISPLAY_2(bluePen);
					}
					label_L_audio_only.Visible = true;
					label_AUDIO_AMP.Visible = true;
					label_CODEC2.Visible = true;
					label_LR_audio.Visible = true;
					label_RX1_LR_audio.Visible = true;
					label_RX2_LR_audio.Visible = true;
					label_AUDIO_MIXER.Visible = true;
					g.DrawRectangle(blackPen, CODEC2);
					g.DrawRectangle(blackPen, AUDIO_AMP);
				}
			}
			basic_Tx_path(redPen);
			ADC0_to_Rx0(bluePen);
			Rx0_to_DSP(bluePen);
			if (!bool_PureSignal & !bool_duplex & !bool_XVTR)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				g.DrawRectangle(blackPen, PA);
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
				label_PA.Visible = true;
				PA15_to_LPF(redPen);
				AMPF_XVTR_TX(redPen);
				AMPF_to_PA15(redPen);
				if (bool_ANT1_TX)
				{
					C9_to_LPF_L(redPen);
				}
				if (bool_ANT2_TX)
				{
					C10_to_LPF_L(redPen);
				}
				if (bool_ANT3_TX)
				{
					C11_to_LPF_L(redPen);
				}
				if (bool_Rx1_0)
				{
					ADC0_to_Rx1(bluePen);
				}
				if (bool_Rx1_1)
				{
					ADC1_to_Rx1(bluePen);
				}
				if (bool_Rx2_0)
				{
					ADC0_to_Rx2(bluePen);
				}
				if (bool_Rx2_1)
				{
					ADC1_to_Rx2(bluePen);
				}
				Rx1_to_DSP(bluePen);
				Rx2_to_DSP(bluePen);
				Rx3_to_DSP(bluePen);
			}
			if (!bool_PureSignal & !bool_duplex & bool_XVTR)
			{
				label_SWR.Visible = false;
				label_LPF.Visible = false;
				label_PA.Visible = false;
				AMPF_XVTR_TX(redPen);
				if (bool_Rx1_0)
				{
					ADC0_to_Rx1(bluePen);
				}
				if (bool_Rx1_1)
				{
					ADC1_to_Rx1(bluePen);
				}
				if (bool_Rx2_0)
				{
					ADC0_to_Rx2(bluePen);
				}
				if (bool_Rx2_1)
				{
					ADC1_to_Rx2(bluePen);
				}
				Rx1_to_DSP(bluePen);
				DSP_out2_to_RX2_DISPLAY(bluePen);
				DSP_in2_to_out2_crossconnect(bluePen);
			}
			if (!bool_PureSignal & bool_duplex & bool_XVTR)
			{
				label_SWR.Visible = false;
				label_LPF.Visible = false;
				label_PA.Visible = false;
				AMPF_XVTR_TX(redPen);
				if (bool_Rx1_0)
				{
					ADC0_to_Rx1(bluePen);
				}
				if (bool_Rx1_1)
				{
					ADC1_to_Rx1(bluePen);
				}
				if (bool_Rx2_0)
				{
					ADC0_to_Rx2(bluePen);
				}
				if (bool_Rx2_1)
				{
					ADC1_to_Rx2(bluePen);
				}
				DSP_in1_to_out1_crossconnect(bluePen);
				DSP_out1_to_RX1_DISPLAY(bluePen);
				Rx1_to_DSP(bluePen);
				DSP_out2_to_RX2_DISPLAY(bluePen);
				DSP_in2_to_out2_crossconnect(bluePen);
				Rx1_to_DSP(bluePen);
				Rx2_to_DSP(bluePen);
				Rx3_to_DSP(bluePen);
				if (!bool_RX1_MUTE)
				{
					SPKR_to_RX1_DISPLAY_2(bluePen);
				}
				if (!bool_RX2_MUTE)
				{
					SPKR_to_RX2_DISPLAY_2(bluePen);
				}
				label_L_audio_only.Visible = true;
				label_AUDIO_AMP.Visible = true;
				label_CODEC2.Visible = true;
				label_LR_audio.Visible = true;
				label_RX1_LR_audio.Visible = true;
				label_RX2_LR_audio.Visible = true;
				label_AUDIO_MIXER.Visible = true;
				g.DrawRectangle(blackPen, CODEC2);
				g.DrawRectangle(blackPen, AUDIO_AMP);
			}
			if (!bool_PureSignal & bool_duplex & !bool_XVTR)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				g.DrawRectangle(blackPen, PA);
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
				label_PA.Visible = true;
				AMPF_XVTR_TX(redPen);
				PA15_to_LPF(redPen);
				g.DrawLine(redPen, PA_corner, PA_B);
				if (bool_ANT1_TX)
				{
					C9_to_LPF_L(redPen);
				}
				if (bool_ANT2_TX)
				{
					C10_to_LPF_L(redPen);
				}
				if (bool_ANT3_TX)
				{
					C11_to_LPF_L(redPen);
				}
				if (bool_Rx1_0)
				{
					ADC0_to_Rx1(bluePen);
				}
				if (bool_Rx1_1)
				{
					ADC1_to_Rx1(bluePen);
				}
				if (bool_Rx2_0)
				{
					ADC0_to_Rx2(bluePen);
				}
				if (bool_Rx2_1)
				{
					ADC1_to_Rx2(bluePen);
				}
				Rx1_to_DSP(bluePen);
				Rx2_to_DSP(bluePen);
				Rx3_to_DSP(bluePen);
				if (!bool_RX1_MUTE)
				{
					SPKR_to_RX1_DISPLAY_2(bluePen);
				}
				if (!bool_RX2_MUTE)
				{
					SPKR_to_RX2_DISPLAY_2(bluePen);
				}
				label_L_audio_only.Visible = true;
				label_AUDIO_AMP.Visible = true;
				label_CODEC2.Visible = true;
				label_LR_audio.Visible = true;
				label_RX1_LR_audio.Visible = true;
				label_RX2_LR_audio.Visible = true;
				label_AUDIO_MIXER.Visible = true;
				g.DrawRectangle(blackPen, CODEC2);
				g.DrawRectangle(blackPen, AUDIO_AMP);
			}
			if (bool_PureSignal & !bool_duplex & !bool_XVTR)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				g.DrawRectangle(blackPen, PA);
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
				label_PA.Visible = true;
				AMPF_XVTR_TX(redPen);
				PA15_to_LPF(redPen);
				g.DrawLine(redPen, PA_corner, PA_B);
				Rx1_to_DSP(redPen);
				DUC0_to_Rx1(redPen);
				ADC1_to_Rx2(bluePen);
				if (bool_ANT1_TX)
				{
					C9_to_LPF_L(redPen);
				}
				if (bool_ANT2_TX)
				{
					C10_to_LPF_L(redPen);
				}
				if (bool_ANT3_TX)
				{
					C11_to_LPF_L(redPen);
				}
			}
			if (bool_PureSignal & bool_duplex & !bool_XVTR)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				g.DrawRectangle(blackPen, PA);
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
				label_PA.Visible = true;
				AMPF_XVTR_TX(redPen);
				PA15_to_LPF(redPen);
				g.DrawLine(redPen, PA_corner, PA_B);
				Rx1_to_DSP(redPen);
				DUC0_to_Rx1(redPen);
				ADC1_to_Rx2(bluePen);
				if (bool_ANT1_TX)
				{
					C9_to_LPF_L(redPen);
				}
				if (bool_ANT2_TX)
				{
					C10_to_LPF_L(redPen);
				}
				if (bool_ANT3_TX)
				{
					C11_to_LPF_L(redPen);
				}
				Rx1_to_DSP(bluePen);
				Rx2_to_DSP(bluePen);
				Rx3_to_DSP(bluePen);
				if (!bool_RX1_MUTE)
				{
					SPKR_to_RX1_DISPLAY_2(bluePen);
				}
				if (!bool_RX2_MUTE)
				{
					SPKR_to_RX2_DISPLAY_2(bluePen);
				}
				label_L_audio_only.Visible = true;
				label_AUDIO_AMP.Visible = true;
				label_CODEC2.Visible = true;
				label_LR_audio.Visible = true;
				label_RX1_LR_audio.Visible = true;
				label_RX2_LR_audio.Visible = true;
				label_AUDIO_MIXER.Visible = true;
				g.DrawRectangle(blackPen, CODEC2);
				g.DrawRectangle(blackPen, AUDIO_AMP);
			}
			if (bool_PureSignal & !bool_duplex & bool_XVTR)
			{
				label_SWR.Visible = false;
				label_LPF.Visible = false;
				label_PA.Visible = false;
				Rx1_to_DSP(redPen);
				DUC0_to_Rx1(redPen);
				AMPF_XVTR_TX(redPen);
				ADC1_to_Rx2(bluePen);
			}
			if (bool_PureSignal & bool_duplex & bool_XVTR)
			{
				ADC1_to_Rx2(bluePen);
				Rx1_to_DSP(bluePen);
				Rx2_to_DSP(bluePen);
				Rx3_to_DSP(bluePen);
				if (!bool_RX1_MUTE)
				{
					SPKR_to_RX1_DISPLAY_2(bluePen);
				}
				if (!bool_RX2_MUTE)
				{
					SPKR_to_RX2_DISPLAY_2(bluePen);
				}
				label_L_audio_only.Visible = true;
				label_AUDIO_AMP.Visible = true;
				label_CODEC2.Visible = true;
				label_LR_audio.Visible = true;
				label_RX1_LR_audio.Visible = true;
				label_RX2_LR_audio.Visible = true;
				label_AUDIO_MIXER.Visible = true;
				g.DrawRectangle(blackPen, CODEC2);
				g.DrawRectangle(blackPen, AUDIO_AMP);
				if (!(bool_RX1_MUTE & bool_RX2_MUTE))
				{
					label_SWR.Visible = false;
					label_LPF.Visible = false;
					label_PA.Visible = false;
					DUC0_to_Rx1(redPen);
					Rx1_to_DSP(redPen);
					AMPF_XVTR_TX(redPen);
				}
			}
		}
		g.DrawRectangle(blackPen, SDR);
		update_diagram = false;
	}

	private void draw_ANAN_100D_PA_rev24()
	{
		hide_all_labels();
		label_hardware_selected.Text = "Routing for ANAN-100D, ANAN-200D (PA rev_24)";
		label_SDR_Hardware.Visible = true;
		label_FPGA.Visible = true;
		label_PC.Visible = true;
		label_DSP.Visible = true;
		label_RX1_DISPLAY.Visible = true;
		if (bool_XVTR)
		{
			label_XVTR_VHF.Visible = true;
		}
		label_NOTE.Visible = true;
		label_UNCHECK.Visible = true;
		hide_controls();
		Update_control_settings();
		LPF.X = C10.X + 110;
		LPF.Y = C10.Y - 25;
		LPF_L.X = LPF.X;
		LPF_L.Y = LPF.Y + 25;
		LPF_L_c.X = LPF.X - 20;
		LPF_L_c.Y = LPF_L.Y;
		LPF_R.X = LPF.X + 50;
		LPF_R.Y = LPF.Y + 25;
		LPF_R_c.X = LPF.Y + 25;
		LPF_T.X = LPF.X + 25;
		LPF_T.Y = LPF.Y;
		LPF_BYPASS_corner.X = LPF.X + 25;
		LPF_BYPASS_corner.Y = C7.Y;
		SWR.X = C10.X + 50;
		SWR.Y = C10.Y - 25;
		SWR_L.X = SWR.X;
		SWR_L.Y = SWR.Y + 25;
		SWR_R.X = SWR.X + 50;
		SWR_R.Y = SWR.Y + 25;
		SWR_T.X = SWR.X + 25;
		SWR_T.Y = SWR.Y;
		HPF.X = PA.X;
		HPF.Y = ADC0.Y;
		HPF_R.X = HPF.X + 50;
		HPF_R.Y = HPF.Y + 25;
		HPF_B.X = HPF.X + 25;
		HPF_B.Y = HPF.Y + 50;
		HPF_L.X = HPF.X;
		HPF_L.Y = HPF.Y + 25;
		LPF_HPF_corner.X = HPF.X + 25;
		LPF_HPF_corner.Y = C10.Y;
		XVTR_HPF_corner.X = HPF_B.X;
		XVTR_HPF_corner.Y = C4.Y;
		EXT1_HPF_corner.X = HPF_B.X;
		EXT1_HPF_corner.Y = C5.Y;
		EXT2_HPF_corner.X = HPF_B.X;
		EXT2_HPF_corner.Y = C6.Y;
		update_labels_PA();
		label_HPF.Visible = false;
		label_ADC0_atten.Visible = true;
		label_ADC0.Visible = true;
		label_DDC0.Visible = true;
		label_Rx0.Visible = true;
		label_DDC1.Visible = true;
		label_Rx1.Visible = true;
		label_ADC1_atten.Visible = true;
		label_ADC1.Visible = true;
		g.DrawRectangle(blackPen2, PC);
		g.DrawRectangle(indianredPen, FPGA);
		g.DrawRectangle(blackPen, DSP);
		g.DrawRectangle(blackPen, RX1_DISPLAY);
		g.DrawRectangle(blackPen, RX2_DISPLAY);
		label_RX2_DISPLAY.Visible = true;
		g.DrawRectangle(blackPen, Rx0);
		g.DrawRectangle(blackPen, Rx1);
		g.DrawRectangle(blackPen, Rx2);
		label_Rx2.Visible = true;
		label_DDC2.Visible = true;
		g.DrawRectangle(blackPen, Rx3);
		label_Rx3.Visible = true;
		label_DDC3.Visible = true;
		g.DrawRectangle(blackPen, Rx4);
		label_Rx4.Visible = true;
		label_DDC4.Visible = true;
		g.DrawRectangle(blackPen, Rx5);
		label_Rx5.Visible = true;
		label_DDC5.Visible = true;
		g.DrawRectangle(blackPen, Rx6);
		label_Rx6.Visible = true;
		label_DDC6.Visible = true;
		g.DrawRectangle(blackPen, ADC0);
		g.DrawRectangle(blackPen, ADC1);
		ADC1_to_RX2(bluePen);
		if (bool_Rx0_0)
		{
			ADC0_to_Rx0(bluePen);
		}
		if (bool_Rx0_1)
		{
			ADC1_to_Rx0(bluePen);
		}
		if (bool_Rx3_0)
		{
			ADC0_to_Rx3(bluePen);
		}
		if (bool_Rx3_1)
		{
			ADC1_to_Rx3(bluePen);
		}
		if (bool_Rx4_0)
		{
			ADC0_to_Rx4(bluePen);
		}
		if (bool_Rx4_1)
		{
			ADC1_to_Rx4(bluePen);
		}
		if (bool_Rx5_0)
		{
			ADC0_to_Rx5(bluePen);
		}
		if (bool_Rx5_1)
		{
			ADC1_to_Rx5(bluePen);
		}
		if (bool_Rx6_0)
		{
			ADC0_to_Rx6(bluePen);
		}
		if (bool_Rx6_1)
		{
			ADC1_to_Rx6(bluePen);
		}
		if (bool_rx)
		{
			label_PA.Visible = false;
			label_AMP.Visible = false;
			label_FILTER.Visible = false;
			label_DAC0.Visible = false;
			label_DUC0.Visible = false;
			if (!bool_RX1_MUTE)
			{
				label_RX1_LR_audio.Visible = true;
			}
			else
			{
				label_RX1_LR_audio.Visible = false;
			}
			if (!bool_RX2_MUTE)
			{
				label_RX2_LR_audio.Visible = true;
			}
			else
			{
				label_RX2_LR_audio.Visible = false;
			}
			if (bool_RX1_MUTE & bool_RX2_MUTE)
			{
				label_AUDIO_AMP.Visible = false;
				label_CODEC2.Visible = false;
				label_L_audio_only.Visible = false;
				label_LR_audio.Visible = false;
				label_AUDIO_MIXER.Visible = false;
			}
			else
			{
				g.DrawRectangle(blackPen, AUDIO_MIXER);
				label_AUDIO_MIXER.Visible = true;
				g.DrawRectangle(blackPen, CODEC2);
				label_CODEC.Visible = false;
				label_AUDIO_AMP.Visible = true;
				label_CODEC2.Visible = true;
				label_L_audio_only.Visible = true;
				label_LR_audio.Visible = true;
				g.DrawRectangle(blackPen, AUDIO_AMP);
			}
			g.DrawLine(bluePen, HPF_R, ADC0_L);
			if (!bool_XVTR & !bool_EXT1 & !bool_EXT2 & !bool_BYPASS)
			{
				if (bool_HPF_BYPASS)
				{
					g.DrawLine(bluePen, HPF_B, HPF_center);
					g.DrawLine(bluePen, HPF_center, HPF_R);
				}
				else
				{
					g.DrawRectangle(blackPen, HPF);
					label_HPF.Visible = true;
				}
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
				LPF_to_HPF_PA15(bluePen);
				if (bool_ANT1)
				{
					C9_to_LPF_L(bluePen);
				}
				if (bool_ANT2)
				{
					C10_to_LPF_L(bluePen);
				}
				if (bool_ANT3)
				{
					C11_to_LPF_L(bluePen);
				}
			}
			if (bool_XVTR & !bool_EXT1 & !bool_EXT2)
			{
				g.DrawRectangle(blackPen, HPF);
				label_HPF.Visible = true;
				XVTR_to_HPF_PA15(bluePen);
				g.DrawRectangle(blackPen, AUDIO_MIXER);
			}
			if (!bool_XVTR & bool_EXT1 & !bool_EXT2)
			{
				g.DrawRectangle(blackPen, HPF);
				label_HPF.Visible = true;
				EXT1_to_HPF_PA15(bluePen);
				g.DrawRectangle(blackPen, AUDIO_MIXER);
			}
			if (!bool_XVTR & !bool_EXT1 & bool_EXT2)
			{
				g.DrawRectangle(blackPen, HPF);
				label_HPF.Visible = true;
				EXT2_to_HPF_PA15(bluePen);
			}
			if (bool_Rx1_0)
			{
				ADC0_to_Rx1(bluePen);
			}
			if (bool_Rx1_1)
			{
				ADC1_to_Rx1(bluePen);
			}
			if (bool_Rx2_0)
			{
				ADC0_to_Rx2(bluePen);
			}
			if (bool_Rx2_1)
			{
				ADC1_to_Rx2(bluePen);
			}
			Rx0_to_DSP(bluePen);
			Rx1_to_DSP(bluePen);
			Rx2_to_DSP(bluePen);
			Rx3_to_DSP(bluePen);
			if (!bool_PureSignal & !bool_diversity)
			{
				DSP_Rx2_to_RX1_DISPLAY(bluePen);
				if (!bool_RX1_MUTE)
				{
					SPKR_to_RX1_DISPLAY_2(bluePen);
				}
				if (!bool_RX2_MUTE)
				{
					SPKR_to_RX2_DISPLAY_2(bluePen);
				}
				label_L_audio_only.Visible = true;
				label_AUDIO_AMP.Visible = true;
				label_CODEC2.Visible = true;
				label_LR_audio.Visible = true;
				label_RX1_LR_audio.Visible = true;
				label_RX2_LR_audio.Visible = true;
				label_AUDIO_MIXER.Visible = true;
				g.DrawRectangle(blackPen, CODEC2);
				g.DrawRectangle(blackPen, AUDIO_AMP);
			}
			if (!bool_PureSignal & bool_diversity)
			{
				DSP_out1_to_RX1_DISPLAY(bluePen);
				DSP_in1_to_out1_crossconnect(bluePen);
				draw_diversity_connection(bluePen);
				if (!bool_RX1_MUTE)
				{
					SPKR_to_RX1_DISPLAY(bluePen);
				}
				if (!bool_RX2_MUTE)
				{
					SPKR_to_RX2_DISPLAY_2(bluePen);
				}
				label_L_audio_only.Visible = true;
				label_AUDIO_AMP.Visible = true;
				label_CODEC2.Visible = true;
				label_LR_audio.Visible = true;
				label_RX1_LR_audio.Visible = true;
				label_RX2_LR_audio.Visible = true;
				label_AUDIO_MIXER.Visible = true;
				g.DrawRectangle(blackPen, CODEC2);
				g.DrawRectangle(blackPen, AUDIO_AMP);
			}
			if (bool_PureSignal & !bool_diversity)
			{
				DSP_Rx2_to_RX1_DISPLAY(bluePen);
				Rx2_to_DSP(bluePen);
				Rx3_to_DSP(bluePen);
				if (!bool_RX1_MUTE)
				{
					SPKR_to_RX1_DISPLAY_2(bluePen);
				}
				if (!bool_RX2_MUTE)
				{
					SPKR_to_RX2_DISPLAY_2(bluePen);
				}
				label_L_audio_only.Visible = true;
				label_AUDIO_AMP.Visible = true;
				label_CODEC2.Visible = true;
				label_LR_audio.Visible = true;
				label_RX1_LR_audio.Visible = true;
				label_RX2_LR_audio.Visible = true;
				label_AUDIO_MIXER.Visible = true;
				g.DrawRectangle(blackPen, CODEC2);
				g.DrawRectangle(blackPen, AUDIO_AMP);
			}
			if (bool_PureSignal & bool_diversity)
			{
				DSP_out1_to_RX1_DISPLAY(bluePen);
				DSP_in1_to_out1_crossconnect(bluePen);
				draw_diversity_connection(bluePen);
				if (!bool_RX1_MUTE)
				{
					SPKR_to_RX1_DISPLAY(bluePen);
				}
				if (!bool_RX2_MUTE)
				{
					SPKR_to_RX2_DISPLAY_2(bluePen);
				}
			}
			g.DrawRectangle(blackPen, ADC1);
		}
		else
		{
			if (bool_EXT1_on_TX)
			{
				bool_EXT1_on_TX = false;
			}
			if (bool_EXT2_on_TX)
			{
				bool_EXT2_on_TX = false;
			}
			g.DrawRectangle(blackPen, AMPF);
			g.DrawRectangle(blackPen, DAC0);
			g.DrawRectangle(blackPen, DUC0);
			g.DrawRectangle(blackPen, CODEC);
			label_PA.Visible = true;
			label_AMP.Visible = true;
			label_FILTER.Visible = true;
			label_DAC0.Visible = true;
			label_DUC0.Visible = true;
			label_CODEC.Visible = true;
			label_L_audio_only.Visible = false;
			label_AUDIO_AMP.Visible = false;
			label_CODEC2.Visible = false;
			label_LR_audio.Visible = false;
			label_RX1_LR_audio.Visible = false;
			label_RX2_LR_audio.Visible = false;
			label_AUDIO_MIXER.Visible = false;
			if (bool_EXT1_on_TX || bool_EXT2_on_TX)
			{
				return;
			}
			if (bool_BYPASS_on_TX)
			{
				BYPASS_to_ADC0(bluePen);
			}
			if (!bool_EXT1_on_TX & !bool_EXT2_on_TX & !bool_BYPASS_on_TX)
			{
				SWR_to_ADC0(bluePen);
			}
			if (!bool_XVTR & !bool_EXT1 & !bool_EXT2 & !bool_BYPASS)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
			}
			if (!bool_XVTR & !bool_EXT1 & !bool_EXT2 & bool_BYPASS)
			{
				BYPASS_to_ADC0(bluePen);
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
			}
			if ((bool_XVTR & !bool_EXT1 & !bool_EXT2 & bool_BYPASS) || (!bool_XVTR & bool_EXT1 & !bool_EXT2 & bool_BYPASS) || (!bool_XVTR & !bool_EXT1 & bool_EXT2 & bool_BYPASS))
			{
				return;
			}
			if (!bool_PureSignal & !bool_diversity)
			{
				DSP_Rx2_to_RX1_DISPLAY(bluePen);
				if (bool_MON)
				{
					if (!bool_RX1_MUTE)
					{
						SPKR_to_RX1_DISPLAY_2(bluePen);
					}
					if (!bool_RX2_MUTE)
					{
						SPKR_to_RX2_DISPLAY_2(bluePen);
					}
					label_L_audio_only.Visible = true;
					label_AUDIO_AMP.Visible = true;
					label_CODEC2.Visible = true;
					label_LR_audio.Visible = true;
					label_RX1_LR_audio.Visible = true;
					label_RX2_LR_audio.Visible = true;
					label_AUDIO_MIXER.Visible = true;
					g.DrawRectangle(blackPen, CODEC2);
					g.DrawRectangle(blackPen, AUDIO_AMP);
				}
			}
			if (!bool_PureSignal & bool_diversity)
			{
				DSP_out1_to_RX1_DISPLAY(bluePen);
				DSP_in1_to_out1_crossconnect(bluePen);
				draw_diversity_connection(bluePen);
				if (bool_MON | bool_duplex)
				{
					if (!bool_RX1_MUTE)
					{
						SPKR_to_RX1_DISPLAY(bluePen);
					}
					if (!bool_RX2_MUTE)
					{
						SPKR_to_RX2_DISPLAY_2(bluePen);
					}
					label_L_audio_only.Visible = true;
					label_AUDIO_AMP.Visible = true;
					label_CODEC2.Visible = true;
					label_LR_audio.Visible = true;
					label_RX1_LR_audio.Visible = true;
					label_RX2_LR_audio.Visible = true;
					label_AUDIO_MIXER.Visible = true;
					g.DrawRectangle(blackPen, CODEC2);
					g.DrawRectangle(blackPen, AUDIO_AMP);
				}
			}
			if (bool_PureSignal & !bool_diversity)
			{
				DSP_Rx2_to_RX1_DISPLAY(bluePen);
				Rx2_to_DSP(bluePen);
				Rx3_to_DSP(bluePen);
				if (bool_MON)
				{
					if (!bool_RX1_MUTE)
					{
						SPKR_to_RX1_DISPLAY_2(bluePen);
					}
					if (!bool_RX2_MUTE)
					{
						SPKR_to_RX2_DISPLAY_2(bluePen);
					}
					label_L_audio_only.Visible = true;
					label_AUDIO_AMP.Visible = true;
					label_CODEC2.Visible = true;
					label_LR_audio.Visible = true;
					label_RX1_LR_audio.Visible = true;
					label_RX2_LR_audio.Visible = true;
					label_AUDIO_MIXER.Visible = true;
					g.DrawRectangle(blackPen, CODEC2);
					g.DrawRectangle(blackPen, AUDIO_AMP);
				}
			}
			if (bool_PureSignal & bool_diversity)
			{
				DSP_Rx2_to_RX1_DISPLAY(bluePen);
				Rx2_to_DSP(bluePen);
				Rx3_to_DSP(bluePen);
				if (bool_MON | bool_duplex)
				{
					if (!bool_RX1_MUTE)
					{
						SPKR_to_RX1_DISPLAY_2(bluePen);
					}
					if (!bool_RX2_MUTE)
					{
						SPKR_to_RX2_DISPLAY_2(bluePen);
					}
					label_L_audio_only.Visible = true;
					label_AUDIO_AMP.Visible = true;
					label_CODEC2.Visible = true;
					label_LR_audio.Visible = true;
					label_RX1_LR_audio.Visible = true;
					label_RX2_LR_audio.Visible = true;
					label_AUDIO_MIXER.Visible = true;
					g.DrawRectangle(blackPen, CODEC2);
					g.DrawRectangle(blackPen, AUDIO_AMP);
				}
			}
			basic_Tx_path(redPen);
			ADC0_to_Rx0(bluePen);
			Rx0_to_DSP(bluePen);
			if (!bool_PureSignal & !bool_duplex & !bool_XVTR)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				g.DrawRectangle(blackPen, PA);
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
				label_PA.Visible = true;
				PA15_to_LPF(redPen);
				AMPF_XVTR_TX(redPen);
				AMPF_to_PA15(redPen);
				if (bool_ANT1_TX)
				{
					C9_to_LPF_L(redPen);
				}
				if (bool_ANT2_TX)
				{
					C10_to_LPF_L(redPen);
				}
				if (bool_ANT3_TX)
				{
					C11_to_LPF_L(redPen);
				}
				if (bool_Rx1_0)
				{
					ADC0_to_Rx1(bluePen);
				}
				if (bool_Rx1_1)
				{
					ADC1_to_Rx1(bluePen);
				}
				if (bool_Rx2_0)
				{
					ADC0_to_Rx2(bluePen);
				}
				if (bool_Rx2_1)
				{
					ADC1_to_Rx2(bluePen);
				}
				Rx1_to_DSP(bluePen);
				Rx2_to_DSP(bluePen);
				Rx3_to_DSP(bluePen);
			}
			if (!bool_PureSignal & !bool_duplex & bool_XVTR)
			{
				label_SWR.Visible = false;
				label_LPF.Visible = false;
				label_PA.Visible = false;
				AMPF_XVTR_TX(redPen);
				if (bool_Rx1_0)
				{
					ADC0_to_Rx1(bluePen);
				}
				if (bool_Rx1_1)
				{
					ADC1_to_Rx1(bluePen);
				}
				if (bool_Rx2_0)
				{
					ADC0_to_Rx2(bluePen);
				}
				if (bool_Rx2_1)
				{
					ADC1_to_Rx2(bluePen);
				}
				Rx1_to_DSP(bluePen);
				DSP_out2_to_RX2_DISPLAY(bluePen);
				DSP_in2_to_out2_crossconnect(bluePen);
			}
			if (!bool_PureSignal & bool_duplex & bool_XVTR)
			{
				label_SWR.Visible = false;
				label_LPF.Visible = false;
				label_PA.Visible = false;
				AMPF_XVTR_TX(redPen);
				if (bool_Rx1_0)
				{
					ADC0_to_Rx1(bluePen);
				}
				if (bool_Rx1_1)
				{
					ADC1_to_Rx1(bluePen);
				}
				if (bool_Rx2_0)
				{
					ADC0_to_Rx2(bluePen);
				}
				if (bool_Rx2_1)
				{
					ADC1_to_Rx2(bluePen);
				}
				DSP_in1_to_out1_crossconnect(bluePen);
				DSP_out1_to_RX1_DISPLAY(bluePen);
				Rx1_to_DSP(bluePen);
				DSP_out2_to_RX2_DISPLAY(bluePen);
				DSP_in2_to_out2_crossconnect(bluePen);
				if (!bool_RX1_MUTE)
				{
					SPKR_to_RX1_DISPLAY_2(bluePen);
				}
				if (!bool_RX2_MUTE)
				{
					SPKR_to_RX2_DISPLAY_2(bluePen);
				}
				label_L_audio_only.Visible = true;
				label_AUDIO_AMP.Visible = true;
				label_CODEC2.Visible = true;
				label_LR_audio.Visible = true;
				label_RX1_LR_audio.Visible = true;
				label_RX2_LR_audio.Visible = true;
				label_AUDIO_MIXER.Visible = true;
				g.DrawRectangle(blackPen, CODEC2);
				g.DrawRectangle(blackPen, AUDIO_AMP);
			}
			if (!bool_PureSignal & bool_duplex & !bool_XVTR)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				g.DrawRectangle(blackPen, PA);
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
				label_PA.Visible = true;
				PA15_to_LPF(redPen);
				AMPF_XVTR_TX(redPen);
				AMPF_to_PA15(redPen);
				if (bool_ANT1_TX)
				{
					C9_to_LPF_L(redPen);
				}
				if (bool_ANT2_TX)
				{
					C10_to_LPF_L(redPen);
				}
				if (bool_ANT3_TX)
				{
					C11_to_LPF_L(redPen);
				}
				if (bool_Rx1_0)
				{
					ADC0_to_Rx1(bluePen);
				}
				if (bool_Rx1_1)
				{
					ADC1_to_Rx1(bluePen);
				}
				if (bool_Rx2_0)
				{
					ADC0_to_Rx2(bluePen);
				}
				if (bool_Rx2_1)
				{
					ADC1_to_Rx2(bluePen);
				}
				Rx1_to_DSP(bluePen);
				Rx2_to_DSP(bluePen);
				Rx3_to_DSP(bluePen);
				if (!bool_RX1_MUTE)
				{
					SPKR_to_RX1_DISPLAY_2(bluePen);
				}
				if (!bool_RX2_MUTE)
				{
					SPKR_to_RX2_DISPLAY_2(bluePen);
				}
				label_L_audio_only.Visible = true;
				label_AUDIO_AMP.Visible = true;
				label_CODEC2.Visible = true;
				label_LR_audio.Visible = true;
				label_RX1_LR_audio.Visible = true;
				label_RX2_LR_audio.Visible = true;
				label_AUDIO_MIXER.Visible = true;
				g.DrawRectangle(blackPen, CODEC2);
				g.DrawRectangle(blackPen, AUDIO_AMP);
			}
			if (bool_PureSignal & !bool_duplex & !bool_XVTR)
			{
				if (!bool_BYPASS)
				{
					SWR_to_ADC0(bluePen);
				}
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				g.DrawRectangle(blackPen, PA);
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
				label_PA.Visible = true;
				AMPF_XVTR_TX(redPen);
				PA15_to_LPF(redPen);
				g.DrawLine(redPen, PA_corner, PA_B);
				Rx1_to_DSP(redPen);
				DUC0_to_Rx1(redPen);
				ADC1_to_Rx2(bluePen);
				if (bool_ANT1_TX)
				{
					C9_to_LPF_L(redPen);
				}
				if (bool_ANT2_TX)
				{
					C10_to_LPF_L(redPen);
				}
				if (bool_ANT3_TX)
				{
					C11_to_LPF_L(redPen);
				}
			}
			if (bool_PureSignal & bool_duplex & !bool_XVTR)
			{
				g.DrawRectangle(blackPen, SWR);
				label_SWR.Visible = true;
				g.DrawRectangle(blackPen, PA);
				g.DrawRectangle(blackPen, LPF);
				label_LPF.Visible = true;
				label_PA.Visible = true;
				AMPF_XVTR_TX(redPen);
				PA15_to_LPF(redPen);
				g.DrawLine(redPen, PA_corner, PA_B);
				Rx1_to_DSP(redPen);
				DUC0_to_Rx1(redPen);
				ADC1_to_Rx2(bluePen);
				if (bool_ANT1_TX)
				{
					C9_to_LPF_L(redPen);
				}
				if (bool_ANT2_TX)
				{
					C10_to_LPF_L(redPen);
				}
				if (bool_ANT3_TX)
				{
					C11_to_LPF_L(redPen);
				}
				if (!bool_RX1_MUTE)
				{
					SPKR_to_RX1_DISPLAY_2(bluePen);
				}
				if (!bool_RX2_MUTE)
				{
					SPKR_to_RX2_DISPLAY_2(bluePen);
				}
				label_L_audio_only.Visible = true;
				label_AUDIO_AMP.Visible = true;
				label_CODEC2.Visible = true;
				label_LR_audio.Visible = true;
				label_RX1_LR_audio.Visible = true;
				label_RX2_LR_audio.Visible = true;
				label_AUDIO_MIXER.Visible = true;
				g.DrawRectangle(blackPen, CODEC2);
				g.DrawRectangle(blackPen, AUDIO_AMP);
			}
			if (bool_PureSignal & !bool_duplex & bool_XVTR)
			{
				label_SWR.Visible = false;
				label_LPF.Visible = false;
				label_PA.Visible = false;
				Rx1_to_DSP(redPen);
				DUC0_to_Rx1(redPen);
				AMPF_XVTR_TX(redPen);
				ADC1_to_Rx2(bluePen);
			}
			if (bool_PureSignal & bool_duplex & bool_XVTR)
			{
				ADC1_to_Rx2(bluePen);
				if (!(bool_RX1_MUTE & bool_RX2_MUTE))
				{
					label_SWR.Visible = false;
					label_LPF.Visible = false;
					label_PA.Visible = false;
					DUC0_to_Rx1(redPen);
					Rx1_to_DSP(redPen);
					AMPF_XVTR_TX(redPen);
				}
			}
		}
		g.DrawRectangle(blackPen, SDR);
		update_diagram = false;
	}

	private void ADC0_to_Rx0(Pen pen)
	{
		g.DrawLine(pen, ADC0_R, Rx0_L);
	}

	private void ADC0_to_Rx1(Pen pen)
	{
		g.DrawLine(pen, ADC0_R, ADC0_corner1_Rx1);
		g.DrawLine(pen, ADC0_corner1_Rx1, ADC0_corner2_Rx1);
		g.DrawLine(pen, ADC0_corner2_Rx1, Rx1_L);
	}

	private void ADC0_to_Rx2(Pen pen)
	{
		g.DrawLine(pen, ADC0_R, ADC0_corner1_Rx2);
		g.DrawLine(pen, ADC0_corner1_Rx2, ADC0_corner2_Rx2);
		g.DrawLine(pen, ADC0_corner2_Rx2, Rx2_L);
	}

	private void ADC0_to_Rx3(Pen pen)
	{
		g.DrawLine(pen, ADC0_R, ADC0_corner1_Rx1);
		g.DrawLine(pen, ADC0_corner1_Rx3, ADC0_corner2_Rx3);
		g.DrawLine(pen, ADC0_corner2_Rx3, Rx3_L);
	}

	private void ADC0_to_Rx4(Pen pen)
	{
		g.DrawLine(pen, ADC0_R, ADC0_corner1_Rx4);
		g.DrawLine(pen, ADC0_corner1_Rx4, ADC0_corner2_Rx4);
		g.DrawLine(pen, ADC0_corner2_Rx4, Rx4_L);
	}

	private void ADC0_to_Rx5(Pen pen)
	{
		g.DrawLine(pen, ADC0_R, ADC0_corner1_Rx5);
		g.DrawLine(pen, ADC0_corner1_Rx5, ADC0_corner2_Rx5);
		g.DrawLine(pen, ADC0_corner2_Rx5, Rx5_L);
	}

	private void ADC0_to_Rx6(Pen pen)
	{
		g.DrawLine(pen, ADC0_R, ADC0_corner1_Rx6);
		g.DrawLine(pen, ADC0_corner1_Rx6, ADC0_corner2_Rx6);
		g.DrawLine(pen, ADC0_corner2_Rx6, Rx6_L);
	}

	private void ADC1_to_ground(Pen pen)
	{
		g.DrawLine(pen, ADC1_L, ADC1_L_corner3);
		g.DrawLine(pen, HPF_GROUND9, HPF_GROUND2);
		g.DrawLine(pen, HPF_GROUND3, HPF_GROUND4);
		g.DrawLine(pen, HPF_GROUND5, HPF_GROUND6);
		g.DrawLine(pen, HPF_GROUND7, HPF_GROUND8);
	}

	private void ADC1_to_Rx0(Pen pen)
	{
		g.DrawLine(pen, ADC1_R, ADC1_corner1_Rx0);
		g.DrawLine(pen, ADC1_corner1_Rx0, ADC1_corner2_Rx0);
		g.DrawLine(pen, ADC1_corner2_Rx0, Rx0_ADC1_input);
	}

	private void ADC1_to_Rx1(Pen pen)
	{
		g.DrawLine(pen, ADC1_R, ADC1_corner1_Rx1);
		g.DrawLine(pen, ADC1_corner1_Rx1, ADC1_corner2_Rx1);
		g.DrawLine(pen, ADC1_corner2_Rx1, Rx1_L);
	}

	private void ADC1_to_Rx2(Pen pen)
	{
		g.DrawLine(pen, ADC1_R, ADC1_corner1_Rx2);
		g.DrawLine(pen, ADC1_corner1_Rx2, ADC1_corner2_Rx2);
		g.DrawLine(pen, ADC1_corner2_Rx2, Rx2_L);
	}

	private void ADC1_to_Rx3(Pen pen)
	{
		g.DrawLine(pen, ADC1_R, ADC1_corner1_Rx3);
		g.DrawLine(pen, ADC1_corner1_Rx3, ADC1_corner2_Rx3);
		g.DrawLine(pen, ADC1_corner2_Rx3, Rx3_L);
	}

	private void ADC1_to_Rx4(Pen pen)
	{
		g.DrawLine(pen, ADC1_R, ADC1_corner1_Rx4);
		g.DrawLine(pen, ADC1_corner1_Rx4, ADC1_corner2_Rx4);
		g.DrawLine(pen, ADC1_corner2_Rx4, Rx4_L);
	}

	private void ADC1_to_Rx5(Pen pen)
	{
		g.DrawLine(pen, ADC1_R, ADC1_corner1_Rx5);
		g.DrawLine(pen, ADC1_corner1_Rx5, ADC1_corner2_Rx5);
		g.DrawLine(pen, ADC1_corner2_Rx5, Rx5_L);
	}

	private void ADC1_to_Rx6(Pen pen)
	{
		g.DrawLine(pen, ADC1_R, ADC1_corner1_Rx6);
		g.DrawLine(pen, ADC1_corner1_Rx6, ADC1_corner2_Rx6);
		g.DrawLine(pen, ADC1_corner2_Rx6, Rx6_L);
	}

	private void ADC1_to_RX2(Pen pen)
	{
		g.DrawLine(pen, ADC1_L, ADC1_L_corner1);
		g.DrawLine(pen, ADC1_L_corner1, ADC1_L_corner2);
		g.DrawLine(pen, ADC1_L_corner2, C2);
	}

	private void ALEX_ANT_to_HPF_B(Pen pen)
	{
		if (bool_ANT1)
		{
			g.DrawLine(pen, ALEX_ANT1, ALEX_ANT1_corner);
			g.DrawLine(pen, ALEX_ANT1_corner, ALEX_HPF_B);
		}
		if (bool_ANT2)
		{
			g.DrawLine(pen, ALEX_ANT2, ALEX_ANT2_corner);
			g.DrawLine(pen, ALEX_ANT2_corner, ALEX_HPF_B);
		}
		if (bool_ANT3)
		{
			g.DrawLine(pen, ALEX_ANT3, ALEX_ANT3_corner);
			g.DrawLine(pen, ALEX_ANT3_corner, ALEX_HPF_B);
		}
	}

	private void ALEX_2_ANT_to_HPF_B(Pen pen)
	{
		if (bool_ANT1)
		{
			g.DrawLine(pen, ALEX_2_ANT1, ALEX_2_ANT1_corner);
			g.DrawLine(pen, ALEX_2_ANT1_corner, ALEX_2_HPF_B);
		}
		if (bool_ANT2)
		{
			g.DrawLine(pen, ALEX_2_ANT2, ALEX_2_ANT2_corner);
			g.DrawLine(pen, ALEX_2_ANT2_corner, ALEX_2_HPF_B);
		}
		if (bool_ANT3)
		{
			g.DrawLine(pen, ALEX_2_ANT3, ALEX_2_ANT3_corner);
			g.DrawLine(pen, ALEX_2_ANT3_corner, ALEX_2_HPF_B);
		}
	}

	private void ALEX_TX_ANT(Pen pen)
	{
		if (bool_ANT1_TX)
		{
			g.DrawLine(pen, PENELOPE_PA_out_corner3, ALEX_LPF_corner1);
			g.DrawLine(pen, ALEX_LPF_corner1, ALEX_LPF_ANT1);
		}
		if (bool_ANT2_TX)
		{
			g.DrawLine(pen, PENELOPE_PA_out_corner3, ALEX_LPF_corner2);
			g.DrawLine(pen, ALEX_LPF_corner2, ALEX_LPF_ANT2);
		}
		if (bool_ANT3_TX)
		{
			g.DrawLine(pen, PENELOPE_PA_out_corner3, ALEX_LPF_corner3);
			g.DrawLine(pen, ALEX_LPF_corner3, ALEX_LPF_ANT3);
		}
	}

	private void ALEX_2_RX_out_to_ADC(Pen pen)
	{
		g.DrawLine(pen, ALEX_2_RX_out, ALEX_2_RX_out_corner1);
		g.DrawLine(pen, ALEX_2_RX_out_corner1, MERCURY_2_ADC_in_corner1);
		g.DrawLine(pen, MERCURY_2_ADC_in_corner1, MERCURY_2_ADC_in);
	}

	private void AMPF_TX_path_PA10(Pen pen)
	{
		g.DrawLine(pen, AMPF_L, TX_AMP_corner);
		g.DrawLine(pen, TX_AMP_corner, TX_corner_PA10);
		g.DrawLine(pen, TX_corner_PA10, C6);
	}

	private void AMPF_to_PA15(Pen pen)
	{
		g.DrawLine(pen, AMPF_L, AMPF_L_PA15);
		g.DrawLine(pen, AMPF_L_PA15, PA_B);
	}

	private void AMPF_TX_path_PA15(Pen pen)
	{
		g.DrawLine(pen, AMPF_L, TX_AMP_corner);
		g.DrawLine(pen, TX_AMP_corner, TX_corner_PA15);
		g.DrawLine(pen, TX_corner_PA15, C13);
	}

	private void AMPF_XVTR_TX(Pen pen)
	{
		g.DrawLine(pen, AMPF_L, TX_AMP_corner);
		g.DrawLine(pen, TX_AMP_corner, TX_corner_PA15);
		g.DrawLine(pen, TX_corner_PA15, C13);
	}

	private void basic_Tx_path(Pen pen)
	{
		g.DrawLine(pen, MIC_R, CODEC_L);
		g.DrawLine(pen, CODEC_R, DSP_bottom_corner);
		g.DrawLine(pen, DSP_bottom_corner, DSP_B_2);
		g.DrawLine(pen, DSP_B_2, DSP_loopback_1);
		g.DrawLine(pen, DSP_loopback_1, DSP_loopback_2);
		g.DrawLine(pen, DSP_loopback_2, DSP_B_1);
		g.DrawLine(pen, DSP_B_1, DUC0_R_corner);
		g.DrawLine(pen, DUC0_R_corner, DUC0_R);
		g.DrawLine(pen, DUC0_L, DAC0_R);
		g.DrawLine(pen, DAC0_L, AMPF_R);
		g.DrawLine(pen, AMPF_L, PA_corner);
	}

	private void BYPASS_to_ADC0(Pen pen)
	{
		g.DrawLine(pen, C7, BYPASS_corner1);
		g.DrawLine(pen, BYPASS_corner1, BYPASS_corner2);
		g.DrawLine(pen, BYPASS_corner2, ADC0_L);
	}

	private void C2_to_Rx0(Pen pen)
	{
		g.DrawLine(pen, C2, ADC0_L);
	}

	private void C3_to_Rx0(Pen pen)
	{
		g.DrawLine(pen, C3, C3_corner2);
		g.DrawLine(pen, C3_corner2, C3_corner3);
		g.DrawLine(pen, C3_corner3, ADC0_L);
	}

	private void C4_to_HPF_PA15_TX(Pen pen)
	{
		g.DrawLine(pen, C4, HPF_TX_corner1);
		g.DrawLine(pen, HPF_TX_corner1, HPF_TX_corner2);
		g.DrawLine(pen, HPF_TX_corner2, HPF_L);
	}

	private void C4_to_Rx0(Pen pen)
	{
		g.DrawLine(pen, C4, C4_corner2);
		g.DrawLine(pen, C4_corner2, C4_corner3);
		g.DrawLine(pen, C4_corner3, ADC0_L);
	}

	private void C2_to_LPF(Pen pen)
	{
		g.DrawLine(pen, C2, C2_c);
		g.DrawLine(pen, C2_c, C2_corner);
		g.DrawLine(pen, C2_corner, LPF_in_corner);
		g.DrawLine(pen, LPF_in_corner, LPF_L);
	}

	private void C3_to_LPF(Pen pen)
	{
		g.DrawLine(pen, C3, C3_c);
		g.DrawLine(pen, C3_c, C3_corner);
		g.DrawLine(pen, C3_corner, LPF_in_corner);
		g.DrawLine(pen, LPF_in_corner, LPF_L);
	}

	private void C4_to_LPF(Pen pen)
	{
		g.DrawLine(pen, C4, C4_c);
		g.DrawLine(pen, C4_c, C4_corner);
		g.DrawLine(pen, C4_corner, LPF_in_corner);
		g.DrawLine(pen, LPF_in_corner, LPF_L);
	}

	private void C5_to_ADC0(Pen pen)
	{
		g.DrawLine(pen, C5, C5_corner);
		g.DrawLine(pen, C5_corner, C5_riser);
		g.DrawLine(pen, C5_riser, ADC0_L);
	}

	private void C5_to_HPF_PA15_TX(Pen pen)
	{
		g.DrawLine(pen, C5, HPF_TX_corner3);
		g.DrawLine(pen, HPF_TX_corner3, HPF_TX_corner2);
		g.DrawLine(pen, HPF_TX_corner2, HPF_L);
	}

	private void C6_to_HPF_PA15_TX(Pen pen)
	{
		g.DrawLine(pen, C6, HPF_TX_corner6);
		g.DrawLine(pen, HPF_TX_corner6, HPF_TX_corner5);
		g.DrawLine(pen, HPF_TX_corner5, HPF_L);
	}

	private void C7_to_ground(Pen pen)
	{
		g.DrawLine(bluePen, C7, HPF_GROUND10);
		g.DrawLine(bluePen, HPF_GROUND10, HPF_GROUND2);
		g.DrawLine(bluePen, HPF_GROUND3, HPF_GROUND4);
		g.DrawLine(bluePen, HPF_GROUND5, HPF_GROUND6);
		g.DrawLine(bluePen, HPF_GROUND7, HPF_GROUND8);
	}

	private void C9_to_LPF_L(Pen pen)
	{
		g.DrawLine(pen, C9, C9_c);
		g.DrawLine(pen, C9_c, C10_c);
		g.DrawLine(pen, C10_c, SWR_L);
		g.DrawLine(pen, SWR_R, LPF_L);
	}

	private void C10_to_LPF_L(Pen pen)
	{
		g.DrawLine(pen, C10, SWR_L);
		g.DrawLine(pen, SWR_R, LPF_L);
	}

	private void C11_to_LPF_L(Pen pen)
	{
		g.DrawLine(pen, C11, C11_c);
		g.DrawLine(pen, C11_c, C10_c);
		g.DrawLine(pen, C10_c, SWR_L);
		g.DrawLine(pen, SWR_R, LPF_L);
	}

	private void CODEC2_to_AUDIO_MIXER(Pen pen)
	{
		g.DrawLine(pen, CODEC2_R, AUDIO_MIXER_external_corner);
		g.DrawLine(pen, AUDIO_MIXER_external_corner, AUDIO_MIXER_B);
	}

	private void draw_diversity_connection(Pen pen)
	{
		g.DrawLine(pen, DSP_L_2, DSP_internal_diversity_corner1);
		g.DrawLine(pen, DSP_internal_diversity_corner1, DSP_internal_diversity_corner2);
	}

	private void DSP_in1_to_out1_crossconnect(Pen pen)
	{
		g.DrawLine(pen, DSP_L_1, DSP_R_1);
	}

	private void DSP_in2_to_out2_crossconnect(Pen pen)
	{
		g.DrawLine(pen, DSP_L_2, DSP_R_2);
	}

	private void DSP_out1_to_RX1_DISPLAY(Pen pen)
	{
		g.DrawLine(pen, DSP_R_1, DSP_R_1_c);
		g.DrawLine(pen, DSP_R_1_c, RX1_DISPLAY_corner1);
		g.DrawLine(pen, RX1_DISPLAY_corner1, RX1_DISPLAY_L);
	}

	private void DSP_out2_to_RX2_DISPLAY(Pen pen)
	{
		g.DrawLine(pen, DSP_R_2, RX2_L_DSP_2);
	}

	private void DUC0_to_Rx1(Pen pen)
	{
		g.DrawLine(pen, DUC0_L_corner_lower_pt, DUC0_L_corner);
		g.DrawLine(pen, DUC0_L_corner, Rx1_L);
	}

	private void EXT1_to_HPF_PA15(Pen pen)
	{
		g.DrawLine(pen, C5, EXT1_HPF_corner);
		g.DrawLine(pen, EXT1_HPF_corner, HPF_B);
	}

	private void EXT2_to_HPF_PA15(Pen pen)
	{
		g.DrawLine(pen, C6, EXT2_HPF_corner);
		g.DrawLine(pen, EXT2_HPF_corner, HPF_B);
	}

	private void HEADPHONES_to_CODEC2(Pen pen)
	{
		g.DrawLine(pen, HEADPHONES, HEADPHONES_1);
		g.DrawLine(pen, HEADPHONES_1, HEADPHONES_2);
		g.DrawLine(pen, HEADPHONES_2, CODEC2_L);
	}

	private void HERMES_PA_to_ALEX_LPF(Pen pen)
	{
		g.DrawLine(pen, PA_T, HERMES_PA_corner1);
		g.DrawLine(pen, HERMES_PA_corner1, PENELOPE_PA_out_corner1);
		g.DrawLine(pen, PENELOPE_PA_out_corner1, PENELOPE_PA_out_corner2);
		g.DrawLine(pen, PENELOPE_PA_out_corner2, PENELOPE_PA_out_corner3);
	}

	private void HPF_to_ground(Pen pen)
	{
		HPF_L.X = HPF.X;
		HPF_L.Y = HPF.Y + 25;
		g.DrawLine(pen, HPF_L, HPF_GROUND1);
		g.DrawLine(pen, HPF_GROUND1, HPF_GROUND2);
		g.DrawLine(pen, HPF_GROUND3, HPF_GROUND4);
		g.DrawLine(pen, HPF_GROUND5, HPF_GROUND6);
		g.DrawLine(pen, HPF_GROUND7, HPF_GROUND8);
	}

	private void HPSDR_DSP_to_AUDIO_MIXER_input_1(Pen pen)
	{
		g.DrawLine(pen, AUDIO_MIXER_L_1, DSP_internal_MIXER1_1);
		g.DrawLine(pen, DSP_internal_MIXER1_1, DSP_internal_MIXER1_3);
	}

	private void HPSDR_DSP_to_AUDIO_MIXER_input_2(Pen pen)
	{
		g.DrawLine(pen, AUDIO_MIXER_L_2, DSP_internal_MIXER2_1);
		g.DrawLine(pen, DSP_internal_MIXER2_1, DSP_internal_MIXER2_3);
	}

	private void line_to_ground(Pen pen)
	{
		g.DrawLine(pen, GROUND1, GROUND2);
		g.DrawLine(pen, GROUND3, GROUND4);
		g.DrawLine(pen, GROUND5, GROUND6);
		g.DrawLine(pen, GROUND7, GROUND8);
	}

	private void loopback_to_RX1_DISPLAY(Pen pen)
	{
		g.DrawLine(pen, loopback_center, loopback_center2);
		g.DrawLine(pen, loopback_center2, DSP_R_1_c);
		g.DrawLine(pen, DSP_R_1_c, RX1_DISPLAY_corner1);
		g.DrawLine(pen, RX1_DISPLAY_corner1, RX1_DISPLAY_L);
	}

	private void LPF_to_ADC0(Pen pen)
	{
		g.DrawLine(pen, LPF_R, LPF_R_c);
		g.DrawLine(pen, LPF_R_c, C5_riser);
		g.DrawLine(pen, C5_riser, ADC0_L);
	}

	private void LPF_to_C2(Pen pen)
	{
		g.DrawLine(pen, LPF_T, LPF_corner_C2);
		g.DrawLine(pen, LPF_corner_C2, C2);
	}

	private void LPF_to_C3(Pen pen)
	{
		g.DrawLine(pen, LPF_T, LPF_corner_C3);
		g.DrawLine(pen, LPF_corner_C3, C3);
	}

	private void LPF_to_C4(Pen pen)
	{
		g.DrawLine(pen, LPF_T, LPF_corner_C4);
		g.DrawLine(pen, LPF_corner_C4, C4);
	}

	private void LPF_to_HPF_PA15(Pen pen)
	{
		g.DrawLine(pen, LPF_R, LPF_HPF_corner);
		g.DrawLine(pen, LPF_HPF_corner, HPF_B);
	}

	private void MERCURY_ADC_to_DDCs(Pen pen)
	{
		g.DrawLine(pen, MERCURY_ADC_out, MERCURY_DDC0_in);
		g.DrawLine(pen, MERCURY_FPGA_corner1, MERCURY_FPGA_corner2);
		g.DrawLine(pen, MERCURY_FPGA_corner2, MERCURY_DDC1_in);
		g.DrawLine(pen, MERCURY_FPGA_corner2, MERCURY_FPGA_corner3);
		g.DrawLine(pen, MERCURY_FPGA_corner3, MERCURY_DDC2_in);
	}

	private void MERCURY_DDC0_to_RX1_DISPLAY(Pen pen)
	{
		g.DrawLine(pen, MERCURY_DDC0_out, RX1_DISPLAY_HPSDR_DDC0);
	}

	private void MERCURY_DDC1_to_RX2_DISPLAY(Pen pen)
	{
		g.DrawLine(pen, MERCURY_DDC1_out, RX2_DISPLAY_HPSDR_DDC1);
	}

	private void MERCURY_2_ADC_to_DDCs(Pen pen)
	{
		g.DrawLine(pen, MERCURY_2_ADC_out, MERCURY_2_DDC0_in);
		g.DrawLine(pen, MERCURY_2_FPGA_corner1, MERCURY_2_FPGA_corner2);
		g.DrawLine(pen, MERCURY_2_FPGA_corner2, MERCURY_2_DDC1_in);
		g.DrawLine(pen, MERCURY_2_FPGA_corner2, MERCURY_2_FPGA_corner3);
		g.DrawLine(pen, MERCURY_2_FPGA_corner3, MERCURY_2_DDC2_in);
	}

	private void MERCURY_2_DDC0_to_METIS_diversity(Pen pen)
	{
		g.DrawLine(pen, MERCURY_2_DDC0_out, MERCURY_2_DDC0_out_corner1);
		g.DrawLine(pen, MERCURY_2_DDC0_out_corner1, MERCURY_2_DDC0_out_corner2);
		g.DrawLine(pen, MERCURY_2_DDC0_out_corner2, METIS_corner1);
		g.DrawLine(pen, METIS_corner1, METIS_corner2);
	}

	private void MERCURY_2_DDC0_to_RX2_DISPLAY(Pen pen)
	{
		g.DrawLine(pen, MERCURY_2_DDC0_out, MERCURY_2_DDC0_out_corner1);
		g.DrawLine(pen, MERCURY_2_DDC0_out_corner1, MERCURY_2_DDC0_out_corner2);
		g.DrawLine(pen, MERCURY_2_DDC0_out_corner2, RX2_DISPLAY_HPSDR_DDC1);
	}

	private void MERCURY_2_AUDIO_INPUT(Pen pen)
	{
		g.DrawLine(pen, MERCURY_2_CODEC_out, MERCURY_2_CODEC_corner1);
		g.DrawLine(pen, MERCURY_2_CODEC_corner1, MERCURY_2_CODEC_corner2);
		g.DrawLine(pen, MERCURY_2_CODEC_LINEOUT, MERCURY_2_CODEC_OUT1);
		g.DrawLine(pen, MERCURY_2_CODEC_PHONES, MERCURY_2_CODEC_OUT2);
	}

	private void MERCURY_RX1_to_AUDIO_MIXER(Pen pen)
	{
		g.DrawLine(pen, MERCURY_CODEC_LINEOUT, MERCURY_CODEC_OUT1);
		g.DrawLine(pen, MERCURY_CODEC_PHONES, MERCURY_CODEC_OUT2);
		g.DrawLine(pen, MERCURY_CODEC_IN, MERCURY_CODEC_corner1);
		g.DrawLine(pen, MERCURY_CODEC_corner1, MERCURY_CODEC_corner2);
		g.DrawLine(pen, MERCURY_CODEC_corner2, AUDIO_MIXER_external_corner2);
		g.DrawLine(pen, AUDIO_MIXER_external_corner2, AUDIO_MIXER_internal_corner1);
		g.DrawLine(pen, AUDIO_MIXER_internal_corner1, AUDIO_MIXER_L_1);
	}

	private void MERCURY_RX2_to_AUDIO_MIXER(Pen pen)
	{
		g.DrawLine(pen, MERCURY_CODEC_LINEOUT, MERCURY_CODEC_OUT1);
		g.DrawLine(pen, MERCURY_CODEC_PHONES, MERCURY_CODEC_OUT2);
		g.DrawLine(pen, MERCURY_CODEC_IN, MERCURY_CODEC_corner1);
		g.DrawLine(pen, MERCURY_CODEC_corner1, MERCURY_CODEC_corner2);
		g.DrawLine(pen, MERCURY_CODEC_corner2, AUDIO_MIXER_external_corner2);
		g.DrawLine(pen, AUDIO_MIXER_external_corner2, AUDIO_MIXER_internal_corner2);
		g.DrawLine(pen, AUDIO_MIXER_internal_corner2, AUDIO_MIXER_L_2);
	}

	private void PA_to_LPF(Pen pen)
	{
		g.DrawLine(pen, PA_T, LPF_B);
	}

	private void LPF_to_BYPASS(Pen pen)
	{
		g.DrawLine(pen, LPF_T, LPF_BYPASS_corner);
		g.DrawLine(pen, LPF_BYPASS_corner, C7);
	}

	private void PA15_to_LPF(Pen pen)
	{
		LPF_R.X = LPF.X + 50;
		LPF_R.Y = LPF.Y + 25;
		PA15_corner3.X = PA_T.X;
		PA15_corner3.Y = LPF_R.Y;
		g.DrawLine(pen, PA_T, PA15_corner3);
		g.DrawLine(pen, PA15_corner3, LPF_R);
	}

	private void PENELOPE_PA_to_ALEX_LPF(Pen pen)
	{
		g.DrawLine(pen, PENELOPE_PA_out, PENELOPE_PA_out_corner1);
		g.DrawLine(pen, PENELOPE_PA_out_corner1, PENELOPE_PA_out_corner2);
		g.DrawLine(pen, PENELOPE_PA_out_corner2, PENELOPE_PA_out_corner3);
	}

	private void PENELOPE_PA_to_DSP(Pen pen)
	{
		g.DrawLine(pen, PENELOPE_PA_R, PENELOPE_AMPF_L);
		g.DrawLine(pen, PENELOPE_AMPF_R, PENELOPE_DAC_L);
		g.DrawLine(pen, PENELOPE_DAC_R, PENELOPE_DUC_L);
		g.DrawLine(pen, PENELOPE_DUC_R, PC_PENELOPE_corner1);
		g.DrawLine(pen, PC_PENELOPE_corner1, PC_PENELOPE_corner2);
		g.DrawLine(pen, PC_PENELOPE_corner2, PC_PENELOPE_corner3);
		g.DrawLine(pen, PC_PENELOPE_corner3, PC_PENELOPE_corner4);
		g.DrawLine(pen, PENELOPE_PA_XVTR_1, PENELOPE_PA_XVTR_2);
		g.DrawLine(pen, PENELOPE_PA_XVTR_2, PENELOPE_PA_XVTR_3);
	}

	private void PENELOPE_DSP_to_CODEC(Pen pen)
	{
		g.DrawLine(pen, PC_PENELOPE_corner4, PENELOPE_CODEC_R);
		g.DrawLine(pen, PENELOPE_CODEC_MIC_1, PENELOPE_CODEC_MIC_2);
		g.DrawLine(pen, PENELOPE_CODEC_LINE_IN_1, PENELOPE_CODEC_LINE_IN_2);
	}

	private void Rx0_to_DSP(Pen pen)
	{
		g.DrawLine(pen, Rx0_R, DSP_L_corner_Rx0);
		g.DrawLine(pen, DSP_L_corner_Rx0, DSP_L_corner2_Rx0);
		g.DrawLine(pen, DSP_L_corner2_Rx0, DSP_L_1);
	}

	private void Rx1_to_DSP(Pen pen)
	{
		g.DrawLine(pen, Rx1_R, DSP_L_2);
	}

	private void Rx2_to_DSP(Pen pen)
	{
		g.DrawLine(pen, Rx2_R, DSP_L_3);
	}

	private void DSP_Rx2_to_RX1_DISPLAY(Pen pen)
	{
		g.DrawLine(pen, DSP_L_3, DSP_R_3);
		g.DrawLine(pen, DSP_R_3, DSP_R_3_RX1_corner);
		g.DrawLine(pen, DSP_R_3_RX1_corner, RX1_DISPLAY_L);
	}

	private void Rx3_to_DSP(Pen pen)
	{
		g.DrawLine(pen, Rx3_R, DSP_L_4);
		g.DrawLine(pen, DSP_L_4, DSP_R_4);
		g.DrawLine(pen, DSP_R_4, DSP_R_4_RX2_corner);
		g.DrawLine(pen, DSP_R_4_RX2_corner, RX2_DISPLAY_L);
	}

	private void SPKR_to_DSP1(Pen pen)
	{
		g.DrawLine(pen, C24, SPKR_corner1);
		g.DrawLine(pen, SPKR_corner1, SPKR_DSP1);
		g.DrawLine(pen, SPKR_DSP1, DSP_R_1);
	}

	private void SPKR_to_DSP2(Pen pen)
	{
		g.DrawLine(pen, C24, SPKR_corner1);
		g.DrawLine(pen, SPKR_corner1, SPKR_DSP2);
		g.DrawLine(pen, SPKR_DSP2, DSP_R_2);
	}

	private void SPKR_to_RX1_DISPLAY(Pen pen)
	{
		g.DrawLine(pen, C24, AUDIO_AMP_L);
		g.DrawLine(pen, AUDIO_AMP_R, CODEC2_L);
		HEADPHONES_to_CODEC2(bluePen);
		CODEC2_to_AUDIO_MIXER(bluePen);
		if (!bool_RX1_MUTE)
		{
			g.DrawLine(pen, DSP_MIXER_1, AUDIO_MIXER_L_1);
			g.DrawLine(pen, AUDIO_MIXER_B, AUDIO_MIXER_internal_corner1);
			g.DrawLine(pen, AUDIO_MIXER_internal_corner1, AUDIO_MIXER_L_1);
			g.DrawLine(pen, AUDIO_MIXER_L_1, DSP_internal_MIXER1_1);
			g.DrawLine(pen, DSP_internal_MIXER1_1, DSP_internal_MIXER1_2);
		}
	}

	private void SPKR_to_RX1_DISPLAY_2(Pen pen)
	{
		g.DrawLine(pen, C24, AUDIO_AMP_L);
		g.DrawLine(pen, AUDIO_AMP_R, CODEC2_L);
		HEADPHONES_to_CODEC2(bluePen);
		CODEC2_to_AUDIO_MIXER(bluePen);
		if (!bool_RX1_MUTE)
		{
			g.DrawLine(pen, DSP_MIXER_1, AUDIO_MIXER_L_1);
			g.DrawLine(pen, AUDIO_MIXER_B, AUDIO_MIXER_internal_corner1);
			g.DrawLine(pen, AUDIO_MIXER_internal_corner1, AUDIO_MIXER_L_1);
			g.DrawLine(pen, AUDIO_MIXER_L_1, DSP_internal_MIXER1_1);
			g.DrawLine(pen, DSP_internal_MIXER1_1, DSP_internal_Rx2_audio_connection);
		}
	}

	private void SPKR_to_RX2_DISPLAY(Pen pen)
	{
		g.DrawLine(pen, C24, AUDIO_AMP_L);
		g.DrawLine(pen, AUDIO_AMP_R, CODEC2_L);
		HEADPHONES_to_CODEC2(bluePen);
		CODEC2_to_AUDIO_MIXER(bluePen);
		if (!bool_RX2_MUTE)
		{
			g.DrawLine(pen, DSP_MIXER_2, AUDIO_MIXER_L_2);
			g.DrawLine(pen, AUDIO_MIXER_B, AUDIO_MIXER_internal_corner2);
			g.DrawLine(pen, AUDIO_MIXER_internal_corner2, AUDIO_MIXER_L_2);
			g.DrawLine(pen, AUDIO_MIXER_L_2, DSP_internal_MIXER2_1);
			g.DrawLine(pen, DSP_internal_MIXER2_1, DSP_internal_MIXER2_2);
		}
	}

	private void SPKR_to_RX2_DISPLAY_2(Pen pen)
	{
		g.DrawLine(pen, C24, AUDIO_AMP_L);
		g.DrawLine(pen, AUDIO_AMP_R, CODEC2_L);
		HEADPHONES_to_CODEC2(bluePen);
		CODEC2_to_AUDIO_MIXER(bluePen);
		if (!bool_RX2_MUTE)
		{
			g.DrawLine(pen, DSP_MIXER_2, AUDIO_MIXER_L_2);
			g.DrawLine(pen, AUDIO_MIXER_B, AUDIO_MIXER_internal_corner2);
			g.DrawLine(pen, AUDIO_MIXER_internal_corner2, AUDIO_MIXER_L_2);
			g.DrawLine(pen, AUDIO_MIXER_L_2, DSP_internal_MIXER2_1);
			g.DrawLine(pen, DSP_internal_MIXER2_1, DSP_internal_Rx3_audio_connection);
		}
	}

	private void SWR_to_ADC0(Pen pen)
	{
		g.DrawLine(pen, SWR_T, SWR_corner_ADC0);
		g.DrawLine(pen, SWR_corner_ADC0, ADC0_L);
	}

	private void update_labels_PA10()
	{
		hide_rear_panel_labels();
		label_rear_panel.Location = new Point(8, 40);
		label_C2.Text = "ANT1";
		C2_label.X = C2.X - 45;
		C2_label.Y = C2.Y - 6;
		label_C2.Location = C2_label;
		label_C2.Visible = true;
		label_C3.Text = "ANT2";
		C3_label.X = C3.X - 45;
		C3_label.Y = C3.Y - 6;
		label_C3.Location = C3_label;
		label_C3.Visible = true;
		label_C4.Text = "ANT3";
		C4_label.X = C4.X - 45;
		C4_label.Y = C4.Y - 6;
		label_C4.Location = C4_label;
		label_C4.Visible = true;
		label_C5.Text = "RX (SMA)";
		C5_label.X = C5.X - 65;
		C5_label.Y = C5.Y - 6;
		label_C5.Location = C5_label;
		label_C5.Visible = true;
		label_C6.Text = "TX (SMA)";
		C6_label.X = C6.X - 65;
		C6_label.Y = C6.Y - 6;
		label_C6.Location = C6_label;
		label_C6.Visible = true;
		label_C7.Text = "XVRT (SMA)";
		C7_label.X = C7.X - 80;
		C7_label.Y = C7.Y - 6;
		label_C7.Location = C7_label;
		label_C7.Visible = true;
		label_C24.Text = "SPKR";
		C24_label.X = C24.X - 45;
		C24_label.Y = C24.Y - 6;
		label_C24.Location = C24_label;
		label_C26.Text = "MIC";
		C26_label.X = C26.X - 35;
		C26_label.Y = C26.Y - 6;
		label_C26.Location = C26_label;
		label_C25.Text = "HDPHONES";
		C25_label.X = C25.X - 75;
		C25_label.Y = C25.Y - 6;
		label_C25.Location = C25_label;
		LPF_label.X = LPF.X + 10;
		LPF_label.Y = LPF.Y + 17;
		label_LPF.Location = LPF_label;
		label_SWR.Visible = false;
	}

	private void update_labels_PA()
	{
		hide_rear_panel_labels();
		label_rear_panel.Location = new Point(8, 40);
		label_C2.Text = "RX 2";
		C2_label.X = C2.X - 35;
		C2_label.Y = C2.Y - 6;
		label_C2.Location = C2_label;
		label_C2.Visible = true;
		label_C4.Text = "XVRT RX";
		C4_label.X = C4.X - 60;
		C4_label.Y = C4.Y - 5;
		label_C4.Location = C4_label;
		label_C4.Visible = true;
		label_C5.Text = "EXT1";
		C5_label.X = C5.X - 40;
		C5_label.Y = C5.Y - 5;
		label_C5.Location = C5_label;
		label_C5.Visible = true;
		label_C6.Text = "EXT2";
		C6_label.X = C6.X - 40;
		C6_label.Y = C6.Y - 5;
		label_C6.Location = C6_label;
		label_C6.Visible = true;
		label_C7.Text = "BYPASS";
		C7_label.X = C7.X - 57;
		C7_label.Y = C7.Y - 5;
		label_C7.Location = C7_label;
		label_C7.Visible = true;
		label_C9.Text = "ANT1";
		C9_label.X = C9.X - 40;
		C9_label.Y = C9.Y - 5;
		label_C9.Location = C9_label;
		label_C9.Visible = true;
		label_C10.Text = "ANT2";
		C10_label.X = C10.X - 41;
		C10_label.Y = C10.Y - 5;
		label_C10.Location = C10_label;
		label_C10.Visible = true;
		label_C11.Text = "ANT3";
		C11_label.X = C11.X - 42;
		C11_label.Y = C11.Y - 5;
		label_C11.Location = C11_label;
		label_C11.Visible = true;
		label_C13.Text = "XVRT TX";
		C13_label.X = C13.X - 60;
		C13_label.Y = C13.Y - 5;
		label_C13.Location = C13_label;
		label_C13.Visible = true;
		label_C24.Text = "SPKR";
		C24_label.X = C24.X - 45;
		C24_label.Y = C24.Y - 6;
		label_C24.Location = C24_label;
		label_C24.Visible = true;
		label_C25.Text = "HDPHONES";
		C25_label.X = C25.X - 75;
		C25_label.Y = C25.Y - 6;
		label_C25.Location = C25_label;
		label_C25.Visible = true;
		label_C26.Text = "MIC";
		C26_label.X = C26.X - 35;
		C26_label.Y = C26.Y - 6;
		label_C26.Location = C26_label;
		label_C26.Visible = true;
		LPF_label.X = LPF.X + 10;
		LPF_label.Y = LPF.Y + 17;
		label_LPF.Location = LPF_label;
		HPF_label.X = HPF.X + 10;
		HPF_label.Y = HPF.Y + 17;
		label_HPF.Location = HPF_label;
	}

	private void XVTR_to_HPF_PA15(Pen pen)
	{
		g.DrawLine(pen, C4, XVTR_HPF_corner);
		g.DrawLine(pen, XVTR_HPF_corner, HPF_B);
	}

	public void pi_Changed()
	{
		do_platform_prep();
		update_diagram = true;
		canvas.Invalidate();
	}

	private void do_platform_prep()
	{
		if (bool_HPSDR)
		{
			draw_HPSDR();
		}
		if (bool_HERMES)
		{
			draw_HERMES();
		}
		if (bool_ANAN_10E)
		{
			draw_ANAN_10E();
		}
		if (bool_ANAN_100_PA_rev15)
		{
			draw_ANAN_100_PA_rev15();
		}
		if (bool_ANAN_100_PA_rev24)
		{
			draw_ANAN_100_PA_rev24();
		}
		if (bool_ANAN_100D_PA_rev15)
		{
			draw_ANAN_100D_PA_rev15();
		}
		if (bool_ANAN_100D_PA_rev24)
		{
			draw_ANAN_100D_PA_rev24();
		}
	}

	private void hide_all_labels()
	{
		label_ADC0.Visible = false;
		label_ADC0_atten.Visible = false;
		label_ADC1.Visible = false;
		label_ADC1_atten.Visible = false;
		label_ADC2.Visible = false;
		label_ADC2_atten.Visible = false;
		label_ALEX.Visible = false;
		label_ALEX_HPF.Visible = false;
		label_ALEX_LPF.Visible = false;
		label_ALEX_To_RX.Visible = false;
		label_ALEX_2.Visible = false;
		label_ALEX_2_HPF.Visible = false;
		label_ALEX_2_LPF.Visible = false;
		label_ALEX_2_To_RX.Visible = false;
		label_AMP.Visible = false;
		label_AUDIO_AMP.Visible = false;
		label_AUDIO_MIXER.Visible = false;
		label_C1.Visible = false;
		label_C2.Visible = false;
		label_C3.Visible = false;
		label_C4.Visible = false;
		label_C5.Visible = false;
		label_C6.Visible = false;
		label_C7.Visible = false;
		label_C8.Visible = false;
		label_C9.Visible = false;
		label_C10.Visible = false;
		label_C11.Visible = false;
		label_SMA.Visible = false;
		label_C12.Visible = false;
		label_C13.Visible = false;
		label_C14.Visible = false;
		label_C15.Visible = false;
		label_C16.Visible = false;
		label_C17.Visible = false;
		label_C18.Visible = false;
		label_C19.Visible = false;
		label_C20.Visible = false;
		label_C20.Visible = false;
		label_C24.Visible = false;
		label_C25.Visible = false;
		label_C26.Visible = false;
		label_CODEC.Visible = false;
		label_CODEC2.Visible = false;
		label_DAC0.Visible = false;
		label_DDC0.Visible = false;
		label_DDC1.Visible = false;
		label_DDC2.Visible = false;
		label_DDC3.Visible = false;
		label_DDC4.Visible = false;
		label_DDC5.Visible = false;
		label_DDC6.Visible = false;
		label_DUAL_MERCURY.Visible = false;
		label_DSP.Visible = false;
		label_DSP_HPSDR.Visible = false;
		label_DUC0.Visible = false;
		label_ext_amp1.Visible = false;
		label_ext_amp2.Visible = false;
		label_FILTER.Visible = false;
		label_FPGA.Visible = false;
		label_front_panel.Visible = false;
		label_HERMES.Visible = false;
		label_HERMES_RX_IN.Visible = false;
		label_HERMES_J5.Visible = false;
		label_HERMES_TX_OUT.Visible = false;
		label_HERMES_J3.Visible = false;
		label_HERMES_XVTR_TX.Visible = false;
		label_HERMES_J1.Visible = false;
		label_HPF.Visible = false;
		label_HPF2.Visible = false;
		label_L_audio_only.Visible = false;
		label_LPF.Visible = false;
		label_LPF2.Visible = false;
		label_LR_audio.Visible = false;
		label_MERCURY.Visible = false;
		label_MERCURY_FPGA.Visible = false;
		label_MERCURY_ADC.Visible = false;
		label_MERCURY_DDC0.Visible = false;
		label_MERCURY_Rx0.Visible = false;
		label_MERCURY_DDC1.Visible = false;
		label_MERCURY_Rx1.Visible = false;
		label_MERCURY_DDC2.Visible = false;
		label_MERCURY_Rx2.Visible = false;
		label_MERCURY_CODEC.Visible = false;
		label_MERCURY_PHONES.Visible = false;
		label_MERCURY_P_OUT.Visible = false;
		label_MERCURY_LINE.Visible = false;
		label_MERCURY_OUT.Visible = false;
		label_MERCURY_ANT.Visible = false;
		label_MERCURY_2.Visible = false;
		label_MERCURY_2_FPGA.Visible = false;
		label_MERCURY_2_ADC.Visible = false;
		label_MERCURY_2_DDC0.Visible = false;
		label_MERCURY_2_Rx0.Visible = false;
		label_MERCURY_2_DDC1.Visible = false;
		label_MERCURY_2_Rx1.Visible = false;
		label_MERCURY_2_DDC2.Visible = false;
		label_MERCURY_2_Rx2.Visible = false;
		label_MERCURY_2_CODEC.Visible = false;
		label_MERCURY_2_PHONES.Visible = false;
		label_MERCURY_2_P_OUT.Visible = false;
		label_MERCURY_2_LINE.Visible = false;
		label_MERCURY_2_OUT.Visible = false;
		label_MERCURY_2_ANT.Visible = false;
		label_METIS.Visible = false;
		label_METIS_FPGA.Visible = false;
		label_PA.Visible = false;
		label_PENELOPE.Visible = false;
		label_PENELOPE_FPGA.Visible = false;
		label_PENELOPE_DAC.Visible = false;
		label_PENELOPE_DUC.Visible = false;
		label_PENELOPE_AMPF.Visible = false;
		label_PENELOPE_FILTER.Visible = false;
		label_PENELOPE_PA.Visible = false;
		label_PENELOPE_CODEC.Visible = false;
		label_PENELOPE_ANT.Visible = false;
		label_PENELOPE_LINE.Visible = false;
		label_PENELOPE_IN.Visible = false;
		label_PENELOPE_MIC.Visible = false;
		label_PENELOPE_XVTR.Visible = false;
		label_PC.Visible = false;
		label_rear_panel.Visible = false;
		label_Rx0.Visible = false;
		label_Rx1.Visible = false;
		label_RX1_DISPLAY.Visible = false;
		label_RX1_LR_audio.Visible = false;
		label_Rx2.Visible = false;
		label_RX2_DISPLAY.Visible = false;
		label_RX2_LR_audio.Visible = false;
		label_Rx3.Visible = false;
		label_Rx4.Visible = false;
		label_Rx5.Visible = false;
		label_Rx6.Visible = false;
		label_SDR_Hardware.Visible = false;
		label_SMA.Visible = false;
		label_SWR.Visible = false;
		label_NOTE.Visible = false;
		label_UNCHECK.Visible = false;
		label_XVTR_VHF.Visible = false;
	}

	private void hide_rear_panel_labels()
	{
		label_C1.Visible = false;
		label_C2.Visible = false;
		label_C3.Visible = false;
		label_C4.Visible = false;
		label_C5.Visible = false;
		label_C6.Visible = false;
		label_C7.Visible = false;
		label_C8.Visible = false;
		label_C9.Visible = false;
		label_C10.Visible = false;
		label_C11.Visible = false;
		label_SMA.Visible = false;
		label_C12.Visible = false;
		label_C13.Visible = false;
		label_C14.Visible = false;
		label_C15.Visible = false;
		label_C16.Visible = false;
		label_C17.Visible = false;
		label_C18.Visible = false;
		label_C19.Visible = false;
		label_C20.Visible = false;
		label_HPF.Visible = false;
		label_HPF2.Visible = false;
		label_LPF2.Visible = false;
		label_ADC1.Visible = false;
		label_ADC1_atten.Visible = false;
		label_ADC2.Visible = false;
		label_ADC2_atten.Visible = false;
		label_Rx2.Visible = false;
		label_Rx3.Visible = false;
		label_Rx4.Visible = false;
		label_Rx5.Visible = false;
		label_Rx6.Visible = false;
		label_DDC2.Visible = false;
		label_DDC3.Visible = false;
		label_DDC4.Visible = false;
		label_DDC5.Visible = false;
		label_DDC6.Visible = false;
	}

	private void hide_controls()
	{
		cb_DUAL_MERCURY_ALEX.Visible = false;
	}

	private void TX_AUDIO_OUT_2_RX_MODELS()
	{
		if (!bool_PureSignal & !bool_duplex & !bool_XVTR)
		{
			g.DrawRectangle(blackPen, RX2_DISPLAY);
			label_RX2_DISPLAY.Visible = true;
			if (bool_MON | bool_duplex)
			{
				if (!bool_RX1_MUTE)
				{
					SPKR_to_RX1_DISPLAY(bluePen);
				}
				if (!bool_RX2_MUTE)
				{
					SPKR_to_RX2_DISPLAY(bluePen);
				}
				if (!bool_RX1_MUTE)
				{
					label_RX1_LR_audio.Visible = true;
				}
				else
				{
					label_RX1_LR_audio.Visible = false;
				}
				if (!bool_RX2_MUTE)
				{
					label_RX2_LR_audio.Visible = true;
				}
				else
				{
					label_RX2_LR_audio.Visible = false;
				}
				if (bool_RX1_MUTE & bool_RX2_MUTE)
				{
					label_AUDIO_AMP.Visible = false;
					label_CODEC2.Visible = false;
					label_L_audio_only.Visible = false;
					label_LR_audio.Visible = false;
					label_AUDIO_MIXER.Visible = false;
				}
				else
				{
					g.DrawRectangle(blackPen, AUDIO_MIXER);
					label_AUDIO_MIXER.Visible = true;
					g.DrawRectangle(blackPen, CODEC2);
					label_AUDIO_AMP.Visible = true;
					label_CODEC2.Visible = true;
					label_L_audio_only.Visible = true;
					label_LR_audio.Visible = true;
					g.DrawRectangle(blackPen, AUDIO_AMP);
				}
			}
		}
		if (!bool_PureSignal & !bool_duplex & bool_XVTR)
		{
			g.DrawRectangle(blackPen, RX2_DISPLAY);
			label_RX2_DISPLAY.Visible = true;
			if (bool_MON)
			{
				if (!bool_RX1_MUTE)
				{
					SPKR_to_RX1_DISPLAY(bluePen);
				}
				if (!bool_RX2_MUTE)
				{
					SPKR_to_RX2_DISPLAY(bluePen);
				}
				if (!bool_RX1_MUTE)
				{
					label_RX1_LR_audio.Visible = true;
				}
				else
				{
					label_RX1_LR_audio.Visible = false;
				}
				if (!bool_RX2_MUTE)
				{
					label_RX2_LR_audio.Visible = true;
				}
				else
				{
					label_RX2_LR_audio.Visible = false;
				}
				if (bool_RX1_MUTE & bool_RX2_MUTE)
				{
					label_AUDIO_AMP.Visible = false;
					label_CODEC2.Visible = false;
					label_L_audio_only.Visible = false;
					label_LR_audio.Visible = false;
					label_AUDIO_MIXER.Visible = false;
				}
				else
				{
					g.DrawRectangle(blackPen, AUDIO_MIXER);
					label_AUDIO_MIXER.Visible = true;
					g.DrawRectangle(blackPen, CODEC2);
					label_AUDIO_AMP.Visible = true;
					label_CODEC2.Visible = true;
					label_L_audio_only.Visible = true;
					label_LR_audio.Visible = true;
					g.DrawRectangle(blackPen, AUDIO_AMP);
				}
			}
		}
		if (!bool_PureSignal & bool_duplex & bool_XVTR)
		{
			g.DrawRectangle(blackPen, RX2_DISPLAY);
			label_RX2_DISPLAY.Visible = true;
			if (!bool_RX1_MUTE)
			{
				SPKR_to_RX1_DISPLAY(bluePen);
			}
			if (!bool_RX2_MUTE)
			{
				SPKR_to_RX2_DISPLAY(bluePen);
			}
			if (!bool_RX1_MUTE)
			{
				label_RX1_LR_audio.Visible = true;
			}
			else
			{
				label_RX1_LR_audio.Visible = false;
			}
			if (!bool_RX2_MUTE)
			{
				label_RX2_LR_audio.Visible = true;
			}
			else
			{
				label_RX2_LR_audio.Visible = false;
			}
			if (bool_RX1_MUTE & bool_RX2_MUTE)
			{
				label_AUDIO_AMP.Visible = false;
				label_CODEC2.Visible = false;
				label_L_audio_only.Visible = false;
				label_LR_audio.Visible = false;
				label_AUDIO_MIXER.Visible = false;
			}
			else
			{
				g.DrawRectangle(blackPen, AUDIO_MIXER);
				label_AUDIO_MIXER.Visible = true;
				g.DrawRectangle(blackPen, CODEC2);
				label_AUDIO_AMP.Visible = true;
				label_CODEC2.Visible = true;
				label_L_audio_only.Visible = true;
				label_LR_audio.Visible = true;
				g.DrawRectangle(blackPen, AUDIO_AMP);
			}
		}
		if (!bool_PureSignal & bool_duplex & !bool_XVTR)
		{
			g.DrawRectangle(blackPen, RX2_DISPLAY);
			label_RX2_DISPLAY.Visible = true;
			if (!bool_RX1_MUTE)
			{
				SPKR_to_RX1_DISPLAY(bluePen);
			}
			if (!bool_RX2_MUTE)
			{
				SPKR_to_RX2_DISPLAY(bluePen);
			}
			if (!bool_RX1_MUTE)
			{
				label_RX1_LR_audio.Visible = true;
			}
			else
			{
				label_RX1_LR_audio.Visible = false;
			}
			if (!bool_RX2_MUTE)
			{
				label_RX2_LR_audio.Visible = true;
			}
			else
			{
				label_RX2_LR_audio.Visible = false;
			}
			if (bool_RX1_MUTE & bool_RX2_MUTE)
			{
				label_AUDIO_AMP.Visible = false;
				label_CODEC2.Visible = false;
				label_L_audio_only.Visible = false;
				label_LR_audio.Visible = false;
				label_AUDIO_MIXER.Visible = false;
			}
			else
			{
				g.DrawRectangle(blackPen, AUDIO_MIXER);
				label_AUDIO_MIXER.Visible = true;
				g.DrawRectangle(blackPen, CODEC2);
				label_AUDIO_AMP.Visible = true;
				label_CODEC2.Visible = true;
				label_L_audio_only.Visible = true;
				label_LR_audio.Visible = true;
				g.DrawRectangle(blackPen, AUDIO_AMP);
			}
		}
		if (bool_PureSignal & !bool_duplex & !bool_XVTR)
		{
			if (!bool_RX2_MUTE)
			{
				bool_RX2_MUTE = true;
			}
			if (bool_duplex)
			{
				bool_duplex = false;
			}
			label_RX2_DISPLAY.Visible = false;
			if (bool_MON)
			{
				if (!bool_RX1_MUTE)
				{
					SPKR_to_RX1_DISPLAY(bluePen);
				}
				label_RX2_LR_audio.Visible = false;
				if (!bool_RX1_MUTE)
				{
					label_RX1_LR_audio.Visible = true;
				}
				else
				{
					label_RX1_LR_audio.Visible = false;
				}
				if (bool_RX1_MUTE & bool_RX2_MUTE)
				{
					label_AUDIO_AMP.Visible = false;
					label_CODEC2.Visible = false;
					label_L_audio_only.Visible = false;
					label_LR_audio.Visible = false;
					label_AUDIO_MIXER.Visible = false;
				}
				else
				{
					g.DrawRectangle(blackPen, AUDIO_MIXER);
					label_AUDIO_MIXER.Visible = true;
					g.DrawRectangle(blackPen, CODEC2);
					label_AUDIO_AMP.Visible = true;
					label_CODEC2.Visible = true;
					label_L_audio_only.Visible = true;
					label_LR_audio.Visible = true;
					g.DrawRectangle(blackPen, AUDIO_AMP);
				}
			}
		}
		if (bool_PureSignal & bool_duplex & !bool_XVTR)
		{
			label_RX2_DISPLAY.Visible = false;
			if (!bool_RX2_MUTE)
			{
				bool_RX2_MUTE = true;
			}
			if (bool_duplex)
			{
				bool_duplex = false;
			}
			if (!bool_RX1_MUTE)
			{
				SPKR_to_RX1_DISPLAY_2(bluePen);
			}
			if (!bool_RX1_MUTE)
			{
				SPKR_to_RX1_DISPLAY_2(bluePen);
			}
			if (!bool_RX1_MUTE)
			{
				label_RX1_LR_audio.Visible = true;
			}
			else
			{
				label_RX1_LR_audio.Visible = false;
			}
			if (bool_RX1_MUTE & bool_RX2_MUTE)
			{
				label_AUDIO_AMP.Visible = false;
				label_CODEC2.Visible = false;
				label_L_audio_only.Visible = false;
				label_LR_audio.Visible = false;
				label_AUDIO_MIXER.Visible = false;
			}
			else
			{
				g.DrawRectangle(blackPen, AUDIO_MIXER);
				label_AUDIO_MIXER.Visible = true;
				g.DrawRectangle(blackPen, CODEC2);
				label_AUDIO_AMP.Visible = true;
				label_CODEC2.Visible = true;
				label_L_audio_only.Visible = true;
				label_LR_audio.Visible = true;
				g.DrawRectangle(blackPen, AUDIO_AMP);
			}
		}
		if (bool_PureSignal & !bool_duplex & bool_XVTR)
		{
			label_RX2_DISPLAY.Visible = false;
			if (!bool_RX2_MUTE)
			{
				bool_RX2_MUTE = true;
			}
			if (bool_MON)
			{
				if (!bool_RX1_MUTE)
				{
					SPKR_to_RX1_DISPLAY(bluePen);
				}
				label_RX2_LR_audio.Visible = false;
				if (!bool_RX1_MUTE)
				{
					label_RX1_LR_audio.Visible = true;
				}
				else
				{
					label_RX1_LR_audio.Visible = false;
				}
				if (bool_RX1_MUTE & bool_RX2_MUTE)
				{
					label_AUDIO_AMP.Visible = false;
					label_CODEC2.Visible = false;
					label_L_audio_only.Visible = false;
					label_LR_audio.Visible = false;
					label_AUDIO_MIXER.Visible = false;
				}
				else
				{
					g.DrawRectangle(blackPen, AUDIO_MIXER);
					label_AUDIO_MIXER.Visible = true;
					g.DrawRectangle(blackPen, CODEC2);
					label_AUDIO_AMP.Visible = true;
					label_CODEC2.Visible = true;
					label_L_audio_only.Visible = true;
					label_LR_audio.Visible = true;
					g.DrawRectangle(blackPen, AUDIO_AMP);
				}
			}
		}
		if (bool_PureSignal & bool_duplex & bool_XVTR)
		{
			label_RX2_DISPLAY.Visible = false;
			if (!bool_RX2_MUTE)
			{
				bool_RX2_MUTE = true;
			}
			if (bool_duplex)
			{
				bool_duplex = false;
			}
			if (!bool_RX1_MUTE)
			{
				SPKR_to_RX1_DISPLAY(bluePen);
			}
			if (!bool_RX2_MUTE)
			{
				SPKR_to_RX2_DISPLAY(bluePen);
			}
			if (!bool_RX1_MUTE)
			{
				label_RX1_LR_audio.Visible = true;
			}
			else
			{
				label_RX1_LR_audio.Visible = false;
			}
			if (!bool_RX2_MUTE)
			{
				label_RX2_LR_audio.Visible = true;
			}
			else
			{
				label_RX2_LR_audio.Visible = false;
			}
			if (bool_RX1_MUTE & bool_RX2_MUTE)
			{
				label_AUDIO_AMP.Visible = false;
				label_CODEC2.Visible = false;
				label_L_audio_only.Visible = false;
				label_LR_audio.Visible = false;
				label_AUDIO_MIXER.Visible = false;
			}
			else
			{
				g.DrawRectangle(blackPen, AUDIO_MIXER);
				label_AUDIO_MIXER.Visible = true;
				g.DrawRectangle(blackPen, CODEC2);
				label_AUDIO_AMP.Visible = true;
				label_CODEC2.Visible = true;
				label_L_audio_only.Visible = true;
				label_LR_audio.Visible = true;
				g.DrawRectangle(blackPen, AUDIO_AMP);
			}
		}
	}

	private void cb_DUAL_MERCURY_ALEX_CheckedChanged(object sender, EventArgs e)
	{
		do_platform_prep();
		update_diagram = true;
		canvas.Invalidate();
	}

	private void rb_rx_CheckedChanged(object sender, EventArgs e)
	{
		do_platform_prep();
		update_diagram = true;
		canvas.Invalidate();
	}

	private void rb_tx_CheckedChanged(object sender, EventArgs e)
	{
		do_platform_prep();
		update_diagram = true;
		canvas.Invalidate();
	}

	private void PI_Resize(object sender, EventArgs e)
	{
		do_platform_prep();
		update_diagram = true;
		canvas.Invalidate();
	}

	private void PI_Disposed(object sender, EventArgs e)
	{
		console.path_Illustrator = null;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.panel1 = new System.Windows.Forms.Panel();
		this.label_XVTR_VHF = new System.Windows.Forms.Label();
		this.label_DUAL_MERCURY = new System.Windows.Forms.Label();
		this.cb_DUAL_MERCURY_ALEX = new System.Windows.Forms.CheckBox();
		this.label_UNCHECK = new System.Windows.Forms.Label();
		this.label_NOTE = new System.Windows.Forms.Label();
		this.groupBox10 = new System.Windows.Forms.GroupBox();
		this.rb_tx = new System.Windows.Forms.RadioButton();
		this.rb_rx = new System.Windows.Forms.RadioButton();
		this.canvas = new System.Windows.Forms.Panel();
		this.label_HERMES_J1 = new System.Windows.Forms.Label();
		this.label_HERMES_J3 = new System.Windows.Forms.Label();
		this.label_HERMES_XVTR_TX = new System.Windows.Forms.Label();
		this.label_HERMES_TX_OUT = new System.Windows.Forms.Label();
		this.label_HERMES_J5 = new System.Windows.Forms.Label();
		this.label_HERMES_RX_IN = new System.Windows.Forms.Label();
		this.label_HERMES = new System.Windows.Forms.Label();
		this.label_ext_amp2 = new System.Windows.Forms.Label();
		this.label_ext_amp1 = new System.Windows.Forms.Label();
		this.label_PENELOPE_MIC = new System.Windows.Forms.Label();
		this.label_DSP_HPSDR = new System.Windows.Forms.Label();
		this.label_PENELOPE_XVTR = new System.Windows.Forms.Label();
		this.label_PENELOPE_IN = new System.Windows.Forms.Label();
		this.label_PENELOPE_LINE = new System.Windows.Forms.Label();
		this.label_PENELOPE_ANT = new System.Windows.Forms.Label();
		this.label_MERCURY_2_ANT = new System.Windows.Forms.Label();
		this.label_MERCURY_ANT = new System.Windows.Forms.Label();
		this.label_MERCURY_2_OUT = new System.Windows.Forms.Label();
		this.label_MERCURY_2_LINE = new System.Windows.Forms.Label();
		this.label_MERCURY_OUT = new System.Windows.Forms.Label();
		this.label_MERCURY_LINE = new System.Windows.Forms.Label();
		this.label_MERCURY_2_P_OUT = new System.Windows.Forms.Label();
		this.label_MERCURY_2_PHONES = new System.Windows.Forms.Label();
		this.label_MERCURY_P_OUT = new System.Windows.Forms.Label();
		this.label_MERCURY_PHONES = new System.Windows.Forms.Label();
		this.label_MERCURY_2_CODEC = new System.Windows.Forms.Label();
		this.label_MERCURY_2_Rx2 = new System.Windows.Forms.Label();
		this.label_MERCURY_2_DDC2 = new System.Windows.Forms.Label();
		this.label_MERCURY_2_Rx1 = new System.Windows.Forms.Label();
		this.label_MERCURY_2_DDC1 = new System.Windows.Forms.Label();
		this.label_MERCURY_2_Rx0 = new System.Windows.Forms.Label();
		this.label_MERCURY_2_DDC0 = new System.Windows.Forms.Label();
		this.label_MERCURY_2_ADC = new System.Windows.Forms.Label();
		this.label12 = new System.Windows.Forms.Label();
		this.label13 = new System.Windows.Forms.Label();
		this.label14 = new System.Windows.Forms.Label();
		this.label_MERCURY_CODEC = new System.Windows.Forms.Label();
		this.label_MERCURY_Rx2 = new System.Windows.Forms.Label();
		this.label_MERCURY_DDC2 = new System.Windows.Forms.Label();
		this.label_MERCURY_Rx1 = new System.Windows.Forms.Label();
		this.label_MERCURY_DDC1 = new System.Windows.Forms.Label();
		this.label_MERCURY_Rx0 = new System.Windows.Forms.Label();
		this.label_MERCURY_DDC0 = new System.Windows.Forms.Label();
		this.label_MERCURY_ADC = new System.Windows.Forms.Label();
		this.label_PENELOPE_CODEC = new System.Windows.Forms.Label();
		this.label_PENELOPE_FILTER = new System.Windows.Forms.Label();
		this.label_PENELOPE_AMPF = new System.Windows.Forms.Label();
		this.label_PENELOPE_PA = new System.Windows.Forms.Label();
		this.label_PENELOPE_DUC = new System.Windows.Forms.Label();
		this.label_PENELOPE_DAC = new System.Windows.Forms.Label();
		this.label_METIS_FPGA = new System.Windows.Forms.Label();
		this.label_METIS = new System.Windows.Forms.Label();
		this.label_ALEX_2_To_RX = new System.Windows.Forms.Label();
		this.label_ALEX_2_LPF = new System.Windows.Forms.Label();
		this.label_ALEX_2_HPF = new System.Windows.Forms.Label();
		this.label_ALEX_2 = new System.Windows.Forms.Label();
		this.label_PENELOPE_FPGA = new System.Windows.Forms.Label();
		this.label_PENELOPE = new System.Windows.Forms.Label();
		this.label_MERCURY_2_FPGA = new System.Windows.Forms.Label();
		this.label_MERCURY_2 = new System.Windows.Forms.Label();
		this.label_ALEX_To_RX = new System.Windows.Forms.Label();
		this.label_MERCURY_FPGA = new System.Windows.Forms.Label();
		this.label_MERCURY = new System.Windows.Forms.Label();
		this.label_ALEX_LPF = new System.Windows.Forms.Label();
		this.label_ALEX_HPF = new System.Windows.Forms.Label();
		this.label_ALEX = new System.Windows.Forms.Label();
		this.label_DDC6 = new System.Windows.Forms.Label();
		this.label_DDC5 = new System.Windows.Forms.Label();
		this.label_DDC4 = new System.Windows.Forms.Label();
		this.label_DDC3 = new System.Windows.Forms.Label();
		this.label_DDC2 = new System.Windows.Forms.Label();
		this.label_DDC1 = new System.Windows.Forms.Label();
		this.label_DDC0 = new System.Windows.Forms.Label();
		this.label_SWR = new System.Windows.Forms.Label();
		this.label_RX2_LR_audio = new System.Windows.Forms.Label();
		this.label_RX1_LR_audio = new System.Windows.Forms.Label();
		this.label_AUDIO_MIXER = new System.Windows.Forms.Label();
		this.label_AUDIO_AMP = new System.Windows.Forms.Label();
		this.label_LR_audio = new System.Windows.Forms.Label();
		this.label_L_audio_only = new System.Windows.Forms.Label();
		this.label_C25 = new System.Windows.Forms.Label();
		this.label_CODEC2 = new System.Windows.Forms.Label();
		this.label_C26 = new System.Windows.Forms.Label();
		this.label_SMA = new System.Windows.Forms.Label();
		this.label_front_panel = new System.Windows.Forms.Label();
		this.label_rear_panel = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.label_CODEC = new System.Windows.Forms.Label();
		this.label_C24 = new System.Windows.Forms.Label();
		this.label_LPF2 = new System.Windows.Forms.Label();
		this.label_HPF2 = new System.Windows.Forms.Label();
		this.label_C20 = new System.Windows.Forms.Label();
		this.label_C19 = new System.Windows.Forms.Label();
		this.label_C18 = new System.Windows.Forms.Label();
		this.label_C17 = new System.Windows.Forms.Label();
		this.label_DUC0 = new System.Windows.Forms.Label();
		this.label_FILTER = new System.Windows.Forms.Label();
		this.label_AMP = new System.Windows.Forms.Label();
		this.label_Rx4 = new System.Windows.Forms.Label();
		this.label_DSP = new System.Windows.Forms.Label();
		this.label_PC = new System.Windows.Forms.Label();
		this.label_FPGA = new System.Windows.Forms.Label();
		this.label_SDR_Hardware = new System.Windows.Forms.Label();
		this.label_hardware_selected = new System.Windows.Forms.Label();
		this.label_ADC2_atten = new System.Windows.Forms.Label();
		this.label_ADC1_atten = new System.Windows.Forms.Label();
		this.label_ADC0_atten = new System.Windows.Forms.Label();
		this.label_C16 = new System.Windows.Forms.Label();
		this.label_C15 = new System.Windows.Forms.Label();
		this.label_C14 = new System.Windows.Forms.Label();
		this.label_C13 = new System.Windows.Forms.Label();
		this.label_C12 = new System.Windows.Forms.Label();
		this.label_C3 = new System.Windows.Forms.Label();
		this.label_C2 = new System.Windows.Forms.Label();
		this.label_C1 = new System.Windows.Forms.Label();
		this.label_HPF = new System.Windows.Forms.Label();
		this.label_C4 = new System.Windows.Forms.Label();
		this.label_C11 = new System.Windows.Forms.Label();
		this.label_C10 = new System.Windows.Forms.Label();
		this.label_C9 = new System.Windows.Forms.Label();
		this.label_C8 = new System.Windows.Forms.Label();
		this.label_PA = new System.Windows.Forms.Label();
		this.label_DAC0 = new System.Windows.Forms.Label();
		this.label_LPF = new System.Windows.Forms.Label();
		this.label_RX2_DISPLAY = new System.Windows.Forms.Label();
		this.label_RX1_DISPLAY = new System.Windows.Forms.Label();
		this.label_Rx6 = new System.Windows.Forms.Label();
		this.label_Rx5 = new System.Windows.Forms.Label();
		this.label_Rx3 = new System.Windows.Forms.Label();
		this.label_Rx2 = new System.Windows.Forms.Label();
		this.label_Rx1 = new System.Windows.Forms.Label();
		this.label_Rx0 = new System.Windows.Forms.Label();
		this.label_ADC2 = new System.Windows.Forms.Label();
		this.label_ADC1 = new System.Windows.Forms.Label();
		this.label_ADC0 = new System.Windows.Forms.Label();
		this.label_C7 = new System.Windows.Forms.Label();
		this.label_C6 = new System.Windows.Forms.Label();
		this.label_C5 = new System.Windows.Forms.Label();
		this.panel1.SuspendLayout();
		this.groupBox10.SuspendLayout();
		this.canvas.SuspendLayout();
		base.SuspendLayout();
		this.panel1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
		this.panel1.Controls.Add(this.label_XVTR_VHF);
		this.panel1.Controls.Add(this.label_DUAL_MERCURY);
		this.panel1.Controls.Add(this.cb_DUAL_MERCURY_ALEX);
		this.panel1.Controls.Add(this.label_UNCHECK);
		this.panel1.Controls.Add(this.label_NOTE);
		this.panel1.Controls.Add(this.groupBox10);
		this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.panel1.Location = new System.Drawing.Point(0, 671);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(1166, 120);
		this.panel1.TabIndex = 0;
		this.label_XVTR_VHF.AutoSize = true;
		this.label_XVTR_VHF.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, 0);
		this.label_XVTR_VHF.ForeColor = System.Drawing.SystemColors.ControlLightLight;
		this.label_XVTR_VHF.Location = new System.Drawing.Point(46, 83);
		this.label_XVTR_VHF.Name = "label_XVTR_VHF";
		this.label_XVTR_VHF.Size = new System.Drawing.Size(281, 13);
		this.label_XVTR_VHF.TabIndex = 224;
		this.label_XVTR_VHF.Text = "assumes VHF+ is setup and  IF is within RX band selected";
		this.label_DUAL_MERCURY.AutoSize = true;
		this.label_DUAL_MERCURY.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, 0);
		this.label_DUAL_MERCURY.ForeColor = System.Drawing.SystemColors.ControlLightLight;
		this.label_DUAL_MERCURY.Location = new System.Drawing.Point(779, 48);
		this.label_DUAL_MERCURY.Name = "label_DUAL_MERCURY";
		this.label_DUAL_MERCURY.Size = new System.Drawing.Size(352, 13);
		this.label_DUAL_MERCURY.TabIndex = 223;
		this.label_DUAL_MERCURY.Text = "* assumes \"Multiple Mercury\" jumpers are installed on the Mercury boards";
		this.cb_DUAL_MERCURY_ALEX.AutoSize = true;
		this.cb_DUAL_MERCURY_ALEX.ForeColor = System.Drawing.SystemColors.ControlLightLight;
		this.cb_DUAL_MERCURY_ALEX.Location = new System.Drawing.Point(782, 25);
		this.cb_DUAL_MERCURY_ALEX.Name = "cb_DUAL_MERCURY_ALEX";
		this.cb_DUAL_MERCURY_ALEX.Size = new System.Drawing.Size(144, 17);
		this.cb_DUAL_MERCURY_ALEX.TabIndex = 222;
		this.cb_DUAL_MERCURY_ALEX.Text = "DUAL MERCURY/ALEX";
		this.cb_DUAL_MERCURY_ALEX.UseVisualStyleBackColor = true;
		this.cb_DUAL_MERCURY_ALEX.CheckedChanged += new System.EventHandler(cb_DUAL_MERCURY_ALEX_CheckedChanged);
		this.label_UNCHECK.AutoSize = true;
		this.label_UNCHECK.BackColor = System.Drawing.SystemColors.ControlDarkDark;
		this.label_UNCHECK.ForeColor = System.Drawing.SystemColors.ActiveBorder;
		this.label_UNCHECK.Location = new System.Drawing.Point(9, 35);
		this.label_UNCHECK.Name = "label_UNCHECK";
		this.label_UNCHECK.Size = new System.Drawing.Size(461, 13);
		this.label_UNCHECK.TabIndex = 221;
		this.label_UNCHECK.Text = "UNCHECK Setup > Ant/Filters > Antenna \"Disable BYPASS\" checkbox  if PA_rev15/16 present";
		this.label_NOTE.AutoSize = true;
		this.label_NOTE.ForeColor = System.Drawing.SystemColors.ActiveBorder;
		this.label_NOTE.Location = new System.Drawing.Point(12, 16);
		this.label_NOTE.Name = "label_NOTE";
		this.label_NOTE.Size = new System.Drawing.Size(449, 13);
		this.label_NOTE.TabIndex = 220;
		this.label_NOTE.Text = "CHECK      Setup > Ant/Filters > Antenna  \"Disable BYPASS\" checkbox  if PA_rev24 present,";
		this.groupBox10.Controls.Add(this.rb_tx);
		this.groupBox10.Controls.Add(this.rb_rx);
		this.groupBox10.ForeColor = System.Drawing.SystemColors.ControlLightLight;
		this.groupBox10.Location = new System.Drawing.Point(495, 16);
		this.groupBox10.Name = "groupBox10";
		this.groupBox10.Size = new System.Drawing.Size(229, 71);
		this.groupBox10.TabIndex = 71;
		this.groupBox10.TabStop = false;
		this.groupBox10.Text = "Signal paths for current option settings";
		this.rb_tx.AutoSize = true;
		this.rb_tx.ForeColor = System.Drawing.SystemColors.ControlLightLight;
		this.rb_tx.Location = new System.Drawing.Point(70, 42);
		this.rb_tx.Name = "rb_tx";
		this.rb_tx.Size = new System.Drawing.Size(68, 17);
		this.rb_tx.TabIndex = 20;
		this.rb_tx.Text = "TX mode";
		this.rb_tx.UseVisualStyleBackColor = true;
		this.rb_tx.CheckedChanged += new System.EventHandler(rb_tx_CheckedChanged);
		this.rb_rx.AutoSize = true;
		this.rb_rx.Checked = true;
		this.rb_rx.ForeColor = System.Drawing.SystemColors.ControlLightLight;
		this.rb_rx.Location = new System.Drawing.Point(70, 19);
		this.rb_rx.Name = "rb_rx";
		this.rb_rx.Size = new System.Drawing.Size(72, 17);
		this.rb_rx.TabIndex = 19;
		this.rb_rx.TabStop = true;
		this.rb_rx.Text = "RX mode ";
		this.rb_rx.UseVisualStyleBackColor = true;
		this.rb_rx.CheckedChanged += new System.EventHandler(rb_rx_CheckedChanged);
		this.canvas.Controls.Add(this.label_HERMES_J1);
		this.canvas.Controls.Add(this.label_HERMES_J3);
		this.canvas.Controls.Add(this.label_HERMES_XVTR_TX);
		this.canvas.Controls.Add(this.label_HERMES_TX_OUT);
		this.canvas.Controls.Add(this.label_HERMES_J5);
		this.canvas.Controls.Add(this.label_HERMES_RX_IN);
		this.canvas.Controls.Add(this.label_HERMES);
		this.canvas.Controls.Add(this.label_ext_amp2);
		this.canvas.Controls.Add(this.label_ext_amp1);
		this.canvas.Controls.Add(this.label_PENELOPE_MIC);
		this.canvas.Controls.Add(this.label_DSP_HPSDR);
		this.canvas.Controls.Add(this.label_PENELOPE_XVTR);
		this.canvas.Controls.Add(this.label_PENELOPE_IN);
		this.canvas.Controls.Add(this.label_PENELOPE_LINE);
		this.canvas.Controls.Add(this.label_PENELOPE_ANT);
		this.canvas.Controls.Add(this.label_MERCURY_2_ANT);
		this.canvas.Controls.Add(this.label_MERCURY_ANT);
		this.canvas.Controls.Add(this.label_MERCURY_2_OUT);
		this.canvas.Controls.Add(this.label_MERCURY_2_LINE);
		this.canvas.Controls.Add(this.label_MERCURY_OUT);
		this.canvas.Controls.Add(this.label_MERCURY_LINE);
		this.canvas.Controls.Add(this.label_MERCURY_2_P_OUT);
		this.canvas.Controls.Add(this.label_MERCURY_2_PHONES);
		this.canvas.Controls.Add(this.label_MERCURY_P_OUT);
		this.canvas.Controls.Add(this.label_MERCURY_PHONES);
		this.canvas.Controls.Add(this.label_MERCURY_2_CODEC);
		this.canvas.Controls.Add(this.label_MERCURY_2_Rx2);
		this.canvas.Controls.Add(this.label_MERCURY_2_DDC2);
		this.canvas.Controls.Add(this.label_MERCURY_2_Rx1);
		this.canvas.Controls.Add(this.label_MERCURY_2_DDC1);
		this.canvas.Controls.Add(this.label_MERCURY_2_Rx0);
		this.canvas.Controls.Add(this.label_MERCURY_2_DDC0);
		this.canvas.Controls.Add(this.label_MERCURY_2_ADC);
		this.canvas.Controls.Add(this.label12);
		this.canvas.Controls.Add(this.label13);
		this.canvas.Controls.Add(this.label14);
		this.canvas.Controls.Add(this.label_MERCURY_CODEC);
		this.canvas.Controls.Add(this.label_MERCURY_Rx2);
		this.canvas.Controls.Add(this.label_MERCURY_DDC2);
		this.canvas.Controls.Add(this.label_MERCURY_Rx1);
		this.canvas.Controls.Add(this.label_MERCURY_DDC1);
		this.canvas.Controls.Add(this.label_MERCURY_Rx0);
		this.canvas.Controls.Add(this.label_MERCURY_DDC0);
		this.canvas.Controls.Add(this.label_MERCURY_ADC);
		this.canvas.Controls.Add(this.label_PENELOPE_CODEC);
		this.canvas.Controls.Add(this.label_PENELOPE_FILTER);
		this.canvas.Controls.Add(this.label_PENELOPE_AMPF);
		this.canvas.Controls.Add(this.label_PENELOPE_PA);
		this.canvas.Controls.Add(this.label_PENELOPE_DUC);
		this.canvas.Controls.Add(this.label_PENELOPE_DAC);
		this.canvas.Controls.Add(this.label_METIS_FPGA);
		this.canvas.Controls.Add(this.label_METIS);
		this.canvas.Controls.Add(this.label_ALEX_2_To_RX);
		this.canvas.Controls.Add(this.label_ALEX_2_LPF);
		this.canvas.Controls.Add(this.label_ALEX_2_HPF);
		this.canvas.Controls.Add(this.label_ALEX_2);
		this.canvas.Controls.Add(this.label_PENELOPE_FPGA);
		this.canvas.Controls.Add(this.label_PENELOPE);
		this.canvas.Controls.Add(this.label_MERCURY_2_FPGA);
		this.canvas.Controls.Add(this.label_MERCURY_2);
		this.canvas.Controls.Add(this.label_ALEX_To_RX);
		this.canvas.Controls.Add(this.label_MERCURY_FPGA);
		this.canvas.Controls.Add(this.label_MERCURY);
		this.canvas.Controls.Add(this.label_ALEX_LPF);
		this.canvas.Controls.Add(this.label_ALEX_HPF);
		this.canvas.Controls.Add(this.label_ALEX);
		this.canvas.Controls.Add(this.label_DDC6);
		this.canvas.Controls.Add(this.label_DDC5);
		this.canvas.Controls.Add(this.label_DDC4);
		this.canvas.Controls.Add(this.label_DDC3);
		this.canvas.Controls.Add(this.label_DDC2);
		this.canvas.Controls.Add(this.label_DDC1);
		this.canvas.Controls.Add(this.label_DDC0);
		this.canvas.Controls.Add(this.label_SWR);
		this.canvas.Controls.Add(this.label_RX2_LR_audio);
		this.canvas.Controls.Add(this.label_RX1_LR_audio);
		this.canvas.Controls.Add(this.label_AUDIO_MIXER);
		this.canvas.Controls.Add(this.label_AUDIO_AMP);
		this.canvas.Controls.Add(this.label_LR_audio);
		this.canvas.Controls.Add(this.label_L_audio_only);
		this.canvas.Controls.Add(this.label_C25);
		this.canvas.Controls.Add(this.label_CODEC2);
		this.canvas.Controls.Add(this.label_C26);
		this.canvas.Controls.Add(this.label_SMA);
		this.canvas.Controls.Add(this.label_front_panel);
		this.canvas.Controls.Add(this.label_rear_panel);
		this.canvas.Controls.Add(this.label4);
		this.canvas.Controls.Add(this.label3);
		this.canvas.Controls.Add(this.label2);
		this.canvas.Controls.Add(this.label_CODEC);
		this.canvas.Controls.Add(this.label_C24);
		this.canvas.Controls.Add(this.label_LPF2);
		this.canvas.Controls.Add(this.label_HPF2);
		this.canvas.Controls.Add(this.label_C20);
		this.canvas.Controls.Add(this.label_C19);
		this.canvas.Controls.Add(this.label_C18);
		this.canvas.Controls.Add(this.label_C17);
		this.canvas.Controls.Add(this.label_DUC0);
		this.canvas.Controls.Add(this.label_FILTER);
		this.canvas.Controls.Add(this.label_AMP);
		this.canvas.Controls.Add(this.label_Rx4);
		this.canvas.Controls.Add(this.label_DSP);
		this.canvas.Controls.Add(this.label_PC);
		this.canvas.Controls.Add(this.label_FPGA);
		this.canvas.Controls.Add(this.label_SDR_Hardware);
		this.canvas.Controls.Add(this.label_hardware_selected);
		this.canvas.Controls.Add(this.label_ADC2_atten);
		this.canvas.Controls.Add(this.label_ADC1_atten);
		this.canvas.Controls.Add(this.label_ADC0_atten);
		this.canvas.Controls.Add(this.label_C16);
		this.canvas.Controls.Add(this.label_C15);
		this.canvas.Controls.Add(this.label_C14);
		this.canvas.Controls.Add(this.label_C13);
		this.canvas.Controls.Add(this.label_C12);
		this.canvas.Controls.Add(this.label_C3);
		this.canvas.Controls.Add(this.label_C2);
		this.canvas.Controls.Add(this.label_C1);
		this.canvas.Controls.Add(this.label_HPF);
		this.canvas.Controls.Add(this.label_C4);
		this.canvas.Controls.Add(this.label_C11);
		this.canvas.Controls.Add(this.label_C10);
		this.canvas.Controls.Add(this.label_C9);
		this.canvas.Controls.Add(this.label_C8);
		this.canvas.Controls.Add(this.label_PA);
		this.canvas.Controls.Add(this.label_DAC0);
		this.canvas.Controls.Add(this.label_LPF);
		this.canvas.Controls.Add(this.label_RX2_DISPLAY);
		this.canvas.Controls.Add(this.label_RX1_DISPLAY);
		this.canvas.Controls.Add(this.label_Rx6);
		this.canvas.Controls.Add(this.label_Rx5);
		this.canvas.Controls.Add(this.label_Rx3);
		this.canvas.Controls.Add(this.label_Rx2);
		this.canvas.Controls.Add(this.label_Rx1);
		this.canvas.Controls.Add(this.label_Rx0);
		this.canvas.Controls.Add(this.label_ADC2);
		this.canvas.Controls.Add(this.label_ADC1);
		this.canvas.Controls.Add(this.label_ADC0);
		this.canvas.Controls.Add(this.label_C7);
		this.canvas.Controls.Add(this.label_C6);
		this.canvas.Controls.Add(this.label_C5);
		this.canvas.Dock = System.Windows.Forms.DockStyle.Fill;
		this.canvas.Location = new System.Drawing.Point(0, 0);
		this.canvas.Name = "canvas";
		this.canvas.Size = new System.Drawing.Size(1166, 671);
		this.canvas.TabIndex = 1;
		this.canvas.Paint += new System.Windows.Forms.PaintEventHandler(canvas_Paint);
		this.label_HERMES_J1.AutoSize = true;
		this.label_HERMES_J1.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, 0);
		this.label_HERMES_J1.Location = new System.Drawing.Point(596, 39);
		this.label_HERMES_J1.Name = "label_HERMES_J1";
		this.label_HERMES_J1.Size = new System.Drawing.Size(28, 15);
		this.label_HERMES_J1.TabIndex = 430;
		this.label_HERMES_J1.Text = "(J1)";
		this.label_HERMES_J3.AutoSize = true;
		this.label_HERMES_J3.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, 0);
		this.label_HERMES_J3.Location = new System.Drawing.Point(492, 39);
		this.label_HERMES_J3.Name = "label_HERMES_J3";
		this.label_HERMES_J3.Size = new System.Drawing.Size(28, 15);
		this.label_HERMES_J3.TabIndex = 429;
		this.label_HERMES_J3.Text = "(J3)";
		this.label_HERMES_XVTR_TX.AutoSize = true;
		this.label_HERMES_XVTR_TX.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_HERMES_XVTR_TX.Location = new System.Drawing.Point(590, 24);
		this.label_HERMES_XVTR_TX.Name = "label_HERMES_XVTR_TX";
		this.label_HERMES_XVTR_TX.Size = new System.Drawing.Size(56, 15);
		this.label_HERMES_XVTR_TX.TabIndex = 428;
		this.label_HERMES_XVTR_TX.Text = "XVTR TX";
		this.label_HERMES_TX_OUT.AutoSize = true;
		this.label_HERMES_TX_OUT.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_HERMES_TX_OUT.Location = new System.Drawing.Point(492, 24);
		this.label_HERMES_TX_OUT.Name = "label_HERMES_TX_OUT";
		this.label_HERMES_TX_OUT.Size = new System.Drawing.Size(49, 15);
		this.label_HERMES_TX_OUT.TabIndex = 427;
		this.label_HERMES_TX_OUT.Text = "TX OUT";
		this.label_HERMES_J5.AutoSize = true;
		this.label_HERMES_J5.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, 0);
		this.label_HERMES_J5.Location = new System.Drawing.Point(440, 39);
		this.label_HERMES_J5.Name = "label_HERMES_J5";
		this.label_HERMES_J5.Size = new System.Drawing.Size(28, 15);
		this.label_HERMES_J5.TabIndex = 426;
		this.label_HERMES_J5.Text = "(J5)";
		this.label_HERMES_RX_IN.AutoSize = true;
		this.label_HERMES_RX_IN.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_HERMES_RX_IN.Location = new System.Drawing.Point(436, 24);
		this.label_HERMES_RX_IN.Name = "label_HERMES_RX_IN";
		this.label_HERMES_RX_IN.Size = new System.Drawing.Size(37, 15);
		this.label_HERMES_RX_IN.TabIndex = 425;
		this.label_HERMES_RX_IN.Text = "RX IN";
		this.label_HERMES.AutoSize = true;
		this.label_HERMES.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_HERMES.Location = new System.Drawing.Point(373, 23);
		this.label_HERMES.Name = "label_HERMES";
		this.label_HERMES.Size = new System.Drawing.Size(55, 15);
		this.label_HERMES.TabIndex = 424;
		this.label_HERMES.Text = "HERMES";
		this.label_ext_amp2.AutoSize = true;
		this.label_ext_amp2.Cursor = System.Windows.Forms.Cursors.Default;
		this.label_ext_amp2.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, 0);
		this.label_ext_amp2.Location = new System.Drawing.Point(348, 372);
		this.label_ext_amp2.Name = "label_ext_amp2";
		this.label_ext_amp2.Size = new System.Drawing.Size(117, 15);
		this.label_ext_amp2.TabIndex = 423;
		this.label_ext_amp2.Text = " insert in series here";
		this.label_ext_amp1.AutoSize = true;
		this.label_ext_amp1.Cursor = System.Windows.Forms.Cursors.Default;
		this.label_ext_amp1.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, 0);
		this.label_ext_amp1.Location = new System.Drawing.Point(348, 357);
		this.label_ext_amp1.Name = "label_ext_amp1";
		this.label_ext_amp1.Size = new System.Drawing.Size(171, 15);
		this.label_ext_amp1.TabIndex = 422;
		this.label_ext_amp1.Text = "* if ext amp used (100W max),";
		this.label_PENELOPE_MIC.AutoSize = true;
		this.label_PENELOPE_MIC.Cursor = System.Windows.Forms.Cursors.Default;
		this.label_PENELOPE_MIC.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_PENELOPE_MIC.Location = new System.Drawing.Point(370, 340);
		this.label_PENELOPE_MIC.Name = "label_PENELOPE_MIC";
		this.label_PENELOPE_MIC.Size = new System.Drawing.Size(28, 15);
		this.label_PENELOPE_MIC.TabIndex = 421;
		this.label_PENELOPE_MIC.Text = "MIC";
		this.label_DSP_HPSDR.AutoSize = true;
		this.label_DSP_HPSDR.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_DSP_HPSDR.Location = new System.Drawing.Point(934, 158);
		this.label_DSP_HPSDR.Name = "label_DSP_HPSDR";
		this.label_DSP_HPSDR.Size = new System.Drawing.Size(31, 15);
		this.label_DSP_HPSDR.TabIndex = 420;
		this.label_DSP_HPSDR.Text = "DSP";
		this.label_PENELOPE_XVTR.AutoSize = true;
		this.label_PENELOPE_XVTR.Cursor = System.Windows.Forms.Cursors.Default;
		this.label_PENELOPE_XVTR.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_PENELOPE_XVTR.Location = new System.Drawing.Point(372, 325);
		this.label_PENELOPE_XVTR.Name = "label_PENELOPE_XVTR";
		this.label_PENELOPE_XVTR.Size = new System.Drawing.Size(38, 15);
		this.label_PENELOPE_XVTR.TabIndex = 419;
		this.label_PENELOPE_XVTR.Text = "XVTR";
		this.label_PENELOPE_IN.AutoSize = true;
		this.label_PENELOPE_IN.Cursor = System.Windows.Forms.Cursors.Default;
		this.label_PENELOPE_IN.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_PENELOPE_IN.Location = new System.Drawing.Point(372, 310);
		this.label_PENELOPE_IN.Name = "label_PENELOPE_IN";
		this.label_PENELOPE_IN.Size = new System.Drawing.Size(18, 15);
		this.label_PENELOPE_IN.TabIndex = 418;
		this.label_PENELOPE_IN.Text = "IN";
		this.label_PENELOPE_LINE.AutoSize = true;
		this.label_PENELOPE_LINE.Cursor = System.Windows.Forms.Cursors.Default;
		this.label_PENELOPE_LINE.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_PENELOPE_LINE.Location = new System.Drawing.Point(372, 295);
		this.label_PENELOPE_LINE.Name = "label_PENELOPE_LINE";
		this.label_PENELOPE_LINE.Size = new System.Drawing.Size(32, 15);
		this.label_PENELOPE_LINE.TabIndex = 417;
		this.label_PENELOPE_LINE.Text = "LINE";
		this.label_PENELOPE_ANT.AutoSize = true;
		this.label_PENELOPE_ANT.Cursor = System.Windows.Forms.Cursors.Default;
		this.label_PENELOPE_ANT.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_PENELOPE_ANT.Location = new System.Drawing.Point(371, 280);
		this.label_PENELOPE_ANT.Name = "label_PENELOPE_ANT";
		this.label_PENELOPE_ANT.Size = new System.Drawing.Size(30, 15);
		this.label_PENELOPE_ANT.TabIndex = 416;
		this.label_PENELOPE_ANT.Text = "ANT";
		this.label_MERCURY_2_ANT.AutoSize = true;
		this.label_MERCURY_2_ANT.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_MERCURY_2_ANT.Location = new System.Drawing.Point(651, 558);
		this.label_MERCURY_2_ANT.Name = "label_MERCURY_2_ANT";
		this.label_MERCURY_2_ANT.Size = new System.Drawing.Size(30, 15);
		this.label_MERCURY_2_ANT.TabIndex = 415;
		this.label_MERCURY_2_ANT.Text = "ANT";
		this.label_MERCURY_ANT.AutoSize = true;
		this.label_MERCURY_ANT.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_MERCURY_ANT.Location = new System.Drawing.Point(580, 558);
		this.label_MERCURY_ANT.Name = "label_MERCURY_ANT";
		this.label_MERCURY_ANT.Size = new System.Drawing.Size(30, 15);
		this.label_MERCURY_ANT.TabIndex = 414;
		this.label_MERCURY_ANT.Text = "ANT";
		this.label_MERCURY_2_OUT.AutoSize = true;
		this.label_MERCURY_2_OUT.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_MERCURY_2_OUT.Location = new System.Drawing.Point(650, 543);
		this.label_MERCURY_2_OUT.Name = "label_MERCURY_2_OUT";
		this.label_MERCURY_2_OUT.Size = new System.Drawing.Size(31, 15);
		this.label_MERCURY_2_OUT.TabIndex = 413;
		this.label_MERCURY_2_OUT.Text = "OUT";
		this.label_MERCURY_2_LINE.AutoSize = true;
		this.label_MERCURY_2_LINE.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_MERCURY_2_LINE.Location = new System.Drawing.Point(650, 528);
		this.label_MERCURY_2_LINE.Name = "label_MERCURY_2_LINE";
		this.label_MERCURY_2_LINE.Size = new System.Drawing.Size(32, 15);
		this.label_MERCURY_2_LINE.TabIndex = 412;
		this.label_MERCURY_2_LINE.Text = "LINE";
		this.label_MERCURY_OUT.AutoSize = true;
		this.label_MERCURY_OUT.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_MERCURY_OUT.Location = new System.Drawing.Point(579, 543);
		this.label_MERCURY_OUT.Name = "label_MERCURY_OUT";
		this.label_MERCURY_OUT.Size = new System.Drawing.Size(31, 15);
		this.label_MERCURY_OUT.TabIndex = 411;
		this.label_MERCURY_OUT.Text = "OUT";
		this.label_MERCURY_LINE.AutoSize = true;
		this.label_MERCURY_LINE.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_MERCURY_LINE.Location = new System.Drawing.Point(579, 528);
		this.label_MERCURY_LINE.Name = "label_MERCURY_LINE";
		this.label_MERCURY_LINE.Size = new System.Drawing.Size(32, 15);
		this.label_MERCURY_LINE.TabIndex = 410;
		this.label_MERCURY_LINE.Text = "LINE";
		this.label_MERCURY_2_P_OUT.AutoSize = true;
		this.label_MERCURY_2_P_OUT.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_MERCURY_2_P_OUT.Location = new System.Drawing.Point(644, 513);
		this.label_MERCURY_2_P_OUT.Name = "label_MERCURY_2_P_OUT";
		this.label_MERCURY_2_P_OUT.Size = new System.Drawing.Size(31, 15);
		this.label_MERCURY_2_P_OUT.TabIndex = 409;
		this.label_MERCURY_2_P_OUT.Text = "OUT";
		this.label_MERCURY_2_PHONES.AutoSize = true;
		this.label_MERCURY_2_PHONES.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_MERCURY_2_PHONES.Location = new System.Drawing.Point(650, 498);
		this.label_MERCURY_2_PHONES.Name = "label_MERCURY_2_PHONES";
		this.label_MERCURY_2_PHONES.Size = new System.Drawing.Size(55, 15);
		this.label_MERCURY_2_PHONES.TabIndex = 408;
		this.label_MERCURY_2_PHONES.Text = "PHONES";
		this.label_MERCURY_P_OUT.AutoSize = true;
		this.label_MERCURY_P_OUT.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_MERCURY_P_OUT.Location = new System.Drawing.Point(579, 513);
		this.label_MERCURY_P_OUT.Name = "label_MERCURY_P_OUT";
		this.label_MERCURY_P_OUT.Size = new System.Drawing.Size(31, 15);
		this.label_MERCURY_P_OUT.TabIndex = 407;
		this.label_MERCURY_P_OUT.Text = "OUT";
		this.label_MERCURY_PHONES.AutoSize = true;
		this.label_MERCURY_PHONES.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_MERCURY_PHONES.Location = new System.Drawing.Point(579, 498);
		this.label_MERCURY_PHONES.Name = "label_MERCURY_PHONES";
		this.label_MERCURY_PHONES.Size = new System.Drawing.Size(55, 15);
		this.label_MERCURY_PHONES.TabIndex = 406;
		this.label_MERCURY_PHONES.Text = "PHONES";
		this.label_MERCURY_2_CODEC.AutoSize = true;
		this.label_MERCURY_2_CODEC.Font = new System.Drawing.Font("Arial", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_MERCURY_2_CODEC.Location = new System.Drawing.Point(645, 482);
		this.label_MERCURY_2_CODEC.Name = "label_MERCURY_2_CODEC";
		this.label_MERCURY_2_CODEC.Size = new System.Drawing.Size(44, 14);
		this.label_MERCURY_2_CODEC.TabIndex = 405;
		this.label_MERCURY_2_CODEC.Text = "CODEC";
		this.label_MERCURY_2_Rx2.AutoSize = true;
		this.label_MERCURY_2_Rx2.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, 0);
		this.label_MERCURY_2_Rx2.Location = new System.Drawing.Point(645, 465);
		this.label_MERCURY_2_Rx2.Name = "label_MERCURY_2_Rx2";
		this.label_MERCURY_2_Rx2.Size = new System.Drawing.Size(37, 15);
		this.label_MERCURY_2_Rx2.TabIndex = 404;
		this.label_MERCURY_2_Rx2.Text = "(Rx2)";
		this.label_MERCURY_2_DDC2.AutoSize = true;
		this.label_MERCURY_2_DDC2.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_MERCURY_2_DDC2.Location = new System.Drawing.Point(644, 450);
		this.label_MERCURY_2_DDC2.Name = "label_MERCURY_2_DDC2";
		this.label_MERCURY_2_DDC2.Size = new System.Drawing.Size(38, 15);
		this.label_MERCURY_2_DDC2.TabIndex = 403;
		this.label_MERCURY_2_DDC2.Text = "DDC2";
		this.label_MERCURY_2_Rx1.AutoSize = true;
		this.label_MERCURY_2_Rx1.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, 0);
		this.label_MERCURY_2_Rx1.Location = new System.Drawing.Point(644, 435);
		this.label_MERCURY_2_Rx1.Name = "label_MERCURY_2_Rx1";
		this.label_MERCURY_2_Rx1.Size = new System.Drawing.Size(37, 15);
		this.label_MERCURY_2_Rx1.TabIndex = 402;
		this.label_MERCURY_2_Rx1.Text = "(Rx1)";
		this.label_MERCURY_2_DDC1.AutoSize = true;
		this.label_MERCURY_2_DDC1.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_MERCURY_2_DDC1.Location = new System.Drawing.Point(644, 420);
		this.label_MERCURY_2_DDC1.Name = "label_MERCURY_2_DDC1";
		this.label_MERCURY_2_DDC1.Size = new System.Drawing.Size(38, 15);
		this.label_MERCURY_2_DDC1.TabIndex = 401;
		this.label_MERCURY_2_DDC1.Text = "DDC1";
		this.label_MERCURY_2_Rx0.AutoSize = true;
		this.label_MERCURY_2_Rx0.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, 0);
		this.label_MERCURY_2_Rx0.Location = new System.Drawing.Point(644, 402);
		this.label_MERCURY_2_Rx0.Name = "label_MERCURY_2_Rx0";
		this.label_MERCURY_2_Rx0.Size = new System.Drawing.Size(37, 15);
		this.label_MERCURY_2_Rx0.TabIndex = 400;
		this.label_MERCURY_2_Rx0.Text = "(Rx0)";
		this.label_MERCURY_2_DDC0.AutoSize = true;
		this.label_MERCURY_2_DDC0.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_MERCURY_2_DDC0.Location = new System.Drawing.Point(644, 387);
		this.label_MERCURY_2_DDC0.Name = "label_MERCURY_2_DDC0";
		this.label_MERCURY_2_DDC0.Size = new System.Drawing.Size(38, 15);
		this.label_MERCURY_2_DDC0.TabIndex = 399;
		this.label_MERCURY_2_DDC0.Text = "DDC0";
		this.label_MERCURY_2_ADC.AutoSize = true;
		this.label_MERCURY_2_ADC.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_MERCURY_2_ADC.Location = new System.Drawing.Point(644, 372);
		this.label_MERCURY_2_ADC.Name = "label_MERCURY_2_ADC";
		this.label_MERCURY_2_ADC.Size = new System.Drawing.Size(31, 15);
		this.label_MERCURY_2_ADC.TabIndex = 398;
		this.label_MERCURY_2_ADC.Text = "ADC";
		this.label12.AutoSize = true;
		this.label12.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label12.Location = new System.Drawing.Point(663, 483);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(0, 15);
		this.label12.TabIndex = 397;
		this.label13.AutoSize = true;
		this.label13.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label13.Location = new System.Drawing.Point(672, 483);
		this.label13.Name = "label13";
		this.label13.Size = new System.Drawing.Size(0, 15);
		this.label13.TabIndex = 396;
		this.label14.AutoSize = true;
		this.label14.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label14.Location = new System.Drawing.Point(661, 483);
		this.label14.Name = "label14";
		this.label14.Size = new System.Drawing.Size(0, 15);
		this.label14.TabIndex = 395;
		this.label_MERCURY_CODEC.AutoSize = true;
		this.label_MERCURY_CODEC.Font = new System.Drawing.Font("Arial", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_MERCURY_CODEC.Location = new System.Drawing.Point(580, 482);
		this.label_MERCURY_CODEC.Name = "label_MERCURY_CODEC";
		this.label_MERCURY_CODEC.Size = new System.Drawing.Size(44, 14);
		this.label_MERCURY_CODEC.TabIndex = 394;
		this.label_MERCURY_CODEC.Text = "CODEC";
		this.label_MERCURY_Rx2.AutoSize = true;
		this.label_MERCURY_Rx2.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, 0);
		this.label_MERCURY_Rx2.Location = new System.Drawing.Point(580, 465);
		this.label_MERCURY_Rx2.Name = "label_MERCURY_Rx2";
		this.label_MERCURY_Rx2.Size = new System.Drawing.Size(37, 15);
		this.label_MERCURY_Rx2.TabIndex = 393;
		this.label_MERCURY_Rx2.Text = "(Rx2)";
		this.label_MERCURY_DDC2.AutoSize = true;
		this.label_MERCURY_DDC2.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_MERCURY_DDC2.Location = new System.Drawing.Point(579, 450);
		this.label_MERCURY_DDC2.Name = "label_MERCURY_DDC2";
		this.label_MERCURY_DDC2.Size = new System.Drawing.Size(38, 15);
		this.label_MERCURY_DDC2.TabIndex = 392;
		this.label_MERCURY_DDC2.Text = "DDC2";
		this.label_MERCURY_Rx1.AutoSize = true;
		this.label_MERCURY_Rx1.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, 0);
		this.label_MERCURY_Rx1.Location = new System.Drawing.Point(579, 435);
		this.label_MERCURY_Rx1.Name = "label_MERCURY_Rx1";
		this.label_MERCURY_Rx1.Size = new System.Drawing.Size(37, 15);
		this.label_MERCURY_Rx1.TabIndex = 391;
		this.label_MERCURY_Rx1.Text = "(Rx1)";
		this.label_MERCURY_DDC1.AutoSize = true;
		this.label_MERCURY_DDC1.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_MERCURY_DDC1.Location = new System.Drawing.Point(579, 420);
		this.label_MERCURY_DDC1.Name = "label_MERCURY_DDC1";
		this.label_MERCURY_DDC1.Size = new System.Drawing.Size(38, 15);
		this.label_MERCURY_DDC1.TabIndex = 390;
		this.label_MERCURY_DDC1.Text = "DDC1";
		this.label_MERCURY_Rx0.AutoSize = true;
		this.label_MERCURY_Rx0.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, 0);
		this.label_MERCURY_Rx0.Location = new System.Drawing.Point(579, 402);
		this.label_MERCURY_Rx0.Name = "label_MERCURY_Rx0";
		this.label_MERCURY_Rx0.Size = new System.Drawing.Size(37, 15);
		this.label_MERCURY_Rx0.TabIndex = 389;
		this.label_MERCURY_Rx0.Text = "(Rx0)";
		this.label_MERCURY_DDC0.AutoSize = true;
		this.label_MERCURY_DDC0.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_MERCURY_DDC0.Location = new System.Drawing.Point(579, 387);
		this.label_MERCURY_DDC0.Name = "label_MERCURY_DDC0";
		this.label_MERCURY_DDC0.Size = new System.Drawing.Size(38, 15);
		this.label_MERCURY_DDC0.TabIndex = 388;
		this.label_MERCURY_DDC0.Text = "DDC0";
		this.label_MERCURY_ADC.AutoSize = true;
		this.label_MERCURY_ADC.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_MERCURY_ADC.Location = new System.Drawing.Point(579, 372);
		this.label_MERCURY_ADC.Name = "label_MERCURY_ADC";
		this.label_MERCURY_ADC.Size = new System.Drawing.Size(31, 15);
		this.label_MERCURY_ADC.TabIndex = 387;
		this.label_MERCURY_ADC.Text = "ADC";
		this.label_PENELOPE_CODEC.AutoSize = true;
		this.label_PENELOPE_CODEC.Cursor = System.Windows.Forms.Cursors.Default;
		this.label_PENELOPE_CODEC.Font = new System.Drawing.Font("Arial", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_PENELOPE_CODEC.Location = new System.Drawing.Point(371, 265);
		this.label_PENELOPE_CODEC.Name = "label_PENELOPE_CODEC";
		this.label_PENELOPE_CODEC.Size = new System.Drawing.Size(44, 14);
		this.label_PENELOPE_CODEC.TabIndex = 386;
		this.label_PENELOPE_CODEC.Text = "CODEC";
		this.label_PENELOPE_FILTER.AutoSize = true;
		this.label_PENELOPE_FILTER.Cursor = System.Windows.Forms.Cursors.Default;
		this.label_PENELOPE_FILTER.Font = new System.Drawing.Font("Arial", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_PENELOPE_FILTER.Location = new System.Drawing.Point(371, 248);
		this.label_PENELOPE_FILTER.Name = "label_PENELOPE_FILTER";
		this.label_PENELOPE_FILTER.Size = new System.Drawing.Size(42, 14);
		this.label_PENELOPE_FILTER.TabIndex = 385;
		this.label_PENELOPE_FILTER.Text = "FILTER";
		this.label_PENELOPE_AMPF.AutoSize = true;
		this.label_PENELOPE_AMPF.Cursor = System.Windows.Forms.Cursors.Default;
		this.label_PENELOPE_AMPF.Font = new System.Drawing.Font("Arial", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_PENELOPE_AMPF.Location = new System.Drawing.Point(371, 233);
		this.label_PENELOPE_AMPF.Name = "label_PENELOPE_AMPF";
		this.label_PENELOPE_AMPF.Size = new System.Drawing.Size(38, 14);
		this.label_PENELOPE_AMPF.TabIndex = 384;
		this.label_PENELOPE_AMPF.Text = "AMP+";
		this.label_PENELOPE_PA.AutoSize = true;
		this.label_PENELOPE_PA.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_PENELOPE_PA.Location = new System.Drawing.Point(371, 215);
		this.label_PENELOPE_PA.Name = "label_PENELOPE_PA";
		this.label_PENELOPE_PA.Size = new System.Drawing.Size(22, 15);
		this.label_PENELOPE_PA.TabIndex = 383;
		this.label_PENELOPE_PA.Text = "PA";
		this.label_PENELOPE_DUC.AutoSize = true;
		this.label_PENELOPE_DUC.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_PENELOPE_DUC.Location = new System.Drawing.Point(369, 200);
		this.label_PENELOPE_DUC.Name = "label_PENELOPE_DUC";
		this.label_PENELOPE_DUC.Size = new System.Drawing.Size(31, 15);
		this.label_PENELOPE_DUC.TabIndex = 382;
		this.label_PENELOPE_DUC.Text = "DUC";
		this.label_PENELOPE_DAC.AutoSize = true;
		this.label_PENELOPE_DAC.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_PENELOPE_DAC.Location = new System.Drawing.Point(371, 183);
		this.label_PENELOPE_DAC.Name = "label_PENELOPE_DAC";
		this.label_PENELOPE_DAC.Size = new System.Drawing.Size(31, 15);
		this.label_PENELOPE_DAC.TabIndex = 381;
		this.label_PENELOPE_DAC.Text = "DAC";
		this.label_METIS_FPGA.AutoSize = true;
		this.label_METIS_FPGA.Font = new System.Drawing.Font("Arial", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_METIS_FPGA.Location = new System.Drawing.Point(536, 58);
		this.label_METIS_FPGA.Name = "label_METIS_FPGA";
		this.label_METIS_FPGA.Size = new System.Drawing.Size(36, 14);
		this.label_METIS_FPGA.TabIndex = 380;
		this.label_METIS_FPGA.Text = "FPGA";
		this.label_METIS.AutoSize = true;
		this.label_METIS.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_METIS.Location = new System.Drawing.Point(533, 39);
		this.label_METIS.Name = "label_METIS";
		this.label_METIS.Size = new System.Drawing.Size(42, 15);
		this.label_METIS.TabIndex = 379;
		this.label_METIS.Text = "METIS";
		this.label_ALEX_2_To_RX.AutoSize = true;
		this.label_ALEX_2_To_RX.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_ALEX_2_To_RX.Location = new System.Drawing.Point(170, 240);
		this.label_ALEX_2_To_RX.Name = "label_ALEX_2_To_RX";
		this.label_ALEX_2_To_RX.Size = new System.Drawing.Size(39, 15);
		this.label_ALEX_2_To_RX.TabIndex = 378;
		this.label_ALEX_2_To_RX.Text = "To RX";
		this.label_ALEX_2_LPF.AutoSize = true;
		this.label_ALEX_2_LPF.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_ALEX_2_LPF.Location = new System.Drawing.Point(170, 208);
		this.label_ALEX_2_LPF.Name = "label_ALEX_2_LPF";
		this.label_ALEX_2_LPF.Size = new System.Drawing.Size(28, 15);
		this.label_ALEX_2_LPF.TabIndex = 377;
		this.label_ALEX_2_LPF.Text = "LPF";
		this.label_ALEX_2_HPF.AutoSize = true;
		this.label_ALEX_2_HPF.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_ALEX_2_HPF.Location = new System.Drawing.Point(177, 183);
		this.label_ALEX_2_HPF.Name = "label_ALEX_2_HPF";
		this.label_ALEX_2_HPF.Size = new System.Drawing.Size(29, 15);
		this.label_ALEX_2_HPF.TabIndex = 376;
		this.label_ALEX_2_HPF.Text = "HPF";
		this.label_ALEX_2.AutoSize = true;
		this.label_ALEX_2.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_ALEX_2.Location = new System.Drawing.Point(170, 163);
		this.label_ALEX_2.Name = "label_ALEX_2";
		this.label_ALEX_2.Size = new System.Drawing.Size(47, 15);
		this.label_ALEX_2.TabIndex = 375;
		this.label_ALEX_2.Text = "ALEX 2";
		this.label_PENELOPE_FPGA.AutoSize = true;
		this.label_PENELOPE_FPGA.Font = new System.Drawing.Font("Arial", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_PENELOPE_FPGA.Location = new System.Drawing.Point(371, 163);
		this.label_PENELOPE_FPGA.Name = "label_PENELOPE_FPGA";
		this.label_PENELOPE_FPGA.Size = new System.Drawing.Size(36, 14);
		this.label_PENELOPE_FPGA.TabIndex = 374;
		this.label_PENELOPE_FPGA.Text = "FPGA";
		this.label_PENELOPE.AutoSize = true;
		this.label_PENELOPE.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_PENELOPE.Location = new System.Drawing.Point(371, 148);
		this.label_PENELOPE.Name = "label_PENELOPE";
		this.label_PENELOPE.Size = new System.Drawing.Size(145, 15);
		this.label_PENELOPE.TabIndex = 373;
		this.label_PENELOPE.Text = "PENELOPE / PENNYLANE";
		this.label_MERCURY_2_FPGA.AutoSize = true;
		this.label_MERCURY_2_FPGA.Font = new System.Drawing.Font("Arial", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_MERCURY_2_FPGA.Location = new System.Drawing.Point(371, 100);
		this.label_MERCURY_2_FPGA.Name = "label_MERCURY_2_FPGA";
		this.label_MERCURY_2_FPGA.Size = new System.Drawing.Size(36, 14);
		this.label_MERCURY_2_FPGA.TabIndex = 372;
		this.label_MERCURY_2_FPGA.Text = "FPGA";
		this.label_MERCURY_2.AutoSize = true;
		this.label_MERCURY_2.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_MERCURY_2.Location = new System.Drawing.Point(371, 83);
		this.label_MERCURY_2.Name = "label_MERCURY_2";
		this.label_MERCURY_2.Size = new System.Drawing.Size(73, 15);
		this.label_MERCURY_2.TabIndex = 371;
		this.label_MERCURY_2.Text = "MERCURY 2";
		this.label_ALEX_To_RX.AutoSize = true;
		this.label_ALEX_To_RX.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_ALEX_To_RX.Location = new System.Drawing.Point(258, 83);
		this.label_ALEX_To_RX.Name = "label_ALEX_To_RX";
		this.label_ALEX_To_RX.Size = new System.Drawing.Size(39, 15);
		this.label_ALEX_To_RX.TabIndex = 370;
		this.label_ALEX_To_RX.Text = "To RX";
		this.label_MERCURY_FPGA.AutoSize = true;
		this.label_MERCURY_FPGA.Font = new System.Drawing.Font("Arial", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_MERCURY_FPGA.Location = new System.Drawing.Point(371, 63);
		this.label_MERCURY_FPGA.Name = "label_MERCURY_FPGA";
		this.label_MERCURY_FPGA.Size = new System.Drawing.Size(36, 14);
		this.label_MERCURY_FPGA.TabIndex = 369;
		this.label_MERCURY_FPGA.Text = "FPGA";
		this.label_MERCURY.AutoSize = true;
		this.label_MERCURY.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_MERCURY.Location = new System.Drawing.Point(371, 45);
		this.label_MERCURY.Name = "label_MERCURY";
		this.label_MERCURY.Size = new System.Drawing.Size(63, 15);
		this.label_MERCURY.TabIndex = 368;
		this.label_MERCURY.Text = "MERCURY";
		this.label_ALEX_LPF.AutoSize = true;
		this.label_ALEX_LPF.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_ALEX_LPF.Location = new System.Drawing.Point(178, 133);
		this.label_ALEX_LPF.Name = "label_ALEX_LPF";
		this.label_ALEX_LPF.Size = new System.Drawing.Size(28, 15);
		this.label_ALEX_LPF.TabIndex = 367;
		this.label_ALEX_LPF.Text = "LPF";
		this.label_ALEX_HPF.AutoSize = true;
		this.label_ALEX_HPF.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_ALEX_HPF.Location = new System.Drawing.Point(178, 83);
		this.label_ALEX_HPF.Name = "label_ALEX_HPF";
		this.label_ALEX_HPF.Size = new System.Drawing.Size(29, 15);
		this.label_ALEX_HPF.TabIndex = 366;
		this.label_ALEX_HPF.Text = "HPF";
		this.label_ALEX.AutoSize = true;
		this.label_ALEX.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_ALEX.Location = new System.Drawing.Point(178, 45);
		this.label_ALEX.Name = "label_ALEX";
		this.label_ALEX.Size = new System.Drawing.Size(37, 15);
		this.label_ALEX.TabIndex = 365;
		this.label_ALEX.Text = "ALEX";
		this.label_DDC6.AutoSize = true;
		this.label_DDC6.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_DDC6.Location = new System.Drawing.Point(735, 348);
		this.label_DDC6.Name = "label_DDC6";
		this.label_DDC6.Size = new System.Drawing.Size(38, 15);
		this.label_DDC6.TabIndex = 364;
		this.label_DDC6.Text = "DDC6";
		this.label_DDC5.AutoSize = true;
		this.label_DDC5.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_DDC5.Location = new System.Drawing.Point(735, 298);
		this.label_DDC5.Name = "label_DDC5";
		this.label_DDC5.Size = new System.Drawing.Size(38, 15);
		this.label_DDC5.TabIndex = 363;
		this.label_DDC5.Text = "DDC5";
		this.label_DDC4.AutoSize = true;
		this.label_DDC4.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_DDC4.Location = new System.Drawing.Point(735, 243);
		this.label_DDC4.Name = "label_DDC4";
		this.label_DDC4.Size = new System.Drawing.Size(38, 15);
		this.label_DDC4.TabIndex = 362;
		this.label_DDC4.Text = "DDC4";
		this.label_DDC3.AutoSize = true;
		this.label_DDC3.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_DDC3.Location = new System.Drawing.Point(735, 200);
		this.label_DDC3.Name = "label_DDC3";
		this.label_DDC3.Size = new System.Drawing.Size(38, 15);
		this.label_DDC3.TabIndex = 361;
		this.label_DDC3.Text = "DDC3";
		this.label_DDC2.AutoSize = true;
		this.label_DDC2.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_DDC2.Location = new System.Drawing.Point(735, 148);
		this.label_DDC2.Name = "label_DDC2";
		this.label_DDC2.Size = new System.Drawing.Size(38, 15);
		this.label_DDC2.TabIndex = 360;
		this.label_DDC2.Text = "DDC2";
		this.label_DDC1.AutoSize = true;
		this.label_DDC1.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_DDC1.Location = new System.Drawing.Point(735, 98);
		this.label_DDC1.Name = "label_DDC1";
		this.label_DDC1.Size = new System.Drawing.Size(38, 15);
		this.label_DDC1.TabIndex = 359;
		this.label_DDC1.Text = "DDC1";
		this.label_DDC0.AutoSize = true;
		this.label_DDC0.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_DDC0.Location = new System.Drawing.Point(735, 45);
		this.label_DDC0.Name = "label_DDC0";
		this.label_DDC0.Size = new System.Drawing.Size(38, 15);
		this.label_DDC0.TabIndex = 358;
		this.label_DDC0.Text = "DDC0";
		this.label_SWR.AutoSize = true;
		this.label_SWR.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_SWR.Location = new System.Drawing.Point(131, 267);
		this.label_SWR.Name = "label_SWR";
		this.label_SWR.Size = new System.Drawing.Size(46, 15);
		this.label_SWR.TabIndex = 357;
		this.label_SWR.Text = "SWR X";
		this.label_RX2_LR_audio.AutoSize = true;
		this.label_RX2_LR_audio.Font = new System.Drawing.Font("Arial", 8.25f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, 0);
		this.label_RX2_LR_audio.Location = new System.Drawing.Point(880, 283);
		this.label_RX2_LR_audio.Name = "label_RX2_LR_audio";
		this.label_RX2_LR_audio.Size = new System.Drawing.Size(78, 14);
		this.label_RX2_LR_audio.TabIndex = 356;
		this.label_RX2_LR_audio.Text = "RX2 L/R audio";
		this.label_RX1_LR_audio.AutoSize = true;
		this.label_RX1_LR_audio.Font = new System.Drawing.Font("Arial", 8.25f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, 0);
		this.label_RX1_LR_audio.Location = new System.Drawing.Point(880, 240);
		this.label_RX1_LR_audio.Name = "label_RX1_LR_audio";
		this.label_RX1_LR_audio.Size = new System.Drawing.Size(78, 14);
		this.label_RX1_LR_audio.TabIndex = 355;
		this.label_RX1_LR_audio.Text = "RX1 L/R audio";
		this.label_AUDIO_MIXER.AutoSize = true;
		this.label_AUDIO_MIXER.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_AUDIO_MIXER.Location = new System.Drawing.Point(1035, 265);
		this.label_AUDIO_MIXER.Name = "label_AUDIO_MIXER";
		this.label_AUDIO_MIXER.Size = new System.Drawing.Size(82, 15);
		this.label_AUDIO_MIXER.TabIndex = 354;
		this.label_AUDIO_MIXER.Text = "AUDIO MIXER";
		this.label_AUDIO_MIXER.UseMnemonic = false;
		this.label_AUDIO_AMP.AutoSize = true;
		this.label_AUDIO_AMP.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_AUDIO_AMP.Location = new System.Drawing.Point(207, 558);
		this.label_AUDIO_AMP.Name = "label_AUDIO_AMP";
		this.label_AUDIO_AMP.Size = new System.Drawing.Size(33, 15);
		this.label_AUDIO_AMP.TabIndex = 353;
		this.label_AUDIO_AMP.Text = "AMP";
		this.label_LR_audio.AutoSize = true;
		this.label_LR_audio.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, 0);
		this.label_LR_audio.Location = new System.Drawing.Point(297, 543);
		this.label_LR_audio.Name = "label_LR_audio";
		this.label_LR_audio.Size = new System.Drawing.Size(60, 15);
		this.label_LR_audio.TabIndex = 352;
		this.label_LR_audio.Text = "L/R audio";
		this.label_L_audio_only.AutoSize = true;
		this.label_L_audio_only.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, 0);
		this.label_L_audio_only.Location = new System.Drawing.Point(110, 543);
		this.label_L_audio_only.Name = "label_L_audio_only";
		this.label_L_audio_only.Size = new System.Drawing.Size(74, 15);
		this.label_L_audio_only.TabIndex = 351;
		this.label_L_audio_only.Text = "L audio only";
		this.label_C25.AutoSize = true;
		this.label_C25.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_C25.Location = new System.Drawing.Point(85, 608);
		this.label_C25.Name = "label_C25";
		this.label_C25.Size = new System.Drawing.Size(29, 15);
		this.label_C25.TabIndex = 350;
		this.label_C25.Text = "C25";
		this.label_CODEC2.AutoSize = true;
		this.label_CODEC2.Font = new System.Drawing.Font("Arial", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_CODEC2.Location = new System.Drawing.Point(461, 550);
		this.label_CODEC2.Name = "label_CODEC2";
		this.label_CODEC2.Size = new System.Drawing.Size(44, 14);
		this.label_CODEC2.TabIndex = 349;
		this.label_CODEC2.Text = "CODEC";
		this.label_C26.AutoSize = true;
		this.label_C26.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_C26.Location = new System.Drawing.Point(85, 633);
		this.label_C26.Name = "label_C26";
		this.label_C26.Size = new System.Drawing.Size(29, 15);
		this.label_C26.TabIndex = 348;
		this.label_C26.Text = "C26";
		this.label_SMA.AutoSize = true;
		this.label_SMA.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label_SMA.Location = new System.Drawing.Point(46, 333);
		this.label_SMA.Name = "label_SMA";
		this.label_SMA.Size = new System.Drawing.Size(39, 15);
		this.label_SMA.TabIndex = 347;
		this.label_SMA.Text = "(SMA)";
		this.label_front_panel.AutoSize = true;
		this.label_front_panel.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, 0);
		this.label_front_panel.Location = new System.Drawing.Point(44, 591);
		this.label_front_panel.Name = "label_front_panel";
		this.label_front_panel.Size = new System.Drawing.Size(65, 15);
		this.label_front_panel.TabIndex = 346;
		this.label_front_panel.Text = "front panel";
		this.label_rear_panel.AutoSize = true;
		this.label_rear_panel.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, 0);
		this.label_rear_panel.Location = new System.Drawing.Point(46, 45);
		this.label_rear_panel.Name = "label_rear_panel";
		this.label_rear_panel.Size = new System.Drawing.Size(63, 15);
		this.label_rear_panel.TabIndex = 345;
		this.label_rear_panel.Text = "rear panel";
		this.label4.AutoSize = true;
		this.label4.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label4.Location = new System.Drawing.Point(598, 483);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(0, 15);
		this.label4.TabIndex = 344;
		this.label3.AutoSize = true;
		this.label3.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label3.Location = new System.Drawing.Point(607, 483);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(0, 15);
		this.label3.TabIndex = 343;
		this.label2.AutoSize = true;
		this.label2.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label2.Location = new System.Drawing.Point(596, 483);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(0, 15);
		this.label2.TabIndex = 342;
		this.label_CODEC.AutoSize = true;
		this.label_CODEC.Font = new System.Drawing.Font("Arial", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_CODEC.Location = new System.Drawing.Point(460, 583);
		this.label_CODEC.Name = "label_CODEC";
		this.label_CODEC.Size = new System.Drawing.Size(44, 14);
		this.label_CODEC.TabIndex = 341;
		this.label_CODEC.Text = "CODEC";
		this.label_C24.AutoSize = true;
		this.label_C24.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_C24.Location = new System.Drawing.Point(166, 522);
		this.label_C24.Name = "label_C24";
		this.label_C24.Size = new System.Drawing.Size(29, 15);
		this.label_C24.TabIndex = 340;
		this.label_C24.Text = "C24";
		this.label_LPF2.AutoSize = true;
		this.label_LPF2.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_LPF2.Location = new System.Drawing.Point(335, 487);
		this.label_LPF2.Name = "label_LPF2";
		this.label_LPF2.Size = new System.Drawing.Size(35, 15);
		this.label_LPF2.TabIndex = 339;
		this.label_LPF2.Text = "LPF2";
		this.label_HPF2.AutoSize = true;
		this.label_HPF2.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_HPF2.Location = new System.Drawing.Point(260, 487);
		this.label_HPF2.Name = "label_HPF2";
		this.label_HPF2.Size = new System.Drawing.Size(36, 15);
		this.label_HPF2.TabIndex = 338;
		this.label_HPF2.Text = "HPF2";
		this.label_C20.AutoSize = true;
		this.label_C20.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_C20.Location = new System.Drawing.Point(85, 532);
		this.label_C20.Name = "label_C20";
		this.label_C20.Size = new System.Drawing.Size(29, 15);
		this.label_C20.TabIndex = 337;
		this.label_C20.Text = "C20";
		this.label_C19.AutoSize = true;
		this.label_C19.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_C19.Location = new System.Drawing.Point(85, 507);
		this.label_C19.Name = "label_C19";
		this.label_C19.Size = new System.Drawing.Size(29, 15);
		this.label_C19.TabIndex = 336;
		this.label_C19.Text = "C19";
		this.label_C18.AutoSize = true;
		this.label_C18.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_C18.Location = new System.Drawing.Point(85, 482);
		this.label_C18.Name = "label_C18";
		this.label_C18.Size = new System.Drawing.Size(29, 15);
		this.label_C18.TabIndex = 335;
		this.label_C18.Text = "C18";
		this.label_C17.AutoSize = true;
		this.label_C17.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_C17.Location = new System.Drawing.Point(85, 457);
		this.label_C17.Name = "label_C17";
		this.label_C17.Size = new System.Drawing.Size(29, 15);
		this.label_C17.TabIndex = 334;
		this.label_C17.Text = "C17";
		this.label_DUC0.AutoSize = true;
		this.label_DUC0.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_DUC0.Location = new System.Drawing.Point(460, 462);
		this.label_DUC0.Name = "label_DUC0";
		this.label_DUC0.Size = new System.Drawing.Size(31, 15);
		this.label_DUC0.TabIndex = 333;
		this.label_DUC0.Text = "DUC";
		this.label_FILTER.AutoSize = true;
		this.label_FILTER.Font = new System.Drawing.Font("Arial", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_FILTER.Location = new System.Drawing.Point(348, 420);
		this.label_FILTER.Name = "label_FILTER";
		this.label_FILTER.Size = new System.Drawing.Size(42, 14);
		this.label_FILTER.TabIndex = 332;
		this.label_FILTER.Text = "FILTER";
		this.label_AMP.AutoSize = true;
		this.label_AMP.Font = new System.Drawing.Font("Arial", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_AMP.Location = new System.Drawing.Point(348, 405);
		this.label_AMP.Name = "label_AMP";
		this.label_AMP.Size = new System.Drawing.Size(41, 14);
		this.label_AMP.TabIndex = 331;
		this.label_AMP.Text = "AMP +";
		this.label_Rx4.AutoSize = true;
		this.label_Rx4.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, 0);
		this.label_Rx4.Location = new System.Drawing.Point(735, 258);
		this.label_Rx4.Name = "label_Rx4";
		this.label_Rx4.Size = new System.Drawing.Size(37, 15);
		this.label_Rx4.TabIndex = 330;
		this.label_Rx4.Text = "(Rx4)";
		this.label_DSP.AutoSize = true;
		this.label_DSP.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_DSP.Location = new System.Drawing.Point(934, 208);
		this.label_DSP.Name = "label_DSP";
		this.label_DSP.Size = new System.Drawing.Size(31, 15);
		this.label_DSP.TabIndex = 329;
		this.label_DSP.Text = "DSP";
		this.label_PC.AutoSize = true;
		this.label_PC.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_PC.Location = new System.Drawing.Point(973, 58);
		this.label_PC.Name = "label_PC";
		this.label_PC.Size = new System.Drawing.Size(23, 15);
		this.label_PC.TabIndex = 328;
		this.label_PC.Text = "PC";
		this.label_FPGA.AutoSize = true;
		this.label_FPGA.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_FPGA.Location = new System.Drawing.Point(492, 58);
		this.label_FPGA.Name = "label_FPGA";
		this.label_FPGA.Size = new System.Drawing.Size(37, 15);
		this.label_FPGA.TabIndex = 327;
		this.label_FPGA.Text = "FPGA";
		this.label_SDR_Hardware.AutoSize = true;
		this.label_SDR_Hardware.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_SDR_Hardware.Location = new System.Drawing.Point(260, 39);
		this.label_SDR_Hardware.Name = "label_SDR_Hardware";
		this.label_SDR_Hardware.Size = new System.Drawing.Size(31, 15);
		this.label_SDR_Hardware.TabIndex = 326;
		this.label_SDR_Hardware.Text = "SDR";
		this.label_hardware_selected.AutoSize = true;
		this.label_hardware_selected.Cursor = System.Windows.Forms.Cursors.Default;
		this.label_hardware_selected.Font = new System.Drawing.Font("Arial", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_hardware_selected.Location = new System.Drawing.Point(13, 8);
		this.label_hardware_selected.Name = "label_hardware_selected";
		this.label_hardware_selected.Size = new System.Drawing.Size(227, 16);
		this.label_hardware_selected.TabIndex = 325;
		this.label_hardware_selected.Text = "Routing for HARDWARE SELECTED";
		this.label_ADC2_atten.AutoSize = true;
		this.label_ADC2_atten.Font = new System.Drawing.Font("Arial", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_ADC2_atten.Location = new System.Drawing.Point(535, 300);
		this.label_ADC2_atten.Name = "label_ADC2_atten";
		this.label_ADC2_atten.Size = new System.Drawing.Size(46, 14);
		this.label_ADC2_atten.TabIndex = 324;
		this.label_ADC2_atten.Text = " ATN2 +";
		this.label_ADC1_atten.AutoSize = true;
		this.label_ADC1_atten.Font = new System.Drawing.Font("Arial", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_ADC1_atten.Location = new System.Drawing.Point(534, 200);
		this.label_ADC1_atten.Name = "label_ADC1_atten";
		this.label_ADC1_atten.Size = new System.Drawing.Size(46, 14);
		this.label_ADC1_atten.TabIndex = 323;
		this.label_ADC1_atten.Text = " ATN1 +";
		this.label_ADC0_atten.AutoSize = true;
		this.label_ADC0_atten.Font = new System.Drawing.Font("Arial", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_ADC0_atten.Location = new System.Drawing.Point(535, 100);
		this.label_ADC0_atten.Name = "label_ADC0_atten";
		this.label_ADC0_atten.Size = new System.Drawing.Size(46, 14);
		this.label_ADC0_atten.TabIndex = 322;
		this.label_ADC0_atten.Text = " ATN0 +";
		this.label_C16.AutoSize = true;
		this.label_C16.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_C16.Location = new System.Drawing.Point(85, 433);
		this.label_C16.Name = "label_C16";
		this.label_C16.Size = new System.Drawing.Size(29, 15);
		this.label_C16.TabIndex = 321;
		this.label_C16.Text = "C16";
		this.label_C15.AutoSize = true;
		this.label_C15.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_C15.Location = new System.Drawing.Point(85, 408);
		this.label_C15.Name = "label_C15";
		this.label_C15.Size = new System.Drawing.Size(29, 15);
		this.label_C15.TabIndex = 320;
		this.label_C15.Text = "C15";
		this.label_C14.AutoSize = true;
		this.label_C14.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_C14.Location = new System.Drawing.Point(85, 383);
		this.label_C14.Name = "label_C14";
		this.label_C14.Size = new System.Drawing.Size(29, 15);
		this.label_C14.TabIndex = 319;
		this.label_C14.Text = "C14";
		this.label_C13.AutoSize = true;
		this.label_C13.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_C13.Location = new System.Drawing.Point(85, 358);
		this.label_C13.Name = "label_C13";
		this.label_C13.Size = new System.Drawing.Size(29, 15);
		this.label_C13.TabIndex = 318;
		this.label_C13.Text = "C13";
		this.label_C12.AutoSize = true;
		this.label_C12.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_C12.Location = new System.Drawing.Point(85, 333);
		this.label_C12.Name = "label_C12";
		this.label_C12.Size = new System.Drawing.Size(29, 15);
		this.label_C12.TabIndex = 317;
		this.label_C12.Text = "C12";
		this.label_C3.AutoSize = true;
		this.label_C3.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_C3.Location = new System.Drawing.Point(85, 108);
		this.label_C3.Name = "label_C3";
		this.label_C3.Size = new System.Drawing.Size(22, 15);
		this.label_C3.TabIndex = 316;
		this.label_C3.Text = "C3";
		this.label_C2.AutoSize = true;
		this.label_C2.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_C2.Location = new System.Drawing.Point(85, 83);
		this.label_C2.Name = "label_C2";
		this.label_C2.Size = new System.Drawing.Size(22, 15);
		this.label_C2.TabIndex = 315;
		this.label_C2.Text = "C2";
		this.label_C1.AutoSize = true;
		this.label_C1.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_C1.Location = new System.Drawing.Point(85, 58);
		this.label_C1.Name = "label_C1";
		this.label_C1.Size = new System.Drawing.Size(22, 15);
		this.label_C1.TabIndex = 314;
		this.label_C1.Text = "C1";
		this.label_HPF.AutoSize = true;
		this.label_HPF.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_HPF.Location = new System.Drawing.Point(260, 213);
		this.label_HPF.Name = "label_HPF";
		this.label_HPF.Size = new System.Drawing.Size(29, 15);
		this.label_HPF.TabIndex = 313;
		this.label_HPF.Text = "HPF";
		this.label_C4.AutoSize = true;
		this.label_C4.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_C4.Location = new System.Drawing.Point(85, 133);
		this.label_C4.Name = "label_C4";
		this.label_C4.Size = new System.Drawing.Size(22, 15);
		this.label_C4.TabIndex = 312;
		this.label_C4.Text = "C4";
		this.label_C11.AutoSize = true;
		this.label_C11.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_C11.Location = new System.Drawing.Point(85, 308);
		this.label_C11.Name = "label_C11";
		this.label_C11.Size = new System.Drawing.Size(28, 15);
		this.label_C11.TabIndex = 311;
		this.label_C11.Text = "C11";
		this.label_C10.AutoSize = true;
		this.label_C10.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_C10.Location = new System.Drawing.Point(85, 283);
		this.label_C10.Name = "label_C10";
		this.label_C10.Size = new System.Drawing.Size(29, 15);
		this.label_C10.TabIndex = 310;
		this.label_C10.Text = "C10";
		this.label_C9.AutoSize = true;
		this.label_C9.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_C9.Location = new System.Drawing.Point(85, 258);
		this.label_C9.Name = "label_C9";
		this.label_C9.Size = new System.Drawing.Size(22, 15);
		this.label_C9.TabIndex = 309;
		this.label_C9.Text = "C9";
		this.label_C8.AutoSize = true;
		this.label_C8.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_C8.Location = new System.Drawing.Point(85, 233);
		this.label_C8.Name = "label_C8";
		this.label_C8.Size = new System.Drawing.Size(22, 15);
		this.label_C8.TabIndex = 308;
		this.label_C8.Text = "C8";
		this.label_PA.AutoSize = true;
		this.label_PA.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_PA.Location = new System.Drawing.Point(335, 313);
		this.label_PA.Name = "label_PA";
		this.label_PA.Size = new System.Drawing.Size(22, 15);
		this.label_PA.TabIndex = 307;
		this.label_PA.Text = "PA";
		this.label_DAC0.AutoSize = true;
		this.label_DAC0.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_DAC0.Location = new System.Drawing.Point(460, 413);
		this.label_DAC0.Name = "label_DAC0";
		this.label_DAC0.Size = new System.Drawing.Size(32, 13);
		this.label_DAC0.TabIndex = 306;
		this.label_DAC0.Text = "DAC";
		this.label_LPF.AutoSize = true;
		this.label_LPF.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_LPF.Location = new System.Drawing.Point(335, 213);
		this.label_LPF.Name = "label_LPF";
		this.label_LPF.Size = new System.Drawing.Size(28, 15);
		this.label_LPF.TabIndex = 305;
		this.label_LPF.Text = "LPF";
		this.label_RX2_DISPLAY.AutoSize = true;
		this.label_RX2_DISPLAY.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_RX2_DISPLAY.Location = new System.Drawing.Point(1035, 213);
		this.label_RX2_DISPLAY.Name = "label_RX2_DISPLAY";
		this.label_RX2_DISPLAY.Size = new System.Drawing.Size(81, 15);
		this.label_RX2_DISPLAY.TabIndex = 304;
		this.label_RX2_DISPLAY.Text = "RX2 DISPLAY";
		this.label_RX2_DISPLAY.UseMnemonic = false;
		this.label_RX1_DISPLAY.AutoSize = true;
		this.label_RX1_DISPLAY.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_RX1_DISPLAY.Location = new System.Drawing.Point(1035, 113);
		this.label_RX1_DISPLAY.Name = "label_RX1_DISPLAY";
		this.label_RX1_DISPLAY.Size = new System.Drawing.Size(87, 13);
		this.label_RX1_DISPLAY.TabIndex = 303;
		this.label_RX1_DISPLAY.Text = "RX1 DISPLAY";
		this.label_RX1_DISPLAY.UseMnemonic = false;
		this.label_Rx6.AutoSize = true;
		this.label_Rx6.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, 0);
		this.label_Rx6.Location = new System.Drawing.Point(735, 363);
		this.label_Rx6.Name = "label_Rx6";
		this.label_Rx6.Size = new System.Drawing.Size(37, 15);
		this.label_Rx6.TabIndex = 302;
		this.label_Rx6.Text = "(Rx6)";
		this.label_Rx5.AutoSize = true;
		this.label_Rx5.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, 0);
		this.label_Rx5.Location = new System.Drawing.Point(735, 313);
		this.label_Rx5.Name = "label_Rx5";
		this.label_Rx5.Size = new System.Drawing.Size(37, 15);
		this.label_Rx5.TabIndex = 301;
		this.label_Rx5.Text = "(Rx5)";
		this.label_Rx3.AutoSize = true;
		this.label_Rx3.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, 0);
		this.label_Rx3.Location = new System.Drawing.Point(735, 213);
		this.label_Rx3.Name = "label_Rx3";
		this.label_Rx3.Size = new System.Drawing.Size(37, 15);
		this.label_Rx3.TabIndex = 300;
		this.label_Rx3.Text = "(Rx3)";
		this.label_Rx2.AutoSize = true;
		this.label_Rx2.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, 0);
		this.label_Rx2.Location = new System.Drawing.Point(735, 163);
		this.label_Rx2.Name = "label_Rx2";
		this.label_Rx2.Size = new System.Drawing.Size(37, 15);
		this.label_Rx2.TabIndex = 299;
		this.label_Rx2.Text = "(Rx2)";
		this.label_Rx1.AutoSize = true;
		this.label_Rx1.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, 0);
		this.label_Rx1.Location = new System.Drawing.Point(735, 113);
		this.label_Rx1.Name = "label_Rx1";
		this.label_Rx1.Size = new System.Drawing.Size(37, 15);
		this.label_Rx1.TabIndex = 298;
		this.label_Rx1.Text = "(Rx1)";
		this.label_Rx0.AutoSize = true;
		this.label_Rx0.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, 0);
		this.label_Rx0.Location = new System.Drawing.Point(735, 63);
		this.label_Rx0.Name = "label_Rx0";
		this.label_Rx0.Size = new System.Drawing.Size(37, 15);
		this.label_Rx0.TabIndex = 297;
		this.label_Rx0.Text = "(Rx0)";
		this.label_ADC2.AutoSize = true;
		this.label_ADC2.Font = new System.Drawing.Font("Arial", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_ADC2.Location = new System.Drawing.Point(535, 313);
		this.label_ADC2.Name = "label_ADC2";
		this.label_ADC2.Size = new System.Drawing.Size(36, 14);
		this.label_ADC2.TabIndex = 296;
		this.label_ADC2.Text = "ADC2";
		this.label_ADC1.AutoSize = true;
		this.label_ADC1.Font = new System.Drawing.Font("Arial", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_ADC1.Location = new System.Drawing.Point(535, 213);
		this.label_ADC1.Name = "label_ADC1";
		this.label_ADC1.Size = new System.Drawing.Size(36, 14);
		this.label_ADC1.TabIndex = 295;
		this.label_ADC1.Text = "ADC1";
		this.label_ADC0.AutoSize = true;
		this.label_ADC0.Font = new System.Drawing.Font("Arial", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_ADC0.Location = new System.Drawing.Point(539, 113);
		this.label_ADC0.Name = "label_ADC0";
		this.label_ADC0.Size = new System.Drawing.Size(36, 14);
		this.label_ADC0.TabIndex = 294;
		this.label_ADC0.Text = "ADC0";
		this.label_C7.AutoSize = true;
		this.label_C7.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_C7.Location = new System.Drawing.Point(85, 208);
		this.label_C7.Name = "label_C7";
		this.label_C7.Size = new System.Drawing.Size(22, 15);
		this.label_C7.TabIndex = 293;
		this.label_C7.Text = "C7";
		this.label_C6.AutoSize = true;
		this.label_C6.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_C6.Location = new System.Drawing.Point(85, 183);
		this.label_C6.Name = "label_C6";
		this.label_C6.Size = new System.Drawing.Size(22, 15);
		this.label_C6.TabIndex = 292;
		this.label_C6.Text = "C6";
		this.label_C5.AutoSize = true;
		this.label_C5.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label_C5.Location = new System.Drawing.Point(85, 158);
		this.label_C5.Name = "label_C5";
		this.label_C5.Size = new System.Drawing.Size(22, 15);
		this.label_C5.TabIndex = 291;
		this.label_C5.Text = "C5";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		base.ClientSize = new System.Drawing.Size(1166, 791);
		base.Controls.Add(this.canvas);
		base.Controls.Add(this.panel1);
		base.Name = "Path_Illustrator";
		base.ShowIcon = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Path_Illustrator v1.2.4 (19 Dec 2015)";
		this.panel1.ResumeLayout(false);
		this.panel1.PerformLayout();
		this.groupBox10.ResumeLayout(false);
		this.groupBox10.PerformLayout();
		this.canvas.ResumeLayout(false);
		this.canvas.PerformLayout();
		base.ResumeLayout(false);
	}
}
