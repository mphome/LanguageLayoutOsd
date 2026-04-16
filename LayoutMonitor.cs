using System;
using System.Threading;

namespace LanguageLayoutOsd
{
    internal sealed class LayoutMonitor : IDisposable
    {
        private readonly LayoutService _layoutService;
        private readonly int _pollingIntervalMs;
        private readonly object _lock = new object();
        private readonly object _stateLock = new object();

        private Timer _timer;
        private string _lastLayoutCode;
        private bool _isRunning;

        public LayoutMonitor(LayoutService layoutService, int pollingIntervalMs)
        {
            _layoutService = layoutService;
            _pollingIntervalMs = pollingIntervalMs;
        }

        public event Action<string> LayoutChanged;

        public void Start()
        {
            lock (_lock)
            {
                if (_isRunning)
                {
                    return;
                }

                _timer = new Timer(OnTimerTick, null, 0, _pollingIntervalMs);
                _isRunning = true;
                DiagnosticLog.Write("LayoutMonitor.Start. Interval=" + _pollingIntervalMs + "ms");
            }
        }

        private void OnTimerTick(object state)
        {
            string current = _layoutService.GetCurrentLayoutCode();
            if (string.IsNullOrEmpty(current))
            {
                return;
            }

            bool changed;
            lock (_stateLock)
            {
                changed = !string.Equals(current, _lastLayoutCode, StringComparison.OrdinalIgnoreCase);
                if (changed)
                {
                    _lastLayoutCode = current;
                }
            }

            if (!changed)
            {
                return;
            }

            var handler = LayoutChanged;
            if (handler != null)
            {
                try
                {
                    handler(current);
                }
                catch (Exception ex)
                {
                    DiagnosticLog.WriteException("LayoutChanged handler failed", ex);
                }
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (!_isRunning)
                {
                    return;
                }

                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                }

                _isRunning = false;
                DiagnosticLog.Write("LayoutMonitor.Dispose");
            }
        }
    }
}
