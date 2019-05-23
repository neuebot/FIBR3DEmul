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
    public partial class GoToLineForm : Form
    {
        int max_line_ = 0;
        int selected_line_ = 0;
        bool success_ = false;

        public int search_line
        {
            get
            {
                return selected_line_;
            }

            set
            {
                selected_line_ = value;
            }
        }

        public bool success
        {
            get
            {
                return success_;
            }

            set
            {
                success_ = value;
            }
        }

        public GoToLineForm(int max_line)
        {
            InitializeComponent();

            max_line_ = max_line;
            label_line.Text = string.Format("Line number(1 - {0})", max_line);
            numericUpDown.Minimum = 1;
            numericUpDown.Maximum = max_line;

            //Sets focus to numericUpDown
            ActiveControl = numericUpDown;
            numericUpDown.Select(0,1);
        }

        private void bOK_Click(object sender, EventArgs e)
        {
            int read_value = Convert.ToInt32(numericUpDown.Value);

            //Truncate
            if (read_value < 1)
                read_value = 1;
            else if (read_value > max_line_)
                read_value = max_line_;

            search_line = read_value;

            success = true;

            Close();
        }

        private void bCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
