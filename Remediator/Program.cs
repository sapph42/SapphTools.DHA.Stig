using SapphTools.DHA.Stig.Remediator.Forms;
using System.Diagnostics;
using System.Text;

namespace SapphTools.DHA.Stig.Remediator {
    internal static class Program {
        private static int _shuttingDown;
        private static readonly List<Process> _excelProcesses = [];
        [STAThread]
        static void Main() {
            ApplicationConfiguration.Initialize();
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            AppDomain.CurrentDomain.UnhandledException += (_, _) => Shutdown();
            AppDomain.CurrentDomain.ProcessExit += (_, _) => Shutdown();
            try {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Application.Run(new Main());
            } finally {
                Shutdown();
            }
        }
        public static void AddExcelProcess(Process excel) {
            lock (_excelProcesses) {
                _excelProcesses.Add(excel);
            }
            excel.EnableRaisingEvents = true;
            excel.Exited += (_, _) => {
                lock (_excelProcesses) {
                    _excelProcesses.Remove(excel);
                }
            };
        }
        public static void Shutdown() {
            if (Interlocked.Exchange(ref _shuttingDown, 1) != 0) {
                return;
            }
            try {
                Process[] processes;
                lock (_excelProcesses) {
                    processes = [.. _excelProcesses];
                }
                foreach (Process process in processes) {
                    try {
                        process.Kill();
                    } catch { }
                }
            } catch { }
        }
    }
}