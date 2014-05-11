using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DSNStatus;

namespace DSNStatusTester
{
    public partial class frmTester : Form
    {
        public frmTester()
        {
            InitializeComponent();
        }

        private void frmTester_Load(object sender, EventArgs e)
        {
            timePicker.Value = DateTime.Now;
        }

        private void btnSpecified_Click(object sender, EventArgs e)
        {
            getDSNStatus(timePicker.Value);
        }

        private void btnNow_Click(object sender, EventArgs e)
        {
            getDSNStatus(DateTime.Now);
        }

        private void getDSNStatus(DateTime dateTime)
        {
            var res = DSNPoller.GetStatus(dateTime);
            string here = "break";
        }
    }
}
