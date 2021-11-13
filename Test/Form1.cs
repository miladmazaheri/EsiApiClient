using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test
{
    public partial class Form1 : Form
    {
        private readonly bool _isEditMode;
        public Form1(bool isEditMode = false)
        {
            _isEditMode = isEditMode;

            InitializeComponent();

            //هم میتونی اینجا بنویسی
            btnEdit.Visible = _isEditMode;

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //هم میتونی اینجا بنویسی 
            btnEdit.Visible = _isEditMode;
        }
    }


    public class Form2
    {
        public void ShowForm1()
        {
            //در حالت جدید
            var frm1InNewMode = new Form1(false);
            frm1InNewMode.ShowDialog();
            // در حالت ویرایش
            var frm2InNewMode = new Form1(true);
            frm2InNewMode.ShowDialog();
        }
    }
}
