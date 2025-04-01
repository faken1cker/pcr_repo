using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace powercontrolRNDdesign
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The main entry point for starting the PSU connection flow.
        /// Another dev can see that we pick either "RND320" or "vcm100mid" by reading the registry.
        /// Then we create a Controller, verify connectivity, and move on to Form3.
        /// </summary>
        private async void button1_Click(object sender, EventArgs e)
        {
            // Log that the start button was clicked
            Logger.Log("Form1: Start button clicked.");

            // Show a 'busy' cursor to let the user know we're working
            this.UseWaitCursor = true;
            Cursor.Current = Cursors.WaitCursor;

            // Read the rigType from registry. (RND320 => single channel, VCM => multi-channel.)
            string rigType = GetRigTypeFromRegistry();
            Logger.Log($"Form1: Registry key 'type' read as '{rigType}'.");

            // Decide which PSU setting name to pass into Controller
            string psuSetting;
            if (rigType.Equals("RND320", StringComparison.OrdinalIgnoreCase))
            {
                // Single-channel config from Psu.json
                psuSetting = "anyRigRnd320_24V";
                Logger.Log($"Form1: Single-channel PSU chosen: {psuSetting}");
            }
            else
            {
                // Default to multi-channel config for VCM rigs
                psuSetting = "vcm100mid";
                Logger.Log($"Form1: Multi-channel PSU chosen: {psuSetting}");
            }

            // Create the Controller with that chosen PSU setting
            Controller testController = new Controller(psuSetting);

            // Verify if it's actually connected to the PSU, if not, warn and return
            if (!testController.IsConnected)
            {
                Logger.Log("Form1: No PSU detected. Aborting start");
                MessageBox.Show("No PSU detected. Please connect a PSU and restart the program.",
                                "PSU Not Found",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                // Restore normal cursor before returning
                this.UseWaitCursor = false;
                Cursor.Current = Cursors.Default;
                return;
            }

            // Step 5: PSU is connected, so open Form3 and hide this form
            Logger.Log("Form1: PSU connected. Launching Form3.");
            this.Hide();
            Form3 form3 = new Form3(testController);
            form3.ShowDialog();

            // Once Form3 is closed, restore cursor and exit
            this.UseWaitCursor = false;
            Cursor.Current = Cursors.Default;
            Logger.Log("Form1: Application closed.");
            this.Close();
        }

        /// <summary>
        /// Reads the Windows registry at HKLM\SOFTWARE\V3rigInfo, value "type",
        /// to figure out which rig we're dealing with. If missing, defaults to "VCM100".
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
                            Logger.Log($"Form1: Registry 'type' found: {rigType}");
                            return rigType;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception so we know what went wrong
                Logger.Log($"Form1: Error reading rig type from registry: {ex.Message}");
            }

            // If we didn't find anything, default to VCM100 so we won't crash
            Logger.Log("Form1: No rig type found in registry, defaulting to 'VCM100'");
            return "VCM100";
        }


        private void openInstructionsFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = System.IO.Path.Combine(baseDir, "instructions.txt");

            if (System.IO.File.Exists(filePath))
            {
                Logger.Log("Form1: Instructions file opened.");
                System.Diagnostics.Process.Start("notepad.exe", filePath);
            }
            else
            {
                Logger.Log("Form1: Instructions file not found.");
                MessageBox.Show("Instructions file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
