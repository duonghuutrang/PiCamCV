using System;
using System.Diagnostics;
using System.Drawing;
using Kraken.Core;
using PiCamCV.ConsoleApp.Runners.PanTilt;

namespace PiCamCV.Common.PanTilt.Controllers.multimode
{
    public abstract class TimeoutStateManager<TOutput> : StateManager
        where TOutput: TrackingCameraPanTiltProcessOutput
    {
        private readonly Stopwatch _stopWatch;
        private readonly ProcessingMode _trackingMode;

        public Point LastDetection { get; set; }


        protected abstract string ObjectName { get; }

        public virtual TimeSpan AbandonDetectionAfterMissing => TimeSpan.FromSeconds(5);

        public TimeSpan TimeSinceLastDetection => _stopWatch.Elapsed;

        protected void ResetTimer()
        {
            _stopWatch.Restart();
        }

        public void Reset()
        {
            ResetTimer();
            LastDetection = Point.Empty;
        }

        protected TimeoutStateManager(ProcessingMode trackingMode, IScreen screen) :base(screen)
        {
            _trackingMode = trackingMode;
            _stopWatch = new Stopwatch();
        }

        public ProcessingMode AcceptOutput(TOutput output)
        {
            if (output.IsDetected)
            {
                LastDetection = output.Target;
                ResetTimer();

                if (LastDetection == Point.Empty)
                {
                    Screen.WriteLine($"{ObjectName} detected");
                }
            }


            if (TimeSinceLastDetection > AbandonDetectionAfterMissing)
            {
                Screen.WriteLine($"{ObjectName} deemed lost afer {AbandonDetectionAfterMissing.ToHumanReadable()}");
                return ProcessingMode.Autonomous;
            }

            // Keep tracking
            return _trackingMode;
        }
    }
}