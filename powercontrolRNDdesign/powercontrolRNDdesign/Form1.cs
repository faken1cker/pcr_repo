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
        /// Button event that starts the main program flow.
        /// This code chooses the correct PSU setting ("anyRigRnd320_24V" or "vcm100mid")
        /// based on what the registry says for "type". 
        /// We also log each step with Logger.Log, so we can see what's happening.
        /// </summary>
        private async void button1_Click(object sender, EventArgs e)
        {
            // Log that the start button was clicked
            Logger.Log("Form1: Start button clicked.");

            // Show a 'busy' cursor to let the user know we're working
            this.UseWaitCursor = true;
            Cursor.Current = Cursors.WaitCursor;

            // Step 1: Read the rig type from registry (VCM100, VCM200, RND320, etc.)
            string rigType = GetRigTypeFromRegistry();
            Logger.Log($"Form1: Registry key 'type' read as '{rigType}'.");

            // Step 2: Decide which PSU setting from Psu.json we should use
            string psuSetting;
            if (rigType.Equals("RND320", StringComparison.OrdinalIgnoreCase))
            {
                // For single-channel RND320 rigs, pick the old code's "anyRigRnd320_24V"
                psuSetting = "anyRigRnd320_24V";
                Logger.Log($"Form1: Single-channel PSU chosen: {psuSetting}");
            }
            else
            {
                // For both "VCM100" and "VCM200" or any other text, pick "vcm100mid"
                // Make sure it matches exactly the "setting" string in Psu.json
                psuSetting = "vcm100mid";
                Logger.Log($"Form1: Multi-channel PSU chosen: {psuSetting}");
            }

            // Step 3: Create the Controller with that chosen PSU setting
            Controller testController = new Controller(psuSetting);

            // Step 4: Verify if it's actually connected to the PSU
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
