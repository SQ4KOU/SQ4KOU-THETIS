using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms;

public class TextBoxTS : TextBox
{
	public new bool AcceptsReturn
	{
		get
		{
			return base.AcceptsReturn;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetTextBoxAcceptsReturn), this, value);
			}
			else
			{
				base.AcceptsReturn = value;
			}
		}
	}

	public new bool AcceptsTab
	{
		get
		{
			return base.AcceptsTab;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetTextBoxBaseAcceptsTab), this, value);
			}
			else
			{
				base.AcceptsTab = value;
			}
		}
	}

	public new string AccessibleDefaultActionDescription
	{
		get
		{
			return base.AccessibleDefaultActionDescription;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlAccessibleDefaultActionDescription), this, value);
			}
			else
			{
				base.AccessibleDefaultActionDescription = value;
			}
		}
	}

	public new string AccessibleDescription
	{
		get
		{
			return base.AccessibleDescription;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlAccessibleDescription), this, value);
			}
			else
			{
				base.AccessibleDescription = value;
			}
		}
	}

	public new string AccessibleName
	{
		get
		{
			return base.AccessibleName;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlAccessibleName), this, value);
			}
			else
			{
				base.AccessibleName = value;
			}
		}
	}

	public new AccessibleRole AccessibleRole
	{
		get
		{
			return base.AccessibleRole;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlAccessibleRole), this, value);
			}
			else
			{
				base.AccessibleRole = value;
			}
		}
	}

	public new bool AllowDrop
	{
		get
		{
			return base.AllowDrop;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlAllowDrop), this, value);
			}
			else
			{
				base.AllowDrop = value;
			}
		}
	}

	public new AnchorStyles Anchor
	{
		get
		{
			return base.Anchor;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlAnchor), this, value);
			}
			else
			{
				base.Anchor = value;
			}
		}
	}

	public new bool AutoSize
	{
		get
		{
			return base.AutoSize;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetTextBoxBaseAutoSize), this, value);
			}
			else
			{
				base.AutoSize = value;
			}
		}
	}

	public new Color BackColor
	{
		get
		{
			return base.BackColor;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetTextBoxBaseBackColor), this, value);
			}
			else
			{
				base.BackColor = value;
			}
		}
	}

	public new virtual Image BackgroundImage
	{
		get
		{
			return base.BackgroundImage;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlBackgroundImage), this, value);
			}
			else
			{
				base.BackgroundImage = value;
			}
		}
	}

	public new virtual BindingContext BindingContext
	{
		get
		{
			return base.BindingContext;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlBindingContext), this, value);
			}
			else
			{
				base.BindingContext = value;
			}
		}
	}

	public new BorderStyle BorderStyle
	{
		get
		{
			return base.BorderStyle;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetTextBoxBaseBorderStyle), this, value);
			}
			else
			{
				base.BorderStyle = value;
			}
		}
	}

	public new Rectangle Bounds
	{
		get
		{
			return base.Bounds;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlBounds), this, value);
			}
			else
			{
				base.Bounds = value;
			}
		}
	}

	public new bool Capture
	{
		get
		{
			return base.Capture;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlCapture), this, value);
			}
			else
			{
				base.Capture = value;
			}
		}
	}

	public new bool CausesValidation
	{
		get
		{
			return base.CausesValidation;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlCausesValidation), this, value);
			}
			else
			{
				base.CausesValidation = value;
			}
		}
	}

	public new CharacterCasing CharacterCasing
	{
		get
		{
			return base.CharacterCasing;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetTextBoxCharacterCasing), this, value);
			}
			else
			{
				base.CharacterCasing = value;
			}
		}
	}

	public new Size ClientSize
	{
		get
		{
			return base.ClientSize;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlClientSize), this, value);
			}
			else
			{
				base.ClientSize = value;
			}
		}
	}

	public new ContextMenuStrip ContextMenuStrip
	{
		get
		{
			return base.ContextMenuStrip;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlContextMenuStrip), this, value);
			}
			else
			{
				base.ContextMenuStrip = value;
			}
		}
	}

	public new Cursor Cursor
	{
		get
		{
			return base.Cursor;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlCursor), this, value);
			}
			else
			{
				base.Cursor = value;
			}
		}
	}

	public new DockStyle Dock
	{
		get
		{
			return base.Dock;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlDock), this, value);
			}
			else
			{
				base.Dock = value;
			}
		}
	}

	public new bool Enabled
	{
		get
		{
			return base.Enabled;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlEnabled), this, value);
			}
			else
			{
				base.Enabled = value;
			}
		}
	}

	public new Font Font
	{
		get
		{
			return base.Font;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlFont), this, value);
			}
			else
			{
				base.Font = value;
			}
		}
	}

	public new Color ForeColor
	{
		get
		{
			return base.ForeColor;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetTextBoxBaseForeColor), this, value);
			}
			else
			{
				base.ForeColor = value;
			}
		}
	}

	public new int Height
	{
		get
		{
			return base.Height;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlHeight), this, value);
			}
			else
			{
				base.Height = value;
			}
		}
	}

	public new bool HideSelection
	{
		get
		{
			return base.HideSelection;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetTextBoxBaseHideSelection), this, value);
			}
			else
			{
				base.HideSelection = value;
			}
		}
	}

	public new ImeMode ImeMode
	{
		get
		{
			return base.ImeMode;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlImeMode), this, value);
			}
			else
			{
				base.ImeMode = value;
			}
		}
	}

	public new bool IsAccessible
	{
		get
		{
			return base.IsAccessible;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlIsAccessible), this, value);
			}
			else
			{
				base.IsAccessible = value;
			}
		}
	}

	public new int Left
	{
		get
		{
			return base.Left;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlLeft), this, value);
			}
			else
			{
				base.Left = value;
			}
		}
	}

	public new string[] Lines
	{
		get
		{
			return base.Lines;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetTextBoxBaseLines), this, value);
			}
			else
			{
				base.Lines = value;
			}
		}
	}

	public new Point Location
	{
		get
		{
			return base.Location;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlLocation), this, value);
			}
			else
			{
				base.Location = value;
			}
		}
	}

	public new int MaxLength
	{
		get
		{
			return base.MaxLength;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetTextBoxBaseMaxLength), this, value);
			}
			else
			{
				base.MaxLength = value;
			}
		}
	}

	public new bool Modified
	{
		get
		{
			return base.Modified;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetTextBoxBaseModified), this, value);
			}
			else
			{
				base.Modified = value;
			}
		}
	}

	public new bool Multiline
	{
		get
		{
			return base.Multiline;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetTextBoxBaseMultiline), this, value);
			}
			else
			{
				base.Multiline = value;
			}
		}
	}

	public new string Name
	{
		get
		{
			return base.Name;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlName), this, value);
			}
			else
			{
				base.Name = value;
			}
		}
	}

	public new Control Parent
	{
		get
		{
			return base.Parent;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlParent), this, value);
			}
			else
			{
				base.Parent = value;
			}
		}
	}

	public new char PasswordChar
	{
		get
		{
			return base.PasswordChar;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetTextBoxPasswordChar), this, value);
			}
			else
			{
				base.PasswordChar = value;
			}
		}
	}

	public new bool ReadOnly
	{
		get
		{
			return base.ReadOnly;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetTextBoxBaseReadOnly), this, value);
			}
			else
			{
				base.ReadOnly = value;
			}
		}
	}

	public new Region Region
	{
		get
		{
			return base.Region;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlRegion), this, value);
			}
			else
			{
				base.Region = value;
			}
		}
	}

	public new RightToLeft RightToLeft
	{
		get
		{
			return base.RightToLeft;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlRightToLeft), this, value);
			}
			else
			{
				base.RightToLeft = value;
			}
		}
	}

	public new ScrollBars ScrollBars
	{
		get
		{
			return base.ScrollBars;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetTextBoxScrollBars), this, value);
			}
			else
			{
				base.ScrollBars = value;
			}
		}
	}

	public new virtual string SelectedText
	{
		get
		{
			return base.SelectedText;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetTextBoxBaseSelectedText), this, value);
			}
			else
			{
				base.SelectedText = value;
			}
		}
	}

	public new int SelectionLength
	{
		get
		{
			return base.SelectionLength;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetTextBoxSelectionLength), this, value);
			}
			else
			{
				base.SelectionLength = value;
			}
		}
	}

	public new int SelectionStart
	{
		get
		{
			return base.SelectionStart;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetTextBoxBaseSelectionStart), this, value);
			}
			else
			{
				base.SelectionStart = value;
			}
		}
	}

	public new ISite Site
	{
		get
		{
			return base.Site;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlSite), this, value);
			}
			else
			{
				base.Site = value;
			}
		}
	}

	public new Size Size
	{
		get
		{
			return base.Size;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlSize), this, value);
			}
			else
			{
				base.Size = value;
			}
		}
	}

	public new int TabIndex
	{
		get
		{
			return base.TabIndex;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlTabIndex), this, value);
			}
			else
			{
				base.TabIndex = value;
			}
		}
	}

	public new bool TabStop
	{
		get
		{
			return base.TabStop;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlTabStop), this, value);
			}
			else
			{
				base.TabStop = value;
			}
		}
	}

	public new object Tag
	{
		get
		{
			return base.Tag;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlTag), this, value);
			}
			else
			{
				base.Tag = value;
			}
		}
	}

	public new string Text
	{
		get
		{
			return base.Text;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetTextBoxText), this, value);
			}
			else
			{
				base.Text = value;
			}
		}
	}

	public new HorizontalAlignment TextAlign
	{
		get
		{
			return base.TextAlign;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetTextBoxTextAlign), this, value);
			}
			else
			{
				base.TextAlign = value;
			}
		}
	}

	public new int Top
	{
		get
		{
			return base.Top;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlTop), this, value);
			}
			else
			{
				base.Top = value;
			}
		}
	}

	public new bool Visible
	{
		get
		{
			return base.Visible;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlVisible), this, value);
			}
			else
			{
				base.Visible = value;
			}
		}
	}

	public new int Width
	{
		get
		{
			return base.Width;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetControlWidth), this, value);
			}
			else
			{
				base.Width = value;
			}
		}
	}

	public new bool WordWrap
	{
		get
		{
			return base.WordWrap;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(new UI.SetCtrlDel(UI.SetTextBoxBaseWordWrap), this, value);
			}
			else
			{
				base.WordWrap = value;
			}
		}
	}

	public new void AppendText(string text)
	{
		if (base.InvokeRequired)
		{
			Invoke(new UI.CtrlVoidFunc(UI.CallTextBoxBaseAppendText), this, new object[1] { text });
		}
		else
		{
			base.AppendText(text);
		}
	}

	public new void BringToFront()
	{
		if (base.InvokeRequired)
		{
			Invoke(new MethodInvoker(base.BringToFront));
		}
		else
		{
			base.BringToFront();
		}
	}

	public new void Clear()
	{
		if (base.InvokeRequired)
		{
			Invoke(new MethodInvoker(base.Clear));
		}
		else
		{
			base.Clear();
		}
	}

	public new void ClearUndo()
	{
		if (base.InvokeRequired)
		{
			Invoke(new MethodInvoker(base.ClearUndo));
		}
		else
		{
			base.ClearUndo();
		}
	}

	public new bool Contains(Control ctl)
	{
		if (base.InvokeRequired)
		{
			IAsyncResult asyncResult = BeginInvoke(new UI.CtrlRetFunc(UI.CallControlContains), this, new object[1] { ctl });
			return (bool)EndInvoke(asyncResult);
		}
		return base.Contains(ctl);
	}

	public new void Copy()
	{
		if (base.InvokeRequired)
		{
			Invoke(new MethodInvoker(base.Copy));
		}
		else
		{
			base.Copy();
		}
	}

	public new void CreateControl()
	{
		if (base.InvokeRequired)
		{
			Invoke(new MethodInvoker(base.CreateControl));
		}
		else
		{
			base.CreateControl();
		}
	}

	public new Graphics CreateGraphics()
	{
		if (base.InvokeRequired)
		{
			IAsyncResult asyncResult = BeginInvoke(new UI.CtrlRetFunc(UI.CallControlCreateGraphics), this, new object[1]);
			return (Graphics)EndInvoke(asyncResult);
		}
		return base.CreateGraphics();
	}

	public new void Cut()
	{
		if (base.InvokeRequired)
		{
			Invoke(new MethodInvoker(base.Cut));
		}
		else
		{
			base.Cut();
		}
	}

	public new virtual void Dispose()
	{
		if (base.InvokeRequired)
		{
			Invoke(new MethodInvoker(base.Dispose));
		}
		else
		{
			base.Dispose();
		}
	}

	public new DragDropEffects DoDragDrop(object data, DragDropEffects allowedEffects)
	{
		if (base.InvokeRequired)
		{
			IAsyncResult asyncResult = BeginInvoke(new UI.CtrlRetFunc(UI.CallControlDoDragDrop), this, new object[2] { data, allowedEffects });
			return (DragDropEffects)EndInvoke(asyncResult);
		}
		return base.DoDragDrop(data, allowedEffects);
	}

	public new virtual object Equals(object obj)
	{
		if (base.InvokeRequired)
		{
			IAsyncResult asyncResult = BeginInvoke(new UI.CtrlRetFunc(UI.CallObjectEquals), this, new object[1] { obj });
			return EndInvoke(asyncResult);
		}
		return base.Equals(obj);
	}

	public new Form FindForm()
	{
		if (base.InvokeRequired)
		{
			IAsyncResult asyncResult = BeginInvoke(new UI.CtrlRetFunc(UI.CallControlFindForm), this, new object[1]);
			return (Form)EndInvoke(asyncResult);
		}
		return base.FindForm();
	}

	public new bool Focus()
	{
		if (base.InvokeRequired)
		{
			IAsyncResult asyncResult = BeginInvoke(new UI.CtrlRetFunc(UI.CallControlFocus), this, new object[1]);
			return (bool)EndInvoke(asyncResult);
		}
		return base.Focus();
	}

	public new Control GetChildAtPoint(Point pt)
	{
		if (base.InvokeRequired)
		{
			IAsyncResult asyncResult = BeginInvoke(new UI.CtrlRetFunc(UI.CallControlGetChildAtPoint), this, new object[1] { pt });
			return (Control)EndInvoke(asyncResult);
		}
		return base.GetChildAtPoint(pt);
	}

	public new IContainerControl GetContainerControl()
	{
		if (base.InvokeRequired)
		{
			IAsyncResult asyncResult = BeginInvoke(new UI.CtrlRetFunc(UI.CallControlGetContainerControl), this, new object[0]);
			return (IContainerControl)EndInvoke(asyncResult);
		}
		return base.GetContainerControl();
	}

	public new virtual int GetHashCode()
	{
		if (base.InvokeRequired)
		{
			IAsyncResult asyncResult = BeginInvoke(new UI.CtrlRetFunc(UI.CallObjectGetHashCode), this, new object[0]);
			return (int)EndInvoke(asyncResult);
		}
		return base.GetHashCode();
	}

	public new virtual object GetLifetimeService()
	{
		if (base.InvokeRequired)
		{
			IAsyncResult asyncResult = BeginInvoke(new UI.CtrlRetFunc(UI.CallMarshalByRefObjectGetLifetimeService), this, new object[0]);
			return EndInvoke(asyncResult);
		}
		return base.GetLifetimeService();
	}

	public new Control GetNextControl(Control ctl, bool forward)
	{
		if (base.InvokeRequired)
		{
			IAsyncResult asyncResult = BeginInvoke(new UI.CtrlRetFunc(UI.CallControlGetNextControl), this, new object[2] { ctl, forward });
			return (Control)EndInvoke(asyncResult);
		}
		return base.GetNextControl(ctl, forward);
	}

	public new Type GetType()
	{
		if (base.InvokeRequired)
		{
			IAsyncResult asyncResult = BeginInvoke(new UI.CtrlRetFunc(UI.CallObjectGetType), this, new object[0]);
			return (Type)EndInvoke(asyncResult);
		}
		return base.GetType();
	}

	public new void Hide()
	{
		if (base.InvokeRequired)
		{
			Invoke(new MethodInvoker(base.Hide));
		}
		else
		{
			base.Hide();
		}
	}

	public new virtual object InitializeLifetimeService()
	{
		if (base.InvokeRequired)
		{
			IAsyncResult asyncResult = BeginInvoke(new UI.CtrlRetFunc(UI.CallMarshalByRefObjectInitializeLifetimeService), this, new object[0]);
			return EndInvoke(asyncResult);
		}
		return base.InitializeLifetimeService();
	}

	public new void Invalidate()
	{
		if (base.InvokeRequired)
		{
			Invoke(new MethodInvoker(base.Invalidate));
		}
		else
		{
			base.Invalidate();
		}
	}

	public new void Invalidate(bool invalidateChildren)
	{
		if (base.InvokeRequired)
		{
			Invoke(new UI.CtrlVoidFunc(UI.CallControlInvalidate), this, new object[1] { invalidateChildren });
		}
		else
		{
			base.Invalidate(invalidateChildren);
		}
	}

	public new void Invalidate(Rectangle rc)
	{
		if (base.InvokeRequired)
		{
			Invoke(new UI.CtrlVoidFunc(UI.CallControlInvalidate), this, new object[1] { rc });
		}
		else
		{
			base.Invalidate(rc);
		}
	}

	public new void Invalidate(Region region)
	{
		if (base.InvokeRequired)
		{
			Invoke(new UI.CtrlVoidFunc(UI.CallControlInvalidate), this, new object[1] { region });
		}
		else
		{
			base.Invalidate(region);
		}
	}

	public new void Invalidate(Rectangle rc, bool invalidateChildren)
	{
		if (base.InvokeRequired)
		{
			Invoke(new UI.CtrlVoidFunc(UI.CallControlInvalidate), this, new object[2] { rc, invalidateChildren });
		}
		else
		{
			base.Invalidate(rc, invalidateChildren);
		}
	}

	public new void Invalidate(Region region, bool invalidateChildren)
	{
		if (base.InvokeRequired)
		{
			Invoke(new UI.CtrlVoidFunc(UI.CallControlInvalidate), this, new object[2] { region, invalidateChildren });
		}
		else
		{
			base.Invalidate(region, invalidateChildren);
		}
	}

	public new void Paste()
	{
		if (base.InvokeRequired)
		{
			Invoke(new MethodInvoker(base.Paste));
		}
		else
		{
			base.Paste();
		}
	}

	public new void PerformLayout()
	{
		if (base.InvokeRequired)
		{
			Invoke(new MethodInvoker(base.PerformLayout));
		}
		else
		{
			base.PerformLayout();
		}
	}

	public new void PerformLayout(Control affectedControl, string affectedProperty)
	{
		if (base.InvokeRequired)
		{
			Invoke(new UI.CtrlVoidFunc(UI.CallControlPerformLayout), this, new object[2] { affectedControl, affectedProperty });
		}
		else
		{
			base.PerformLayout(affectedControl, affectedProperty);
		}
	}

	public new Point PointToClient(Point p)
	{
		if (base.InvokeRequired)
		{
			IAsyncResult asyncResult = BeginInvoke(new UI.CtrlRetFunc(UI.CallControlPointToClient), this, new object[1] { p });
			return (Point)EndInvoke(asyncResult);
		}
		return base.PointToClient(p);
	}

	public new Point PointToScreen(Point p)
	{
		if (base.InvokeRequired)
		{
			IAsyncResult asyncResult = BeginInvoke(new UI.CtrlRetFunc(UI.CallControlPointToScreen), this, new object[1] { p });
			return (Point)EndInvoke(asyncResult);
		}
		return base.PointToScreen(p);
	}

	public new virtual bool PreProcessMessage(ref Message msg)
	{
		if (base.InvokeRequired)
		{
			IAsyncResult asyncResult = BeginInvoke(new UI.CtrlRetFunc(UI.CallControlPreProcessMessage), this, new object[1] { msg });
			return (bool)EndInvoke(asyncResult);
		}
		return base.PreProcessMessage(ref msg);
	}

	public new Rectangle RectangleToClient(Rectangle r)
	{
		if (base.InvokeRequired)
		{
			IAsyncResult asyncResult = BeginInvoke(new UI.CtrlRetFunc(UI.CallControlRectangleToClient), this, new object[1] { r });
			return (Rectangle)EndInvoke(asyncResult);
		}
		return base.RectangleToClient(r);
	}

	public new Rectangle RectangleToScreen(Rectangle r)
	{
		if (base.InvokeRequired)
		{
			IAsyncResult asyncResult = BeginInvoke(new UI.CtrlRetFunc(UI.CallControlRectangleToScreen), this, new object[1] { r });
			return (Rectangle)EndInvoke(asyncResult);
		}
		return base.RectangleToScreen(r);
	}

	public new virtual void Refresh()
	{
		if (base.InvokeRequired)
		{
			Invoke(new MethodInvoker(base.Refresh));
		}
		else
		{
			base.Refresh();
		}
	}

	public new virtual void ResetBackColor()
	{
		if (base.InvokeRequired)
		{
			Invoke(new MethodInvoker(base.ResetBackColor));
		}
		else
		{
			base.ResetBackColor();
		}
	}

	public new void ResetBindings()
	{
		if (base.InvokeRequired)
		{
			Invoke(new MethodInvoker(base.ResetBindings));
		}
		else
		{
			base.ResetBindings();
		}
	}

	public new virtual void ResetCursor()
	{
		if (base.InvokeRequired)
		{
			Invoke(new MethodInvoker(base.ResetCursor));
		}
		else
		{
			base.ResetCursor();
		}
	}

	public new virtual void ResetFont()
	{
		if (base.InvokeRequired)
		{
			Invoke(new MethodInvoker(base.ResetFont));
		}
		else
		{
			base.ResetFont();
		}
	}

	public new virtual void ResetForeColor()
	{
		if (base.InvokeRequired)
		{
			Invoke(new MethodInvoker(base.ResetForeColor));
		}
		else
		{
			base.ResetForeColor();
		}
	}

	public new void ResetImeMode()
	{
		if (base.InvokeRequired)
		{
			Invoke(new MethodInvoker(base.ResetImeMode));
		}
		else
		{
			base.ResetImeMode();
		}
	}

	public new virtual void ResetRightToLeft()
	{
		if (base.InvokeRequired)
		{
			Invoke(new MethodInvoker(base.ResetRightToLeft));
		}
		else
		{
			base.ResetRightToLeft();
		}
	}

	public new virtual void ResetText()
	{
		if (base.InvokeRequired)
		{
			Invoke(new MethodInvoker(base.ResetText));
		}
		else
		{
			base.ResetText();
		}
	}

	public new void ResumeLayout()
	{
		if (base.InvokeRequired)
		{
			Invoke(new MethodInvoker(base.ResumeLayout));
		}
		else
		{
			base.ResumeLayout();
		}
	}

	public new void ResumeLayout(bool performLayout)
	{
		if (base.InvokeRequired)
		{
			Invoke(new UI.CtrlVoidFunc(UI.CallControlResumeLayout), this, new object[1] { performLayout });
		}
		else
		{
			base.ResumeLayout(performLayout);
		}
	}

	public new void Scale(SizeF ratio)
	{
		if (base.InvokeRequired)
		{
			Invoke(new UI.CtrlVoidFunc(UI.CallControlScale), this, new object[1] { ratio });
		}
		else
		{
			base.Scale(ratio);
		}
	}

	public new void ScrollToCaret()
	{
		if (base.InvokeRequired)
		{
			Invoke(new MethodInvoker(base.ScrollToCaret));
		}
		else
		{
			base.ScrollToCaret();
		}
	}

	public new void Select(int start, int length)
	{
		if (base.InvokeRequired)
		{
			Invoke(new UI.CtrlVoidFunc(UI.CallTextBoxBaseSelect), this, new object[2] { start, length });
		}
		else
		{
			base.Select(start, length);
		}
	}

	public new void Select()
	{
		if (base.InvokeRequired)
		{
			Invoke(new MethodInvoker(base.Select));
		}
		else
		{
			base.Select();
		}
	}

	public new void SelectAll()
	{
		if (base.InvokeRequired)
		{
			Invoke(new MethodInvoker(base.SelectAll));
		}
		else
		{
			base.SelectAll();
		}
	}

	public new bool SelectNextControl(Control ctl, bool forward, bool tabStopOnly, bool nested, bool wrap)
	{
		if (base.InvokeRequired)
		{
			IAsyncResult asyncResult = BeginInvoke(new UI.CtrlRetFunc(UI.CallControlSelectNextControl), this, new object[5] { ctl, forward, tabStopOnly, nested, wrap });
			return (bool)EndInvoke(asyncResult);
		}
		return base.SelectNextControl(ctl, forward, tabStopOnly, nested, wrap);
	}

	public new void SendToBack()
	{
		if (base.InvokeRequired)
		{
			Invoke(new MethodInvoker(base.SendToBack));
		}
		else
		{
			base.SendToBack();
		}
	}

	public new void SetBounds(int x, int y, int width, int height)
	{
		if (base.InvokeRequired)
		{
			Invoke(new UI.CtrlVoidFunc(UI.CallControlSetBounds), this, new object[4] { x, y, width, height });
		}
		else
		{
			base.SetBounds(x, y, width, height);
		}
	}

	public new void SetBounds(int x, int y, int width, int height, BoundsSpecified specified)
	{
		if (base.InvokeRequired)
		{
			Invoke(new UI.CtrlVoidFunc(UI.CallControlSetBounds), this, new object[5] { x, y, width, height, specified });
		}
		else
		{
			base.SetBounds(x, y, width, height, specified);
		}
	}

	public new void Show()
	{
		if (base.InvokeRequired)
		{
			Invoke(new MethodInvoker(base.Show));
		}
		else
		{
			base.Show();
		}
	}

	public new void SuspendLayout()
	{
		if (base.InvokeRequired)
		{
			Invoke(new MethodInvoker(base.SuspendLayout));
		}
		else
		{
			base.SuspendLayout();
		}
	}

	public new virtual string ToString()
	{
		if (base.InvokeRequired)
		{
			IAsyncResult asyncResult = BeginInvoke(new UI.CtrlRetFunc(UI.CallObjectToString), this, new object[0]);
			return (string)EndInvoke(asyncResult);
		}
		return base.ToString();
	}

	public new void Undo()
	{
		if (base.InvokeRequired)
		{
			Invoke(new MethodInvoker(base.Undo));
		}
		else
		{
			base.Undo();
		}
	}

	public new void Update()
	{
		if (base.InvokeRequired)
		{
			Invoke(new MethodInvoker(base.Update));
		}
		else
		{
			base.Update();
		}
	}
}
