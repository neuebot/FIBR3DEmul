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
    public partial class Collisions : Form
    {
        private int selected_index_;

        public event CollisionLineSelectedEventHandler CollisionLineSelectedEvent;

        public int selected_index
        {
            get
            {
                return selected_index_;
            }

            set
            {
                selected_index_ = value;
            }
        }

        public Collisions(List<int> collisions)
        {
            InitializeComponent();

            foreach(int line in collisions)
            {
                //Add to GUI listbox
                lbCollisions.Items.Add("Collision at " + (line + 1).ToString());
            }
        }

        public void AddCollision(int collision)
        {
            //Add to GUI listbox
            lbCollisions.Items.Add("Collision at " + (collision + 1).ToString());
            //Scroll to last index
            lbCollisions.SelectedIndex = lbCollisions.Items.Count - 1;
        }

        private void lbCollisions_SelectedIndexChanged(object sender, EventArgs e)
        {
            selected_index = lbCollisions.SelectedIndex;

            CollisionLineSelectedEvent(this, selected_index);
        }
    }
}
