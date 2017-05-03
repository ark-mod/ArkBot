using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArkBot.Extensions
{
    public static class ProcessExtensions
    {
        public static void KillTree(this Process self)
        {
            KillProcessAndChildrenInternal(self.Id);
        }

        private static void KillProcessAndChildrenInternal(int pid)
        {
            var searcher = new ManagementObjectSearcher ("Select * From Win32_Process Where ParentProcessID=" + pid);
            foreach (ManagementObject mo in searcher.Get()) KillProcessAndChildrenInternal(Convert.ToInt32(mo["ProcessID"]));

            try
            {
                var pname = Process.GetProcessById(pid)?.ProcessName;
                Process.GetProcessById(pid)?.Kill();
            }
            catch (Exception) { }
        }
    }
}
