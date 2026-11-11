using System;
using System.Globalization;
using System.IO.Ports;
using System.Text.RegularExpressions;

namespace TRS2 {
    public class StepMicro : Step {
        private SerialPort? port;
        private int axis;

        public StepMicro(int index) : base(index) { }

        protected override void InitDev() {
            var set = Set.Step[iS];
            axis = set.Axis; // C# Set.Step.Axis is already 1-based (default = 1)
            port = new SerialPort($"COM{set.Com}", 2000000, Parity.None, 8, StopBits.One) {
                ReadTimeout = 800,
                WriteTimeout = 300,
                Handshake = Handshake.None,
                NewLine = "\n"
            };
            port.Open();
            SendDev('W', set.Lcd switch { "ALL" => 1, "END" => 2, _ => 0 });
            SendDev('H', set.Hold switch { "MEAS" => 1, "ON" => 2, _ => 0 });
            SendDev('M', set.FreqMin);
            SendDev('A', set.FreqDelta);
        }

        protected override void MoveDev(long goal) {
            SendDev('G', goal < 0 ? goal - 1 : goal);
        }

        protected override long TellPosDev() {
            SendDev('T', 0);
            string reply = port!.ReadLine();
            var numbers = Regex.Matches(reply, @"-?\d+");
            if (numbers.Count == 0) throw new FormatException("MICRO position reply: " + reply);
            return long.Parse(numbers[numbers.Count - 1].Value, CultureInfo.InvariantCulture);
        }

        protected override void SetVelDev(double freq) { SendDev('V', (long)freq); }
        protected override void StopDev() { SendDev('F', 0); }

        protected override void CloseDev() {
            if (port == null) return;
            try { if (port.IsOpen) SendDev('E', 0); }
            finally { port.Dispose(); port = null; }
        }

        private void SendDev(char cmd, long value) {
            port!.Write($"{axis}{cmd}{value}\n");
        }
    }
}
