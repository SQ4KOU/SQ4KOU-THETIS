using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Thetis;

public class RAForm : Form
{
	private Console console;

	private BinaryWriter writer;

	private string RA_version = "Radio Astronomy data collection utility, v1.3 (16 Feb 2016)";

	private byte[] RA_data;

	private byte[] header_data;

	private double time0;

	private byte[] CRLF;

	private int RA_count;

	private int max_count = 1;

	private int data_points;

	private float sig_level;

	private float sig_level2;

	private double sig_level_pwr;

	private double sig_level2_pwr;

	private float sig_level_avg;

	private float sig_level2_avg;

	private float sig_max;

	private float sig_min;

	private float display_y_max;

	private float display_y_min = -150f;

	private float display_x_max = 100000f;

	private float display_x_min;

	private int Y_range;

	private int X_range;

	private bool rescale;

	private float[] sig_data = new float[2000000];

	private float[] sig_data2 = new float[2000000];

	private float[] time_data = new float[2000000];

	private double time_elapsed;

	private double ref_power;

	private string s2 = "";

	private NumberFormatInfo nfi = NumberFormatInfo.InvariantInfo;

	private GroupBoxTS groupBox2;

	private CheckBoxTS RArecordCheckBox;

	private GroupBoxTS groupBoxTS1;

	private IContainer components;

	private NumericUpDownTS numericUpDown_measurements_per_point;

	private Timer RA_timer;

	private TextBoxTS textBox_pts_collected;

	private GroupBoxTS groupBoxTS2;

	private NumericUpDownTS numericUpDown_mSec_between_measurements;

	private GroupBoxTS groupBoxTS3;

	private TextBoxTS textBox_Rx1;

	private TextBoxTS textBoxTS3;

	private PictureBox picRAGraph;

	private GroupBoxTS groupBoxTS4;

	private GroupBoxTS groupBox_signal;

	private LabelTS labelTS1;

	private GroupBoxTS groupBoxTS6;

	private RadioButtonTS button_dBm;

	private RadioButtonTS button_linear;

	private LabelTS txtCursorPower;

	private LabelTS txtCursorTime;

	private LabelTS labelTS3;

	private LabelTS labelTS2;

	private NumericUpDownTS manual_ymin;

	private RadioButtonTS auto_rescale;

	private GroupBoxTS groupBoxTS5;

	private GroupBoxTS groupBox_scaling;

	private GroupBoxTS groupBoxTS7;

	private RadioButtonTS manual_rescale;

	private LabelTS labelTS4;

	private LabelTS labelTS5;

	private GroupBoxTS groupBoxTS8;

	private LabelTS labelTS7;

	private NumericUpDownTS manual_xmin;

	private LabelTS labelTS6;

	private NumericUpDownTS manual_xmax;

	private LabelTS labelTS8;

	private LabelTS labelTS9;

	private LabelTS labelTS10;

	private NumericUpDownTS manual_ymax;

	private LabelTS labelTS12;

	private LabelTS labelTS13;

	private ButtonTS button_readFile;

	private OpenFileDialog openFileDialog3;

	private ButtonTS button_writeFile;

	private SaveFileDialog saveFileDialog1;

	private GroupBoxTS groupBox_Rx2;

	private TextBoxTS textBox_Rx2;

	private GroupBoxTS groupBox_Rx_select;

	private RadioButtonTS radioButton_Rx2_only;

	private RadioButtonTS radioButton_Rx1_only;

	private RadioButtonTS radioButton_both;

	private TextBox textBox_file_date_time;

	private TextBox textBox_file_comment;

	public RAForm(Console c)
	{
		InitializeComponent();
		console = c;
		CRLF = new byte[2] { 13, 10 };
		RArecordCheckBox.BackColor = Color.DarkGreen;
		RArecordCheckBox.ForeColor = Color.White;
		RArecordCheckBox.Text = "Start";
		labelTS10.Text = "";
		max_count = 1;
		textBox_file_date_time.Visible = false;
		textBox_file_comment.Visible = false;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void RArecordCheckBox_CheckedChanged(object sender, EventArgs e)
	{
		header_data = new byte[60];
		if (RArecordCheckBox.Checked)
		{
			RArecordCheckBox.BackColor = Color.LimeGreen;
			RArecordCheckBox.ForeColor = Color.Black;
			RArecordCheckBox.Text = "Stop";
			string appDataPath = console.AppDataPath;
			try
			{
				writer = new BinaryWriter(File.Open(appDataPath + "RA_data.csv", FileMode.Create));
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			textBox_file_date_time.Visible = false;
			textBox_file_comment.Visible = false;
			construct_header();
			time0 = Environment.TickCount;
			sig_level = console.NewMeterData;
			sig_level2 = console.Rx2MeterData;
			sig_level_pwr = Math.Pow(10.0, sig_level);
			sig_level2_pwr = Math.Pow(10.0, sig_level2);
			sig_max = sig_level + 10f;
			sig_min = sig_max - 10f;
			rescale = true;
			RA_count = 1;
			for (int i = 0; i < 10000; i++)
			{
				sig_data[i] = 0f;
				time_data[i] = 0f;
			}
			data_points = 1;
			picRAGraph.Invalidate();
			labelTS10.Text = "";
			button_readFile.Enabled = false;
			RA_timer.Enabled = true;
		}
		else
		{
			RA_timer.Enabled = false;
			RArecordCheckBox.BackColor = Color.DarkGreen;
			RArecordCheckBox.ForeColor = Color.White;
			RArecordCheckBox.Text = "Start";
			writer.Flush();
			writer.Close();
			labelTS10.BackColor = SystemColors.InactiveCaptionText;
			labelTS10.ForeColor = SystemColors.Highlight;
			labelTS10.Text = "data written to: RA_data.csv";
			button_readFile.Enabled = true;
		}
	}

	private void construct_header()
	{
		write_line_to_file(RA_version);
		write_line_to_file("");
		write_line_to_file(Convert.ToString(DateTime.Now));
		write_line_to_file("");
		write_line_to_file("Comment: none");
		write_line_to_file("");
		write_line_to_file("mSec between measurements, # measurements averaged per point");
		write_line_to_file(numericUpDown_mSec_between_measurements.Text + ", " + numericUpDown_measurements_per_point.Text);
		write_line_to_file("");
		write_line_to_file("time (seconds); RX1 signal (dBm); RX2 signal (dBm)");
	}

	private void write_line_to_file(string s)
	{
		header_data = StrToByteArray(s);
		int count = header_data.Length;
		writer.Write(header_data, 0, count);
		writer.Write(CRLF, 0, 2);
	}

	private void numericUpDownTS1_ValueChanged(object sender, EventArgs e)
	{
		max_count = (int)numericUpDown_measurements_per_point.Value;
	}

	private void RA_timer_Tick(object sender, EventArgs e)
	{
		RA_data = new byte[30];
		if (RA_count == max_count)
		{
			RA_timer.Enabled = false;
			data_points++;
			textBox_pts_collected.Text = data_points.ToString();
			double num = Environment.TickCount;
			time_elapsed = 0.001 * (num - time0);
			string text = string.Format(nfi, "{0:F3}", (float)time_elapsed);
			sig_level_pwr /= RA_count;
			sig_level2_pwr /= RA_count;
			sig_level_avg = (float)Math.Log10(sig_level_pwr);
			sig_level2_avg = (float)Math.Log10(sig_level2_pwr);
			sig_data[data_points] = sig_level_avg;
			sig_data2[data_points] = sig_level2_avg;
			time_data[data_points] = (float)time_elapsed;
			if (sig_level_avg > sig_max)
			{
				sig_max = sig_level_avg;
				rescale = true;
			}
			if (sig_min > sig_level_avg)
			{
				sig_min = sig_level_avg;
				rescale = true;
			}
			textBoxTS3.Text = string.Format(nfi, "{0:F1}", (float)time_elapsed);
			textBox_Rx1.Text = string.Format(nfi, "{0:F1}", sig_level_avg);
			textBox_Rx2.Text = string.Format(nfi, "{0:F1}", sig_level2_avg);
			string text2 = string.Format(nfi, "{0:F2}", sig_level_avg);
			string text3 = string.Format(nfi, "{0:F2}", sig_level2_avg);
			string str = text + ", " + text2 + ", " + text3;
			RA_data = StrToByteArray(str);
			int count = RA_data.Length;
			writer.Write(RA_data, 0, count);
			writer.Write(CRLF, 0, 2);
			sig_level = console.NewMeterData;
			sig_level2 = console.Rx2MeterData;
			sig_level_pwr = Math.Pow(10.0, sig_level);
			sig_level2_pwr = Math.Pow(10.0, sig_level2);
			RA_count = 1;
			picRAGraph.Invalidate();
			if (RArecordCheckBox.Checked)
			{
				RA_timer.Enabled = true;
			}
		}
		else
		{
			sig_level_pwr += Math.Pow(10.0, console.NewMeterData);
			sig_level2_pwr += Math.Pow(10.0, console.Rx2MeterData);
			RA_count++;
		}
	}

	private void numericUpDownTS2_ValueChanged(object sender, EventArgs e)
	{
		RA_timer.Interval = (int)numericUpDown_mSec_between_measurements.Value;
	}

	private void picRAGraph_Paint(object sender, PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		new Rectangle(0, 0, picRAGraph.Width, picRAGraph.Height);
		Math.Min(picRAGraph.Width, picRAGraph.Height);
		Pen pen = new Pen(Brushes.Black);
		new Pen(Brushes.Yellow);
		new Pen(Brushes.White);
		Pen pen2 = new Pen(Brushes.LightGray);
		Pen pen3 = new Pen(Brushes.Red);
		graphics.CompositingQuality = CompositingQuality.HighQuality;
		graphics.InterpolationMode = InterpolationMode.Bicubic;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
		graphics.FillRectangle(Brushes.AliceBlue, 0f, 0f, picRAGraph.Width, picRAGraph.Height);
		graphics.DrawRectangle(pen, 0, 0, picRAGraph.Width - 1, picRAGraph.Height - 1);
		new Point(picRAGraph.Left, picRAGraph.Top);
		Point point = new Point(picRAGraph.Left, picRAGraph.Bottom);
		Point point2 = new Point(picRAGraph.Right, picRAGraph.Top);
		Point point3 = new Point(picRAGraph.Right, picRAGraph.Bottom);
		Point point4 = default(Point);
		Point pt = default(Point);
		Point pt2 = default(Point);
		Point pt3 = default(Point);
		int num = point.X;
		int num2 = point3.X;
		int num3 = point2.Y;
		int num4 = point3.Y;
		int num5 = 0;
		float num6 = 0f;
		float num7 = 0f;
		double num8 = 0.0;
		double num9 = 0.0;
		int num10 = 0;
		int num11 = 0;
		Font font = new Font("Microsft Sans Serif", 10f);
		SolidBrush brush = new SolidBrush(Color.Black);
		Y_range = num4 - num3;
		float num12;
		if (button_dBm.Checked)
		{
			labelTS12.Text = "                  Signal (dBm)";
			if (auto_rescale.Checked)
			{
				groupBox_scaling.Enabled = false;
				if (rescale)
				{
					display_y_max = sig_max + 10f;
					display_y_min = sig_min - 10f;
				}
			}
			else
			{
				groupBox_scaling.Enabled = true;
				display_y_max = (float)manual_ymax.Value;
				display_y_min = (float)manual_ymin.Value;
			}
			num12 = display_y_max - display_y_min;
			if (num12 > 30f)
			{
				num8 = Math.Truncate(display_y_min / 10f) * 10.0;
				num6 = 10f;
			}
			else if (num12 > 15f)
			{
				num8 = Math.Truncate(display_y_min / 5f) * 5.0;
				num6 = 5f;
			}
			else
			{
				num8 = Math.Truncate(display_y_min);
				num6 = 1f;
			}
			num5 = (int)Math.Truncate((double)num12 / (double)num6) + 1;
			num10 = Y_range + (int)(((double)display_y_min - num8) / (double)num12 * (double)Y_range);
		}
		else
		{
			ref_power = Math.Pow(10.0, (double)sig_max / 10.0);
			this.s2 = string.Format(nfi, "{0:F1}", (float)ref_power);
			labelTS12.Text = "Signal ( x " + this.s2 + " mW)";
			float num13 = (float)Math.Pow(10.0, sig_max / 10f) / (float)ref_power;
			_ = (float)Math.Pow(10.0, sig_min / 10f) / (float)ref_power;
			if (auto_rescale.Checked)
			{
				groupBox_scaling.Enabled = false;
				if (rescale)
				{
					display_y_max = 1.1f * num13;
					display_y_min = 0f;
				}
			}
			else
			{
				groupBox_scaling.Enabled = true;
				display_y_max = (float)manual_ymax.Value;
				display_y_min = (float)manual_ymin.Value;
			}
			if (display_y_max <= 0f)
			{
				display_y_max = 100f;
			}
			if (display_y_min < 0f)
			{
				display_y_min = 0f;
			}
			num12 = display_y_max - display_y_min;
			if (num12 > 500f)
			{
				num8 = Math.Truncate(display_y_min / 100f) * 100.0;
				num6 = 100f;
			}
			else if (num12 > 300f)
			{
				num8 = Math.Truncate(display_y_min / 50f) * 50.0;
				num6 = 50f;
			}
			else if (num12 > 200f)
			{
				num8 = Math.Truncate(display_y_min / 20f) * 20.0;
				num6 = 20f;
			}
			else if (num12 > 30f)
			{
				num8 = Math.Truncate(display_y_min / 10f) * 10.0;
				num6 = 10f;
			}
			else if (num12 > 15f)
			{
				num8 = Math.Truncate(display_y_min / 5f) * 5.0;
				num6 = 5f;
			}
			else
			{
				num8 = Math.Truncate(display_y_min);
				num6 = 1f;
			}
			num5 = (int)Math.Truncate((double)num12 / (double)num6) + 1;
			num10 = Y_range + (int)(((double)display_y_min - num8) / (double)num12 * (double)Y_range);
		}
		float num14 = (float)num8;
		for (int i = 0; i < num5; i++)
		{
			int num15 = num10 - (int)((float)i * num6 / num12 * (float)Y_range);
			point4.X = 0;
			point4.Y = num15;
			pt.X = num2;
			pt.Y = num15;
			graphics.DrawLine(pen2, point4, pt);
			point4.Y -= (int)(0.027 * (double)Y_range);
			string s = string.Format(nfi, "{0:F0}", num14);
			graphics.DrawString(s, font, brush, point4);
			num14 += num6;
		}
		float num16 = display_x_max - display_x_min;
		X_range = num2 - num;
		if (num16 > 40000f)
		{
			num9 = Math.Truncate(display_x_min / 10000f) * 10000.0;
			num7 = 10000f;
		}
		else if (num16 > 6000f)
		{
			num9 = Math.Truncate(display_x_min / 1000f) * 1000.0;
			num7 = 1000f;
		}
		else if (num16 > 3000f)
		{
			num9 = Math.Truncate(display_x_min / 500f) * 500.0;
			num7 = 500f;
		}
		else if (num16 > 1500f)
		{
			num9 = Math.Truncate(display_x_min / 100f) * 100.0;
			num7 = 200f;
		}
		else if (num16 > 500f)
		{
			num9 = Math.Truncate(display_x_min / 100f) * 100.0;
			num7 = 100f;
		}
		else if (num16 > 300f)
		{
			num9 = Math.Truncate(display_x_min / 50f) * 50.0;
			num7 = 50f;
		}
		else if (num16 > 200f)
		{
			num9 = Math.Truncate(display_x_min / 20f) * 20.0;
			num7 = 20f;
		}
		else if (num16 > 60f)
		{
			num9 = Math.Truncate(display_x_min / 10f) * 10.0;
			num7 = 10f;
		}
		else if (num16 > 15f)
		{
			num9 = Math.Truncate(display_x_min / 5f) * 5.0;
			num7 = 5f;
		}
		else
		{
			num9 = Math.Truncate(display_x_min);
			num7 = 1f;
		}
		num5 = (int)Math.Truncate((double)num16 / (double)num7) + 1;
		num11 = (int)((num9 - (double)display_x_min) / (double)num16 * (double)X_range);
		float num17 = (float)num9;
		for (int i = 0; i < num5; i++)
		{
			int num18 = (point4.X = num11 + (int)((float)i * num7 / num16 * (float)X_range));
			point4.Y = 0;
			pt.X = num18;
			pt.Y = num4;
			graphics.DrawLine(pen2, point4, pt);
			point4.X -= (int)(0.015 * (double)X_range);
			point4.Y = Y_range - (int)(0.05 * (double)Y_range);
			string s2 = string.Format(nfi, "{0:F0}", num17);
			graphics.DrawString(s2, font, brush, point4);
			num17 += num7;
		}
		labelTS4.Text = string.Format(nfi, "{0:F1}", display_y_max);
		labelTS5.Text = string.Format(nfi, "{0:F1}", display_y_min);
		labelTS8.Text = string.Format(nfi, "{0:F0}", display_x_min);
		labelTS9.Text = string.Format(nfi, "{0:F0}", display_x_max);
		_ = time_elapsed / 600.0;
		float num20 = 0f;
		float num21 = 0f;
		float num22 = 0f;
		float num23 = 0f;
		for (int i = 1; i <= data_points; i++)
		{
			if (button_dBm.Checked)
			{
				num20 = sig_data[i];
				num21 = sig_data[i - 1];
				num22 = sig_data2[i];
				num23 = sig_data2[i - 1];
			}
			else if (sig_data[i] == 0f)
			{
				num20 = 0f;
				num21 = 0f;
				num22 = 0f;
				num23 = 0f;
			}
			else
			{
				if (sig_data[2] == 0f)
				{
					sig_data[2] = sig_data[i];
				}
				if (sig_data2[2] == 0f)
				{
					sig_data2[2] = sig_data2[i];
				}
				num20 = 100f * (float)(Math.Pow(10.0, sig_data[i] / 10f) / ref_power);
				num21 = 100f * (float)(Math.Pow(10.0, sig_data[i - 1] / 10f) / ref_power);
				num22 = 100f * (float)(Math.Pow(10.0, sig_data2[i] / 10f) / ref_power);
				num23 = 100f * (float)(Math.Pow(10.0, sig_data2[i - 1] / 10f) / ref_power);
			}
			pt.Y = (int)((display_y_max - num20) / (display_y_max - display_y_min) * (float)Y_range);
			pt3.Y = (int)((display_y_max - num22) / (display_y_max - display_y_min) * (float)Y_range);
			if (button_linear.Checked)
			{
				if (manual_ymin.Value < 0m)
				{
					manual_ymin.BackColor = Color.Yellow;
				}
				else
				{
					manual_ymin.BackColor = SystemColors.Window;
				}
				if ((manual_ymin.Value < 0m) | (manual_ymax.Value <= manual_ymin.Value))
				{
					manual_ymin.BackColor = Color.Yellow;
					manual_ymax.BackColor = SystemColors.Window;
				}
				else
				{
					manual_ymin.BackColor = SystemColors.Window;
				}
			}
			if (pt.Y >= Y_range)
			{
				pt.Y = Y_range - 1;
			}
			if (pt.Y <= 0)
			{
				pt.Y = 1;
			}
			if (pt3.Y >= Y_range)
			{
				pt3.Y = Y_range - 1;
			}
			if (pt3.Y <= 0)
			{
				pt3.Y = 1;
			}
			pt.X = (int)((time_data[i] - display_x_min) / (display_x_max - display_x_min) * (float)X_range);
			pt3.X = pt.X;
			point4.Y = (int)((display_y_max - num21) / (display_y_max - display_y_min) * (float)Y_range);
			pt2.Y = (int)((display_y_max - num23) / (display_y_max - display_y_min) * (float)Y_range);
			if (point4.Y > Y_range)
			{
				point4.Y = Y_range - 1;
			}
			if (point4.Y < 0)
			{
				point4.Y = 1;
			}
			if (pt2.Y > Y_range)
			{
				pt2.Y = Y_range - 1;
			}
			if (pt2.Y < 0)
			{
				pt2.Y = 1;
			}
			point4.X = (int)((time_data[i - 1] - display_x_min) / (display_x_max - display_x_min) * (float)X_range);
			pt2.X = point4.X;
			if (radioButton_both.Checked)
			{
				graphics.DrawLine(pen, point4, pt);
				graphics.DrawLine(pen3, pt2, pt3);
			}
			if (radioButton_Rx1_only.Checked)
			{
				graphics.DrawLine(pen, point4, pt);
			}
			if (radioButton_Rx2_only.Checked)
			{
				graphics.DrawLine(pen3, pt2, pt3);
			}
		}
		if (data_points >= 1999999)
		{
			RA_timer.Enabled = false;
			string s = "Maximum # data points collected...acquisition halted";
			font = new Font("Microsft Sans Serif", 18f);
			brush = new SolidBrush(Color.Red);
			Point point5 = new Point(70, 150);
			graphics.DrawString(s, font, brush, point5);
			RArecordCheckBox.Checked = false;
		}
		rescale = false;
	}

	private void button_dBm_CheckedChanged(object sender, EventArgs e)
	{
		if (button_dBm.Checked)
		{
			labelTS3.Text = "manual Ymax (dBm)";
			labelTS2.Text = "manual Ymin (dBm)";
			manual_ymax.BackColor = SystemColors.Window;
			manual_ymin.BackColor = SystemColors.Window;
			manual_ymax.Value = 0m;
			manual_ymin.Value = -150m;
		}
		else
		{
			labelTS3.Text = "manual Ymax";
			labelTS2.Text = "manual Ymin";
			manual_ymax.Value = 110m;
			manual_ymin.Value = 0m;
		}
		picRAGraph.Invalidate();
	}

	private void button_linear_CheckedChanged(object sender, EventArgs e)
	{
		s2 = string.Format(nfi, "{0:E1}", (float)ref_power);
		picRAGraph.Invalidate();
	}

	private void picRAGraph_MouseMove(object sender, MouseEventArgs e)
	{
		float num = (float)e.Y / (float)Y_range * (display_y_min - display_y_max) + display_y_max;
		float num2 = (float)e.X / (float)X_range * (display_x_max - display_x_min) + display_x_min;
		if (button_dBm.Checked)
		{
			txtCursorPower.Text = string.Format(nfi, "{0:F1}", num) + " dBm";
		}
		else
		{
			txtCursorPower.Text = string.Format(nfi, "{0:F1}", num) + " x " + s2 + " mW";
		}
		txtCursorTime.Text = string.Format(nfi, "{0:F1}", num2) + " seconds";
	}

	private void RAForm_MouseMove(object sender, MouseEventArgs e)
	{
		txtCursorPower.Text = "";
		txtCursorTime.Text = "";
	}

	private void button_readFile_Click(object sender, EventArgs e)
	{
		textBox_file_date_time.Visible = true;
		textBox_file_comment.Visible = true;
		string currentDirectory = Environment.CurrentDirectory;
		openFileDialog3.InitialDirectory = currentDirectory;
		openFileDialog3.FileName = "RA_data.csv";
		openFileDialog3.Filter = "RA data files (*.csv) | *.csv|All files (*.*)|*.*";
		if (openFileDialog3.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		StreamReader streamReader = new StreamReader(openFileDialog3.FileName);
		int num = 0;
		string text = streamReader.ReadLine();
		for (int i = 0; i < 8; i++)
		{
			text = streamReader.ReadLine();
			if (i == 1)
			{
				textBox_file_date_time.Text = "Date/time:  " + text;
			}
			if (i == 3)
			{
				textBox_file_comment.Text = text;
			}
			if (i == 6)
			{
				string[] array = text.Split(',');
				MyTryParse(array[0], out var result);
				MyTryParse(array[1], out var result2);
				numericUpDown_mSec_between_measurements.Text = Convert.ToString(result);
				numericUpDown_measurements_per_point.Text = Convert.ToString(result2);
			}
		}
		while (!streamReader.EndOfStream)
		{
			num++;
			text = streamReader.ReadLine();
			string[] array2 = text.Split(',');
			if (num > 1)
			{
				MyTryParse(array2[0], out time_data[num]);
				MyTryParse(array2[1], out sig_data[num]);
				MyTryParse(array2[2], out sig_data2[num]);
			}
		}
		streamReader.Close();
		data_points = num - 1;
		labelTS10.Text = "";
		labelTS10.BackColor = SystemColors.InactiveCaptionText;
		labelTS10.ForeColor = SystemColors.Highlight;
		textBox_pts_collected.Text = data_points.ToString("f0");
		if (data_points > 1)
		{
			manual_xmax.Value = (decimal)time_data[num];
		}
		else
		{
			labelTS10.ForeColor = Color.White;
			labelTS10.BackColor = Color.Red;
			labelTS10.Text = "NO VALID DATA IN THE FILE";
		}
		sig_max = -140f;
		for (int j = 2; j < data_points; j++)
		{
			if ((sig_data[j] > sig_max) & (sig_data[j] < 0f))
			{
				sig_max = sig_data[j];
			}
			if ((sig_data2[j] > sig_max) & (sig_data2[j] < 0f))
			{
				sig_max = sig_data2[j];
			}
		}
		picRAGraph.Invalidate();
	}

	private bool MyTryParse(string inValue, out float result)
	{
		try
		{
			result = Convert.ToSingle(inValue, nfi);
			return true;
		}
		catch
		{
			result = 0f;
			return false;
		}
	}

	private void button_writeFile_Click(object sender, EventArgs e)
	{
		string text = " ";
		string text2 = "";
		string currentDirectory = Environment.CurrentDirectory;
		saveFileDialog1.InitialDirectory = currentDirectory;
		saveFileDialog1.Filter = "RA data file (*.csv) | *.csv";
		DialogResult num = saveFileDialog1.ShowDialog();
		string fileName = saveFileDialog1.FileName;
		if (num != DialogResult.OK)
		{
			return;
		}
		StreamWriter streamWriter = new StreamWriter(fileName);
		for (int i = 0; i <= data_points; i++)
		{
			if (i < 1)
			{
				streamWriter.WriteLine(RA_version);
				streamWriter.WriteLine("");
				streamWriter.WriteLine(Convert.ToString(DateTime.Now));
				streamWriter.WriteLine("");
				string text3 = Prompt.ShowDialog("Optional comment:", "Comment dialog window");
				streamWriter.WriteLine("Comment: " + text3);
				streamWriter.WriteLine("");
				text2 = "mSec between measurements, # measurements averaged per point";
				streamWriter.WriteLine(text2);
				text2 = numericUpDown_mSec_between_measurements.Text + ", " + numericUpDown_measurements_per_point.Text;
				streamWriter.WriteLine(text2);
				streamWriter.WriteLine("");
				text = "time (seconds); Rx1 signal (dBm); Rx2 signal (dBm)";
			}
			else
			{
				text = string.Format(nfi, "{0:F3}", time_data[i]) + ", " + string.Format(nfi, "{0:F3}", sig_data[i]) + ", " + string.Format(nfi, "{0:F3}", sig_data2[i]);
			}
			streamWriter.WriteLine(text);
		}
		streamWriter.Close();
	}

	private void manual_ymax_ValueChanged(object sender, EventArgs e)
	{
		if (manual_ymax.Value <= manual_ymin.Value)
		{
			manual_ymax.Value = manual_ymin.Value + 1m;
		}
		display_y_max = (float)manual_ymax.Value;
		picRAGraph.Invalidate();
	}

	private void manual_ymin_ValueChanged(object sender, EventArgs e)
	{
		if (button_linear.Checked & (manual_ymin.Value < 0m))
		{
			manual_ymin.Value = 0m;
		}
		if (manual_ymin.Value >= manual_ymax.Value)
		{
			manual_ymin.Value = manual_ymax.Value - 1m;
		}
		display_y_min = (float)manual_ymin.Value;
		picRAGraph.Invalidate();
	}

	private void manual_xmax_ValueChanged(object sender, EventArgs e)
	{
		if (manual_xmax.Value <= manual_xmin.Value)
		{
			manual_xmax.Value = manual_xmin.Value + 1m;
		}
		display_x_max = (float)manual_xmax.Value;
		picRAGraph.Invalidate();
	}

	private void manual_xmin_ValueChanged(object sender, EventArgs e)
	{
		if (manual_xmin.Value >= manual_xmax.Value)
		{
			manual_xmin.Value = manual_xmax.Value - 1m;
		}
		display_x_min = (float)manual_xmin.Value;
		picRAGraph.Invalidate();
	}

	private byte[] StrToByteArray(string str)
	{
		return new UTF8Encoding().GetBytes(str);
	}

	private void RAForm_Load(object sender, EventArgs e)
	{
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Thetis.RAForm));
		this.RA_timer = new System.Windows.Forms.Timer(this.components);
		this.picRAGraph = new System.Windows.Forms.PictureBox();
		this.openFileDialog3 = new System.Windows.Forms.OpenFileDialog();
		this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
		this.textBox_file_date_time = new System.Windows.Forms.TextBox();
		this.textBox_file_comment = new System.Windows.Forms.TextBox();
		this.groupBox_Rx_select = new System.Windows.Forms.GroupBoxTS();
		this.radioButton_Rx2_only = new System.Windows.Forms.RadioButtonTS();
		this.radioButton_Rx1_only = new System.Windows.Forms.RadioButtonTS();
		this.radioButton_both = new System.Windows.Forms.RadioButtonTS();
		this.groupBox_Rx2 = new System.Windows.Forms.GroupBoxTS();
		this.textBox_Rx2 = new System.Windows.Forms.TextBoxTS();
		this.button_writeFile = new System.Windows.Forms.ButtonTS();
		this.button_readFile = new System.Windows.Forms.ButtonTS();
		this.labelTS13 = new System.Windows.Forms.LabelTS();
		this.labelTS12 = new System.Windows.Forms.LabelTS();
		this.labelTS10 = new System.Windows.Forms.LabelTS();
		this.labelTS9 = new System.Windows.Forms.LabelTS();
		this.labelTS8 = new System.Windows.Forms.LabelTS();
		this.groupBoxTS8 = new System.Windows.Forms.GroupBoxTS();
		this.labelTS7 = new System.Windows.Forms.LabelTS();
		this.manual_xmin = new System.Windows.Forms.NumericUpDownTS();
		this.labelTS6 = new System.Windows.Forms.LabelTS();
		this.manual_xmax = new System.Windows.Forms.NumericUpDownTS();
		this.labelTS5 = new System.Windows.Forms.LabelTS();
		this.labelTS4 = new System.Windows.Forms.LabelTS();
		this.txtCursorTime = new System.Windows.Forms.LabelTS();
		this.txtCursorPower = new System.Windows.Forms.LabelTS();
		this.groupBoxTS6 = new System.Windows.Forms.GroupBoxTS();
		this.groupBoxTS7 = new System.Windows.Forms.GroupBoxTS();
		this.manual_rescale = new System.Windows.Forms.RadioButtonTS();
		this.auto_rescale = new System.Windows.Forms.RadioButtonTS();
		this.groupBox_scaling = new System.Windows.Forms.GroupBoxTS();
		this.manual_ymax = new System.Windows.Forms.NumericUpDownTS();
		this.manual_ymin = new System.Windows.Forms.NumericUpDownTS();
		this.labelTS2 = new System.Windows.Forms.LabelTS();
		this.labelTS3 = new System.Windows.Forms.LabelTS();
		this.groupBoxTS5 = new System.Windows.Forms.GroupBoxTS();
		this.button_linear = new System.Windows.Forms.RadioButtonTS();
		this.button_dBm = new System.Windows.Forms.RadioButtonTS();
		this.groupBox_signal = new System.Windows.Forms.GroupBoxTS();
		this.textBox_Rx1 = new System.Windows.Forms.TextBoxTS();
		this.groupBoxTS4 = new System.Windows.Forms.GroupBoxTS();
		this.textBoxTS3 = new System.Windows.Forms.TextBoxTS();
		this.groupBoxTS3 = new System.Windows.Forms.GroupBoxTS();
		this.numericUpDown_mSec_between_measurements = new System.Windows.Forms.NumericUpDownTS();
		this.groupBoxTS2 = new System.Windows.Forms.GroupBoxTS();
		this.labelTS1 = new System.Windows.Forms.LabelTS();
		this.textBox_pts_collected = new System.Windows.Forms.TextBoxTS();
		this.groupBoxTS1 = new System.Windows.Forms.GroupBoxTS();
		this.numericUpDown_measurements_per_point = new System.Windows.Forms.NumericUpDownTS();
		this.groupBox2 = new System.Windows.Forms.GroupBoxTS();
		this.RArecordCheckBox = new System.Windows.Forms.CheckBoxTS();
		((System.ComponentModel.ISupportInitialize)this.picRAGraph).BeginInit();
		this.groupBox_Rx_select.SuspendLayout();
		this.groupBox_Rx2.SuspendLayout();
		this.groupBoxTS8.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.manual_xmin).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.manual_xmax).BeginInit();
		this.groupBoxTS6.SuspendLayout();
		this.groupBoxTS7.SuspendLayout();
		this.groupBox_scaling.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.manual_ymax).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.manual_ymin).BeginInit();
		this.groupBoxTS5.SuspendLayout();
		this.groupBox_signal.SuspendLayout();
		this.groupBoxTS4.SuspendLayout();
		this.groupBoxTS3.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown_mSec_between_measurements).BeginInit();
		this.groupBoxTS2.SuspendLayout();
		this.groupBoxTS1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown_measurements_per_point).BeginInit();
		this.groupBox2.SuspendLayout();
		base.SuspendLayout();
		this.RA_timer.Interval = 50;
		this.RA_timer.Tick += new System.EventHandler(RA_timer_Tick);
		this.picRAGraph.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.picRAGraph.Location = new System.Drawing.Point(196, 76);
		this.picRAGraph.Name = "picRAGraph";
		this.picRAGraph.Size = new System.Drawing.Size(738, 429);
		this.picRAGraph.TabIndex = 17;
		this.picRAGraph.TabStop = false;
		this.picRAGraph.Paint += new System.Windows.Forms.PaintEventHandler(picRAGraph_Paint);
		this.picRAGraph.MouseMove += new System.Windows.Forms.MouseEventHandler(picRAGraph_MouseMove);
		this.openFileDialog3.FileName = "openFileDialog3";
		this.textBox_file_date_time.Location = new System.Drawing.Point(226, 77);
		this.textBox_file_date_time.Name = "textBox_file_date_time";
		this.textBox_file_date_time.Size = new System.Drawing.Size(181, 20);
		this.textBox_file_date_time.TabIndex = 40;
		this.textBox_file_date_time.Visible = false;
		this.textBox_file_comment.Location = new System.Drawing.Point(226, 98);
		this.textBox_file_comment.Multiline = true;
		this.textBox_file_comment.Name = "textBox_file_comment";
		this.textBox_file_comment.Size = new System.Drawing.Size(707, 20);
		this.textBox_file_comment.TabIndex = 41;
		this.textBox_file_comment.Visible = false;
		this.groupBox_Rx_select.BackColor = System.Drawing.SystemColors.ControlDarkDark;
		this.groupBox_Rx_select.Controls.Add(this.radioButton_Rx2_only);
		this.groupBox_Rx_select.Controls.Add(this.radioButton_Rx1_only);
		this.groupBox_Rx_select.Controls.Add(this.radioButton_both);
		this.groupBox_Rx_select.ForeColor = System.Drawing.SystemColors.Control;
		this.groupBox_Rx_select.Location = new System.Drawing.Point(12, 271);
		this.groupBox_Rx_select.Name = "groupBox_Rx_select";
		this.groupBox_Rx_select.Size = new System.Drawing.Size(90, 83);
		this.groupBox_Rx_select.TabIndex = 39;
		this.groupBox_Rx_select.TabStop = false;
		this.groupBox_Rx_select.Text = "display";
		this.radioButton_Rx2_only.AutoSize = true;
		this.radioButton_Rx2_only.Image = null;
		this.radioButton_Rx2_only.Location = new System.Drawing.Point(6, 55);
		this.radioButton_Rx2_only.Name = "radioButton_Rx2_only";
		this.radioButton_Rx2_only.Size = new System.Drawing.Size(66, 17);
		this.radioButton_Rx2_only.TabIndex = 10;
		this.radioButton_Rx2_only.Text = "Rx2 only";
		this.radioButton_Rx2_only.UseVisualStyleBackColor = true;
		this.radioButton_Rx1_only.AutoSize = true;
		this.radioButton_Rx1_only.Image = null;
		this.radioButton_Rx1_only.Location = new System.Drawing.Point(5, 33);
		this.radioButton_Rx1_only.Name = "radioButton_Rx1_only";
		this.radioButton_Rx1_only.Size = new System.Drawing.Size(66, 17);
		this.radioButton_Rx1_only.TabIndex = 9;
		this.radioButton_Rx1_only.Text = "Rx1 only";
		this.radioButton_Rx1_only.UseVisualStyleBackColor = true;
		this.radioButton_both.AutoSize = true;
		this.radioButton_both.Checked = true;
		this.radioButton_both.Image = null;
		this.radioButton_both.Location = new System.Drawing.Point(4, 14);
		this.radioButton_both.Name = "radioButton_both";
		this.radioButton_both.Size = new System.Drawing.Size(47, 17);
		this.radioButton_both.TabIndex = 3;
		this.radioButton_both.TabStop = true;
		this.radioButton_both.Text = "Both";
		this.radioButton_both.UseVisualStyleBackColor = true;
		this.groupBox_Rx2.BackColor = System.Drawing.SystemColors.ControlDarkDark;
		this.groupBox_Rx2.Controls.Add(this.textBox_Rx2);
		this.groupBox_Rx2.ForeColor = System.Drawing.SystemColors.Control;
		this.groupBox_Rx2.Location = new System.Drawing.Point(734, 14);
		this.groupBox_Rx2.Name = "groupBox_Rx2";
		this.groupBox_Rx2.Size = new System.Drawing.Size(200, 56);
		this.groupBox_Rx2.TabIndex = 38;
		this.groupBox_Rx2.TabStop = false;
		this.groupBox_Rx2.Text = "Rx2 signal (dBm)";
		this.textBox_Rx2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.textBox_Rx2.Location = new System.Drawing.Point(38, 17);
		this.textBox_Rx2.Name = "textBox_Rx2";
		this.textBox_Rx2.Size = new System.Drawing.Size(124, 29);
		this.textBox_Rx2.TabIndex = 13;
		this.textBox_Rx2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.button_writeFile.BackColor = System.Drawing.SystemColors.ControlDarkDark;
		this.button_writeFile.Image = null;
		this.button_writeFile.Location = new System.Drawing.Point(12, 482);
		this.button_writeFile.Name = "button_writeFile";
		this.button_writeFile.Selectable = true;
		this.button_writeFile.Size = new System.Drawing.Size(127, 23);
		this.button_writeFile.TabIndex = 36;
		this.button_writeFile.Text = "write  data  file";
		this.button_writeFile.UseVisualStyleBackColor = false;
		this.button_writeFile.Click += new System.EventHandler(button_writeFile_Click);
		this.button_readFile.BackColor = System.Drawing.SystemColors.ControlDarkDark;
		this.button_readFile.Image = null;
		this.button_readFile.Location = new System.Drawing.Point(12, 457);
		this.button_readFile.Name = "button_readFile";
		this.button_readFile.Selectable = true;
		this.button_readFile.Size = new System.Drawing.Size(127, 23);
		this.button_readFile.TabIndex = 35;
		this.button_readFile.Text = "read  data  file";
		this.button_readFile.UseVisualStyleBackColor = false;
		this.button_readFile.Click += new System.EventHandler(button_readFile_Click);
		this.labelTS13.AutoSize = true;
		this.labelTS13.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
		this.labelTS13.ForeColor = System.Drawing.SystemColors.Control;
		this.labelTS13.Image = null;
		this.labelTS13.Location = new System.Drawing.Point(560, 514);
		this.labelTS13.Name = "labelTS13";
		this.labelTS13.Size = new System.Drawing.Size(79, 13);
		this.labelTS13.TabIndex = 33;
		this.labelTS13.Text = "Time (seconds)";
		this.labelTS13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.labelTS12.AutoSize = true;
		this.labelTS12.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
		this.labelTS12.ForeColor = System.Drawing.SystemColors.Control;
		this.labelTS12.Image = null;
		this.labelTS12.Location = new System.Drawing.Point(64, 301);
		this.labelTS12.Name = "labelTS12";
		this.labelTS12.Size = new System.Drawing.Size(126, 13);
		this.labelTS12.TabIndex = 32;
		this.labelTS12.Text = "                    Signal (dBm)";
		this.labelTS12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.labelTS10.AutoSize = true;
		this.labelTS10.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
		this.labelTS10.ForeColor = System.Drawing.SystemColors.Highlight;
		this.labelTS10.Image = null;
		this.labelTS10.Location = new System.Drawing.Point(9, 512);
		this.labelTS10.Name = "labelTS10";
		this.labelTS10.Size = new System.Drawing.Size(55, 13);
		this.labelTS10.TabIndex = 30;
		this.labelTS10.Text = "labelTS10";
		this.labelTS10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelTS9.AutoSize = true;
		this.labelTS9.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
		this.labelTS9.ForeColor = System.Drawing.SystemColors.Control;
		this.labelTS9.Image = null;
		this.labelTS9.Location = new System.Drawing.Point(911, 512);
		this.labelTS9.Name = "labelTS9";
		this.labelTS9.Size = new System.Drawing.Size(49, 13);
		this.labelTS9.TabIndex = 29;
		this.labelTS9.Text = "labelTS9";
		this.labelTS9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.labelTS8.AutoSize = true;
		this.labelTS8.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
		this.labelTS8.ForeColor = System.Drawing.SystemColors.Control;
		this.labelTS8.Image = null;
		this.labelTS8.Location = new System.Drawing.Point(197, 512);
		this.labelTS8.Name = "labelTS8";
		this.labelTS8.Size = new System.Drawing.Size(49, 13);
		this.labelTS8.TabIndex = 28;
		this.labelTS8.Text = "labelTS8";
		this.labelTS8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.groupBoxTS8.BackColor = System.Drawing.SystemColors.ControlDarkDark;
		this.groupBoxTS8.Controls.Add(this.labelTS7);
		this.groupBoxTS8.Controls.Add(this.manual_xmin);
		this.groupBoxTS8.Controls.Add(this.labelTS6);
		this.groupBoxTS8.Controls.Add(this.manual_xmax);
		this.groupBoxTS8.ForeColor = System.Drawing.SystemColors.Control;
		this.groupBoxTS8.Location = new System.Drawing.Point(948, 388);
		this.groupBoxTS8.Name = "groupBoxTS8";
		this.groupBoxTS8.Size = new System.Drawing.Size(122, 117);
		this.groupBoxTS8.TabIndex = 27;
		this.groupBoxTS8.TabStop = false;
		this.groupBoxTS8.Text = "X axis";
		this.labelTS7.AutoSize = true;
		this.labelTS7.Image = null;
		this.labelTS7.Location = new System.Drawing.Point(18, 66);
		this.labelTS7.Name = "labelTS7";
		this.labelTS7.Size = new System.Drawing.Size(79, 13);
		this.labelTS7.TabIndex = 11;
		this.labelTS7.Text = "Xmin (seconds)";
		this.manual_xmin.Increment = new decimal(new int[4] { 1, 0, 0, 0 });
		this.manual_xmin.Location = new System.Drawing.Point(15, 81);
		this.manual_xmin.Maximum = new decimal(new int[4] { 99999, 0, 0, 0 });
		this.manual_xmin.Minimum = new decimal(new int[4]);
		this.manual_xmin.Name = "manual_xmin";
		this.manual_xmin.Size = new System.Drawing.Size(86, 20);
		this.manual_xmin.TabIndex = 10;
		this.manual_xmin.TinyStep = false;
		this.manual_xmin.Value = new decimal(new int[4]);
		this.manual_xmin.ValueChanged += new System.EventHandler(manual_xmin_ValueChanged);
		this.labelTS6.AutoSize = true;
		this.labelTS6.Image = null;
		this.labelTS6.Location = new System.Drawing.Point(16, 19);
		this.labelTS6.Name = "labelTS6";
		this.labelTS6.Size = new System.Drawing.Size(82, 13);
		this.labelTS6.TabIndex = 9;
		this.labelTS6.Text = "Xmax (seconds)";
		this.manual_xmax.Increment = new decimal(new int[4] { 1, 0, 0, 0 });
		this.manual_xmax.Location = new System.Drawing.Point(14, 35);
		this.manual_xmax.Maximum = new decimal(new int[4] { 100000, 0, 0, 0 });
		this.manual_xmax.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.manual_xmax.Name = "manual_xmax";
		this.manual_xmax.Size = new System.Drawing.Size(86, 20);
		this.manual_xmax.TabIndex = 8;
		this.manual_xmax.TinyStep = false;
		this.manual_xmax.Value = new decimal(new int[4] { 1000, 0, 0, 0 });
		this.manual_xmax.ValueChanged += new System.EventHandler(manual_xmax_ValueChanged);
		this.labelTS5.AutoSize = true;
		this.labelTS5.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
		this.labelTS5.ForeColor = System.Drawing.SystemColors.Control;
		this.labelTS5.Image = null;
		this.labelTS5.Location = new System.Drawing.Point(145, 492);
		this.labelTS5.Name = "labelTS5";
		this.labelTS5.Size = new System.Drawing.Size(49, 13);
		this.labelTS5.TabIndex = 26;
		this.labelTS5.Text = "labelTS5";
		this.labelTS5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.labelTS4.AutoSize = true;
		this.labelTS4.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
		this.labelTS4.ForeColor = System.Drawing.SystemColors.Control;
		this.labelTS4.Image = null;
		this.labelTS4.Location = new System.Drawing.Point(154, 74);
		this.labelTS4.Name = "labelTS4";
		this.labelTS4.Size = new System.Drawing.Size(49, 13);
		this.labelTS4.TabIndex = 25;
		this.labelTS4.Text = "labelTS4";
		this.labelTS4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.txtCursorTime.AutoSize = true;
		this.txtCursorTime.BackColor = System.Drawing.SystemColors.ControlDarkDark;
		this.txtCursorTime.Image = null;
		this.txtCursorTime.Location = new System.Drawing.Point(252, 513);
		this.txtCursorTime.Name = "txtCursorTime";
		this.txtCursorTime.Size = new System.Drawing.Size(10, 13);
		this.txtCursorTime.TabIndex = 22;
		this.txtCursorTime.Text = " ";
		this.txtCursorPower.AutoSize = true;
		this.txtCursorPower.BackColor = System.Drawing.SystemColors.ControlDarkDark;
		this.txtCursorPower.Image = null;
		this.txtCursorPower.Location = new System.Drawing.Point(378, 513);
		this.txtCursorPower.Name = "txtCursorPower";
		this.txtCursorPower.Size = new System.Drawing.Size(10, 13);
		this.txtCursorPower.TabIndex = 21;
		this.txtCursorPower.Text = " ";
		this.groupBoxTS6.BackColor = System.Drawing.SystemColors.ControlDarkDark;
		this.groupBoxTS6.Controls.Add(this.groupBoxTS7);
		this.groupBoxTS6.Controls.Add(this.groupBox_scaling);
		this.groupBoxTS6.Controls.Add(this.groupBoxTS5);
		this.groupBoxTS6.ForeColor = System.Drawing.SystemColors.Control;
		this.groupBoxTS6.Location = new System.Drawing.Point(954, 74);
		this.groupBoxTS6.Name = "groupBoxTS6";
		this.groupBoxTS6.Size = new System.Drawing.Size(122, 267);
		this.groupBoxTS6.TabIndex = 20;
		this.groupBoxTS6.TabStop = false;
		this.groupBoxTS6.Text = "Y axis";
		this.groupBoxTS7.Controls.Add(this.manual_rescale);
		this.groupBoxTS7.Controls.Add(this.auto_rescale);
		this.groupBoxTS7.ForeColor = System.Drawing.SystemColors.Control;
		this.groupBoxTS7.Location = new System.Drawing.Point(8, 81);
		this.groupBoxTS7.Name = "groupBoxTS7";
		this.groupBoxTS7.Size = new System.Drawing.Size(108, 59);
		this.groupBoxTS7.TabIndex = 10;
		this.groupBoxTS7.TabStop = false;
		this.groupBoxTS7.Text = "scaling mode";
		this.manual_rescale.AutoSize = true;
		this.manual_rescale.Checked = true;
		this.manual_rescale.Image = null;
		this.manual_rescale.Location = new System.Drawing.Point(5, 32);
		this.manual_rescale.Name = "manual_rescale";
		this.manual_rescale.Size = new System.Drawing.Size(59, 17);
		this.manual_rescale.TabIndex = 9;
		this.manual_rescale.TabStop = true;
		this.manual_rescale.Text = "manual";
		this.manual_rescale.UseVisualStyleBackColor = true;
		this.auto_rescale.AutoSize = true;
		this.auto_rescale.Image = null;
		this.auto_rescale.Location = new System.Drawing.Point(4, 14);
		this.auto_rescale.Name = "auto_rescale";
		this.auto_rescale.Size = new System.Drawing.Size(71, 17);
		this.auto_rescale.TabIndex = 3;
		this.auto_rescale.Text = "automatic";
		this.auto_rescale.UseVisualStyleBackColor = true;
		this.groupBox_scaling.Controls.Add(this.manual_ymax);
		this.groupBox_scaling.Controls.Add(this.manual_ymin);
		this.groupBox_scaling.Controls.Add(this.labelTS2);
		this.groupBox_scaling.Controls.Add(this.labelTS3);
		this.groupBox_scaling.ForeColor = System.Drawing.SystemColors.Control;
		this.groupBox_scaling.Location = new System.Drawing.Point(10, 146);
		this.groupBox_scaling.Name = "groupBox_scaling";
		this.groupBox_scaling.Size = new System.Drawing.Size(106, 110);
		this.groupBox_scaling.TabIndex = 8;
		this.groupBox_scaling.TabStop = false;
		this.groupBox_scaling.Text = "manual scaling";
		this.manual_ymax.Increment = new decimal(new int[4] { 1, 0, 0, 0 });
		this.manual_ymax.Location = new System.Drawing.Point(9, 36);
		this.manual_ymax.Maximum = new decimal(new int[4] { 1000, 0, 0, 0 });
		this.manual_ymax.Minimum = new decimal(new int[4] { 140, 0, 0, -2147483648 });
		this.manual_ymax.Name = "manual_ymax";
		this.manual_ymax.Size = new System.Drawing.Size(86, 20);
		this.manual_ymax.TabIndex = 32;
		this.manual_ymax.TinyStep = false;
		this.manual_ymax.Value = new decimal(new int[4]);
		this.manual_ymax.ValueChanged += new System.EventHandler(manual_ymax_ValueChanged);
		this.manual_ymin.Increment = new decimal(new int[4] { 1, 0, 0, 0 });
		this.manual_ymin.Location = new System.Drawing.Point(7, 79);
		this.manual_ymin.Maximum = new decimal(new int[4] { 1000, 0, 0, 0 });
		this.manual_ymin.Minimum = new decimal(new int[4] { 150, 0, 0, -2147483648 });
		this.manual_ymin.Name = "manual_ymin";
		this.manual_ymin.Size = new System.Drawing.Size(86, 20);
		this.manual_ymin.TabIndex = 4;
		this.manual_ymin.TinyStep = false;
		this.manual_ymin.Value = new decimal(new int[4] { 150, 0, 0, -2147483648 });
		this.manual_ymin.ValueChanged += new System.EventHandler(manual_ymin_ValueChanged);
		this.labelTS2.AutoSize = true;
		this.labelTS2.Image = null;
		this.labelTS2.Location = new System.Drawing.Point(5, 65);
		this.labelTS2.Name = "labelTS2";
		this.labelTS2.Size = new System.Drawing.Size(97, 13);
		this.labelTS2.TabIndex = 5;
		this.labelTS2.Text = "manual Ymin (dBm)";
		this.labelTS3.AutoSize = true;
		this.labelTS3.Image = null;
		this.labelTS3.Location = new System.Drawing.Point(2, 21);
		this.labelTS3.Name = "labelTS3";
		this.labelTS3.Size = new System.Drawing.Size(100, 13);
		this.labelTS3.TabIndex = 6;
		this.labelTS3.Text = "manual Ymax (dBm)";
		this.groupBoxTS5.Controls.Add(this.button_linear);
		this.groupBoxTS5.Controls.Add(this.button_dBm);
		this.groupBoxTS5.Location = new System.Drawing.Point(7, 13);
		this.groupBoxTS5.Name = "groupBoxTS5";
		this.groupBoxTS5.Size = new System.Drawing.Size(106, 66);
		this.groupBoxTS5.TabIndex = 2;
		this.groupBoxTS5.TabStop = false;
		this.button_linear.AutoSize = true;
		this.button_linear.Image = null;
		this.button_linear.Location = new System.Drawing.Point(5, 34);
		this.button_linear.Name = "button_linear";
		this.button_linear.Size = new System.Drawing.Size(50, 17);
		this.button_linear.TabIndex = 0;
		this.button_linear.Text = "linear";
		this.button_linear.UseVisualStyleBackColor = true;
		this.button_linear.CheckedChanged += new System.EventHandler(button_linear_CheckedChanged);
		this.button_dBm.AutoSize = true;
		this.button_dBm.Checked = true;
		this.button_dBm.Image = null;
		this.button_dBm.Location = new System.Drawing.Point(5, 12);
		this.button_dBm.Name = "button_dBm";
		this.button_dBm.Size = new System.Drawing.Size(46, 17);
		this.button_dBm.TabIndex = 1;
		this.button_dBm.TabStop = true;
		this.button_dBm.Text = "dBm";
		this.button_dBm.UseVisualStyleBackColor = true;
		this.button_dBm.CheckedChanged += new System.EventHandler(button_dBm_CheckedChanged);
		this.groupBox_signal.BackColor = System.Drawing.SystemColors.ControlDarkDark;
		this.groupBox_signal.Controls.Add(this.textBox_Rx1);
		this.groupBox_signal.ForeColor = System.Drawing.SystemColors.Control;
		this.groupBox_signal.Location = new System.Drawing.Point(512, 13);
		this.groupBox_signal.Name = "groupBox_signal";
		this.groupBox_signal.Size = new System.Drawing.Size(200, 56);
		this.groupBox_signal.TabIndex = 19;
		this.groupBox_signal.TabStop = false;
		this.groupBox_signal.Text = "Rx1 signal (dBm)";
		this.textBox_Rx1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.textBox_Rx1.Location = new System.Drawing.Point(38, 17);
		this.textBox_Rx1.Name = "textBox_Rx1";
		this.textBox_Rx1.Size = new System.Drawing.Size(124, 29);
		this.textBox_Rx1.TabIndex = 13;
		this.textBox_Rx1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.groupBoxTS4.BackColor = System.Drawing.SystemColors.ControlDarkDark;
		this.groupBoxTS4.Controls.Add(this.textBoxTS3);
		this.groupBoxTS4.ForeColor = System.Drawing.SystemColors.Control;
		this.groupBoxTS4.Location = new System.Drawing.Point(200, 8);
		this.groupBoxTS4.Name = "groupBoxTS4";
		this.groupBoxTS4.Size = new System.Drawing.Size(184, 56);
		this.groupBoxTS4.TabIndex = 18;
		this.groupBoxTS4.TabStop = false;
		this.groupBoxTS4.Text = "elapsed time (sec)";
		this.textBoxTS3.Font = new System.Drawing.Font("Microsoft Sans Serif", 14f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.textBoxTS3.Location = new System.Drawing.Point(26, 17);
		this.textBoxTS3.Name = "textBoxTS3";
		this.textBoxTS3.Size = new System.Drawing.Size(124, 29);
		this.textBoxTS3.TabIndex = 14;
		this.textBoxTS3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.groupBoxTS3.BackColor = System.Drawing.SystemColors.ControlDarkDark;
		this.groupBoxTS3.Controls.Add(this.numericUpDown_mSec_between_measurements);
		this.groupBoxTS3.ForeColor = System.Drawing.SystemColors.Control;
		this.groupBoxTS3.Location = new System.Drawing.Point(12, 121);
		this.groupBoxTS3.Name = "groupBoxTS3";
		this.groupBoxTS3.Size = new System.Drawing.Size(127, 59);
		this.groupBoxTS3.TabIndex = 12;
		this.groupBoxTS3.TabStop = false;
		this.groupBoxTS3.Text = "mSec between measurements";
		this.numericUpDown_mSec_between_measurements.Increment = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDown_mSec_between_measurements.Location = new System.Drawing.Point(27, 31);
		this.numericUpDown_mSec_between_measurements.Maximum = new decimal(new int[4] { 1000, 0, 0, 0 });
		this.numericUpDown_mSec_between_measurements.Minimum = new decimal(new int[4] { 50, 0, 0, 0 });
		this.numericUpDown_mSec_between_measurements.Name = "numericUpDown_mSec_between_measurements";
		this.numericUpDown_mSec_between_measurements.Size = new System.Drawing.Size(63, 20);
		this.numericUpDown_mSec_between_measurements.TabIndex = 11;
		this.numericUpDown_mSec_between_measurements.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown_mSec_between_measurements.TinyStep = false;
		this.numericUpDown_mSec_between_measurements.Value = new decimal(new int[4] { 50, 0, 0, 0 });
		this.numericUpDown_mSec_between_measurements.ValueChanged += new System.EventHandler(numericUpDownTS2_ValueChanged);
		this.groupBoxTS2.BackColor = System.Drawing.SystemColors.ControlDarkDark;
		this.groupBoxTS2.Controls.Add(this.labelTS1);
		this.groupBoxTS2.Controls.Add(this.textBox_pts_collected);
		this.groupBoxTS2.ForeColor = System.Drawing.SystemColors.Control;
		this.groupBoxTS2.Location = new System.Drawing.Point(12, 375);
		this.groupBoxTS2.Name = "groupBoxTS2";
		this.groupBoxTS2.Size = new System.Drawing.Size(127, 76);
		this.groupBoxTS2.TabIndex = 10;
		this.groupBoxTS2.TabStop = false;
		this.groupBoxTS2.Text = "# of averaged  points collected/saved";
		this.labelTS1.AutoSize = true;
		this.labelTS1.Image = null;
		this.labelTS1.Location = new System.Drawing.Point(22, 53);
		this.labelTS1.Name = "labelTS1";
		this.labelTS1.Size = new System.Drawing.Size(83, 13);
		this.labelTS1.TabIndex = 20;
		this.labelTS1.Text = "(2,000,000 max)";
		this.textBox_pts_collected.Location = new System.Drawing.Point(13, 29);
		this.textBox_pts_collected.Name = "textBox_pts_collected";
		this.textBox_pts_collected.Size = new System.Drawing.Size(100, 20);
		this.textBox_pts_collected.TabIndex = 9;
		this.textBox_pts_collected.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.groupBoxTS1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
		this.groupBoxTS1.Controls.Add(this.numericUpDown_measurements_per_point);
		this.groupBoxTS1.ForeColor = System.Drawing.SystemColors.Control;
		this.groupBoxTS1.Location = new System.Drawing.Point(12, 187);
		this.groupBoxTS1.Name = "groupBoxTS1";
		this.groupBoxTS1.Size = new System.Drawing.Size(127, 65);
		this.groupBoxTS1.TabIndex = 8;
		this.groupBoxTS1.TabStop = false;
		this.groupBoxTS1.Text = "# of measurements to average per point";
		this.numericUpDown_measurements_per_point.Increment = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDown_measurements_per_point.Location = new System.Drawing.Point(27, 35);
		this.numericUpDown_measurements_per_point.Maximum = new decimal(new int[4] { 2000, 0, 0, 0 });
		this.numericUpDown_measurements_per_point.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDown_measurements_per_point.Name = "numericUpDown_measurements_per_point";
		this.numericUpDown_measurements_per_point.Size = new System.Drawing.Size(69, 20);
		this.numericUpDown_measurements_per_point.TabIndex = 0;
		this.numericUpDown_measurements_per_point.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown_measurements_per_point.TinyStep = false;
		this.numericUpDown_measurements_per_point.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDown_measurements_per_point.ValueChanged += new System.EventHandler(numericUpDownTS1_ValueChanged);
		this.groupBox2.BackColor = System.Drawing.SystemColors.ControlDarkDark;
		this.groupBox2.Controls.Add(this.RArecordCheckBox);
		this.groupBox2.ForeColor = System.Drawing.SystemColors.Control;
		this.groupBox2.Location = new System.Drawing.Point(12, 8);
		this.groupBox2.Name = "groupBox2";
		this.groupBox2.Size = new System.Drawing.Size(127, 64);
		this.groupBox2.TabIndex = 6;
		this.groupBox2.TabStop = false;
		this.groupBox2.Text = "Acquire Data";
		this.RArecordCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
		this.RArecordCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.RArecordCheckBox.ForeColor = System.Drawing.SystemColors.Control;
		this.RArecordCheckBox.Image = null;
		this.RArecordCheckBox.Location = new System.Drawing.Point(9, 17);
		this.RArecordCheckBox.Name = "RArecordCheckBox";
		this.RArecordCheckBox.Size = new System.Drawing.Size(106, 39);
		this.RArecordCheckBox.TabIndex = 0;
		this.RArecordCheckBox.Text = "Start";
		this.RArecordCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.RArecordCheckBox.CheckedChanged += new System.EventHandler(RArecordCheckBox_CheckedChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
		base.ClientSize = new System.Drawing.Size(1088, 562);
		base.Controls.Add(this.textBox_file_comment);
		base.Controls.Add(this.textBox_file_date_time);
		base.Controls.Add(this.groupBox_Rx_select);
		base.Controls.Add(this.groupBox_Rx2);
		base.Controls.Add(this.button_writeFile);
		base.Controls.Add(this.button_readFile);
		base.Controls.Add(this.labelTS13);
		base.Controls.Add(this.labelTS12);
		base.Controls.Add(this.labelTS10);
		base.Controls.Add(this.labelTS9);
		base.Controls.Add(this.labelTS8);
		base.Controls.Add(this.groupBoxTS8);
		base.Controls.Add(this.labelTS5);
		base.Controls.Add(this.labelTS4);
		base.Controls.Add(this.txtCursorTime);
		base.Controls.Add(this.txtCursorPower);
		base.Controls.Add(this.groupBoxTS6);
		base.Controls.Add(this.groupBox_signal);
		base.Controls.Add(this.groupBoxTS4);
		base.Controls.Add(this.picRAGraph);
		base.Controls.Add(this.groupBoxTS3);
		base.Controls.Add(this.groupBoxTS2);
		base.Controls.Add(this.groupBoxTS1);
		base.Controls.Add(this.groupBox2);
		this.ForeColor = System.Drawing.SystemColors.Control;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.Name = "RAForm";
		this.Text = "Radio Astromony data collection utility   v1.3 (16 Feb 2016)";
		base.Load += new System.EventHandler(RAForm_Load);
		base.MouseMove += new System.Windows.Forms.MouseEventHandler(RAForm_MouseMove);
		((System.ComponentModel.ISupportInitialize)this.picRAGraph).EndInit();
		this.groupBox_Rx_select.ResumeLayout(false);
		this.groupBox_Rx_select.PerformLayout();
		this.groupBox_Rx2.ResumeLayout(false);
		this.groupBox_Rx2.PerformLayout();
		this.groupBoxTS8.ResumeLayout(false);
		this.groupBoxTS8.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.manual_xmin).EndInit();
		((System.ComponentModel.ISupportInitialize)this.manual_xmax).EndInit();
		this.groupBoxTS6.ResumeLayout(false);
		this.groupBoxTS7.ResumeLayout(false);
		this.groupBoxTS7.PerformLayout();
		this.groupBox_scaling.ResumeLayout(false);
		this.groupBox_scaling.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.manual_ymax).EndInit();
		((System.ComponentModel.ISupportInitialize)this.manual_ymin).EndInit();
		this.groupBoxTS5.ResumeLayout(false);
		this.groupBoxTS5.PerformLayout();
		this.groupBox_signal.ResumeLayout(false);
		this.groupBox_signal.PerformLayout();
		this.groupBoxTS4.ResumeLayout(false);
		this.groupBoxTS4.PerformLayout();
		this.groupBoxTS3.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.numericUpDown_mSec_between_measurements).EndInit();
		this.groupBoxTS2.ResumeLayout(false);
		this.groupBoxTS2.PerformLayout();
		this.groupBoxTS1.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.numericUpDown_measurements_per_point).EndInit();
		this.groupBox2.ResumeLayout(false);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
