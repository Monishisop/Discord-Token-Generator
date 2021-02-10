using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AccountCreator
{
    public partial class FormStart : Form
    {
        public bool Start = false;
        public int Delay;
        public int ThreadsAmount;
        public bool VerifyEmail;
        public FormStart()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Start = true;
            ThreadsAmount = (int)numericUpDown1.Value;
            Delay = (int)numericUpDown2.Value;
            VerifyEmail = checkBox3.Checked;
            this.Close();
        }

        private void FormStart_Load(object sender, EventArgs e)
        {

        }
    }
}
