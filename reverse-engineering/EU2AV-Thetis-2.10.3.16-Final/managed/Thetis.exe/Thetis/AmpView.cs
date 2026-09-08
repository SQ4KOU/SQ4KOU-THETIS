using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Thetis;

public class AmpView : Form
{
	private PSForm _psform;

	private const int max_ints = 16;

	private const int max_samps = 4096;

	private const int np = 512;

	private double[] x = new double[4096];

	private double[] ym = new double[4096];

	private double[] yc = new double[4096];

	private double[] ys = new double[4096];

	private double[] cm = new double[64];

	private double[] cc = new double[64];

	private double[] cs = new double[64];

	private double[] xm_cor = new double[4096];

	private double[] ym_cor = new double[4096];

	private double[] xa_cor = new double[4096];

	private double[] ya_cor = new double[4096];

	private int[] nsamps_out = new int[1];

	private int[] cpts_out = new int[1];

	private double[] phs_ref_deg_out = new double[1];

	private double[] t = new double[17];

	private int skip = 1;

	private bool showgain;

	private static object intslock = new object();

	private bool _init = true;

	private bool _is_closing;

	private int _oldIntsSpi = -1;

	private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

	private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

	private const uint SWP_NOMOVE = 2u;

	private const uint SWP_NOSIZE = 1u;

	private const uint SWP_NOACTIVATE = 16u;

	private const uint SWP_SHOWWINDOW = 64u;

	private IContainer components;

	private Chart chart1;

	private Timer timer1;

	private CheckBoxTS chkAVShowGain;

	private CheckBoxTS chkAVLowRes;

	private CheckBoxTS chkAVPhaseZoom;

	private CheckBoxTS chkStayOnTop;

	public AmpView(PSForm ps)
	{
		InitializeComponent();
		Common.DoubleBufferAll(this, enabled: true);
		_psform = ps;
	}

	private void AmpView_Load(object sender, EventArgs e)
	{
		Common.FadeIn(this);
		base.ClientSize = new Size(560, 445);
		Common.RestoreForm(this, "AmpView", restore_size: true);
		int num = _psform.Ints;
		if (num < 1)
		{
			num = 16;
		}
		double num2 = 1.0 / (double)num;
		t[0] = 0.0;
		for (int i = 1; i <= num; i++)
		{
			t[i] = t[i - 1] + num2;
		}
		EventArgs empty = EventArgs.Empty;
		chkAVShowGain_CheckedChanged(this, empty);
		chkAVLowRes_CheckedChanged(this, empty);
		chkAVPhaseZoom_CheckedChanged(this, empty);
		chkStayOnTop_CheckedChanged(this, empty);
	}

	private void disp_setup()
	{
		chart1.ChartAreas[0].AxisX.Minimum = 0.0;
		chart1.ChartAreas[0].AxisX.Maximum = 1.0;
		chart1.ChartAreas[0].AxisY.Minimum = 0.0;
		chart1.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.LightSalmon;
		chart1.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.LightSalmon;
		chart1.ChartAreas[0].AxisY2.LabelStyle.ForeColor = Color.LightSalmon;
		chart1.ChartAreas[0].AxisX.Title = "Input Magnitude";
		chart1.ChartAreas[0].AxisX.TitleForeColor = Color.LightSalmon;
		chart1.ChartAreas[0].AxisY.TitleForeColor = Color.LightSalmon;
		chart1.ChartAreas[0].AxisY2.Title = "Phase";
		chart1.ChartAreas[0].AxisY2.TitleForeColor = Color.LightSalmon;
	}

	private void init_data(int ints, int spi)
	{
		chart1.Series["Ref"].Points.Clear();
		chart1.Series["MagCorr"].Points.Clear();
		chart1.Series["PhsCorr"].Points.Clear();
		chart1.Series["MagAmp"].Points.Clear();
		chart1.Series["PhsAmp"].Points.Clear();
		if (!showgain)
		{
			chart1.Series["Ref"].Points.AddXY(0.0, 0.0);
			chart1.Series["Ref"].Points.AddXY(1.0, 1.0);
			chart1.Series["Ref"].Points.AddXY(1.0, 0.5);
			chart1.Series["Ref"].Points.AddXY(0.0, 0.5);
		}
		else
		{
			chart1.Series["Ref"].Points.AddXY(0.0, 1.0);
			chart1.Series["Ref"].Points.AddXY(1.0, 1.0);
		}
		chart1.Series["MagCorr"].Points.AddXY(0.0, 0.0);
		for (int i = 1; i <= 512; i++)
		{
			chart1.Series["MagCorr"].Points.AddXY(0.0, 0.0);
			chart1.Series["PhsCorr"].Points.AddXY(0.0, 0.0);
		}
		for (int j = 0; j < ints * spi; j++)
		{
			chart1.Series["MagAmp"].Points.AddXY(0.0, 0.0);
			chart1.Series["PhsAmp"].Points.AddXY(0.0, 0.0);
		}
	}

	private void disp_data_Update(int ints, int spi)
	{
		double num = 1.0 / 512.0;
		double num2 = num;
		double num3 = 1.0 / (double)ints;
		t[0] = 0.0;
		for (int i = 1; i <= ints; i++)
		{
			t[i] = t[i - 1] + num3;
		}
		if (!showgain)
		{
			chart1.Series["Ref"].Points[0].SetValueXY(0.0, 0.0);
			chart1.Series["Ref"].Points[1].SetValueXY(1.0, 1.0);
			chart1.Series["Ref"].Points[2].SetValueXY(1.0, 0.5);
			chart1.Series["Ref"].Points[3].SetValueXY(0.0, 0.5);
		}
		else
		{
			chart1.Series["Ref"].Points[0].SetValueXY(0.0, 1.0);
			chart1.Series["Ref"].Points[1].SetValueXY(1.0, 1.0);
		}
		chart1.Series["MagCorr"].Points[0].SetValueXY(0.0, 0.0);
		int num4 = ints - 1;
		double num5 = t[ints] - t[ints - 1];
		double num6 = cc[4 * num4] + num5 * (cc[4 * num4 + 1] + num5 * (cc[4 * num4 + 2] + num5 * cc[4 * num4 + 3]));
		double num7 = cs[4 * num4] + num5 * (cs[4 * num4 + 1] + num5 * (cs[4 * num4 + 2] + num5 * cs[4 * num4 + 3]));
		double num8 = 180.0 / Math.PI * Math.Atan2(num7, num6);
		int num9 = -1;
		for (int j = 1; j <= 512; j++)
		{
			if ((num4 = (int)(num2 * (double)ints)) > ints - 1)
			{
				num4 = ints - 1;
			}
			num5 = num2 - t[num4];
			double num10 = cm[4 * num4] + num5 * (cm[4 * num4 + 1] + num5 * (cm[4 * num4 + 2] + num5 * cm[4 * num4 + 3]));
			num6 = cc[4 * num4] + num5 * (cc[4 * num4 + 1] + num5 * (cc[4 * num4 + 2] + num5 * cc[4 * num4 + 3]));
			num7 = cs[4 * num4] + num5 * (cs[4 * num4 + 1] + num5 * (cs[4 * num4 + 2] + num5 * cs[4 * num4 + 3]));
			if (!showgain)
			{
				chart1.Series["MagCorr"].Points[j].SetValueXY(num2, num10 * num2);
			}
			else
			{
				chart1.Series["MagCorr"].Points[j].SetValueXY(num2, num10);
			}
			double num11 = 180.0 / Math.PI * Math.Atan2(num7, num6) - num8;
			if (num11 > -180.0 && num11 < 180.0)
			{
				chart1.Series["PhsCorr"].Points[j - 1].SetValueXY(num2, num11);
				num9 = j - 1;
			}
			else if (num9 != -1)
			{
				DataPoint dataPoint = chart1.Series["PhsCorr"].Points[num9];
				chart1.Series["PhsCorr"].Points[j - 1].SetValueXY(dataPoint.XValue, dataPoint.YValues[0]);
			}
			else
			{
				chart1.Series["PhsCorr"].Points[j - 1].SetValueXY(0.0, 0.0);
			}
			num2 += num;
		}
		num4 = ints * spi - 1;
		num8 = 180.0 / Math.PI * Math.Atan2(yc[num4], ys[num4]);
		num9 = -1;
		for (int k = 0; k < ints * spi; k++)
		{
			if (k % skip == 0)
			{
				if (!showgain)
				{
					chart1.Series["MagAmp"].Points[k].SetValueXY(ym[k] * x[k], x[k]);
				}
				else
				{
					chart1.Series["MagAmp"].Points[k].SetValueXY(ym[k] * x[k], 1.0 / ym[k]);
				}
				double num11 = 180.0 / Math.PI * Math.Atan2(yc[k], ys[k]) - num8;
				chart1.Series["PhsAmp"].Points[k].SetValueXY(x[k], num11);
				num9 = k;
			}
			else if (num9 != -1)
			{
				DataPoint dataPoint2 = chart1.Series["MagAmp"].Points[num9];
				chart1.Series["MagAmp"].Points[k].SetValueXY(dataPoint2.XValue, dataPoint2.YValues[0]);
				dataPoint2 = chart1.Series["PhsAmp"].Points[num9];
				chart1.Series["PhsAmp"].Points[k].SetValueXY(dataPoint2.XValue, dataPoint2.YValues[0]);
			}
			else
			{
				chart1.Series["MagAmp"].Points[k].SetValueXY(0.0, 0.0);
				chart1.Series["PhsAmp"].Points[k].SetValueXY(0.0, 0.0);
			}
		}
	}

	private void chkStayOnTop_CheckedChanged(object sender, EventArgs e)
	{
		FixOnTop();
	}

	public void CloseDown()
	{
		_is_closing = true;
		timer1.Stop();
		Close();
		Application.ExitThread();
	}

	private unsafe void timer1_Tick(object sender, EventArgs e)
	{
		timer1.Stop();
		if (_is_closing)
		{
			return;
		}
		disp_setup();
		fixed (double* value = x)
		{
			fixed (double* value2 = ym)
			{
				fixed (double* value3 = yc)
				{
					fixed (double* value4 = ys)
					{
						fixed (double* ptr = xm_cor)
						{
							fixed (double* ptr2 = ym_cor)
							{
								fixed (double* ptr3 = xa_cor)
								{
									fixed (double* value5 = ya_cor)
									{
										fixed (double* value6 = cm)
										{
											fixed (double* value7 = cc)
											{
												fixed (double* value8 = cs)
												{
													fixed (int* value9 = nsamps_out)
													{
														fixed (int* value10 = cpts_out)
														{
															fixed (double* value11 = phs_ref_deg_out)
															{
																puresignal.GetPSDisp(WDSP.id(1u, 0u), new IntPtr(value), new IntPtr(value2), new IntPtr(value3), new IntPtr(value4), new IntPtr(value6), new IntPtr(value7), new IntPtr(value8), new IntPtr(value5), new IntPtr(value9), new IntPtr(value10), new IntPtr(value11));
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}
		lock (intslock)
		{
			chart1.Series.SuspendUpdates();
			chart1.Series["Ref"].Points.SuspendUpdates();
			chart1.Series["MagCorr"].Points.SuspendUpdates();
			chart1.Series["PhsCorr"].Points.SuspendUpdates();
			chart1.Series["MagAmp"].Points.SuspendUpdates();
			chart1.Series["PhsAmp"].Points.SuspendUpdates();
			int num = _psform.Ints;
			int num2 = _psform.Spi;
			if (num < 1)
			{
				num = 16;
			}
			if (num2 < 1)
			{
				num2 = 256;
			}
			int num3 = num * num2;
			if (_oldIntsSpi != num3)
			{
				_oldIntsSpi = num3;
				_init = true;
			}
			if (_init)
			{
				init_data(num, num2);
				_init = false;
			}
			disp_data_Update(num, num2);
			chart1.Series["PhsAmp"].Points.ResumeUpdates();
			chart1.Series["MagAmp"].Points.ResumeUpdates();
			chart1.Series["PhsCorr"].Points.ResumeUpdates();
			chart1.Series["MagCorr"].Points.ResumeUpdates();
			chart1.Series["Ref"].Points.ResumeUpdates();
			chart1.Series.ResumeUpdates();
			chart1.Invalidate();
		}
		if (!_is_closing)
		{
			timer1.Start();
		}
	}

	private void chkAVShowGain_CheckedChanged(object sender, EventArgs e)
	{
		if (chkAVShowGain.Checked)
		{
			chart1.ChartAreas[0].AxisY.Title = "Gain";
			chart1.ChartAreas[0].AxisY.Maximum = 2.0;
			chart1.Series["MagCorr"].LegendText = "Gain Corr";
			chart1.Series["_magamp"].LegendText = "Gain Amp";
			showgain = true;
		}
		else
		{
			chart1.ChartAreas[0].AxisY.Title = "Magnitude";
			chart1.ChartAreas[0].AxisY.Maximum = 1.0;
			chart1.Series["MagCorr"].LegendText = "Mag Corr";
			chart1.Series["_magamp"].LegendText = "Mag Amp";
			showgain = false;
		}
		_init = true;
	}

	private void chkAVLowRes_CheckedChanged(object sender, EventArgs e)
	{
		if (chkAVLowRes.Checked)
		{
			skip = 4;
		}
		else
		{
			skip = 1;
		}
	}

	private void AmpView_FormClosing(object sender, FormClosingEventArgs e)
	{
		Common.SaveForm(this, "AmpView");
	}

	private void chkAVPhaseZoom_CheckedChanged(object sender, EventArgs e)
	{
		if (chkAVPhaseZoom.Checked)
		{
			chart1.ChartAreas[0].AxisY2.Minimum = -45.0;
			chart1.ChartAreas[0].AxisY2.Maximum = 45.0;
		}
		else
		{
			chart1.ChartAreas[0].AxisY2.Minimum = -180.0;
			chart1.ChartAreas[0].AxisY2.Maximum = 180.0;
		}
	}

	private void AmpView_FormClosed(object sender, FormClosedEventArgs e)
	{
		PSForm.ampv = null;
	}

	[DllImport("user32.dll")]
	private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

	public void FixOnTop()
	{
		if (!base.IsDisposed)
		{
			if (base.InvokeRequired)
			{
				BeginInvoke(new MethodInvoker(FixOnTop));
			}
			else if (base.IsHandleCreated)
			{
				bool flag = (base.TopMost = chkStayOnTop.Checked);
				IntPtr hWndInsertAfter = (flag ? HWND_TOPMOST : HWND_NOTOPMOST);
				SetWindowPos(base.Handle, hWndInsertAfter, 0, 0, 0, 0, 83u);
			}
		}
	}

	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);
		FixOnTop();
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
		this.components = new System.ComponentModel.Container();
		System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
		System.Windows.Forms.DataVisualization.Charting.Legend legend = new System.Windows.Forms.DataVisualization.Charting.Legend();
		System.Windows.Forms.DataVisualization.Charting.Series series = new System.Windows.Forms.DataVisualization.Charting.Series();
		System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
		System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
		System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
		System.Windows.Forms.DataVisualization.Charting.Series series5 = new System.Windows.Forms.DataVisualization.Charting.Series();
		System.Windows.Forms.DataVisualization.Charting.Series series6 = new System.Windows.Forms.DataVisualization.Charting.Series();
		System.Windows.Forms.DataVisualization.Charting.Series series7 = new System.Windows.Forms.DataVisualization.Charting.Series();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Thetis.AmpView));
		this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
		this.timer1 = new System.Windows.Forms.Timer(this.components);
		this.chkStayOnTop = new System.Windows.Forms.CheckBoxTS();
		this.chkAVPhaseZoom = new System.Windows.Forms.CheckBoxTS();
		this.chkAVLowRes = new System.Windows.Forms.CheckBoxTS();
		this.chkAVShowGain = new System.Windows.Forms.CheckBoxTS();
		((System.ComponentModel.ISupportInitialize)this.chart1).BeginInit();
		base.SuspendLayout();
		this.chart1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.chart1.BackColor = System.Drawing.Color.Black;
		chartArea.AxisX.InterlacedColor = System.Drawing.Color.White;
		chartArea.AxisX.LineColor = System.Drawing.Color.DimGray;
		chartArea.AxisX.MajorGrid.LineColor = System.Drawing.Color.DimGray;
		chartArea.AxisY.LineColor = System.Drawing.Color.DimGray;
		chartArea.AxisY.MajorGrid.LineColor = System.Drawing.Color.DimGray;
		chartArea.AxisY2.LineColor = System.Drawing.Color.DimGray;
		chartArea.AxisY2.MajorGrid.LineColor = System.Drawing.Color.DimGray;
		chartArea.BackColor = System.Drawing.Color.Black;
		chartArea.BackSecondaryColor = System.Drawing.Color.White;
		chartArea.Name = "ChartArea1";
		this.chart1.ChartAreas.Add(chartArea);
		legend.BackColor = System.Drawing.Color.Transparent;
		legend.ForeColor = System.Drawing.Color.LightSalmon;
		legend.Name = "Legend1";
		legend.Position.Auto = false;
		legend.Position.Height = 15f;
		legend.Position.Width = 16.99463f;
		legend.Position.X = 70.5f;
		legend.Position.Y = 70.5f;
		this.chart1.Legends.Add(legend);
		this.chart1.Location = new System.Drawing.Point(0, 2);
		this.chart1.Name = "chart1";
		series.BackSecondaryColor = System.Drawing.Color.White;
		series.ChartArea = "ChartArea1";
		series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
		series.Color = System.Drawing.Color.DimGray;
		series.IsVisibleInLegend = false;
		series.Legend = "Legend1";
		series.Name = "Ref";
		series2.ChartArea = "ChartArea1";
		series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
		series2.Color = System.Drawing.Color.DodgerBlue;
		series2.LabelForeColor = System.Drawing.Color.LightSalmon;
		series2.Legend = "Legend1";
		series2.LegendText = "Mag Amp";
		series2.Name = "_magamp";
		series3.ChartArea = "ChartArea1";
		series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastPoint;
		series3.Color = System.Drawing.Color.DodgerBlue;
		series3.IsVisibleInLegend = false;
		series3.Legend = "Legend1";
		series3.LegendText = "Mag Amp";
		series3.MarkerSize = 2;
		series3.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
		series3.Name = "MagAmp";
		series4.ChartArea = "ChartArea1";
		series4.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
		series4.Color = System.Drawing.Color.Gold;
		series4.Legend = "Legend1";
		series4.LegendText = "Phs Amp";
		series4.Name = "_phsamp";
		series5.ChartArea = "ChartArea1";
		series5.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastPoint;
		series5.Color = System.Drawing.Color.Gold;
		series5.IsVisibleInLegend = false;
		series5.Legend = "Legend1";
		series5.LegendText = "Phs Amp";
		series5.MarkerSize = 2;
		series5.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
		series5.Name = "PhsAmp";
		series5.YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
		series6.ChartArea = "ChartArea1";
		series6.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
		series6.Color = System.Drawing.Color.Crimson;
		series6.Legend = "Legend1";
		series6.LegendText = "Mag Corr";
		series6.Name = "MagCorr";
		series7.ChartArea = "ChartArea1";
		series7.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
		series7.Color = System.Drawing.Color.Lime;
		series7.Legend = "Legend1";
		series7.LegendText = "Phs Corr";
		series7.Name = "PhsCorr";
		series7.YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
		this.chart1.Series.Add(series);
		this.chart1.Series.Add(series2);
		this.chart1.Series.Add(series3);
		this.chart1.Series.Add(series4);
		this.chart1.Series.Add(series5);
		this.chart1.Series.Add(series6);
		this.chart1.Series.Add(series7);
		this.chart1.Size = new System.Drawing.Size(564, 377);
		this.chart1.TabIndex = 0;
		this.chart1.Text = "chart1";
		this.timer1.Enabled = true;
		this.timer1.Tick += new System.EventHandler(timer1_Tick);
		this.chkStayOnTop.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.chkStayOnTop.AutoSize = true;
		this.chkStayOnTop.BackColor = System.Drawing.Color.Black;
		this.chkStayOnTop.ForeColor = System.Drawing.Color.LightSalmon;
		this.chkStayOnTop.Image = null;
		this.chkStayOnTop.Location = new System.Drawing.Point(490, 378);
		this.chkStayOnTop.Name = "chkStayOnTop";
		this.chkStayOnTop.Size = new System.Drawing.Size(62, 17);
		this.chkStayOnTop.TabIndex = 4;
		this.chkStayOnTop.Text = "On Top";
		this.chkStayOnTop.UseVisualStyleBackColor = false;
		this.chkStayOnTop.CheckedChanged += new System.EventHandler(chkStayOnTop_CheckedChanged);
		this.chkAVPhaseZoom.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
		this.chkAVPhaseZoom.AutoSize = true;
		this.chkAVPhaseZoom.BackColor = System.Drawing.Color.Black;
		this.chkAVPhaseZoom.ForeColor = System.Drawing.Color.LightSalmon;
		this.chkAVPhaseZoom.Image = null;
		this.chkAVPhaseZoom.Location = new System.Drawing.Point(242, 378);
		this.chkAVPhaseZoom.Name = "chkAVPhaseZoom";
		this.chkAVPhaseZoom.Size = new System.Drawing.Size(86, 17);
		this.chkAVPhaseZoom.TabIndex = 3;
		this.chkAVPhaseZoom.Text = "Phase Zoom";
		this.chkAVPhaseZoom.UseVisualStyleBackColor = false;
		this.chkAVPhaseZoom.CheckedChanged += new System.EventHandler(chkAVPhaseZoom_CheckedChanged);
		this.chkAVLowRes.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.chkAVLowRes.AutoSize = true;
		this.chkAVLowRes.BackColor = System.Drawing.Color.Black;
		this.chkAVLowRes.Checked = true;
		this.chkAVLowRes.CheckState = System.Windows.Forms.CheckState.Checked;
		this.chkAVLowRes.ForeColor = System.Drawing.Color.LightSalmon;
		this.chkAVLowRes.Image = null;
		this.chkAVLowRes.Location = new System.Drawing.Point(404, 378);
		this.chkAVLowRes.Name = "chkAVLowRes";
		this.chkAVLowRes.Size = new System.Drawing.Size(68, 17);
		this.chkAVLowRes.TabIndex = 2;
		this.chkAVLowRes.Text = "Low Res";
		this.chkAVLowRes.UseVisualStyleBackColor = false;
		this.chkAVLowRes.CheckedChanged += new System.EventHandler(chkAVLowRes_CheckedChanged);
		this.chkAVShowGain.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.chkAVShowGain.AutoSize = true;
		this.chkAVShowGain.BackColor = System.Drawing.Color.Black;
		this.chkAVShowGain.ForeColor = System.Drawing.Color.LightSalmon;
		this.chkAVShowGain.Image = null;
		this.chkAVShowGain.Location = new System.Drawing.Point(7, 378);
		this.chkAVShowGain.Name = "chkAVShowGain";
		this.chkAVShowGain.Size = new System.Drawing.Size(78, 17);
		this.chkAVShowGain.TabIndex = 1;
		this.chkAVShowGain.Text = "Show Gain";
		this.chkAVShowGain.UseVisualStyleBackColor = false;
		this.chkAVShowGain.CheckedChanged += new System.EventHandler(chkAVShowGain_CheckedChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.Black;
		base.ClientSize = new System.Drawing.Size(564, 401);
		base.Controls.Add(this.chkStayOnTop);
		base.Controls.Add(this.chkAVPhaseZoom);
		base.Controls.Add(this.chkAVLowRes);
		base.Controls.Add(this.chkAVShowGain);
		base.Controls.Add(this.chart1);
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		this.MinimumSize = new System.Drawing.Size(440, 380);
		base.Name = "AmpView";
		this.Text = "AmpView 1.0";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(AmpView_FormClosing);
		base.FormClosed += new System.Windows.Forms.FormClosedEventHandler(AmpView_FormClosed);
		base.Load += new System.EventHandler(AmpView_Load);
		((System.ComponentModel.ISupportInitialize)this.chart1).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
