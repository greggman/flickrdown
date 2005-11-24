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

        public bool UseProxy { get { return this.useProxy.Checked; } set { this.useProxy.Checked = value; } }
        public string ProxyAddress { get { return this.proxyAddress.Text; } set { this.proxyAddress.Text = value; } }
        public bool UseProxyAuth { get { return this.useAuth.Checked; } set { this.useAuth.Checked = value; } }
        public string ProxyUsername { get { return this.username.Text; } set { this.username.Text = value; } }
        public string ProxyPassword { get { return this.password.Text; } set { this.password.Text = value; } }
    }
}