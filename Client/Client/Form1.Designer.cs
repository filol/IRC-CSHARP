namespace Client
{
    partial class Form1
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
            this.chanelList = new System.Windows.Forms.ListBox();
            this.join = new System.Windows.Forms.Button();
            this.create = new System.Windows.Forms.Button();
            this.msgArea = new System.Windows.Forms.TextBox();
            this.button3 = new System.Windows.Forms.Button();
            this.Connect = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.Nick = new System.Windows.Forms.TextBox();
            this.Password = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.ServerHost = new System.Windows.Forms.TextBox();
            this.chatBody = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // chanelList
            // 
            this.chanelList.FormattingEnabled = true;
            this.chanelList.Location = new System.Drawing.Point(9, 36);
            this.chanelList.Margin = new System.Windows.Forms.Padding(2);
            this.chanelList.Name = "chanelList";
            this.chanelList.Size = new System.Drawing.Size(140, 238);
            this.chanelList.TabIndex = 0;
            // 
            // join
            // 
            this.join.Location = new System.Drawing.Point(9, 279);
            this.join.Margin = new System.Windows.Forms.Padding(2);
            this.join.Name = "join";
            this.join.Size = new System.Drawing.Size(139, 19);
            this.join.TabIndex = 1;
            this.join.Text = "Rejoindre";
            this.join.UseVisualStyleBackColor = true;
            this.join.Click += new System.EventHandler(this.join_Click);
            // 
            // create
            // 
            this.create.Location = new System.Drawing.Point(9, 303);
            this.create.Margin = new System.Windows.Forms.Padding(2);
            this.create.Name = "create";
            this.create.Size = new System.Drawing.Size(139, 19);
            this.create.TabIndex = 2;
            this.create.Text = "Creer";
            this.create.UseVisualStyleBackColor = true;
            this.create.Click += new System.EventHandler(this.create_Click);
            // 
            // msgArea
            // 
            this.msgArea.Location = new System.Drawing.Point(175, 303);
            this.msgArea.Margin = new System.Windows.Forms.Padding(2);
            this.msgArea.Name = "msgArea";
            this.msgArea.Size = new System.Drawing.Size(306, 20);
            this.msgArea.TabIndex = 3;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(485, 302);
            this.button3.Margin = new System.Windows.Forms.Padding(2);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(106, 19);
            this.button3.TabIndex = 5;
            this.button3.Text = "Envoyer";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // Connect
            // 
            this.Connect.Location = new System.Drawing.Point(535, 9);
            this.Connect.Margin = new System.Windows.Forms.Padding(2);
            this.Connect.Name = "Connect";
            this.Connect.Size = new System.Drawing.Size(56, 19);
            this.Connect.TabIndex = 6;
            this.Connect.Text = "Connexion";
            this.Connect.UseVisualStyleBackColor = true;
            this.Connect.Click += new System.EventHandler(this.Connect_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(175, 10);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Identifiant";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(346, 12);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Mot de passe";
            // 
            // Nick
            // 
            this.Nick.Location = new System.Drawing.Point(231, 10);
            this.Nick.Margin = new System.Windows.Forms.Padding(2);
            this.Nick.Name = "Nick";
            this.Nick.Size = new System.Drawing.Size(111, 20);
            this.Nick.TabIndex = 9;
            // 
            // Password
            // 
            this.Password.Location = new System.Drawing.Point(420, 10);
            this.Password.Margin = new System.Windows.Forms.Padding(2);
            this.Password.Name = "Password";
            this.Password.Size = new System.Drawing.Size(111, 20);
            this.Password.TabIndex = 10;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 9);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Server";
            // 
            // ServerHost
            // 
            this.ServerHost.Location = new System.Drawing.Point(52, 7);
            this.ServerHost.Margin = new System.Windows.Forms.Padding(2);
            this.ServerHost.Name = "ServerHost";
            this.ServerHost.Size = new System.Drawing.Size(97, 20);
            this.ServerHost.TabIndex = 12;
            // 
            // chatBody
            // 
            this.chatBody.Location = new System.Drawing.Point(175, 36);
            this.chatBody.Margin = new System.Windows.Forms.Padding(2);
            this.chatBody.Name = "chatBody";
            this.chatBody.Size = new System.Drawing.Size(417, 262);
            this.chatBody.TabIndex = 13;
            this.chatBody.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 332);
            this.Controls.Add(this.chatBody);
            this.Controls.Add(this.ServerHost);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Password);
            this.Controls.Add(this.Nick);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Connect);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.msgArea);
            this.Controls.Add(this.create);
            this.Controls.Add(this.join);
            this.Controls.Add(this.chanelList);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox chanelList;
        private System.Windows.Forms.Button join;
        private System.Windows.Forms.Button create;
        private System.Windows.Forms.TextBox msgArea;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button Connect;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox Nick;
        private System.Windows.Forms.TextBox Password;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox ServerHost;
        private System.Windows.Forms.RichTextBox chatBody;
    }
}

