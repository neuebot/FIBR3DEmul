namespace GCodeInterpreter
{
    partial class Report
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
            this.progressBarParsed = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.labelPrintTime = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.labelDepositedFilament = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.labelMaxVel = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.labelAvgVel = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // progressBarParsed
            // 
            this.progressBarParsed.Location = new System.Drawing.Point(175, 198);
            this.progressBarParsed.Name = "progressBarParsed";
            this.progressBarParsed.Size = new System.Drawing.Size(122, 23);
            this.progressBarParsed.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(254, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "Print Report (work in progress)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(17, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "Print Time";
            // 
            // labelPrintTime
            // 
            this.labelPrintTime.AutoSize = true;
            this.labelPrintTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPrintTime.Location = new System.Drawing.Point(252, 66);
            this.labelPrintTime.Name = "labelPrintTime";
            this.labelPrintTime.Size = new System.Drawing.Size(0, 16);
            this.labelPrintTime.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(17, 93);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(125, 16);
            this.label4.TabIndex = 4;
            this.label4.Text = "Deposited Filament";
            // 
            // labelDepositedFilament
            // 
            this.labelDepositedFilament.AutoSize = true;
            this.labelDepositedFilament.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDepositedFilament.Location = new System.Drawing.Point(252, 93);
            this.labelDepositedFilament.Name = "labelDepositedFilament";
            this.labelDepositedFilament.Size = new System.Drawing.Size(0, 16);
            this.labelDepositedFilament.TabIndex = 5;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(17, 120);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(116, 16);
            this.label6.TabIndex = 6;
            this.label6.Text = "Maximum Velocity";
            // 
            // labelMaxVel
            // 
            this.labelMaxVel.AutoSize = true;
            this.labelMaxVel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMaxVel.Location = new System.Drawing.Point(252, 120);
            this.labelMaxVel.Name = "labelMaxVel";
            this.labelMaxVel.Size = new System.Drawing.Size(0, 16);
            this.labelMaxVel.TabIndex = 7;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(17, 147);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(111, 16);
            this.label8.TabIndex = 8;
            this.label8.Text = "Average Velocity";
            // 
            // labelAvgVel
            // 
            this.labelAvgVel.AutoSize = true;
            this.labelAvgVel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAvgVel.Location = new System.Drawing.Point(252, 147);
            this.labelAvgVel.Name = "labelAvgVel";
            this.labelAvgVel.Size = new System.Drawing.Size(0, 16);
            this.labelAvgVel.TabIndex = 9;
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(17, 200);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(125, 21);
            this.okButton.TabIndex = 10;
            this.okButton.Text = "&OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // Report
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(309, 233);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.labelAvgVel);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.labelMaxVel);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.labelDepositedFilament);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.labelPrintTime);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.progressBarParsed);
            this.Name = "Report";
            this.Text = "Report";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBarParsed;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label labelPrintTime;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label labelDepositedFilament;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label labelMaxVel;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label labelAvgVel;
        private System.Windows.Forms.Button okButton;
    }
}