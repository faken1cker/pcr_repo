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

        /// <summary>
        /// Indicates whether the PSU is currently connected.
        /// This is set by ConnectToPsu(...) if the regex match succeeds.
        /// </summary>
        public bool IsConnected { get; private set; } = false;

        /// <summary>
        /// Constructor that loads a config for the specified psuSetting
        /// (e.g. "vcm100mid" or "anyRigRnd320_24V"), then attempts to connect.
        /// </summary>
        public Controller(string psuSetting)
        {
            // 1) Load from Psu.json, searching for a 'setting' that matches psuSetting
            if (LoadPsuSetting(psuSetting))
            {
                // 2) Create a SerialManager for the selected baud rate
                _serialManager = new SerialManager(_currentPsuSetting.baudrate);

                // 3) Attempt to connect using the loaded regex
                if (ConnectToPsu(_currentPsuSetting.regex))
                {
                    // Optionally, we could call ApplyPsuSettings(_currentPsuSetting) here
                    // if we want to apply default volts/currents on startup.
                }
            }
            else
            {
                // If no match was found in Psu.json, log a warning
                Logger.LogAction($"PSU setting '{psuSetting}' not found in Psu.json.", "Warning");
            }
        }

        /// <summary>
        /// Loads Psu.json and searches for psuSetting (e.g. "vcm100mid").
        /// Returns true if found, false otherwise.
        /// </summary>
        private bool LoadPsuSetting(string psuSetting)
        {
            if (ReadPsuCfg()) // Populates _psu
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
        /// Reads the config file Psu.json from ../../config/Psu.json
        /// and stores it in the _psu list. Returns true if successful.
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
        /// Attempts to connect to the PSU using the specified psuRegex
        /// (e.g., "790" or "320"). If it matches the PSU's *IDN? response,
        /// we mark IsConnected = true, otherwise false.
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
        /// Disconnects from the PSU if connected.
        /// Logs a warning if the disconnection fails,
        /// and optionally unlocks the PSU keys at the end.
        /// </summary>
        public void Disconnect()
        {
            if (_serialManager != null && !_serialManager.DisconnectPsu())
            {
                Logger.LogAction("Failed to disconnect PSU.", "Warning");
            }
            else
            {
                // Optionally ensure PSU keys are unlocked before finalizing
                _serialManager?.LockKeyOnPowerSupply(false);
            }
            IsConnected = false;
        }

        /// <summary>
        /// Applies the default on/off, default voltage, and default current
        /// for each channel as specified in the loaded PsuCfg.
        /// Called automatically if you want to set all channels to their default states.
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
        /// Sets the requested voltage on a given channel (1-4), if connected.
        /// Also checks deployment mode so we don't write while read-only.
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
        /// Sets the requested current on a given channel (1-4), if connected.
        /// Checks deployment mode similarly to SetVoltage.
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
        /// Reads the user-defined (set) voltage from the PSU for a given channel.
        /// Logs the result or a warning if it fails.
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
        /// Power cycles the specified channel by toggling output off, waiting,
        /// then turning it on again. This version has been updated to handle 
        /// RND320 (single-channel) as well as RND790 (multi-channel).
        /// 
        /// All existing logic remains, including the distinct wait times for
        /// channel 4 vs. channels 1-3. We simply detect if regex == "320" and 
        /// call single-channel commands in that case.
        /// </summary>
        public async Task PowerCycleChannel(int channelToCycle)
        {
            // 1) Basic checks: connected, not null, and not in deployment read-only mode
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

            // 2) Decide how long to wait after turning off and before turning on.
            //    For channel 4, we have longer waits by design.
            int firstOffWait = (channelToCycle == 4) ? 3000 : 1500;
            int secondOffWait = (channelToCycle == 4) ? 3000 : 1500;
            int onWait = (channelToCycle == 4) ? 2500 : 1250;

            // 3) If this is an RND320 (single-channel) setting, we must ignore the requested channel
            //    and just call the single-channel command. Otherwise, we continue with multi-channel logic.
            bool singleChannel = (_currentPsuSetting.regex == "320");
            Logger.LogAction($"PowerCycleChannel: Start on channel {channelToCycle}, singleChannel={singleChannel}.");

            // 4) Turn the output off
            if (singleChannel)
            {
                // RND320 => always channel 1
                _serialManager.EnableVoutOnChannel1(false);
            }
            else
            {
                // RND790 => channel-specific
                _serialManager.EnableVoutOnChannel1_4(channelToCycle, false);
            }
            await Task.Delay(firstOffWait);

            // 5) Read the voltage after turning off
            double? voutBefore = await _serialManager.ReadVoutChannel1_4Async(channelToCycle);
            await Task.Delay(secondOffWait);

            // 6) Turn the output on
            if (singleChannel)
            {
                _serialManager.EnableVoutOnChannel1(true);
            }
            else
            {
                _serialManager.EnableVoutOnChannel1_4(channelToCycle, true);
            }
            await Task.Delay(onWait);

            // 7) Read the voltage after turning on
            double? voutAfter = await _serialManager.ReadVoutChannel1_4Async(channelToCycle);

            // 8) Log the final result
            Logger.LogAction($"Channel {channelToCycle} power cycle: was {voutBefore ?? -1} V, now {voutAfter ?? -1} V.", "Info");
        }

        /// <summary>
        /// Reads the measured (actual) voltage on a channel. Returns null if not connected.
        /// </summary>
        public async Task<double?> GetMeasuredVoltageFromChannelAsync(int channel)
        {
            if (!IsConnected || _serialManager == null) return null;

            double? voltage = await _serialManager.ReadVoutChannel1_4Async(channel);
            return voltage;
        }

        /// <summary>
        /// Reads the measured (actual) current on a channel. Returns null if not connected.
        /// </summary>
        public async Task<double?> GetMeasuredCurrentFromChannelAsync(int channel)
        {
            if (!IsConnected || _serialManager == null) return null;

            double? current = await _serialManager.ReadIoutChannel1_4Async(channel);
            return current;
        }

        /// <summary>
        /// Exposes the deployment status from SerialManager. 
        /// If this is active, we block write operations like voltage/current changes.
        /// </summary>
        public bool DeploymentActive
        {
            get { return _serialManager != null && _serialManager.DeploymentActive; }
        }
    }
}
