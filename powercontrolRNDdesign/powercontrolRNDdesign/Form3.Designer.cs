namespace powercontrolRNDdesign
{
    partial class Form3
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form3));
            this.labelCh1Voltage = new System.Windows.Forms.Label();
            this.labelCh1Current = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.instructionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openInstructionsFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.applyButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxVoltage = new System.Windows.Forms.TextBox();
            this.textBoxCurrent = new System.Windows.Forms.TextBox();
            this.labelCh2Current = new System.Windows.Forms.Label();
            this.labelCh2Voltage = new System.Windows.Forms.Label();
            this.labelCh3Current = new System.Windows.Forms.Label();
            this.labelCh3Voltage = new System.Windows.Forms.Label();
            this.labelCh4Current = new System.Windows.Forms.Label();
            this.labelCh4Voltage = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.labelCh1Name = new System.Windows.Forms.Label();
            this.labelCh2Name = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.powerCycleChannel4Button = new System.Windows.Forms.Button();
            this.powerCycleChannel3Button = new System.Windows.Forms.Button();
            this.powerCycleChannel2Button = new System.Windows.Forms.Button();
            this.powerCycleChannel1Button = new System.Windows.Forms.Button();
            this.comboBoxChannels = new System.Windows.Forms.ComboBox();
            this.labelDeploymentStatus = new System.Windows.Forms.Label();
            this.DeploymentTimer = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelCh1Voltage
            // 
            this.labelCh1Voltage.AutoSize = true;
            this.labelCh1Voltage.Font = new System.Drawing.Font("Volvo Novum 3.1", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCh1Voltage.Location = new System.Drawing.Point(12, 53);
            this.labelCh1Voltage.Name = "labelCh1Voltage";
            this.labelCh1Voltage.Size = new System.Drawing.Size(226, 29);
            this.labelCh1Voltage.TabIndex = 9;
            this.labelCh1Voltage.Text = "Current Voltage (V):";
            // 
            // labelCh1Current
            // 
            this.labelCh1Current.AutoSize = true;
            this.labelCh1Current.Font = new System.Drawing.Font("Volvo Novum 3.1", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCh1Current.Location = new System.Drawing.Point(12, 82);
            this.labelCh1Current.Name = "labelCh1Current";
            this.labelCh1Current.Size = new System.Drawing.Size(257, 29);
            this.labelCh1Current.TabIndex = 10;
            this.labelCh1Current.Text = "Current Amperage (A):";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(819, 24);
            this.menuStrip1.TabIndex = 14;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.instructionsToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // instructionsToolStripMenuItem
            // 
            this.instructionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openInstructionsFileToolStripMenuItem});
            this.instructionsToolStripMenuItem.Name = "instructionsToolStripMenuItem";
            this.instructionsToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.instructionsToolStripMenuItem.Text = "Instructions";
            // 
            // openInstructionsFileToolStripMenuItem
            // 
            this.openInstructionsFileToolStripMenuItem.Name = "openInstructionsFileToolStripMenuItem";
            this.openInstructionsFileToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.openInstructionsFileToolStripMenuItem.Text = "Open Instructions File";
            this.openInstructionsFileToolStripMenuItem.Click += new System.EventHandler(this.openInstructionsFileToolStripMenuItem_Click);
            // 
            // applyButton
            // 
            this.applyButton.Font = new System.Drawing.Font("Volvo Broad Spread", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.applyButton.Location = new System.Drawing.Point(557, 277);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(140, 40);
            this.applyButton.TabIndex = 16;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Volvo Novum 3.1", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(552, 114);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(213, 29);
            this.label2.TabIndex = 17;
            this.label2.Text = "Select Voltage (V):";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Volvo Novum 3.1", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(552, 194);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(266, 29);
            this.label3.TabIndex = 18;
            this.label3.Text = "Select Max Current (A):";
            // 
            // textBoxVoltage
            // 
            this.textBoxVoltage.Font = new System.Drawing.Font("Volvo Novum 3.1", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxVoltage.Location = new System.Drawing.Point(557, 155);
            this.textBoxVoltage.Name = "textBoxVoltage";
            this.textBoxVoltage.Size = new System.Drawing.Size(178, 36);
            this.textBoxVoltage.TabIndex = 19;
            // 
            // textBoxCurrent
            // 
            this.textBoxCurrent.Font = new System.Drawing.Font("Volvo Novum 3.1", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxCurrent.Location = new System.Drawing.Point(557, 226);
            this.textBoxCurrent.Name = "textBoxCurrent";
            this.textBoxCurrent.Size = new System.Drawing.Size(178, 36);
            this.textBoxCurrent.TabIndex = 20;
            // 
            // labelCh2Current
            // 
            this.labelCh2Current.AutoSize = true;
            this.labelCh2Current.Font = new System.Drawing.Font("Volvo Novum 3.1", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCh2Current.Location = new System.Drawing.Point(12, 205);
            this.labelCh2Current.Name = "labelCh2Current";
            this.labelCh2Current.Size = new System.Drawing.Size(257, 29);
            this.labelCh2Current.TabIndex = 26;
            this.labelCh2Current.Text = "Current Amperage (A):";
            // 
            // labelCh2Voltage
            // 
            this.labelCh2Voltage.AutoSize = true;
            this.labelCh2Voltage.Font = new System.Drawing.Font("Volvo Novum 3.1", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCh2Voltage.Location = new System.Drawing.Point(12, 176);
            this.labelCh2Voltage.Name = "labelCh2Voltage";
            this.labelCh2Voltage.Size = new System.Drawing.Size(226, 29);
            this.labelCh2Voltage.TabIndex = 25;
            this.labelCh2Voltage.Text = "Current Voltage (V):";
            // 
            // labelCh3Current
            // 
            this.labelCh3Current.AutoSize = true;
            this.labelCh3Current.Font = new System.Drawing.Font("Volvo Novum 3.1", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCh3Current.Location = new System.Drawing.Point(12, 337);
            this.labelCh3Current.Name = "labelCh3Current";
            this.labelCh3Current.Size = new System.Drawing.Size(257, 29);
            this.labelCh3Current.TabIndex = 29;
            this.labelCh3Current.Text = "Current Amperage (A):";
            // 
            // labelCh3Voltage
            // 
            this.labelCh3Voltage.AutoSize = true;
            this.labelCh3Voltage.Font = new System.Drawing.Font("Volvo Novum 3.1", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCh3Voltage.Location = new System.Drawing.Point(12, 308);
            this.labelCh3Voltage.Name = "labelCh3Voltage";
            this.labelCh3Voltage.Size = new System.Drawing.Size(226, 29);
            this.labelCh3Voltage.TabIndex = 28;
            this.labelCh3Voltage.Text = "Current Voltage (V):";
            // 
            // labelCh4Current
            // 
            this.labelCh4Current.AutoSize = true;
            this.labelCh4Current.Font = new System.Drawing.Font("Volvo Novum 3.1", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCh4Current.Location = new System.Drawing.Point(12, 467);
            this.labelCh4Current.Name = "labelCh4Current";
            this.labelCh4Current.Size = new System.Drawing.Size(257, 29);
            this.labelCh4Current.TabIndex = 32;
            this.labelCh4Current.Text = "Current Amperage (A):";
            // 
            // labelCh4Voltage
            // 
            this.labelCh4Voltage.AutoSize = true;
            this.labelCh4Voltage.Font = new System.Drawing.Font("Volvo Novum 3.1", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCh4Voltage.Location = new System.Drawing.Point(12, 438);
            this.labelCh4Voltage.Name = "labelCh4Voltage";
            this.labelCh4Voltage.Size = new System.Drawing.Size(226, 29);
            this.labelCh4Voltage.TabIndex = 31;
            this.labelCh4Voltage.Text = "Current Voltage (V):";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Volvo Novum 3.1", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(552, 45);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(185, 29);
            this.label15.TabIndex = 34;
            this.label15.Text = "Select Channel:";
            // 
            // labelCh1Name
            // 
            this.labelCh1Name.AutoSize = true;
            this.labelCh1Name.Font = new System.Drawing.Font("Volvo Novum 3.1", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCh1Name.Location = new System.Drawing.Point(12, 24);
            this.labelCh1Name.Name = "labelCh1Name";
            this.labelCh1Name.Size = new System.Drawing.Size(201, 29);
            this.labelCh1Name.TabIndex = 35;
            this.labelCh1Name.Text = "VCM/VCM_P(1)";
            // 
            // labelCh2Name
            // 
            this.labelCh2Name.AutoSize = true;
            this.labelCh2Name.Font = new System.Drawing.Font("Volvo Novum 3.1", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCh2Name.Location = new System.Drawing.Point(12, 147);
            this.labelCh2Name.Name = "labelCh2Name";
            this.labelCh2Name.Size = new System.Drawing.Size(133, 29);
            this.labelCh2Name.TabIndex = 36;
            this.labelCh2Name.Text = "VCM_S(2)";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Volvo Novum 3.1", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label18.Location = new System.Drawing.Point(12, 280);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(123, 29);
            this.label18.TabIndex = 37;
            this.label18.Text = "Vocom(3)";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("Volvo Novum 3.1", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label19.Location = new System.Drawing.Point(12, 409);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(119, 29);
            this.label19.TabIndex = 38;
            this.label19.Text = "Vector(4)";
            // 
            // powerCycleChannel4Button
            // 
            this.powerCycleChannel4Button.Font = new System.Drawing.Font("Volvo Broad Spread", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.powerCycleChannel4Button.Location = new System.Drawing.Point(17, 499);
            this.powerCycleChannel4Button.Name = "powerCycleChannel4Button";
            this.powerCycleChannel4Button.Size = new System.Drawing.Size(140, 40);
            this.powerCycleChannel4Button.TabIndex = 39;
            this.powerCycleChannel4Button.Text = "Power Cycle";
            this.powerCycleChannel4Button.UseVisualStyleBackColor = true;
            this.powerCycleChannel4Button.Click += new System.EventHandler(this.powerCycleChannel4Button_Click);
            // 
            // powerCycleChannel3Button
            // 
            this.powerCycleChannel3Button.Font = new System.Drawing.Font("Volvo Broad Spread", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.powerCycleChannel3Button.Location = new System.Drawing.Point(17, 369);
            this.powerCycleChannel3Button.Name = "powerCycleChannel3Button";
            this.powerCycleChannel3Button.Size = new System.Drawing.Size(140, 40);
            this.powerCycleChannel3Button.TabIndex = 40;
            this.powerCycleChannel3Button.Text = "Power Cycle";
            this.powerCycleChannel3Button.UseVisualStyleBackColor = true;
            this.powerCycleChannel3Button.Click += new System.EventHandler(this.powerCycleChannel3Button_Click);
            // 
            // powerCycleChannel2Button
            // 
            this.powerCycleChannel2Button.Font = new System.Drawing.Font("Volvo Broad Spread", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.powerCycleChannel2Button.Location = new System.Drawing.Point(17, 237);
            this.powerCycleChannel2Button.Name = "powerCycleChannel2Button";
            this.powerCycleChannel2Button.Size = new System.Drawing.Size(140, 40);
            this.powerCycleChannel2Button.TabIndex = 41;
            this.powerCycleChannel2Button.Text = "Power Cycle";
            this.powerCycleChannel2Button.UseVisualStyleBackColor = true;
            this.powerCycleChannel2Button.Click += new System.EventHandler(this.powerCycleChannel2Button_Click);
            // 
            // powerCycleChannel1Button
            // 
            this.powerCycleChannel1Button.Font = new System.Drawing.Font("Volvo Broad Spread", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.powerCycleChannel1Button.Location = new System.Drawing.Point(17, 109);
            this.powerCycleChannel1Button.Name = "powerCycleChannel1Button";
            this.powerCycleChannel1Button.Size = new System.Drawing.Size(140, 40);
            this.powerCycleChannel1Button.TabIndex = 42;
            this.powerCycleChannel1Button.Text = "Power Cycle";
            this.powerCycleChannel1Button.UseVisualStyleBackColor = true;
            this.powerCycleChannel1Button.Click += new System.EventHandler(this.powerCycleChannel1Button_Click);
            // 
            // comboBoxChannels
            // 
            this.comboBoxChannels.Font = new System.Drawing.Font("Volvo Novum 3.1", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxChannels.FormattingEnabled = true;
            this.comboBoxChannels.Items.AddRange(new object[] {
            "VCM_P(1)",
            "VCM_S(2)",
            "Vocom(3)",
            "Vector(4)"});
            this.comboBoxChannels.Location = new System.Drawing.Point(557, 77);
            this.comboBoxChannels.Name = "comboBoxChannels";
            this.comboBoxChannels.Size = new System.Drawing.Size(178, 37);
            this.comboBoxChannels.TabIndex = 43;
            // 
            // labelDeploymentStatus
            // 
            this.labelDeploymentStatus.AutoSize = true;
            this.labelDeploymentStatus.Font = new System.Drawing.Font("Volvo Novum 3.1", 18F);
            this.labelDeploymentStatus.Location = new System.Drawing.Point(347, 467);
            this.labelDeploymentStatus.Name = "labelDeploymentStatus";
            this.labelDeploymentStatus.Size = new System.Drawing.Size(81, 29);
            this.labelDeploymentStatus.TabIndex = 44;
            this.labelDeploymentStatus.Text = "label1";
            // 
            // DeploymentTimer
            // 
            this.DeploymentTimer.Enabled = true;
            this.DeploymentTimer.Tick += new System.EventHandler(this.DeploymentTimer_Tick);
            // 
            // Form3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.SlateGray;
            this.ClientSize = new System.Drawing.Size(819, 553);
            this.Controls.Add(this.labelDeploymentStatus);
            this.Controls.Add(this.comboBoxChannels);
            this.Controls.Add(this.powerCycleChannel1Button);
            this.Controls.Add(this.powerCycleChannel2Button);
            this.Controls.Add(this.powerCycleChannel3Button);
            this.Controls.Add(this.powerCycleChannel4Button);
            this.Controls.Add(this.label19);
            this.Controls.Add(this.label18);
            this.Controls.Add(this.labelCh2Name);
            this.Controls.Add(this.labelCh1Name);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.labelCh4Current);
            this.Controls.Add(this.labelCh4Voltage);
            this.Controls.Add(this.labelCh3Current);
            this.Controls.Add(this.labelCh3Voltage);
            this.Controls.Add(this.labelCh2Current);
            this.Controls.Add(this.labelCh2Voltage);
            this.Controls.Add(this.textBoxCurrent);
            this.Controls.Add(this.textBoxVoltage);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.applyButton);
            this.Controls.Add(this.labelCh1Current);
            this.Controls.Add(this.labelCh1Voltage);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form3";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Power Control RND";
            this.Load += new System.EventHandler(this.Form3_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelCh1Voltage;
        private System.Windows.Forms.Label labelCh1Current;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem instructionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openInstructionsFileToolStripMenuItem;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxVoltage;
        private System.Windows.Forms.TextBox textBoxCurrent;
        private System.Windows.Forms.Label labelCh2Current;
        private System.Windows.Forms.Label labelCh2Voltage;
        private System.Windows.Forms.Label labelCh3Current;
        private System.Windows.Forms.Label labelCh3Voltage;
        private System.Windows.Forms.Label labelCh4Current;
        private System.Windows.Forms.Label labelCh4Voltage;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label labelCh1Name;
        private System.Windows.Forms.Label labelCh2Name;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Button powerCycleChannel4Button;
        private System.Windows.Forms.Button powerCycleChannel3Button;
        private System.Windows.Forms.Button powerCycleChannel2Button;
        private System.Windows.Forms.Button powerCycleChannel1Button;
        private System.Windows.Forms.ComboBox comboBoxChannels;
        private System.Windows.Forms.Label labelDeploymentStatus;
        private System.Windows.Forms.Timer DeploymentTimer;
    }
}