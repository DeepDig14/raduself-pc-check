using System.Drawing.Drawing2D;

namespace RaduSelf.PCCheck;

// Keep WinForms scrolling (wheel, focus and AutoScrollPosition), while placing
// its system scrollbar outside the clipped viewport. A themed control mirrors it.
sealed class BlueScrollFrame : Panel
{
    readonly FlowLayoutPanel content;
    readonly BlueScrollThumb scrollbar;
    bool arranging;
    internal BlueScrollFrame(FlowLayoutPanel content, string accessibleName)
    {
        this.content = content;
        DoubleBuffered = true;
        BackColor = Color.FromArgb(119,181,254);
        content.Dock = DockStyle.None;
        scrollbar = new BlueScrollThumb(content) { AccessibleName = accessibleName };
        Controls.Add(content); Controls.Add(scrollbar);
        content.Scroll += (_,_) => scrollbar.Invalidate();
        content.Layout += (_,_) => scrollbar.Invalidate();
        content.SizeChanged += (_,_) => scrollbar.Invalidate();
    }
    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);
        if (arranging || content == null || scrollbar == null) return;
        arranging = true;
        try
        {
            int nativeWidth = SystemInformation.VerticalScrollBarWidth;
            // The entire native vertical bar lies beyond this panel's right edge.
            content.SetBounds(0,0,ClientSize.Width + nativeWidth,ClientSize.Height);
            int width = Math.Max(14,(int)(16 * DeviceDpi / 96f));
            scrollbar.SetBounds(Math.Max(0,ClientSize.Width-width),0,width,ClientSize.Height);
            scrollbar.BringToFront(); scrollbar.Invalidate();
        }
        finally { arranging = false; }
    }
}

sealed class BlueScrollThumb : Control
{
    readonly FlowLayoutPanel content;
    bool dragging, hovering;
    int dragOffset;
    internal BlueScrollThumb(FlowLayoutPanel content)
    {
        this.content = content;
        DoubleBuffered = true; TabStop = true; Cursor = Cursors.Hand;
        AccessibleRole = AccessibleRole.ScrollBar;
        SetStyle(ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
        BackColor = Color.FromArgb(119,181,254);
    }
    int Range() => Math.Max(0, content.DisplayRectangle.Height - content.ClientSize.Height);
    int Position() => Math.Clamp(-content.AutoScrollPosition.Y,0,Range());
    Rectangle Thumb()
    {
        int inset = Math.Max(3,(int)(4*DeviceDpi/96f));
        int track = Math.Max(1,Height-inset*2);
        int length = Math.Min(track, Math.Max((int)(36*DeviceDpi/96f),(int)(track*(double)Math.Max(1,content.ClientSize.Height)/Math.Max(1,content.DisplayRectangle.Height))));
        int y = inset + (Range()==0 ? 0 : (int)((track-length)*(double)Position()/Range()));
        return new Rectangle(inset,y,Math.Max(4,Width-inset*2),length);
    }
    void SetPosition(int value)
    {
        content.AutoScrollPosition = new Point(0,Math.Clamp(value,0,Range()));
        Invalidate();
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(Color.FromArgb(119,181,254));
        if(Range()==0) return;
        var rect = Thumb();
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = new GraphicsPath();
        int diameter = Math.Min(rect.Width,rect.Height);
        path.AddArc(rect.Left,rect.Top,diameter,diameter,180,180);
        path.AddArc(rect.Left,rect.Bottom-diameter,diameter,diameter,0,180); path.CloseFigure();
        using var fill = new SolidBrush(dragging || hovering ? Color.FromArgb(20,49,87) : Color.FromArgb(89,153,222));
        e.Graphics.FillPath(fill,path);
        if(Focused && ShowFocusCues) ControlPaint.DrawFocusRectangle(e.Graphics,new Rectangle(1,1,Width-2,Height-2));
    }
    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if(e.Button!=MouseButtons.Left || Range()==0) return;
        Focus(); var thumb=Thumb();
        if(thumb.Contains(e.Location)) { dragging=true; dragOffset=e.Y-thumb.Y; Capture=true; }
        else SetPosition(Position()+(e.Y<thumb.Top?-1:1)*content.ClientSize.Height);
        Invalidate();
    }
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if(!dragging)return;
        int inset=Math.Max(3,(int)(4*DeviceDpi/96f));
        int travel=Math.Max(1,Height-inset*2-Thumb().Height);
        SetPosition((int)Math.Round((e.Y-dragOffset-inset)*(double)Range()/travel));
    }
    protected override void OnMouseUp(MouseEventArgs e) { base.OnMouseUp(e); dragging=false; Capture=false; Invalidate(); }
    protected override void OnMouseCaptureChanged(EventArgs e) { base.OnMouseCaptureChanged(e); if(!Capture) dragging=false; Invalidate(); }
    protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); hovering=true; Invalidate(); }
    protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); hovering=false; Invalidate(); }
    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        int lines=SystemInformation.MouseWheelScrollLines;
        int step=lines<0?content.ClientSize.Height:Math.Max(1,lines)*Font.Height;
        SetPosition(Position()-(int)Math.Round(e.Delta/120d*step));
    }
    protected override bool IsInputKey(Keys keyData) => (keyData & Keys.KeyCode) is Keys.Up or Keys.Down or Keys.PageUp or Keys.PageDown or Keys.Home or Keys.End || base.IsInputKey(keyData);
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        int value=Position();
        switch(e.KeyCode)
        {
            case Keys.Up: value-=Font.Height*3; break;
            case Keys.Down: value+=Font.Height*3; break;
            case Keys.PageUp: value-=content.ClientSize.Height; break;
            case Keys.PageDown: value+=content.ClientSize.Height; break;
            case Keys.Home: value=0; break;
            case Keys.End: value=Range(); break;
            default:return;
        }
        SetPosition(value); e.Handled=true; e.SuppressKeyPress=true;
    }
}
