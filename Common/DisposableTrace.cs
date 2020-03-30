using System;
using System.Diagnostics;

namespace Common
{
    public class DisposableTrace : IDisposable
    {
        private const int SpacesPerTab = 3;

        [ThreadStatic]
        private static int tabLevel = 0;

        // To detect redundant calls
        private bool disposedValue;

        private string methodName = string.Empty;

        private Stopwatch stopwatch;

        public DisposableTrace()
        {
            StackTrace st = new StackTrace();
            try
            {
                StartTrace(st.GetFrame(1).GetMethod().ToString());
            }
            catch (Exception ex)
            {
                StartTrace("No Label");
                Trace.WriteLine(ex.ToString());
            }
        }

        public DisposableTrace(string explicitLabel)
        {
            StartTrace(explicitLabel);
        }

        ~DisposableTrace()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(false);

            //base.Finalize();
        }

        public static void WriteEntry(string st)
        {
            string spacing = string.Empty.PadRight(SpacesPerTab * tabLevel);
            Trace.WriteLine(spacing + st);
        }

        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(bool disposing).
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void StartTrace(string label)
        {
            methodName = label;
            WriteEntry($">> {methodName}");
            tabLevel += 1;
            stopwatch = Stopwatch.StartNew();
        }

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    stopwatch.Stop();
                    tabLevel -= 1;
                    WriteEntry($"<< {methodName} ({stopwatch.ElapsedMilliseconds} ms)");
                }
            }

            this.disposedValue = true;
        }
    }
}
