using System;
using System.Globalization;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace powercontrolRNDdesign
{
    public class SerialManager
    {
        private string _comPort = "";
        private readonly int _baudRate;
        private readonly int _dataBit;
        private bool _connected = false;
        private string _id = "";
        private string _buffer = "";
        private int _channelRequested;

        private SemaphoreSlim _triggerForSerialBuffer;

        private SerialPort _serialPort;

        public SerialManager(int baudrate)
        {
            _baudRate = baudrate;
            _dataBit = 8;
        }

        #region Talk to the power supply

        private void SendCmdToSerial(string _cmd)
        {
            _serialPort.WriteLine(_cmd);
        }

        private void SendCmdToRnd(string _cmd)
        {
            if (_connected)
            {
                _serialPort.WriteLine(_cmd);
            }
        }

        private string ReadSerialPort()
        {
            return _serialPort.ReadExisting();
        }

        /// <summary>
        /// Reads data asynchronously, no debug logs to avoid spamming the log file.
        /// </summary>
        private async Task ReadSerialPortAsync()
        {
            if (_connected)
            {
                _triggerForSerialBuffer = new SemaphoreSlim(0, 1);
                await Task.Run(() => _triggerForSerialBuffer.WaitAsync());
            }
            else
            {
                Logger.LogAction("Not connected when trying to read buffer", "Warning");
            }
        }
        #endregion

        #region Commands to power supply

        public void LockKeyOnPowerSupply(bool lockState)
        {
            if (lockState) SendCmdToRnd("LOCK:1");
            else SendCmdToRnd("LOCK:0");
        }

        public void EnableVoutOnChannel1(bool channelState)
        {
            if (_connected)
            {
                string onOff = channelState ? "1" : "0";
                string cmd = "OUT" + onOff;
                SendCmdToRnd(cmd);
            }
        }

        public void EnableVoutOnChannel1_4(int channelToSet, bool channelState)
        {
            if (_connected && CheckValidChannel(channelToSet))
            {
                string onOff = channelState ? "1" : "0";
                string cmd = "OUT" + channelToSet + ":" + onOff;
                SendCmdToRnd(cmd);
            }
        }

        public void SetVoutChannel1_4_0_30V(int channelToSet, double vout)
        {
            if (CheckValidChannel(channelToSet) && CheckValidVout(vout))
            {
                string cmd = $"VSET{channelToSet}:{vout.ToString("F3", CultureInfo.InvariantCulture)}";
                SendCmdToRnd(cmd);
            }
        }

        public void SetIoutLimitChannel1_4_0_5A(int channelToSet, double iout)
        {
            if (CheckValidChannel(channelToSet) && CheckValidIout(iout))
            {
                string cmd = $"ISET{channelToSet}:{iout.ToString("F3", CultureInfo.InvariantCulture)}";
                SendCmdToRnd(cmd);
            }
        }

        /// <summary>
        /// Reads the actual output voltage (VOUTx?) for channel 1–4.
        /// No "Read Vout async..." log statements, just parse failures if needed.
        /// </summary>
        public async Task<double?> ReadVoutChannel1_4Async(int channelToRead)
        {
            _channelRequested = channelToRead;
            string cmd = $"VOUT{channelToRead}?";
            SendCmdToRnd(cmd);

            await ReadSerialPortAsync();

            _buffer = _buffer.Trim();
            if (double.TryParse(_buffer, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsedVal))
            {
                return parsedVal;
            }
            else
            {
                Logger.LogAction($"Failed to parse Vout from response: {_buffer}", "Warning");
                return null;
            }
        }

        /// <summary>
        /// Reads the actual output current (IOUTx?) for channel 1–4.
        /// No "Read Iout async..." log statements, just parse failures if needed.
        /// </summary>
        public async Task<double?> ReadIoutChannel1_4Async(int channelToRead)
        {
            _channelRequested = channelToRead;
            string cmd = $"IOUT{channelToRead}?";
            SendCmdToRnd(cmd);

            await ReadSerialPortAsync();

            _buffer = _buffer.Trim();
            if (double.TryParse(_buffer, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsedVal))
            {
                return parsedVal;
            }
            else
            {
                Logger.LogAction($"Failed to parse Iout from response: {_buffer}", "Warning");
                return null;
            }
        }

        private bool CheckValidChannel(int ch) => ch >= 1 && ch <= 4;
        private bool CheckValidVout(double v) => v >= 0.0 && v <= 30.0;
        private bool CheckValidIout(double i) => i >= 0.0 && i <= 5.0;

        #endregion

        #region Handle connection to the power supply

        public bool ConnectToPsu(string psuRegEx)
        {
            _connected = false;

            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                if (TryOpenComPort(port))
                {
                    if (_serialPort.IsOpen)
                    {
                        SendCmdToSerial("*IDN?");
                        Thread.Sleep(1500);
                        _id = ReadSerialPort();

                        Regex regex = new Regex(psuRegEx);
                        Match match = regex.Match(_id);

                        if (match.Success)
                        {
                            _comPort = port;
                            _serialPort.DiscardOutBuffer();
                            _serialPort.DiscardInBuffer();
                            _serialPort.DataReceived += OnTriggerReadSerialBufferAndWriteBackToSystemVariables;

                            _connected = true;
                            Logger.LogAction($"Connected with regex {psuRegEx}", "Info");
                            break;
                        }
                        else
                        {
                            _serialPort.Close();
                        }
                    }
                }
            }

            return _connected;
        }

        public bool DisconnectPsu()
        {
            if (_connected)
            {
                LockKeyOnPowerSupply(false);
                _serialPort.Close();
                _connected = false;
                Logger.LogAction("Disconnected from PSU", "Info");
                return true;
            }
            else
            {
                Logger.LogAction("Failed to disconnect (PSU was not connected).", "Warning");
                return false;
            }
        }

        public bool IsConnected() => _connected;

        private bool TryOpenComPort(string portName)
        {
            bool result = false;
            try
            {
                _serialPort = new SerialPort(portName, _baudRate, Parity.None, _dataBit, StopBits.One)
                {
                    Handshake = Handshake.None,
                    ReadTimeout = 5000
                };
                _serialPort.Open();
                result = true;
            }
            catch (Exception ex)
            {
                _serialPort.Close();
                Logger.LogAction($"Could not open port {portName}: {ex.Message}", "Warning");
            }
            return result;
        }

        private void OnTriggerReadSerialBufferAndWriteBackToSystemVariables(object sender, SerialDataReceivedEventArgs e)
        {
            _buffer = _serialPort.ReadExisting();

            _serialPort.DiscardInBuffer();
            _serialPort.DiscardOutBuffer();
            _serialPort.BaseStream.Flush();

            _triggerForSerialBuffer.Release();
        }

        #endregion

        public override string ToString()
        {
            return _connected
                ? $"{_id} connected to port {_comPort}"
                : "Not connected";
        }

        /// <summary>
        /// Reads the "Set Voltage" from the PSU for a given channel, i.e., 
        /// the user-set target, not necessarily the actual measured Vout.
        /// Also doesn't log each read call, just parse failures.
        /// </summary>
        public async Task<double?> GetSetVoltageAsync(int channelToRead)
        {
            if (!CheckValidChannel(channelToRead))
            {
                Logger.LogAction($"Invalid channel: {channelToRead}", "Warning");
                return null;
            }

            string cmd = $"VSET{channelToRead}?";
            SendCmdToRnd(cmd);

            await ReadSerialPortAsync();

            _buffer = _buffer.Trim();
            if (double.TryParse(_buffer, NumberStyles.Float, CultureInfo.InvariantCulture, out double voltage))
            {
                // We could log success or skip it. If you want to see fewer logs, skip it:
                // Logger.LogAction($"Read set voltage for channel {channelToRead}: {voltage} V", "Info");
                return voltage;
            }
            else
            {
                Logger.LogAction($"Failed to parse set voltage from response: {_buffer}", "Warning");
                return null;
            }
        }
    }
}
