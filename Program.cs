using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Security.Principal;

namespace RaduSelf.PCCheck;
static class Program
{
    [STAThread] static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        try
        {
            bool systemCleanup=args.Contains("--system-cleanup");
            if(systemCleanup && !new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                MessageBox.Show("Sunt necesare drepturi de administrator / Administrator rights are required.","RADUSELF PC Check");
                return;
            }
            Application.Run(new MainWindow(systemCleanup,args.Contains("--english")));
        }
        catch (Exception ex) { MessageBox.Show("Aplicația nu a putut porni / Application could not start.\n\n" + ex.Message, "RADUSELF PC Check", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}
sealed class MainWindow : Form
{
    bool english;
    readonly bool systemCleanup;
    readonly PageHost tabs = new() { Dock = DockStyle.Fill };
    readonly string pref = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RaduSelfPCCheck", "language.txt");
    string T(string ro, string en) => english ? en : ro;
    public MainWindow(bool systemCleanup=false, bool startEnglish=false)
    {
        this.systemCleanup=systemCleanup;
        try { english = File.Exists(pref) && File.ReadAllText(pref) == "en"; } catch { }
        if(systemCleanup)english=startEnglish;
        Text = "RADUSELF PC Check · 0.4.10"+(systemCleanup?" · Administrator":""); Width = 1180; Height = 780; MinimumSize = new Size(1060, 700);
        using(var iconStream=typeof(MainWindow).Assembly.GetManifestResourceStream("RaduSelf.AppIcon"))
        {
            if(iconStream!=null)
            {
                using var appIcon=new Icon(iconStream,new Size(32,32));
                Icon=(Icon)appIcon.Clone();
            }
        }
        Font = new Font("Segoe UI", 10); BackColor = Color.FromArgb(119,181,254);
        Build();
    }
    void Build()
    {
        int selected = tabs.SelectedIndex;
        SuspendLayout();
        tabs.Parent?.Controls.Remove(tabs);
        tabs.ClearPages();
        foreach (Control control in Controls.Cast<Control>().ToArray())
            if (control != tabs) { Controls.Remove(control); control.Dispose(); }
        Controls.Remove(tabs);
        var header = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(24,16,24,16), ColumnCount = 2, RowCount = 2, BackColor = Color.FromArgb(119,181,254) };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100)); header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        header.RowStyles.Add(new RowStyle(SizeType.AutoSize)); header.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        header.Controls.Add(new Label { Text = "RADUSELF PC Check", AutoSize = true, ForeColor = Color.FromArgb(20,49,87), Font = new Font("Segoe UI", 19, FontStyle.Bold), Margin = new Padding(0,0,12,5) },0,0);
        header.Controls.Add(new Label { Text = T("Instrumente pentru un Windows mai ușor de gestionat", "Tools for a more manageable Windows"), AutoSize = true, ForeColor = Color.FromArgb(20,49,87), Font = new Font("Segoe UI", 10), Margin = new Padding(0,0,12,0) },0,1);
        var language = Button(english ? "Română" : "English", () => { english = !english; try { Directory.CreateDirectory(Path.GetDirectoryName(pref)!); File.WriteAllText(pref, english ? "en" : "ro"); } catch { } Build(); });
        language.BackColor = Color.FromArgb(20,49,87); language.ForeColor = Color.White; language.FlatAppearance.BorderSize = 0; language.Cursor = Cursors.Hand;
        var headerActions = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Anchor = AnchorStyles.Top | AnchorStyles.Right, Margin = new Padding(0) };
        headerActions.Controls.Add(Button(T("Susține", "Support"), Donation));
        headerActions.Controls.Add(language);
        header.Controls.Add(headerActions,1,0); header.SetRowSpan(headerActions,2);
        var sidebar = new Panel { Dock = DockStyle.Left, Width = 238, BackColor = Color.FromArgb(20,49,87), Padding = new Padding(12) };
        var navigation = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        var brand = new BrandedPanel { Dock = DockStyle.Bottom, Height = 150, BackColor = Color.FromArgb(119,181,254) };
        sidebar.Controls.Add(navigation); sidebar.Controls.Add(brand);
        var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) }; body.Controls.Add(tabs);
        Controls.Add(body); Controls.Add(sidebar); Controls.Add(header);
        if(systemCleanup) CleanupPage();
        else
        {
            QuickCheckPage(); ResourcesPage(); CatalogPage(T("Scurtături", "Shortcuts"), Catalog.Shortcuts);
            CatalogPage(T("CMD și PowerShell", "CMD & PowerShell"), Catalog.Commands);
            AppsPage(); CleanupPage(); SettingsPage(); SecurityPage(); AboutPage();
        }
        var buttons = new List<Button>();
        void Select(int index)
        {
            tabs.SelectPage(index);
            for (int i = 0; i < buttons.Count; i++) { buttons[i].BackColor = i == index ? Color.FromArgb(35,105,180) : sidebar.BackColor; buttons[i].ForeColor = Color.White; }
        }
        for (int i = 0; i < tabs.Pages.Count; i++)
        {
            int index = i;
            var button = Button(tabs.Pages[i].Text, () => Select(index));
            button.AutoSize = false; button.Size = new Size(210, 42); button.Margin = new Padding(0,0,0,6); button.TextAlign = ContentAlignment.MiddleLeft;
            button.FlatAppearance.BorderSize = 0; buttons.Add(button); navigation.Controls.Add(button);
        }
        Select(Math.Min(selected, tabs.Pages.Count - 1));
        ApplyPalette(body);
        ResumeLayout(true);
    }
    static void ApplyPalette(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            if(child is Button button)
            {
                button.ForeColor=button.Tag is ActionButtonState { Empty:true } ? Color.FromArgb(20,49,87) : Color.White;
                button.BackColor=button.Tag is ActionButtonState { Empty:true } ? Color.FromArgb(245,207,102) : button.Tag is ActionButtonState { Done:true } ? Color.FromArgb(36,115,75) : Color.FromArgb(20,49,87);
            }
            else
            {
                child.ForeColor = Color.FromArgb(20,49,87);
                child.BackColor = Color.FromArgb(119,181,254);
            }
            if (child is Label textLabel) textLabel.UseMnemonic=false;
            if (child is ListView list) { list.BorderStyle = BorderStyle.None; list.GridLines = false; }
            if (child is DataGridView grid)
            {
                grid.BackgroundColor = Color.FromArgb(119,181,254);
                grid.DefaultCellStyle.BackColor = Color.FromArgb(119,181,254);
                grid.DefaultCellStyle.ForeColor = Color.FromArgb(20,49,87);
                grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(20,68,130);
                grid.DefaultCellStyle.SelectionForeColor = Color.White;
                grid.GridColor = Color.FromArgb(95,160,235);
            }
            ApplyPalette(child);
        }
    }
    Button Button(string title, Action action)
    {
        var b = new Button { UseMnemonic = false, Text = title, AutoSize = true, MinimumSize = new Size(140,40), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(20,49,87), ForeColor = Color.White, Margin = new Padding(5,5,5,9) };
        b.FlatAppearance.BorderSize=0; b.Cursor=Cursors.Hand; b.Tag=new ActionButtonState(title,false);
        b.Click += (_,_) => { SetButtonCompleted(b,false); try { action(); } catch (Exception ex) { MessageBox.Show(this, ex.Message, T("Operația nu a reușit", "Operation failed")); } }; return b;
    }
    sealed record ActionButtonState(string Title, bool Done, bool Empty=false);
    void SetButtonCompleted(Button button, bool done)
    {
        if(button.IsDisposed)return;
        string title=button.Tag is ActionButtonState state?state.Title:button.Text;
        button.Tag=new ActionButtonState(title,done);
        button.Text=title+(done?"  ✓":"");
        button.BackColor=done?Color.FromArgb(36,115,75):Color.FromArgb(20,49,87);
        button.ForeColor=Color.White;
        button.AccessibleDescription=done?T("Acțiune finalizată sau confirmată", "Action completed or confirmed"):"";
    }
    void SetButtonNothingToClean(Button button)
    {
        if(button.IsDisposed)return;
        string title=button.Tag is ActionButtonState state?state.Title:button.Text;
        button.Tag=new ActionButtonState(title,false,true);
        button.Text=title+T(" — Nimic de curățat", " — Nothing to clean");
        button.BackColor=Color.FromArgb(245,207,102);
        button.ForeColor=Color.FromArgb(20,49,87);
        button.AccessibleDescription=T("Analiză finalizată: nimic de curățat. Nu s-au șters fișiere.", "Analysis complete: nothing to clean. No files were deleted.");
    }
    FlowLayoutPanel Page(string title)
    {
        var tab = new Panel { Text = title }; var p = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(18) }; var scrollFrame = new BlueScrollFrame(p, T("Derulare pagină", "Page scrolling")) { Dock = DockStyle.Fill }; tab.Controls.Add(scrollFrame); tabs.AddPage(tab);
        p.Controls.Add(new Label { Text = title, AutoSize = true, Font = new Font("Segoe UI", 19, FontStyle.Bold), ForeColor = Color.FromArgb(20,49,87), Margin = new Padding(5,0,5,18) });
        p.SizeChanged += (_,_) => { int width = Math.Max(200, p.ClientSize.Width - 65); foreach (Control child in p.Controls) { if (child is Label label) label.MaximumSize = new Size(width,0); else if (child is ListView) child.Width = width; } };
        return p;
    }
    void Note(FlowLayoutPanel p, string text) => p.Controls.Add(new Label { Text = text, AutoSize = true, MaximumSize = new Size(960,0), Margin = new Padding(5,10,5,14) });
    static void Open(string target) => Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });
    void QuickCheckPage()
    {
        var p = Page(T("Verificare rapidă", "Quick check"));
        Note(p,T("Un instantaneu al memoriei RAM și al spațiului de pe discurile locale. Nu este un test hardware sau o verificare antivirus. Nu modifică setările și nu șterge fișiere.","A snapshot of RAM and local disk space. This is not a hardware test or an antivirus scan. It does not change settings or delete files."));
        var status = new Label { Text=T("Apasă butonul pentru a începe.","Press the button to start."), AutoSize=true, MaximumSize=new Size(800,0), Margin=new Padding(5,8,5,12) };
        var results = new FlowLayoutPanel { Width=800, AutoSize=true, AutoSizeMode=AutoSizeMode.GrowAndShrink, FlowDirection=FlowDirection.TopDown, WrapContents=false, Margin=new Padding(5,0,5,12) };
        void Fit()
        {
            int width=Math.Max(240,p.ClientSize.Width-65); results.Width=width; status.MaximumSize=new Size(width,0);
            foreach(Control item in results.Controls) item.MaximumSize=new Size(width-12,0);
        }
        void Show(string title, string detail)
        {
            results.Controls.Add(new Label { Text=title, AutoSize=true, Font=new Font("Segoe UI",11,FontStyle.Bold), ForeColor=Color.FromArgb(20,49,87), BackColor=Color.FromArgb(119,181,254), Margin=new Padding(0,8,0,4) });
            results.Controls.Add(new Label { Text=detail, AutoSize=true, ForeColor=Color.FromArgb(20,49,87), BackColor=Color.FromArgb(119,181,254), Margin=new Padding(0,0,0,12) });
        }
        Button? run=null;
        run=Button(T("Verifică acum", "Check now"), async ()=>{
            run!.Enabled=false; status.Text=T("Se citesc datele locale…", "Reading local data…");
            foreach(Control item in results.Controls.Cast<Control>().ToArray()) item.Dispose();
            results.Controls.Clear();
            try
            {
                var snapshot=await Task.Run(ReadQuickSnapshot);
                if(p.IsDisposed)return;
                int attention=0;
                if(snapshot.TotalRam>0)
                {
                    double used=(double)(snapshot.TotalRam-snapshot.AvailableRam)/snapshot.TotalRam;
                    bool high=used>=0.9; if(high)attention++;
                    Show(T("Memorie RAM", "RAM memory")+" • "+(used*100).ToString("N0")+T("% utilizat", "% used"),
                        GiB(snapshot.AvailableRam)+T(" disponibili din ", " available out of ")+GiB(snapshot.TotalRam)+". "+
                        (high ? T("Utilizarea este ridicată acum. Salvează lucrul și închide aplicațiile de care nu ai nevoie; verifică procesele în Task Manager.","Usage is high right now. Save your work and close apps you do not need; review processes in Task Manager.") : T("La această citire, utilizarea RAM este sub pragul orientativ de 90%.","At this reading, RAM usage is below the indicative 90% threshold.")));
                }
                else Show(T("RAM necitit", "RAM not read"),T("Nu s-au putut citi datele memoriei. Aceasta nu indică o defecțiune hardware.","Memory data could not be read. This does not indicate a hardware fault."));
                foreach(var disk in snapshot.Disks)
                {
                    double used=(double)(disk.Total-disk.Free)/disk.Total;
                    bool high=used>=0.9; if(high)attention++;
                    Show(T("Disc ","Drive ")+disk.Name+" • "+(used*100).ToString("N0")+T("% ocupat","% used"),
                        GiB((ulong)disk.Free)+T(" liberi din "," free out of ")+GiB((ulong)disk.Total)+". "+
                        (high ? T("Spațiu redus: revizuiește fișierele mari și aplicațiile nefolosite. Verifică atent categoriile din Setări stocare înainte de ștergere.","Low space: review large files and unused apps. Carefully review the categories in Storage settings before deleting anything.") : T("Spațiul ocupat este sub pragul orientativ de 90%.","Used space is below the indicative 90% threshold.")));
                }
                if(snapshot.Disks.Count==0) Show(T("Discuri", "Drives"),T("Nu s-au putut evalua volume locale de cel puțin 1 GiB.","No local volumes of at least 1 GiB could be assessed."));
                if(snapshot.Unreadable>0) Show(T("Citire incompletă", "Incomplete reading"),snapshot.Unreadable+T(" volume nu au putut fi citite. Nu sunt clasificate ca fiind în regulă."," volumes could not be read. They are not classified as OK."));
                status.Text=T("Ultima verificare: ","Last checked: ")+DateTime.Now.ToString("HH:mm:ss")+" • "+attention+T(" aspecte de verificat în datele citite."," items to review in the data read.");
                Show(T("Aplicații la pornire — verificare manuală", "Startup apps — manual review"),T("Nu am analizat automat impactul aplicațiilor la pornire. Butonul de mai jos deschide lista Windows, unde poți dezactiva pornirea aplicațiilor opționale pe care le recunoști.","Startup app impact was not analyzed automatically. The button below opens the Windows list, where you can disable startup for optional apps you recognize."));
                SetButtonCompleted(run!,attention==0 && snapshot.TotalRam>0 && snapshot.Disks.Count>0 && snapshot.Unreadable==0);
                Fit();
            }
            catch(Exception ex) { if(!p.IsDisposed)status.Text=T("Verificarea nu a fost finalizată: ","The check did not finish: ")+ex.Message; }
            finally { if(!run.IsDisposed)run.Enabled=true; }
        });
        p.Controls.Add(run); p.Controls.Add(status); p.Controls.Add(results);
        p.Controls.Add(Button(T("Aplicații la pornire", "Startup apps"),()=>Open("ms-settings:startupapps")));
        p.Controls.Add(Button(T("Setări stocare", "Storage settings"),()=>Open("ms-settings:storagesense")));
        p.Controls.Add(Button("Task Manager",()=>Open("taskmgr.exe")));
        Note(p,T("Pragul de 90% este orientativ, nu un diagnostic. Volumele mai mici de 1 GiB sunt omise pentru a evita alertele despre partiții mici de sistem. Datele rămân locale; verificarea nu rulează periodic.","The 90% threshold is guidance, not a diagnosis. Volumes smaller than 1 GiB are skipped to avoid alerts about small system partitions. Data stays local; the check does not run periodically."));
        p.SizeChanged+=(_,_)=>Fit(); Fit();
    }
    sealed record QuickDisk(string Name, long Total, long Free);
    sealed record QuickSnapshot(ulong TotalRam, ulong AvailableRam, List<QuickDisk> Disks, int Unreadable);
    static QuickSnapshot ReadQuickSnapshot()
    {
        var m=new MemoryStatus(); m.Length=(uint)Marshal.SizeOf<MemoryStatus>();
        bool read=GlobalMemoryStatusEx(ref m);
        var disks=new List<QuickDisk>(); int unreadable=0;
        foreach(var disk in DriveInfo.GetDrives())
        {
            try
            {
                if(disk.DriveType!=DriveType.Fixed)continue;
                if(!disk.IsReady) { unreadable++; continue; }
                long total=disk.TotalSize; if(total<1073741824L)continue;
                long free=disk.TotalFreeSpace;
                if(free<0 || free>total) { unreadable++; continue; }
                disks.Add(new(disk.Name,total,free));
            }
            catch(IOException) { unreadable++; }
            catch(UnauthorizedAccessException) { unreadable++; }
        }
        return new(read?m.Total:0,read?Math.Min(m.Available,m.Total):0,disks,unreadable);
    }
    void ResourcesPage()
    {
        var p = Page(T("RAM și stocare", "RAM & storage"));
        Note(p, T("Utilizare curentă • Actualizare la cerere", "Current usage • Refresh on request"));
        var cards = new TableLayoutPanel { Width = 800, Height = 200, AutoSize = false, ColumnCount = 2, Margin = new Padding(5,0,5,12) };
        cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50)); cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50)); p.Controls.Add(cards);
        var list = new DataGridView { Width = 800, Height = 300, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BorderStyle = BorderStyle.None, EnableHeadersVisualStyles = false, ColumnHeadersHeight = 40, RowTemplate = { Height = 34 } };
        list.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(95,160,235); list.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(20,49,87);
        list.Columns.Add("name",T("Aplicație deschisă", "Open application")); list.Columns.Add("ram","RAM (MiB)"); list.Columns.Add("pid","PID"); list.Columns[0].FillWeight=200;
        void Fit() { int width = Math.Max(300,p.ClientSize.Width-65); cards.Width=width; list.Width=width; }
        p.SizeChanged += (_,_)=>Fit();
        void Refresh()
        {
            cards.SuspendLayout();
            foreach(Control card in cards.Controls.Cast<Control>().ToArray()) card.Dispose();
            cards.Controls.Clear(); cards.RowStyles.Clear(); cards.RowCount=0;
            int count=0;
            void Add(string title, string value, string detail, double fraction, bool warning)
            {
                int row=count/2;
                if(count%2==0) { cards.RowCount=row+1; cards.RowStyles.Add(new RowStyle(SizeType.AutoSize)); }
                var card=new UsageCard { Heading=title, MainValue=value, Detail=detail, Fraction=fraction, Warning=warning, Dock=DockStyle.Fill, Height=(int)(154 * DeviceDpi / 96f), AccessibleName=title+" "+value+" "+detail };
                cards.Controls.Add(card,count%2,row); count++;
            }
            var m = new MemoryStatus(); m.Length = (uint)Marshal.SizeOf<MemoryStatus>();
            if(GlobalMemoryStatusEx(ref m) && m.Total>0) Add("RAM",GiB(m.Total-m.Available)+T(" utilizați"," used"),GiB(m.Available)+T(" disponibili • "," available • ")+GiB(m.Total)+T(" total"," total"),(double)(m.Total-m.Available)/m.Total,false);
            else Add("RAM",T("Indisponibil","Unavailable"),T("Citirea memoriei a eșuat.","Memory reading failed."),0,false);
            foreach(var d in DriveInfo.GetDrives())
            {
                try { if(d.IsReady && d.DriveType==DriveType.Fixed && d.TotalSize>0) { double fraction=(double)(d.TotalSize-d.TotalFreeSpace)/d.TotalSize; Add(d.Name,GiB((ulong)d.TotalFreeSpace)+T(" liberi"," free"),GiB((ulong)d.TotalSize)+T(" total • "," total • ")+(fraction*100).ToString("N0")+T("% ocupat","% used"),fraction,fraction>=0.9); } } catch { }
            }
            cards.Height = ((count + 1) / 2) * ((int)(154 * DeviceDpi / 96f) + 12);
            cards.ResumeLayout(true);
            list.Rows.Clear(); var rows=new List<(string Name,long Ram,int Id)>();
            foreach(var proc in Process.GetProcesses()) using(proc) { try { if(proc.Id!=Environment.ProcessId && proc.MainWindowHandle!=IntPtr.Zero) rows.Add((proc.ProcessName,proc.WorkingSet64,proc.Id)); } catch { } }
            foreach(var row in rows.OrderByDescending(r=>r.Ram)) { int i=list.Rows.Add(row.Name,(row.Ram/1048576d).ToString("N1"),row.Id); list.Rows[i].Tag=row.Id; }
            Fit();
        }
        p.Controls.Add(Button(T("Actualizează", "Refresh"),Refresh));
        Note(p,T("Aplicații deschise", "Open applications")); p.Controls.Add(list);
        p.Controls.Add(Button(T("Închide aplicația selectată", "Close selected application"),()=>{
            if(list.SelectedRows.Count==0) { MessageBox.Show(T("Selectează întâi o aplicație.","Select an application first.")); return; }
            if(MessageBox.Show(this,T("Salvează lucrul înainte de închidere. Trimiți cererea aplicației selectate?","Save your work first. Send a close request to the selected application?"),"RADUSELF",MessageBoxButtons.YesNo,MessageBoxIcon.Question,MessageBoxDefaultButton.Button2)!=DialogResult.Yes)return;
            using var proc=Process.GetProcessById((int)list.SelectedRows[0].Tag!);
            if(!proc.CloseMainWindow()) MessageBox.Show(T("Aplicația nu a acceptat cererea.","The application did not accept the request."));
        }));
        p.Controls.Add(Button("Task Manager",()=>Open("taskmgr.exe")));
        Note(p,T("RAM per proces reprezintă working set. Închiderea nu este forțată și nu garantează o anumită cantitate de RAM eliberată.","Per-process RAM is the working set. Closing is not forced and does not guarantee an amount of freed RAM."));
        Refresh();
    }
    static string GiB(ulong value) => (value/1073741824d).ToString("N2") + " GiB";
    void CatalogPage(string title, Entry[] entries)
    {
        var tab = new Panel { Text = title }; tabs.AddPage(tab);
        var search = new TextBox { Dock = DockStyle.Top, PlaceholderText = T("Caută după nume, tastă sau comandă…", "Search by name, key or command…") };
        var grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, RowHeadersVisible = false };
        grid.BackgroundColor = Color.FromArgb(119,181,254); grid.BorderStyle = BorderStyle.None; grid.EnableHeadersVisualStyles = false; grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(95,160,235); grid.ColumnHeadersHeight = 40; grid.DefaultCellStyle.Padding = new Padding(7); grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True; grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells; grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(133,190,255);
        grid.Columns.Add("category",T("Categorie", "Category")); grid.Columns.Add("keys",T("Scurtătură / comandă", "Shortcut / command")); grid.Columns.Add("description",T("Explicație", "Description")); grid.Columns[2].FillWeight=210;
        var copy = Button(T("Copiază selecția", "Copy selection"),()=> { if(grid.SelectedRows.Count>0) Clipboard.SetText(grid.SelectedRows[0].Cells[1].Value?.ToString() ?? ""); }); copy.Dock=DockStyle.Bottom;
        void Filter() { grid.Rows.Clear(); foreach(var e in entries) { var desc=english?e.En:e.Ro; var category = e.Category.Contains(" / ") ? e.Category.Split(" / ")[english ? 1 : 0] : e.Category; if((e.Code+desc+category).Contains(search.Text,StringComparison.OrdinalIgnoreCase)) grid.Rows.Add(category,e.Code,desc); } }
        search.TextChanged += (_,_)=>Filter(); tab.Controls.Add(grid); tab.Controls.Add(search); tab.Controls.Add(copy); Filter();
    }
    void AppsPage()
    {
        var p=Page(T("Aplicații nedorite", "Unwanted apps"));
        Note(p,T("Bloatware înseamnă aplicații de care nu ai nevoie; nu orice aplicație preinstalată este inutilă. În această versiune dezinstalarea se confirmă în setările Windows. Nu eliminăm în masă componente de sistem.","Bloatware means apps you do not need; not every preinstalled app is unnecessary. In this version uninstalling is confirmed in Windows Settings. System components are not removed in bulk."));
        var list=new ListView { Width=960,Height=350,View=View.Details,FullRowSelect=true }; list.Columns.Add(T("Aplicație desktop instalată", "Installed desktop application"),470); list.Columns.Add(T("Editor", "Publisher"),300); p.Controls.Add(list);
        p.Controls.Add(Button(T("Identifică aplicațiile desktop", "List desktop applications"),()=>{
            list.Items.Clear(); var rows=new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach(var hive in new[]{RegistryHive.CurrentUser,RegistryHive.LocalMachine}) foreach(var view in new[]{RegistryView.Registry32,RegistryView.Registry64}) {
                using var root=RegistryKey.OpenBaseKey(hive,view); using var key=root.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"); if(key==null) continue;
                foreach(var name in key.GetSubKeyNames()) { using var sub=key.OpenSubKey(name); if(sub?.GetValue("DisplayName") is string display && !string.IsNullOrWhiteSpace(display) && !Equals(sub.GetValue("SystemComponent"),1)) rows.Add(display+"\t"+(sub.GetValue("Publisher") as string ?? "")); }
            }
            foreach(var row in rows) { var parts=row.Split('\t'); var item=new ListViewItem(parts[0]); item.SubItems.Add(parts[1]); list.Items.Add(item); }
        }));
        p.Controls.Add(Button(T("Dezinstalează din setări", "Uninstall in Settings"),()=>Open("ms-settings:appsfeatures")));
        Note(p,T("Lista locală include aplicații desktop înregistrate, nu toate pachetele Microsoft Store. Setările Windows afișează și alte aplicații. Alege doar programe pe care le recunoști și nu le folosești.","The local list includes registered desktop applications, not every Microsoft Store package. Windows Settings shows additional apps. Choose programs you recognize and do not use."));
    }
    string CleanupReasons(Dictionary<string,int> reasons)
    {
        var lines=new List<string>();
        foreach(var item in reasons)
        {
            string label=item.Key switch
            {
                "access" => T("Acces refuzat", "Access denied"),
                "locked" => T("Fișiere în uz sau blocate", "Files in use or locked"),
                "missing" => T("Fișiere sau foldere care nu mai există", "Files or folders no longer present"),
                "changed" => T("Fișiere modificate după analiză", "Files changed after analysis"),
                "readonly" => T("Fișiere doar pentru citire", "Read-only files"),
                "links" => T("Legături sau căi nepermise", "Links or disallowed paths"),
                _ => T("Alte erori de citire/scriere", "Other input/output errors")
            };
            lines.Add(label+": "+item.Value);
        }
        return lines.Count==0 ? "" : "\n"+string.Join("\n",lines);
    }
    void OpenSystemCleanup()
    {
        string? executable=Environment.ProcessPath;
        if(string.IsNullOrWhiteSpace(executable) || !Path.GetExtension(executable).Equals(".exe",StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(T("Pornește aplicația publicată RaduSelf.PCCheck.exe pentru această opțiune.","Run the published RaduSelf.PCCheck.exe to use this option."));
        try
        {
            var start=new ProcessStartInfo(executable) { UseShellExecute=true, Verb="runas", WorkingDirectory=AppContext.BaseDirectory };
            start.ArgumentList.Add("--system-cleanup");
            if(english)start.ArgumentList.Add("--english");
            Process.Start(start);
        }
        catch(System.ComponentModel.Win32Exception ex) when(ex.NativeErrorCode==1223)
        {
            MessageBox.Show(this,T("Cererea de administrator a fost anulată. Nu s-a pornit curățarea.","The administrator request was cancelled. Cleanup was not started."),T("Curățare", "Cleanup"));
        }
    }
    void CleanupPage()
    {
        var p = Page(T("Curățare", "Cleanup"));
        if(systemCleanup)
            Note(p,T("Mod administrator: doar Windows TEMP și Prefetch. Nicio ștergere nu pornește automat. Fișierele blocate sau protejate rămân omise; permisiunile nu sunt modificate. Închide această fereastră pentru a reveni la aplicația normală.","Administrator mode: Windows TEMP and Prefetch only. No deletion starts automatically. Locked or protected files are still skipped; permissions are unchanged. Close this window to return to the normal app."));
        else
        {
            Note(p,T("Pentru acces la folderele de sistem, poți deschide o fereastră separată cu drepturi de administrator. Windows îți cere acordul sau datele unui administrator. TEMP-ul utilizatorului rămâne în această fereastră.","For access to system folders, you can open a separate administrator window. Windows asks for consent or administrator credentials. User TEMP stays in this window."));
            p.Controls.Add(Button(T("Curățare sistem — administrator", "System cleanup — administrator"),OpenSystemCleanup));
        }
        Note(p, T("Ștergerea este definitivă. Închide aplicațiile și salvează lucrul înainte de curățare. TEMP: păstrăm fișierele modificate în ultimele 7 zile. Fișierele blocate, legăturile și elementele fără acces sunt omise. Folderele nu sunt șterse.", "Deletion is permanent. Close applications and save your work first. TEMP: files modified within the last 7 days are kept. Locked files, links and inaccessible items are skipped. Directories are not deleted."));
        void Add(string title, string root, bool prefetch)
        {
            Note(p, title + "\n" + root);
            Button? clean=null;
            clean=Button(T("Analizează și curăță: ", "Analyze and clean: ") + title, async () => {
                p.Enabled = false;
                try
                {
                    var cutoff = prefetch ? DateTime.MaxValue : DateTime.UtcNow.AddDays(-7);
                    var scan = await Task.Run(() => CleanupFiles.Scan(root, cutoff, prefetch));
                    if(p.IsDisposed)return;
                    if (scan.Files.Count == 0)
                    {
                        if(scan.Skipped==0)
                        {
                            SetButtonNothingToClean(clean!);
                            MessageBox.Show(this,T("Nimic de curățat: analiza nu a găsit fișiere eligibile. Nu s-a șters nimic.", "Nothing to clean: analysis found no eligible files. Nothing was deleted."),title);
                        }
                        else MessageBox.Show(this,T("Analiză incompletă: nu s-au găsit fișiere eligibile accesibile. Elemente omise: ", "Incomplete analysis: no accessible eligible files found. Skipped items: ")+scan.Skipped+T(". Nu s-a șters nimic.", ". Nothing was deleted.")+CleanupReasons(scan.Reasons),title);
                        return;
                    }
                    string warning = prefetch ? T("Prefetch ajută la lansarea aplicațiilor. Curățarea nu este recomandată ca optimizare de rutină și poate încetini temporar pornirea aplicațiilor. Se șterg doar fișiere .pf, nu baza de date SysMain.\n\n", "Prefetch helps applications launch. Routine cleanup is not recommended as an optimization and may temporarily slow launches. Only .pf files are removed, not the SysMain database.\n\n") : "";
                    if (MessageBox.Show(this, warning + root + "\n" + scan.Files.Count + T(" fișiere eligibile, ", " eligible files, ") + (scan.Bytes / 1048576d).ToString("N2") + " MiB\n" + T("Ștergi definitiv aceste fișiere?", "Permanently delete these files?"), T("Confirmă curățarea", "Confirm cleanup"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
                    var result = await Task.Run(() => CleanupFiles.Delete(root, scan.Files, cutoff));
                    if(p.IsDisposed)return;
                    SetButtonCompleted(clean!,result.Deleted>0 && result.Skipped==0 && scan.Skipped==0);
                    MessageBox.Show(this, T("Fișiere șterse: ", "Files deleted: ") + result.Deleted + T("\nFișiere omise: ", "\nFiles skipped: ") + result.Skipped + T("\nDimensiunea fișierelor șterse: ", "\nSize of deleted files: ") + (result.Bytes / 1048576d).ToString("N2") + " MiB" + CleanupReasons(result.Reasons) + T("\nElemente omise la analiză: ", "\nItems skipped during analysis: ") + scan.Skipped + CleanupReasons(scan.Reasons) + T("\nPermisiunile nu sunt modificate. Fișierele blocate sau protejate sunt omise.", "\nPermissions are unchanged. Locked or protected files are skipped."));
                }
                catch (Exception ex) { if(!p.IsDisposed)MessageBox.Show(this, ex.Message, T("Curățarea nu a reușit", "Cleanup failed")); }
                finally { if(!p.IsDisposed)p.Enabled = true; }
            });
            p.Controls.Add(clean);
        }
        if(!systemCleanup)Add(T("TEMP utilizator", "User TEMP"), Path.GetTempPath(), false);
        var windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        Add("Windows TEMP", Path.Combine(windows, "Temp"), false);
        Note(p, T("Opțiune avansată: Prefetch se curăță separat, doar la cerere. Poate necesita permisiuni suplimentare; accesul refuzat este raportat, nu ocolit.", "Advanced option: Prefetch is cleaned separately, only on request. Additional permissions may be required; denied access is reported, not bypassed."));
        Add("Prefetch", Path.Combine(windows, "Prefetch"), true);
    }
    void PowerControls(FlowLayoutPanel p)
    {
        p.Controls.Add(new Label { Text=T("Alimentare și pornire", "Power & startup"), AutoSize=true, Font=new Font("Segoe UI",13,FontStyle.Bold), Margin=new Padding(5,10,5,8) });
        Note(p,T("Salvează documentele și închide lucrul înainte de continuare. Oprirea sau repornirea începe imediat după confirmare. Nu folosim închiderea forțată; Windows poate bloca acțiunea dacă o aplicație nu se închide.","Save documents and finish your work before continuing. Shutdown or restart starts immediately after confirmation. Forced closure is not used; Windows may block the action if an app does not close."));
        var status=new Label { Text="", AutoSize=true, MaximumSize=new Size(800,0), Margin=new Padding(5,8,5,12) };
        bool busy=false;
        void Add(string label, string arguments, bool firmware)
        {
            p.Controls.Add(Button(label,async ()=>{
                if(busy)return;
                if(firmware && !PowerActions.IsUefi())
                {
                    MessageBox.Show(this,T("Windows nu a confirmat pornirea în mod UEFI. Intrarea automată în BIOS nu este disponibilă aici. Poți folosi tasta indicată de producător la pornire (de exemplu Del sau F2) ori opțiunile din Recuperare, dacă sunt oferite.","Windows did not confirm UEFI boot mode. Automatic BIOS entry is not available here. Use the manufacturer's boot key (for example Del or F2), or Recovery options if offered."),"BIOS / UEFI"); return;
                }
                string extra=firmware?T("\n\nSe solicită drepturi de administrator. Anularea ferestrei Windows anulează cererea. Calculatorul trebuie să permită intrarea în setările firmware din Windows.","\n\nAdministrator permission will be requested. Canceling the Windows prompt cancels this request. The computer must support entering firmware settings from Windows."):"";
                if(MessageBox.Show(this,label+"?\n\n"+T("Ai salvat lucrul? Acțiunea va începe imediat după Yes. Nu forțăm închiderea programelor.","Have you saved your work? The action starts immediately after Yes. Apps will not be forcibly closed.")+extra,T("Confirmă acțiunea", "Confirm action"),MessageBoxButtons.YesNo,MessageBoxIcon.Warning,MessageBoxDefaultButton.Button2)!=DialogResult.Yes)return;
                busy=true; p.Enabled=false;
                try
                {
                    var result=await PowerActions.RunAsync(arguments,firmware);
                    if(p.IsDisposed)return;
                    status.Text=result.Canceled?T("Cererea de administrator a fost anulată. Nu s-a trimis comanda.","The administrator prompt was canceled. No command was sent."):
                        result.ExitCode==0?T("Windows a acceptat cererea. O aplicație poate împiedica finalizarea. Nu apăsa repetat butonul.","Windows accepted the request. An app may prevent completion. Do not press the button repeatedly."):
                        T("Windows nu a acceptat cererea. Cod: ","Windows did not accept the request. Code: ")+result.ExitCode+"\n"+result.Error+T("\nPentru BIOS/UEFI poți deschide Recuperare sau folosi tasta de pornire a producătorului. Nu se execută o repornire alternativă automat.","\nFor BIOS/UEFI, open Recovery or use the manufacturer's boot key. No alternative restart is performed automatically.");
                }
                catch(Exception ex) { if(!p.IsDisposed)status.Text=T("Acțiunea nu a putut fi pornită: ","The action could not start: ")+ex.Message; }
                finally { busy=false; if(!p.IsDisposed)p.Enabled=true; }
            }));
        }
        Add(T("Oprește PC-ul", "Shut down PC"), "/s /t 0", false);
        Add(T("Repornește PC-ul", "Restart PC"), "/r /t 0", false);
        Add(T("Repornește în BIOS/UEFI", "Restart into BIOS/UEFI"), "/r /fw /t 0", true);
        p.Controls.Add(status);
        p.Controls.Add(Button(T("Deschide setările de Recuperare", "Open Recovery settings"),()=>Open("ms-settings:recovery")));
    }
    void SettingsPage()
    {
        var p=Page(T("Optimizări Windows", "Windows tweaks"));
        PowerControls(p);
        (string ro,string en,string uri,string tipRo,string tipEn)[] settings = [
            ("Aplicații la pornire","Startup apps","ms-settings:startupapps","Dezactivează pornirea automată pentru aplicații opționale. Poți reactiva oricând.","Disable automatic startup for optional apps. Re-enable any time."),
            ("Stocare și fișiere temporare","Storage & temporary files","ms-settings:storagesense","Verifică fiecare categorie înainte de ștergere, mai ales Descărcări. Ștergerea fișierelor poate fi definitivă.","Review each category before deleting, especially Downloads. File deletion can be permanent."),
            ("Consum și energie","Power & energy","ms-settings:powersleep","Performanța ridicată poate crește temperatura și consumul. Revino la modul anterior pentru anulare.","Higher performance may increase heat and energy use. Restore the previous mode to undo."),
            ("Efecte vizuale","Visual effects","ms-settings:easeofaccess-visualeffects","Reducerea animațiilor poate face interfața mai simplă. Modificările se pot reactiva.","Reducing animations can simplify the interface. Changes can be re-enabled."),
            ("Actualizări Windows","Windows Update","ms-settings:windowsupdate","Verifică actualizările și eventualele reporniri necesare.","Check updates and required restarts."),
            ("Confidențialitate","Privacy","ms-settings:privacy","Revizuiește permisiunile în funcție de aplicațiile folosite.","Review permissions according to the applications you use.") ];
        Note(p,T("Pentru recomandările din Setările Windows, verde cu ✓ înseamnă că ai confirmat aplicarea lor. Aplicația nu verifică automat aceste setări. Starea se păstrează doar pe pagina din sesiunea curentă.","For Windows Settings recommendations, green with ✓ means you confirmed applying them. The app does not verify these settings automatically. The status lasts only on this page in the current session."));
        foreach(var s in settings)
        {
            Note(p,english?s.tipEn:s.tipRo);
            Button? confirm=null;
            var open=Button(english?s.en:s.ro,()=>{ Open(s.uri); confirm!.Visible=true; });
            confirm=Button(T("Confirm că am aplicat recomandarea", "I confirm I applied the recommendation"),()=>{ SetButtonCompleted(open,true); confirm!.Visible=false; });
            confirm.Visible=false;
            p.Controls.Add(open); p.Controls.Add(confirm);
        }
    }
    void SecurityPage()
    {
        var p=Page(T("Securitate", "Security"));
        Note(p,T("Deschide protecțiile Windows și verifică starea afișată acolo. RADUSELF nu rulează o scanare antivirus proprie și nu verifică automat dacă PC-ul este protejat.","Open Windows protection tools and review the status shown there. RADUSELF does not run its own antivirus scan or automatically verify whether your PC is protected."));
        void Launch(string uri)
        {
            try { Open(uri); }
            catch(Exception)
            {
                MessageBox.Show(this,T("Pagina nu a putut fi deschisă. Încearcă din Start → Securitate Windows sau Setări → Confidențialitate și securitate → Securitate Windows. Unele opțiuni pot fi gestionate de organizație sau de antivirusul instalat.","The page could not be opened. Try Start → Windows Security or Settings → Privacy & security → Windows Security. Some options may be managed by your organization or installed antivirus."),T("Securitate", "Security"),MessageBoxButtons.OK,MessageBoxIcon.Information);
            }
        }
        p.Controls.Add(Button(T("Deschide Windows Security", "Open Windows Security"),()=>Launch("windowsdefender:")));
        Note(p,T("Antivirus: în Protecție împotriva virușilor și amenințărilor, alege Scanare rapidă sau Opțiuni de scanare. Verifică rezultatul în Windows Security. Dacă folosești alt antivirus, consultă aplicația furnizorului activ.","Antivirus: in Virus & threat protection, choose Quick scan or Scan options. Review the result in Windows Security. If you use another antivirus, consult the active provider's app."));
        p.Controls.Add(Button(T("Protecție antivirus și scanare", "Antivirus protection & scanning"),()=>Launch("windowsdefender://threat/")));
        Note(p,T("Firewall: verifică dacă protecția este activată pentru rețeaua folosită. Păstrează firewallul activ; nu permite aplicații pe care nu le recunoști.","Firewall: check that protection is on for the network in use. Keep the firewall on; do not allow apps you do not recognize."));
        p.Controls.Add(Button(T("Firewall și protecție rețea", "Firewall & network protection"),()=>Launch("windowsdefender://network/")));
        Note(p,T("Actualizări: verifică actualizările disponibile și mesajele despre repornire în Windows Update. Salvează lucrul înainte de orice repornire.","Updates: review available updates and restart messages in Windows Update. Save your work before restarting."));
        p.Controls.Add(Button(T("Actualizări Windows", "Windows Update"),()=>Launch("ms-settings:windowsupdate")));
        Note(p,T("Butoanele deschid paginile Windows. Deschiderea unei pagini nu confirmă finalizarea scanării sau remedierea unei probleme.","Buttons open Windows pages. Opening a page does not confirm that a scan finished or an issue was resolved."));
    }
    void AboutPage()
    {
        var p=Page(T("Despre și donații","About & donations"));
        Note(p,T("Versiune inițială 0.4.10 pentru testare. Gratuită, în română și engleză. Nu rulează scanări în fundal și nu trimite inventarul aplicațiilor sau informațiile RAM către un server. Preferința limbii se salvează local.","Initial version 0.4.10 for testing. Free, in Romanian and English. No background scans; app inventory and RAM information are not transmitted to a server. Language preference is stored locally."));
        Note(p,T("Scurtăturile sunt un catalog extensibil, nu o listă exhaustivă a tuturor programelor. Comenzile se copiază și se verifică înainte de folosire; nu sunt executate automat.","Shortcuts are an expandable catalog, not an exhaustive list for all programs. Commands are copied and reviewed before use; they are not run automatically."));
        p.Controls.Add(Button(T("Donează", "Donate"),Donation)); p.Controls.Add(Button("Buy me a coffee",Donation));
        Note(p,T("Suport: ","Support: ")+"raduself.oficial@gmail.com");
    }
    void Donation() { MessageBox.Show(this,T("Mulțumim pentru sprijin! Informațiile despre susținerea proiectului sunt disponibile pe raduself.ro.","Thank you for your support! Information about supporting the project is available on raduself.ro."),T("Susține RADUSELF","Support RADUSELF"),MessageBoxButtons.OK,MessageBoxIcon.Information); }
    [StructLayout(LayoutKind.Sequential)] struct MemoryStatus { public uint Length,Load; public ulong Total,Available,TotalPage,AvailablePage,TotalVirtual,AvailableVirtual,AvailableExtended; }
    [DllImport("kernel32.dll",SetLastError=true)] [return:MarshalAs(UnmanagedType.Bool)] static extern bool GlobalMemoryStatusEx(ref MemoryStatus status);
}
record Entry(string Category,string Code,string Ro,string En);
