namespace GCodeInterpreter
{
    partial class Collisions
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
            this.lbCollisions = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // lbCollisions
            // 
            this.lbCollisions.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.lbCollisions.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbCollisions.FormattingEnabled = true;
            this.lbCollisions.ItemHeight = 16;
            this.lbCollisions.Location = new System.Drawing.Point(13, 20);
            this.lbCollisions.Name = "lbCollisions";
            this.lbCollisions.Size = new System.Drawing.Size(155, 212);
            this.lbCollisions.TabIndex = 0;
            this.lbCollisions.SelectedIndexChanged += new System.EventHandler(this.lbCollisions_SelectedIndexChanged);
            // 
            // Collisions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(176, 240);
            this.Controls.Add(this.lbCollisions);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Collisions";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Collisions";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lbCollisions;
    }
}