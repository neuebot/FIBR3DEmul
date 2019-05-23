using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GCodeInterpreter
{
    public partial class Report : Form
    {
        public double print_time = 0.0;
        public double deposited_distance = 0.0;
        public double max_vel = 0.0;
        public double avg_vel = 0.0;

        public Report()
        {
            InitializeComponent();
            
            this.labelPrintTime.Text = String.Format("{0} s", print_time);
            this.labelDepositedFilament.Text = String.Format("{0} m", deposited_distance);
            this.labelMaxVel.Text = String.Format("{0} m/s", max_vel);
            this.labelAvgVel.Text = String.Format("{0} m/s", avg_vel);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        public void ParsedProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBarParsed.Value = e.ProgressPercentage;

            Console.WriteLine("progress: " + e.ProgressPercentage);
        }
    }
}
