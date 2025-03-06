using powercontrolRNDdesign.psu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace powercontrolRNDdesign
{
    public class Controller
    {
        private SerialManager _serialManager;
        private List<PsuCfg> _psu; // Holds all possible PSU configs from psu.json
        private PsuCfg _currentPsuSetting; // The active PSU setting
        public bool IsConnected { get; private set; } = false;

        /// <summary>
        /// Constructor that loads a config for the specified psuSetting
        /// and attempts to connect using the loaded baudrate and regex.
        /// </summary>
        /// <param name="psuSetting">A key from psu.json, e.g. "vcm100Mid" or similar</param>
        public Controller(string psuSetting)
        {
            // Attempt to load psu.json, find the specified setting, and connect
            if (LoadPsuSetting(psuSetting))
            {
                _serialManager = new SerialManager(_currentPsuSetting.baudrate);

                if (ConnectToPsu(_currentPsuSetting.regex))
                {
                    ApplyPsuSettings(_currentPsuSetting);
                }
            }
            else
            {
                Logger.LogAction($"PSU setting '{psuSetting}' not found in psu.json.", "Warning");
            }
        }

        /// <summary>
        /// Loads psu.json from ../../config/Psu.json,
        /// then searches for the given psuSetting in the loaded list.
        /// </summary>
        private bool LoadPsuSetting(string psuSetting)
        {
            if (ReadPsuCfg()) // populates _psu
            {
                foreach (PsuCfg candidate in _psu)
                {
                    if (candidate.setting == psuSetting)
                    {
                        _currentPsuSetting = candidate;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Reads the config file Psu.json and stores it into _psu.
        /// Returns true if successful, false otherwise.
        /// </summary>
        private bool ReadPsuCfg()
        {
            try
            {
                string json = File.ReadAllText("../../config/Psu.json");
                _psu = JsonSerializer.Deserialize<List<PsuCfg>>(json);
                return _psu != null && _psu.Count > 0;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to read psu.json: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Connects the serial manager to the PSU, using a regex to match its identity.
        /// Sets IsConnected to true if match is found, otherwise false.
        /// </summary>
        private bool ConnectToPsu(string psuRegex)
        {
            if (_serialManager.ConnectToPsu(psuRegex))
            {
                IsConnected = true;
                Logger.LogAction($"Connected to PSU with setting '{_currentPsuSetting.setting}'.");
            }
            else
            {
                Logger.LogAction("Failed to connect to PSU.", "Warning");
                IsConnected = false;
            }
            return IsConnected;
        }

        /// <summary>
        /// Disconnects from the PSU, logs if unsuccessful.
        /// </summary>
        public void Disconnect()
        {
            if (_serialManager != null && !_serialManager.DisconnectPsu())
            {
                Logger.LogAction("Failed to disconnect PSU.", "Warning");
            }
            else
            {
                // Optionally unlock PSU keys here
                _serialManager?.LockKeyOnPowerSupply(false);
            }
            IsConnected = false;
        }

        /// <summary>
        /// Applies default on/off state, default voltage, and default current
        /// for each channel defined in the current PSU config (e.g. 1–4 channels).
        /// </summary>
        private void ApplyPsuSettings(PsuCfg p)
        {
            if (_serialManager == null) return;

            foreach (var channelCfg in p.channel)
            {
                _serialManager.EnableVoutOnChannel1_4(channelCfg.id, channelCfg.defaultOn);
                _serialManager.SetVoutChannel1_4_0_30V(channelCfg.id, channelCfg.defaultVout);
                _serialManager.SetIoutLimitChannel1_4_0_5A(channelCfg.id, channelCfg.defaultImax);
            }
            Logger.LogAction($"Default PSU settings applied for '{p.setting}'.");
        }

        /// <summary>
        /// Sets voltage on the given channel (1–4), if connected.
        /// </summary>
        public void SetVoltage(int channel, double voltage)
        {
            if (!IsConnected || _serialManager == null) return;
            _serialManager.SetVoutChannel1_4_0_30V(channel, voltage);
            Logger.LogAction($"Set voltage on channel {channel} to {voltage} V.");
        }

        /// <summary>
        /// Sets current limit on the given channel (1–4), if connected.
        /// </summary>
        public void SetCurrent(int channel, double current)
        {
            if (!IsConnected || _serialManager == null) return;
            _serialManager.SetIoutLimitChannel1_4_0_5A(channel, current);
            Logger.LogAction($"Set current on channel {channel} to {current} A.");
        }

        /// <summary>
        /// Reads the "set voltage" for a particular channel, i.e., the user-defined
        /// voltage setting (not necessarily the actual measured output).
        /// </summary>
        public async Task<double?> GetSetVoltageFromChannelAsync(int channel)
        {
            if (_serialManager != null && _serialManager.IsConnected())
            {
                double? voltage = await _serialManager.GetSetVoltageAsync(channel);
                if (voltage.HasValue)
                {
                    Logger.LogAction($"Retrieved set voltage from channel {channel}: {voltage.Value} V.");
                    return voltage;
                }
                else
                {
                    Logger.LogAction($"Failed to read set voltage from channel {channel}.", "Warning");
                    return null;
                }
            }
            else
            {
                Logger.LogAction("PSU not connected, cannot read set voltage.", "Warning");
                return null;
            }
        }

        /// <summary>
        /// Initiates a power cycle on the given channel (1–4), asynchronously.
        /// Disables, waits, re-enables, etc.
        /// </summary>
        public async Task PowerCycleChannel(int channelToCycle)
        {
            // 1) Check if SerialManager is valid and connected
            if (_serialManager == null)
            {
                Logger.LogAction("SerialManager is null; cannot power cycle.", "Warning");
                return;
            }
            if (!_serialManager.IsConnected())
            {
                Logger.LogAction("PSU not connected; cannot power cycle.", "Warning");
                return;
            }

            // For channel 4, Vector hardware needs more time to powercycle
            // If it's channel 4, use custom times. Otherwise, use the defaults from your original code.

            // First wait after turning the channel off
            int firstOffWait = (channelToCycle == 4) ? 3000 : 1500;
            // Second wait before turning channel on
            int secondOffWait = (channelToCycle == 4) ? 3000 : 1500;
            // Final wait after turning the channel on
            int onWait = (channelToCycle == 4) ? 2500 : 1250;

            // 2) Turn off the channel
            _serialManager.EnableVoutOnChannel1_4(channelToCycle, false);
            await Task.Delay(firstOffWait);

            // Read the voltage after turning off
            double? voutBefore = await _serialManager.ReadVoutChannel1_4Async(channelToCycle);
            await Task.Delay(secondOffWait);

            // 3) Turn on the channel
            _serialManager.EnableVoutOnChannel1_4(channelToCycle, true);
            await Task.Delay(onWait);

            // Read the voltage after turning on
            double? voutAfter = await _serialManager.ReadVoutChannel1_4Async(channelToCycle);

            Logger.LogAction(
                $"Channel {channelToCycle} power cycle: was {voutBefore ?? -1} V, now {voutAfter ?? -1} V.",
                "Info"
            );
        }

        /// <summary>
        /// Reads the measured (actual) voltage for a particular channel (1–4),
        /// using the same approach as power cycle's read method. If your PSU
        /// indeed uses VOUTx? to report actual measured voltage, this is valid.
        /// </summary>
        public async Task<double?> GetMeasuredVoltageFromChannelAsync(int channel)
        {
            if (!IsConnected || _serialManager == null) return null;

            double? voltage = await _serialManager.ReadVoutChannel1_4Async(channel);
            return voltage;
        }

        /// <summary>
        /// Reads the measured (actual) current for a particular channel (1–4).
        /// This calls SerialManager's ReadIoutChannel1_4Async method (IOUTx?).
        /// </summary>
        public async Task<double?> GetMeasuredCurrentFromChannelAsync(int channel)
        {
            if (!IsConnected || _serialManager == null) return null;

            double? current = await _serialManager.ReadIoutChannel1_4Async(channel);
            return current;
        }
    }
}
