﻿#region header

// MouseJiggler - MainForm.cs
// 
// Created by: Alistair J R Young (avatar) at 2021/01/24 1:57 AM.

#endregion

#region using

using System;
using System.Windows.Forms;

using ArkaneSystems.MouseJiggler.Properties;

#endregion

namespace ArkaneSystems.MouseJiggler
{
    public partial class MainForm : Form
    {
        /// <summary>
        ///     Constructor for use by the form designer.
        /// </summary>
        public MainForm()
            : this(jiggleOnStartup: false, minimizeOnStartup: false, zenJiggleEnabled: false, randomTimer: false, jigglePeriod: 1)
        { }

        public MainForm(bool jiggleOnStartup, bool minimizeOnStartup, bool zenJiggleEnabled, bool randomTimer, int jigglePeriod )
        {
            this.InitializeComponent();

            // Jiggling on startup?
            this.JiggleOnStartup = jiggleOnStartup;

            // Set settings properties
            // We do this by setting the controls, and letting them set the properties.

            this.cbMinimize.Checked = minimizeOnStartup;
            this.cbZen.Checked = zenJiggleEnabled;
            this.tbPeriod.Value = jigglePeriod;
            this.cbRandom.Checked = randomTimer;
        }

        public bool JiggleOnStartup { get; }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (this.JiggleOnStartup)
            {
                this.cbJiggling.Checked = true;
                this.jigglingTrayMenuItem.Checked = true;
            }
        }

        private void UpdateNotificationAreaText()
        {
            if (!this.cbJiggling.Checked)
            {
                this.niTray.Text = "Not jiggling the mouse.";
            }
            else
            {
                string? ww = this.ZenJiggleEnabled ? "with" : "without";
                this.niTray.Text = $"Jiggling mouse every {this.JigglePeriod} s, {ww} Zen.";
            }
        }

        private void cmdAbout_Click(object sender, EventArgs e)
        {
            new AboutBox().ShowDialog(owner: this);
        }

        #region Property synchronization

        private void cbSettings_CheckedChanged(object sender, EventArgs e)
        {
            this.panelSettings.Visible = this.cbSettings.Checked;
        }

        private void cbMinimize_CheckedChanged(object sender, EventArgs e)
        {
            this.MinimizeOnStartup = this.cbMinimize.Checked;
        }

        private void cbZen_CheckedChanged(object sender, EventArgs e)
        {
            this.ZenJiggleEnabled = this.cbZen.Checked;
        }

        private void tbPeriod_ValueChanged(object sender, EventArgs e)
        {
            this.JigglePeriod = this.tbPeriod.Value;
        }

        private void cbRandom_CheckedChanged(object sender, EventArgs e)
        {
            this.RandomTimer = this.cbRandom.Checked;
        }

        #endregion Property synchronization

        #region Do the Jiggle!

        protected bool Zig = true;

        private void cbJiggling_CheckedChanged (object sender, EventArgs e)
        {
            this.JigglePeriod = tbPeriod.Value;
            this.jiggleTimer.Enabled = this.cbJiggling.Checked;
            this.jigglingTrayMenuItem.Checked = this.cbJiggling.Checked;
        }

        private void jiggleTimer_Tick (object sender, EventArgs e)
        {
            if (this.ZenJiggleEnabled)
                Helpers.Jiggle (delta: 0);
            else if (this.Zig)
                Helpers.Jiggle (delta: 4);
            else //zag
                Helpers.Jiggle (delta: -4);

            this.Zig = !this.Zig;

            if (this.RandomTimer)
            {
                this.jigglePeriod = (new Random().Next(1, tbPeriod.Value)) * 1000;
                this.lbRandom.Text = $"{this.jigglePeriod / 1000} s";
                this.jiggleTimer.Interval = this.jigglePeriod;
            }
        }

        #endregion Do the Jiggle!

        #region Minimize and restore

        private void cmdTrayify_Click (object sender, EventArgs e)
        {
            this.MinimizeToTray ();
        }

        private void niTray_DoubleClick (object sender, EventArgs e)
        {
            this.RestoreFromTray ();
        }

        private void MinimizeToTray ()
        {
            this.Visible        = false;
            this.ShowInTaskbar  = false;
            this.niTray.Visible = true;

            this.UpdateNotificationAreaText ();
        }

        private void RestoreFromTray ()
        {
            this.Visible        = true;
            this.ShowInTaskbar  = true;
            this.niTray.Visible = false;
        }

        #endregion Minimize and restore

        #region Settings property backing fields

        private int jigglePeriod;

        private bool minimizeOnStartup;

        private bool zenJiggleEnabled;

        private bool randomTimer;

        #endregion Settings property backing fields

        #region Settings properties

        public bool MinimizeOnStartup
        {
            get => this.minimizeOnStartup;
            set
            {
                this.minimizeOnStartup             = value;
                Settings.Default.MinimizeOnStartup = value;
                Settings.Default.Save ();
            }
        }

        public bool ZenJiggleEnabled
        {
            get => this.zenJiggleEnabled;
            set
            {
                this.zenJiggleEnabled      = value;
                Settings.Default.ZenJiggle = value;
                Settings.Default.Save ();
            }
        }

        public int JigglePeriod
        {
            get => this.jigglePeriod;
            set
            {
                this.jigglePeriod = value;
                Settings.Default.JigglePeriod = value;
                Settings.Default.Save();

                this.jiggleTimer.Interval = value * 1000;
                this.lbPeriod.Text = $"{value} s";
            }
        }

        public bool RandomTimer
        {
            get => this.randomTimer;
            set
            {
                this.randomTimer = value;
                Settings.Default.RandomTimer = value;
                Settings.Default.Save();
            }
        }

        #endregion Settings properties

        #region Minimize on start

        private bool firstShown = true;

        private void MainForm_Shown (object sender, EventArgs e)
        {
            if (this.firstShown && this.MinimizeOnStartup)
                this.MinimizeToTray ();

            this.firstShown = false;
        }

        #endregion

        #region Tray Menu

        private void exitTrayMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void showMouseJigglerMenuItem_Click(object sender, EventArgs e)
        {
            this.RestoreFromTray();
        }

        private void jigglingTrayMenuItem_Click(object sender, EventArgs e)
        {
            cbJiggling.Checked = this.jigglingTrayMenuItem.Checked;
            this.UpdateNotificationAreaText();
        }
        private void niTray_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                System.Reflection.MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                mi.Invoke(niTray, null);
            }
        }

        #endregion Tray Menu

    }
}
