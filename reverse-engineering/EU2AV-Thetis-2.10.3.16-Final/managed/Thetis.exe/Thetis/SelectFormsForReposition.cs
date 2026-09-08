using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis;

public class SelectFormsForReposition : Form
{
	private class RepositionItem
	{
		public Form form;

		public RepositionItem(Form form)
		{
			this.form = form;
		}

		public override string ToString()
		{
			if (form == null)
			{
				return "";
			}
			string obj = (string.IsNullOrEmpty(form.Name) ? "(unnamed)" : form.Name);
			string text = (string.IsNullOrEmpty(form.Text) ? "" : form.Text);
			return obj + "  —  " + text;
		}
	}

	private CheckedListBox _list_box;

	private Button _ok_button;

	private Button _cancel_button;

	private List<Form> _source_forms;

	private int _start_x;

	private int _start_y;

	private int _step;

	public SelectFormsForReposition(List<Form> forms, int start_x = 100, int start_y = 100, int step = 20)
	{
		_source_forms = new List<Form>(forms);
		_start_x = start_x;
		_start_y = start_y;
		_step = step;
		Text = "Select Forms to Reposition:";
		base.StartPosition = FormStartPosition.CenterParent;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MinimizeBox = false;
		base.MaximizeBox = false;
		base.ClientSize = new Size(420, 360);
		_list_box = new CheckedListBox();
		_list_box.Location = new Point(12, 12);
		_list_box.Size = new Size(396, 276);
		_list_box.CheckOnClick = true;
		for (int i = 0; i < _source_forms.Count; i++)
		{
			Form form = _source_forms[i];
			_list_box.Items.Add(new RepositionItem(form), isChecked: false);
		}
		_ok_button = new Button();
		_ok_button.Text = "OK";
		_ok_button.Size = new Size(90, 28);
		_ok_button.Location = new Point(base.ClientSize.Width - 198, base.ClientSize.Height - 44);
		_ok_button.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		_ok_button.Click += on_ok_clicked;
		_cancel_button = new Button();
		_cancel_button.Text = "Cancel";
		_cancel_button.Size = new Size(90, 28);
		_cancel_button.Location = new Point(base.ClientSize.Width - 96, base.ClientSize.Height - 44);
		_cancel_button.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		_cancel_button.Click += on_cancel_clicked;
		base.AcceptButton = _ok_button;
		base.CancelButton = _cancel_button;
		base.Controls.Add(_list_box);
		base.Controls.Add(_ok_button);
		base.Controls.Add(_cancel_button);
		base.TopMost = true;
	}

	public static List<Form> getAllOpenForms()
	{
		List<Form> list = new List<Form>();
		for (int i = 0; i < Application.OpenForms.Count; i++)
		{
			Form form = Application.OpenForms[i];
			if (!(form.Name == "frmInfoBarPopup") && !(form.Name == "frmQuickRecallPopupList"))
			{
				list.Add(form);
			}
		}
		return list;
	}

	private static void repositionForms(List<Form> forms, int start_x = 100, int start_y = 100, int step = 20)
	{
		int num = start_x;
		int num2 = start_y;
		for (int i = 0; i < forms.Count; i++)
		{
			Form form = forms[i];
			if (form.WindowState != FormWindowState.Normal)
			{
				form.WindowState = FormWindowState.Normal;
			}
			form.Location = new Point(num, num2);
			Common.ForceFormOnScreen(form);
			num += step;
			num2 += step;
		}
	}

	private void on_ok_clicked(object sender, EventArgs e)
	{
		List<Form> list = new List<Form>();
		for (int i = 0; i < _list_box.CheckedItems.Count; i++)
		{
			RepositionItem repositionItem = (RepositionItem)_list_box.CheckedItems[i];
			if (repositionItem.form != null)
			{
				list.Add(repositionItem.form);
			}
		}
		if (list.Count > 0)
		{
			repositionForms(list, _start_x, _start_y, _step);
		}
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void on_cancel_clicked(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
	}
}
