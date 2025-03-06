using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using powercontrolRNDdesign;
using powercontrolRNDdesign.psu;

namespace powercontrolRNDdesign
{
    /// <summary>
    /// A console-oriented command controller that:
    /// 1) Loads a PSU setting from Psu.json
    /// 2) Connects to the PSU via SerialManager
    /// 3) Executes console commands like "powerCycle", "applySetting", "powerCycleWithSetting"
    /// 4) Writes results to the console (0 or -1) for automation
    /// 5) Uses logger instead of log4net
    /// </summary>
    internal class ControllerCmd
    {
        private SerialManager _serialManager;

        private readonly string _cmd;
        private readonly string _target;
        private readonly string _psuSetting;

        private List<PsuCfg> _allPsu;
        private PsuCfg _currentPsuSetting;

        /// <summary>
        /// Constructor that captures the incoming command/target/psuSetting,
        /// then calls MainTask() to do the console flow.
        /// </summary>
        public ControllerCmd(string cmd, string target, string psuSetting)
        {
            _cmd = cmd;
            _target = target;
            _psuSetting = psuSetting;
            _currentPsuSetting = new PsuCfg();

            MainTask();
        }

        /// <summary>
        /// The main flow:
        /// 1) Load the specified PSU setting from Psu.json
        /// 2) Connect to PSU
        /// 3) Run the requested command
        /// 4) Disconnect
        /// Writes "0" or "-1" to the console on success/failure.
        /// </summary>
        private void MainTask()
        {
            if (LoadPsuSetting(_psuSetting))
            {
                Logger.LogAction($"Loaded PSU setting '{_currentPsuSetting.setting}' OK.");
                _serialManager = new SerialManager(_currentPsuSetting.baudrate);

                if (ConnectToPsu(_currentPsuSetting.regex))
                {
                    Logger.LogAction("START Task.Run ExecuteCmd");
                    // Because ExecuteCmd is async, we use Task.Run(...).Wait() or .Result
                    // to block in a console environment.
                    Task.Run(() => ExecuteCmd()).Wait();
                    Logger.LogAction("STOP Task.Run ExecuteCmd");

                    Disconnect();
                }
                else
                {
                    Logger.LogAction(
                        $"Could not connect to power supply with regex '{_currentPsuSetting.regex}'",
                        "Warning"
                    );
                    Console.WriteLine("-1");
                }
            }
            else
            {
                // Couldn’t load that PSU setting
                Console.WriteLine("-1");
                Logger.LogAction($"No support for psuSetting '{_psuSetting}'", "Warning");
            }
        }

        /// <summary>
        /// Attempts to connect using the given psuRegex. Returns true if connected.
        /// </summary>
        private bool ConnectToPsu(string psuRegex)
        {
            bool resultConnect = false;

            if (_serialManager.ConnectToPsu(psuRegex))
            {
                resultConnect = true;
                Logger.LogAction($"Connected with PSU setting '{_currentPsuSetting.setting}'.");
            }
            else
            {
                Logger.LogAction("Failed to connect to PSU.", "Warning");
            }

            return resultConnect;
        }

        /// <summary>
        /// Disconnect from the PSU
        /// </summary>
        private void Disconnect()
        {
            if (_serialManager != null && _serialManager.DisconnectPsu())
            {
                Logger.LogAction($"Power supply '{_currentPsuSetting.setting}' disconnected.");
                _serialManager = null;
            }
            else
            {
                Logger.LogAction(
                    $"Could not disconnect power supply '{_currentPsuSetting.setting}'",
                    "Warning"
                );
            }
        }

        /// <summary>
        /// Executes the command specified by _cmd: applySetting, powerCycle, or powerCycleWithSetting
        /// asynchronously.
        /// </summary>
        private async Task ExecuteCmd()
        {
            switch (_cmd)
            {
                case "applySetting":
                    Logger.LogAction($"Execute cmd '{_cmd}'");
                    CmdApplySetting();
                    break;

                case "powerCycle":
                    Logger.LogAction($"Execute cmd '{_cmd}'");
                    await CmdPowerCycle();
                    break;

                case "powerCycleWithSetting":
                    Logger.LogAction($"Execute cmd '{_cmd}'");
                    await CmdPowerCycleWithSetting();
                    break;

                default:
                    Logger.LogAction($"No support for cmd '{_cmd}'", "Warning");
                    break;
            }
        }

        /// <summary>
        /// Apply the default PSU settings from the loaded config, then print 0.
        /// </summary>
        private void CmdApplySetting()
        {
            Logger.LogAction($"Apply settings for '{_currentPsuSetting.setting}'");
            ApplyPsuSettings(_currentPsuSetting);
        }

        /// <summary>
        /// Finds the channel for _target, power cycles it, prints 0 on success or -1 on failure.
        /// </summary>
        private async Task CmdPowerCycle()
        {
            int channelToCycle = GetChannelForTarget(_currentPsuSetting);
            if (channelToCycle != 0)
            {
                Logger.LogAction($"Perform power cycle for '{_target}' on channel {channelToCycle}");
                await PowerCycleChannel(channelToCycle);
                Console.WriteLine("0");
            }
            else
            {
                Console.WriteLine("-1");
                Logger.LogAction($"No support for target '{_target}'", "Warning");
            }
        }

        /// <summary>
        /// First applies default PSU settings, then does a power cycle on the matching channel.
        /// Prints -1 if no channel matched.
        /// </summary>
        private async Task CmdPowerCycleWithSetting()
        {
            int channelToCycle = GetChannelForTarget(_currentPsuSetting);
            if (channelToCycle != 0)
            {
                Logger.LogAction($"Apply settings for '{_currentPsuSetting.setting}'");
                ApplyPsuSettings(_currentPsuSetting);

                Logger.LogAction($"Perform power cycle for '{_target}' on channel {channelToCycle}");
                await PowerCycleChannel(channelToCycle);
            }
            else
            {
                Console.WriteLine("-1");
                Logger.LogAction($"No support for target '{_target}'", "Warning");
            }
        }

        /// <summary>
        /// Loads psu.json, finds the specified PSU setting. Returns true if found.
        /// </summary>
        private bool LoadPsuSetting(string psuSetting)
        {
            bool loadResult = false;
            if (ReadPsuCfg())
            {
                foreach (var psu in _allPsu)
                {
                    if (psu.setting == psuSetting)
                    {
                        _currentPsuSetting = psu;
                        loadResult = true;
                        Logger.LogAction($"Power supply settings loaded ok for '{_currentPsuSetting.setting}'.");
                    }
                }
            }

            return loadResult;
        }

        /// <summary>
        /// Reads psu.json from ../../config/Psu.json. Adjust as needed
        /// </summary>
        private bool ReadPsuCfg()
        {
            Logger.LogAction("Reading psu.json config file.");
            bool resultReadJson = true;

            string jsonPath = "../../config/Psu.json";
            string json = File.ReadAllText(jsonPath);
            _allPsu = JsonSerializer.Deserialize<List<PsuCfg>>(json);

            if (_allPsu == null || _allPsu.Count < 1)
            {
                resultReadJson = false;
            }

            return resultReadJson;
        }

        /// <summary>
        /// Applies each channel's defaultVout, defaultImax, and defaultOn from the config to the PSU.
        /// Then prints 0 to console if success.
        /// </summary>
        private void ApplyPsuSettings(PsuCfg p)
        {
            foreach (var item in p.channel)
            {
                RequestRightEnableVoutRndVariantCommand(item.id, item.defaultOn);
                _serialManager.SetVoutChannel1_4_0_30V(item.id, item.defaultVout);
                _serialManager.SetIoutLimitChannel1_4_0_5A(item.id, item.defaultImax);

                Logger.LogAction($"Channel {item.id}", "Info");
                Logger.LogAction($"Vout {item.defaultVout} V", "Info");
                Logger.LogAction($"Imax {item.defaultImax} A", "Info");
                Logger.LogAction($"Enable {item.defaultOn}", "Info");
            }
            Console.WriteLine("0");
        }

        /// <summary>
        /// Finds which channel has usage==_target and defaultOn==true. Returns that channel or 0 if none.
        /// </summary>
        private int GetChannelForTarget(PsuCfg p)
        {
            int resultChannel = 0;
            foreach (var item in p.channel)
            {
                // We only match the channel if usage == _target AND defaultOn == true
                if (item.usage == _target && item.defaultOn)
                {
                    resultChannel = item.id;
                }
            }
            return resultChannel;
        }

        /// <summary>
        /// Power cycle a given channel if the PSU is connected. This is the console-based version,
        /// uses await Task.Delay instead of Thread.Sleep.
        /// </summary>
        public async Task PowerCycleChannel(int channel)
        {
            if (_serialManager.IsConnected())
            {
                Logger.LogAction($"START powerCycle on channel {channel}", "Debug");
                await DoPowerCycle(channel);
                Logger.LogAction($"STOP powerCycle on channel {channel}", "Debug");
            }
            else
            {
                Logger.LogAction("PSU is not connected, cannot power cycle.", "Warning");
            }
        }

        /// <summary>
        /// The actual power cycle logic: disable channel, wait, read voltage, wait, enable channel, wait, read again.
        /// All using async/await. Replaces Thread.Sleep with await Task.Delay.
        /// </summary>
        private async Task DoPowerCycle(int channelToCycle)
        {
            // Longer delays if channel == 4
            int firstOffWait = (channelToCycle == 4) ? 3000 : 1500;
            int secondOffWait = (channelToCycle == 4) ? 3000 : 1500;
            int onWait = (channelToCycle == 4) ? 2500 : 1250;

            Logger.LogAction($"Disable channel {channelToCycle}", "Info");
            RequestRightEnableVoutRndVariantCommand(channelToCycle, false);

            // 1) Wait for the channel to drop voltage
            await Task.Delay(firstOffWait);

            Logger.LogAction($"Reading Vout on channel {channelToCycle} after disable", "Debug");
            double? voutBefore = await _serialManager.ReadVoutChannel1_4Async(channelToCycle);
            Logger.LogAction($"voutBefore = {voutBefore ?? -1} V", "Debug");

            // 2) Wait some more before turning channel on
            await Task.Delay(secondOffWait);

            Logger.LogAction($"Enable channel {channelToCycle}", "Info");
            RequestRightEnableVoutRndVariantCommand(channelToCycle, true);

            // 3) Wait for channel to stabilize
            await Task.Delay(onWait);

            Logger.LogAction($"Reading Vout on channel {channelToCycle} after enable", "Debug");
            double? voutAfter = await _serialManager.ReadVoutChannel1_4Async(channelToCycle);
            Logger.LogAction($"voutAfter = {voutAfter ?? -1} V", "Debug");
        }


        /// <summary>
        /// Because some PSUs (regex=320) are single-channel, some (regex=790) are 4-channel,
        /// we pick the right method to enable/disable Vout.
        /// </summary>
        private void RequestRightEnableVoutRndVariantCommand(int channelToCycle, bool state)
        {
            switch (_currentPsuSetting.regex)
            {
                case "320":
                    _serialManager.EnableVoutOnChannel1(state);
                    break;

                case "790":
                    _serialManager.EnableVoutOnChannel1_4(channelToCycle, state);
                    break;

                default:
                    Logger.LogAction($"Unrecognized PSU type for regex '{_currentPsuSetting.regex}'", "Warning");
                    break;
            }
        }
    }
}
