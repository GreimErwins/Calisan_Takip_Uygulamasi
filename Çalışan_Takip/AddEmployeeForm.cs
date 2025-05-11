using System;
using System.Windows.Forms;

namespace Çalışan_Takip
{
    public partial class AddEmployeeForm : Form
    {
        public Employee Employee { get; private set; }
        private TextBox txtAd, txtSoyad, txtDepartman, txtPozisyon;
        public AddEmployeeForm()
        {
            InitializeComponent();
            this.Text = "Yeni Çalışan Ekle";
            this.Size = new System.Drawing.Size(400, 300);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            Label lblAd = new Label { Text = "Ad", Left = 20, Top = 20, Width = 100 };
            txtAd = new TextBox { Left = 130, Top = 20, Width = 200 };
            Label lblSoyad = new Label { Text = "Soyad", Left = 20, Top = 60, Width = 100 };
            txtSoyad = new TextBox { Left = 130, Top = 60, Width = 200 };
            Label lblDepartman = new Label { Text = "Departman", Left = 20, Top = 100, Width = 100 };
            txtDepartman = new TextBox { Left = 130, Top = 100, Width = 200 };
            Label lblPozisyon = new Label { Text = "Pozisyon", Left = 20, Top = 140, Width = 100 };
            txtPozisyon = new TextBox { Left = 130, Top = 140, Width = 200 };

            Button btnKaydet = new Button { Text = "Kaydet", Left = 130, Top = 200, Width = 80 };
            btnKaydet.Click += BtnKaydet_Click;
            Button btnIptal = new Button { Text = "İptal", Left = 250, Top = 200, Width = 80 };
            btnIptal.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.Add(lblAd);
            this.Controls.Add(txtAd);
            this.Controls.Add(lblSoyad);
            this.Controls.Add(txtSoyad);
            this.Controls.Add(lblDepartman);
            this.Controls.Add(txtDepartman);
            this.Controls.Add(lblPozisyon);
            this.Controls.Add(txtPozisyon);
            this.Controls.Add(btnKaydet);
            this.Controls.Add(btnIptal);
        }

        private void BtnKaydet_Click(object sender, EventArgs e)
        {
            Employee = new Employee
            {
                Ad = txtAd.Text,
                Soyad = txtSoyad.Text,
                Departman = txtDepartman.Text,
                Pozisyon = txtPozisyon.Text
            };
            this.DialogResult = DialogResult.OK;
        }
    }
} 