using System;
using System.Windows.Forms;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace powercontrolRNDdesign
{
    public partial class Form3 : Form
    {
        private Controller _controller;
        private Timer _refreshTimer;

        // Constructor that takes an existing Controller from Form1
        public Form3(Controller controller)
        {
            InitializeComponent();
            _controller = controller;

            // Set up a timer that periodically reads channel voltages/currents
            // If you already do this in Designer, you can remove these lines.
            _refreshTimer = new Timer();
            _refreshTimer.Interval = 2000; // 2 seconds
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();
        }

        /// <summary>
        /// When the form loads, we do the following:
        /// 1) Read "type" from the registry (VCM100, VCM200, RND320, etc.).
        /// 2) Label channel names based on rigType.
        /// 3) Populate the channel dropdown similarly.
        /// 4) Update a label "PSU Type: RND320" or "PSU Type: RND790"
        /// so the user sees what PSU is detected.
        /// </summary>
        private void Form3_Load(object sender, EventArgs e)
        {
            Logger.LogAction("Form3_Load: Start");

            // 1) Read rigType from the registry
            string rigType = GetRigTypeFromRegistry();
            Logger.LogAction($"Form3_Load: Found rigType = '{rigType}'.");

            // 2) Show "PSU Type" in labelPsuType
            // If it's RND320 => "RND320", else "RND790" (covers VCM100, VCM200)
            if (rigType.Equals("RND320", StringComparison.OrdinalIgnoreCase))
            {
                labelPsuType.Text = "PSU Type: RND320";
            }
            else
            {
                labelPsuType.Text = "PSU Type: RND790";
            }
            Logger.LogAction($"Form3_Load: labelPsuType => {labelPsuType.Text}");

            // 3) Update channel labels:
            // If VCM100 => CH1="VCM(CH1)", CH2="N/A(CH2)", CH3="Vocom(CH3)", CH4="Vector(CH4)"
            // If VCM200 => CH1="VCM_P(CH1)", CH2="VCM_S(CH2)", CH3="Vocom(CH3)", CH4="Vector(CH4)"
            // If RND320 => CH1="RND320 (CH1)", CH2-4="N/A"
            // Fallback for any unknown type
            if (rigType.Equals("VCM100", StringComparison.OrdinalIgnoreCase))
            {
                labelCh1Name.Text = "VCM(CH1)";
                labelCh2Name.Text = "N/A(CH2)";
                labelCh3Name.Text = "Vocom(CH3)";
                labelCh4Name.Text = "Vector(CH4)";
            }
            else if (rigType.Equals("VCM200", StringComparison.OrdinalIgnoreCase))
            {
                labelCh1Name.Text = "VCM_P(CH1)";
                labelCh2Name.Text = "VCM_S(CH2)";
                labelCh3Name.Text = "Vocom(CH3)";
                labelCh4Name.Text = "Vector(CH4)";
            }
            else if (rigType.Equals("RND320", StringComparison.OrdinalIgnoreCase))
            {
                labelCh1Name.Text = "RND320 (CH1)";
                labelCh2Name.Text = "N/A(CH2)";
                labelCh3Name.Text = "N/A(CH3)";
                labelCh4Name.Text = "N/A(CH4)";
            }
            else
            {
                labelCh1Name.Text = "Channel 1";
                labelCh2Name.Text = "Channel 2";
                labelCh3Name.Text = "Channel 3";
                labelCh4Name.Text = "Channel 4";
            }

            // 4) Populate comboBoxChannels with similar logic
            PopulateChannelDropdown(rigType);

            Logger.LogAction("Form3_Load: Done");
        }

        /// <summary>
        /// Populates comboBoxChannels (if used) with names matching 
        /// the channel labels from Form3_Load.
        /// </summary>
        private void PopulateChannelDropdown(string rigType)
        {
            comboBoxChannels.Items.Clear();

            if (rigType.Equals("VCM100", StringComparison.OrdinalIgnoreCase))
            {
                comboBoxChannels.Items.Add("VCM(CH1)");
                comboBoxChannels.Items.Add("N/A(CH2)");
                comboBoxChannels.Items.Add("Vocom(CH3)");
                comboBoxChannels.Items.Add("Vector(CH4)");
            }
            else if (rigType.Equals("VCM200", StringComparison.OrdinalIgnoreCase))
            {
                comboBoxChannels.Items.Add("VCM_P(CH1)");
                comboBoxChannels.Items.Add("VCM_S(CH2)");
                comboBoxChannels.Items.Add("Vocom(CH3)");
                comboBoxChannels.Items.Add("Vector(CH4)");
            }
            else if (rigType.Equals("RND320", StringComparison.OrdinalIgnoreCase))
            {
                comboBoxChannels.Items.Add("RND320(CH1)");
                comboBoxChannels.Items.Add("N/A(CH2)");
                comboBoxChannels.Items.Add("N/A(CH3)");
                comboBoxChannels.Items.Add("N/A(CH4)");
            }
            else
            {
                comboBoxChannels.Items.Add("Channel 1");
                comboBoxChannels.Items.Add("Channel 2");
                comboBoxChannels.Items.Add("Channel 3");
                comboBoxChannels.Items.Add("Channel 4");
            }

            if (comboBoxChannels.Items.Count > 0)
            {
                comboBoxChannels.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Reads the registry key "type" under HKLM\SOFTWARE\V3rigInfo,
        /// logs its value, and returns "VCM100" if none is found.
        /// </summary>
        private string GetRigTypeFromRegistry()
        {
            try
            {
                using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (RegistryKey key = baseKey.OpenSubKey(@"SOFTWARE\V3rigInfo", false))
                {
                    if (key != null)
                    {
                        object value = key.GetValue("type");
                        if (value != null)
                        {
                            string rigType = value.ToString().Trim();
                            Logger.LogAction("Form3: Registry 'type' => " + rigType);
                            return rigType;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogAction("Form3: Error reading 'type' from registry: " + ex.Message, "Error");
            }

            Logger.LogAction("Form3: No 'type' found in registry, defaulting to 'VCM100'.");
            return "VCM100";
        }

        /// <summary>
        /// Timer event that fires every 2 seconds (_refreshTimer).
        /// Reads measured voltage/current from each channel via the Controller,
        /// and updates labels so the user sees live values.
        /// 
        /// If it's a single-channel PSU (RND320), channels 2-4 will remain N/A
        /// or 0. 
        /// </summary>
        private async void RefreshTimer_Tick(object sender, EventArgs e)
        {
            if (_controller != null && _controller.IsConnected)
            {
                // Channel 1
                double? ch1Volt = await _controller.GetMeasuredVoltageFromChannelAsync(1);
                double? ch1Curr = await _controller.GetMeasuredCurrentFromChannelAsync(1);

                labelCh1Voltage.Text = ch1Volt.HasValue
                    ? $"Current Voltage (V): {ch1Volt.Value:F2}"
                    : "Current Voltage (V): N/A";

                labelCh1Current.Text = ch1Curr.HasValue
                    ? $"Current Amperage (A): {ch1Curr.Value:F2}"
                    : "Current Amperage (A): N/A";

                // Channel 2
                double? ch2Volt = await _controller.GetMeasuredVoltageFromChannelAsync(2);
                double? ch2Curr = await _controller.GetMeasuredCurrentFromChannelAsync(2);

                labelCh2Voltage.Text = ch2Volt.HasValue
                    ? $"Current Voltage (V): {ch2Volt.Value:F2}"
                    : "Current Voltage (V): N/A";

                labelCh2Current.Text = ch2Curr.HasValue
                    ? $"Current Amperage (A): {ch2Curr.Value:F2}"
                    : "Current Amperage (A): N/A";

                // Channel 3
                double? ch3Volt = await _controller.GetMeasuredVoltageFromChannelAsync(3);
                double? ch3Curr = await _controller.GetMeasuredCurrentFromChannelAsync(3);

                labelCh3Voltage.Text = ch3Volt.HasValue
                    ? $"Current Voltage (V): {ch3Volt.Value:F2}"
                    : "Current Voltage (V): N/A";

                labelCh3Current.Text = ch3Curr.HasValue
                    ? $"Current Amperage (A): {ch3Curr.Value:F2}"
                    : "Current Amperage (A): N/A";

                // Channel 4
                double? ch4Volt = await _controller.GetMeasuredVoltageFromChannelAsync(4);
                double? ch4Curr = await _controller.GetMeasuredCurrentFromChannelAsync(4);

                labelCh4Voltage.Text = ch4Volt.HasValue
                    ? $"Current Voltage (V): {ch4Volt.Value:F2}"
                    : "Current Voltage (V): N/A";

                labelCh4Current.Text = ch4Curr.HasValue
                    ? $"Current Amperage (A): {ch4Curr.Value:F2}"
                    : "Current Amperage (A): N/A";
            }
            else
            {
                // If not connected, show them all as N/A
                labelCh1Voltage.Text = "Current Voltage (V): N/A";
                labelCh1Current.Text = "Current Amperage (A): N/A";
                labelCh2Voltage.Text = "Current Voltage (V): N/A";
                labelCh2Current.Text = "Current Amperage (A): N/A";
                labelCh3Voltage.Text = "Current Voltage (V): N/A";
                labelCh3Current.Text = "Current Amperage (A): N/A";
                labelCh4Voltage.Text = "Current Voltage (V): N/A";
                labelCh4Current.Text = "Current Amperage (A): N/A";
            }
        }

        /// <summary>
        /// Apply button event. Sets voltage/current on the chosen channel (via comboBoxChannels).
        /// This logic is unchanged from your original code, we only add logs or minor clarifications.
        /// </summary>
        private async void applyButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBoxChannels.SelectedIndex < 0)
                {
                    MessageBox.Show("Please choose a channel before pressing apply.",
                                    "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int channel = comboBoxChannels.SelectedIndex + 1;

                if (!double.TryParse(textBoxVoltage.Text, out double voltage) || voltage < 0 || voltage > 24.0)
                {
                    MessageBox.Show("Please enter a valid voltage (0 - 24.0 V).",
                                    "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!double.TryParse(textBoxCurrent.Text, out double current) || current < 0 || current > 5.0)
                {
                    MessageBox.Show("Please enter a valid current (0 - 5.0 A).",
                                    "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _controller.SetVoltage(channel, voltage);
                _controller.SetCurrent(channel, current);

                double? confirmedVoltage = await _controller.GetSetVoltageFromChannelAsync(channel);
                MessageBox.Show($"Channel {channel} voltage set to: {confirmedVoltage} V",
                                "Settings Applied", MessageBoxButtons.OK, MessageBoxIcon.Information);

                Logger.LogAction($"Form3: Apply succeeded => {voltage}V, {current}A on channel {channel}.", "Info");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Form3: applyButton_Click error => {ex.Message}");
                MessageBox.Show($"An error occurred: {ex.Message}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Power cycle button event handlers, one per channel (1-4).
        /// These call the Controller's PowerCycleChannel, which now 
        /// knows if it's single- or multi-channel behind the scenes.
        /// </summary>
        private async void powerCycleChannel1Button_Click(object sender, EventArgs e)
        {
            await PowerCycleChannel(1);
        }
        private async void powerCycleChannel2Button_Click(object sender, EventArgs e)
        {
            await PowerCycleChannel(2);
        }
        private async void powerCycleChannel3Button_Click(object sender, EventArgs e)
        {
            await PowerCycleChannel(3);
        }
        private async void powerCycleChannel4Button_Click(object sender, EventArgs e)
        {
            await PowerCycleChannel(4);
        }

        /// <summary>
        /// A helper method that calls Controller.PowerCycleChannel 
        /// and displays a message once done. This logic remains the same 
        /// as your original code, with optional logs added.
        /// </summary>
        private async Task PowerCycleChannel(int channel)
        {
            try
            {
                Logger.LogAction($"Form3: Start power cycle on channel {channel}.");
                await _controller.PowerCycleChannel(channel);
                Logger.LogAction($"Form3: Power cycle completed on channel {channel}.");
                MessageBox.Show($"Channel {channel} power cycle completed.",
                                "Power Cycle", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Form3: Error in power cycle channel {channel}: {ex.Message}");
                MessageBox.Show($"Error during power cycle on channel {channel}: {ex.Message}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Menu item that opens instructions.txt in Notepad, 
        /// preserving your original functionality.
        /// </summary>
        private void openInstructionsFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = System.IO.Path.Combine(baseDir, "instructions.txt");

            if (System.IO.File.Exists(filePath))
            {
                Logger.Log("Form3: Instructions file opened.");
                System.Diagnostics.Process.Start("notepad.exe", filePath);
            }
            else
            {
                Logger.Log("Form3: Instructions file not found.");
                MessageBox.Show("Instructions file not found.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// If you have a deployment timer in the Designer named 'DeploymentTimer', 
        /// this event updates labelDeploymentStatus.
        /// </summary>
        private void DeploymentTimer_Tick(object sender, EventArgs e)
        {
            if (_controller != null)
            {
                bool deploymentActive = _controller.DeploymentActive;
                labelDeploymentStatus.Text = deploymentActive
                    ? "Deployment Status: Active (Read-Only)"
                    : "Deployment Status: Inactive (Read/Write)";

                labelDeploymentStatus.ForeColor = deploymentActive
                    ? System.Drawing.Color.Red
                    : System.Drawing.Color.Green;
            }
        }

        /// <summary>
        /// Disconnect from the PSU when Form3 closes, so it isn't 
        /// left locked or connected. We log the action for clarity.
        /// </summary>
        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            _controller.Disconnect();
            Logger.Log("Form3: PSU disconnected, form closing.");
        }
    }
}
