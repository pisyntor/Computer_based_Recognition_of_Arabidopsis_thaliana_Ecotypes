using System.Drawing;
using System.Windows.Forms;

namespace PlantEnhancer
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
            this.groupBox_Root_Input_Folder = new System.Windows.Forms.GroupBox();
            this.button_Browse_Root_Input_Folder = new System.Windows.Forms.Button();
            this.textBox_Root_Input_FolderName = new System.Windows.Forms.TextBox();
            this.label_Root_Input_Folder = new System.Windows.Forms.Label();
            this.groupBox_Root_Output_Folder = new System.Windows.Forms.GroupBox();
            this.button_Browse_Root_Output_Folder = new System.Windows.Forms.Button();
            this.textBox_Root_Output_FolderName = new System.Windows.Forms.TextBox();
            this.label_Root_Output_Folder = new System.Windows.Forms.Label();
            this.groupBox_Class_Rep_Name_Lists = new System.Windows.Forms.GroupBox();
            this.listBox_Images = new System.Windows.Forms.ListBox();
            this.listBox_Reps = new System.Windows.Forms.ListBox();
            this.listBox_Classes = new System.Windows.Forms.ListBox();
            this.label_Image_List = new System.Windows.Forms.Label();
            this.label_Rep_List = new System.Windows.Forms.Label();
            this.label_Class_List = new System.Windows.Forms.Label();
            this.label_Current_Source_Image_File_Name = new System.Windows.Forms.Label();
            this.textBox_Current_Source_Image_File_Name = new System.Windows.Forms.TextBox();
            this.pictureBox_Input_Image = new System.Windows.Forms.PictureBox();
            this.pictureBox_Output_Image = new System.Windows.Forms.PictureBox();
            this.button_Save_Image = new System.Windows.Forms.Button();
            this.button_Exit = new System.Windows.Forms.Button();
            this.label_Cursor_Position = new System.Windows.Forms.Label();
            this.textBox_Cursor_Position = new System.Windows.Forms.TextBox();
            this.label_Pixel_Value = new System.Windows.Forms.Label();
            this.textBox_RGB_Input = new System.Windows.Forms.TextBox();
            this.textBox_RGB_Output = new System.Windows.Forms.TextBox();
            this.label_RGB_From_To = new System.Windows.Forms.Label();
            this.button_Process_All_Images = new System.Windows.Forms.Button();
            this.groupBox_Color_Deconvolution = new System.Windows.Forms.GroupBox();
            this.numericUpDown_Multiply = new System.Windows.Forms.NumericUpDown();
            this.label_Multiply = new System.Windows.Forms.Label();
            this.numericUpDown_Undesired_B = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_Undesired_G = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_Undesired_R = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_Desired_B = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_Desired_G = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_Desired_R = new System.Windows.Forms.NumericUpDown();
            this.label_Undesired_Color = new System.Windows.Forms.Label();
            this.label_Desired_Color = new System.Windows.Forms.Label();
            this.checkBox_Show_Mask = new System.Windows.Forms.CheckBox();
            this.numericUpDown_Suppress_Background = new System.Windows.Forms.NumericUpDown();
            this.checkBox_Suppress_Background = new System.Windows.Forms.CheckBox();
            this.numericUpDown_Otsu_Shift = new System.Windows.Forms.NumericUpDown();
            this.label_Otsu_Shift = new System.Windows.Forms.Label();
            this.button_Process_Selected_Class = new System.Windows.Forms.Button();
            this.checkBox_Smooth = new System.Windows.Forms.CheckBox();
            this.numericUpDown_Gaussian_sigma = new System.Windows.Forms.NumericUpDown();
            this.groupBox_Root_Input_Folder.SuspendLayout();
            this.groupBox_Root_Output_Folder.SuspendLayout();
            this.groupBox_Class_Rep_Name_Lists.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_Input_Image)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_Output_Image)).BeginInit();
            this.groupBox_Color_Deconvolution.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Multiply)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Undesired_B)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Undesired_G)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Undesired_R)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Desired_B)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Desired_G)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Desired_R)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Suppress_Background)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Otsu_Shift)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Gaussian_sigma)).BeginInit();
            this.SuspendLayout();

            // GUI colors
            Color color_imageBackground = System.Drawing.Color.Black;

            //// Colors #0 initial
            //Color color_window = System.Drawing.SystemColors.GradientInactiveCaption;
            //Color color_groupBox = System.Drawing.Color.AntiqueWhite;
            //Color color_browseButton = System.Drawing.Color.LightPink;
            //Color color_processButton = System.Drawing.Color.YellowGreen;
            //Color color_saveExitButton = System.Drawing.Color.Gold;

            //// Colors #1
            //Color color_window = System.Drawing.SystemColors.GradientInactiveCaption;
            //Color color_groupBox = System.Drawing.Color.Azure;
            //Color color_browseButton = System.Drawing.Color.LightSteelBlue;
            //Color color_processButton = color_browseButton;
            //Color color_saveExitButton = color_browseButton;

            //// Colors #2
            //Color color_window = System.Drawing.Color.PaleTurquoise;
            //Color color_groupBox = System.Drawing.Color.Azure;
            //Color color_browseButton = System.Drawing.Color.Turquoise;
            //Color color_processButton = color_browseButton;
            //Color color_saveExitButton = color_browseButton;

            //// Colors #3
            //Color color_window = System.Drawing.Color.Moccasin;
            //Color color_groupBox = System.Drawing.Color.LightYellow;
            //Color color_browseButton = System.Drawing.Color.BurlyWood;
            //Color color_processButton = color_browseButton;
            //Color color_saveExitButton = color_browseButton;

            //// Colors #4
            //Color color_window = System.Drawing.Color.PowderBlue;
            //Color color_groupBox = System.Drawing.Color.AliceBlue;
            //Color color_browseButton = System.Drawing.Color.Silver;
            //Color color_processButton = System.Drawing.Color.DarkGray;
            //Color color_saveExitButton = color_processButton;

            //// Colors #5
            //Color color_window = System.Drawing.Color.LightGray;
            //Color color_groupBox = System.Drawing.Color.Snow;
            //Color color_browseButton = System.Drawing.Color.Silver;
            //Color color_processButton = System.Drawing.Color.DarkGray;
            //Color color_saveExitButton = color_processButton;

            //// Colors #6
            //Color color_window = System.Drawing.Color.PaleTurquoise;
            //Color color_groupBox = System.Drawing.Color.LightCyan;
            //Color color_browseButton = System.Drawing.Color.LightGray;
            //Color color_processButton = System.Drawing.Color.Silver;
            //Color color_saveExitButton = color_processButton;

            //// Colors #7
            //Color color_window = System.Drawing.Color.Gainsboro;
            //Color color_groupBox = System.Drawing.Color.Honeydew;
            //Color color_browseButton = System.Drawing.Color.MediumAquamarine;
            //Color color_processButton = System.Drawing.Color.MediumAquamarine;
            //Color color_saveExitButton = color_processButton;

            //// Colors #8
            //Color color_window = System.Drawing.Color.Silver;
            //Color color_groupBox = System.Drawing.Color.Lavender;
            //Color color_browseButton = System.Drawing.Color.CadetBlue;
            //Color color_processButton = System.Drawing.Color.CadetBlue;
            //Color color_saveExitButton = color_processButton;

            //// Colors #9
            //Color color_window = System.Drawing.Color.LightSteelBlue;
            //Color color_groupBox = System.Drawing.Color.Lavender;
            //Color color_browseButton = System.Drawing.Color.CadetBlue;
            //Color color_processButton = System.Drawing.Color.CadetBlue;
            //Color color_saveExitButton = color_processButton;

            //// Colors #10
            //Color color_window = System.Drawing.Color.Moccasin;
            //Color color_groupBox = System.Drawing.Color.Cornsilk;
            //Color color_browseButton = System.Drawing.Color.BurlyWood;
            //Color color_processButton = System.Drawing.Color.BurlyWood;
            //Color color_saveExitButton = color_processButton;

            //// Colors #11
            //Color color_window = System.Drawing.Color.Gainsboro;
            //Color color_groupBox = System.Drawing.Color.AntiqueWhite;
            //Color color_browseButton = System.Drawing.Color.LightPink;
            //Color color_processButton = System.Drawing.Color.YellowGreen;
            //Color color_saveExitButton = System.Drawing.Color.Gold;

            //// Colors #12
            //Color color_window = System.Drawing.Color.Silver;
            //Color color_groupBox = System.Drawing.Color.AntiqueWhite;
            //Color color_browseButton = System.Drawing.Color.LightPink;
            //Color color_processButton = System.Drawing.Color.YellowGreen;
            //Color color_saveExitButton = System.Drawing.Color.Gold;

            //// Colors #13
            //Color color_window = System.Drawing.Color.PowderBlue;
            //Color color_groupBox = System.Drawing.Color.AntiqueWhite;
            //Color color_browseButton = System.Drawing.Color.LightPink;
            //Color color_processButton = System.Drawing.Color.YellowGreen;
            //Color color_saveExitButton = System.Drawing.Color.Gold;

            // Colors #14
            Color color_window = System.Drawing.Color.PowderBlue;
            Color color_groupBox = System.Drawing.Color.AntiqueWhite;
            Color color_browseButton = System.Drawing.Color.YellowGreen;
            Color color_processButton = System.Drawing.Color.Gold;
            Color color_saveExitButton = System.Drawing.Color.Gold;

            // 
            // groupBox_Root_Input_Folder
            // 
            this.groupBox_Root_Input_Folder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            //this.groupBox_Root_Input_Folder.BackColor = System.Drawing.Color.AntiqueWhite;
            this.groupBox_Root_Input_Folder.BackColor = color_groupBox;
            this.groupBox_Root_Input_Folder.Controls.Add(this.button_Browse_Root_Input_Folder);
            this.groupBox_Root_Input_Folder.Controls.Add(this.textBox_Root_Input_FolderName);
            this.groupBox_Root_Input_Folder.Controls.Add(this.label_Root_Input_Folder);
            this.groupBox_Root_Input_Folder.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox_Root_Input_Folder.Location = new System.Drawing.Point(14, 12);
            this.groupBox_Root_Input_Folder.Name = "groupBox_Root_Input_Folder";
            this.groupBox_Root_Input_Folder.Size = new System.Drawing.Size(1421, 61);
            this.groupBox_Root_Input_Folder.TabIndex = 7;
            this.groupBox_Root_Input_Folder.TabStop = false;
            this.groupBox_Root_Input_Folder.Text = "Root Folder of Source Images";
            // 
            // button_Browse_Root_Input_Folder
            // 
            this.button_Browse_Root_Input_Folder.Anchor = System.Windows.Forms.AnchorStyles.Right;
            //this.button_Browse_Root_Input_Folder.BackColor = System.Drawing.Color.LightPink;
            this.button_Browse_Root_Input_Folder.BackColor = color_browseButton;
            this.button_Browse_Root_Input_Folder.Location = new System.Drawing.Point(1324, 24);
            this.button_Browse_Root_Input_Folder.Name = "button_Browse_Root_Input_Folder";
            this.button_Browse_Root_Input_Folder.Size = new System.Drawing.Size(89, 27);
            this.button_Browse_Root_Input_Folder.TabIndex = 50;
            this.button_Browse_Root_Input_Folder.Text = "Browse";
            this.button_Browse_Root_Input_Folder.UseVisualStyleBackColor = false;
            this.button_Browse_Root_Input_Folder.Click += new System.EventHandler(this.button_Browse_Root_Input_Folder_Click);
            // 
            // textBox_Root_Input_FolderName
            // 
            this.textBox_Root_Input_FolderName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_Root_Input_FolderName.Location = new System.Drawing.Point(213, 28);
            this.textBox_Root_Input_FolderName.Name = "textBox_Root_Input_FolderName";
            this.textBox_Root_Input_FolderName.Size = new System.Drawing.Size(1101, 20);
            this.textBox_Root_Input_FolderName.TabIndex = 46;
            this.textBox_Root_Input_FolderName.TextChanged += new System.EventHandler(this.textBox_Root_Input_FolderName_TextChanged);
            // 
            // label_Root_Input_Folder
            // 
            this.label_Root_Input_Folder.AutoSize = true;
            this.label_Root_Input_Folder.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_Root_Input_Folder.Location = new System.Drawing.Point(22, 30);
            this.label_Root_Input_Folder.Name = "label_Root_Input_Folder";
            this.label_Root_Input_Folder.Size = new System.Drawing.Size(161, 13);
            this.label_Root_Input_Folder.TabIndex = 0;
            this.label_Root_Input_Folder.Text = "Name of Root Input Folder:";
            // 
            // groupBox_Root_Output_Folder
            // 
            this.groupBox_Root_Output_Folder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            //this.groupBox_Root_Output_Folder.BackColor = System.Drawing.Color.AntiqueWhite;
            this.groupBox_Root_Output_Folder.BackColor = color_groupBox;
            this.groupBox_Root_Output_Folder.Controls.Add(this.button_Browse_Root_Output_Folder);
            this.groupBox_Root_Output_Folder.Controls.Add(this.textBox_Root_Output_FolderName);
            this.groupBox_Root_Output_Folder.Controls.Add(this.label_Root_Output_Folder);
            this.groupBox_Root_Output_Folder.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox_Root_Output_Folder.Location = new System.Drawing.Point(14, 79);
            this.groupBox_Root_Output_Folder.Name = "groupBox_Root_Output_Folder";
            this.groupBox_Root_Output_Folder.Size = new System.Drawing.Size(1421, 61);
            this.groupBox_Root_Output_Folder.TabIndex = 8;
            this.groupBox_Root_Output_Folder.TabStop = false;
            this.groupBox_Root_Output_Folder.Text = "Root Folder of Enhanced Images";
            // 
            // button_Browse_Root_Output_Folder
            // 
            this.button_Browse_Root_Output_Folder.Anchor = System.Windows.Forms.AnchorStyles.Right;
            //this.button_Browse_Root_Output_Folder.BackColor = System.Drawing.Color.LightPink;
            this.button_Browse_Root_Output_Folder.BackColor = color_browseButton;
            this.button_Browse_Root_Output_Folder.Location = new System.Drawing.Point(1324, 24);
            this.button_Browse_Root_Output_Folder.Name = "button_Browse_Root_Output_Folder";
            this.button_Browse_Root_Output_Folder.Size = new System.Drawing.Size(89, 27);
            this.button_Browse_Root_Output_Folder.TabIndex = 50;
            this.button_Browse_Root_Output_Folder.Text = "Browse";
            this.button_Browse_Root_Output_Folder.UseVisualStyleBackColor = false;
            this.button_Browse_Root_Output_Folder.MouseCaptureChanged += new System.EventHandler(this.button_Browse_Root_Output_Folder_MouseCaptureChanged);
            // 
            // textBox_Root_Output_FolderName
            // 
            this.textBox_Root_Output_FolderName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_Root_Output_FolderName.Location = new System.Drawing.Point(213, 28);
            this.textBox_Root_Output_FolderName.Name = "textBox_Root_Output_FolderName";
            this.textBox_Root_Output_FolderName.Size = new System.Drawing.Size(1101, 20);
            this.textBox_Root_Output_FolderName.TabIndex = 46;
            // 
            // label_Root_Output_Folder
            // 
            this.label_Root_Output_Folder.AutoSize = true;
            //this.label_Root_Output_Folder.BackColor = System.Drawing.Color.AntiqueWhite;
            this.label_Root_Output_Folder.BackColor = color_groupBox;
            this.label_Root_Output_Folder.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_Root_Output_Folder.Location = new System.Drawing.Point(8, 30);
            this.label_Root_Output_Folder.Name = "label_Root_Output_Folder";
            this.label_Root_Output_Folder.Size = new System.Drawing.Size(170, 13);
            this.label_Root_Output_Folder.TabIndex = 0;
            this.label_Root_Output_Folder.Text = "Name of Root Output Folder:";
            // 
            // groupBox_Class_Rep_Name_Lists
            // 
            this.groupBox_Class_Rep_Name_Lists.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            //this.groupBox_Class_Rep_Name_Lists.BackColor = System.Drawing.Color.AntiqueWhite;
            this.groupBox_Class_Rep_Name_Lists.BackColor = color_groupBox;
            this.groupBox_Class_Rep_Name_Lists.Controls.Add(this.listBox_Images);
            this.groupBox_Class_Rep_Name_Lists.Controls.Add(this.listBox_Reps);
            this.groupBox_Class_Rep_Name_Lists.Controls.Add(this.listBox_Classes);
            this.groupBox_Class_Rep_Name_Lists.Controls.Add(this.label_Image_List);
            this.groupBox_Class_Rep_Name_Lists.Controls.Add(this.label_Rep_List);
            this.groupBox_Class_Rep_Name_Lists.Controls.Add(this.label_Class_List);
            this.groupBox_Class_Rep_Name_Lists.Location = new System.Drawing.Point(14, 175);
            this.groupBox_Class_Rep_Name_Lists.Name = "groupBox_Class_Rep_Name_Lists";
            this.groupBox_Class_Rep_Name_Lists.Size = new System.Drawing.Size(357, 237);
            this.groupBox_Class_Rep_Name_Lists.TabIndex = 49;
            this.groupBox_Class_Rep_Name_Lists.TabStop = false;
            // 
            // listBox_Images
            // 
            this.listBox_Images.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBox_Images.FormattingEnabled = true;
            this.listBox_Images.Location = new System.Drawing.Point(286, 35);
            this.listBox_Images.Name = "listBox_Images";
            this.listBox_Images.Size = new System.Drawing.Size(54, 186);
            this.listBox_Images.TabIndex = 16;
            this.listBox_Images.SelectedIndexChanged += new System.EventHandler(this.listBox_Images_SelectedIndexChanged);
            // 
            // listBox_Reps
            // 
            this.listBox_Reps.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBox_Reps.FormattingEnabled = true;
            this.listBox_Reps.Location = new System.Drawing.Point(208, 35);
            this.listBox_Reps.Name = "listBox_Reps";
            this.listBox_Reps.Size = new System.Drawing.Size(68, 186);
            this.listBox_Reps.TabIndex = 15;
            this.listBox_Reps.SelectedIndexChanged += new System.EventHandler(this.listBox_Reps_SelectedIndexChanged);
            // 
            // listBox_Classes
            // 
            this.listBox_Classes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBox_Classes.FormattingEnabled = true;
            this.listBox_Classes.Location = new System.Drawing.Point(13, 35);
            this.listBox_Classes.Name = "listBox_Classes";
            this.listBox_Classes.Size = new System.Drawing.Size(187, 186);
            this.listBox_Classes.TabIndex = 14;
            this.listBox_Classes.SelectedIndexChanged += new System.EventHandler(this.listBox_Classes_SelectedIndexChanged);
            // 
            // label_Image_List
            // 
            this.label_Image_List.AutoSize = true;
            this.label_Image_List.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_Image_List.Location = new System.Drawing.Point(286, 15);
            this.label_Image_List.Name = "label_Image_List";
            this.label_Image_List.Size = new System.Drawing.Size(38, 13);
            this.label_Image_List.TabIndex = 2;
            this.label_Image_List.Text = "Data:";
            // 
            // label_Rep_List
            // 
            this.label_Rep_List.AutoSize = true;
            this.label_Rep_List.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_Rep_List.Location = new System.Drawing.Point(205, 15);
            this.label_Rep_List.Name = "label_Rep_List";
            this.label_Rep_List.Size = new System.Drawing.Size(40, 13);
            this.label_Rep_List.TabIndex = 1;
            //this.label_Rep_List.Text = "Reps:";
            this.label_Rep_List.Text = "Replicates:";
            // 
            // label_Class_List
            // 
            this.label_Class_List.AutoSize = true;
            this.label_Class_List.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_Class_List.Location = new System.Drawing.Point(10, 15);
            this.label_Class_List.Name = "label_Class_List";
            this.label_Class_List.Size = new System.Drawing.Size(69, 13);
            this.label_Class_List.TabIndex = 0;
            //this.label_Class_List.Text = "Accession:";
            this.label_Class_List.Text = "Ecotype:";
            // 
            // label_Current_Source_Image_File_Name
            // 
            this.label_Current_Source_Image_File_Name.AutoSize = true;
            this.label_Current_Source_Image_File_Name.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_Current_Source_Image_File_Name.Location = new System.Drawing.Point(59, 152);
            this.label_Current_Source_Image_File_Name.Name = "label_Current_Source_Image_File_Name";
            this.label_Current_Source_Image_File_Name.Size = new System.Drawing.Size(138, 13);
            this.label_Current_Source_Image_File_Name.TabIndex = 50;
            this.label_Current_Source_Image_File_Name.Text = "Current Source Image::";
            // 
            // textBox_Current_Source_Image_File_Name
            // 
            this.textBox_Current_Source_Image_File_Name.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_Current_Source_Image_File_Name.Enabled = false;
            this.textBox_Current_Source_Image_File_Name.Location = new System.Drawing.Point(227, 149);
            this.textBox_Current_Source_Image_File_Name.Name = "textBox_Current_Source_Image_File_Name";
            this.textBox_Current_Source_Image_File_Name.Size = new System.Drawing.Size(1207, 20);
            this.textBox_Current_Source_Image_File_Name.TabIndex = 51;
            this.textBox_Current_Source_Image_File_Name.TextChanged += new System.EventHandler(this.textBox_Current_Source_Image_File_Name_TextChanged);
            // 
            // pictureBox_Input_Image
            // 
            this.pictureBox_Input_Image.BackColor = color_imageBackground;
            this.pictureBox_Input_Image.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox_Input_Image.Cursor = System.Windows.Forms.Cursors.Cross;
            this.pictureBox_Input_Image.Location = new System.Drawing.Point(379, 176);
            this.pictureBox_Input_Image.Name = "pictureBox_Input_Image";
            this.pictureBox_Input_Image.Size = new System.Drawing.Size(525, 444);
            this.pictureBox_Input_Image.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox_Input_Image.TabIndex = 52;
            this.pictureBox_Input_Image.TabStop = false;
            this.pictureBox_Input_Image.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox_Image_MouseDown);
            this.pictureBox_Input_Image.MouseLeave += new System.EventHandler(this.pictureBox_Image_MouseLeave);
            this.pictureBox_Input_Image.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox_Image_MouseMove);
            // 
            // pictureBox_Output_Image
            // 
            this.pictureBox_Output_Image.BackColor = color_imageBackground;
            this.pictureBox_Output_Image.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox_Output_Image.Cursor = System.Windows.Forms.Cursors.Cross;
            this.pictureBox_Output_Image.Location = new System.Drawing.Point(910, 176);
            this.pictureBox_Output_Image.Name = "pictureBox_Output_Image";
            this.pictureBox_Output_Image.Size = new System.Drawing.Size(525, 444);
            this.pictureBox_Output_Image.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox_Output_Image.TabIndex = 53;
            this.pictureBox_Output_Image.TabStop = false;
            this.pictureBox_Output_Image.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox_Image_MouseDown);
            this.pictureBox_Output_Image.MouseLeave += new System.EventHandler(this.pictureBox_Image_MouseLeave);
            this.pictureBox_Output_Image.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox_Image_MouseMove);
            // 
            // button_Save_Image
            // 
            this.button_Save_Image.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            //this.button_Save_Image.BackColor = System.Drawing.Color.Gold;
            this.button_Save_Image.BackColor = color_saveExitButton;
            this.button_Save_Image.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_Save_Image.Location = new System.Drawing.Point(1141, 627);
            this.button_Save_Image.Name = "button_Save_Image";
            this.button_Save_Image.Size = new System.Drawing.Size(188, 28);
            this.button_Save_Image.TabIndex = 59;
            //this.button_Save_Image.Text = "Save Enhanced Image";
            this.button_Save_Image.Text = "Save enhanced image";
            this.button_Save_Image.UseVisualStyleBackColor = false;
            this.button_Save_Image.Click += new System.EventHandler(this.button_Save_Image_Click);
            // 
            // button_Exit
            // 
            this.button_Exit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            //this.button_Exit.BackColor = System.Drawing.Color.LightGray;
            this.button_Exit.BackColor = color_saveExitButton;
            this.button_Exit.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_Exit.Location = new System.Drawing.Point(1338, 627);
            this.button_Exit.Name = "button_Exit";
            this.button_Exit.Size = new System.Drawing.Size(97, 28);
            this.button_Exit.TabIndex = 58;
            this.button_Exit.Text = "Exit";
            this.button_Exit.UseVisualStyleBackColor = false;
            this.button_Exit.Click += new System.EventHandler(this.button_Exit_Click);
            // 
            // label_Cursor_Position
            // 
            this.label_Cursor_Position.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label_Cursor_Position.AutoSize = true;
            this.label_Cursor_Position.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_Cursor_Position.Location = new System.Drawing.Point(376, 635);
            this.label_Cursor_Position.Name = "label_Cursor_Position";
            this.label_Cursor_Position.Size = new System.Drawing.Size(96, 13);
            this.label_Cursor_Position.TabIndex = 61;
            this.label_Cursor_Position.Text = "Cursor Position:";
            // 
            // textBox_Cursor_Position
            // 
            this.textBox_Cursor_Position.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_Cursor_Position.Enabled = false;
            this.textBox_Cursor_Position.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_Cursor_Position.Location = new System.Drawing.Point(495, 632);
            this.textBox_Cursor_Position.Name = "textBox_Cursor_Position";
            this.textBox_Cursor_Position.Size = new System.Drawing.Size(117, 20);
            this.textBox_Cursor_Position.TabIndex = 60;
            this.textBox_Cursor_Position.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label_Pixel_Value
            // 
            this.label_Pixel_Value.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label_Pixel_Value.AutoSize = true;
            this.label_Pixel_Value.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_Pixel_Value.Location = new System.Drawing.Point(652, 635);
            this.label_Pixel_Value.Name = "label_Pixel_Value";
            this.label_Pixel_Value.Size = new System.Drawing.Size(120, 13);
            this.label_Pixel_Value.TabIndex = 62;
            this.label_Pixel_Value.Text = "Pixel Value (R,G,B):";
            // 
            // textBox_RGB_Input
            // 
            this.textBox_RGB_Input.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_RGB_Input.Enabled = false;
            this.textBox_RGB_Input.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_RGB_Input.Location = new System.Drawing.Point(799, 632);
            this.textBox_RGB_Input.Name = "textBox_RGB_Input";
            this.textBox_RGB_Input.Size = new System.Drawing.Size(117, 20);
            this.textBox_RGB_Input.TabIndex = 63;
            this.textBox_RGB_Input.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBox_RGB_Output
            // 
            this.textBox_RGB_Output.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_RGB_Output.Enabled = false;
            this.textBox_RGB_Output.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_RGB_Output.Location = new System.Drawing.Point(958, 632);
            this.textBox_RGB_Output.Name = "textBox_RGB_Output";
            this.textBox_RGB_Output.Size = new System.Drawing.Size(117, 20);
            this.textBox_RGB_Output.TabIndex = 64;
            this.textBox_RGB_Output.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label_RGB_From_To
            // 
            this.label_RGB_From_To.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label_RGB_From_To.AutoSize = true;
            this.label_RGB_From_To.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_RGB_From_To.Location = new System.Drawing.Point(920, 635);
            this.label_RGB_From_To.Name = "label_RGB_From_To";
            this.label_RGB_From_To.Size = new System.Drawing.Size(28, 13);
            this.label_RGB_From_To.TabIndex = 65;
            this.label_RGB_From_To.Text = ">>>";
            // 
            // button_Process_All_Images
            // 
            this.button_Process_All_Images.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            //this.button_Process_All_Images.BackColor = System.Drawing.Color.YellowGreen;
            this.button_Process_All_Images.BackColor = color_processButton;
            this.button_Process_All_Images.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_Process_All_Images.Location = new System.Drawing.Point(216, 627);
            this.button_Process_All_Images.Name = "button_Process_All_Images";
            this.button_Process_All_Images.Size = new System.Drawing.Size(155, 28);
            this.button_Process_All_Images.TabIndex = 66;
            //this.button_Process_All_Images.Text = "Process All Images";
            this.button_Process_All_Images.Text = "Process all ecotypes";
            this.button_Process_All_Images.UseVisualStyleBackColor = false;
            this.button_Process_All_Images.Click += new System.EventHandler(this.button_Process_All_Images_Click);
            // 
            // groupBox_Color_Deconvolution
            // 
            this.groupBox_Color_Deconvolution.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            //this.groupBox_Color_Deconvolution.BackColor = System.Drawing.Color.AntiqueWhite;
            this.groupBox_Color_Deconvolution.BackColor = color_groupBox;
            this.groupBox_Color_Deconvolution.Controls.Add(this.numericUpDown_Gaussian_sigma);
            this.groupBox_Color_Deconvolution.Controls.Add(this.checkBox_Smooth);
            this.groupBox_Color_Deconvolution.Controls.Add(this.numericUpDown_Multiply);
            this.groupBox_Color_Deconvolution.Controls.Add(this.label_Multiply);
            this.groupBox_Color_Deconvolution.Controls.Add(this.numericUpDown_Undesired_B);
            this.groupBox_Color_Deconvolution.Controls.Add(this.numericUpDown_Undesired_G);
            this.groupBox_Color_Deconvolution.Controls.Add(this.numericUpDown_Undesired_R);
            this.groupBox_Color_Deconvolution.Controls.Add(this.numericUpDown_Desired_B);
            this.groupBox_Color_Deconvolution.Controls.Add(this.numericUpDown_Desired_G);
            this.groupBox_Color_Deconvolution.Controls.Add(this.numericUpDown_Desired_R);
            this.groupBox_Color_Deconvolution.Controls.Add(this.label_Undesired_Color);
            this.groupBox_Color_Deconvolution.Controls.Add(this.label_Desired_Color);
            this.groupBox_Color_Deconvolution.Controls.Add(this.numericUpDown_Suppress_Background);
            this.groupBox_Color_Deconvolution.Controls.Add(this.checkBox_Suppress_Background);
            this.groupBox_Color_Deconvolution.Controls.Add(this.numericUpDown_Otsu_Shift);
            this.groupBox_Color_Deconvolution.Controls.Add(this.label_Otsu_Shift);
            this.groupBox_Color_Deconvolution.Controls.Add(this.checkBox_Show_Mask);
            this.groupBox_Color_Deconvolution.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox_Color_Deconvolution.Location = new System.Drawing.Point(15, 418);
            this.groupBox_Color_Deconvolution.Name = "groupBox_Color_Deconvolution";
            this.groupBox_Color_Deconvolution.Size = new System.Drawing.Size(357, 188);  // (357, 168);
            this.groupBox_Color_Deconvolution.TabIndex = 159;
            this.groupBox_Color_Deconvolution.TabStop = false;
            this.groupBox_Color_Deconvolution.Text = "Color Correction";
            // 
            // numericUpDown_Multiply
            // 
            this.numericUpDown_Multiply.DecimalPlaces = 1;
            this.numericUpDown_Multiply.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDown_Multiply.Location = new System.Drawing.Point(181, 139);
            this.numericUpDown_Multiply.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            65536});
            this.numericUpDown_Multiply.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.numericUpDown_Multiply.Name = "numericUpDown_Multiply";
            this.numericUpDown_Multiply.Size = new System.Drawing.Size(51, 20);
            this.numericUpDown_Multiply.TabIndex = 174;
            this.numericUpDown_Multiply.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDown_Multiply.Value = new decimal(new int[] {
            15,
            0,
            0,
            65536});
            this.numericUpDown_Multiply.ValueChanged += new System.EventHandler(this.Color_Deconvolution_Parameter_Changed);
            // 
            // label_Multiply
            // 
            this.label_Multiply.AutoSize = true;
            this.label_Multiply.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_Multiply.Location = new System.Drawing.Point(124, 141);
            this.label_Multiply.Name = "label_Multiply";
            this.label_Multiply.Size = new System.Drawing.Size(54, 13);
            this.label_Multiply.TabIndex = 173;
            this.label_Multiply.Text = "Multiply:";
            // 
            // numericUpDown_Undesired_B
            // 
            this.numericUpDown_Undesired_B.Location = new System.Drawing.Point(297, 49);
            this.numericUpDown_Undesired_B.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDown_Undesired_B.Name = "numericUpDown_Undesired_B";
            this.numericUpDown_Undesired_B.Size = new System.Drawing.Size(51, 20);
            this.numericUpDown_Undesired_B.TabIndex = 172;
            this.numericUpDown_Undesired_B.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDown_Undesired_B.Value = new decimal(new int[] {
            123,
            0,
            0,
            0});
            this.numericUpDown_Undesired_B.ValueChanged += new System.EventHandler(this.Color_Deconvolution_Parameter_Changed);
            // 
            // numericUpDown_Undesired_G
            // 
            this.numericUpDown_Undesired_G.Location = new System.Drawing.Point(239, 49);
            this.numericUpDown_Undesired_G.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDown_Undesired_G.Name = "numericUpDown_Undesired_G";
            this.numericUpDown_Undesired_G.Size = new System.Drawing.Size(51, 20);
            this.numericUpDown_Undesired_G.TabIndex = 171;
            this.numericUpDown_Undesired_G.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDown_Undesired_G.Value = new decimal(new int[] {
            83,
            0,
            0,
            0});
            this.numericUpDown_Undesired_G.ValueChanged += new System.EventHandler(this.Color_Deconvolution_Parameter_Changed);
            // 
            // numericUpDown_Undesired_R
            // 
            this.numericUpDown_Undesired_R.Location = new System.Drawing.Point(181, 49);
            this.numericUpDown_Undesired_R.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDown_Undesired_R.Name = "numericUpDown_Undesired_R";
            this.numericUpDown_Undesired_R.Size = new System.Drawing.Size(51, 20);
            this.numericUpDown_Undesired_R.TabIndex = 170;
            this.numericUpDown_Undesired_R.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDown_Undesired_R.Value = new decimal(new int[] {
            12,
            0,
            0,
            0});
            this.numericUpDown_Undesired_R.ValueChanged += new System.EventHandler(this.Color_Deconvolution_Parameter_Changed);
            // 
            // numericUpDown_Desired_B
            // 
            this.numericUpDown_Desired_B.Location = new System.Drawing.Point(297, 21);
            this.numericUpDown_Desired_B.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDown_Desired_B.Name = "numericUpDown_Desired_B";
            this.numericUpDown_Desired_B.Size = new System.Drawing.Size(51, 20);
            this.numericUpDown_Desired_B.TabIndex = 169;
            this.numericUpDown_Desired_B.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDown_Desired_B.Value = new decimal(new int[] {
            70,
            0,
            0,
            0});
            this.numericUpDown_Desired_B.ValueChanged += new System.EventHandler(this.Color_Deconvolution_Parameter_Changed);
            // 
            // numericUpDown_Desired_G
            // 
            this.numericUpDown_Desired_G.Location = new System.Drawing.Point(239, 21);
            this.numericUpDown_Desired_G.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDown_Desired_G.Name = "numericUpDown_Desired_G";
            this.numericUpDown_Desired_G.Size = new System.Drawing.Size(51, 20);
            this.numericUpDown_Desired_G.TabIndex = 168;
            this.numericUpDown_Desired_G.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDown_Desired_G.Value = new decimal(new int[] {
            103,
            0,
            0,
            0});
            this.numericUpDown_Desired_G.ValueChanged += new System.EventHandler(this.Color_Deconvolution_Parameter_Changed);
            // 
            // numericUpDown_Desired_R
            // 
            this.numericUpDown_Desired_R.Location = new System.Drawing.Point(181, 21);
            this.numericUpDown_Desired_R.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDown_Desired_R.Name = "numericUpDown_Desired_R";
            this.numericUpDown_Desired_R.Size = new System.Drawing.Size(51, 20);
            this.numericUpDown_Desired_R.TabIndex = 167;
            this.numericUpDown_Desired_R.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDown_Desired_R.Value = new decimal(new int[] {
            88,
            0,
            0,
            0});
            this.numericUpDown_Desired_R.ValueChanged += new System.EventHandler(this.Color_Deconvolution_Parameter_Changed);
            // 
            // label_Undesired_Color
            // 
            this.label_Undesired_Color.AutoSize = true;
            this.label_Undesired_Color.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_Undesired_Color.Location = new System.Drawing.Point(7, 51);
            this.label_Undesired_Color.Name = "label_Undesired_Color";
            this.label_Undesired_Color.Size = new System.Drawing.Size(147, 13);
            this.label_Undesired_Color.TabIndex = 166;
            this.label_Undesired_Color.Text = "Undesired Color (R,G,B):";
            // 
            // label_Desired_Color
            // 
            this.label_Desired_Color.AutoSize = true;
            this.label_Desired_Color.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_Desired_Color.Location = new System.Drawing.Point(21, 23);
            this.label_Desired_Color.Name = "label_Desired_Color";
            this.label_Desired_Color.Size = new System.Drawing.Size(133, 13);
            this.label_Desired_Color.TabIndex = 165;
            this.label_Desired_Color.Text = "Desired Color (R,G,B):";
            // 
            // checkBox_Show_Mask
            // 
            this.checkBox_Show_Mask.AutoSize = true;
            this.checkBox_Show_Mask.Checked = false;
            this.checkBox_Show_Mask.CheckState = System.Windows.Forms.CheckState.Unchecked;
            this.checkBox_Show_Mask.Location = new System.Drawing.Point(263, 168);
            this.checkBox_Show_Mask.Name = "checkBox_Show_Mask";
            this.checkBox_Show_Mask.Size = new System.Drawing.Size(114, 17);
            this.checkBox_Show_Mask.TabIndex = 177;
            this.checkBox_Show_Mask.Text = "Show Mask";
            this.checkBox_Show_Mask.UseVisualStyleBackColor = true;
            this.checkBox_Show_Mask.CheckedChanged += new System.EventHandler(this.Color_Deconvolution_Parameter_Changed);
            // 
            // numericUpDown_Suppress_Background
            // 
            this.numericUpDown_Suppress_Background.DecimalPlaces = 1;
            this.numericUpDown_Suppress_Background.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDown_Suppress_Background.Location = new System.Drawing.Point(297, 111);
            this.numericUpDown_Suppress_Background.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDown_Suppress_Background.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown_Suppress_Background.Name = "numericUpDown_Suppress_Background";
            this.numericUpDown_Suppress_Background.Size = new System.Drawing.Size(51, 20);
            this.numericUpDown_Suppress_Background.TabIndex = 164;
            this.numericUpDown_Suppress_Background.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDown_Suppress_Background.Value = new decimal(new int[] {
            30,
            0,
            0,
            65536});
            this.numericUpDown_Suppress_Background.ValueChanged += new System.EventHandler(this.Color_Deconvolution_Parameter_Changed);
            // 
            // checkBox_Suppress_Background
            // 
            this.checkBox_Suppress_Background.AutoSize = true;
            this.checkBox_Suppress_Background.Checked = true;
            this.checkBox_Suppress_Background.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_Suppress_Background.Location = new System.Drawing.Point(87, 112);
            this.checkBox_Suppress_Background.Name = "checkBox_Suppress_Background";
            this.checkBox_Suppress_Background.Size = new System.Drawing.Size(206, 17);
            this.checkBox_Suppress_Background.TabIndex = 163;
            this.checkBox_Suppress_Background.Text = "Suppress Background Level by:";
            this.checkBox_Suppress_Background.UseVisualStyleBackColor = true;
            this.checkBox_Suppress_Background.CheckedChanged += new System.EventHandler(this.Color_Deconvolution_Parameter_Changed);
            // 
            // numericUpDown_Otsu_Shift
            // 
            this.numericUpDown_Otsu_Shift.Location = new System.Drawing.Point(297, 139);
            this.numericUpDown_Otsu_Shift.Maximum = new decimal(new int[] {
            254,
            0,
            0,
            0});
            this.numericUpDown_Otsu_Shift.Name = "numericUpDown_Otsu_Shift";
            this.numericUpDown_Otsu_Shift.Size = new System.Drawing.Size(51, 20);
            this.numericUpDown_Otsu_Shift.TabIndex = 162;
            this.numericUpDown_Otsu_Shift.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDown_Otsu_Shift.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDown_Otsu_Shift.ValueChanged += new System.EventHandler(this.Color_Deconvolution_Parameter_Changed);
            // 
            // label_Otsu_Shift
            // 
            this.label_Otsu_Shift.AutoSize = true;
            this.label_Otsu_Shift.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_Otsu_Shift.Location = new System.Drawing.Point(253, 141);
            this.label_Otsu_Shift.Name = "label_Otsu_Shift";
            this.label_Otsu_Shift.Size = new System.Drawing.Size(37, 13);
            this.label_Otsu_Shift.TabIndex = 161;
            this.label_Otsu_Shift.Text = "Shift:";
            // 
            // button_Process_Selected_Class
            // 
            this.button_Process_Selected_Class.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            //this.button_Process_Selected_Class.BackColor = System.Drawing.Color.YellowGreen;
            this.button_Process_Selected_Class.BackColor = color_processButton;
            this.button_Process_Selected_Class.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_Process_Selected_Class.Location = new System.Drawing.Point(15, 627);
            this.button_Process_Selected_Class.Name = "button_Process_Selected_Class";
            this.button_Process_Selected_Class.Size = new System.Drawing.Size(191, 28);
            this.button_Process_Selected_Class.TabIndex = 160;
            //this.button_Process_Selected_Class.Text = "Process Selected Class";
            this.button_Process_Selected_Class.Text = "Process selected ecotype";
            this.button_Process_Selected_Class.UseVisualStyleBackColor = false;
            this.button_Process_Selected_Class.Click += new System.EventHandler(this.button_Process_Selected_Class_Click);
            // 
            // checkBox_Smooth
            // 
            this.checkBox_Smooth.AutoSize = true;
            this.checkBox_Smooth.Checked = true;
            this.checkBox_Smooth.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_Smooth.Location = new System.Drawing.Point(171, 85);
            this.checkBox_Smooth.Name = "checkBox_Smooth";
            this.checkBox_Smooth.Size = new System.Drawing.Size(124, 17);
            this.checkBox_Smooth.TabIndex = 175;
            this.checkBox_Smooth.Text = "Smoothing Level:";
            this.checkBox_Smooth.UseVisualStyleBackColor = true;
            this.checkBox_Smooth.CheckedChanged += new System.EventHandler(this.Color_Deconvolution_Parameter_Changed);
            // 
            // numericUpDown_Gaussian_sigma
            // 
            this.numericUpDown_Gaussian_sigma.DecimalPlaces = 1;
            this.numericUpDown_Gaussian_sigma.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDown_Gaussian_sigma.Location = new System.Drawing.Point(297, 81);
            this.numericUpDown_Gaussian_sigma.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDown_Gaussian_sigma.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDown_Gaussian_sigma.Name = "numericUpDown_Gaussian_sigma";
            this.numericUpDown_Gaussian_sigma.Size = new System.Drawing.Size(52, 20);
            this.numericUpDown_Gaussian_sigma.TabIndex = 176;
            this.numericUpDown_Gaussian_sigma.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDown_Gaussian_sigma.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown_Gaussian_sigma.ValueChanged += new System.EventHandler(this.Color_Deconvolution_Parameter_Changed);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            //this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.BackColor = color_window;
            this.ClientSize = new System.Drawing.Size(1449, 662);
            this.Controls.Add(this.button_Process_Selected_Class);
            this.Controls.Add(this.groupBox_Color_Deconvolution);
            this.Controls.Add(this.button_Process_All_Images);
            this.Controls.Add(this.label_RGB_From_To);
            this.Controls.Add(this.textBox_RGB_Output);
            this.Controls.Add(this.textBox_RGB_Input);
            this.Controls.Add(this.label_Pixel_Value);
            this.Controls.Add(this.label_Cursor_Position);
            this.Controls.Add(this.textBox_Cursor_Position);
            this.Controls.Add(this.button_Save_Image);
            this.Controls.Add(this.button_Exit);
            this.Controls.Add(this.pictureBox_Output_Image);
            this.Controls.Add(this.pictureBox_Input_Image);
            this.Controls.Add(this.textBox_Current_Source_Image_File_Name);
            this.Controls.Add(this.label_Current_Source_Image_File_Name);
            this.Controls.Add(this.groupBox_Class_Rep_Name_Lists);
            this.Controls.Add(this.groupBox_Root_Output_Folder);
            this.Controls.Add(this.groupBox_Root_Input_Folder);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(1465, 700);
            this.Name = "Form1";
            this.Text = "PlantEnhancer - Development Environment";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResizeBegin += new System.EventHandler(this.Form1_ResizeBegin);
            this.ResizeEnd += new System.EventHandler(this.Form1_ResizeEnd);
            this.SizeChanged += new System.EventHandler(this.Form1_SizeChanged);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.groupBox_Root_Input_Folder.ResumeLayout(false);
            this.groupBox_Root_Input_Folder.PerformLayout();
            this.groupBox_Root_Output_Folder.ResumeLayout(false);
            this.groupBox_Root_Output_Folder.PerformLayout();
            this.groupBox_Class_Rep_Name_Lists.ResumeLayout(false);
            this.groupBox_Class_Rep_Name_Lists.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_Input_Image)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_Output_Image)).EndInit();
            this.groupBox_Color_Deconvolution.ResumeLayout(false);
            this.groupBox_Color_Deconvolution.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Multiply)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Undesired_B)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Undesired_G)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Undesired_R)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Desired_B)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Desired_G)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Desired_R)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Suppress_Background)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Otsu_Shift)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Gaussian_sigma)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox_Root_Input_Folder;
        private System.Windows.Forms.Button button_Browse_Root_Input_Folder;
        private System.Windows.Forms.TextBox textBox_Root_Input_FolderName;
        private System.Windows.Forms.Label label_Root_Input_Folder;
        private System.Windows.Forms.GroupBox groupBox_Root_Output_Folder;
        private System.Windows.Forms.Button button_Browse_Root_Output_Folder;
        private System.Windows.Forms.TextBox textBox_Root_Output_FolderName;
        private System.Windows.Forms.Label label_Root_Output_Folder;
        private System.Windows.Forms.GroupBox groupBox_Class_Rep_Name_Lists;
        private System.Windows.Forms.ListBox listBox_Images;
        private System.Windows.Forms.ListBox listBox_Reps;
        private System.Windows.Forms.ListBox listBox_Classes;
        private System.Windows.Forms.Label label_Image_List;
        private System.Windows.Forms.Label label_Rep_List;
        private System.Windows.Forms.Label label_Class_List;
        private System.Windows.Forms.Label label_Current_Source_Image_File_Name;
        private System.Windows.Forms.TextBox textBox_Current_Source_Image_File_Name;
        private System.Windows.Forms.PictureBox pictureBox_Input_Image;
        private System.Windows.Forms.PictureBox pictureBox_Output_Image;
        private System.Windows.Forms.Button button_Save_Image;
        private System.Windows.Forms.Button button_Exit;
        private System.Windows.Forms.Label label_Cursor_Position;
        private System.Windows.Forms.TextBox textBox_Cursor_Position;
        private System.Windows.Forms.Label label_Pixel_Value;
        private System.Windows.Forms.TextBox textBox_RGB_Input;
        private System.Windows.Forms.TextBox textBox_RGB_Output;
        private System.Windows.Forms.Label label_RGB_From_To;
        private System.Windows.Forms.Button button_Process_All_Images;
        private System.Windows.Forms.GroupBox groupBox_Color_Deconvolution;
        private System.Windows.Forms.NumericUpDown numericUpDown_Otsu_Shift;
        private System.Windows.Forms.Label label_Otsu_Shift;
        private System.Windows.Forms.Button button_Process_Selected_Class;
        private System.Windows.Forms.CheckBox checkBox_Show_Mask;
        private System.Windows.Forms.NumericUpDown numericUpDown_Suppress_Background;
        private System.Windows.Forms.CheckBox checkBox_Suppress_Background;
        private System.Windows.Forms.NumericUpDown numericUpDown_Undesired_B;
        private System.Windows.Forms.NumericUpDown numericUpDown_Undesired_G;
        private System.Windows.Forms.NumericUpDown numericUpDown_Undesired_R;
        private System.Windows.Forms.NumericUpDown numericUpDown_Desired_B;
        private System.Windows.Forms.NumericUpDown numericUpDown_Desired_G;
        private System.Windows.Forms.NumericUpDown numericUpDown_Desired_R;
        private System.Windows.Forms.Label label_Undesired_Color;
        private System.Windows.Forms.Label label_Desired_Color;
        private System.Windows.Forms.NumericUpDown numericUpDown_Multiply;
        private System.Windows.Forms.Label label_Multiply;
        private System.Windows.Forms.NumericUpDown numericUpDown_Gaussian_sigma;
        private System.Windows.Forms.CheckBox checkBox_Smooth;
    }
}

