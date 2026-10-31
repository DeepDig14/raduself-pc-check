using System.Drawing.Drawing2D;
namespace RaduSelf.PCCheck;

sealed class NavigationButton : Button
{
    public NavigationButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Cursor = Cursors.Hand;
        DoubleBuffered = true;
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.Clear(Parent?.BackColor ?? BackColor);
        int r = Math.Max(8, (int)(12 * DeviceDpi / 96f));
        var bounds = new Rectangle(0,0,Math.Max(r,Width-1),Math.Max(r,Height-1));
        using var shape = new GraphicsPath();
        shape.AddArc(bounds.Left,bounds.Top,r,r,180,90);
        shape.AddArc(bounds.Right-r,bounds.Top,r,r,270,90);
        shape.AddArc(bounds.Right-r,bounds.Bottom-r,r,r,0,90);
        shape.AddArc(bounds.Left,bounds.Bottom-r,r,r,90,90); shape.CloseFigure();
        using var brush = new SolidBrush(BackColor); e.Graphics.FillPath(brush,shape);
        TextRenderer.DrawText(e.Graphics,Text,Font,new Rectangle(14,0,Width-24,Height),ForeColor,TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);
        if(Focused && ShowFocusCues) ControlPaint.DrawFocusRectangle(e.Graphics,Rectangle.Inflate(bounds,-5,-5),ForeColor,BackColor);
    }
    protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); Invalidate(); }
    protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); Invalidate(); }
}
