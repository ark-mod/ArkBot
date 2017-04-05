using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Interop
{
    [Flags]
    public enum RestartRestrictions
    {
        None = 0,
        NotOnCrash = 1,
        NotOnHang = 2,
        NotOnPatch = 4,
        NotOnReboot = 8
    }

    public delegate int RecoveryDelegate(RecoveryData parameter);

    public static class Kernel32
    {
        [DllImport("kernel32.dll")]
        public static extern void ApplicationRecoveryFinished(
            bool success);

        [DllImport("kernel32.dll")]
        public static extern int ApplicationRecoveryInProgress(
            out bool canceled);

        [DllImport("kernel32.dll")]
        public static extern int GetApplicationRecoveryCallback(
            IntPtr processHandle,
            out RecoveryDelegate recoveryCallback,
            out RecoveryData parameter,
            out uint pingInterval,
            out uint flags);

        [DllImport("KERNEL32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetApplicationRestartSettings(
            IntPtr process,
            IntPtr commandLine,
            ref uint size,
            out uint flags);

        [DllImport("kernel32.dll")]
        public static extern int RegisterApplicationRecoveryCallback(
            RecoveryDelegate recoveryCallback,
            RecoveryData parameter,
            uint pingInterval,
            uint flags);

        [DllImport("kernel32.dll")]
        public static extern int RegisterApplicationRestart(
            [MarshalAs(UnmanagedType.BStr)] string commandLineArgs,
            int flags);

        [DllImport("kernel32.dll")]
        public static extern int UnregisterApplicationRecoveryCallback();

        [DllImport("kernel32.dll")]
        public static extern int UnregisterApplicationRestart();
    }

    public class RecoveryData
    {
        string currentUser;

        public RecoveryData(string who)
        {
            currentUser = who;
        }
        public string CurrentUser
        {
            get { return currentUser; }
        }
    }
}
