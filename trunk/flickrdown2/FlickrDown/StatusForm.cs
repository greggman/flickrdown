using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FlickrDown
{
    public partial class StatusForm : Form
    {
        private BackgroundWorker _bgw;
        private bool bFinished = false;

        public StatusForm(BackgroundWorker bgw, List<DLPhoto> dlPhotos)
        {
            object[] photos = new object[dlPhotos.Count];

            for (int ii = 0; ii < dlPhotos.Count; ii++)
            {
                photos[ii] = dlPhotos[ii];
            }

            _bgw = bgw;
            InitializeComponent();
            todoListBox.Items.AddRange(photos);
            progressBar.Minimum = 0;
            progressBar.Maximum = todoListBox.Items.Count;
            todoListBox.SelectedIndex = 0;
            todoListBox.TopIndex = 0;

        }

        public void CheckOff(int ndx)
        {
            progressBar.Value = ndx;
            todoListBox.SetItemCheckState (ndx, CheckState.Checked);
            ndx++;
            if (ndx < todoListBox.Items.Count)
            {
                todoListBox.SelectedIndex = ndx;
                if (ndx > 2)
                {
                    todoListBox.TopIndex = ndx - 2;
                }
            }
            else
            {
                bFinished = true;
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            _bgw.CancelAsync();
            this.Text = "Cancelling...";
            cancelButton.Enabled = false;
        }

        private void StatusForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!bFinished)
            {
                cancelButton_Click(sender, new EventArgs());
                e.Cancel = true;
            }
        }
    }
}