using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CIEID
{
    public class MoveablePictureBox : PictureBox
    {
        private enum ResizeHandle
        {
            None,
            TopLeft, TopCenter, TopRight,
            MiddleLeft, MiddleRight,
            BottomLeft, BottomCenter, BottomRight
        }

        private static readonly ResizeHandle[] HandleOrder =
        {
            ResizeHandle.TopLeft, ResizeHandle.TopCenter, ResizeHandle.TopRight,
            ResizeHandle.MiddleLeft, ResizeHandle.MiddleRight,
            ResizeHandle.BottomLeft, ResizeHandle.BottomCenter, ResizeHandle.BottomRight
        };

        private Point boxLocation;
        private ResizeHandle activeHandle = ResizeHandle.None;
        private Point dragStartMouse;
        private Rectangle dragStartBounds;
        private float originalAspectRatio;
        private float activeAspectRatio;
        private Image originalImage;

        private const int HANDLE_SIZE = 10;
        private const int HANDLE_RADIUS = 5;
        private const int HIT_MARGIN = 8;
        private const int MIN_DIM = 25;

        public MoveablePictureBox()
        {
            this.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        public void SetImage(Image img, int initialWidth)
        {
            this.originalImage = img;
            this.originalAspectRatio = (float)img.Width / img.Height;

            int initialHeight = (int)(initialWidth / originalAspectRatio);
            this.Image = new Bitmap(img, initialWidth, initialHeight);
            this.Width = initialWidth;
            this.Height = initialHeight;
        }

        private Point[] GetHandlePoints()
        {
            int w = this.Width;
            int h = this.Height;
            return new Point[]
            {
                new Point(0, 0),           // TopLeft
                new Point(w / 2, 0),       // TopCenter
                new Point(w - 1, 0),       // TopRight
                new Point(0, h / 2),       // MiddleLeft
                new Point(w - 1, h / 2),   // MiddleRight
                new Point(0, h - 1),       // BottomLeft
                new Point(w / 2, h - 1),   // BottomCenter
                new Point(w - 1, h - 1)    // BottomRight
            };
        }

        private ResizeHandle GetHandleAt(Point p)
        {
            Point[] pts = GetHandlePoints();
            for (int i = 0; i < pts.Length; i++)
            {
                if (Math.Abs(p.X - pts[i].X) <= HIT_MARGIN && Math.Abs(p.Y - pts[i].Y) <= HIT_MARGIN)
                    return HandleOrder[i];
            }
            return ResizeHandle.None;
        }

        private static Cursor CursorForHandle(ResizeHandle h)
        {
            switch (h)
            {
                case ResizeHandle.TopLeft:
                case ResizeHandle.BottomRight:
                    return Cursors.SizeNWSE;
                case ResizeHandle.TopRight:
                case ResizeHandle.BottomLeft:
                    return Cursors.SizeNESW;
                case ResizeHandle.TopCenter:
                case ResizeHandle.BottomCenter:
                    return Cursors.SizeNS;
                case ResizeHandle.MiddleLeft:
                case ResizeHandle.MiddleRight:
                    return Cursors.SizeWE;
                default:
                    return Cursors.SizeAll;
            }
        }

        private static bool IsCornerHandle(ResizeHandle h)
        {
            return h == ResizeHandle.TopLeft || h == ResizeHandle.TopRight ||
                   h == ResizeHandle.BottomLeft || h == ResizeHandle.BottomRight;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            activeHandle = GetHandleAt(e.Location);
            if (activeHandle != ResizeHandle.None)
            {
                dragStartMouse = this.Parent.PointToClient(this.PointToScreen(e.Location));
                dragStartBounds = this.Bounds;
                activeAspectRatio = (this.Height > 0) ? (float)this.Width / this.Height : 1f;
            }
            else
            {
                boxLocation = e.Location;
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.None)
            {
                this.Cursor = CursorForHandle(GetHandleAt(e.Location));
            }
            else if (e.Button == MouseButtons.Left)
            {
                if (activeHandle != ResizeHandle.None)
                    DoResize(e);
                else
                    DoDrag(e);
            }
            base.OnMouseMove(e);
        }

        private void DoResize(MouseEventArgs e)
        {
            Point currentMouse = this.Parent.PointToClient(this.PointToScreen(e.Location));
            int dx = currentMouse.X - dragStartMouse.X;
            int dy = currentMouse.Y - dragStartMouse.Y;

            int newLeft = dragStartBounds.Left;
            int newTop = dragStartBounds.Top;
            int newWidth = dragStartBounds.Width;
            int newHeight = dragStartBounds.Height;

            bool lockAspect = IsCornerHandle(activeHandle);

            switch (activeHandle)
            {
                case ResizeHandle.BottomRight:
                    newWidth = dragStartBounds.Width + dx;
                    newHeight = lockAspect ? (int)(newWidth / activeAspectRatio) : dragStartBounds.Height + dy;
                    break;
                case ResizeHandle.BottomLeft:
                    newWidth = dragStartBounds.Width - dx;
                    newHeight = lockAspect ? (int)(newWidth / activeAspectRatio) : dragStartBounds.Height + dy;
                    newLeft = dragStartBounds.Right - newWidth;
                    break;
                case ResizeHandle.TopRight:
                    newWidth = dragStartBounds.Width + dx;
                    newHeight = lockAspect ? (int)(newWidth / activeAspectRatio) : dragStartBounds.Height - dy;
                    newTop = dragStartBounds.Bottom - newHeight;
                    break;
                case ResizeHandle.TopLeft:
                    newWidth = dragStartBounds.Width - dx;
                    newHeight = lockAspect ? (int)(newWidth / activeAspectRatio) : dragStartBounds.Height - dy;
                    newLeft = dragStartBounds.Right - newWidth;
                    newTop = dragStartBounds.Bottom - newHeight;
                    break;
                case ResizeHandle.MiddleRight:
                    newWidth = dragStartBounds.Width + dx;
                    break;
                case ResizeHandle.MiddleLeft:
                    newWidth = dragStartBounds.Width - dx;
                    newLeft = dragStartBounds.Right - newWidth;
                    break;
                case ResizeHandle.BottomCenter:
                    newHeight = dragStartBounds.Height + dy;
                    break;
                case ResizeHandle.TopCenter:
                    newHeight = dragStartBounds.Height - dy;
                    newTop = dragStartBounds.Bottom - newHeight;
                    break;
            }

            // Enforce minimums
            if (newWidth < MIN_DIM)
            {
                newWidth = MIN_DIM;
                if (activeHandle == ResizeHandle.BottomLeft || activeHandle == ResizeHandle.TopLeft || activeHandle == ResizeHandle.MiddleLeft)
                    newLeft = dragStartBounds.Right - MIN_DIM;
                if (lockAspect)
                {
                    newHeight = (int)(MIN_DIM / activeAspectRatio);
                    if (activeHandle == ResizeHandle.TopLeft || activeHandle == ResizeHandle.TopRight)
                        newTop = dragStartBounds.Bottom - newHeight;
                }
            }
            if (newHeight < MIN_DIM)
            {
                newHeight = MIN_DIM;
                if (activeHandle == ResizeHandle.TopLeft || activeHandle == ResizeHandle.TopRight || activeHandle == ResizeHandle.TopCenter)
                    newTop = dragStartBounds.Bottom - MIN_DIM;
                if (lockAspect)
                {
                    newWidth = (int)(MIN_DIM * activeAspectRatio);
                    if (activeHandle == ResizeHandle.TopLeft || activeHandle == ResizeHandle.BottomLeft)
                        newLeft = dragStartBounds.Right - newWidth;
                }
            }

            // Clamp to parent bounds
            if (this.Parent != null)
            {
                if (newLeft < 0) newLeft = 0;
                if (newTop < 0) newTop = 0;
                if (newLeft + newWidth > this.Parent.Width)
                    newWidth = this.Parent.Width - newLeft;
                if (newTop + newHeight > this.Parent.Height)
                    newHeight = this.Parent.Height - newTop;
            }

            this.SetBounds(newLeft, newTop, newWidth, newHeight);
        }

        private void DoDrag(MouseEventArgs e)
        {
            int tempLeft = this.Left + (e.X - boxLocation.X);
            int tempTop = this.Top + (e.Y - boxLocation.Y);

            if (tempLeft < 0) tempLeft = 0;
            if (tempTop < 0) tempTop = 0;

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

        protected override void OnMouseUp(MouseEventArgs e)
        {
            activeHandle = ResizeHandle.None;
            base.OnMouseUp(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            this.Cursor = Cursors.Default;
            base.OnMouseLeave(e);
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            // Reset to original image aspect ratio, keeping current width
            int newHeight = (int)(this.Width / originalAspectRatio);
            if (newHeight < MIN_DIM) newHeight = MIN_DIM;

            // Clamp to parent bounds
            if (this.Parent != null && this.Top + newHeight > this.Parent.Height)
                newHeight = this.Parent.Height - this.Top;

            int newWidth = (int)(newHeight * originalAspectRatio);
            this.SetBounds(this.Left, this.Top, newWidth, newHeight);
            base.OnDoubleClick(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Point[] pts = GetHandlePoints();

            using (var fillBrush = new SolidBrush(Color.FromArgb(66, 133, 244)))
            using (var borderPen = new Pen(Color.White, 2f))
            {
                foreach (var pt in pts)
                {
                    var rect = new Rectangle(
                        pt.X - HANDLE_RADIUS, pt.Y - HANDLE_RADIUS,
                        HANDLE_SIZE, HANDLE_SIZE);
                    e.Graphics.FillEllipse(fillBrush, rect);
                    e.Graphics.DrawEllipse(borderPen, rect);
                }
            }
        }
    }
}
