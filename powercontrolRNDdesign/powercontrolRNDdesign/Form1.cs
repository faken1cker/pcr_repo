using System;
using System.Windows.Forms;

namespace powercontrolRNDdesign
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            Logger.Log("Form1: Start button clicked.");

            // Set the wait cursor immediately so the user sees a busy cursor
            this.UseWaitCursor = true;
            Cursor.Current = Cursors.WaitCursor;

            // Create the controller instance (only one instance is created here)
            Controller testController = new Controller("vcm100mid");

            // Check if the PSU is connected; if not, show an error and abort
            if (!testController.IsConnected)
            {
                Logger.Log("Form1: No PSU detected. Aborting start");
                MessageBox.Show("No PSU detected. Please connect a PSU and restart the program.",
                                "PSU Not Found",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                // Reset the cursor before returning
                this.UseWaitCursor = false;
                Cursor.Current = Cursors.Default;
                return;
            }

            // PSU is connected so hide Form1 and open Form3 (passing the controller)
            this.Hide();
            Form3 form3 = new Form3(testController);
            form3.ShowDialog();

            // Once Form3 is closed, reset the cursor
            this.UseWaitCursor = false;
            Cursor.Current = Cursors.Default;

            Logger.Log("Form1: Application closed.");
            this.Close();
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
