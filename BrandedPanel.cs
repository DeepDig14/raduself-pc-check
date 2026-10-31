using System.Drawing.Imaging;
using System.Reflection;

namespace RaduSelf.PCCheck;

sealed class BrandedPanel : FlowLayoutPanel
{
    readonly Image logo;
    public BrandedPanel()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RaduSelf.BrandLogo")
            ?? throw new InvalidOperationException("Brand logo resource missing.");
        using var original = Image.FromStream(stream);
        logo = new Bitmap(original);
        DoubleBuffered = true;
        ResizeRedraw = true;
    }
    protected override void OnPaintBackground(PaintEventArgs e)
    {
        base.OnPaintBackground(e);
        int inset = Math.Max(1, (int)Math.Round(12 * DeviceDpi / 96.0));
        int availableWidth = ClientSize.Width - 2 * inset;
        int availableHeight = ClientSize.Height - 2 * inset;
        if (availableWidth <= 0 || availableHeight <= 0) return;
        double scale = Math.Min((double)availableWidth / logo.Width, (double)availableHeight / logo.Height);
        int width = Math.Max(1, (int)Math.Floor(logo.Width * scale));
        int height = Math.Max(1, (int)Math.Floor(logo.Height * scale));
        var bounds = new Rectangle((ClientSize.Width - width) / 2, (ClientSize.Height - height) / 2, width, height);
        using var attributes = new ImageAttributes();
        var matrix = new ColorMatrix { Matrix33 = 0.22f };
        attributes.SetColorMatrix(matrix);
        e.Graphics.DrawImage(logo, bounds, 0, 0, logo.Width, logo.Height, GraphicsUnit.Pixel, attributes);
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing) logo.Dispose();
        base.Dispose(disposing);
    }
}
