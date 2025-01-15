using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;

namespace LogDisplayDemo
{
    public partial class MainForm : Form
    {
        readonly LogControl logger;

        public MainForm()
        {
            InitializeComponent();

            this.Icon = new Icon(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "AppIcon.ico"));
            this.Text = "Log Control Demo";
            this.Load += MainFormOnLoad;

            // Create our local Font object
            PrivateFontCollection pfc = new PrivateFontCollection();
            pfc.AddFontFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "UMR.ttf"));
            Font customFont = new Font(pfc.Families[0], 12f, FontStyle.Regular, GraphicsUnit.Point);
            logger = new LogControl() 
            {
                Dock = DockStyle.Fill,
                WordWrap = false,
                DarkMode = true,
                Font = customFont
            };
            logger.ControlToInvoke = this; // assign SynchronizationContext for logger control

            #region [Add controls to form]
            var button = new Button() 
            {
                Dock = DockStyle.Bottom,
                Height = 32,
                Padding = new Padding(0),
                Text = "&Save Log"
            };
            button.Click += (s, e) => 
            {
                logger.AddText("Opening dialog for saving log contents", Messagetype.Information);
                logger.SaveLog();

                #region [Skip SaveFileDialog]
                // This is unnecessary, but it simulates some amount of time for the
                // process to finish so the button state can be used as an indicator
                // for the process. This technique can also be used to prevent button
                // spamming from the user.
                //logger.AddText("Saving log contents", Messagetype.Information);
                //Task.Run(async () =>
                //{
                //    ToggleEnabled(button, false);
                //    BeginInvoke(new Action(() => { logger.SaveFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RichTextLog.rtf")); }));
                //    await Task.Delay(500);
                //})
                //.ContinueWith((t) =>
                //{
                //    ToggleEnabled(button, true);
                //});
                #endregion
            };

            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterWidth = 8
            };

            var picBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = Image.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "AppIcon.png")),
            };
            picBox.MouseUp += (s, e) => { logger.AddText($"MouseUp PictureBox Event", Messagetype.Information); };
            picBox.MouseDown += (s, e) => { logger.AddText($"MouseDown PictureBox Event", Messagetype.Information); };

            splitContainer.SplitterWidth = 8;
            splitContainer.SplitterDistance = 36;
            splitContainer.Panel1.Controls.Add(picBox);
            splitContainer.Panel1.Controls.Add(button);
            splitContainer.Panel2.Controls.Add(logger);

            this.Controls.Add(splitContainer);
            #endregion
        }

        /// <summary>
        /// Form event
        /// </summary>
        void MainFormOnLoad(object sender, EventArgs e)
        {
            logger.AddText("A reusable, color-coded, text box logging control.", Messagetype.Information);
            logger.AddText("[DEBUG] Inside OnLoad Event", Messagetype.Debug);
            logger.AddText("This is a fake warning", Messagetype.Warning);
            logger.AddText("This is a fake error", Messagetype.Error);
            logger.AddText("Message from terminal", Messagetype.FromTerminal);
            logger.AddText("Message to terminal", Messagetype.ToTerminal);
        }

        #region [Control helpers]
        /// <summary>
        /// Thread-safe method
        /// </summary>
        /// <param name="ctrl"><see cref="Control"/></param>
        /// <param name="state">true=enabled, false=disabled</param>
        public void ToggleEnabled(Control ctrl, bool state)
        {
            if (InvokeRequired)
                BeginInvoke(new Action(() => ToggleEnabled(ctrl, state)));
            else
                ctrl.Enabled = state;
        }

        /// <summary>
        /// Thread-safe method
        /// </summary>
        /// <param name="ctrl"><see cref="Control"/></param>
        /// <param name="state">true=visible, false=invisible</param>
        public void ToggleVisible(Control ctrl, bool state)
        {
            if (InvokeRequired)
                BeginInvoke(new Action(() => ToggleVisible(ctrl, state)));
            else
                ctrl.Visible = state;
        }

        /// <summary>
        /// Thread-safe method
        /// </summary>
        /// <param name="ctrl"><see cref="Control"/></param>
        public void SetFocus(Control ctrl)
        {
            if (InvokeRequired)
                BeginInvoke(new Action(() => SetFocus(ctrl)));
            else
                ctrl.Focus();
        }

        /// <summary>
        /// Thread-safe method
        /// </summary>
        /// <param name="ctrl"><see cref="Control"/></param>
        /// <param name="text">string data</param>
        public void SetText(Control ctrl, string text)
        {
            if (InvokeRequired)
                BeginInvoke(new Action(() => SetText(ctrl, text)));
            else
                ctrl.Text = text;
        }
        #endregion
    }
}
