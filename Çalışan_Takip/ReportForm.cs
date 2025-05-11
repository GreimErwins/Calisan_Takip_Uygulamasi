using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Çalışan_Takip
{
    public partial class ReportForm : Form
    {
        public ReportForm(List<Employee> employees)
        {
            InitializeComponent();
            this.Text = "Rapor";
            this.Size = new System.Drawing.Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;

            int toplam = employees.Count;
            var departmanlar = employees.GroupBy(e => e.Departman)
                .Select(g => $"{g.Key}: {g.Count()} kişi").ToList();

            Label lblToplam = new Label { Text = $"Toplam Çalışan: {toplam}", Left = 20, Top = 20, Width = 300 };
            ListBox lbDepartman = new ListBox { Left = 20, Top = 60, Width = 300, Height = 150 };
            lbDepartman.Items.AddRange(departmanlar.ToArray());

            this.Controls.Add(lblToplam);
            this.Controls.Add(lbDepartman);
        }
    }
} 