namespace RaduSelf.PCCheck;

sealed class PageHost : Panel
{
    public List<Panel> Pages { get; } = [];
    public int SelectedIndex { get; private set; }
    public void AddPage(Panel page)
    {
        page.Dock = DockStyle.Fill;
        page.BackColor = Color.FromArgb(119, 181, 254);
        Pages.Add(page); Controls.Add(page); page.Visible = Pages.Count == 1;
    }
    public void SelectPage(int index)
    {
        if (index < 0 || index >= Pages.Count) return;
        SelectedIndex = index;
        for (int i = 0; i < Pages.Count; i++) Pages[i].Visible = i == index;
        Pages[index].BringToFront();
    }
    public void ClearPages()
    {
        foreach (var page in Pages) page.Dispose();
        Pages.Clear(); Controls.Clear();
    }
}
