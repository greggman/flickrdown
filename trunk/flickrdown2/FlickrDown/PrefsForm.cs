using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FlickrDown
{
    public partial class PrefsForm : Form
    {
        public PrefsForm()
        {
            InitializeComponent();
        }

        private int _downloadLimit;

        public bool UseProxy { get { return this.useProxy.Checked; } set { this.useProxy.Checked = value; } }
        public string ProxyAddress { get { return this.proxyAddress.Text; } set { this.proxyAddress.Text = value; } }
        public bool UseProxyAuth { get { return this.useAuth.Checked; } set { this.useAuth.Checked = value; } }
        public string ProxyUsername { get { return this.username.Text; } set { this.username.Text = value; } }
        public string ProxyPassword { get { return this.password.Text; } set { this.password.Text = value; } }
        public int DownloadLimit { get { return _downloadLimit; } set { _downloadLimit = value; this.downloadLimit.Text = value.ToString(); } }

        private void downloadLimit_TextChanged(object sender, EventArgs e)
        {
            bool success = false;
            try
            {
                int val = int.Parse(this.downloadLimit.Text);
                if (val > 0)
                {
                    _downloadLimit = val;
                    success = true;
                }
            }
            catch (Exception ex)
            {
                Console.Beep();
            }

            if (!success)
            {
                this.downloadLimit.Text = _downloadLimit.ToString();
            }
        }
    }
}