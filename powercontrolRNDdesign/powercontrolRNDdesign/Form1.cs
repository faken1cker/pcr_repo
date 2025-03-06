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

        private void button1_Click(object sender, EventArgs e)
        {
            Logger.Log("Form1: Start button clicked.");

            // Hide Form1 instead of closing it
            this.Hide();

            // Directly open Form3
            Form3 form3 = new Form3();
            form3.ShowDialog(); // Wait for Form3 to close

            // After Form3 is closed, close Form1
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
