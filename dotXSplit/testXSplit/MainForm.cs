using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using dotXSplit;

namespace dotXSplit
{
    public partial class MainForm : Form
    {
        private XSplit xsplit;
        public MainForm()
        {
            InitializeComponent();


        }

        private void button1_Click(object sender, EventArgs e)
        {
            xsplit = new XSplit();
            MessageBox.Show( String.Format("{0}",xsplit.Encoded ) );
        }

    }
}
