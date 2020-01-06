using System;
using System.Diagnostics;

namespace Solera.MediaInfo.E2eTests
{
    public class LiveProcess : IDisposable
    {
        private readonly Process _process;

        public LiveProcess(Process process)
        {
            _process = process;
        }

        public void Dispose()
        {
            if (!_process.HasExited)
            {
                try
                {
                    _process.Kill();
                }
                catch (InvalidOperationException)
                {
                    // in case of race condition
                }
            }

        }
    }
}
