using System;
using System.Drawing;
using System.Windows.Forms;

namespace CIEID
{
    public class MoveablePictureBox : PictureBox
    {
        private Point boxLocation;
        private bool isResizing;
        private float aspectRatio;
        private Image originalImage;

        private const int RESIZE_GRIP = 10;
        private const int MIN_WIDTH = 30;

        public MoveablePictureBox()
        {
            this.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        public void SetImage(Image img, int initialWidth)
        {
            this.originalImage = img;
            this.aspectRatio = (float)img.Width / img.Height;

            int initialHeight = (int)(initialWidth / aspectRatio);
            this.Image = new Bitmap(img, initialWidth, initialHeight);
            this.Width = initialWidth;
            this.Height = initialHeight;
        }

        private bool IsInResizeGrip(Point p)
        {
            return p.X >= this.Width - RESIZE_GRIP && p.Y >= this.Height - RESIZE_GRIP;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (IsInResizeGrip(e.Location))
            {
                isResizing = true;
            }
            else
            {
                isResizing = false;
                boxLocation = e.Location;
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.None)
            {
                this.Cursor = IsInResizeGrip(e.Location)
                    ? Cursors.SizeNWSE
                    : Cursors.SizeAll;
            }
            else if (e.Button == MouseButtons.Left)
            {
                if (isResizing)
                {
                    int newWidth = Math.Max(MIN_WIDTH, e.X);
                    int newHeight = (int)(newWidth / aspectRatio);
                    int minHeight = (int)(MIN_WIDTH / aspectRatio);
                    if (newHeight < minHeight) newHeight = minHeight;

                    if (this.Parent != null)
                    {
                        if (this.Left + newWidth > this.Parent.Width)
                            newWidth = this.Parent.Width - this.Left;
                        newHeight = (int)(newWidth / aspectRatio);

                        if (this.Top + newHeight > this.Parent.Height)
                        {
                            newHeight = this.Parent.Height - this.Top;
                            newWidth = (int)(newHeight * aspectRatio);
                        }
                    }

                    this.Width = newWidth;
                    this.Height = newHeight;
                }
                else
                {
                    int tempLeft = this.Left + (e.X - boxLocation.X);
                    int tempTop = this.Top + (e.Y - boxLocation.Y);

                    if (tempLeft < 0)
                        tempLeft = 0;
                    if (tempTop < 0)
                        tempTop = 0;

                    if (this.Parent != null)
                    {
                        if (tempLeft > this.Parent.Width - this.Width)
                            tempLeft = this.Parent.Width - this.Width;
                        if (tempTop > this.Parent.Height - this.Height)
                            tempTop = this.Parent.Height - this.Height;
                    }

                    this.Left = tempLeft;
                    this.Top = tempTop;
                }
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            isResizing = false;
            base.OnMouseUp(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            this.Cursor = Cursors.Default;
            base.OnMouseLeave(e);
        }
    }
}
