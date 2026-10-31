using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RaduSelf.PCCheck;

static class PowerActions
{
    internal sealed record Result(int ExitCode, bool Canceled, string Error);
    internal static bool IsUefi()
    {
        try { return GetFirmwareType(out uint type) && type == 2; }
        catch (EntryPointNotFoundException) { return false; }
    }
    internal static async Task<Result> RunAsync(string arguments, bool elevate)
    {
        // Fixed allowlist. Never accept arbitrary executable names or shell input.
        if (arguments is not ("/s /t 0" or "/r /t 0" or "/r /fw /t 0"))
            throw new ArgumentException("Unsupported power action.");
        if (elevate != (arguments == "/r /fw /t 0"))
            throw new ArgumentException("Invalid elevation request.");
        var info = new ProcessStartInfo
        {
            FileName=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),"shutdown.exe"),
            Arguments=arguments,
            UseShellExecute=elevate,
            CreateNoWindow=!elevate,
            RedirectStandardError=!elevate,
            RedirectStandardOutput=!elevate
        };
        if(elevate)info.Verb="runas";
        try
        {
            using var process=Process.Start(info) ?? throw new InvalidOperationException("Windows could not start shutdown.exe.");
            Task<string>? error=elevate?null:process.StandardError.ReadToEndAsync();
            Task<string>? output=elevate?null:process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            string detail=error==null?"":await error;
            if(output!=null) { string text=await output; if(string.IsNullOrWhiteSpace(detail))detail=text; }
            return new(process.ExitCode,false,detail.Trim());
        }
        catch(Win32Exception ex) when(ex.NativeErrorCode==1223)
        {
            return new(-1,true,"");
        }
    }
    [DllImport("kernel32.dll", SetLastError=true)]
    [return:MarshalAs(UnmanagedType.Bool)]
    static extern bool GetFirmwareType(out uint firmwareType);
}
