using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Çalışan_Takip;
using QRCoder;
using Font = System.Drawing.Font;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Çalışan_Takip
{
    public partial class Form1 : Form
    {
        private List<Employee> employees = new List<Employee>();
        private DataGridView dgv;
        private Label lblRapor;
        private Panel addPanel;
        private TextBox txtAd, txtSoyad, txtDepartman, txtPozisyon, txtAra, txtTC, txtTelefon, txtMaas;
        private Button btnSil, btnDuzenle, btnKaydet, btnYukle, btnPdf;
        private Button btnIzinEkle, btnIzinListe;
        private int? editingIndex = null;
        private string dataFile = "calisanlar.json";
        private Timer welcomeFadeTimer;
        private int welcomeAlpha = 0;
        private Label welcomeLabel;
        private Panel contentPanel;
        private int highlightRowIndex = -1;
        private Timer highlightTimer;
        private Timer rotationTimer;
        private float rotationAngle = 0;
        private RotatingTextLabel rotatingLabel;
        private Timer semiCircleTimer;
        private SemiCircleMarqueeLabel semiCircleLabel;
        private Label typewriterLabel;
        private Timer typewriterTimer;
        private string typewriterFullText = "Çalışan Takip Sistemine Hoş Geldiniz";
        private int typewriterIndex = 0;
        private Timer dgvFlashTimer;
        private int flashRowIndex = -1;
        private FlowLayoutPanel searchPanel;
        private Button btnExcel;
        private ContextMenuStrip dgvContextMenu;
        private Panel dashboardPanel;
        private Label lblTotalEmployees, lblIzinliEmployees;
        private Timer dashboardAnimTimer;
        private int dashboardAnimStep = 0, dashboardTotalTarget = 0, dashboardIzinliTarget = 0;
        private ToolStripMenuItem temaMenu, lightModeMenuItem, darkModeMenuItem;
        private string currentTheme = "Light";
        private Panel panelTotalEmployees, panelIzinliEmployees;
        private Timer toastTimer;
        private Label toastLabel;

        public Form1()
        {
            InitializeComponent();
            this.MinimumSize = new Size(900, 600);
            this.WindowState = FormWindowState.Maximized;
            InitializeCustomComponents();
            LoadEmployeesFromFile();
            UpdateDashboard();
            this.Resize += (s, e) => ResponsiveLayout();
            InitializeThemeMenu();
            InitializeToastNotification();
            CheckUpcomingLeaveEndings();
        }

        private void InitializeCustomComponents()
        {
            // Menü
            MenuStrip mainMenu = new MenuStrip();
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("Dosya");
            ToolStripMenuItem employeeMenu = new ToolStripMenuItem("Çalışanlar");
            ToolStripMenuItem reportsMenu = new ToolStripMenuItem("Raporlar");
            ToolStripMenuItem izinMenu = new ToolStripMenuItem("İzinler");

            fileMenu.DropDownItems.Add("Kaydet", null, (s, e) => SaveEmployeesToFile());
            fileMenu.DropDownItems.Add("Yükle", null, (s, e) => { LoadEmployeesFromFile(); ShowEmployeeList(); });
            fileMenu.DropDownItems.Add("PDF'e Aktar", null, (s, e) => ExportToPdf());
            fileMenu.DropDownItems.Add("Excel'e Aktar", null, (s, e) => ExportToExcel());
            fileMenu.DropDownItems.Add("Çıkış", null, (s, e) => { SaveEmployeesToFile(); Application.Exit(); });
            
            employeeMenu.DropDownItems.Add("Çalışan Ekle", null, (s, e) => ShowAddEmployeePanel());
            employeeMenu.DropDownItems.Add("Çalışan Listesi", null, (s, e) => ShowEmployeeList());
            
            reportsMenu.DropDownItems.Add("Rapor", null, (s, e) => ShowReport());
            
            izinMenu.DropDownItems.Add("İzin Ekle", null, (s, e) => ShowPermissionSelectionForm());
            izinMenu.DropDownItems.Add("İzin Listesi", null, (s, e) => ShowPermissionList());
            izinMenu.DropDownItems.Add("İzin Kaldır", null, (s, e) => RemoveEmployeePermission());

            mainMenu.Items.Add(fileMenu);
            mainMenu.Items.Add(employeeMenu);
            mainMenu.Items.Add(reportsMenu);
            mainMenu.Items.Add(izinMenu);
            this.MainMenuStrip = mainMenu;
            this.Controls.Add(mainMenu);

            // TableLayoutPanel ile modern ve responsive tasarım
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1,
                BackColor = Color.White
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60)); // Başlık
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // Arama ve butonlar
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // İçerik
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120)); // Rapor
            this.Controls.Add(mainLayout);

            // İçerik paneli (liste, ekleme paneli)
            contentPanel = new RoundedPanel { Dock = DockStyle.Fill, BackColor = Color.White, CornerRadius = 18, ShadowColor = Color.LightGray, Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
            mainLayout.Controls.Add(contentPanel, 0, 2);

            // Çalışan ekleme paneli
            addPanel = new Panel { Location = new Point(0, 180), Size = new Size(350, 260), Visible = false, BackColor = Color.White, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom };
            Label lblAd = new Label { Text = "Ad", Left = 0, Top = 0, Width = 100 };
            txtAd = new TextBox { Left = 110, Top = 0, Width = 200, MaxLength = 30 };
            Label lblSoyad = new Label { Text = "Soyad", Left = 0, Top = 30, Width = 100 };
            txtSoyad = new TextBox { Left = 110, Top = 30, Width = 200, MaxLength = 30 };
            Label lblDepartman = new Label { Text = "Departman", Left = 0, Top = 60, Width = 100 };
            txtDepartman = new TextBox { Left = 110, Top = 60, Width = 200, MaxLength = 30 };
            Label lblPozisyon = new Label { Text = "Pozisyon", Left = 0, Top = 90, Width = 100 };
            txtPozisyon = new TextBox { Left = 110, Top = 90, Width = 200, MaxLength = 30 };
            Label lblTC = new Label { Text = "TC Kimlik No", Left = 0, Top = 120, Width = 100 };
            txtTC = new TextBox { Left = 110, Top = 120, Width = 200, MaxLength = 11 };
            Label lblTelefon = new Label { Text = "Telefon No", Left = 0, Top = 150, Width = 100 };
            txtTelefon = new TextBox { Left = 110, Top = 150, Width = 200, MaxLength = 10 };
            Label lblMaas = new Label { Text = "Maaş", Left = 0, Top = 180, Width = 100 };
            txtMaas = new TextBox { Left = 110, Top = 180, Width = 200, MaxLength = 12 };
            Button btnKaydetPanel = new Button { Text = "Kaydet", Left = 110, Top = 200, Width = 80 };
            Button btnIptal = new Button { Text = "İptal", Left = 230, Top = 200, Width = 80 };
            addPanel.Controls.AddRange(new Control[] { lblAd, txtAd, lblSoyad, txtSoyad, lblDepartman, txtDepartman, lblPozisyon, txtPozisyon, lblTC, txtTC, lblTelefon, txtTelefon, lblMaas, txtMaas, btnKaydetPanel, btnIptal });
            contentPanel.Controls.Add(addPanel);

            // Hoş geldin yazısı
            typewriterLabel = new Label
            {
                Text = "",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 36, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Black,
                Dock = DockStyle.Top,
                Height = 60
            };
            this.Controls.Add(typewriterLabel);
            typewriterLabel.BringToFront();

            // Arama ve butonlar paneli
            searchPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(20, 5, 0, 10),
                AutoSize = false
            };
            Label lblAra = new Label { Text = "Ara:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0, 8, 0, 0) };
            txtAra = new TextBox { Width = 200, Margin = new Padding(5, 5, 20, 5) };
            txtAra.TextChanged += (s, e) => FilterEmployeeList();
            searchPanel.Controls.Add(lblAra);
            searchPanel.Controls.Add(txtAra);
            this.Controls.Add(searchPanel);
            searchPanel.BringToFront();

            // Sonra dgv
            dgv = new DataGridView
            {
                Location = new Point(10, 80),
                Size = new Size(contentPanel.Width - 20, contentPanel.Height - 140),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Visible = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White
            };
            dgv.CellDoubleClick += Dgv_CellDoubleClick;
            contentPanel.Controls.Add(dgv);

            // Sil butonu
            btnSil = new RoundedButton
            {
                Text = "Seçili Çalışanı Sil",
                Location = new Point(0, 210),
                Anchor = AnchorStyles.Bottom,
                Size = new Size(120, 35),
                Visible = false,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                CornerRadius = 12
            };
            btnSil.Click += BtnSil_Click;
            btnSil.MouseEnter += (s, e) => btnSil.BackColor = Color.FromArgb(200, 40, 50);
            btnSil.MouseLeave += (s, e) => btnSil.BackColor = Color.FromArgb(220, 53, 69);
            btnSil.MouseDown += (s, e) => btnSil.BackColor = Color.FromArgb(180, 30, 40);
            btnSil.MouseUp += (s, e) => btnSil.BackColor = Color.FromArgb(200, 40, 50);
            contentPanel.Controls.Add(btnSil);

            // Düzenle butonu
            btnDuzenle = new RoundedButton
            {
                Text = "Seçili Çalışanı Düzenle",
                Location = new Point(130, 210),
                Anchor = AnchorStyles.Bottom,
                Size = new Size(150, 35),
                Visible = false,
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                CornerRadius = 12
            };
            btnDuzenle.Click += BtnDuzenle_Click;
            btnDuzenle.MouseEnter += (s, e) => btnDuzenle.BackColor = Color.FromArgb(0, 90, 200);
            btnDuzenle.MouseLeave += (s, e) => btnDuzenle.BackColor = Color.FromArgb(0, 123, 255);
            btnDuzenle.MouseDown += (s, e) => btnDuzenle.BackColor = Color.FromArgb(0, 70, 150);
            btnDuzenle.MouseUp += (s, e) => btnDuzenle.BackColor = Color.FromArgb(0, 90, 200);
            contentPanel.Controls.Add(btnDuzenle);

            // PDF'e aktar butonu
            btnPdf = new RoundedButton
            {
                Text = "PDF'e Aktar",
                Location = new Point(290, 210),
                Anchor = AnchorStyles.Bottom,
                Size = new Size(120, 35),
                Visible = false,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                CornerRadius = 12
            };
            btnPdf.Click += (s, e) => ExportToPdf();
            btnPdf.MouseEnter += (s, e) => btnPdf.BackColor = Color.FromArgb(30, 140, 50);
            btnPdf.MouseLeave += (s, e) => btnPdf.BackColor = Color.FromArgb(40, 167, 69);
            btnPdf.MouseDown += (s, e) => btnPdf.BackColor = Color.FromArgb(20, 100, 30);
            btnPdf.MouseUp += (s, e) => btnPdf.BackColor = Color.FromArgb(30, 140, 50);
            contentPanel.Controls.Add(btnPdf);

            // Excel'e aktar butonu
            btnExcel = new RoundedButton
            {
                Text = "Excel'e Aktar",
                Location = new Point(420, 210),
                Anchor = AnchorStyles.Bottom,
                Size = new Size(120, 35),
                Visible = false,
                BackColor = Color.FromArgb(255, 193, 7),
                ForeColor = Color.Black,
                CornerRadius = 12
            };
            btnExcel.Click += (s, e) => ExportToExcel();
            btnExcel.MouseEnter += (s, e) => btnExcel.BackColor = Color.FromArgb(255, 213, 77);
            btnExcel.MouseLeave += (s, e) => btnExcel.BackColor = Color.FromArgb(255, 193, 7);
            btnExcel.MouseDown += (s, e) => btnExcel.BackColor = Color.FromArgb(255, 160, 0);
            btnExcel.MouseUp += (s, e) => btnExcel.BackColor = Color.FromArgb(255, 213, 77);
            contentPanel.Controls.Add(btnExcel);

            // Rapor etiketi
            lblRapor = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                Visible = false,
                Padding = new Padding(20, 10, 0, 0)
            };
            mainLayout.Controls.Add(lblRapor, 0, 3);

            // Buton eventleri
            btnKaydetPanel.Click += (s, e) =>
            {
                try
                {
                    // Alan kontrolleri
                    if (string.IsNullOrWhiteSpace(txtAd.Text) || !IsOnlyLetters(txtAd.Text))
                        throw new Exception("Ad alanı boş olamaz ve sadece harf içermelidir.");
                    if (string.IsNullOrWhiteSpace(txtSoyad.Text) || !IsOnlyLetters(txtSoyad.Text))
                        throw new Exception("Soyad alanı boş olamaz ve sadece harf içermelidir.");
                    if (string.IsNullOrWhiteSpace(txtDepartman.Text) || !IsOnlyLetters(txtDepartman.Text))
                        throw new Exception("Departman alanı boş olamaz ve sadece harf içermelidir.");
                    if (string.IsNullOrWhiteSpace(txtPozisyon.Text) || !IsOnlyLetters(txtPozisyon.Text))
                        throw new Exception("Pozisyon alanı boş olamaz ve sadece harf içermelidir.");
                    if (txtTC.Text.Length != 11)
                        throw new Exception("TC Kimlik No 11 haneli olmalıdır.");
                    if (!txtTC.Text.All(char.IsDigit))
                        throw new Exception("TC Kimlik No sadece rakam içermelidir.");
                    if (txtTelefon.Text.Length != 10)
                        throw new Exception("Telefon No 10 haneli olmalıdır.");
                    if (!txtTelefon.Text.All(char.IsDigit))
                        throw new Exception("Telefon No sadece rakam içermelidir.");
                    if (string.IsNullOrWhiteSpace(txtMaas.Text))
                        throw new Exception("Maaş alanı boş olamaz.");
                    if (!decimal.TryParse(txtMaas.Text, out decimal maas) || maas < 0)
                        throw new Exception("Maaş alanı sadece pozitif rakam ve nokta içermelidir.");

                    if (editingIndex.HasValue)
                    {
                        employees[editingIndex.Value].Ad = txtAd.Text;
                        employees[editingIndex.Value].Soyad = txtSoyad.Text;
                        employees[editingIndex.Value].Departman = txtDepartman.Text;
                        employees[editingIndex.Value].Pozisyon = txtPozisyon.Text;
                        employees[editingIndex.Value].TC = txtTC.Text;
                        employees[editingIndex.Value].Telefon = txtTelefon.Text;
                        employees[editingIndex.Value].Maas = maas;
                        MessageBox.Show("Çalışan güncellendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        highlightRowIndex = editingIndex.Value;
                        ShowNotification("Çalışan başarıyla güncellendi!");
                        editingIndex = null;
                    }
                    else
                    {
                        employees.Add(new Employee
                        {
                            Ad = txtAd.Text,
                            Soyad = txtSoyad.Text,
                            Departman = txtDepartman.Text,
                            Pozisyon = txtPozisyon.Text,
                            TC = txtTC.Text,
                            Telefon = txtTelefon.Text,
                            Maas = maas
                        });
                        // Sadece bir kez mesaj göster
                        // MessageBox.Show("Çalışan başarıyla eklendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        highlightRowIndex = employees.Count - 1;
                        ShowNotification("Çalışan başarıyla eklendi!");
                    }
                    txtAd.Text = txtSoyad.Text = txtDepartman.Text = txtPozisyon.Text = txtTC.Text = txtTelefon.Text = txtMaas.Text = "";
                    addPanel.Visible = false;
                    SaveEmployeesToFile();
                    ShowEmployeeList();
                    FilterEmployeeList();
                    StartHighlightTimer();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            btnIptal.Click += (s, e) => { addPanel.Visible = false; editingIndex = null; };

            // Sadece harf girilmesi için KeyPress eventleri
            txtAd.KeyPress += OnlyLetter_KeyPress;
            txtSoyad.KeyPress += OnlyLetter_KeyPress;
            txtDepartman.KeyPress += OnlyLetter_KeyPress;
            txtPozisyon.KeyPress += OnlyLetter_KeyPress;
            // Sadece rakam girilmesi için KeyPress eventleri
            txtTC.KeyPress += OnlyDigit_KeyPress;
            txtTelefon.KeyPress += OnlyDigit_KeyPress;
            // Sadece rakam ve nokta için
            txtMaas.KeyPress += OnlyDecimal_KeyPress;

            // Eski izin butonları kodunu kaldır
            // Butonları contentPanel'e ekle
            contentPanel.Controls.Add(addPanel);
            contentPanel.Controls.Add(dgv);
            contentPanel.Controls.Add(btnSil);
            contentPanel.Controls.Add(btnDuzenle);
            contentPanel.Controls.Add(btnPdf);
            contentPanel.Controls.Add(btnExcel);

            // DataGridView focus kaybolsun: boş bir yere tıklayınca satır seçimi kalksın
            dgv.CellClick += (s, e) =>
            {
                if (e.RowIndex == -1)
                {
                    dgv.ClearSelection();
                }
                else if (e.RowIndex >= 0 && e.RowIndex < dgv.Rows.Count)
                {
                    // Satır animasyonu (parlama)
                    flashRowIndex = e.RowIndex;
                    if (dgvFlashTimer != null) dgvFlashTimer.Stop();
                    dgv.Rows[flashRowIndex].DefaultCellStyle.BackColor = Color.LightSkyBlue;
                    dgvFlashTimer = new Timer { Interval = 300 };
                    dgvFlashTimer.Tick += (s2, e2) =>
                    {
                        if (flashRowIndex >= 0 && flashRowIndex < dgv.Rows.Count)
                        {
                            dgv.Rows[flashRowIndex].DefaultCellStyle.BackColor = Color.White;
                        }
                        dgvFlashTimer.Stop();
                    };
                    dgvFlashTimer.Start();
                }
            };
            dgv.SelectionChanged += (s, e) =>
            {
                if (dgv.SelectedRows.Count == 0 && dgv.CurrentCell == null)
                {
                    dgv.ClearSelection();
                }
            };
            // Durum sütunu renklendirme
            dgv.DataBindingComplete += (s, e) =>
            {
                if (dgv.Columns["Durum"] != null)
                {
                    foreach (DataGridViewRow row in dgv.Rows)
                    {
                        var val = row.Cells["Durum"].Value?.ToString();
                        if (val == "İzinli")
                            row.Cells["Durum"].Style.ForeColor = Color.Orange;
                        else if (val == "Çalışıyor")
                            row.Cells["Durum"].Style.ForeColor = Color.Green;
                    }
                }
            };

            // --- TYPEWRITER (YAZI MAKİNESİ) EFEKTİ ---
            typewriterTimer = new Timer { Interval = 40 };
            typewriterTimer.Tick += (s, e) =>
            {
                if (typewriterIndex < typewriterFullText.Length)
                {
                    typewriterLabel.Text += typewriterFullText[typewriterIndex];
                    typewriterIndex++;
                }
                else
                {
                    typewriterTimer.Stop();
                }
            };
            this.Load += (s, e) =>
            {
                typewriterLabel.Text = "";
                typewriterIndex = 0;
                typewriterTimer.Start();
            };

            // Butonlara tıklama animasyonu
            void ButtonFlash(Button btn, Color flashColor)
            {
                Color original = btn.BackColor;
                btn.BackColor = flashColor;
                Timer t = new Timer { Interval = 120 };
                t.Tick += (s, e) => { btn.BackColor = original; t.Stop(); t.Dispose(); };
                t.Start();
            }
            btnSil.Click += (s, e) => ButtonFlash(btnSil, Color.LightSkyBlue);
            btnDuzenle.Click += (s, e) => ButtonFlash(btnDuzenle, Color.LightSkyBlue);
            btnPdf.Click += (s, e) => ButtonFlash(btnPdf, Color.LightSkyBlue);
            // Eğer btnExcel varsa:
            if (btnExcel != null)
                btnExcel.Click += (s, e) => ButtonFlash(btnExcel, Color.LightSkyBlue);

            // DataGridView sağ tık menüsü (QR Kod ve İzin Kaldır)
            dgvContextMenu = new ContextMenuStrip();
            var qrMenuItem = new ToolStripMenuItem("QR Kod Göster");
            qrMenuItem.Click += (s, e) => ShowEmployeeQrCode();
            dgvContextMenu.Items.Add(qrMenuItem);
            var izinKaldirMenuItem = new ToolStripMenuItem("İzin Kaldır");
            izinKaldirMenuItem.Click += (s, e) => RemoveEmployeePermission();
            dgvContextMenu.Items.Add(izinKaldirMenuItem);
            dgv.ContextMenuStrip = dgvContextMenu;

            // Dashboard paneli
            dashboardPanel = new Panel
            {
                Height = 90,
                Dock = DockStyle.Top,
                BackColor = Color.White,
                Padding = new Padding(0, 0, 0, 10)
            };
            lblTotalEmployees = new Label
            {
                Text = "0",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 123, 255),
                BackColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Width = 200,
                Height = 70,
                Location = new Point(40, 10)
            };
            Label lblTotalTitle = new Label
            {
                Text = "Toplam Çalışan",
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                ForeColor = Color.Gray,
                BackColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.TopCenter,
                Width = 200,
                Height = 20,
                Location = new Point(40, 60)
            };
            lblIzinliEmployees = new Label
            {
                Text = "0",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 193, 7),
                BackColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Width = 200,
                Height = 70,
                Location = new Point(260, 10)
            };
            Label lblIzinliTitle = new Label
            {
                Text = "İzinli Çalışan",
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                ForeColor = Color.Gray,
                BackColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.TopCenter,
                Width = 200,
                Height = 20,
                Location = new Point(260, 60)
            };
            dashboardPanel.Controls.Add(lblTotalEmployees);
            dashboardPanel.Controls.Add(lblTotalTitle);
            dashboardPanel.Controls.Add(lblIzinliEmployees);
            dashboardPanel.Controls.Add(lblIzinliTitle);
            contentPanel.Controls.Add(dashboardPanel);
            dashboardPanel.BringToFront();
            // Dashboard animasyon timer
            dashboardAnimTimer = new Timer { Interval = 20 };
            dashboardAnimTimer.Tick += (s, e) => AnimateDashboard();
            // Dashboard ilk yükleme
            UpdateDashboard();
        }

        private void InitializeThemeMenu()
        {
            temaMenu = new ToolStripMenuItem("Tema");
            lightModeMenuItem = new ToolStripMenuItem("Açık Mod");
            darkModeMenuItem = new ToolStripMenuItem("Koyu Mod");
            lightModeMenuItem.Click += (s, e) => { ApplyLightTheme(); currentTheme = "Light"; };
            darkModeMenuItem.Click += (s, e) => { ApplyDarkTheme(); currentTheme = "Dark"; };
            temaMenu.DropDownItems.Add(lightModeMenuItem);
            temaMenu.DropDownItems.Add(darkModeMenuItem);
            // MenuStrip'e ekle
            if (this.MainMenuStrip != null)
                this.MainMenuStrip.Items.Add(temaMenu);
        }

        private void InitializeToastNotification()
        {
            toastLabel = new Label
            {
                Visible = false,
                AutoSize = false,
                Height = 40,
                Width = 350,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            this.Controls.Add(toastLabel);
            toastLabel.BringToFront();
            toastLabel.Left = this.ClientSize.Width - toastLabel.Width - 30;
            toastLabel.Top = this.ClientSize.Height - toastLabel.Height - 30;
            this.Resize += (s, e) =>
            {
                toastLabel.Left = this.ClientSize.Width - toastLabel.Width - 30;
                toastLabel.Top = this.ClientSize.Height - toastLabel.Height - 30;
            };
            toastTimer = new Timer { Interval = 3000 };
            toastTimer.Tick += (s, e) => { toastLabel.Visible = false; toastTimer.Stop(); };
        }

        private void ShowToastNotification(string message)
        {
            toastLabel.Text = message;
            toastLabel.Visible = true;
            toastTimer.Stop();
            toastTimer.Start();
        }

        private void CheckUpcomingLeaveEndings()
        {
            var now = DateTime.Now.Date;
            var soonEnding = employees.Where(e => e.Izinli && e.IzinBaslangic != default && (e.IzinBaslangic.AddDays(e.IzinGunu) - now).TotalDays <= 3 && (e.IzinBaslangic.AddDays(e.IzinGunu) - now).TotalDays >= 0).ToList();
            if (soonEnding.Count > 0)
            {
                string msg = "Yaklaşan izin bitişleri: " + string.Join(", ", soonEnding.Select(e => $"{e.Ad} {e.Soyad} ({(e.IzinBaslangic.AddDays(e.IzinGunu) - now).TotalDays} gün kaldı)"));
                ShowToastNotification(msg);
            }
        }

        private void ShowAddEmployeePanel()
        {
            // Slide animasyonu ile ekleme panelini göster
            addPanel.Left = -addPanel.Width;
            addPanel.Top = 180;
            addPanel.Visible = true;
            dgv.Visible = false;
            btnSil.Visible = false;
            btnDuzenle.Visible = false;
            btnPdf.Visible = false;
            btnExcel.Visible = false;
            lblRapor.Visible = false;
            txtAd.Text = txtSoyad.Text = txtDepartman.Text = txtPozisyon.Text = txtTC.Text = txtTelefon.Text = txtMaas.Text = "";
            editingIndex = null;
            Timer slideTimer = new Timer { Interval = 10 };
            slideTimer.Tick += (s, e) =>
            {
                if (addPanel.Left < 0)
                    addPanel.Left += 20;
                else
                {
                    addPanel.Left = 0;
                    slideTimer.Stop();
                }
            };
            slideTimer.Start();
            UpdateDashboard();
        }
        private void ShowEmployeeList()
        {
            dgv.DataSource = null;
            var displayList = FilteredEmployees().Select(emp => new
            {
                emp.Ad,
                emp.Soyad,
                emp.Departman,
                emp.Pozisyon,
                emp.TC,
                emp.Telefon,
                emp.Maas,
                Durum = emp.Izinli ? "İzinli" : "Çalışıyor"
            }).ToList();
            dgv.DataSource = displayList;

            // Durum sütununu renklendir
            if (dgv.Columns["Durum"] != null)
            {
                dgv.Columns["Durum"].DefaultCellStyle.ForeColor = Color.Black;
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (row.Cells["Durum"].Value?.ToString() == "İzinli")
                    {
                        row.Cells["Durum"].Style.ForeColor = Color.Orange;
                    }
                    else
                    {
                        row.Cells["Durum"].Style.ForeColor = Color.Green;
                    }
                }
            }

            dgv.Visible = true;
            addPanel.Visible = false;
            btnSil.Visible = true;
            btnDuzenle.Visible = true;
            btnPdf.Visible = true;
            btnExcel.Visible = true;
            lblRapor.Visible = false;
            UpdateDashboard();
        }
        private void ShowReport()
        {
            int toplam = employees.Count;
            var departmanlar = employees.GroupBy(e => e.Departman)
                .Select(g => $"{g.Key}: {g.Count()} kişi").ToList();
            lblRapor.Text = $"Toplam Çalışan: {toplam}\n" + string.Join("\n", departmanlar);
            lblRapor.Visible = true;
            addPanel.Visible = false;
            dgv.Visible = false;
            btnSil.Visible = false;
            btnDuzenle.Visible = false;
            btnPdf.Visible = false;
            btnExcel.Visible = false;
        }
        private void BtnSil_Click(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null)
            {
                MessageBox.Show("Lütfen silinecek çalışanı seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dgv.CurrentRow.Index >= 0 && dgv.CurrentRow.Index < dgv.Rows.Count)
            {
                int index = dgv.CurrentRow.Index;
                var filtered = FilteredEmployees();
                var silinecek = filtered.ElementAtOrDefault(index);
                if (silinecek != null)
                {
                    int realIndex = employees.FindIndex(emp =>
                        emp.Ad == silinecek.Ad &&
                        emp.Soyad == silinecek.Soyad &&
                        emp.Departman == silinecek.Departman &&
                        emp.Pozisyon == silinecek.Pozisyon &&
                        emp.TC == silinecek.TC &&
                        emp.Telefon == silinecek.Telefon);
                    if (realIndex >= 0)
                    {
                        var result = MessageBox.Show($"{silinecek.Ad} {silinecek.Soyad} adlı çalışanı silmek istiyor musunuz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result == DialogResult.Yes)
                        {
                            employees.RemoveAt(realIndex);
                            SaveEmployeesToFile();
                            ShowEmployeeList();
                            FilterEmployeeList();
                            ShowNotification("Çalışan başarıyla silindi!");
                        }
                    }
                }
            }
        }
        private void BtnDuzenle_Click(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null)
            {
                MessageBox.Show("Lütfen düzenlenecek çalışanı seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dgv.CurrentRow.Index >= 0 && dgv.CurrentRow.Index < dgv.Rows.Count)
            {
                int index = dgv.CurrentRow.Index;
                var filtered = FilteredEmployees();
                var duzenlenecek = filtered.ElementAtOrDefault(index);
                if (duzenlenecek != null)
                {
                    int realIndex = employees.FindIndex(emp =>
                        emp.Ad == duzenlenecek.Ad &&
                        emp.Soyad == duzenlenecek.Soyad &&
                        emp.Departman == duzenlenecek.Departman &&
                        emp.Pozisyon == duzenlenecek.Pozisyon &&
                        emp.TC == duzenlenecek.TC &&
                        emp.Telefon == duzenlenecek.Telefon);
                    if (realIndex >= 0)
                    {
                        txtAd.Text = duzenlenecek.Ad;
                        txtSoyad.Text = duzenlenecek.Soyad;
                        txtDepartman.Text = duzenlenecek.Departman;
                        txtPozisyon.Text = duzenlenecek.Pozisyon;
                        txtTC.Text = duzenlenecek.TC;
                        txtTelefon.Text = duzenlenecek.Telefon;
                        editingIndex = realIndex;
                        addPanel.Visible = true;
                        dgv.Visible = false;
                        btnSil.Visible = false;
                        btnDuzenle.Visible = false;
                        btnPdf.Visible = false;
                        btnExcel.Visible = false;
                        lblRapor.Visible = false;
                    }
                }
            }
        }
        private void Dgv_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgv.Rows.Count)
            {
                var detay = FilteredEmployees().ElementAtOrDefault(e.RowIndex);
                if (detay != null)
                {
                    MessageBox.Show($"Ad: {detay.Ad}\nSoyad: {detay.Soyad}\nDepartman: {detay.Departman}\nPozisyon: {detay.Pozisyon}", "Çalışan Detayları", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        private void FilterEmployeeList()
        {
            string arama = txtAra.Text.ToLower();
            var filtered = FilteredEmployees();
            dgv.DataSource = null;
            dgv.DataSource = filtered.Select(emp => new 
            { 
                emp.Ad, 
                emp.Soyad, 
                emp.Departman, 
                emp.Pozisyon, 
                emp.TC, 
                emp.Telefon, 
                emp.Maas,
                Durum = emp.Izinli ? "İzinli" : "Çalışıyor"
            }).ToList();

            // Durum sütununu renklendir
            if (dgv.Columns["Durum"] != null)
            {
                dgv.Columns["Durum"].DefaultCellStyle.ForeColor = Color.Black;
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (row.Cells["Durum"].Value?.ToString() == "İzinli")
                    {
                        row.Cells["Durum"].Style.ForeColor = Color.Orange;
                    }
                    else
                    {
                        row.Cells["Durum"].Style.ForeColor = Color.Green;
                    }
                }
            }

            if (highlightRowIndex >= 0 && highlightRowIndex < dgv.Rows.Count)
            {
                dgv.ClearSelection();
                dgv.Rows[highlightRowIndex].Selected = true;
                dgv.Rows[highlightRowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
            }
        }
        private List<Employee> FilteredEmployees()
        {
            string arama = txtAra.Text.ToLower();
            return employees.Where(emp =>
                emp.Ad.ToLower().Contains(arama) ||
                emp.Soyad.ToLower().Contains(arama) ||
                emp.Departman.ToLower().Contains(arama) ||
                emp.Pozisyon.ToLower().Contains(arama)
            ).ToList();
        }
        private void SaveEmployeesToFile()
        {
            try
            {
                File.WriteAllText(dataFile, JsonConvert.SerializeObject(employees));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kaydetme hatası: " + ex.Message);
            }
        }
        private void LoadEmployeesFromFile()
        {
            try
            {
                if (File.Exists(dataFile))
                {
                    employees = JsonConvert.DeserializeObject<List<Employee>>(File.ReadAllText(dataFile)) ?? new List<Employee>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Yükleme hatası: " + ex.Message);
            }
        }
        private void ExportToPdf()
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog { Filter = "PDF Dosyası|*.pdf", FileName = "calisanlar.pdf" };
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    using (var fs = new FileStream(sfd.FileName, FileMode.Create))
                    {
                        Document doc = new Document(PageSize.A4, 30, 30, 30, 30);
                        PdfWriter writer = PdfWriter.GetInstance(doc, fs);
                        doc.Open();

                        // Başlık
                        PdfPTable titleTable = new PdfPTable(1);
                        titleTable.WidthPercentage = 100;
                        PdfPCell titleCell = new PdfPCell(new Phrase("ÇALIŞAN LİSTESİ", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 16, iTextSharp.text.Font.BOLD, new BaseColor(255,255,255))))
                        {
                            BackgroundColor = new BaseColor(0, 123, 255),
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            Border = iTextSharp.text.Rectangle.NO_BORDER,
                            PaddingTop = 10f,
                            PaddingBottom = 10f
                        };
                        titleTable.AddCell(titleCell);
                        doc.Add(titleTable);
                        doc.Add(new Paragraph(" "));

                        // Sütun başlıkları
                        string[] headers = { "Ad", "Soyad", "Departman", "Pozisyon", "TC Kimlik No", "Telefon", "Maaş", "Durum" };
                        PdfPTable table = new PdfPTable(headers.Length);
                        table.WidthPercentage = 100;
                        table.SetWidths(new float[] { 1.2f, 1.2f, 1.2f, 1.2f, 1.5f, 1.5f, 1.2f, 1.2f });

                        foreach (var h in headers)
                        {
                            PdfPCell cell = new PdfPCell(new Phrase(h, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 11, iTextSharp.text.Font.BOLD, new BaseColor(0,0,0))))
                            {
                                BackgroundColor = new BaseColor(240, 240, 240),
                                HorizontalAlignment = Element.ALIGN_CENTER,
                                Padding = 6f
                            };
                            table.AddCell(cell);
                        }

                        // Veriler
                        foreach (var emp in FilteredEmployees())
                        {
                            table.AddCell(new PdfPCell(new Phrase(emp.Ad)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5f });
                            table.AddCell(new PdfPCell(new Phrase(emp.Soyad)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5f });
                            table.AddCell(new PdfPCell(new Phrase(emp.Departman)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5f });
                            table.AddCell(new PdfPCell(new Phrase(emp.Pozisyon)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5f });
                            table.AddCell(new PdfPCell(new Phrase(emp.TC)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5f });
                            table.AddCell(new PdfPCell(new Phrase(emp.Telefon)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5f });
                            table.AddCell(new PdfPCell(new Phrase(emp.Maas.ToString("N2") + " ₺")) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5f });
                            // Durum renkli
                            var durum = emp.Izinli ? "İzinli" : "Çalışıyor";
                            var durumColor = emp.Izinli ? new BaseColor(255, 140, 0) : new BaseColor(0, 180, 0);
                            table.AddCell(new PdfPCell(new Phrase(durum, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 11, iTextSharp.text.Font.BOLD, durumColor)))
                            {
                                HorizontalAlignment = Element.ALIGN_CENTER,
                                Padding = 5f
                            });
                        }

                        // Kenarlıklar
                        foreach (PdfPCell cell in table.Rows.SelectMany(r => r.GetCells()))
                        {
                            cell.BorderWidth = 0.7f;
                            cell.BorderColor = new BaseColor(180, 180, 180);
                        }

                        doc.Add(table);
                        doc.Add(new Paragraph(" "));

                        // Özet Bilgiler
                        PdfPTable summaryTable = new PdfPTable(1);
                        summaryTable.WidthPercentage = 60;
                        summaryTable.HorizontalAlignment = Element.ALIGN_LEFT;

                        // Başlık
                        PdfPCell summaryTitle = new PdfPCell(new Phrase("ÖZET BİLGİLER", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.BOLD, new BaseColor(255,255,255))))
                        {
                            BackgroundColor = new BaseColor(0, 123, 255),
                            Padding = 7f,
                            BorderWidth = 0.7f,
                            BorderColor = new BaseColor(180, 180, 180)
                        };
                        summaryTable.AddCell(summaryTitle);

                        string[] summaryLines = {
                            $"Toplam Çalışan: {employees.Count}",
                            $"İzinli Çalışan: {employees.Count(e => e.Izinli)}",
                            $"Çalışan Sayısı: {employees.Count(e => !e.Izinli)}",
                            $"Oluşturulma Tarihi: {DateTime.Now:dd.MM.yyyy HH:mm}"
                        };
                        foreach (var line in summaryLines)
                        {
                            PdfPCell infoCell = new PdfPCell(new Phrase(line, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 11, iTextSharp.text.Font.BOLD, new BaseColor(0,0,0))))
                            {
                                BackgroundColor = new BaseColor(240, 240, 240),
                                Padding = 6f,
                                BorderWidth = 0.7f,
                                BorderColor = new BaseColor(180, 180, 180)
                            };
                            summaryTable.AddCell(infoCell);
                        }

                        doc.Add(summaryTable);
                        doc.Close();
                    }
                    MessageBox.Show("PDF başarıyla oluşturuldu!", "Başarılı");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("PDF oluşturma hatası: " + ex.Message);
            }
        }
        private void ExportToExcel()
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog { Filter = "Excel Dosyası|*.xlsx", FileName = "calisanlar.xlsx" };
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    // EPPlus lisans modunu ayarla
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                    using (var package = new ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("Çalışanlar");

                        // Başlık satırı
                        worksheet.Cells[1, 1].Value = "ÇALIŞAN LİSTESİ";
                        using (var range = worksheet.Cells[1, 1, 1, 8])
                        {
                            range.Merge = true;
                            range.Style.Font.Bold = true;
                            range.Style.Font.Size = 14;
                            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 123, 255));
                            range.Style.Font.Color.SetColor(Color.White);
                        }

                        // Sütun başlıkları
                        string[] headers = { "Ad", "Soyad", "Departman", "Pozisyon", "TC Kimlik No", "Telefon", "Maaş", "Durum" };
                        for (int i = 0; i < headers.Length; i++)
                        {
                            worksheet.Cells[2, i + 1].Value = headers[i];
                        }

                        // Başlık formatı
                        using (var range = worksheet.Cells[2, 1, 2, 8])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(240, 240, 240));
                            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        }

                        // Verileri ekle
                        int row = 3;
                        foreach (var emp in FilteredEmployees())
                        {
                            worksheet.Cells[row, 1].Value = emp.Ad;
                            worksheet.Cells[row, 2].Value = emp.Soyad;
                            worksheet.Cells[row, 3].Value = emp.Departman;
                            worksheet.Cells[row, 4].Value = emp.Pozisyon;
                            worksheet.Cells[row, 5].Value = emp.TC;
                            worksheet.Cells[row, 6].Value = emp.Telefon;
                            worksheet.Cells[row, 7].Value = emp.Maas;
                            worksheet.Cells[row, 8].Value = emp.Izinli ? "İzinli" : "Çalışıyor";

                            // Durum sütununu renklendir
                            if (emp.Izinli)
                            {
                                worksheet.Cells[row, 8].Style.Font.Color.SetColor(Color.Orange);
                            }
                            else
                            {
                                worksheet.Cells[row, 8].Style.Font.Color.SetColor(Color.Green);
                            }

                            // Satır formatı
                            using (var range = worksheet.Cells[row, 1, row, 8])
                            {
                                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            }

                            row++;
                        }

                        // Maaş sütununu para birimi formatına çevir
                        using (var range = worksheet.Cells[3, 7, row - 1, 7])
                        {
                            range.Style.Numberformat.Format = "#,##0.00 ₺";
                        }

                        // Sütun genişliklerini otomatik ayarla
                        worksheet.Cells.AutoFitColumns();

                        // Alt bilgi satırları
                        row += 2;
                        // Özet başlığı
                        worksheet.Cells[row, 1].Value = "ÖZET BİLGİLER";
                        using (var range = worksheet.Cells[row, 1, row, 8])
                        {
                            range.Merge = true;
                            range.Style.Font.Bold = true;
                            range.Style.Font.Size = 12;
                            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 123, 255));
                            range.Style.Font.Color.SetColor(Color.White);
                        }
                        row++;
                        // Her bilgi ayrı satırda, sadece ilk sütunda
                        string[] summaryLines = {
                            $"Toplam Çalışan: {employees.Count}",
                            $"İzinli Çalışan: {employees.Count(e => e.Izinli)}",
                            $"Çalışan Sayısı: {employees.Count(e => !e.Izinli)}",
                            $"Oluşturulma Tarihi: {DateTime.Now:dd.MM.yyyy HH:mm}"
                        };
                        foreach (var line in summaryLines)
                        {
                            worksheet.Cells[row, 1].Value = line;
                            using (var range = worksheet.Cells[row, 1, row, 8])
                            {
                                range.Merge = true;
                                range.Style.Font.Bold = true;
                                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(240, 240, 240));
                                range.Style.Font.Color.SetColor(Color.Black);
                                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            }
                            row++;
                        }

                        // Dosyayı kaydet
                        package.SaveAs(new FileInfo(sfd.FileName));
                        MessageBox.Show("Excel dosyası başarıyla oluşturuldu!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Excel oluşturma hatası: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveEmployeesToFile();
            base.OnFormClosing(e);
        }
        private void OnlyLetter_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsLetter(e.KeyChar))
                e.Handled = true;
        }
        private void OnlyDigit_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }
        private void OnlyDecimal_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                e.Handled = true;
            // Sadece bir tane nokta olsun
            if (e.KeyChar == '.' && (sender as TextBox).Text.Contains('.'))
                e.Handled = true;
        }
        private bool IsOnlyLetters(string text)
        {
            return text.All(c => char.IsLetter(c) || char.IsWhiteSpace(c));
        }
        private void StartHighlightTimer()
        {
            if (highlightTimer != null)
            {
                highlightTimer.Stop();
                highlightTimer.Dispose();
            }
            highlightTimer = new Timer { Interval = 800 };
            highlightTimer.Tick += (s, e) =>
            {
                if (highlightRowIndex >= 0 && highlightRowIndex < dgv.Rows.Count)
                {
                    dgv.Rows[highlightRowIndex].DefaultCellStyle.BackColor = Color.White;
                    highlightRowIndex = -1;
                }
                highlightTimer.Stop();
                highlightTimer.Dispose();
            };
            highlightTimer.Start();
        }
        private void ResponsiveLayout()
        {
            if (contentPanel != null && dgv != null)
            {
                int topOffset = 180;
                int bottomOffset = 180;
                dgv.Location = new Point(10, topOffset);
                dgv.Width = contentPanel.Width - 20;
                dgv.Height = contentPanel.Height - topOffset - bottomOffset;
                addPanel.Width = contentPanel.Width / 2 - 30;
                addPanel.Height = contentPanel.Height - 80;

                // Butonlar alt kısımda ortalanacak ve tam görünecek
                int y = contentPanel.Height - 60;
                int totalBtnWidth = btnSil.Width + btnDuzenle.Width + btnPdf.Width + btnExcel.Width + 60;
                int startX = (contentPanel.Width - totalBtnWidth) / 2;

                btnSil.Location = new Point(startX, y);
                btnDuzenle.Location = new Point(startX + btnSil.Width + 20, y);
                btnPdf.Location = new Point(startX + btnSil.Width + btnDuzenle.Width + 40, y);
                btnExcel.Location = new Point(startX + btnSil.Width + btnDuzenle.Width + btnPdf.Width + 60, y);

                // Hoş geldin yazısını ortala
                if (typewriterLabel != null)
                {
                    typewriterLabel.Width = this.Width;
                    typewriterLabel.Location = new Point(0, 20);
                }
            }
        }
        private void ShowPermissionSelectionForm()
        {
            Form secimForm = new Form
            {
                Text = "İzin Eklenecek Çalışanı Seçin",
                Size = new Size(800, 500),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            DataGridView secimDgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            var calisanlar = employees.Select(e => new
            {
                e.Ad,
                e.Soyad,
                e.Departman,
                e.Pozisyon,
                Durum = e.Izinli ? "İzinli" : "Çalışıyor"
            }).ToList();

            secimDgv.DataSource = calisanlar;
            secimDgv.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    var secilenCalisan = employees[e.RowIndex];
                    secimForm.Close();
                    ShowPermissionPanel(secilenCalisan);
                }
            };

            Button btnSec = new Button
            {
                Text = "Seç",
                Dock = DockStyle.Bottom,
                Height = 40
            };
            btnSec.Click += (s, e) =>
            {
                try
                {
                    if (secimDgv.CurrentRow == null)
                        throw new Exception("Lütfen izin eklenecek çalışanı seçin!");

                    var secilenCalisan = employees[secimDgv.CurrentRow.Index];
                    secimForm.Close();
                    ShowPermissionPanel(secilenCalisan);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            secimForm.Controls.Add(secimDgv);
            secimForm.Controls.Add(btnSec);
            secimForm.ShowDialog();
        }

        private void ShowPermissionPanel(Employee calisan)
        {
            Form izinForm = new Form
            {
                Text = $"İzin Ekle - {calisan.Ad} {calisan.Soyad}",
                Size = new Size(300, 200),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label lblGun = new Label { Text = "İzin Günü:", Left = 20, Top = 20, Width = 100 };
            TextBox txtGun = new TextBox { Left = 130, Top = 20, Width = 100 };
            txtGun.KeyPress += OnlyDigit_KeyPress;

            Button btnKaydet = new Button { Text = "Kaydet", Left = 130, Top = 100, Width = 80 };
            Button btnIptal = new Button { Text = "İptal", Left = 220, Top = 100, Width = 80 };

            btnKaydet.Click += (s, e) =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(txtGun.Text))
                        throw new Exception("Lütfen izin günü girin!");

                    if (!int.TryParse(txtGun.Text, out int gun) || gun <= 0)
                        throw new Exception("Lütfen geçerli bir gün sayısı girin!");

                    var index = employees.FindIndex(emp =>
                        emp.Ad == calisan.Ad &&
                        emp.Soyad == calisan.Soyad &&
                        emp.Departman == calisan.Departman &&
                        emp.Pozisyon == calisan.Pozisyon);

                    if (index >= 0)
                    {
                        employees[index].Izinli = true;
                        employees[index].IzinGunu = gun;
                        employees[index].IzinBaslangic = DateTime.Now;
                        SaveEmployeesToFile();
                        ShowEmployeeList();
                        FilterEmployeeList();
                        MessageBox.Show("İzin başarıyla eklendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        izinForm.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnIptal.Click += (s, e) => izinForm.Close();

            izinForm.Controls.AddRange(new Control[] { lblGun, txtGun, btnKaydet, btnIptal });
            izinForm.ShowDialog();
        }

        private void ShowPermissionList()
        {
            Form izinListeForm = new Form
            {
                Text = "İzinli Çalışanlar",
                Size = new Size(600, 400),
                StartPosition = FormStartPosition.CenterParent
            };

            DataGridView izinDgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true
            };

            var izinliCalisanlar = employees.Where(e => e.Izinli).Select(e => new
            {
                e.Ad,
                e.Soyad,
                e.Departman,
                e.Pozisyon,
                IzinGunu = e.IzinGunu,
                IzinBaslangic = e.IzinBaslangic.ToString("dd.MM.yyyy"),
                IzinBitis = e.IzinBaslangic.AddDays(e.IzinGunu).ToString("dd.MM.yyyy")
            }).ToList();

            izinDgv.DataSource = izinliCalisanlar;
            izinListeForm.Controls.Add(izinDgv);
            izinListeForm.ShowDialog();
        }

        private void ShowEmployeeQrCode()
        {
            if (dgv.CurrentRow == null) return;
            int index = dgv.CurrentRow.Index;
            var filtered = FilteredEmployees();
            var emp = filtered.ElementAtOrDefault(index);
            if (emp == null) return;
            string info = $"Ad: {emp.Ad}\nSoyad: {emp.Soyad}\nDepartman: {emp.Departman}\nPozisyon: {emp.Pozisyon}\nTC: {emp.TC}\nTelefon: {emp.Telefon}\nMaaş: {emp.Maas}";
            using (var qrGen = new QRCodeGenerator())
            using (var data = qrGen.CreateQrCode(info, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new QRCode(data))
            using (var bmp = qrCode.GetGraphic(10))
            {
                Form qrForm = new Form
                {
                    Text = $"{emp.Ad} {emp.Soyad} - QR Kod",
                    Size = new Size(350, 370),
                    StartPosition = FormStartPosition.CenterParent
                };
                PictureBox pb = new PictureBox
                {
                    Image = new Bitmap(bmp),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Dock = DockStyle.Fill
                };
                qrForm.Controls.Add(pb);
                qrForm.ShowDialog();
            }
        }

        private void UpdateDashboard()
        {
            lblTotalEmployees.Text = "0";
            lblIzinliEmployees.Text = "0";
            dashboardAnimStep = 0;
            dashboardTotalTarget = employees.Count(e => !e.Izinli);
            dashboardIzinliTarget = employees.Count(e => e.Izinli);
            dashboardAnimTimer.Start();
        }
        private void AnimateDashboard()
        {
            int total = int.TryParse(lblTotalEmployees.Text, out int t) ? t : 0;
            int izinli = int.TryParse(lblIzinliEmployees.Text, out int i) ? i : 0;
            bool done = true;
            if (total < dashboardTotalTarget)
            {
                lblTotalEmployees.Text = (total + 1).ToString();
                done = false;
            }
            if (izinli < dashboardIzinliTarget)
            {
                lblIzinliEmployees.Text = (izinli + 1).ToString();
                done = false;
            }
            if (done)
                dashboardAnimTimer.Stop();
        }
        private void ShowNotification(string message)
        {
            MessageBox.Show(message, "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void RemoveEmployeePermission()
        {
            // Yeni formda sadece izinli çalışanlar listelensin
            var izinliList = employees.Where(e => e.Izinli).ToList();
            if (izinliList.Count == 0)
            {
                MessageBox.Show("İzinli çalışan yok!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            Form izinKaldirForm = new Form
            {
                Text = "İzin Kaldır - İzinli Çalışanlar",
                Size = new Size(700, 400),
                StartPosition = FormStartPosition.CenterParent
            };
            DataGridView izinDgv = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 300,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            izinDgv.DataSource = izinliList.Select(e => new
            {
                e.Ad,
                e.Soyad,
                e.Departman,
                e.Pozisyon,
                e.TC,
                e.Telefon,
                e.Maas,
                IzinGunu = e.IzinGunu,
                IzinBaslangic = e.IzinBaslangic.ToString("dd.MM.yyyy")
            }).ToList();
            Button btnKaldir = new Button
            {
                Text = "İzni Kaldır",
                Dock = DockStyle.Bottom,
                Height = 40
            };
            btnKaldir.Click += (s, e) =>
            {
                if (izinDgv.CurrentRow == null)
                {
                    MessageBox.Show("Lütfen bir çalışan seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                int idx = izinDgv.CurrentRow.Index;
                var secilen = izinliList[idx];
                var result = MessageBox.Show($"{secilen.Ad} {secilen.Soyad} adlı çalışanın iznini kaldırmak istiyor musunuz?", "İzin Kaldır", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    var realIndex = employees.FindIndex(emp =>
                        emp.Ad == secilen.Ad &&
                        emp.Soyad == secilen.Soyad &&
                        emp.Departman == secilen.Departman &&
                        emp.Pozisyon == secilen.Pozisyon &&
                        emp.TC == secilen.TC &&
                        emp.Telefon == secilen.Telefon);
                    if (realIndex >= 0)
                    {
                        employees[realIndex].Izinli = false;
                        employees[realIndex].IzinGunu = 0;
                        employees[realIndex].IzinBaslangic = default;
                        SaveEmployeesToFile();
                        ShowEmployeeList();
                        ShowNotification("İzin kaldırıldı!");
                        izinKaldirForm.Close();
                    }
                }
            };
            izinKaldirForm.Controls.Add(izinDgv);
            izinKaldirForm.Controls.Add(btnKaldir);
            izinKaldirForm.ShowDialog();
        }

        private void ApplyLightTheme()
        {
            this.BackColor = Color.White;
            SetPanelBackgrounds(this.Controls, Color.White);
            if (searchPanel != null)
            {
                searchPanel.BackColor = Color.White;
                foreach (Control c in searchPanel.Controls)
                {
                    if (c is Label) { c.BackColor = Color.White; c.ForeColor = Color.Black; }
                    if (c is TextBox) { c.BackColor = Color.White; c.ForeColor = Color.Black; }
                }
            }
            // Çalışan ekleme panelindeki label'lar siyah
            if (addPanel != null)
            {
                foreach (Control c in addPanel.Controls)
                {
                    if (c is Label lbl) lbl.ForeColor = Color.Black;
                }
            }
            // Çalışan listesi DataGridView light mode
            if (dgv != null)
            {
                dgv.BackgroundColor = Color.White;
                dgv.DefaultCellStyle.BackColor = Color.White;
                dgv.DefaultCellStyle.ForeColor = Color.Black;
                dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.White;
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
                dgv.RowHeadersDefaultCellStyle.BackColor = Color.White;
                dgv.RowHeadersDefaultCellStyle.ForeColor = Color.Black;
                dgv.GridColor = Color.LightGray;
                dgv.EnableHeadersVisualStyles = false;
            }
            // Çalışan sayısı kutularının paneli ve label'ı light mode'da beyaz, yazı renkli
            if (panelTotalEmployees != null) panelTotalEmployees.BackColor = Color.White;
            if (lblTotalEmployees != null) { lblTotalEmployees.BackColor = Color.White; lblTotalEmployees.ForeColor = Color.Blue; }
            if (panelIzinliEmployees != null) panelIzinliEmployees.BackColor = Color.White;
            if (lblIzinliEmployees != null) { lblIzinliEmployees.BackColor = Color.White; lblIzinliEmployees.ForeColor = Color.Orange; }
            // Buton renkleri ve diğer ayarlar...
            if (btnSil != null)
            {
                btnSil.BackColor = Color.FromArgb(220, 53, 69);
                btnSil.ForeColor = Color.White;
                btnSil.FlatStyle = FlatStyle.Flat;
                btnSil.FlatAppearance.BorderColor = Color.FromArgb(220, 53, 69);
                btnSil.FlatAppearance.BorderSize = 2;
            }
            if (btnDuzenle != null)
            {
                btnDuzenle.BackColor = Color.FromArgb(0, 123, 255);
                btnDuzenle.ForeColor = Color.White;
                btnDuzenle.FlatStyle = FlatStyle.Flat;
                btnDuzenle.FlatAppearance.BorderColor = Color.FromArgb(0, 123, 255);
                btnDuzenle.FlatAppearance.BorderSize = 2;
            }
            if (btnPdf != null)
            {
                btnPdf.BackColor = Color.FromArgb(40, 167, 69);
                btnPdf.ForeColor = Color.White;
                btnPdf.FlatStyle = FlatStyle.Flat;
                btnPdf.FlatAppearance.BorderColor = Color.FromArgb(40, 167, 69);
                btnPdf.FlatAppearance.BorderSize = 2;
            }
            if (btnExcel != null)
            {
                btnExcel.BackColor = Color.FromArgb(255, 193, 7);
                btnExcel.ForeColor = Color.Black;
                btnExcel.FlatStyle = FlatStyle.Flat;
                btnExcel.FlatAppearance.BorderColor = Color.FromArgb(255, 193, 7);
                btnExcel.FlatAppearance.BorderSize = 2;
            }
        }

        private void ApplyDarkTheme()
        {
            this.BackColor = Color.FromArgb(30, 30, 30);
            SetPanelBackgrounds(this.Controls, Color.FromArgb(30, 30, 30));
            // Arama paneli de koyu, ama içindeki label ve textbox açık
            if (searchPanel != null)
            {
                searchPanel.BackColor = Color.FromArgb(30, 30, 30);
                foreach (Control c in searchPanel.Controls)
                {
                    if (c is Label) { c.BackColor = Color.FromArgb(30, 30, 30); c.ForeColor = Color.White; }
                    if (c is TextBox) { c.BackColor = Color.White; c.ForeColor = Color.Black; }
                }
            }
            // Çalışan ekleme panelindeki label'lar beyaz
            if (addPanel != null)
            {
                foreach (Control c in addPanel.Controls)
                {
                    if (c is Label lbl) lbl.ForeColor = Color.White;
                }
            }
            // Çalışan listesi DataGridView dark mode
            if (dgv != null)
            {
                dgv.BackgroundColor = Color.Black;
                dgv.DefaultCellStyle.BackColor = Color.White;
                dgv.DefaultCellStyle.ForeColor = Color.Black;
                dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.Black;
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                dgv.RowHeadersDefaultCellStyle.BackColor = Color.Black;
                dgv.RowHeadersDefaultCellStyle.ForeColor = Color.White;
                dgv.GridColor = Color.FromArgb(60, 60, 60);
                dgv.EnableHeadersVisualStyles = false;
            }
            // Çalışan sayısı kutularının paneli ve label'ı dark mode'da koyu, yazı renkli
            if (panelTotalEmployees != null) panelTotalEmployees.BackColor = Color.FromArgb(30, 30, 30);
            if (lblTotalEmployees != null) { lblTotalEmployees.BackColor = Color.FromArgb(30, 30, 30); lblTotalEmployees.ForeColor = Color.Blue; }
            if (panelIzinliEmployees != null) panelIzinliEmployees.BackColor = Color.FromArgb(30, 30, 30);
            if (lblIzinliEmployees != null) { lblIzinliEmployees.BackColor = Color.FromArgb(30, 30, 30); lblIzinliEmployees.ForeColor = Color.Orange; }
        }

        private void SetPanelBackgrounds(Control.ControlCollection controls, Color backColor)
        {
            foreach (Control ctrl in controls)
            {
                if (ctrl is Panel || ctrl is TableLayoutPanel || ctrl is GroupBox || ctrl is FlowLayoutPanel)
                {
                    ctrl.BackColor = backColor;
                    SetPanelBackgrounds(ctrl.Controls, backColor);
                }
            }
        }
    }

    public class Employee
    {
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public string Departman { get; set; }
        public string Pozisyon { get; set; }
        public string TC { get; set; }
        public string Telefon { get; set; }
        public decimal Maas { get; set; }
        public bool Izinli { get; set; }
        public int IzinGunu { get; set; }
        public DateTime IzinBaslangic { get; set; }
    }

    // Yuvarlak panel
    public class RoundedPanel : Panel
    {
        public int CornerRadius { get; set; } = 12;
        public Color ShadowColor { get; set; } = Color.LightGray;
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var rect = new System.Drawing.Rectangle(this.ClientRectangle.X, this.ClientRectangle.Y, this.ClientRectangle.Width, this.ClientRectangle.Height);
            rect.Inflate(-1, -1);
            using (var path = RoundedRect(rect, CornerRadius))
            using (var brush = new SolidBrush(this.BackColor))
            using (var shadow = new Pen(ShadowColor, 2))
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.DrawPath(shadow, path);
                e.Graphics.FillPath(brush, path);
            }
        }
        private System.Drawing.Drawing2D.GraphicsPath RoundedRect(System.Drawing.Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
    // Yuvarlak buton
    public class RoundedButton : Button
    {
        public int CornerRadius { get; set; } = 8;
        protected override void OnPaint(PaintEventArgs pevent)
        {
            var rect = new System.Drawing.Rectangle(this.ClientRectangle.X, this.ClientRectangle.Y, this.ClientRectangle.Width, this.ClientRectangle.Height);
            using (var path = RoundedRect(rect, CornerRadius))
            using (var brush = new SolidBrush(this.BackColor))
            using (var pen = new Pen(Color.LightGray, 1))
            {
                pevent.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                pevent.Graphics.FillPath(brush, path);
                pevent.Graphics.DrawPath(pen, path);
                TextRenderer.DrawText(pevent.Graphics, this.Text, this.Font, rect, this.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }
        private System.Drawing.Drawing2D.GraphicsPath RoundedRect(System.Drawing.Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    // YATAY DÖNEN YAZI KONTROLÜ
    public class RotatingTextLabel : Control
    {
        public string RotatingText { get; set; } = "Çalışan Takip Sistemine Hoş Geldiniz";
        public Font RotatingFont { get; set; } = new Font("Segoe UI", 18, FontStyle.Bold);
        public Color RotatingColor { get; set; } = Color.Black;
        public float RotationAngle { get; set; } = 0;

        public RotatingTextLabel()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.DoubleBuffered = true;
            this.Size = new Size(600, 50);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var textSize = TextRenderer.MeasureText(RotatingText, RotatingFont);
            var center = new PointF(this.Width / 2f, this.Height / 2f);

            using (var bmp = new Bitmap(textSize.Width, textSize.Height))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                TextRenderer.DrawText(g, RotatingText, RotatingFont, new System.Drawing.Rectangle(0, 0, textSize.Width, textSize.Height), RotatingColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                e.Graphics.TranslateTransform(center.X, center.Y);
                e.Graphics.RotateTransform(RotationAngle);
                e.Graphics.TranslateTransform(-textSize.Width / 2f, -textSize.Height / 2f);
                e.Graphics.DrawImage(bmp, 0, 0);
                e.Graphics.ResetTransform();
            }
        }
    }

    // YARIM ÇEMBERDE YATAY KAYAN YAZI KONTROLÜ
    public class SemiCircleMarqueeLabel : Control
    {
        public string MarqueeText { get; set; } = "Çalışan Takip Sistemine Hoş Geldiniz";
        public Font MarqueeFont { get; set; } = new Font("Segoe UI", 18, FontStyle.Bold);
        public Color MarqueeColor { get; set; } = Color.Black;
        public float RotationAngle { get; set; } = 0;

        public SemiCircleMarqueeLabel()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.DoubleBuffered = true;
            this.Size = new Size(600, 120);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            float radius = Math.Min(this.Width, this.Height) / 2f - 10;
            var center = new PointF(this.Width / 2f, this.Height / 2f + 20); // Alt yarıya kaydır

            float angleStep = 180f / MarqueeText.Length; // Sadece alt yarı
            for (int i = 0; i < MarqueeText.Length; i++)
            {
                float angle = 180 + RotationAngle + i * angleStep; // 180'den başla, alt yarı
                float rad = (float)(Math.PI * angle / 180.0);
                float x = center.X + (float)(radius * Math.Cos(rad));
                float y = center.Y + (float)(radius * Math.Sin(rad));
                var charSize = e.Graphics.MeasureString(MarqueeText[i].ToString(), MarqueeFont);
                // Her harfi yatay ve düz çiz
                e.Graphics.DrawString(MarqueeText[i].ToString(), MarqueeFont, new SolidBrush(MarqueeColor), x - charSize.Width / 2, y - charSize.Height / 2);
            }
        }
    }
}
