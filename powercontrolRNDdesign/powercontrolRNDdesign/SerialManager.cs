using System;
using System.Globalization;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace powercontrolRNDdesign
{
    // SerialManager handles all low-level serial comms with the PSU.
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

        /// <summary>
        /// Checks the deployment flag from the registry.
        /// The registry key is HKLM\SOFTWARE\V3rigInfo and the DWORD value "deployment_status" is expected.
        /// A value of 1 indicates deployment active (read-only), while 0 means normal (read/write).
        /// </summary>
        private bool IsDeploymentActive()
        {
            try
            {
                // Specify the registry view explicitly.
                using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (RegistryKey key = baseKey.OpenSubKey(@"SOFTWARE\V3rigInfo", false))
                {
                    if (key == null)
                    {
                        Logger.LogAction("Registry key 'SOFTWARE\\V3rigInfo' not found. Assuming deployment inactive.", "Warning");
                        return false;
                    }

                    object rawValue = key.GetValue("deployment_status");
                    if (rawValue == null)
                    {
                        Logger.LogAction("Registry value 'deployment_status' is missing. This indicates an error.", "Error");
                        return false;
                    }

                    int status = (int)rawValue;
                    return status == 1;
                }
            }
            catch (Exception ex)
            {
                Logger.LogAction($"Error reading deployment_status from registry: {ex.Message}", "Error");
                return false;
            }
        }

        public bool DeploymentActive
        {
            get { return IsDeploymentActive(); }
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
        /// Reads data asynchronously, without spamming debug logs.
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

        /// <summary>
        /// Locks or unlocks the PSU keys.
        /// </summary>
        public void LockKeyOnPowerSupply(bool lockState)
        {
            // Write operation – check deployment flag
            if (IsDeploymentActive())
            {
                Logger.LogAction("Deployment is active, blocking LockKeyOnPowerSupply command.", "Warning");
                return;
            }

            if (lockState) SendCmdToRnd("LOCK:1");
            else SendCmdToRnd("LOCK:0");
        }

        /// <summary>
        /// Enables/disables output for a single-channel PSU.
        /// </summary>
        public void EnableVoutOnChannel1(bool channelState)
        {
            // Write operation – check deployment flag
            if (IsDeploymentActive())
            {
                Logger.LogAction("Deployment is active, blocking EnableVoutOnChannel1 command.", "Warning");
                return;
            }

            if (_connected)
            {
                string onOff = channelState ? "1" : "0";
                string cmd = "OUT" + onOff;
                SendCmdToRnd(cmd);
            }
        }

        /// <summary>
        /// Enables/disables output for a multi-channel PSU.
        /// </summary>
        public void EnableVoutOnChannel1_4(int channelToSet, bool channelState)
        {
            // Write operation – check deployment flag
            if (IsDeploymentActive())
            {
                Logger.LogAction("Deployment is active, blocking EnableVoutOnChannel1_4 command.", "Warning");
                return;
            }

            if (_connected && CheckValidChannel(channelToSet))
            {
                string onOff = channelState ? "1" : "0";
                string cmd = "OUT" + channelToSet + ":" + onOff;
                SendCmdToRnd(cmd);
            }
        }

        /// <summary>
        /// Sets the voltage for a specific channel.
        /// </summary>
        public void SetVoutChannel1_4_0_30V(int channelToSet, double vout)
        {
            // Write operation – check deployment flag
            if (IsDeploymentActive())
            {
                Logger.LogAction("Deployment is active, blocking SetVoutChannel1_4_0_30V command.", "Warning");
                return;
            }

            if (CheckValidChannel(channelToSet) && CheckValidVout(vout))
            {
                string cmd = $"VSET{channelToSet}:{vout.ToString("F3", CultureInfo.InvariantCulture)}";
                SendCmdToRnd(cmd);
            }
        }

        /// <summary>
        /// Sets the current limit for a specific channel.
        /// </summary>
        public void SetIoutLimitChannel1_4_0_5A(int channelToSet, double iout)
        {
            // Write operation – check deployment flag
            if (IsDeploymentActive())
            {
                Logger.LogAction("Deployment is active, blocking SetIoutLimitChannel1_4_0_5A command.", "Warning");
                return;
            }

            if (CheckValidChannel(channelToSet) && CheckValidIout(iout))
            {
                string cmd = $"ISET{channelToSet}:{iout.ToString("F3", CultureInfo.InvariantCulture)}";
                SendCmdToRnd(cmd);
            }
        }

        /// <summary>
        /// Reads the actual output voltage (VOUTx?) for channel 1–4.
        /// No detailed debug logs are produced unless parsing fails.
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
        /// No detailed debug logs are produced unless parsing fails.
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

        public bool ConnectToPsu(string psuRegEx)  // Enumerates COM ports, tries to match psuRegEx against the PSU's *IDN? response.
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

        public bool DisconnectPsu() // If connected, send lock off, close the port, log the result.
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

        private bool TryOpenComPort(string portName)    // Attempts to open the given COM port at _baudRate.
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
        /// Reads the "Set Voltage" from the PSU for a given channel,
        /// i.e., the user-set target, not necessarily the actual measured Vout.
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
