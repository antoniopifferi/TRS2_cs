using System;
using System.Diagnostics;

namespace TRS2 {
    public class StepTest : Step {
        private long start;
        private long goal;
        private long started;
        private double speed = 100;

        public StepTest(int index) : base(index) { }

        protected override void SetVelDev(double freq) {
            speed = freq > 0 ? freq : 100;
        }

        protected override void MoveDev(long goal) {
            start = TellPosDev();
            this.goal = goal;
            started = Stopwatch.GetTimestamp();
        }

        protected override long TellPosDev() {
            long distance = goal - start;
            if (distance == 0) return goal;
            long moved = (long)((Stopwatch.GetTimestamp() - started) / (double)Stopwatch.Frequency * speed);
            if (moved >= Math.Abs(distance)) return goal;
            return start + Math.Sign(distance) * moved;
        }

        protected override void StopDev() {
            start = TellPosDev();
            goal = start;
        }
    }
}
