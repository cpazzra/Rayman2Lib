using Modern.Forms;
namespace BNKReader
{
    partial class BNMReaderMainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BNMReaderMainForm));
            this.label1 = new Label();
            this.decodeButton = new Button();
            this.addIndexCheckBox = new CheckBox();
            this.openFileDialog1 = new OpenFileDialog();
            // this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
            | AnchorStyles.Right)));
            this.label1.Location = new System.Drawing.Point(12, 19);
            this.label1.Margin = new Padding(3, 10, 3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(345, 38);
            this.label1.TabIndex = 1;
            this.label1.Text = "This tool extracts all sounds from .bnm files with their original names.";
            this.label1.TextAlign = ContentAlignment.TopCenter;
            // 
            // decodeButton
            // 
            this.decodeButton.Anchor = ((AnchorStyles)(((AnchorStyles.Bottom | AnchorStyles.Left) 
            | AnchorStyles.Right)));
            this.decodeButton.Location = new System.Drawing.Point(12, 63);
            this.decodeButton.Name = "decodeButton";
            this.decodeButton.Size = new System.Drawing.Size(345, 23);
            this.decodeButton.TabIndex = 3;
            this.decodeButton.Text = "Extract";
            this.decodeButton.Click += (sender, args) => this.decodeButton_Click(sender, args);
            // 
            // addIndexCheckBox
            // 
            this.addIndexCheckBox.Anchor = ((AnchorStyles)(((AnchorStyles.Bottom | AnchorStyles.Left) 
            | AnchorStyles.Right)));
            this.addIndexCheckBox.AutoSize = true;
            this.addIndexCheckBox.Location = new System.Drawing.Point(12, 92);
            this.addIndexCheckBox.Name = "addIndexCheckBox";
            this.addIndexCheckBox.Size = new System.Drawing.Size(130, 17);
            this.addIndexCheckBox.TabIndex = 4;
            this.addIndexCheckBox.Text = "Add index to file name";
            // 
            // openFileDialog1
            // 
            // this.openFileDialog1.Filter = "Sound bank|*.bnm";
            // 
            // Form1
            // 
            // this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            // this.AutoScaleMode = AutoScaleMode.Font;
            // this.ClientSize = new System.Drawing.Size(369, 121);
            this.Controls.Add(this.addIndexCheckBox);
            this.Controls.Add(this.decodeButton);
            this.Controls.Add(this.label1);
            // this.FormBorderStyle = FormBorderStyle.FixedSingle;
            // this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            // this.Name = "Form1";
            this.Text = "Sound Bank Extractor";
            // this.Load += new System.EventHandler(this.Form1_Load);
            // this.ResumeLayout(false);
            // this.PerformLayout();

            this.repackButton = new Button();
            this.folderBrowserDialog1 = new FolderBrowserDialog();
            this.saveFileDialog1 = new SaveFileDialog();

            // repackButton
            this.repackButton.Anchor = ((AnchorStyles)(((AnchorStyles.Bottom | AnchorStyles.Left) | AnchorStyles.Right)));
            this.repackButton.Location = new System.Drawing.Point(12, 120); // Positioned below the checkbox
            this.repackButton.Name = "repackButton";
            this.repackButton.Size = new System.Drawing.Size(345, 23);
            this.repackButton.TabIndex = 5;
            this.repackButton.Text = "Repack .bnm";
            this.repackButton.Click += (sender, args) => this.repackButton_Click(sender, args);

            // saveFileDialog1
            this.saveFileDialog1.AddFilter("Sound bank",".bnm");

            this.Controls.Add(this.repackButton);
            // Adjust Form ClientSize to accommodate the new button
            this.Size = new System.Drawing.Size(369, 155); 

        }

        #endregion

        private Label label1;
        private Button decodeButton;
        private CheckBox addIndexCheckBox;
        private OpenFileDialog openFileDialog1;
        private Button repackButton;
        private FolderBrowserDialog folderBrowserDialog1;
        private SaveFileDialog saveFileDialog1;
    }
}

