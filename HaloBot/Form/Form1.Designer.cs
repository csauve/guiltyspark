namespace HaloBot
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.drawPanel = new System.Windows.Forms.Panel();
            this.clickpanel = new System.Windows.Forms.Panel();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.zoomSelector = new System.Windows.Forms.NumericUpDown();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.button4 = new System.Windows.Forms.Button();
            this.textBox6 = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.graphScaleSelector = new System.Windows.Forms.NumericUpDown();
            this.autoRotateCheckBox = new System.Windows.Forms.CheckBox();
            this.checkBox5 = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.horizontalAngleUpDown = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.verticalAngleUpDown = new System.Windows.Forms.NumericUpDown();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.lookAheadCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.nodeRadiusSelector = new System.Windows.Forms.NumericUpDown();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.button3 = new System.Windows.Forms.Button();
            this.checkBox6 = new System.Windows.Forms.CheckBox();
            this.aiTextBox = new System.Windows.Forms.TextBox();
            this.loadAIButton = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.startAIButton = new System.Windows.Forms.Button();
            this.stopAIButton = new System.Windows.Forms.Button();
            this.aiIntervalSelector = new System.Windows.Forms.NumericUpDown();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.aboutLabel = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.zoomSelector)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.graphScaleSelector)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.horizontalAngleUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.verticalAngleUpDown)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nodeRadiusSelector)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.aiIntervalSelector)).BeginInit();
            this.tabPage3.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.drawPanel);
            this.groupBox2.Controls.Add(this.clickpanel);
            this.groupBox2.Controls.Add(this.button2);
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Location = new System.Drawing.Point(274, 6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(312, 348);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Node Graph";
            // 
            // drawPanel
            // 
            this.drawPanel.Enabled = false;
            this.drawPanel.Location = new System.Drawing.Point(174, 18);
            this.drawPanel.Name = "drawPanel";
            this.drawPanel.Size = new System.Drawing.Size(107, 51);
            this.drawPanel.TabIndex = 4;
            // 
            // clickpanel
            // 
            this.clickpanel.BackColor = System.Drawing.Color.Transparent;
            this.clickpanel.CausesValidation = false;
            this.clickpanel.Location = new System.Drawing.Point(6, 45);
            this.clickpanel.Name = "clickpanel";
            this.clickpanel.Size = new System.Drawing.Size(300, 297);
            this.clickpanel.TabIndex = 3;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(87, 18);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "Save As";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(6, 18);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Open";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(247, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(34, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "Zoom";
            // 
            // zoomSelector
            // 
            this.zoomSelector.Location = new System.Drawing.Point(201, 14);
            this.zoomSelector.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.zoomSelector.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.zoomSelector.Name = "zoomSelector";
            this.zoomSelector.Size = new System.Drawing.Size(40, 20);
            this.zoomSelector.TabIndex = 4;
            this.zoomSelector.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.zoomSelector.ValueChanged += new System.EventHandler(this.zoomSelector_ValueChanged);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // textBox5
            // 
            this.textBox5.Location = new System.Drawing.Point(5, 310);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(182, 20);
            this.textBox5.TabIndex = 13;
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(193, 310);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 14;
            this.button4.Text = "Run";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // textBox6
            // 
            this.textBox6.AcceptsReturn = true;
            this.textBox6.AcceptsTab = true;
            this.textBox6.BackColor = System.Drawing.Color.White;
            this.textBox6.Location = new System.Drawing.Point(5, 6);
            this.textBox6.MaxLength = 50000;
            this.textBox6.Multiline = true;
            this.textBox6.Name = "textBox6";
            this.textBox6.ReadOnly = true;
            this.textBox6.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox6.Size = new System.Drawing.Size(262, 298);
            this.textBox6.TabIndex = 15;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.graphScaleSelector);
            this.groupBox3.Controls.Add(this.autoRotateCheckBox);
            this.groupBox3.Controls.Add(this.checkBox5);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.horizontalAngleUpDown);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.checkBox4);
            this.groupBox3.Controls.Add(this.checkBox3);
            this.groupBox3.Controls.Add(this.verticalAngleUpDown);
            this.groupBox3.Controls.Add(this.checkBox2);
            this.groupBox3.Controls.Add(this.checkBox1);
            this.groupBox3.Controls.Add(this.zoomSelector);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Location = new System.Drawing.Point(274, 360);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(312, 92);
            this.groupBox3.TabIndex = 16;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "View Settings";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(56, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 17;
            this.label3.Text = "Link Size";
            // 
            // graphScaleSelector
            // 
            this.graphScaleSelector.Location = new System.Drawing.Point(10, 66);
            this.graphScaleSelector.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.graphScaleSelector.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.graphScaleSelector.Name = "graphScaleSelector";
            this.graphScaleSelector.Size = new System.Drawing.Size(40, 20);
            this.graphScaleSelector.TabIndex = 16;
            this.graphScaleSelector.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.graphScaleSelector.ValueChanged += new System.EventHandler(this.graphScaleSelector_ValueChanged);
            // 
            // autoRotateCheckBox
            // 
            this.autoRotateCheckBox.AutoSize = true;
            this.autoRotateCheckBox.Location = new System.Drawing.Point(10, 48);
            this.autoRotateCheckBox.Name = "autoRotateCheckBox";
            this.autoRotateCheckBox.Size = new System.Drawing.Size(78, 17);
            this.autoRotateCheckBox.TabIndex = 15;
            this.autoRotateCheckBox.Text = "Auto-rotate";
            this.autoRotateCheckBox.UseVisualStyleBackColor = true;
            this.autoRotateCheckBox.CheckedChanged += new System.EventHandler(this.autoRotateCheckBox_CheckedChanged);
            // 
            // checkBox5
            // 
            this.checkBox5.AutoSize = true;
            this.checkBox5.Location = new System.Drawing.Point(10, 32);
            this.checkBox5.Name = "checkBox5";
            this.checkBox5.Size = new System.Drawing.Size(84, 17);
            this.checkBox5.TabIndex = 14;
            this.checkBox5.Text = "Use Camera";
            this.checkBox5.UseVisualStyleBackColor = true;
            this.checkBox5.CheckedChanged += new System.EventHandler(this.checkBox5_CheckedChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(247, 38);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(57, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "Hor. Angle";
            // 
            // horizontalAngleUpDown
            // 
            this.horizontalAngleUpDown.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.horizontalAngleUpDown.Location = new System.Drawing.Point(201, 36);
            this.horizontalAngleUpDown.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.horizontalAngleUpDown.Name = "horizontalAngleUpDown";
            this.horizontalAngleUpDown.Size = new System.Drawing.Size(40, 20);
            this.horizontalAngleUpDown.TabIndex = 12;
            this.horizontalAngleUpDown.ValueChanged += new System.EventHandler(this.horizontalAngleUpDown_ValueChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(247, 60);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(62, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "Vert.  Angle";
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.Checked = true;
            this.checkBox4.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox4.Location = new System.Drawing.Point(114, 16);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(51, 17);
            this.checkBox4.TabIndex = 9;
            this.checkBox4.Text = "Links";
            this.checkBox4.UseVisualStyleBackColor = true;
            this.checkBox4.CheckedChanged += new System.EventHandler(this.checkBox4_CheckedChanged);
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Checked = true;
            this.checkBox3.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox3.Location = new System.Drawing.Point(114, 48);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(68, 17);
            this.checkBox3.TabIndex = 8;
            this.checkBox3.Text = "Numbers";
            this.checkBox3.UseVisualStyleBackColor = true;
            this.checkBox3.CheckedChanged += new System.EventHandler(this.checkBox3_CheckedChanged);
            // 
            // verticalAngleUpDown
            // 
            this.verticalAngleUpDown.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.verticalAngleUpDown.Location = new System.Drawing.Point(201, 58);
            this.verticalAngleUpDown.Maximum = new decimal(new int[] {
            90,
            0,
            0,
            0});
            this.verticalAngleUpDown.Name = "verticalAngleUpDown";
            this.verticalAngleUpDown.Size = new System.Drawing.Size(40, 20);
            this.verticalAngleUpDown.TabIndex = 10;
            this.verticalAngleUpDown.Value = new decimal(new int[] {
            90,
            0,
            0,
            0});
            this.verticalAngleUpDown.ValueChanged += new System.EventHandler(this.verticalAngleUpDown_ValueChanged);
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Checked = true;
            this.checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox2.Location = new System.Drawing.Point(114, 32);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(57, 17);
            this.checkBox2.TabIndex = 7;
            this.checkBox2.Text = "Circles";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Location = new System.Drawing.Point(10, 16);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(88, 17);
            this.checkBox1.TabIndex = 6;
            this.checkBox1.Text = "Follow Player";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // lookAheadCheckBox
            // 
            this.lookAheadCheckBox.AutoSize = true;
            this.lookAheadCheckBox.Location = new System.Drawing.Point(6, 19);
            this.lookAheadCheckBox.Name = "lookAheadCheckBox";
            this.lookAheadCheckBox.Size = new System.Drawing.Size(84, 17);
            this.lookAheadCheckBox.TabIndex = 3;
            this.lookAheadCheckBox.Text = "Look Ahead";
            this.lookAheadCheckBox.UseVisualStyleBackColor = true;
            this.lookAheadCheckBox.CheckedChanged += new System.EventHandler(this.lookAheadCheckBox_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.nodeRadiusSelector);
            this.groupBox1.Controls.Add(this.lookAheadCheckBox);
            this.groupBox1.Location = new System.Drawing.Point(5, 337);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(262, 115);
            this.groupBox1.TabIndex = 17;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Path Following Settings";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(77, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(128, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Node Radius (world units)";
            // 
            // nodeRadiusSelector
            // 
            this.nodeRadiusSelector.DecimalPlaces = 1;
            this.nodeRadiusSelector.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nodeRadiusSelector.Location = new System.Drawing.Point(6, 42);
            this.nodeRadiusSelector.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.nodeRadiusSelector.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nodeRadiusSelector.Name = "nodeRadiusSelector";
            this.nodeRadiusSelector.Size = new System.Drawing.Size(65, 20);
            this.nodeRadiusSelector.TabIndex = 4;
            this.nodeRadiusSelector.Value = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.nodeRadiusSelector.ValueChanged += new System.EventHandler(this.nodeRadiusSelector_ValueChanged);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(-1, -2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(603, 484);
            this.tabControl1.TabIndex = 19;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.groupBox2);
            this.tabPage1.Controls.Add(this.textBox5);
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Controls.Add(this.button4);
            this.tabPage1.Controls.Add(this.groupBox3);
            this.tabPage1.Controls.Add(this.textBox6);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(595, 458);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Node Graph";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.button3);
            this.tabPage2.Controls.Add(this.checkBox6);
            this.tabPage2.Controls.Add(this.aiTextBox);
            this.tabPage2.Controls.Add(this.loadAIButton);
            this.tabPage2.Controls.Add(this.groupBox5);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(595, 458);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Artificial Intelligence";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(6, 429);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 22;
            this.button3.Text = "Clear";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // checkBox6
            // 
            this.checkBox6.AutoSize = true;
            this.checkBox6.Location = new System.Drawing.Point(241, 433);
            this.checkBox6.Name = "checkBox6";
            this.checkBox6.Size = new System.Drawing.Size(123, 17);
            this.checkBox6.TabIndex = 21;
            this.checkBox6.Text = "Show Debug Output";
            this.checkBox6.UseVisualStyleBackColor = true;
            this.checkBox6.CheckedChanged += new System.EventHandler(this.checkBox6_CheckedChanged);
            // 
            // aiTextBox
            // 
            this.aiTextBox.AcceptsReturn = true;
            this.aiTextBox.AcceptsTab = true;
            this.aiTextBox.BackColor = System.Drawing.Color.White;
            this.aiTextBox.Location = new System.Drawing.Point(6, 35);
            this.aiTextBox.MaxLength = 50000;
            this.aiTextBox.Multiline = true;
            this.aiTextBox.Name = "aiTextBox";
            this.aiTextBox.ReadOnly = true;
            this.aiTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.aiTextBox.Size = new System.Drawing.Size(358, 388);
            this.aiTextBox.TabIndex = 16;
            // 
            // loadAIButton
            // 
            this.loadAIButton.Location = new System.Drawing.Point(6, 6);
            this.loadAIButton.Name = "loadAIButton";
            this.loadAIButton.Size = new System.Drawing.Size(177, 23);
            this.loadAIButton.TabIndex = 0;
            this.loadAIButton.Text = "Load AI";
            this.loadAIButton.UseVisualStyleBackColor = true;
            this.loadAIButton.Click += new System.EventHandler(this.loadAIButton_Click);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.label2);
            this.groupBox5.Controls.Add(this.startAIButton);
            this.groupBox5.Controls.Add(this.stopAIButton);
            this.groupBox5.Controls.Add(this.aiIntervalSelector);
            this.groupBox5.Location = new System.Drawing.Point(370, 6);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(214, 444);
            this.groupBox5.TabIndex = 24;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Control";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(62, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(143, 13);
            this.label2.TabIndex = 20;
            this.label2.Text = "Refresh Rate (lower is faster)";
            // 
            // startAIButton
            // 
            this.startAIButton.Location = new System.Drawing.Point(6, 19);
            this.startAIButton.Name = "startAIButton";
            this.startAIButton.Size = new System.Drawing.Size(75, 23);
            this.startAIButton.TabIndex = 17;
            this.startAIButton.Text = "Start";
            this.startAIButton.UseVisualStyleBackColor = true;
            this.startAIButton.Click += new System.EventHandler(this.startAIButton_Click);
            // 
            // stopAIButton
            // 
            this.stopAIButton.Enabled = false;
            this.stopAIButton.Location = new System.Drawing.Point(87, 19);
            this.stopAIButton.Name = "stopAIButton";
            this.stopAIButton.Size = new System.Drawing.Size(75, 23);
            this.stopAIButton.TabIndex = 18;
            this.stopAIButton.Text = "Stop";
            this.stopAIButton.UseVisualStyleBackColor = true;
            this.stopAIButton.Click += new System.EventHandler(this.stopAIButton_Click);
            // 
            // aiIntervalSelector
            // 
            this.aiIntervalSelector.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.aiIntervalSelector.Location = new System.Drawing.Point(6, 48);
            this.aiIntervalSelector.Maximum = new decimal(new int[] {
            60000,
            0,
            0,
            0});
            this.aiIntervalSelector.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.aiIntervalSelector.Name = "aiIntervalSelector";
            this.aiIntervalSelector.Size = new System.Drawing.Size(50, 20);
            this.aiIntervalSelector.TabIndex = 19;
            this.aiIntervalSelector.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.aiIntervalSelector.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.aboutLabel);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(595, 458);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "About";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // aboutLabel
            // 
            this.aboutLabel.AutoSize = true;
            this.aboutLabel.Location = new System.Drawing.Point(9, 11);
            this.aboutLabel.Name = "aboutLabel";
            this.aboutLabel.Size = new System.Drawing.Size(35, 13);
            this.aboutLabel.TabIndex = 0;
            this.aboutLabel.Text = "label3";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 480);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(599, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 20;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(118, 17);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(599, 502);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Guilty Spark";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.zoomSelector)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.graphScaleSelector)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.horizontalAngleUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.verticalAngleUpDown)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nodeRadiusSelector)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.aiIntervalSelector)).EndInit();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.TextBox textBox5;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.TextBox textBox6;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.NumericUpDown zoomSelector;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.CheckBox checkBox4;
		private System.Windows.Forms.CheckBox checkBox3;
		private System.Windows.Forms.CheckBox checkBox2;
		private System.Windows.Forms.CheckBox checkBox1;
		private System.Windows.Forms.NumericUpDown verticalAngleUpDown;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.NumericUpDown horizontalAngleUpDown;
		private System.Windows.Forms.CheckBox lookAheadCheckBox;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown nodeRadiusSelector;
        private System.Windows.Forms.CheckBox checkBox5;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.TextBox aiTextBox;
        private System.Windows.Forms.Button loadAIButton;
        private System.Windows.Forms.NumericUpDown aiIntervalSelector;
        public System.Windows.Forms.Button stopAIButton;
        public System.Windows.Forms.Button startAIButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox checkBox6;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Label aboutLabel;
        private System.Windows.Forms.CheckBox autoRotateCheckBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown graphScaleSelector;
        private System.Windows.Forms.Panel clickpanel;
        private System.Windows.Forms.Panel drawPanel;
    }
}

