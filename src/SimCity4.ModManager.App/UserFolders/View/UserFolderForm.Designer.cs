namespace SimCity4.ModManager.App.UserFolders.View
{
    partial class UserFolderForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserFolderForm));
            this.managePluginsButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.sizeOfPluginsLabel = new System.Windows.Forms.Label();
            this.numberOfPluginsLabel = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // managePluginsButton
            // 
            this.managePluginsButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.managePluginsButton.Location = new System.Drawing.Point(45, 20);
            this.managePluginsButton.Name = "managePluginsButton";
            this.managePluginsButton.Size = new System.Drawing.Size(100, 100);
            this.managePluginsButton.TabIndex = 0;
            this.managePluginsButton.Text = "Manage plugins";
            this.managePluginsButton.UseVisualStyleBackColor = true;
            this.managePluginsButton.Click += new System.EventHandler(this.ManagePluginsButtonClick);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.panel2, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel3, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel4, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(392, 197);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // panel2
            // 
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(199, 53);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(190, 141);
            this.panel2.TabIndex = 3;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.label2);
            this.panel3.Controls.Add(this.label1);
            this.panel3.Controls.Add(this.sizeOfPluginsLabel);
            this.panel3.Controls.Add(this.numberOfPluginsLabel);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(3, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(190, 44);
            this.panel3.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Size of plugin folder";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Known plugins";
            // 
            // sizeOfPluginsLabel
            // 
            this.sizeOfPluginsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.sizeOfPluginsLabel.Location = new System.Drawing.Point(127, 19);
            this.sizeOfPluginsLabel.Name = "sizeOfPluginsLabel";
            this.sizeOfPluginsLabel.Size = new System.Drawing.Size(60, 13);
            this.sizeOfPluginsLabel.TabIndex = 3;
            this.sizeOfPluginsLabel.Text = "[X GB]";
            this.sizeOfPluginsLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // numberOfPluginsLabel
            // 
            this.numberOfPluginsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numberOfPluginsLabel.Location = new System.Drawing.Point(127, 6);
            this.numberOfPluginsLabel.Name = "numberOfPluginsLabel";
            this.numberOfPluginsLabel.Size = new System.Drawing.Size(60, 13);
            this.numberOfPluginsLabel.TabIndex = 1;
            this.numberOfPluginsLabel.Text = "[X]";
            this.numberOfPluginsLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // panel4
            // 
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(199, 3);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(190, 44);
            this.panel4.TabIndex = 5;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.managePluginsButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 53);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(190, 141);
            this.panel1.TabIndex = 6;
            // 
            // UserFolderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(392, 197);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "UserFolderForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "User Folder Management";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UserFolderFormFormClosing);
            this.Load += new System.EventHandler(this.UserFolderFormLoad);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button managePluginsButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label sizeOfPluginsLabel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label numberOfPluginsLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel1;

    }
}