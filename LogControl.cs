using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace LogDisplayDemo
{
    public enum Messagetype
    {
        Debug,
        Information,
        Warning,
        Error,
        FromTerminal,
        ToTerminal,
    }

    class LogControl : RichTextBox
    {
        public bool DarkMode { set; get; } = true;
        public Control ControlToInvoke { set; get; }

        public LogControl()
        {
            //Font = new Font(new FontFamily("Consolas"), 10, FontStyle.Regular);
            if (DarkMode)
            {
                BackColor = Color.FromArgb(24, 24, 24);
                ForeColor = Color.WhiteSmoke;
            }
            else
            {
                BackColor = Color.White;
                ForeColor = Color.Black;
            }
        }

        public LogControl(bool darkMode) : this()
        {
            DarkMode = darkMode;
        }

        /// <summary>
        /// A <see cref="Control.BeginInvoke(Delegate)"/> alias helper.
        /// </summary>
        public void AddText(string message, Messagetype msgType = Messagetype.Information)
        {
            if (ControlToInvoke == null)
            {
                Debug.WriteLine($"[WARNING] No sync context has been assigned for the UI thread.");
                return;
            }

            ControlToInvoke.BeginInvoke(new ThreadStart(delegate()
            {
                Color oldColor = SelectionColor;
                SelectionColor = GetColor(msgType);
                message = TextWithPrefix(msgType, message);
                if (!Text.EndsWith("\n"))
                    message = "\n" + message;
                AppendText(message);
                ScrollToCaret();
                SelectionColor = oldColor;
                Invalidate();
            }));
        }

        public void AddTextWithParams(string message, object[] args, Messagetype messagetype)
        {
            if (args != null && args.Length > 0)
            {
                message += " With parameters: ";
                foreach (var item in args)
                {
                    message += item == null ? "null" : item.ToString().Replace("\n", "").Replace("\r", "").Replace("  ", "") + "\n";
                }
            }
            AddText(message, messagetype);
        }

        string TextWithPrefix(Messagetype msgType, string message)
        {
            string prefix = DateTime.Now.ToString("hh:mm:ss.fff tt");

            switch (msgType)
            {
                case Messagetype.FromTerminal: prefix += $" ⇨ ";
                    break;
                case Messagetype.ToTerminal: prefix += " ⇦ ";
                    break;
                case Messagetype.Information: prefix += $" ✏️ ";
                    break;
                case Messagetype.Warning: prefix += " ⚠️ ";
                    break;
                case Messagetype.Error: prefix += " ⚠️ ";
                    break;
                default: prefix += " 🔎 ";
                    break;
            }

            return prefix + message + "\n";
        }

        /// <summary>
        /// Saves content via the <see cref="RichTextBox.SaveFile(string, RichTextBoxStreamType)"/>.
        /// </summary>
        internal void SaveLog()
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RichTextLog.rtf");
                sfd.AutoUpgradeEnabled = true;
                sfd.CheckPathExists = true;
                sfd.CheckFileExists = false;
                sfd.AddExtension = true;
                sfd.Title = "Save current log";
                sfd.Filter = "RTF files (*.rtf)|*.rtf|log files (*.log)|*.log|All files (*.*)|*.*";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    this.SaveFile(sfd.FileName, RichTextBoxStreamType.PlainText);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not save file!{Environment.NewLine}{ex.Message}", "SaveLog", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        Color GetColor(Messagetype msgType)
        {
            switch (msgType)
            {
                case Messagetype.FromTerminal:
                    return DarkMode ? Color.SpringGreen : Color.Green;
                case Messagetype.ToTerminal:
                    return DarkMode ? Color.DodgerBlue : Color.Blue;
                case Messagetype.Debug:
                    return DarkMode ? Color.Gray : Color.DimGray;
                case Messagetype.Information:
                    return DarkMode ? Color.WhiteSmoke : Color.Black;
                case Messagetype.Warning:
                    return DarkMode ? Color.Orange : Color.DarkOrange;
                case Messagetype.Error:
                    return DarkMode ? Color.Red : Color.DarkRed;
                default:
                    return DarkMode ? Color.WhiteSmoke : Color.Black;
            }
        }

    }

}
