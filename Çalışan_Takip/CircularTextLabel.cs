using System;
using System.Drawing;
using System.Windows.Forms;

namespace Çalışan_Takip
{
    public class CircularTextLabel : Control
    {
        public string CircularText { get; set; } = "Çalışan Takip Sistemine Hoş Geldiniz";
        public System.Drawing.Font CircularFont { get; set; } = new System.Drawing.Font("Segoe UI", 18, FontStyle.Bold);
        public Color CircularColor { get; set; } = Color.Black;
        public float RotationAngle { get; set; } = 0; // Derece cinsinden

        public CircularTextLabel()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.DoubleBuffered = true;
            this.Size = new Size(600, 80);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            float radius = Math.Min(this.Width, this.Height) / 2f - 10;
            var center = new PointF(this.Width / 2f, this.Height / 2f);

            float angleStep = 360f / CircularText.Length;
            for (int i = 0; i < CircularText.Length; i++)
            {
                float angle = RotationAngle + i * angleStep;
                float rad = (float)(Math.PI * angle / 180.0);
                float x = center.X + (float)(radius * Math.Cos(rad));
                float y = center.Y + (float)(radius * Math.Sin(rad));
                e.Graphics.TranslateTransform(x, y);
                e.Graphics.RotateTransform(angle + 90);
                using (Brush b = new SolidBrush(CircularColor))
                {
                    e.Graphics.DrawString(CircularText[i].ToString(), CircularFont, b, 0, 0);
                }
                e.Graphics.ResetTransform();
            }
        }
    }
} 