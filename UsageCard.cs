using System.Drawing.Drawing2D;
namespace RaduSelf.PCCheck;

sealed class UsageCard : Control
{
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public string Heading { get; set; } = "";
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public string MainValue { get; set; } = "";
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public string Detail { get; set; } = "";
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public double Fraction { get; set; }
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool Warning { get; set; }
    public UsageCard()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.ResizeRedraw, true);
        Size = new Size(320, 160); Margin = new Padding(0, 0, 12, 12);
        AccessibleRole = AccessibleRole.StaticText;
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        float scale = DeviceDpi / 96f;
        int pad = (int)(16 * scale), radius = (int)(18 * scale);
        var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(1, 1, Math.Max(2, Width - 3), Math.Max(2, Height - 3));
        using var shape = new GraphicsPath();
        shape.AddArc(rect.Left, rect.Top, radius, radius, 180, 90);
        shape.AddArc(rect.Right-radius, rect.Top, radius, radius, 270, 90);
        shape.AddArc(rect.Right-radius, rect.Bottom-radius, radius, radius, 0, 90);
        shape.AddArc(rect.Left, rect.Bottom-radius, radius, radius, 90, 90); shape.CloseFigure();
        using var fill = new SolidBrush(Color.FromArgb(136,191,255));
        using var border = new Pen(Color.FromArgb(93,158,234));
        g.FillPath(fill, shape); g.DrawPath(border, shape);
        var navy = Color.FromArgb(20,49,87);
        using var small = new Font("Segoe UI", 10);
        using var large = new Font("Segoe UI", 18, FontStyle.Bold);
        int line = TextRenderer.MeasureText("Ag", small).Height;
        int mainLine = TextRenderer.MeasureText("Ag", large).Height;
        var flags = TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;
        TextRenderer.DrawText(g, Heading, small, new Rectangle(pad,pad,Width-pad*2,line),navy,flags);
        int mainY = pad + line + (int)(6*scale);
        TextRenderer.DrawText(g, MainValue, large, new Rectangle(pad,mainY,Width-pad*2,mainLine),navy,flags);
        int detailY = mainY + mainLine + (int)(3*scale);
        TextRenderer.DrawText(g, Detail, small, new Rectangle(pad,detailY,Width-pad*2,line),navy,flags);
        var bar = new Rectangle(pad, Height-pad-(int)(7*scale), Math.Max(1,Width-pad*2), (int)(7*scale));
        using var track = new SolidBrush(Color.FromArgb(100,166,242));
        using var accent = new SolidBrush(Warning ? Color.FromArgb(169,71,0) : Color.FromArgb(20,68,130));
        g.FillRectangle(track,bar);
        g.FillRectangle(accent,bar.X,bar.Y,(int)(bar.Width*Math.Clamp(Fraction,0,1)),bar.Height);
    }
}
