using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ArkBot.Helpers;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;

namespace ArkBot.Tests
{
    [TestClass]
    public class ArkServerService_UpdateServer
    {
        [TestMethod]
        public void UpdateServer()
        {
            var r = new Regex(@"(?<task>\w+),\s+progress\:\s+(?<progress>[\d\.]+)", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
            var sb = new StringBuilder();
            var result = ProcessHelper.RunCommandLineTool("C:\\Temp\\steamcmd\\updateArkServer.bat", "", outputDataReceived: new Func<string, int>((s) =>
            {
                //Success! App '376030' already up to date.
                //Update state (0x11) preallocating, progress: 76.88 (3826221085 / 4976891598)
                //Update state (0x61) downloading, progress: 5.88 (292616661 / 4976891598)
                if (s != null) sb.AppendLine(s);

                return 0;
            }), hiddenWindow: false).Result;

            Assert.AreEqual(0, result.ExitCode);
        }
    }
}
