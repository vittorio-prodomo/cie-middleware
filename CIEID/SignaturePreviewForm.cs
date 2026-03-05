using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CIEID
{
    public class SignaturePreviewForm : Form
    {
        private PdfPreview pdfPreview;
        private Panel previewPanel;
        private Label fileNameLabel;
        private Button upButton;
        private Button downButton;
        private Button proceedButton;
        private Button cancelButton;
        private string pdfPath;
        private string signImagePath;
        private bool isRefreshing;

        public Dictionary<string, float> SignImageInfos { get; private set; }
        public string SignImagePath => signImagePath;

        public SignaturePreviewForm(string pdfPath, string signImagePath)
        {
            this.pdfPath = pdfPath;
            this.signImagePath = signImagePath;
            InitializeFormComponents();
            this.Load += (s, e) => LoadPdfPreview();
        }

        private void InitializeFormComponents()
        {
            this.Text = "Anteprima firma grafometrica";
            this.MinimumSize = new Size(600, 450);
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;

            // File name label at top
            fileNameLabel = new Label();
            fileNameLabel.Text = System.IO.Path.GetFileName(pdfPath);
            fileNameLabel.AutoEllipsis = true;
            fileNameLabel.Dock = DockStyle.Top;
            fileNameLabel.Height = 30;
            fileNameLabel.TextAlign = ContentAlignment.MiddleLeft;
            fileNameLabel.Padding = new Padding(10, 0, 10, 0);
            fileNameLabel.Font = new Font(this.Font.FontFamily, 9f);

            // Bottom button panel
            Panel buttonPanel = new Panel();
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.Height = 50;
            buttonPanel.Padding = new Padding(10, 5, 10, 5);

            upButton = new Button();
            upButton.Text = "Pag. \u25B2";
            upButton.Size = new Size(80, 35);
            upButton.Location = new Point(10, 7);
            upButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            upButton.Click += UpButton_Click;

            downButton = new Button();
            downButton.Text = "Pag. \u25BC";
            downButton.Size = new Size(80, 35);
            downButton.Location = new Point(95, 7);
            downButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            downButton.Click += DownButton_Click;

            cancelButton = new Button();
            cancelButton.Text = "Annulla";
            cancelButton.Size = new Size(100, 35);
            cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cancelButton.Click += CancelButton_Click;

            proceedButton = new Button();
            proceedButton.Text = "PROSEGUI";
            proceedButton.Size = new Size(120, 35);
            proceedButton.BackColor = Color.CornflowerBlue;
            proceedButton.ForeColor = Color.White;
            proceedButton.FlatStyle = FlatStyle.Flat;
            proceedButton.FlatAppearance.BorderSize = 0;
            proceedButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            proceedButton.Click += ProceedButton_Click;

            buttonPanel.Controls.AddRange(new Control[] { upButton, downButton, cancelButton, proceedButton });

            // Preview panel fills remaining space
            previewPanel = new Panel();
            previewPanel.Dock = DockStyle.Fill;
            previewPanel.BackColor = SystemColors.ButtonFace;
            previewPanel.Padding = new Padding(10);
            previewPanel.Resize += PreviewPanel_Resize;

            // Add controls in correct order for docking (Fill must be added first)
            this.Controls.Add(previewPanel);
            this.Controls.Add(buttonPanel);
            this.Controls.Add(fileNameLabel);

            this.CancelButton = cancelButton;

            // Position right-anchored buttons after panel is sized
            buttonPanel.Layout += (s, e) =>
            {
                cancelButton.Location = new Point(buttonPanel.ClientSize.Width - 240, 7);
                proceedButton.Location = new Point(buttonPanel.ClientSize.Width - 130, 7);
            };
        }

        private void LoadPdfPreview()
        {
            pdfPreview = new PdfPreview(previewPanel, pdfPath, signImagePath);
            bool singlePage = pdfPreview.getPdfPages() <= 1;
            upButton.Enabled = !singlePage;
            downButton.Enabled = !singlePage;
        }

        private void PreviewPanel_Resize(object sender, EventArgs e)
        {
            if (isRefreshing || pdfPreview == null) return;
            isRefreshing = true;
            try
            {
                pdfPreview.RefreshPreview();
            }
            finally
            {
                isRefreshing = false;
            }
        }

        private void UpButton_Click(object sender, EventArgs e)
        {
            pdfPreview?.pageUp();
        }

        private void DownButton_Click(object sender, EventArgs e)
        {
            pdfPreview?.pageDown();
        }

        private void ProceedButton_Click(object sender, EventArgs e)
        {
            SignImageInfos = pdfPreview.getSignImageInfos();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (pdfPreview != null)
            {
                pdfPreview.pdfPreviewRemoveObjects();
                pdfPreview.freeTempFolder();
            }
            base.OnFormClosing(e);
        }
    }
}
