using System.Windows.Forms;

namespace Thetis;

public static class Prompt
{
	public static string ShowDialog(string text, string caption)
	{
		Form prompt = new Form
		{
			Width = 800,
			Height = 150,
			FormBorderStyle = FormBorderStyle.FixedDialog,
			Text = caption,
			StartPosition = FormStartPosition.CenterScreen
		};
		Label value = new Label
		{
			Left = 50,
			Top = 20,
			Text = text
		};
		TextBox textBox = new TextBox
		{
			Left = 50,
			Top = 50,
			Width = 700
		};
		Button button = new Button
		{
			Text = "Ok",
			Left = 350,
			Width = 100,
			Top = 70,
			DialogResult = DialogResult.OK
		};
		button.Click += delegate
		{
			prompt.Close();
		};
		prompt.Controls.Add(textBox);
		prompt.Controls.Add(button);
		prompt.Controls.Add(value);
		prompt.AcceptButton = button;
		if (prompt.ShowDialog() != DialogResult.OK)
		{
			return "";
		}
		return textBox.Text;
	}
}
