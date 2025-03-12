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
        private List<PsuCfg> _psu; // Holds all possible PSU configs from Psu.json
        private PsuCfg _currentPsuSetting; // The active PSU setting
        public bool IsConnected { get; private set; } = false;

        /// <summary>
        /// Constructor that loads a config for the specified psuSetting
        /// and attempts to connect using the loaded baudrate and regex.
        /// </summary>
        /// <param name="psuSetting">A key from Psu.json, e.g. "vcm100mid" or similar</param>
        public Controller(string psuSetting)
        {
            // Attempt to load Psu.json, find the specified setting, and connect.
            if (LoadPsuSetting(psuSetting))
            {
                _serialManager = new SerialManager(_currentPsuSetting.baudrate);

                if (ConnectToPsu(_currentPsuSetting.regex))
                {
                   // ApplyPsuSettings(_currentPsuSetting); Remove comment to apply default settings on startup
                }
            }
            else
            {
                Logger.LogAction($"PSU setting '{psuSetting}' not found in Psu.json.", "Warning");
            }
        }

        /// <summary>
        /// Loads Psu.json from ../../config/Psu.json and searches for the given psuSetting.
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
                Logger.LogError($"Failed to read Psu.json: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Connects the SerialManager to the PSU using a regex to match its identity.
        /// Sets IsConnected to true if match is found.
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
        /// Disconnects from the PSU, logging if unsuccessful.
        /// </summary>
        public void Disconnect()
        {
            if (_serialManager != null && !_serialManager.DisconnectPsu())
            {
                Logger.LogAction("Failed to disconnect PSU.", "Warning");
            }
            else
            {
                // Optionally unlock PSU keys here.
                _serialManager?.LockKeyOnPowerSupply(false);
            }
            IsConnected = false;
        }

        /// <summary>
        /// Applies default on/off state, default voltage, and default current for each channel.
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
        /// Sets voltage on the given channel (1–4) if connected.
        /// Throws an exception if the deployment flag is active.
        /// </summary>
        public void SetVoltage(int channel, double voltage)
        {
            if (!IsConnected || _serialManager == null) return;
            if (_serialManager.DeploymentActive)
            {
                throw new InvalidOperationException("Operation failed: Deployment status active (read-only).");
            }
            _serialManager.SetVoutChannel1_4_0_30V(channel, voltage);
            Logger.LogAction($"Set voltage on channel {channel} to {voltage} V.");
        }

        /// <summary>
        /// Sets current limit on the given channel (1–4) if connected.
        /// Throws an exception if the deployment flag is active.
        /// </summary>
        public void SetCurrent(int channel, double current)
        {
            if (!IsConnected || _serialManager == null) return;
            if (_serialManager.DeploymentActive)
            {
                throw new InvalidOperationException("Operation failed: Deployment status active (read-only).");
            }
            _serialManager.SetIoutLimitChannel1_4_0_5A(channel, current);
            Logger.LogAction($"Set current on channel {channel} to {current} A.");
        }

        /// <summary>
        /// Reads the "set voltage" for a particular channel (the user-defined voltage).
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
        /// Initiates a power cycle on the given channel (1–4) asynchronously.
        /// Throws an exception if the deployment flag is active.
        /// </summary>
        public async Task PowerCycleChannel(int channelToCycle)
        {
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
            if (_serialManager.DeploymentActive)
            {
                throw new InvalidOperationException("Operation failed: Deployment status active (read-only).");
            }

            // Determine wait times: if channel 4, use longer delays.
            int firstOffWait = (channelToCycle == 4) ? 3000 : 1500;
            int secondOffWait = (channelToCycle == 4) ? 3000 : 1500;
            int onWait = (channelToCycle == 4) ? 2500 : 1250;

            // 1) Turn off the channel.
            _serialManager.EnableVoutOnChannel1_4(channelToCycle, false);
            await Task.Delay(firstOffWait);

            // Read voltage after turning off.
            double? voutBefore = await _serialManager.ReadVoutChannel1_4Async(channelToCycle);
            await Task.Delay(secondOffWait);

            // 2) Turn on the channel.
            _serialManager.EnableVoutOnChannel1_4(channelToCycle, true);
            await Task.Delay(onWait);

            // Read voltage after turning on.
            double? voutAfter = await _serialManager.ReadVoutChannel1_4Async(channelToCycle);

            Logger.LogAction($"Channel {channelToCycle} power cycle: was {voutBefore ?? -1} V, now {voutAfter ?? -1} V.", "Info");
        }

        /// <summary>
        /// Reads the measured (actual) voltage for a particular channel (1–4).
        /// </summary>
        public async Task<double?> GetMeasuredVoltageFromChannelAsync(int channel)
        {
            if (!IsConnected || _serialManager == null) return null;

            double? voltage = await _serialManager.ReadVoutChannel1_4Async(channel);
            return voltage;
        }

        /// <summary>
        /// Reads the measured (actual) current for a particular channel (1–4).
        /// </summary>
        public async Task<double?> GetMeasuredCurrentFromChannelAsync(int channel)
        {
            if (!IsConnected || _serialManager == null) return null;

            double? current = await _serialManager.ReadIoutChannel1_4Async(channel);
            return current;
        }

        /// <summary>
        /// Exposes the deployment status from SerialManager.
        /// </summary>
        public bool DeploymentActive
        {
            get { return _serialManager != null && _serialManager.DeploymentActive; }
        }
    }
}
