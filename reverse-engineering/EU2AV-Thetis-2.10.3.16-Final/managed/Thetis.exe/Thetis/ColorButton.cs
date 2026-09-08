using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis;

[DefaultEvent("Changed")]
public class ColorButton : ButtonTS
{
	protected class ColorPanel : Form
	{
		private ColorButton colorButton;

		private int colorIndex = -1;

		private int keyboardIndex = -50;

		private Color[] colorList = new Color[40]
		{
			Color.FromArgb(0, 0, 0),
			Color.FromArgb(153, 51, 0),
			Color.FromArgb(51, 51, 0),
			Color.FromArgb(0, 51, 0),
			Color.FromArgb(0, 51, 102),
			Color.FromArgb(0, 0, 128),
			Color.FromArgb(51, 51, 153),
			Color.FromArgb(51, 51, 51),
			Color.FromArgb(128, 0, 0),
			Color.FromArgb(255, 102, 0),
			Color.FromArgb(128, 128, 0),
			Color.FromArgb(0, 128, 0),
			Color.FromArgb(0, 128, 128),
			Color.FromArgb(0, 0, 255),
			Color.FromArgb(102, 102, 153),
			Color.FromArgb(128, 128, 128),
			Color.FromArgb(255, 0, 0),
			Color.FromArgb(255, 153, 0),
			Color.FromArgb(153, 204, 0),
			Color.FromArgb(51, 153, 102),
			Color.FromArgb(51, 204, 204),
			Color.FromArgb(51, 102, 255),
			Color.FromArgb(128, 0, 128),
			Color.FromArgb(153, 153, 153),
			Color.FromArgb(255, 0, 255),
			Color.FromArgb(255, 204, 0),
			Color.FromArgb(255, 255, 0),
			Color.FromArgb(0, 255, 0),
			Color.FromArgb(0, 255, 255),
			Color.FromArgb(0, 204, 255),
			Color.FromArgb(153, 51, 102),
			Color.FromArgb(192, 192, 192),
			Color.FromArgb(255, 153, 204),
			Color.FromArgb(255, 204, 153),
			Color.FromArgb(255, 255, 153),
			Color.FromArgb(204, 255, 204),
			Color.FromArgb(204, 255, 255),
			Color.FromArgb(153, 204, 255),
			Color.FromArgb(204, 153, 255),
			Color.FromArgb(255, 255, 255)
		};

		public ColorPanel(Point pt, ColorButton button)
		{
			colorButton = button;
			base.FormBorderStyle = FormBorderStyle.FixedDialog;
			base.MinimizeBox = false;
			base.MaximizeBox = false;
			base.ControlBox = false;
			base.ShowInTaskbar = false;
			base.TopMost = true;
			SetStyle(ControlStyles.DoubleBuffer, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			base.Width = 156;
			base.Height = 100;
			if (colorButton.autoButton != "")
			{
				base.Height += 23;
			}
			if (colorButton.moreButton != "")
			{
				base.Height += 23;
			}
			CenterToScreen();
			base.Location = pt;
			base.Capture = true;
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			colorButton.panelVisible = false;
			colorButton.Refresh();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			Pen pen = new Pen(SystemColors.ControlDark);
			Pen pen2 = new Pen(SystemColors.ControlLightLight);
			SolidBrush brush = new SolidBrush(SystemColors.ControlLightLight);
			bool flag = false;
			int num = 6;
			int num2 = 5;
			if (colorButton.autoButton != "")
			{
				flag = colorButton.Color == Color.Transparent;
				DrawButton(e, num, num2, colorButton.autoButton, 100, flag);
				num2 += 23;
			}
			for (int i = 0; i < 40; i++)
			{
				if (colorButton.Color.ToArgb() == colorList[i].ToArgb())
				{
					flag = true;
				}
				if (colorIndex == i)
				{
					e.Graphics.DrawRectangle(pen2, num - 3, num2 - 3, 17, 17);
					e.Graphics.DrawLine(pen, num - 2, num2 + 14, num + 14, num2 + 14);
					e.Graphics.DrawLine(pen, num + 14, num2 - 2, num + 14, num2 + 14);
				}
				else if (colorButton.Color.ToArgb() == colorList[i].ToArgb())
				{
					if (keyboardIndex == -50)
					{
						keyboardIndex = i;
					}
					e.Graphics.FillRectangle(brush, num - 3, num2 - 3, 18, 18);
					e.Graphics.DrawLine(pen, num - 3, num2 - 3, num + 13, num2 - 3);
					e.Graphics.DrawLine(pen, num - 3, num2 - 3, num - 3, num2 + 13);
				}
				e.Graphics.FillRectangle(new SolidBrush(colorList[i]), num, num2, 11, 11);
				e.Graphics.DrawRectangle(pen, num, num2, 11, 11);
				if ((i + 1) % 8 == 0)
				{
					num = 6;
					num2 += 18;
				}
				else
				{
					num += 18;
				}
			}
			if (colorButton.moreButton != "")
			{
				DrawButton(e, num, num2, colorButton.moreButton, 101, !flag);
			}
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				Close();
			}
			else if (e.KeyCode == Keys.Left)
			{
				MoveIndex(-1);
			}
			else if (e.KeyCode == Keys.Up)
			{
				MoveIndex(-8);
			}
			else if (e.KeyCode == Keys.Down)
			{
				MoveIndex(8);
			}
			else if (e.KeyCode == Keys.Right)
			{
				MoveIndex(1);
			}
			else if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Space)
			{
				OnClick(EventArgs.Empty);
			}
			else
			{
				base.OnKeyDown(e);
			}
		}

		private void MoveIndex(int delta)
		{
			int num = ((colorButton.autoButton != "") ? (-8) : 0);
			int num2 = 39 + ((colorButton.moreButton != "") ? 8 : 0);
			int num3 = num2 - num + 1;
			if (delta == -1 && keyboardIndex < 0)
			{
				keyboardIndex = num2;
			}
			else if (delta == 1 && keyboardIndex > 39)
			{
				keyboardIndex = num;
			}
			else if (delta == 1 && keyboardIndex < 0)
			{
				keyboardIndex = 0;
			}
			else if (delta == -1 && keyboardIndex > 39)
			{
				keyboardIndex = 39;
			}
			else
			{
				keyboardIndex += delta;
			}
			if (keyboardIndex < num)
			{
				keyboardIndex += num3;
			}
			if (keyboardIndex > num2)
			{
				keyboardIndex -= num3;
			}
			if (keyboardIndex < 0)
			{
				colorIndex = 100;
			}
			else if (keyboardIndex > 39)
			{
				colorIndex = 101;
			}
			else
			{
				colorIndex = keyboardIndex;
			}
			Refresh();
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (RectangleToScreen(base.ClientRectangle).Contains(Cursor.Position))
			{
				base.OnMouseDown(e);
			}
			else
			{
				Close();
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (RectangleToScreen(base.ClientRectangle).Contains(Cursor.Position))
			{
				Point pt = PointToClient(Cursor.Position);
				int num = 6;
				int num2 = 5;
				if (colorButton.autoButton != "")
				{
					if (SetColorIndex(new Rectangle(num - 3, num2 - 3, 143, 22), pt, 100))
					{
						return;
					}
					num2 += 23;
				}
				for (int i = 0; i < 40; i++)
				{
					if (SetColorIndex(new Rectangle(num - 3, num2 - 3, 17, 17), pt, i))
					{
						return;
					}
					if ((i + 1) % 8 == 0)
					{
						num = 6;
						num2 += 18;
					}
					else
					{
						num += 18;
					}
				}
				if (colorButton.moreButton != "" && SetColorIndex(new Rectangle(num - 3, num2 - 3, 143, 22), pt, 101))
				{
					return;
				}
			}
			if (colorIndex != -1)
			{
				colorIndex = -1;
				Invalidate();
			}
		}

		protected override void OnClick(EventArgs e)
		{
			if (colorIndex < 0)
			{
				return;
			}
			if (colorIndex < 40)
			{
				colorButton.Color = colorList[colorIndex];
			}
			else if (colorIndex == 100)
			{
				colorButton.Color = Color.Transparent;
			}
			else
			{
				ColorDialog colorDialog = new ColorDialog();
				colorDialog.Color = colorButton.Color;
				colorDialog.FullOpen = true;
				if (colorDialog.ShowDialog(this) != DialogResult.OK)
				{
					Close();
					return;
				}
				colorButton.Color = colorDialog.Color;
			}
			Close();
			colorButton.OnChanged(EventArgs.Empty);
		}

		protected void DrawButton(PaintEventArgs e, int x, int y, string text, int index, bool selected)
		{
			Pen pen = new Pen(SystemColors.ControlDark);
			Pen pen2 = new Pen(SystemColors.ControlLightLight);
			SolidBrush brush = new SolidBrush(SystemColors.ControlLightLight);
			if (colorIndex == index)
			{
				e.Graphics.DrawRectangle(pen2, x - 3, y - 3, 143, 22);
				e.Graphics.DrawLine(pen, x - 2, y + 19, x + 140, y + 19);
				e.Graphics.DrawLine(pen, x + 140, y - 2, x + 140, y + 19);
			}
			else if (selected)
			{
				e.Graphics.FillRectangle(brush, x - 3, y - 3, 144, 23);
				e.Graphics.DrawLine(pen, x - 3, y - 3, x + 139, y - 3);
				e.Graphics.DrawLine(pen, x - 3, y - 3, x - 3, y + 18);
			}
			Rectangle rectangle = new Rectangle(x, y, 137, 16);
			SolidBrush brush2 = new SolidBrush(SystemColors.ControlText);
			StringFormat stringFormat = new StringFormat();
			stringFormat.Alignment = StringAlignment.Center;
			stringFormat.LineAlignment = StringAlignment.Center;
			e.Graphics.DrawRectangle(pen, rectangle);
			e.Graphics.DrawString(text, colorButton.Font, brush2, rectangle, stringFormat);
		}

		protected bool SetColorIndex(Rectangle rc, Point pt, int index)
		{
			if (rc.Contains(pt))
			{
				if (colorIndex != index)
				{
					colorIndex = index;
					Invalidate();
				}
				return true;
			}
			return false;
		}
	}

	private Container components;

	private Color buttonColor = Color.Transparent;

	private string autoButton = "Automatic";

	private string moreButton = "More Colors...";

	private bool buttonPushed;

	private bool panelVisible;

	public Color Color
	{
		get
		{
			return buttonColor;
		}
		set
		{
			buttonColor = value;
			Refresh();
			OnChanged(EventArgs.Empty);
		}
	}

	public string Automatic
	{
		get
		{
			return autoButton;
		}
		set
		{
			autoButton = value;
		}
	}

	public string MoreColors
	{
		get
		{
			return moreButton;
		}
		set
		{
			moreButton = value;
		}
	}

	public event EventHandler Changed;

	protected virtual void OnChanged(EventArgs e)
	{
		if (Changed != null)
		{
			Changed(this, e);
		}
	}

	public ColorButton()
	{
		InitializeComponent();
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
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		int num = 0;
		if (panelVisible || (buttonPushed && RectangleToScreen(base.ClientRectangle).Contains(Cursor.Position)))
		{
			ControlPaint.DrawButton(e.Graphics, e.ClipRectangle, ButtonState.Pushed);
			num = 1;
		}
		Rectangle rect = new Rectangle(5 + num, 5 + num, base.Width - 24, base.Height - 11);
		Pen pen = new Pen(SystemColors.ControlDark);
		if (base.Enabled)
		{
			e.Graphics.FillRectangle(new SolidBrush(buttonColor), rect);
			e.Graphics.DrawRectangle(pen, rect);
		}
		e.Graphics.DrawLine(pen, rect.Right + 4, rect.Top, rect.Right + 4, rect.Bottom);
		e.Graphics.DrawLine(new Pen(SystemColors.ControlLightLight), rect.Right + 5, rect.Top, rect.Right + 5, rect.Bottom);
		Pen pen2 = new Pen(base.Enabled ? SystemColors.ControlText : SystemColors.GrayText);
		Point point = new Point(rect.Right, rect.Top + rect.Height / 2);
		e.Graphics.DrawLine(pen2, point.X + 9, point.Y - 1, point.X + 13, point.Y - 1);
		e.Graphics.DrawLine(pen2, point.X + 10, point.Y, point.X + 12, point.Y);
		e.Graphics.DrawLine(pen2, point.X + 11, point.Y, point.X + 11, point.Y + 1);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		buttonPushed = true;
		base.OnMouseDown(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		buttonPushed = false;
		base.OnMouseUp(e);
	}

	protected override void OnClick(EventArgs e)
	{
		panelVisible = true;
		Refresh();
		new ColorPanel(base.Parent.PointToScreen(new Point(base.Left, base.Bottom)), this).Show();
	}
}
