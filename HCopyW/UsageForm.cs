using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HCopy
{
    public partial class UsageForm : Form
    {
        public UsageForm()
        {
            InitializeComponent();
        }

        private void UsageForm_Load(object sender, EventArgs e)
        {
            MaximumSize = Screen.GetWorkingArea(this).Size;
            textBoxCommandLine.Text = Environment.CommandLine;
            textBoxUsage.Text = Properties.Resources.Usage;
            using (Graphics g = textBoxUsage.CreateGraphics())
            {
                SizeF s2 = g.MeasureString(textBoxUsage.Text, textBoxUsage.Font);
                SizeF s1 = g.MeasureString(textBoxCommandLine.Text, textBoxCommandLine.Font, (int)s2.Width);
                textBoxCommandLine.Height = (int)s1.Height;
                ClientSize = new Size((int)s2.Width + 20, (int)s1.Height + (int)s2.Height + panelBottom.Height);
            }
            textBoxUsage.Select(0, 0);
        }
    }
}
