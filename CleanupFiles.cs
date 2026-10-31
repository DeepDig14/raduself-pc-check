namespace RaduSelf.PCCheck;

// Deliberately never follows junctions/symlinks, removes directories or elevates.
static class CleanupFiles
{
    internal sealed record Candidate(string Path, long Length, DateTime Modified);
    internal sealed record ScanResult(List<Candidate> Files, long Bytes, int Skipped, Dictionary<string,int> Reasons);
    internal sealed record DeleteResult(int Deleted, int Skipped, long Bytes, Dictionary<string,int> Reasons);
    static void Count(Dictionary<string,int> reasons, string reason) => reasons[reason] = reasons.GetValueOrDefault(reason) + 1;
    static string Reason(IOException ex) => (ex.HResult & 0xffff) switch
    {
        32 or 33 => "locked",
        2 or 3 => "missing",
        _ => "io"
    };
    static string Normalize(string path) => System.IO.Path.TrimEndingDirectorySeparator(System.IO.Path.GetFullPath(path));
    static bool NoLinks(string path)
    {
        for (string? current = path; current != null; current = System.IO.Path.GetDirectoryName(current))
            if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0) return false;
        return true;
    }
    static string ValidateRoot(string root)
    {
        root = Normalize(root);
        string windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        string userTemp = Normalize(System.IO.Path.GetTempPath());
        bool user = root.Equals(userTemp, StringComparison.OrdinalIgnoreCase) &&
            new[] { "temp", "tmp" }.Contains(System.IO.Path.GetFileName(root), StringComparer.OrdinalIgnoreCase);
        bool system = root.Equals(Normalize(System.IO.Path.Combine(windows, "Temp")), StringComparison.OrdinalIgnoreCase) ||
            root.Equals(Normalize(System.IO.Path.Combine(windows, "Prefetch")), StringComparison.OrdinalIgnoreCase);
        if ((!user && !system) || !Directory.Exists(root) || !NoLinks(root))
            throw new IOException("Cleanup folder is unavailable, redirected, or unsupported: " + root);
        return root;
    }
    internal static ScanResult Scan(string root, DateTime cutoff, bool prefetch)
    {
        root = ValidateRoot(root);
        var files = new List<Candidate>(); long bytes = 0; int skipped = 0;
        var reasons = new Dictionary<string,int>();
        var dirs = new Stack<string>(); dirs.Push(root);
        while (dirs.TryPop(out var dir))
        {
            try
            {
                if (!NoLinks(dir)) { skipped++; Count(reasons,"links"); continue; }
                foreach (var path in Directory.GetFiles(dir))
                {
                    try
                    {
                        var info = new FileInfo(path);
                        if ((info.Attributes & (FileAttributes.ReparsePoint | FileAttributes.ReadOnly)) != 0 || info.LastWriteTimeUtc >= cutoff) continue;
                        if (prefetch && !info.Extension.Equals(".pf", StringComparison.OrdinalIgnoreCase)) continue;
                        files.Add(new(path, info.Length, info.LastWriteTimeUtc)); bytes += info.Length;
                    }
                    catch (IOException ex) { skipped++; Count(reasons,Reason(ex)); }
                    catch (UnauthorizedAccessException) { skipped++; Count(reasons,"access"); }
                }
                if (!prefetch) foreach (var child in Directory.GetDirectories(dir)) dirs.Push(child);
            }
            catch (IOException ex) { skipped++; Count(reasons,Reason(ex)); }
            catch (UnauthorizedAccessException) { skipped++; Count(reasons,"access"); }
        }
        return new(files, bytes, skipped, reasons);
    }
    internal static DeleteResult Delete(string root, List<Candidate> files, DateTime cutoff)
    {
        root = ValidateRoot(root);
        int deleted = 0, skipped = 0; long bytes = 0;
        var reasons = new Dictionary<string,int>();
        foreach (var candidate in files)
        {
            try
            {
                var path = Normalize(candidate.Path);
                if (!path.StartsWith(root + System.IO.Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || !NoLinks(path)) { skipped++; Count(reasons,"links"); continue; }
                var info = new FileInfo(path);
                if ((info.Attributes & FileAttributes.ReadOnly) != 0) { skipped++; Count(reasons,"readonly"); continue; }
                if (info.Length != candidate.Length || info.LastWriteTimeUtc != candidate.Modified || info.LastWriteTimeUtc >= cutoff)
                { skipped++; Count(reasons,"changed"); continue; }
                // Exclusive access catches files in use before attempting deletion.
                using (new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None)) { }
                File.Delete(path); deleted++; bytes += candidate.Length;
            }
            catch (IOException ex) { skipped++; Count(reasons,Reason(ex)); }
            catch (UnauthorizedAccessException) { skipped++; Count(reasons,"access"); }
        }
        return new(deleted, skipped, bytes, reasons);
    }
}
