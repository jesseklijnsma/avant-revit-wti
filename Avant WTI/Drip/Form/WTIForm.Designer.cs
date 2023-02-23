using System;
using System.Windows.Forms;

namespace Avant.WTI.Drip.Form
{
    partial class WTIForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WTIForm));
            this.titleLabel = new System.Windows.Forms.Label();
            this.canvas = new System.Windows.Forms.Panel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.selectLabel = new System.Windows.Forms.Label();
            this.selectButton = new System.Windows.Forms.Button();
            this.pipetypelabel = new System.Windows.Forms.Label();
            this.combo_pipetype = new System.Windows.Forms.ComboBox();
            this.transportsystemtypelabel = new System.Windows.Forms.Label();
            this.combo_transportsystem = new System.Windows.Forms.ComboBox();
            this.distributionsystemtypelabel = new System.Windows.Forms.Label();
            this.combo_distributionsystem = new System.Windows.Forms.ComboBox();
            this.valvelabel = new System.Windows.Forms.Label();
            this.combo_valvefamily = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.num_valveheight = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.combo_distributiondiameter = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.num_pipecolumndistance = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.num_valvecolumndistance = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.num_backwalldistance = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.num_interdistance = new System.Windows.Forms.NumericUpDown();
            this.transporteheightlabel = new System.Windows.Forms.Label();
            this.num_distributionheight = new System.Windows.Forms.NumericUpDown();
            this.distributionheightlabel = new System.Windows.Forms.Label();
            this.num_transportheight = new System.Windows.Forms.NumericUpDown();
            this.combo_transportdiameter = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.generateButton = new System.Windows.Forms.Button();
            this.button_convertplaceholders = new System.Windows.Forms.CheckBox();
            this.flowLayoutPanel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.num_valveheight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_pipecolumndistance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_valvecolumndistance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_backwalldistance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_interdistance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_distributionheight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_transportheight)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 14.25F);
            this.titleLabel.Location = new System.Drawing.Point(7, 10);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(144, 32);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Text = "WTI Preview";
            // 
            // canvas
            // 
            this.canvas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.canvas.Location = new System.Drawing.Point(13, 53);
            this.canvas.Name = "canvas";
            this.canvas.Size = new System.Drawing.Size(918, 734);
            this.canvas.TabIndex = 1;
            this.canvas.Paint += new System.Windows.Forms.PaintEventHandler(this.Canvas_paint);
            this.canvas.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Canvas_mousedown);
            this.canvas.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Canvas_mousemove);
            this.canvas.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Canvas_mouseup);
            this.canvas.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.Canvas_mousewheel);
            this.canvas.Resize += new System.EventHandler(this.Canvas_resize);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.selectLabel);
            this.flowLayoutPanel1.Controls.Add(this.selectButton);
            this.flowLayoutPanel1.Controls.Add(this.pipetypelabel);
            this.flowLayoutPanel1.Controls.Add(this.combo_pipetype);
            this.flowLayoutPanel1.Controls.Add(this.transportsystemtypelabel);
            this.flowLayoutPanel1.Controls.Add(this.combo_transportsystem);
            this.flowLayoutPanel1.Controls.Add(this.distributionsystemtypelabel);
            this.flowLayoutPanel1.Controls.Add(this.combo_distributionsystem);
            this.flowLayoutPanel1.Controls.Add(this.valvelabel);
            this.flowLayoutPanel1.Controls.Add(this.combo_valvefamily);
            this.flowLayoutPanel1.Controls.Add(this.groupBox2);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 21);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.flowLayoutPanel1.Size = new System.Drawing.Size(350, 634);
            this.flowLayoutPanel1.TabIndex = 2;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // selectLabel
            // 
            this.selectLabel.AutoSize = true;
            this.selectLabel.Location = new System.Drawing.Point(7, 5);
            this.selectLabel.Name = "selectLabel";
            this.selectLabel.Size = new System.Drawing.Size(217, 17);
            this.selectLabel.TabIndex = 0;
            this.selectLabel.Text = "Select main pipelines (placeholders)";
            // 
            // selectButton
            // 
            this.selectButton.Location = new System.Drawing.Point(7, 25);
            this.selectButton.MinimumSize = new System.Drawing.Size(100, 30);
            this.selectButton.Name = "selectButton";
            this.selectButton.Size = new System.Drawing.Size(100, 30);
            this.selectButton.TabIndex = 1;
            this.selectButton.Text = "Select";
            this.selectButton.UseVisualStyleBackColor = true;
            this.selectButton.Click += new System.EventHandler(this.SelectButton_Click);
            // 
            // pipetypelabel
            // 
            this.pipetypelabel.AutoSize = true;
            this.pipetypelabel.Location = new System.Drawing.Point(7, 58);
            this.pipetypelabel.Name = "pipetypelabel";
            this.pipetypelabel.Size = new System.Drawing.Size(71, 17);
            this.pipetypelabel.TabIndex = 2;
            this.pipetypelabel.Text = "Pipe Type: ";
            // 
            // combo_pipetype
            // 
            this.combo_pipetype.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.combo_pipetype.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.combo_pipetype.FormattingEnabled = true;
            this.combo_pipetype.Location = new System.Drawing.Point(7, 78);
            this.combo_pipetype.Name = "combo_pipetype";
            this.combo_pipetype.Size = new System.Drawing.Size(353, 25);
            this.combo_pipetype.TabIndex = 3;
            this.combo_pipetype.SelectedValueChanged += new System.EventHandler(this.Combo_pipetype_SelectedValueChanged);
            // 
            // transportsystemtypelabel
            // 
            this.transportsystemtypelabel.AutoSize = true;
            this.transportsystemtypelabel.Location = new System.Drawing.Point(7, 106);
            this.transportsystemtypelabel.Name = "transportsystemtypelabel";
            this.transportsystemtypelabel.Size = new System.Drawing.Size(144, 17);
            this.transportsystemtypelabel.TabIndex = 4;
            this.transportsystemtypelabel.Text = "Transport system type: ";
            // 
            // combo_transportsystem
            // 
            this.combo_transportsystem.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.combo_transportsystem.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.combo_transportsystem.FormattingEnabled = true;
            this.combo_transportsystem.Location = new System.Drawing.Point(7, 126);
            this.combo_transportsystem.Name = "combo_transportsystem";
            this.combo_transportsystem.Size = new System.Drawing.Size(353, 25);
            this.combo_transportsystem.TabIndex = 5;
            this.combo_transportsystem.SelectedValueChanged += new System.EventHandler(this.Combo_transportsystem_SelectedValueChanged);
            // 
            // distributionsystemtypelabel
            // 
            this.distributionsystemtypelabel.AutoSize = true;
            this.distributionsystemtypelabel.Location = new System.Drawing.Point(7, 154);
            this.distributionsystemtypelabel.Name = "distributionsystemtypelabel";
            this.distributionsystemtypelabel.Size = new System.Drawing.Size(155, 17);
            this.distributionsystemtypelabel.TabIndex = 6;
            this.distributionsystemtypelabel.Text = "Distribution system type: ";
            // 
            // combo_distributionsystem
            // 
            this.combo_distributionsystem.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.combo_distributionsystem.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.combo_distributionsystem.FormattingEnabled = true;
            this.combo_distributionsystem.Location = new System.Drawing.Point(7, 174);
            this.combo_distributionsystem.Name = "combo_distributionsystem";
            this.combo_distributionsystem.Size = new System.Drawing.Size(353, 25);
            this.combo_distributionsystem.TabIndex = 7;
            this.combo_distributionsystem.SelectedValueChanged += new System.EventHandler(this.Combo_distributionsystem_SelectedValueChanged);
            // 
            // valvelabel
            // 
            this.valvelabel.Location = new System.Drawing.Point(7, 202);
            this.valvelabel.Name = "valvelabel";
            this.valvelabel.Size = new System.Drawing.Size(81, 17);
            this.valvelabel.TabIndex = 8;
            this.valvelabel.Text = "Valve Family:";
            // 
            // combo_valvefamily
            // 
            this.combo_valvefamily.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.combo_valvefamily.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.combo_valvefamily.FormattingEnabled = true;
            this.combo_valvefamily.Location = new System.Drawing.Point(7, 222);
            this.combo_valvefamily.Name = "combo_valvefamily";
            this.combo_valvefamily.Size = new System.Drawing.Size(353, 25);
            this.combo_valvefamily.TabIndex = 9;
            this.combo_valvefamily.SelectedValueChanged += new System.EventHandler(this.Combo_valvefamily_SelectedValueChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.AutoSize = true;
            this.groupBox2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox2.Controls.Add(this.tableLayoutPanel1);
            this.groupBox2.Location = new System.Drawing.Point(7, 253);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(3, 3, 5, 3);
            this.groupBox2.Size = new System.Drawing.Size(353, 327);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Dimensions";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.num_valveheight, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.label7, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.combo_distributiondiameter, 1, 8);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 8);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.num_pipecolumndistance, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.num_valvecolumndistance, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.num_backwalldistance, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.num_interdistance, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.transporteheightlabel, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.num_distributionheight, 1, 7);
            this.tableLayoutPanel1.Controls.Add(this.distributionheightlabel, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.num_transportheight, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.combo_transportdiameter, 1, 6);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 24);
            this.tableLayoutPanel1.MinimumSize = new System.Drawing.Size(322, 124);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 9;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(339, 279);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // num_valveheight
            // 
            this.num_valveheight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.num_valveheight.Location = new System.Drawing.Point(168, 127);
            this.num_valveheight.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.num_valveheight.Minimum = new decimal(new int[] {
            10000000,
            0,
            0,
            -2147483648});
            this.num_valveheight.Name = "num_valveheight";
            this.num_valveheight.Size = new System.Drawing.Size(168, 25);
            this.num_valveheight.TabIndex = 18;
            this.num_valveheight.ValueChanged += new System.EventHandler(this.num_valveheight_ValueChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label7.Location = new System.Drawing.Point(3, 124);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(159, 31);
            this.label7.TabIndex = 17;
            this.label7.Text = "Valve height";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // combo_distributiondiameter
            // 
            this.combo_distributiondiameter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.combo_distributiondiameter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.combo_distributiondiameter.FormattingEnabled = true;
            this.combo_distributiondiameter.Location = new System.Drawing.Point(168, 251);
            this.combo_distributiondiameter.Name = "combo_distributiondiameter";
            this.combo_distributiondiameter.Size = new System.Drawing.Size(168, 25);
            this.combo_distributiondiameter.TabIndex = 15;
            this.combo_distributiondiameter.SelectedValueChanged += new System.EventHandler(this.Combo_distributiondiameter_SelectedValueChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label6.Location = new System.Drawing.Point(3, 248);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(159, 31);
            this.label6.TabIndex = 16;
            this.label6.Text = "Distribution Line Diameter";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Location = new System.Drawing.Point(3, 186);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(159, 31);
            this.label5.TabIndex = 12;
            this.label5.Text = "Transport Line Diameter";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(3, 93);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(159, 31);
            this.label4.TabIndex = 6;
            this.label4.Text = "Pipe to column distance";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // num_pipecolumndistance
            // 
            this.num_pipecolumndistance.Dock = System.Windows.Forms.DockStyle.Fill;
            this.num_pipecolumndistance.Location = new System.Drawing.Point(168, 96);
            this.num_pipecolumndistance.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.num_pipecolumndistance.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.num_pipecolumndistance.Name = "num_pipecolumndistance";
            this.num_pipecolumndistance.Size = new System.Drawing.Size(168, 25);
            this.num_pipecolumndistance.TabIndex = 7;
            this.num_pipecolumndistance.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.num_pipecolumndistance.ValueChanged += new System.EventHandler(this.Num_pipecolumndistance_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(3, 62);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(159, 31);
            this.label3.TabIndex = 4;
            this.label3.Text = "Valve to column distance";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // num_valvecolumndistance
            // 
            this.num_valvecolumndistance.Dock = System.Windows.Forms.DockStyle.Fill;
            this.num_valvecolumndistance.Location = new System.Drawing.Point(168, 65);
            this.num_valvecolumndistance.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.num_valvecolumndistance.Name = "num_valvecolumndistance";
            this.num_valvecolumndistance.Size = new System.Drawing.Size(168, 25);
            this.num_valvecolumndistance.TabIndex = 5;
            this.num_valvecolumndistance.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.num_valvecolumndistance.ValueChanged += new System.EventHandler(this.Num_valvecolumndistance_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(3, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(159, 31);
            this.label2.TabIndex = 2;
            this.label2.Text = "Distance from back wall";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // num_backwalldistance
            // 
            this.num_backwalldistance.Dock = System.Windows.Forms.DockStyle.Fill;
            this.num_backwalldistance.Location = new System.Drawing.Point(168, 34);
            this.num_backwalldistance.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.num_backwalldistance.Name = "num_backwalldistance";
            this.num_backwalldistance.Size = new System.Drawing.Size(168, 25);
            this.num_backwalldistance.TabIndex = 3;
            this.num_backwalldistance.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.num_backwalldistance.ValueChanged += new System.EventHandler(this.Num_backwalldistance_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(159, 31);
            this.label1.TabIndex = 0;
            this.label1.Text = "Intermediate Distance";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // num_interdistance
            // 
            this.num_interdistance.Dock = System.Windows.Forms.DockStyle.Fill;
            this.num_interdistance.Location = new System.Drawing.Point(168, 3);
            this.num_interdistance.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.num_interdistance.Name = "num_interdistance";
            this.num_interdistance.Size = new System.Drawing.Size(168, 25);
            this.num_interdistance.TabIndex = 1;
            this.num_interdistance.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.num_interdistance.ValueChanged += new System.EventHandler(this.Num_interdistance_ValueChanged);
            // 
            // transporteheightlabel
            // 
            this.transporteheightlabel.AutoSize = true;
            this.transporteheightlabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.transporteheightlabel.Location = new System.Drawing.Point(3, 155);
            this.transporteheightlabel.Name = "transporteheightlabel";
            this.transporteheightlabel.Size = new System.Drawing.Size(159, 31);
            this.transporteheightlabel.TabIndex = 10;
            this.transporteheightlabel.Text = "Transport Line Height";
            this.transporteheightlabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // num_distributionheight
            // 
            this.num_distributionheight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.num_distributionheight.Location = new System.Drawing.Point(168, 220);
            this.num_distributionheight.Maximum = new decimal(new int[] {
            -1,
            -1,
            -1,
            0});
            this.num_distributionheight.Minimum = new decimal(new int[] {
            -1,
            -1,
            -1,
            -2147483648});
            this.num_distributionheight.Name = "num_distributionheight";
            this.num_distributionheight.Size = new System.Drawing.Size(168, 25);
            this.num_distributionheight.TabIndex = 13;
            this.num_distributionheight.ValueChanged += new System.EventHandler(this.Num_distributionheight_ValueChanged);
            // 
            // distributionheightlabel
            // 
            this.distributionheightlabel.AutoSize = true;
            this.distributionheightlabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.distributionheightlabel.Location = new System.Drawing.Point(3, 217);
            this.distributionheightlabel.Name = "distributionheightlabel";
            this.distributionheightlabel.Size = new System.Drawing.Size(159, 31);
            this.distributionheightlabel.TabIndex = 12;
            this.distributionheightlabel.Text = "Distribution Line Height";
            this.distributionheightlabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // num_transportheight
            // 
            this.num_transportheight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.num_transportheight.Location = new System.Drawing.Point(168, 158);
            this.num_transportheight.Maximum = new decimal(new int[] {
            -1,
            -1,
            -1,
            0});
            this.num_transportheight.Minimum = new decimal(new int[] {
            -1,
            -1,
            -1,
            -2147483648});
            this.num_transportheight.Name = "num_transportheight";
            this.num_transportheight.Size = new System.Drawing.Size(168, 25);
            this.num_transportheight.TabIndex = 9;
            this.num_transportheight.Value = new decimal(new int[] {
            400,
            0,
            0,
            -2147483648});
            this.num_transportheight.ValueChanged += new System.EventHandler(this.Num_transportheight_ValueChanged);
            // 
            // combo_transportdiameter
            // 
            this.combo_transportdiameter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.combo_transportdiameter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.combo_transportdiameter.FormattingEnabled = true;
            this.combo_transportdiameter.Location = new System.Drawing.Point(168, 189);
            this.combo_transportdiameter.Name = "combo_transportdiameter";
            this.combo_transportdiameter.Size = new System.Drawing.Size(168, 25);
            this.combo_transportdiameter.TabIndex = 11;
            this.combo_transportdiameter.SelectedValueChanged += new System.EventHandler(this.Combo_transportdiameter_SelectedValueChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.flowLayoutPanel1);
            this.groupBox1.Location = new System.Drawing.Point(937, 53);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(356, 658);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "WTI Settings";
            // 
            // generateButton
            // 
            this.generateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.generateButton.Location = new System.Drawing.Point(1135, 729);
            this.generateButton.Name = "generateButton";
            this.generateButton.Size = new System.Drawing.Size(158, 58);
            this.generateButton.TabIndex = 2;
            this.generateButton.Text = "Generate";
            this.generateButton.UseVisualStyleBackColor = true;
            this.generateButton.Click += new System.EventHandler(this.GenerateButton_Click);
            // 
            // button_convertplaceholders
            // 
            this.button_convertplaceholders.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_convertplaceholders.AutoSize = true;
            this.button_convertplaceholders.Location = new System.Drawing.Point(947, 764);
            this.button_convertplaceholders.Margin = new System.Windows.Forms.Padding(5);
            this.button_convertplaceholders.Name = "button_convertplaceholders";
            this.button_convertplaceholders.Size = new System.Drawing.Size(154, 21);
            this.button_convertplaceholders.TabIndex = 1;
            this.button_convertplaceholders.Text = "Convert placeholders";
            this.button_convertplaceholders.UseVisualStyleBackColor = true;
            this.button_convertplaceholders.CheckedChanged += new System.EventHandler(this.CheckBox1_CheckedChanged);
            // 
            // WTIForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1305, 803);
            this.Controls.Add(this.button_convertplaceholders);
            this.Controls.Add(this.generateButton);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.titleLabel);
            this.Controls.Add(this.canvas);
            this.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(800, 850);
            this.Name = "WTIForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "Avant WTI";
            this.Activated += new System.EventHandler(this.WTIForm_Activated);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.num_valveheight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_pipecolumndistance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_valvecolumndistance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_backwalldistance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_interdistance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_distributionheight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_transportheight)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }





        #endregion

        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Panel canvas;
        private FlowLayoutPanel flowLayoutPanel1;
        private Label selectLabel;
        private GroupBox groupBox1;
        private Button selectButton;
        private Label pipetypelabel;
        private ComboBox combo_pipetype;
        private Label transportsystemtypelabel;
        private ComboBox combo_transportsystem;
        private Label distributionsystemtypelabel;
        private ComboBox combo_distributionsystem;
        private Label valvelabel;
        private ComboBox combo_valvefamily;
        private Label transporteheightlabel;
        private NumericUpDown num_transportheight;
        private Label distributionheightlabel;
        private NumericUpDown num_distributionheight;
        private GroupBox groupBox2;
        private TableLayoutPanel tableLayoutPanel1;
        private Label label1;
        private Label label2;
        private Label label3;
        private NumericUpDown num_valvecolumndistance;
        private NumericUpDown num_backwalldistance;
        private NumericUpDown num_interdistance;
        private Label label4;
        private NumericUpDown num_pipecolumndistance;
        private Button generateButton;
        private Label label5;
        private ComboBox combo_distributiondiameter;
        private Label label6;
        private ComboBox combo_transportdiameter;
        private CheckBox button_convertplaceholders;
        private NumericUpDown num_valveheight;
        private Label label7;
    }
}

