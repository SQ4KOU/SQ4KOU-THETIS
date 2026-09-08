using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Thetis;

[DefaultEvent("Scroll")]
public class PrettyTrackBar : PictureBox
{
	public class LimitConstraint : EventArgs
	{
		public int LimitValue;

		public bool MouseWheel;
	}

	public delegate void ScrollHandler(object sender, EventArgs e);

	private Rectangle _head_rect;

	private bool sliding;

	private bool _limitSliding;

	private int down_x;

	private int down_y;

	private Rectangle _limitBar_rect;

	private Image _head_image;

	private int min;

	private int max = 100;

	private int val;

	private Color _limitBarColor = Color.Red;

	private int _nLimitValue;

	private bool _bLimitEnabled;

	private int small_change = 1;

	private int large_change = 5;

	private Orientation orientation;

	private bool m_bGreenThumb;

	public Image HeadImage
	{
		get
		{
			return _head_image;
		}
		set
		{
			_head_image = value;
			if (_head_image != null)
			{
				_head_rect.Width = _head_image.Width;
				_head_rect.Height = _head_image.Height;
			}
			else
			{
				_head_rect.Width = 1;
				_head_rect.Height = 1;
			}
			UpdateHeadRectPos();
			UpdateLimitBar();
			Invalidate();
		}
	}

	public int Minimum
	{
		get
		{
			return min;
		}
		set
		{
			min = value;
			if (val < min)
			{
				val = min;
			}
			if (_nLimitValue < min)
			{
				_nLimitValue = min;
			}
			UpdateHeadRectPos();
			UpdateLimitBar();
			Invalidate();
		}
	}

	public int Maximum
	{
		get
		{
			return max;
		}
		set
		{
			max = value;
			if (val > max)
			{
				val = max;
			}
			if (_nLimitValue > max)
			{
				_nLimitValue = max;
			}
			UpdateHeadRectPos();
			UpdateLimitBar();
			Invalidate();
		}
	}

	public int Value
	{
		get
		{
			return val;
		}
		set
		{
			val = value;
			if (val < min)
			{
				val = min;
			}
			if (val > max)
			{
				val = max;
			}
			UpdateHeadRectPos();
			Invalidate();
		}
	}

	public Color LimitBarColor
	{
		get
		{
			return _limitBarColor;
		}
		set
		{
			_limitBarColor = value;
			Invalidate();
		}
	}

	public bool IsConstrained
	{
		get
		{
			if (_bLimitEnabled)
			{
				return val > _nLimitValue;
			}
			return false;
		}
	}

	public int ConstrainedValue
	{
		get
		{
			if (!_bLimitEnabled || val <= _nLimitValue)
			{
				return val;
			}
			return _nLimitValue;
		}
	}

	public int LimitValue
	{
		get
		{
			return _nLimitValue;
		}
		set
		{
			_nLimitValue = value;
			if (_nLimitValue < min)
			{
				_nLimitValue = min;
			}
			if (_nLimitValue > max)
			{
				_nLimitValue = max;
			}
			UpdateLimitBar();
			Invalidate();
		}
	}

	public bool LimitEnabled
	{
		get
		{
			return _bLimitEnabled;
		}
		set
		{
			_bLimitEnabled = value;
			UpdateLimitBar();
			Invalidate();
		}
	}

	public int SmallChange
	{
		get
		{
			return small_change;
		}
		set
		{
			small_change = value;
		}
	}

	public int LargeChange
	{
		get
		{
			return large_change;
		}
		set
		{
			large_change = value;
		}
	}

	public Orientation Orientation
	{
		get
		{
			return orientation;
		}
		set
		{
			orientation = value;
			Invalidate();
		}
	}

	public bool GreenThumb
	{
		get
		{
			return m_bGreenThumb;
		}
		set
		{
			m_bGreenThumb = value;
			Invalidate();
		}
	}

	public event ScrollHandler Scroll;

	public PrettyTrackBar()
	{
		SetStyle(ControlStyles.Selectable, value: true);
		base.TabStop = true;
		_head_rect = new Rectangle(0, 0, 1, 1);
	}

	private void UpdateHeadRectPos()
	{
		int num = 0;
		int num2 = 0;
		switch (orientation)
		{
		case Orientation.Horizontal:
			if (_head_image == null)
			{
				int num4 = base.Width;
				num = (int)((double)(val - min) / (double)(max - min) * (double)num4);
				num2 = base.Height / 2;
			}
			else
			{
				int num4 = base.Width - _head_rect.Width - base.Padding.Horizontal;
				num = (int)Math.Round((double)(val - min) / (double)(max - min) * (double)num4) + base.Padding.Left;
				num2 = (base.Height - base.Padding.Vertical) / 2 - _head_rect.Height / 2 + base.Padding.Top;
			}
			break;
		case Orientation.Vertical:
			if (_head_image == null)
			{
				int num3 = base.Height;
				num = base.Width / 2;
				num2 = (int)((double)(val - min) / (double)(max - min) * (double)num3);
			}
			else
			{
				int num3 = base.Height - _head_rect.Height - base.Padding.Vertical;
				num = (base.Width - base.Padding.Horizontal) / 2 - _head_rect.Width / 2 + base.Padding.Top;
				num2 = (int)((double)num3 - (double)(val - min) / (double)(max - min) * (double)num3) + base.Padding.Top;
			}
			break;
		}
		_head_rect.X = num;
		_head_rect.Y = num2;
	}

	private void UpdateLimitBar()
	{
		switch (orientation)
		{
		case Orientation.Horizontal:
		{
			int num6 = _head_rect.Width;
			int num7 = base.Width - num6 - base.Padding.Horizontal;
			int num3 = (int)Math.Round((double)(_nLimitValue - min) / (double)(max - min) * (double)num7);
			int num4 = num7;
			int num5 = (base.Width - num7) / 2;
			_limitBar_rect.X = num3 + num5;
			_limitBar_rect.Width = num4 - num3;
			_limitBar_rect.Y = base.Height / 2 - 1;
			_limitBar_rect.Height = ((base.Height % 2 == 0) ? 2 : 3);
			break;
		}
		case Orientation.Vertical:
		{
			int num = _head_rect.Height;
			int num2 = base.Height - num - base.Padding.Vertical;
			int num3 = (int)((double)(_nLimitValue - min) / (double)(max - min) * (double)num2);
			int num4 = num2;
			int num5 = (base.Height - num2) / 2;
			_limitBar_rect.X = base.Width / 2 - 1;
			_limitBar_rect.Width = ((base.Width % 2 == 0) ? 2 : 3);
			_limitBar_rect.Y = num3 + num5;
			_limitBar_rect.Height = num4 - num3;
			break;
		}
		}
	}

	public int ConstrainAValue(int value)
	{
		if (!_bLimitEnabled || value <= _nLimitValue)
		{
			return value;
		}
		return _nLimitValue;
	}

	protected override void OnEnabledChanged(EventArgs e)
	{
		Invalidate();
		base.OnEnabledChanged(e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		if ((e.Button != MouseButtons.Left && e.Button != MouseButtons.Right) || _head_rect.IsEmpty)
		{
			return;
		}
		if (base.Enabled)
		{
			if (e.Button == MouseButtons.Left)
			{
				if (_head_rect.Contains(e.X, e.Y))
				{
					down_x = e.X;
					down_y = e.Y;
					sliding = true;
				}
				else
				{
					int num = val;
					switch (orientation)
					{
					case Orientation.Horizontal:
						if (e.Y >= _head_rect.Y && e.Y <= _head_rect.Y + _head_rect.Height)
						{
							int num2 = ((e.X >= _head_rect.X) ? (num + large_change) : (num - large_change));
							if (num2 < min)
							{
								num2 = min;
							}
							if (num2 > max)
							{
								num2 = max;
							}
							Value = num2;
							OnScroll(this, e);
						}
						break;
					case Orientation.Vertical:
						if (e.X >= _head_rect.X && e.X <= _head_rect.X + _head_rect.Width)
						{
							int num2 = ((e.Y <= _head_rect.Y) ? (num + large_change) : (num - large_change));
							if (num2 < min)
							{
								num2 = min;
							}
							if (num2 > max)
							{
								num2 = max;
							}
							Value = num2;
							OnScroll(this, e);
						}
						break;
					}
				}
			}
			else if (e.Button == MouseButtons.Right && _bLimitEnabled)
			{
				down_x = e.X;
				down_y = e.Y;
				_limitSliding = true;
				int nLimitValue = _nLimitValue;
				int num3 = nLimitValue;
				switch (orientation)
				{
				case Orientation.Horizontal:
				{
					int num7 = _head_rect.Width;
					int num8 = base.Width - num7 - base.Padding.Horizontal;
					if (down_x < base.Padding.Left + num7 / 2)
					{
						down_x = base.Padding.Left + num7 / 2;
					}
					if (down_x > base.Width - num7 / 2 - base.Padding.Right)
					{
						down_x = base.Width - num7 / 2 - base.Padding.Right;
					}
					double num6 = (double)(down_x - base.Padding.Left - num7 / 2) / (double)num8;
					num3 = min + (int)Math.Round(num6 * (double)(max - min));
					if (num3 < min)
					{
						num3 = min;
					}
					if (num3 > max)
					{
						num3 = max;
					}
					break;
				}
				case Orientation.Vertical:
				{
					int num4 = _head_rect.Height;
					int num5 = base.Height - num4 - base.Padding.Vertical;
					if (down_y < base.Padding.Top + num4 / 2)
					{
						down_y = base.Padding.Top + num4 / 2;
					}
					if (down_y > base.Height - num4 / 2 - base.Padding.Bottom)
					{
						down_y = base.Height - num4 / 2 - base.Padding.Bottom;
					}
					double num6 = 1.0 - (double)(down_y - base.Padding.Top - num4 / 2) / (double)num5;
					num3 = min + (int)(num6 * (double)(max - min));
					if (num3 < min)
					{
						num3 = min;
					}
					if (num3 > max)
					{
						num3 = max;
					}
					break;
				}
				}
				if (num3 != nLimitValue)
				{
					_nLimitValue = num3;
					UpdateLimitBar();
				}
				OnScroll(this, new LimitConstraint
				{
					LimitValue = _nLimitValue,
					MouseWheel = false
				});
				Invalidate();
			}
		}
		base.OnMouseDown(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (_head_rect.IsEmpty || !base.Enabled)
		{
			return;
		}
		if (sliding)
		{
			int num = val;
			int num2 = num;
			switch (orientation)
			{
			case Orientation.Horizontal:
			{
				int num3 = e.X - down_x;
				int num6 = base.Width - _head_rect.Width - base.Padding.Horizontal;
				if ((_head_rect.X <= base.Padding.Left && num3 < 0) || (_head_rect.X >= num6 + base.Padding.Left && num3 > 0))
				{
					return;
				}
				double num5 = (double)(_head_rect.X - base.Padding.Left + num3) / (double)num6;
				num2 = min + (int)Math.Round(num5 * (double)(max - min));
				if (num2 < min)
				{
					num2 = min;
				}
				if (num2 > max)
				{
					num2 = max;
				}
				down_x = e.X;
				if (down_x < base.Padding.Left)
				{
					down_x = base.Padding.Left;
				}
				if (down_x > num6 + base.Padding.Left + _head_rect.Width)
				{
					down_x = num6 + base.Padding.Left + _head_rect.Width;
				}
				_head_rect.X += num3;
				if (_head_rect.X < base.Padding.Left)
				{
					_head_rect.X = base.Padding.Left;
				}
				if (_head_rect.X > num6 + base.Padding.Left)
				{
					_head_rect.X = num6 + base.Padding.Left;
				}
				break;
			}
			case Orientation.Vertical:
			{
				int num3 = e.Y - down_y;
				int num4 = base.Height - _head_rect.Height - base.Padding.Vertical;
				if ((_head_rect.Y <= base.Padding.Top && num3 < 0) || (_head_rect.Y >= num4 + base.Padding.Top && num3 > 0))
				{
					return;
				}
				double num5 = 1.0 - (double)(_head_rect.Y - base.Padding.Top + num3) / (double)num4;
				num2 = min + (int)(num5 * (double)(max - min));
				if (num2 < min)
				{
					num2 = min;
				}
				if (num2 > max)
				{
					num2 = max;
				}
				down_y = e.Y;
				if (down_y < base.Padding.Top)
				{
					down_y = base.Padding.Top;
				}
				if (down_y > num4 + base.Padding.Top + _head_rect.Height)
				{
					down_y = num4 + base.Padding.Top + _head_rect.Height;
				}
				_head_rect.Y += num3;
				if (_head_rect.Y < base.Padding.Top)
				{
					_head_rect.Y = base.Padding.Top;
				}
				if (_head_rect.Y > num4 + base.Padding.Top)
				{
					_head_rect.Y = num4 + base.Padding.Top;
				}
				break;
			}
			}
			if (num2 != num)
			{
				val = num2;
				OnScroll(this, e);
			}
			Invalidate();
		}
		else
		{
			if (!_limitSliding)
			{
				return;
			}
			int num = _nLimitValue;
			int num2 = num;
			switch (orientation)
			{
			case Orientation.Horizontal:
			{
				int num8 = _head_rect.Width;
				int num3 = e.X - down_x;
				int num6 = base.Width - num8 - base.Padding.Horizontal;
				double num5 = (double)(_limitBar_rect.X - base.Padding.Left - num8 / 2 + num3) / (double)num6;
				num2 = min + (int)Math.Round(num5 * (double)(max - min));
				if (num2 < min)
				{
					num2 = min;
				}
				if (num2 > max)
				{
					num2 = max;
				}
				down_x = e.X;
				if (down_x < base.Padding.Left + num8 / 2)
				{
					down_x = base.Padding.Left + num8 / 2;
				}
				if (down_x > base.Width - num8 / 2 - base.Padding.Right)
				{
					down_x = base.Width - num8 / 2 - base.Padding.Right;
				}
				break;
			}
			case Orientation.Vertical:
			{
				int num7 = _head_rect.Height;
				int num3 = e.Y - down_y;
				int num4 = base.Height - num7 - base.Padding.Vertical;
				double num5 = 1.0 - (double)(_limitBar_rect.Y - base.Padding.Top - num7 / 2 + num3) / (double)num4;
				num2 = min + (int)(num5 * (double)(max - min));
				if (num2 < min)
				{
					num2 = min;
				}
				if (num2 > max)
				{
					num2 = max;
				}
				down_y = e.Y;
				if (down_y < base.Padding.Top + num7 / 2)
				{
					down_y = base.Padding.Top + num7 / 2;
				}
				if (down_y > base.Height - num7 / 2 - base.Padding.Bottom)
				{
					down_y = base.Height - num7 / 2 - base.Padding.Bottom;
				}
				break;
			}
			}
			if (num2 != num)
			{
				_nLimitValue = num2;
				UpdateLimitBar();
				OnScroll(this, new LimitConstraint
				{
					LimitValue = _nLimitValue,
					MouseWheel = false
				});
			}
			Invalidate();
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		if ((e.Button == MouseButtons.Left || e.Button == MouseButtons.Right) && !_head_rect.IsEmpty)
		{
			sliding = false;
			_limitSliding = false;
			Invalidate();
			base.OnMouseUp(e);
		}
	}

	protected override void OnPaint(PaintEventArgs pe)
	{
		Graphics graphics = pe.Graphics;
		if (base.Enabled)
		{
			float num = (m_bGreenThumb ? 0.5f : 1f);
			float gamma = 1f;
			float num2 = 1f - 1f;
			float[][] newColorMatrix = new float[5][]
			{
				new float[5] { num, 0f, 0f, 0f, 0f },
				new float[5] { 0f, 1f, 0f, 0f, 0f },
				new float[5] { 0f, 0f, num, 0f, 0f },
				new float[5] { 0f, 0f, 0f, 1f, 0f },
				new float[5] { num2, num2, num2, 0f, 1f }
			};
			ImageAttributes imageAttributes = new ImageAttributes();
			imageAttributes.ClearColorMatrix();
			imageAttributes.SetColorMatrix(new ColorMatrix(newColorMatrix), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
			imageAttributes.SetGamma(gamma, ColorAdjustType.Bitmap);
			if (_bLimitEnabled)
			{
				Brush brush;
				if (_limitSliding)
				{
					brush = new SolidBrush(Color.FromArgb(255, _limitBarColor));
				}
				else if (val > _nLimitValue)
				{
					float num3 = 2f / 3f;
					brush = new SolidBrush(Color.FromArgb(255, (int)((float)(int)_limitBarColor.R * num3), (int)((float)(int)_limitBarColor.G * num3), (int)((float)(int)_limitBarColor.B * num3)));
				}
				else
				{
					float num4 = 22f / 51f;
					brush = new SolidBrush(Color.FromArgb(255, (int)((float)(int)_limitBarColor.R * num4), (int)((float)(int)_limitBarColor.G * num4), (int)((float)(int)_limitBarColor.B * num4)));
				}
				graphics.FillRectangle(brush, _limitBar_rect);
				brush.Dispose();
			}
			if (_head_image != null)
			{
				graphics.DrawImage(_head_image, new Rectangle(_head_rect.X, _head_rect.Y, _head_rect.Width, _head_rect.Height), 0, 0, _head_rect.Width, _head_rect.Height, GraphicsUnit.Pixel, imageAttributes);
			}
			imageAttributes.Dispose();
			imageAttributes = null;
			return;
		}
		float num5 = 0.5f;
		float gamma2 = 1f;
		float num6 = 1f - 1f;
		float[][] newColorMatrix2 = new float[5][]
		{
			new float[5] { num5, 0f, 0f, 0f, 0f },
			new float[5] { 0f, num5, 0f, 0f, 0f },
			new float[5] { 0f, 0f, num5, 0f, 0f },
			new float[5] { 0f, 0f, 0f, 1f, 0f },
			new float[5] { num6, num6, num6, 0f, 1f }
		};
		ImageAttributes imageAttributes2 = new ImageAttributes();
		imageAttributes2.ClearColorMatrix();
		imageAttributes2.SetColorMatrix(new ColorMatrix(newColorMatrix2), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
		imageAttributes2.SetGamma(gamma2, ColorAdjustType.Bitmap);
		if (_bLimitEnabled)
		{
			using Brush brush2 = new SolidBrush(Color.FromArgb(255, 64, 64, 64));
			graphics.FillRectangle(brush2, _limitBar_rect);
		}
		if (_head_image != null)
		{
			graphics.DrawImage(_head_image, new Rectangle(_head_rect.X, _head_rect.Y, _head_rect.Width, _head_rect.Height), 0, 0, _head_rect.Width, _head_rect.Height, GraphicsUnit.Pixel, imageAttributes2);
		}
		imageAttributes2.Dispose();
		imageAttributes2 = null;
	}

	protected virtual void OnScroll(object sender, EventArgs e)
	{
		if (Scroll != null)
		{
			Scroll(sender, e);
		}
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		if (Common.ShiftKeyDown)
		{
			if (e.Delta >= 120 && LimitValue + 1 <= Maximum)
			{
				LimitValue++;
				OnScroll(this, new LimitConstraint
				{
					LimitValue = _nLimitValue,
					MouseWheel = true
				});
			}
			else if (e.Delta <= -120 && LimitValue - 1 >= Minimum)
			{
				LimitValue--;
				OnScroll(this, new LimitConstraint
				{
					LimitValue = _nLimitValue,
					MouseWheel = true
				});
			}
		}
		else if (e.Delta >= 120 && Value + large_change <= Maximum)
		{
			Value += large_change;
			OnScroll(this, e);
		}
		else if (e.Delta <= -120 && Value - large_change >= Minimum)
		{
			Value -= large_change;
			OnScroll(this, e);
		}
		base.OnMouseWheel(e);
	}
}
