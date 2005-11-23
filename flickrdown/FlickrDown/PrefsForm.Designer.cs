namespace FlickrDown
{
    partial class PrefsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.useProxy = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.proxyAddress = new System.Windows.Forms.TextBox();
            this.useAuth = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.username = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.password = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // useProxy
            //
            this.useProxy.AutoSize = true;
            this.useProxy.Location = new System.Drawing.Point(13, 13);
            this.useProxy.Name = "useProxy";
            this.useProxy.Size = new System.Drawing.Size(70, 17);
            this.useProxy.TabIndex = 0;
            this.useProxy.Text = "Use Proxy";
            //
            // label1
            //
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Proxy Address";
            //
            // proxyAddress
            //
            this.proxyAddress.Location = new System.Drawing.Point(13, 54);
            this.proxyAddress.Name = "proxyAddress";
            this.proxyAddress.Size = new System.Drawing.Size(267, 20);
            this.proxyAddress.TabIndex = 2;
            //
            // useAuth
            //
            this.useAuth.AutoSize = true;
            this.useAuth.Location = new System.Drawing.Point(14, 81);
            this.useAuth.Name = "useAuth";
            this.useAuth.Size = new System.Drawing.Size(112, 17);
            this.useAuth.TabIndex = 3;
            this.useAuth.Text = "Use Authentication";
            //
            // label2
            //
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 105);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Username";
            //
            // username
            //
            this.username.Location = new System.Drawing.Point(14, 122);
            this.username.Name = "username";
            this.username.Size = new System.Drawing.Size(266, 20);
            this.username.TabIndex = 5;
            //
            // label3
            //
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 149);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Password";
            //
            // password
            //
            this.password.Location = new System.Drawing.Point(13, 166);
            this.password.Name = "password";
            this.password.PasswordChar = 'Åú';
            this.password.Size = new System.Drawing.Size(267, 20);
            this.password.TabIndex = 7;
            this.password.UseSystemPasswordChar = true;
            //
            // button1
            //
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(204, 231);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 8;
            this.button1.Text = "Okay";
            //
            // button2
            //
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(123, 231);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 9;
            this.button2.Text = "Cancel";
            //
            // PrefsForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.password);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.username);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.useAuth);
            this.Controls.Add(this.proxyAddress);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.useProxy);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "PrefsForm";
            this.Text = "FlickrDown Preferences";
            this.Icon = FlickrDown.Properties.Resources.icon;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox useProxy;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox proxyAddress;
        private System.Windows.Forms.CheckBox useAuth;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox username;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox password;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}