namespace SimCity4.ModManager.App.Plugins.View
{
    partial class RemoveUnnecessaryFilesForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.fileTypesListView = new System.Windows.Forms.ListView();
            this.fileTypeNameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.descriptiveNameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.fileTypeDescriptionHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.numberOfFilesHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.removeSelectedButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(266, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Check what filetypes you want removed from this folder";
            // 
            // fileTypesListView
            // 
            this.fileTypesListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fileTypesListView.CheckBoxes = true;
            this.fileTypesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.descriptiveNameHeader,
            this.fileTypeNameHeader,
            this.numberOfFilesHeader,
            this.fileTypeDescriptionHeader});
            this.fileTypesListView.Location = new System.Drawing.Point(12, 25);
            this.fileTypesListView.Name = "fileTypesListView";
            this.fileTypesListView.Size = new System.Drawing.Size(468, 262);
            this.fileTypesListView.TabIndex = 1;
            this.fileTypesListView.UseCompatibleStateImageBehavior = false;
            this.fileTypesListView.View = System.Windows.Forms.View.Details;
            // 
            // fileTypeNameHeader
            // 
            this.fileTypeNameHeader.Text = "File extension";
            // 
            // descriptiveNameHeader
            // 
            this.descriptiveNameHeader.Text = "Type";
            // 
            // fileTypeDescriptionHeader
            // 
            this.fileTypeDescriptionHeader.Text = "Description";
            this.fileTypeDescriptionHeader.Width = 208;
            // 
            // numberOfFilesHeader
            // 
            this.numberOfFilesHeader.Text = "Number of files";
            this.numberOfFilesHeader.Width = 118;
            // 
            // removeSelectedButton
            // 
            this.removeSelectedButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.removeSelectedButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.removeSelectedButton.Location = new System.Drawing.Point(289, 293);
            this.removeSelectedButton.Name = "removeSelectedButton";
            this.removeSelectedButton.Size = new System.Drawing.Size(110, 23);
            this.removeSelectedButton.TabIndex = 2;
            this.removeSelectedButton.Text = "Remove selected";
            this.removeSelectedButton.UseVisualStyleBackColor = true;
            this.removeSelectedButton.Click += new System.EventHandler(this.RemoveSelectedButtonClick);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(405, 293);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // RemoveUnnecessaryFilesForm
            // 
            this.AcceptButton = this.removeSelectedButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(492, 328);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.removeSelectedButton);
            this.Controls.Add(this.fileTypesListView);
            this.Controls.Add(this.label1);
            this.Name = "RemoveUnnecessaryFilesForm";
            this.Text = "RemoveUnnecessaryFilesForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView fileTypesListView;
        private System.Windows.Forms.ColumnHeader fileTypeNameHeader;
        private System.Windows.Forms.ColumnHeader fileTypeDescriptionHeader;
        private System.Windows.Forms.ColumnHeader numberOfFilesHeader;
        private System.Windows.Forms.Button removeSelectedButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.ColumnHeader descriptiveNameHeader;
    }
}