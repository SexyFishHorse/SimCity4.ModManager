namespace SimCity4.ModManager.App.Application.View
{
    partial class ChangelogForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChangelogForm));
            this.changelogWebBrowser = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // changelogWebBrowser
            // 
            this.changelogWebBrowser.AllowNavigation = false;
            this.changelogWebBrowser.AllowWebBrowserDrop = false;
            this.changelogWebBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.changelogWebBrowser.IsWebBrowserContextMenuEnabled = false;
            this.changelogWebBrowser.Location = new System.Drawing.Point(0, 0);
            this.changelogWebBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.changelogWebBrowser.Name = "changelogWebBrowser";
            this.changelogWebBrowser.ScriptErrorsSuppressed = true;
            this.changelogWebBrowser.Size = new System.Drawing.Size(784, 561);
            this.changelogWebBrowser.TabIndex = 0;
            this.changelogWebBrowser.Url = new System.Uri("https://github.com/IrradiatedGames/SimCity-4-Buddy-Project/wiki/Changelog", System.UriKind.Absolute);
            // 
            // ChangelogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.changelogWebBrowser);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ChangelogForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Changelog";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser changelogWebBrowser;
    }
}