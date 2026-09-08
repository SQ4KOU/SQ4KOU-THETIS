using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Thetis;

[DefaultEvent("Changed")]
public class ucLGPicker : UserControl
{
	public struct ColourGradientData
	{
		public Color color;

		public float percent;
	}

	private struct GradColours
	{
		public float percent;

		public Color color;

		public bool enabled;

		public bool highlighted;
	}

	public delegate void GripperSelectedEventHandler(object sender, ColourEventArgs e);

	public delegate void GripperDBMChangedEventHandler(object sender, GripperEventArgs e);

	public delegate void GripperMouseEnterEventHandler(object sender, GripperEventArgs e);

	public delegate void GripperMouseLeaveEventHandler(object sender, GripperEventArgs e);

	private const int LOW = 150;

	private const int HIGH = 10;

	private const int SPAN = 160;

	private const int m_nPadding = 16;

	private const int m_nGrippers = 8;

	private int m_nSelectedGripper = -1;

	private bool m_bGripperSelectedForDrag;

	private bool m_bChangedDueToDrag;

	private bool m_bIncludeAlphaInPreview;

	private int m_nMouseOverIndex = -1;

	private bool _show_percent;

	private Dictionary<int, GradColours> m_dictColours;

	private IOrderedEnumerable<KeyValuePair<int, GradColours>> m_kvpSortedColours;

	private Font drawFont = new Font("Microsft Sans Serif", 8f);

	private readonly object m_objListLocker = new object();

	private IContainer components;

	private ToolTip toolTip1;

	public int Low => -150;

	public int High => 10;

	public bool ShowAsPercent
	{
		get
		{
			return _show_percent;
		}
		set
		{
			_show_percent = value;
			Invalidate();
		}
	}

	private int actualWidth
	{
		get
		{
			return base.Width - 32;
		}
		set
		{
		}
	}

	public Color ColourForSelectedGripper
	{
		get
		{
			if (m_nSelectedGripper == -1)
			{
				return Color.Empty;
			}
			return m_dictColours[m_nSelectedGripper].color;
		}
		set
		{
			if (m_nSelectedGripper != -1)
			{
				setColour(m_nSelectedGripper, value, bRefresh: true);
				OnChanged(EventArgs.Empty);
			}
		}
	}

	public override string Text
	{
		get
		{
			string text = m_dictColours.Count + "|";
			for (int i = 0; i < m_dictColours.Count; i++)
			{
				text = ((!m_dictColours[i].enabled) ? (text + "0|") : (text + "1|"));
				text = text + m_dictColours[i].percent.ToString("0.000") + "|";
				text = text + m_dictColours[i].color.ToArgb() + "|";
			}
			return text;
		}
		set
		{
			string[] array = value.Split('|');
			bool flag = false;
			Dictionary<int, GradColours> dictionary = new Dictionary<int, GradColours>();
			if (!int.TryParse(array[0], out var result))
			{
				return;
			}
			flag = array.Length == result * 3 + 2;
			if (flag)
			{
				flag = false;
				for (int i = 0; i < result; i++)
				{
					bool bEnable = false;
					int result2 = 0;
					float result3 = 0f;
					int num = 1 + i * 3;
					if (array[num] == "1")
					{
						bEnable = true;
					}
					flag = float.TryParse(array[num + 1], out result3);
					if (flag)
					{
						flag = int.TryParse(array[num + 2], out result2);
					}
					if (flag)
					{
						addColour(i, result3, Color.FromArgb(result2), dictionary, bEnable);
					}
					if (!flag)
					{
						break;
					}
				}
			}
			if (!flag)
			{
				return;
			}
			lock (m_objListLocker)
			{
				m_dictColours.Clear();
				foreach (KeyValuePair<int, GradColours> item in dictionary)
				{
					m_dictColours.Add(item.Key, item.Value);
				}
			}
			rebuildSortedColours();
			HighlightFirstGripper();
			Invalidate();
			OnChanged(EventArgs.Empty);
		}
	}

	public bool IncludeAlphaInPreview
	{
		get
		{
			return m_bIncludeAlphaInPreview;
		}
		set
		{
			m_bIncludeAlphaInPreview = value;
			rebuildSortedColours();
			Invalidate();
		}
	}

	public string EncodedText
	{
		get
		{
			try
			{
				return Convert.ToBase64String(Encoding.UTF8.GetBytes(Text));
			}
			catch
			{
				return "";
			}
		}
		set
		{
			try
			{
				Text = Encoding.UTF8.GetString(Convert.FromBase64String(value));
			}
			catch
			{
			}
		}
	}

	public event EventHandler Changed;

	public event GripperSelectedEventHandler GripperSelected;

	public event GripperDBMChangedEventHandler GripperDBMChanged;

	public event GripperMouseEnterEventHandler GripperMouseEnter;

	public event GripperMouseLeaveEventHandler GripperMouseLeave;

	public ucLGPicker()
	{
		InitializeComponent();
		m_dictColours = new Dictionary<int, GradColours>();
		for (int i = 0; i < 8; i++)
		{
			addColour(i, (float)i / 8f, Color.FromArgb(255, i * 31, i * 31, i * 31), m_dictColours);
		}
		addColour(8, 1f, Color.FromArgb(255, 255, 255, 255), m_dictColours);
		for (int j = 1; j < 8; j++)
		{
			enableGripper(j, bEnable: false);
		}
		rebuildSortedColours();
	}

	private void rebuildSortedColours()
	{
		lock (m_objListLocker)
		{
			List<KeyValuePair<int, GradColours>> source = m_dictColours.ToList();
			m_kvpSortedColours = source.Where(delegate(KeyValuePair<int, GradColours> pair)
			{
				KeyValuePair<int, GradColours> keyValuePair = pair;
				return keyValuePair.Value.enabled;
			}).OrderBy(delegate(KeyValuePair<int, GradColours> pair)
			{
				KeyValuePair<int, GradColours> keyValuePair = pair;
				return keyValuePair.Value.percent;
			});
		}
	}

	private void enableGripper(int index, bool bEnable)
	{
		lock (m_objListLocker)
		{
			GradColours value = m_dictColours[index];
			value.enabled = bEnable;
			value.highlighted = false;
			m_dictColours[index] = value;
		}
	}

	private void addColour(int index, float percpos, Color c, Dictionary<int, GradColours> lstColours, bool bEnable = true)
	{
		lock (m_objListLocker)
		{
			lstColours.Add(index, new GradColours
			{
				color = c,
				percent = percpos,
				enabled = bEnable,
				highlighted = false
			});
		}
	}

	private void drawTextCentre(Graphics g, string s, int x)
	{
		Size size = TextRenderer.MeasureText(s, drawFont);
		Brush brush = (base.Enabled ? Brushes.Black : Brushes.Gray);
		g.DrawString(s, drawFont, brush, new Point(x - size.Width / 2, 24));
	}

	private void drawScales(Graphics g)
	{
		int num = 150;
		int num2 = 160;
		if (_show_percent)
		{
			drawTextCentre(g, "LOW", 16);
			drawTextCentre(g, "HIGH", 16 + actualWidth);
			drawTextCentre(g, "MID", 16 + actualWidth / 2);
		}
		else
		{
			drawTextCentre(g, (-150).ToString(), 16);
			drawTextCentre(g, 10.ToString(), 16 + actualWidth);
			drawTextCentre(g, "0", 16 + (int)((float)actualWidth / (float)num2 * (float)num));
			drawTextCentre(g, "-93", 16 + (int)((float)actualWidth / (float)num2 * (float)(num - 93)));
			drawTextCentre(g, "-73", 16 + (int)((float)actualWidth / (float)num2 * (float)(num - 73)));
		}
	}

	private void LGPicker_Paint(object sender, PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		graphics.PixelOffsetMode = PixelOffsetMode.Half;
		Point point = new Point(16 + actualWidth, 12);
		GradColours gradColours = m_kvpSortedColours.First().Value;
		bool flag = true;
		Brush brush2;
		foreach (KeyValuePair<int, GradColours> kvpSortedColour in m_kvpSortedColours)
		{
			if (!flag)
			{
				GradColours value = kvpSortedColour.Value;
				Point point2 = new Point(16 + (int)((float)actualWidth * gradColours.percent), 12);
				Point point3 = new Point(16 + (int)((float)actualWidth * value.percent), 12);
				if (point2.X < point3.X)
				{
					Color color = (base.Enabled ? gradColours.color : Color.Gray);
					Color color2 = (base.Enabled ? value.color : Color.Gray);
					if (!m_bIncludeAlphaInPreview)
					{
						color = Color.FromArgb(255, color);
						color2 = Color.FromArgb(255, color2);
					}
					using LinearGradientBrush brush = new LinearGradientBrush(point2, point3, color, color2);
					using Pen pen = new Pen(brush, 24f);
					graphics.DrawLine(pen, point2, point3);
					brush2 = (gradColours.highlighted ? Brushes.Red : Brushes.White);
					brush2 = (base.Enabled ? brush2 : Brushes.Gray);
					graphics.FillEllipse(brush2, point2.X - 6, point2.Y - 6, 12, 12);
					graphics.DrawEllipse(Pens.Black, point2.X - 6, point2.Y - 6, 12, 12);
				}
				gradColours = value;
			}
			flag = false;
		}
		brush2 = (gradColours.highlighted ? Brushes.Red : Brushes.White);
		brush2 = (base.Enabled ? brush2 : Brushes.Gray);
		graphics.FillEllipse(brush2, point.X - 6, point.Y - 6, 12, 12);
		graphics.DrawEllipse(Pens.Black, point.X - 6, point.Y - 6, 12, 12);
		drawScales(graphics);
	}

	private int findColourIndexGrip(int X, int Y)
	{
		int result = -1;
		for (int i = 0; i < m_dictColours.Count; i++)
		{
			int num = 16 + (int)((float)actualWidth * m_dictColours[i].percent);
			if (m_dictColours[i].enabled && X >= num - 6 && X <= num + 6 && Y >= 0 && Y <= 24)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	private void LGPicker_MouseMove(object sender, MouseEventArgs e)
	{
		int num = e.X;
		int num2 = e.Y;
		if (m_bGripperSelectedForDrag && m_nSelectedGripper > 0 && m_nSelectedGripper < m_dictColours.Count - 1)
		{
			float num3 = percFromPixels(num);
			int key = indexOfClosestToLeft(m_nSelectedGripper);
			int key2 = indexOfClosestToRight(m_nSelectedGripper);
			if ((float)actualWidth * num3 < (float)actualWidth * m_dictColours[key].percent + 6f)
			{
				num3 = ((float)actualWidth * m_dictColours[key].percent + 6f) / (float)actualWidth;
			}
			if ((float)actualWidth * num3 > (float)actualWidth * m_dictColours[key2].percent - 6f)
			{
				num3 = ((float)actualWidth * m_dictColours[key2].percent - 6f) / (float)actualWidth;
			}
			lock (m_objListLocker)
			{
				GradColours value = m_dictColours[m_nSelectedGripper];
				value.percent = num3;
				m_dictColours[m_nSelectedGripper] = value;
			}
			m_bChangedDueToDrag = true;
			rebuildSortedColours();
			Invalidate();
			OnGripperDBMChanged(-150 + (int)(160f * num3), num3);
			return;
		}
		int num4 = findColourIndexGrip(num, num2);
		if (num4 != -1)
		{
			if (num4 != m_nMouseOverIndex)
			{
				if (m_nMouseOverIndex != -1)
				{
					OnGripperMouseLeave(0, 0f);
				}
				int dbm = -150 + (int)(160f * m_dictColours[num4].percent);
				OnGripperMouseEnter(dbm, m_dictColours[num4].percent);
				m_nMouseOverIndex = num4;
			}
		}
		else if (m_nMouseOverIndex != -1)
		{
			OnGripperMouseLeave(0, 0f);
			m_nMouseOverIndex = -1;
		}
	}

	private void highlightGripper(int index, bool bHightlight)
	{
		if (index >= 0 && index <= m_dictColours.Count - 1)
		{
			lock (m_objListLocker)
			{
				GradColours value = m_dictColours[index];
				value.highlighted = bHightlight;
				m_dictColours[index] = value;
			}
			rebuildSortedColours();
		}
	}

	private float percFromPixels(int X)
	{
		float num = (float)(X - 16) / (float)actualWidth;
		if (num < 0f)
		{
			num = 0f;
		}
		if (num > 1f)
		{
			num = 1f;
		}
		return num;
	}

	private void LGPicker_MouseDown(object sender, MouseEventArgs e)
	{
		int num = e.X;
		int num2 = e.Y;
		int num3 = findColourIndexGrip(num, num2);
		if (num3 != m_nSelectedGripper)
		{
			highlightGripper(m_nSelectedGripper, bHightlight: false);
		}
		if (num3 == -1 && num2 >= 0 && num2 <= 24)
		{
			float perc = percFromPixels(num);
			int num4 = indexOfClosestToLeft(perc);
			int num5 = indexOfClosestToRight(perc);
			if (num4 == -1 || num5 == -1)
			{
				return;
			}
			float num6 = m_dictColours[num5].percent - m_dictColours[num4].percent;
			if (num6 * (float)actualWidth > 32f)
			{
				float perc2 = m_dictColours[num4].percent + num6 / 2f;
				int num7 = addGripper(perc2, GetColourAtPercent(perc2));
				if (num7 != -1)
				{
					highlightGripper(num7, bHightlight: true);
					m_nSelectedGripper = num7;
					OnGripperSelected(m_dictColours[m_nSelectedGripper].color);
					Invalidate();
				}
			}
		}
		else if (num3 != -1)
		{
			if (e.Button != MouseButtons.None)
			{
				m_bGripperSelectedForDrag = true;
				highlightGripper(num3, bHightlight: true);
				m_nSelectedGripper = num3;
				OnGripperSelected(m_dictColours[m_nSelectedGripper].color);
			}
			Invalidate();
		}
	}

	private void LGPicker_MouseUp(object sender, MouseEventArgs e)
	{
		_ = e.X;
		_ = e.Y;
		if (m_bChangedDueToDrag)
		{
			m_bChangedDueToDrag = false;
			OnChanged(EventArgs.Empty);
		}
		m_bGripperSelectedForDrag = false;
		if (m_nSelectedGripper == -1)
		{
			OnGripperSelected(Color.Empty);
		}
	}

	private int indexOfClosestToLeft(float perc)
	{
		int result = -1;
		float num = float.MaxValue;
		for (int i = 0; i < m_dictColours.Count; i++)
		{
			if (m_dictColours[i].enabled && m_dictColours[i].percent < perc && perc - m_dictColours[i].percent < num)
			{
				num = perc - m_dictColours[i].percent;
				result = i;
			}
		}
		return result;
	}

	private int indexOfClosestToLeft(int index)
	{
		int result = -1;
		float num = float.MaxValue;
		for (int i = 0; i < m_dictColours.Count; i++)
		{
			if (m_dictColours[i].enabled && m_dictColours[i].percent < m_dictColours[index].percent && m_dictColours[index].percent - m_dictColours[i].percent < num)
			{
				num = m_dictColours[index].percent - m_dictColours[i].percent;
				result = i;
			}
		}
		return result;
	}

	private int indexAtPerc(float perc)
	{
		int result = -1;
		for (int i = 0; i < m_dictColours.Count; i++)
		{
			if (m_dictColours[i].enabled && m_dictColours[i].percent == perc)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	private int indexOfClosestToRight(float perc)
	{
		int result = -1;
		float num = float.MaxValue;
		for (int i = 0; i < m_dictColours.Count; i++)
		{
			if (m_dictColours[i].enabled && m_dictColours[i].percent > perc && m_dictColours[i].percent - perc < num)
			{
				num = m_dictColours[i].percent - perc;
				result = i;
			}
		}
		return result;
	}

	private int indexOfClosestToRight(int index)
	{
		int result = -1;
		float num = float.MaxValue;
		for (int i = 0; i < m_dictColours.Count; i++)
		{
			if (m_dictColours[i].enabled && m_dictColours[i].percent > m_dictColours[index].percent && m_dictColours[i].percent - m_dictColours[index].percent < num)
			{
				num = m_dictColours[i].percent - m_dictColours[index].percent;
				result = i;
			}
		}
		return result;
	}

	private void LGPicker_MouseLeave(object sender, EventArgs e)
	{
		if (m_nMouseOverIndex != -1)
		{
			OnGripperMouseLeave(0, 0f);
			m_nMouseOverIndex = -1;
		}
	}

	private void setColour(int index, Color c, bool bRefresh = false)
	{
		if (index >= 0 && index <= m_dictColours.Count - 1)
		{
			lock (m_objListLocker)
			{
				GradColours value = m_dictColours[index];
				value.color = c;
				m_dictColours[index] = value;
			}
			rebuildSortedColours();
			if (bRefresh)
			{
				Invalidate();
			}
		}
	}

	public void RemoveSelectedGripper(bool bRefresh = false)
	{
		if (m_nSelectedGripper != -1 && m_nSelectedGripper != 0 && m_nSelectedGripper != m_dictColours.Count - 1)
		{
			enableGripper(m_nSelectedGripper, bEnable: false);
			m_nSelectedGripper = -1;
			rebuildSortedColours();
			if (bRefresh)
			{
				Invalidate();
			}
			OnChanged(EventArgs.Empty);
		}
	}

	private int addGripper(float perc, Color colour, bool bRefresh = false)
	{
		int num = findFreeDisabledGripper();
		if (num == -1)
		{
			return -1;
		}
		lock (m_objListLocker)
		{
			GradColours value = m_dictColours[num];
			value.color = colour;
			value.percent = perc;
			value.enabled = true;
			value.highlighted = false;
			m_dictColours[num] = value;
		}
		rebuildSortedColours();
		if (bRefresh)
		{
			Invalidate();
		}
		OnChanged(EventArgs.Empty);
		return num;
	}

	private int findFreeDisabledGripper()
	{
		int result = -1;
		for (int i = 1; i < m_dictColours.Count - 1; i++)
		{
			if (!m_dictColours[i].enabled)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	public void HighlightFirstGripper()
	{
		for (int i = 0; i < m_dictColours.Count; i++)
		{
			if (m_dictColours[i].enabled)
			{
				highlightGripper(i, bHightlight: true);
				m_nSelectedGripper = i;
				Invalidate();
				OnGripperSelected(m_dictColours[i].color);
				break;
			}
		}
	}

	public void Clear()
	{
		for (int i = 1; i < m_dictColours.Count - 1; i++)
		{
			enableGripper(i, bEnable: false);
		}
		rebuildSortedColours();
		Invalidate();
		OnChanged(EventArgs.Empty);
	}

	private void OnChanged(EventArgs e)
	{
		Changed?.Invoke(this, e);
	}

	private void OnGripperSelected(Color c)
	{
		GripperSelected?.Invoke(this, new ColourEventArgs(c));
	}

	private void OnGripperMouseEnter(int dbm, float percent)
	{
		GripperMouseEnter?.Invoke(this, new GripperEventArgs(dbm, percent));
	}

	private void OnGripperMouseLeave(int dbm, float percent)
	{
		GripperMouseLeave?.Invoke(this, new GripperEventArgs(dbm, percent));
	}

	private void OnGripperDBMChanged(int dbm, float percent)
	{
		GripperDBMChanged?.Invoke(this, new GripperEventArgs(dbm, percent));
	}

	private void LGPicker_EnabledChanged(object sender, EventArgs e)
	{
		Invalidate();
	}

	public Color GetColourForDBM(float dbm)
	{
		float percForDBM = GetPercForDBM(dbm);
		return GetColourAtPercent(percForDBM);
	}

	public Color GetColourAtPercent(float perc)
	{
		Color result = Color.Empty;
		int num = indexAtPerc(perc);
		if (num == -1)
		{
			num = indexOfClosestToLeft(perc);
		}
		int num2 = indexOfClosestToRight(perc);
		if (num2 == -1)
		{
			num2 = num;
		}
		if (num != -1 && num2 != -1)
		{
			float num3 = m_dictColours[num2].percent - m_dictColours[num].percent;
			float num4 = ((num3 == 0f) ? 0f : (1f / num3));
			perc = (perc - m_dictColours[num].percent) * num4;
			result = ColorInterpolator.InterpolateBetween(m_dictColours[num].color, m_dictColours[num2].color, perc);
		}
		return result;
	}

	public float GetPercForDBM(float dbm)
	{
		float num = 160f;
		dbm += 150f;
		float num2 = dbm / num;
		if (num2 < 0f)
		{
			num2 = 0f;
		}
		if (num2 > 1f)
		{
			num2 = 1f;
		}
		return num2;
	}

	private void addColourGradientData(List<ColourGradientData> lst, Color color, float perc)
	{
		lst.Add(new ColourGradientData
		{
			color = color,
			percent = perc
		});
	}

	public List<ColourGradientData> GetColourGradientDataForDBMRange(float low, float high)
	{
		lock (m_objListLocker)
		{
			List<ColourGradientData> list = new List<ColourGradientData>();
			float perc = GetPercForDBM(low);
			float percForDBM = GetPercForDBM(high);
			int num = indexAtPerc(perc);
			int num2 = indexAtPerc(percForDBM);
			if (num != -1)
			{
				addColourGradientData(list, m_dictColours[num].color, m_dictColours[num].percent);
				perc = m_dictColours[num].percent;
			}
			else
			{
				addColourGradientData(list, GetColourAtPercent(perc), perc);
			}
			bool flag = true;
			while (flag)
			{
				int num3 = indexOfClosestToRight(perc);
				if (num3 != -1)
				{
					if (m_dictColours[num3].percent < percForDBM)
					{
						addColourGradientData(list, m_dictColours[num3].color, m_dictColours[num3].percent);
					}
					else
					{
						if (num2 != -1)
						{
							addColourGradientData(list, m_dictColours[num2].color, m_dictColours[num2].percent);
						}
						else
						{
							addColourGradientData(list, GetColourAtPercent(percForDBM), percForDBM);
						}
						flag = false;
					}
					perc = m_dictColours[num3].percent;
				}
				else
				{
					flag = false;
				}
			}
			float percent = list.First().percent;
			float percent2 = list.Last().percent;
			float num4 = 1f / (percent2 - percent);
			for (int i = 0; i < list.Count; i++)
			{
				ColourGradientData value = list[i];
				value.percent -= percent;
				value.percent *= num4;
				list[i] = value;
			}
			return list;
		}
	}

	public void ApplyGlobalAlpha(int A)
	{
		lock (m_objListLocker)
		{
			for (int i = 0; i < m_dictColours.Count; i++)
			{
				GradColours value = m_dictColours[i];
				if (value.enabled)
				{
					value.color = Color.FromArgb(A, value.color);
					m_dictColours[i] = value;
				}
			}
		}
		rebuildSortedColours();
		Invalidate();
		OnChanged(EventArgs.Empty);
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
		this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.DoubleBuffered = true;
		base.Name = "ucLGPicker";
		base.Size = new System.Drawing.Size(315, 53);
		base.EnabledChanged += new System.EventHandler(LGPicker_EnabledChanged);
		base.Paint += new System.Windows.Forms.PaintEventHandler(LGPicker_Paint);
		base.MouseDown += new System.Windows.Forms.MouseEventHandler(LGPicker_MouseDown);
		base.MouseLeave += new System.EventHandler(LGPicker_MouseLeave);
		base.MouseMove += new System.Windows.Forms.MouseEventHandler(LGPicker_MouseMove);
		base.MouseUp += new System.Windows.Forms.MouseEventHandler(LGPicker_MouseUp);
		base.ResumeLayout(false);
	}
}
