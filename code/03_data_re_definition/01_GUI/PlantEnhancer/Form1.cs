using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

using System.Runtime.InteropServices;
using System.Reflection;


namespace PlantEnhancer
{
    public partial class Form1 : Form
    {
        // bitmaps
        public static Bitmap Input_Bitmap;              // input bitmap - loaded from input image file
        /*public static*/ Bitmap Output_Bitmap;             // output bitmap, extracted from input bitmap 
        /*public static*/ Bitmap Output_Mask;             // output mask, extracted from input bitmap 

        // Parameters for extended layout management
        private Rectangle Form1_rect;
        private Rectangle pictureBox_Input_rect;
        private Rectangle pictureBox_Output_rect;
        private bool bResizeStarted;                    // true, if "resize" is in progress

        // misc.
        /*public static*/ Boolean Processing_Activated;     // true, if processing one or all classes is started
        Thread singleThread, multiThread;               //Process class in thread

        //----------------------------------------------------------------------------------------------
        public Form1()
        {
            InitializeComponent();

            // initialize params. of color deconvolution
            numericUpDown_Desired_R.Value = 48;
            numericUpDown_Desired_G.Value = 37;
            numericUpDown_Desired_B.Value = 56;
            numericUpDown_Undesired_R.Value = 17;
            numericUpDown_Undesired_G.Value = 68;
            numericUpDown_Undesired_B.Value = 88;
            numericUpDown_Multiply.Value = Convert.ToDecimal(1.5);

            // initialize the constraint, variables and controls
            ClearBitmaps(false);

            // Init. layout management extension
            Form1_rect = ClientRectangle;
            pictureBox_Input_rect = pictureBox_Input_Image.Bounds;
            pictureBox_Output_rect = pictureBox_Output_Image.Bounds;
            bResizeStarted = false;

            // append the 'Version' to App's window title
            Assembly MyAsm = Assembly.Load("PlantEnhancer");
            AssemblyName aName = MyAsm.GetName();
            Version ver = aName.Version;
            this.Text += "   [Version: " + ver + " ]";

            // misc.
            Processing_Activated = false;

        }

        //private void ChangeBackColor(Color color)
        //{
        //    BackColor = color;
        //}
        
        //----------------------------------------------------------------------------------------------
        // managing input/output folders

        private void button_Browse_Root_Input_Folder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.Description =
                "Select the root directory of input plant classes.";
            dlg.SelectedPath = textBox_Root_Input_FolderName.Text;
            dlg.ShowNewFolderButton = false;    // prevent the user for creating new folder
            DialogResult result = dlg.ShowDialog();
            if (result != DialogResult.OK)
                return;

            // accepted by the user - try to load folder and file names
            // REM: the path will be processed by textbox's handler
            textBox_Root_Input_FolderName.Text = dlg.SelectedPath;

        }
        private void button_Browse_Root_Output_Folder_MouseCaptureChanged(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.Description =
                "Select the root directory for outputs.";
            dlg.SelectedPath = textBox_Root_Output_FolderName.Text;
            dlg.ShowNewFolderButton = true;    // allow creating new folder
            DialogResult result = dlg.ShowDialog();
            if (result != DialogResult.OK)
                return;

            textBox_Root_Output_FolderName.Text = dlg.SelectedPath;
        }

        private void textBox_Root_Input_FolderName_TextChanged(object sender, EventArgs e)
        {
            if (textBox_Root_Input_FolderName.Text == "")
                return;

            DirectoryInfo dinfo = new DirectoryInfo(textBox_Root_Input_FolderName.Text);
            if (dinfo.Exists == false)
            {
                // directory does not exist
                textBox_Root_Input_FolderName.ForeColor = Color.Red;
                return;
            }
            DirectoryInfo[] directories = dinfo.GetDirectories();
            if (directories.Count() == 0)
            {
                // selected directory is empty
                textBox_Root_Input_FolderName.ForeColor = Color.Red;
                return;
            }

            // probably acceptable
            textBox_Root_Input_FolderName.ForeColor = Color.Black;
            listBox_Classes.Items.Clear();
            foreach (DirectoryInfo directory in directories)
            {
                if (directory.Name != "_Bars" && directory.Name != "_Plots" && directory.Name != "_Excels" && directory.Name != "_Saved_Lists")
                    listBox_Classes.Items.Add(directory.Name);
            }

            listBox_Classes.SelectedIndex = 0;

        }

        private void listBox_Classes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox_Classes.Items.Count == 0 || Processing_Activated == true)
                return;

            listBox_Reps.Items.Clear();

            string classname = (string)listBox_Classes.SelectedItem;
            string classfoldername = textBox_Root_Input_FolderName.Text + "\\" + classname;
            DirectoryInfo dinfo = new DirectoryInfo(classfoldername);
            if (dinfo.Exists == false)
                return;
            DirectoryInfo[] directories = dinfo.GetDirectories();
            if (directories.Count() == 0)
                return;
            foreach (DirectoryInfo directory in directories)
            {
                string rep_item = directory.Name;
                string imagesfoldername = textBox_Root_Input_FolderName.Text + "\\" + listBox_Classes.SelectedItem + "\\" + rep_item;
                while (rep_item.Length < 15)
                    rep_item += " ";
                int nbofimagefiles = 0;
                DirectoryInfo dinfo2 = new DirectoryInfo(imagesfoldername);
                if (dinfo2.Exists == true)
                {
                    FileInfo[] files = dinfo2.GetFiles();
                    nbofimagefiles = files.Count();
                }
                rep_item += Convert.ToString(nbofimagefiles);
                listBox_Reps.Items.Add(rep_item);

            }

            pictureBox_Input_Image.Invalidate();

            listBox_Reps.SelectedIndex = 0;
        }

        private void listBox_Reps_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox_Reps.Items.Count == 0 || Processing_Activated == true)
                return;

            listBox_Images.Items.Clear();
            string rep_item = (string)listBox_Reps.SelectedItem;
            rep_item = rep_item.Substring(0, rep_item.IndexOf(' '));
            string imagesfoldername = textBox_Root_Input_FolderName.Text + "\\" + listBox_Classes.SelectedItem + "\\" + rep_item;
            DirectoryInfo dinfo = new DirectoryInfo(imagesfoldername);
            if (dinfo.Exists == false)
                return;
            FileInfo[] files = dinfo.GetFiles();
            if (files.Count() == 0)
                return;
            int counter = 0;
            foreach (FileInfo file in files)
            {
                string filename = file.Name;
                counter++;
                string item = counter < 10 ? "0" : "";
                item += Convert.ToString(counter);
                item += "       ";
                if (counter < 100)
                    item += ' ';
                item += filename;
                listBox_Images.Items.Add(item);
            }

            listBox_Images.SelectedIndex = 0;
        }

        private void listBox_Images_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox_Images.Items.Count == 0 || Processing_Activated == true)
                return;

            string filename = (string)listBox_Images.SelectedItem;
            filename = filename.Remove(0, 10);
            textBox_Current_Source_Image_File_Name.Text = filename;
        }

        private void textBox_Current_Source_Image_File_Name_TextChanged(object sender, EventArgs e)
        {
            if (Processing_Activated == true)
                return;

            LoadandShowCurrentImage();
        }

        //----------------------------------------------------------------------------------------------
        // extended layout management

        private void Form1_ResizeBegin(object sender, EventArgs e)
        {
            bResizeStarted = true;
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            bResizeStarted = false;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            ResizeLayout();                 // continuous repaint of controls while resizing
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (bResizeStarted)
                return;                     //resize is in progress by dragging window's border
            Form1_ResizeEnd(sender, e);
        }

        void ResizeLayout()
        {
            int xdiff = ClientRectangle.Width - Form1_rect.Width;
            int ydiff = ClientRectangle.Height - Form1_rect.Height;

            AdjustControlPosition(pictureBox_Input_Image, pictureBox_Input_rect, 0, 0, xdiff / 2, ydiff);
            AdjustControlPosition(pictureBox_Output_Image, pictureBox_Output_rect, xdiff / 2, 0, xdiff / 2, ydiff);
        }

        void AdjustControlPosition(Control control, Rectangle previous_rect,
    int delta_x, int delta_y, int delta_width, int delta_height)
        {
            Rectangle rect = previous_rect;
            rect.X = rect.X + delta_x;
            rect.Y = rect.Y + delta_y;
            rect.Width = rect.Width + delta_width;
            rect.Height = rect.Height + delta_height;
            control.SetBounds(rect.X, rect.Y, rect.Width, rect.Height);
        }

        //----------------------------------------------------------------------------------------------

        private void button_Save_Image_Click(object sender, EventArgs e)
        {
            if (pictureBox_Output_Image.Image == null)
                return;

            // get the currently visualized content
            Bitmap bmp = (Bitmap)pictureBox_Output_Image.Image.Clone();
            if (bmp == null)
                return;

            // choose name and format for saving
            // REM: all rices will be saved separately, and the serial number (ID) will be appended to the file name
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter =
                "bitmap images (*.bmp)|*.bmp|jpeg images (*.jpg,*.jpeg)|*.jpg;*.jpeg|png images (*.png)|*.png|All files (*.*)|*.*";

            // the default format is ".jpg"
            dlg.FilterIndex = 2;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;             // cancelled by user

            ImageFormat format;
            string fileName = dlg.FileName;
            if (dlg.FilterIndex == 1)
            {
                format = ImageFormat.Bmp;
                if (!System.IO.Path.HasExtension(fileName) || System.IO.Path.GetExtension(fileName) != ".bmp")
                    fileName = fileName + ".bmp";
            }
            else if (dlg.FilterIndex == 2)
            {
                format = ImageFormat.Jpeg;
                if (!System.IO.Path.HasExtension(fileName) || System.IO.Path.GetExtension(fileName) != ".jpg")
                    fileName = fileName + ".jpg";
            }
            else
            {
                format = ImageFormat.Png;
                if (!System.IO.Path.HasExtension(fileName) || System.IO.Path.GetExtension(fileName) != ".png")
                    fileName = fileName + ".png";
            }

            ImageProcessing ip = new ImageProcessing();
            ip.SaveBitmap(bmp, fileName, format);
        }

        private void button_Exit_Click(object sender, EventArgs e)
        {
            Close();
        }


        //----------------------------------------------------------------------------------------------
        // mouse events

        private void pictureBox_Image_MouseMove(object sender, MouseEventArgs e)
        {
            // When processing class, return
            if (Processing_Activated)
                return;

            if (pictureBox_Input_Image.Image == null || Input_Bitmap == null)
            {
                ShowPixelData(-1, -1);
                return;
            }

            Point pos = new Point(e.X, e.Y);
            pos = WindowToImage(pos);

            // check, if outside?
            if (pos.X < 0 || pos.X > (Input_Bitmap.Width - 1) || pos.Y < 0 || pos.Y > (Input_Bitmap.Height - 1))
            {
                ShowPixelData(-1, -1);
                return;
            }
            ShowPixelData(pos.X, pos.Y);
        }
        private void pictureBox_Image_MouseLeave(object sender, EventArgs e)
        {
            ShowPixelData(-1, -1);
        }
        private void pictureBox_Image_MouseDown(object sender, MouseEventArgs e)
        {
            // When processing class, return
            if (Processing_Activated || pictureBox_Input_Image.Image == null || Input_Bitmap == null)
                return;

            Point pos = new Point(e.X, e.Y);
            pos = WindowToImage(pos);

            // check, if outside?
            if (pos.X < 0 || pos.X > (Input_Bitmap.Width - 1) || pos.Y < 0 || pos.Y > (Input_Bitmap.Height - 1))
                return;

            // get input pixel's color
            Color pixelvalue = Input_Bitmap.GetPixel(pos.X, pos.Y);

            // ...
        }

        private void button_Process_Selected_Class_Click(object sender, EventArgs e)
        {
            if (Input_Bitmap == null)
            {
                MessageBox.Show(" Input folder name must be given!");
                return;
            }

            // check, if the output folder is given, and create it if does not exist
            if (textBox_Root_Output_FolderName.Text == "")
            {
                MessageBox.Show(" Output folder name must be given!");
                return;
            }

            Cursor = Cursors.WaitCursor;
            Processing_Activated = true;

            string output_root_foldername = textBox_Root_Output_FolderName.Text;
            if (!Directory.Exists(output_root_foldername))
                Directory.CreateDirectory(output_root_foldername);

            // process the images of selected class
            if (listBox_Classes.SelectedIndex != -1)
            {
                string classname = (string)listBox_Classes.SelectedItem;
                string[] pclassnames = new string[1];
                pclassnames[0] = classname;

                ProcessMultipleClassWorker worker = new ProcessMultipleClassWorker(pclassnames, false);
                multiThread = new Thread(worker.Run);
                worker.ThreadDone += ProcessMultiClassThreadDone;
                multiThread.Start(this);
            }

            Cursor = Cursors.Default;
            Processing_Activated = false;

        }

        private void button_Process_All_Images_Click(object sender, EventArgs e)
        {
            if (Input_Bitmap == null)
            {
                MessageBox.Show(" Input folder name must be given!");
                return;
            }

            // check, if the output folder is given, and create it if does not exist
            if (textBox_Root_Output_FolderName.Text == "")
            {
                MessageBox.Show(" Output folder name must be given!");
                return;
            }

            Cursor = Cursors.WaitCursor;
            Processing_Activated = true;

            string output_root_foldername = textBox_Root_Output_FolderName.Text;
            if (!Directory.Exists(output_root_foldername))
                Directory.CreateDirectory(output_root_foldername);

            // process all images of all classes
            string classname = "";
            int nbofclasses = listBox_Classes.Items.Count;
            string[] pclassnames = new string[nbofclasses];
            for (int index = 0; index < nbofclasses; index++)
            {
                classname = listBox_Classes.Items[index].ToString();
                pclassnames[index] = classname;
            }

            ProcessMultipleClassWorker worker = new ProcessMultipleClassWorker(pclassnames, true);
            multiThread = new Thread(worker.Run);
            worker.ThreadDone += ProcessMultiClassThreadDone;
            multiThread.Start(this);

            Cursor = Cursors.Default;
            Processing_Activated = false;
        }

        private void Process_Selected_Image(Bitmap bitmap, Boolean bShowOutputBitmap)
        {
            if (bitmap == null)
                return;

            //ClearBitmaps(true);

            ImageProcessing ip = new ImageProcessing();

            int red = Convert.ToInt16(numericUpDown_Desired_R.Value);
            int green = Convert.ToInt16(numericUpDown_Desired_G.Value);
            int blue = Convert.ToInt16(numericUpDown_Desired_B.Value);
            Color desired_color = Color.FromArgb(red, green, blue);
            red = Convert.ToInt16(numericUpDown_Undesired_R.Value);
            green = Convert.ToInt16(numericUpDown_Undesired_G.Value);
            blue = Convert.ToInt16(numericUpDown_Undesired_B.Value);
            Color undesired_color = Color.FromArgb(red, green, blue);
            double mult = Convert.ToDouble(numericUpDown_Multiply.Value);

            Bitmap tmp_bitmap = ip.Color_Deconvolution(bitmap, desired_color, undesired_color, mult);

            if (checkBox_Smooth.Checked == true)
            {
                double sigma = Convert.ToDouble(numericUpDown_Gaussian_sigma.Value);
                ip.Smooth_Color_Bitmap(tmp_bitmap, sigma);
            }

            int Otsu_Shift = Convert.ToUInt16(numericUpDown_Otsu_Shift.Value);
            double Suppress_Bkg = checkBox_Suppress_Background.Checked ? Convert.ToDouble(numericUpDown_Suppress_Background.Value) : 1.0;
            Output_Mask = new Bitmap(Input_Bitmap.Width, Input_Bitmap.Height, PixelFormat.Format24bppRgb);
            ip.Remove_Background(tmp_bitmap, Output_Mask, Otsu_Shift, Suppress_Bkg);
            Output_Bitmap = ip.ImageSharpen(tmp_bitmap);
            tmp_bitmap.Dispose();

            if (bShowOutputBitmap == true)
            {
                if (checkBox_Show_Mask.Checked)
                    pictureBox_Output_Image.Image = Output_Mask;
                else
                    pictureBox_Output_Image.Image = Output_Bitmap;
                //pictureBox_Output_Image.Refresh();
                //Invoke in UI thread
                pictureBox_Output_Image.Invoke((MethodInvoker)delegate
                {
                    pictureBox_Output_Image.Refresh();
                });
            }

        }

        // Multiple class processing worker thread
        class ProcessMultipleClassWorker
        {
            // Switch to your favourite Action<T> or Func<T>
            string[] classNames;
            Boolean bSelectClassInListBox;

            public event EventHandler ThreadDone;

            public ProcessMultipleClassWorker(string[] pClassNames, Boolean bSelectClass)
            {
                this.classNames = new string[pClassNames.Length];
                Array.Copy(pClassNames, classNames, pClassNames.Length);
                bSelectClassInListBox = bSelectClass;
            }

            public void Run(object sender)
            {
                if (!(sender is Form1 && sender.GetType() == typeof(Form1)))
                    return;
                Form1 fm1 = (Form1)sender;

                // Execute the color correction
                string classname;
                string output_root_foldername = fm1.textBox_Root_Output_FolderName.Text;

                // do not show the generated output bitmaps
                fm1.pictureBox_Output_Image.Invoke((MethodInvoker)delegate
                {
                    fm1.pictureBox_Output_Image.Image = null;
                });

                for (int index = 0; index < classNames.Length; index++)
                {
                    classname = (string)classNames[index];

                    fm1.Processing_Activated = true;
                    if (bSelectClassInListBox == true)
                    {
                        fm1.listBox_Classes.Invoke((MethodInvoker)delegate
                        {
                            fm1.listBox_Classes.SelectedIndex = index;
                        });
                    }

                    // check, if the class subdirectory exists in output folder- create it if missing
                    string output_class_folder_name = output_root_foldername + "\\" + classname;
                    if (!Directory.Exists(output_class_folder_name))
                        Directory.CreateDirectory(output_class_folder_name);

                    // fill the list of Reps
                    //Invoke in UI thread
                    fm1.listBox_Reps.Invoke((MethodInvoker)delegate {
                        fm1.listBox_Reps.Items.Clear();
                    });

                    string classfoldername = fm1.textBox_Root_Input_FolderName.Text + "\\" + classname;
                    DirectoryInfo dinfo = new DirectoryInfo(classfoldername);
                    if (dinfo.Exists == false)
                        return;
                    DirectoryInfo[] directories = dinfo.GetDirectories();
                    if (directories.Count() == 0)
                        return;
                    int idx = 0;
                    foreach (DirectoryInfo directory in directories)
                    {
                        string rep_item = directory.Name;
                        string imagesfoldername = fm1.textBox_Root_Input_FolderName.Text + "\\" + classname + "\\" + rep_item + "\\";
                        while (rep_item.Length < 15)
                            rep_item += " ";
                        int nbofimagefiles = 0;
                        DirectoryInfo dinfo2 = new DirectoryInfo(imagesfoldername);
                        if (dinfo2.Exists == true)
                        {
                            FileInfo[] files = dinfo2.GetFiles();
                            nbofimagefiles = files.Count();
                        }
                        rep_item += Convert.ToString(nbofimagefiles);
                        //Invoke in UI thread
                        fm1.listBox_Reps.Invoke((MethodInvoker)delegate
                        {
                            fm1.listBox_Reps.Items.Add(rep_item);
                        });
                        idx++;
                    }
                    //Invoke in UI thread
                    fm1.listBox_Reps.Invoke((MethodInvoker)delegate
                    {
                        fm1.listBox_Reps.Refresh();
                    });

                    // process all reps
                    int nbofreps = fm1.listBox_Reps.Items.Count;
                    if (nbofreps == 0)
                        return;

                    for (int repindex = 0; repindex < nbofreps; repindex++)
                    {
                        //Invoke in UI thread
                        fm1.listBox_Reps.Invoke((MethodInvoker)delegate
                        {
                            fm1.listBox_Reps.SelectedIndex = repindex;
                            fm1.listBox_Reps.Refresh();
                        });

                        // fill the list of Images
                        //Invoke in UI thread
                        fm1.listBox_Images.Invoke((MethodInvoker)delegate
                        {
                            fm1.listBox_Images.Items.Clear();
                        });

                        string rep_item = "";

                        fm1.listBox_Reps.Invoke((MethodInvoker)delegate
                        {
                            fm1.listBox_Reps.SelectedIndex = repindex;
                            fm1.listBox_Reps.Refresh();
                            rep_item = (string)fm1.listBox_Reps.SelectedItem;
                        });

                        rep_item = rep_item.Substring(0, rep_item.IndexOf(' '));

                        // check, if the rep subdirectory exists in currently processed output class folder- create it if missing
                        string output_rep_folder_name = output_class_folder_name + "\\" + rep_item;
                        if (!Directory.Exists(output_rep_folder_name))
                            Directory.CreateDirectory(output_rep_folder_name);

                        string imagesfoldername = fm1.textBox_Root_Input_FolderName.Text + "\\" + classname + "\\" + rep_item + "\\";
                        DirectoryInfo d2info = new DirectoryInfo(imagesfoldername);
                        if (d2info.Exists == false)
                            return;
                        FileInfo[] files = d2info.GetFiles();
                        if (files.Count() == 0)
                            return;
                        int counter = 0;


                        foreach (FileInfo file in files)
                        {
                            string filename = file.Name;
                            counter++;
                            string item = counter < 10 ? "0" : "";
                            item += Convert.ToString(counter);
                            item += "       ";
                            if (counter < 100)
                                item += ' ';
                            item += filename;
                            //Invoke in UI thread
                            fm1.listBox_Images.Invoke((MethodInvoker)delegate
                            {
                                fm1.listBox_Images.Items.Add(item);
                            });
                        }
                        //Invoke in UI thread
                        fm1.listBox_Images.Invoke((MethodInvoker)delegate
                        {
                            fm1.listBox_Images.Refresh();
                        });

                        //----------------------------------------------------------------------
                        // process all images of current Rep
                        for (int imageindex = 0; imageindex < counter; imageindex++)
                        {
                            //Invoke in UI thread
                            string filename = "";
                            fm1.listBox_Images.Invoke((MethodInvoker)delegate
                            {
                                fm1.listBox_Images.SelectedIndex = imageindex;
                                filename = (string)fm1.listBox_Images.SelectedItem;
                            });

                            filename = filename.Remove(0, 10);
                            //Invoke in UI thread
                            fm1.textBox_Current_Source_Image_File_Name.Invoke((MethodInvoker)delegate
                            {
                                fm1.textBox_Current_Source_Image_File_Name.Text = filename;
                                fm1.textBox_Current_Source_Image_File_Name.Refresh();
                            });


                            // load and show current image
                            fm1.LoadandShowCurrentImage();

                            //fm1.Process_Selected_Image(Input_Bitmap, false);
                            fm1.Process_Selected_Image(Input_Bitmap, true);

                            // save the output bitmap
                            string output_file_name = output_rep_folder_name + "\\" + filename;
                            fm1.Output_Bitmap.Save(output_file_name);
;
                        }

                    }

                }

                if (ThreadDone != null)
                    ThreadDone(this, EventArgs.Empty);
            }
        }


        //Called when multiple class processing is done
        void ProcessMultiClassThreadDone(object sender, EventArgs e)
        {
            // create the overall growth rate bars
            this.Invoke((MethodInvoker)delegate
            {
                Processing_Activated = false;
                Cursor = Cursors.Default;
                pictureBox_Input_Image.Invalidate();
                pictureBox_Output_Image.Invalidate();
                multiThread = null;
            });
        }


        //----------------------------------------------------------------------------------------------
        // color deconvolution


        private void Color_Deconvolution_Parameter_Changed(object sender, EventArgs e)
        {
            if (Input_Bitmap == null || Processing_Activated==true)
                return;

            Process_Selected_Image(Input_Bitmap, true);
        }

        //----------------------------------------------------------------------------------------------
        // misc.
        private void ClearBitmaps(Boolean bClearOnlyOutput)
        {
            if (!bClearOnlyOutput)
            {
                if (pictureBox_Input_Image.Image != null)
                {
                    pictureBox_Input_Image.Image.Dispose();
                    pictureBox_Input_Image.Image = null;
                }
                if (Input_Bitmap != null)
                {
                    Input_Bitmap.Dispose();
                    Input_Bitmap = null;
                }
            }

            if (pictureBox_Output_Image.Image != null)
            {
                pictureBox_Output_Image.Image.Dispose();
                pictureBox_Output_Image.Image = null;
            }
            if (Output_Bitmap != null)
            {
                Output_Bitmap.Dispose();
                Output_Bitmap = null;
            }
            if (Output_Mask != null)
            {
                Output_Mask.Dispose();
                Output_Mask = null;
            }
        }


        private void LoadandShowCurrentImage()
        {
            if (textBox_Root_Input_FolderName.Text == "")
                return;
            if (listBox_Classes.Items.Count == 0)
                return;
            if (listBox_Reps.Items.Count == 0)
                return;
            if (textBox_Current_Source_Image_File_Name.Text == "")
                return;

            //pictureBox_Input_Image.Image = null;
            //if (Input_Bitmap != null)
            //{
            //    Input_Bitmap.Dispose();
            //    Input_Bitmap = null;
            //}
            string filename = textBox_Root_Input_FolderName.Text;

            //Invoke in UI thread
            listBox_Classes.Invoke((MethodInvoker)delegate
            {
                filename += "\\" + listBox_Classes.SelectedItem;
            });
            string rep_item = "";
            listBox_Reps.Invoke((MethodInvoker)delegate
            {
                rep_item = (string)listBox_Reps.SelectedItem;
            });
            if (rep_item == null)
                return;

            rep_item = rep_item.Substring(0, rep_item.IndexOf(' '));
            filename += "\\" + rep_item;
            filename += "\\" + textBox_Current_Source_Image_File_Name.Text;
            try
            {
                Bitmap tmp_bitmap = new Bitmap(filename); // exception, if unsuccessful;
                Input_Bitmap = new Bitmap(tmp_bitmap);
                tmp_bitmap.Dispose();

            }
            //catch (Exception ex)
            catch (Exception)
            {
                MessageBox.Show(" Image file format is not acceptable!");
                return;
            }

            //pictureBox_Input_Image.Image = Input_Bitmap;

            if (Processing_Activated==false)
                Process_Selected_Image(Input_Bitmap, true);

            pictureBox_Input_Image.Image = Input_Bitmap;

            //Invoke in UI thread
            pictureBox_Input_Image.Invoke((MethodInvoker)delegate
            {
                pictureBox_Input_Image.Refresh();
            });

            // do not show pixel data
            textBox_Cursor_Position.Invoke((MethodInvoker)delegate
            {
                textBox_Cursor_Position.Text = "";
            });

            textBox_RGB_Input.Invoke((MethodInvoker)delegate
            {
                textBox_RGB_Input.Text = "";
            });

            textBox_RGB_Output.Invoke((MethodInvoker)delegate
            {
                textBox_RGB_Output.Text = "";
            });
        }

        private Point WindowToImage(Point pos)
        {
            Point pt = new Point(0, 0);
            if (pictureBox_Input_Image.Image == null)
                return pt;

            // transform the positions from windows's coordinate system to
            // the coordinate system of image
            double ratio_image = (double)Input_Bitmap.Width / (double)Input_Bitmap.Height;
            double ratio_PictureBox = (double)pictureBox_Input_Image.Width / (double)pictureBox_Input_Image.Height;
            double ratio;
            double shiftx, shifty;
            if (ratio_image > ratio_PictureBox)
            {
                // the pictureBox's height is bigger than image's one
                ratio = (double)pictureBox_Input_Image.Width / (double)Input_Bitmap.Width;
                shiftx = 0;
                shifty = (double)(pictureBox_Input_Image.Height - ratio * (double)Input_Bitmap.Height) / 2.0;
            }
            else
            {
                // the pictureBox's width is bigger than image's one
                ratio = (double)pictureBox_Input_Image.Height / (double)Input_Bitmap.Height;
                shiftx = (double)(pictureBox_Input_Image.Width - ratio * (double)Input_Bitmap.Width) / 2.0;
                shifty = 0;
            }
            pt.X = (int)((double)(pos.X - shiftx) / ratio + 0.5);
            pt.Y = (int)((double)(pos.Y - shifty) / ratio + 0.5);

            return pt;
        }

        private void ShowPixelData(int xpos, int ypos)
        {
            textBox_Cursor_Position.Text = (Processing_Activated==true || xpos == -1) ? "" : "( " + Convert.ToString(xpos) + ", " + Convert.ToString(ypos) + " )";
            textBox_Cursor_Position.Refresh();

            if (Processing_Activated==true || xpos == -1)
            {
                textBox_RGB_Input.Text = "";
                textBox_RGB_Output.Text = "";
            }
            else
            {
                Color pixelvalue = new Color();
                string str = "";
                if (Input_Bitmap != null)
                {
                    pixelvalue = Input_Bitmap.GetPixel(xpos, ypos);
                    str = "( " + Convert.ToString(pixelvalue.R) + " , " + Convert.ToString(pixelvalue.G) + " , " + Convert.ToString(pixelvalue.B) + " )";
                    textBox_RGB_Input.Text = str;
                }
                if (Output_Bitmap != null)
                {
                    pixelvalue = Output_Bitmap.GetPixel(xpos, ypos);
                    str = "( " + Convert.ToString(pixelvalue.R) + " , " + Convert.ToString(pixelvalue.G) + " , " + Convert.ToString(pixelvalue.B) + " )";
                    textBox_RGB_Output.Text = str;
                }

            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------
        private void Form1_Load(object sender, EventArgs e)
        {
            typeof(PictureBox).InvokeMember("DoubleBuffered", BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic, null, pictureBox_Input_Image, new object[] { true });
            typeof(PictureBox).InvokeMember("DoubleBuffered", BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic, null, pictureBox_Output_Image, new object[] { true });

            //Register delete of closing Form
            this.FormClosed += new FormClosedEventHandler(Form1_FormClosed);

            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.DoubleBuffer,
                true);
        }

        void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            // When closing form, abort the current processing thread
            if (singleThread != null)
            {
                singleThread.Abort();
                singleThread = null;
            }

            if (multiThread != null)
            {
                multiThread.Abort();
                multiThread = null;
            }

        }


    }
}
