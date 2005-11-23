using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FlickrDown
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();

            this.label3.Text = Util.GetVersion();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://greggman.com/");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "could not go to website\n" + ex.Message);
            }
        }
    }
}