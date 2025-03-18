using System;
using System.Windows.Forms;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace powercontrolRNDdesign
{
    public partial class Form3 : Form
    {
        private Controller _controller;

        // Timer for periodically reading measured voltage/current
        private System.Windows.Forms.Timer _refreshTimer;

        public Form3()
        {
            InitializeComponent();
            // Initialize the Controller with the desired PSU setting from your JSON config
            _controller = new Controller("vcm100mid"); // capital M to match the JSON

            // Set up the Timer in the constructor
            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Interval = 2000; // 2 seconds between updates
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            // Retrieve the rig type from the registry.
            string rigType = GetRigTypeFromRegistry();

            // Populate combobox dynamically.
            PopulateChannelDropdown();

            // Update channel labels based on the rig type.
            if (rigType.Equals("VCM100", StringComparison.OrdinalIgnoreCase))
            {
                // For VCM100, only channel 1 is connected.
                labelCh1Name.Text = "VCM(CH1)";
                labelCh2Name.Text = "Nothing connected(CH2)";
            }
            else if (rigType.Equals("VCM200", StringComparison.OrdinalIgnoreCase))
            {
                // For VCM200, channels 1 and 2 are used.
                labelCh1Name.Text = "VCM_P(CH1)";  // Primary
                labelCh2Name.Text = "VCM_S(CH2)";  // Secondary
            }
            else
            {
                // Default labels if the rig type is unknown.
                labelCh1Name.Text = "Channel 1";
                labelCh2Name.Text = "Channel 2";
            }

            // (Optional) Update other channel labels if needed.
        }


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
        /// Timer tick event: reads the measured voltage/current for each channel
        /// and updates the labels. This runs every 2 seconds (as per _refreshTimer.Interval).
        /// </summary>
        private async void RefreshTimer_Tick(object sender, EventArgs e)
        {
            // Only proceed if the PSU is connected
            if (_controller != null && _controller.IsConnected)
            {
                // ----- CHANNEL 1 -----
                double? ch1Volt = await _controller.GetMeasuredVoltageFromChannelAsync(1);
                double? ch1Curr = await _controller.GetMeasuredCurrentFromChannelAsync(1);

                labelCh1Voltage.Text = ch1Volt.HasValue
                    ? $"Current Voltage (V): {ch1Volt.Value:F2}"
                    : "Current Voltage (V): N/A";

                labelCh1Current.Text = ch1Curr.HasValue
                    ? $"Current Amperage (A): {ch1Curr.Value:F2}"
                    : "Current Amperage (A): N/A";

                // ----- CHANNEL 2 -----
                double? ch2Volt = await _controller.GetMeasuredVoltageFromChannelAsync(2);
                double? ch2Curr = await _controller.GetMeasuredCurrentFromChannelAsync(2);

                labelCh2Voltage.Text = ch2Volt.HasValue
                    ? $"Current Voltage (V): {ch2Volt.Value:F2}"
                    : "Current Voltage (V): N/A";

                labelCh2Current.Text = ch2Curr.HasValue
                    ? $"Current Amperage (A): {ch2Curr.Value:F2}"
                    : "Current Amperage (A): N/A";

                // ----- CHANNEL 3 -----
                double? ch3Volt = await _controller.GetMeasuredVoltageFromChannelAsync(3);
                double? ch3Curr = await _controller.GetMeasuredCurrentFromChannelAsync(3);

                labelCh3Voltage.Text = ch3Volt.HasValue
                    ? $"Current Voltage (V): {ch3Volt.Value:F2}"
                    : "Current Voltage (V): N/A";

                labelCh3Current.Text = ch3Curr.HasValue
                    ? $"Current Amperage (A): {ch3Curr.Value:F2}"
                    : "Current Amperage (A): N/A";

                // ----- CHANNEL 4 -----
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
                // If not connected, maybe set them all to N/A
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
        /// Apply button event handler: sets the user-specified voltage/current on the chosen channel
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

                Logger.LogAction($"Apply succeeded: {voltage} V, {current} A on channel {channel}.", "Info");
            }
            catch (Exception ex)
            {
                Logger.LogError($"An error occurred in applyButton_Click: {ex.Message}");
                MessageBox.Show($"An error occurred: {ex.Message}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Power cycle button event handlers (one per channel):
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
        /// Helper method that initiates a power cycle on the specified channel using the Controller.
        /// Logs the start and completion, then displays a confirmation message.
        /// </summary>
        private async Task PowerCycleChannel(int channel)
        {
            try
            {
                Logger.LogAction($"Power cycle started on channel {channel}.", "Info");
                await _controller.PowerCycleChannel(channel);
                Logger.LogAction($"Power cycle completed on channel {channel}.", "Info");
                MessageBox.Show($"Channel {channel} power cycle completed.",
                                "Power Cycle", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error during power cycle on channel {channel}: {ex.Message}");
                MessageBox.Show($"Error during power cycle on channel {channel}: {ex.Message}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PopulateChannelDropdown()
        {
            // Get rigtype from registry.
            string rigType = GetRigTypeFromRegistry();

            // Clear previous values in combobox.
            comboBoxChannels.Items.Clear();

            // Kanal 1: Dynamic based on rigtype.
            if (rigType.Equals("VCM100", StringComparison.OrdinalIgnoreCase))
            {
                comboBoxChannels.Items.Add("VCM(CH1)");
            }
            else if (rigType.Equals("VCM200", StringComparison.OrdinalIgnoreCase))
            {
                comboBoxChannels.Items.Add("VCM_P(CH1)");
            }
            else
            {
                // If rigtype is unknown.
                comboBoxChannels.Items.Add("Channel 1");
            }

            // Channel 2: Dynamic based on rigtype.
            if (rigType.Equals("VCM100", StringComparison.OrdinalIgnoreCase))
            {
                comboBoxChannels.Items.Add("N/A(CH2)");
            }
            else if (rigType.Equals("VCM200", StringComparison.OrdinalIgnoreCase))
            {
                comboBoxChannels.Items.Add("VCM_S(CH2)");
            }
            else
            {
                comboBoxChannels.Items.Add("Channel 2");
            }

            // Channel 3 : Always Vocom(3).
            comboBoxChannels.Items.Add("Vocom(CH3)");

            // Channel 4 : Always Vector(4)
            comboBoxChannels.Items.Add("Vector(CH4)");
        }

        private string GetRigTypeFromRegistry()
        {
            try
            {
                // Open base key with 64-bit view explicitly.
                using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (RegistryKey key = baseKey.OpenSubKey(@"SOFTWARE\V3rigInfo", false))
                {
                    if (key != null)
                    {
                        object value = key.GetValue("type");
                        if (value != null)
                        {
                            string rigType = value.ToString().Trim();
                            Logger.LogAction("Rig type read from registry: " + rigType, "Info");
                            return rigType;
                        }
                        else
                        {
                            Logger.LogAction("Registry value 'type' not found.", "Warning");
                        }
                    }
                    else
                    {
                        Logger.LogAction("Registry key 'SOFTWARE\\V3rigInfo' not found.", "Warning");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogAction("Error reading rig type from registry: " + ex.Message, "Error");
            }
            return "VCM100";
        }


        /// <summary>
        /// Ensures the PSU connection is closed when the form is closing.
        /// </summary>
        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            _controller.Disconnect();
        }

        private void DeploymentTimer_Tick(object sender, EventArgs e)
        {
            if (_controller != null)
            {
                // Get the deployment status from Controller (which gets it from SerialManager)
                bool deploymentActive = _controller.DeploymentActive;

                // Update the label text based on the status
                labelDeploymentStatus.Text = deploymentActive
                    ? "Deployment Status: Active (Read-Only)"
                    : "Deployment Status: Inactive (Read/Write)";

                // Change the label color for better visibility
                labelDeploymentStatus.ForeColor = deploymentActive ? System.Drawing.Color.Red : System.Drawing.Color.Green;
            }
        }

    }
}
