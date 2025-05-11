using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Çalışan_Takip
{
    public partial class EmployeeListForm : Form
    {
        public EmployeeListForm(List<Employee> employees)
        {
            InitializeComponent();
            this.Text = "Çalışan Listesi";
            this.Size = new System.Drawing.Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;

            DataGridView dgv = new DataGridView
            {
                DataSource = employees,
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            this.Controls.Add(dgv);
        }
    }
} 