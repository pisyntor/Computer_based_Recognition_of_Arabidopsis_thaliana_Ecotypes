using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using System.Drawing;
using System.Drawing.Imaging;


namespace PlantEnhancer
{


    public class ImageProcessing
    {

//------------------------------------------------------------------------------
        public Bitmap Color_Deconvolution(Bitmap Input_Bitmap, Color desired_color, Color undesired_color, double mult)
        {
            // check the parameters
            if (Input_Bitmap == null)
                return null;
            if (Input_Bitmap.PixelFormat != PixelFormat.Format8bppIndexed &&
                    Input_Bitmap.PixelFormat != PixelFormat.Format24bppRgb &&
                    Input_Bitmap.PixelFormat != PixelFormat.Format32bppArgb &&
                    Input_Bitmap.PixelFormat != PixelFormat.Format32bppRgb)
                return null;

            // create the output bitmap (24bpp RGB)
            Bitmap Output_Bitmap = new Bitmap(Input_Bitmap.Width, Input_Bitmap.Height, PixelFormat.Format24bppRgb);


            // get the input pixels
            Rectangle rect = new Rectangle(0, 0, Input_Bitmap.Width, Input_Bitmap.Height);
            BitmapData input_data = Input_Bitmap.LockBits(rect, ImageLockMode.ReadOnly, Input_Bitmap.PixelFormat);
            IntPtr ptr1 = input_data.Scan0;
            int bytesbpp = 0;    // bytes_per_pixel
            switch (Input_Bitmap.PixelFormat)
            {
                case PixelFormat.Format8bppIndexed:
                    bytesbpp = 1;
                    break;
                case PixelFormat.Format24bppRgb:
                    bytesbpp = 3;
                    break;
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppRgb:
                    bytesbpp = 4;
                    break;
            }
            int inputstride = (input_data.Width * bytesbpp + 3) / 4 * 4;
            int nbofinputbytes = inputstride * input_data.Height;
            byte[] inp = new byte[nbofinputbytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr1, inp, 0, nbofinputbytes);
            ColorPalette palette = Input_Bitmap.Palette;

            // get the output buffer
            BitmapData output_data = Output_Bitmap.LockBits(rect, ImageLockMode.ReadWrite, Output_Bitmap.PixelFormat);
            IntPtr ptr2 = output_data.Scan0;
            int outputstride = (output_data.Width * 3 + 3) / 4 * 4;
            int nbofoutputbytes = outputstride * output_data.Height;
            byte[] outp = new byte[nbofoutputbytes];

            // create twist matrix for color deconvolution
            byte bkg_R = 0;             // background color
            byte bkg_G = 0;
            byte bkg_B = 0;
            double[,] twistMatrix = new double[3,4];
            double p1, p2, p3;          // background color's RGB components, respectively
            double d1, d2, d3;          // desired color's RGB components (diff vector from background), respectively
            double u1, u2, u3;          // undesired color's RGB components (diff vector from background), respectively
            double n1, n2, n3;          // normal (d x u)
            double denom, nom;          // 

            //	assign bkg and difference vectors 
            p1 = (double)bkg_R; p2 = (double)bkg_G; p3 = (double)bkg_B;
            d1 = p1 - (double)undesired_color.R; d2 = p1 - (double)undesired_color.G; d3 = p1 - (double)undesired_color.B;
            u1 = p1 - (double)desired_color.R; u2 = p1 - (double)desired_color.G; u3 = p1 - (double)desired_color.B;

            //	calculate normal for new RGB basis
            n1 = d3 * p2 - d2 * p3 - d3 * u2 + p3 * u2 + d2 * u3 - p2 * u3;
            n2 = d1 * p3 - d3 * p1 + d3 * u1 - p3 * u1 - d1 * u3 + p1 * u3;
            n3 = d2 * p1 - d1 * p2 - d2 * u1 + p2 * u1 + d1 * u2 - p1 * u2;

            //	d,u,n define basis
            denom = d3 * n2 * u1 - d2 * n3 * u1 - d3 * n1 * u2 + d1 * n3 * u2 + d2 * n1 * u3 - d1 * n2 * u3;
            if (denom == 0.0)
            {
                // invalid parameters - return with empty output
                Input_Bitmap.UnlockBits(input_data);
                Output_Bitmap.UnlockBits(output_data);
                return Output_Bitmap;
            }
            nom = d3 * n2 * p1 - d2 * n3 * p1 - d3 * n1 * p2 + d1 * n3 * p2 + d2 * n1 * p3 - d1 * n2 * p3;

            //	create twist matrix
            twistMatrix[0,0] = (d2 * n1 * u3 - d1 * n2 * u3 - d3 * n1 * u2 + d1 * n3 * u2) / denom;
            twistMatrix[0,1] = (d3 * n1 - d1 * n3) * u1 / denom;
            twistMatrix[0,2] = (d1 * n2 - d2 * n1) * u1 / denom;
            twistMatrix[0,3] = (nom * u1) / denom;
            twistMatrix[1,0] = (d2 * n3 - d3 * n2) * u2 / denom;
            twistMatrix[1,1] = (d2 * n1 * u3 - d1 * n2 * u3 + d3 * n2 * u1 - d2 * n3 * u1) / denom;
            twistMatrix[1,2] = (d1 * n2 - d2 * n1) * u2 / denom;
            twistMatrix[1,3] = (nom * u2) / denom;
            twistMatrix[2,0] = (d2 * n3 - d3 * n2) * u3 / denom;
            twistMatrix[2,1] = (d3 * n1 - d1 * n3) * u3 / denom;
            twistMatrix[2,2] = (d3 * n2 * u1 - d2 * n3 * u1 - d3 * n1 * u2 + d1 * n3 * u2) / denom;
            twistMatrix[2,3] = (nom * u3) / denom;


            byte B, G, R;
            for (int jj = 0; jj < Input_Bitmap.Height; jj++)
            {
                for (int ii = 0; ii < Input_Bitmap.Width; ii++)
                {
                    if (bytesbpp==1)
                    {
                        int index = inp[ii + jj * inputstride];
                        Color color = palette.Entries[index];
                        R = color.R;
                        G = color.G;
                        B = color.B;
                    }
                    else
                    {
                        B = inp[bytesbpp * ii + 0 + jj * inputstride];
                        G = inp[bytesbpp * ii + 1 + jj * inputstride];
                        R = inp[bytesbpp * ii + 2 + jj * inputstride];
                    }

                    double c1 = (double)B;
					double c2 = (double)G;
					double c3 = (double)R;
					double c1_out = twistMatrix[0,0] * c1 + twistMatrix[0,1] * c2 + twistMatrix[0,2] * c3 + twistMatrix[0,3];
                    double c2_out = twistMatrix[1,0] * c1 + twistMatrix[1,1] * c2 + twistMatrix[1,2] * c3 + twistMatrix[1,3];
                    double c3_out = twistMatrix[2,0] * c1 + twistMatrix[2,1] * c2 + twistMatrix[2,2] * c3 + twistMatrix[2,3];

                    // c1_out *= mult;
                    c2_out *= mult;
                    // c3_out *= mult;

                    c1_out = Math.Max(0, c1_out); c2_out = Math.Max(0, c2_out); c3_out = Math.Max(0, c3_out);
                    c1_out = Math.Min(255, c1_out); c2_out = Math.Min(255, c2_out); c3_out = Math.Min(255, c3_out);

                    outp[3 * ii + jj * outputstride] = (byte)c1_out;
                    outp[3 * ii + 1 + jj * outputstride] = (byte)c2_out;
                    outp[3 * ii + 2 + jj * outputstride] = (byte)c3_out;
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(outp, 0, ptr2, nbofoutputbytes);
            Input_Bitmap.UnlockBits(input_data);
            Output_Bitmap.UnlockBits(output_data);

            return Output_Bitmap;
        }


        //------------------------------------------------------------------------------

        public void Smooth_Color_Bitmap( Bitmap IO_bitmap, double sigma)
        {
            // Only 24bpp RGB bitmap is accepted.
            if (IO_bitmap == null || IO_bitmap.PixelFormat != PixelFormat.Format24bppRgb)
                return;

            // get the input pixels (that will be overwritten after smoothing)
            Rectangle rect1 = new Rectangle(0, 0, IO_bitmap.Width, IO_bitmap.Height);
            BitmapData input_data = IO_bitmap.LockBits(rect1, ImageLockMode.ReadWrite, IO_bitmap.PixelFormat);
            IntPtr ptr1 = input_data.Scan0;
            int width = IO_bitmap.Width;
            int height = IO_bitmap.Height;
            int bytesbpp = 3;   // (as it is 24bpp RGB)
            int inputstride = (input_data.Width * bytesbpp + 3) / 4 * 4;
            int nbofinputbytes = inputstride * height;
            byte[] inp = new byte[nbofinputbytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr1, inp, 0, nbofinputbytes);

            // create buffer for output
            byte[] outp = new byte[nbofinputbytes];

            // create 1-dimensional gaussian smoothing kernel
            int windowsize = 1 + 2 * (int)(2.5 * sigma + 0.5);
            int center = windowsize / 2;
            double sum = 0.0;
            double x, fx;
            double[] kernel = new double[windowsize];
            for (int ii = 0; ii < windowsize; ii++)
            {
                x = (double)(ii - center);
                fx = Math.Pow(2.71828, -0.5 * x * x / (sigma * sigma)) / (sigma * Math.Sqrt(6.2831853));
                kernel[ii] = fx;
                sum += fx;
            }
            for (int ii = 0; ii < windowsize; ii++)
                kernel[ii] /= sum;

            // create temporary array
            double[] tmp = new double[bytesbpp * width * height];

            // blur in x-direction
            double dotR, dotG, dotB;
            byte pixelvalue;
            for (int jj = 0; jj < height; jj++)
            {
                for (int ii = 0; ii < width; ii++)
                {
                    dotR = 0.0; dotG = 0.0; dotB = 0.0;
                    sum = 0.0;
                    for (int cc = (-center); cc <= center; cc++)
                    {
                        if (((ii + cc) >= 0) && ((ii + cc) < width))
                        {
                            pixelvalue = inp[jj * inputstride + bytesbpp * (ii + cc)];
                            dotB += (double)pixelvalue * kernel[center + cc];

                            pixelvalue = inp[jj * inputstride + bytesbpp * (ii + cc) + 1];
                            dotG += (double)pixelvalue * kernel[center + cc];

                            pixelvalue = inp[jj * inputstride + bytesbpp * (ii + cc) + 2];
                            dotR += (double)pixelvalue * kernel[center + cc];

                            sum += kernel[center + cc];

                        }
                    }
                    tmp[bytesbpp * (jj * width + ii)] = dotB / sum;
                    tmp[bytesbpp * (jj * width + ii) + 1] = dotG / sum;
                    tmp[bytesbpp * (jj * width + ii) + 2] = dotR / sum;
                }
            }

            // blur in y-direction
            for (int ii = 0; ii < width; ii++)
            {
                for (int jj = 0; jj < height; jj++)
                {
                    dotR = 0.0; dotG = 0.0; dotB = 0.0;
                    sum = 0.0;
                    for (int rr = (-center); rr <= center; rr++)
                    {
                        if (((jj + rr) >= 0) && ((jj + rr) < height))
                        {
                            dotB += tmp[bytesbpp * ((jj + rr) * width + ii)] * kernel[center + rr];
                            dotG += tmp[bytesbpp * ((jj + rr) * width + ii) + 1] * kernel[center + rr];
                            dotR += tmp[bytesbpp * ((jj + rr) * width + ii) + 2] * kernel[center + rr];
                            sum += kernel[center + rr];
                        }
                    }
                    outp[jj * inputstride + bytesbpp * ii] = (byte)(dotB / sum + 0.5);
                    outp[jj * inputstride + bytesbpp * ii + 1] = (byte)(dotG / sum + 0.5);
                    outp[jj * inputstride + bytesbpp * ii + 2] = (byte)(dotR / sum + 0.5);
                }
            }


            // copy the processed pixels back to the input bitmap
            System.Runtime.InteropServices.Marshal.Copy(outp, 0, ptr1, nbofinputbytes);
            IO_bitmap.UnlockBits(input_data);

        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        // ### remove/decrease background ###

        public void Remove_Background(Bitmap IO_bitmap, Bitmap Plant_Mask, int Otsu_Shift, double Suppress_Bkg)
        {
            // Only 24bpp RGB bitmap is accepted.
            if (IO_bitmap == null || IO_bitmap.PixelFormat != PixelFormat.Format24bppRgb)
                return;

            // get the input pixels (that will be overwritten after processing)
            Rectangle rect1 = new Rectangle(0, 0, IO_bitmap.Width, IO_bitmap.Height);
            BitmapData input_data = IO_bitmap.LockBits(rect1, ImageLockMode.ReadWrite, IO_bitmap.PixelFormat);
            IntPtr ptr1 = input_data.Scan0;
            int width = IO_bitmap.Width;
            int height = IO_bitmap.Height;
            int bytesbpp = 3;   // (as it is 24bpp RGB)
            int inputstride = (input_data.Width * bytesbpp + 3) / 4 * 4;
            int nbofinputbytes = inputstride * height;
            byte[] inp = new byte[nbofinputbytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr1, inp, 0, nbofinputbytes);

            // Set plant mask to zero
            byte[] maskp = new byte[nbofinputbytes];
            for (int i = 0; i < nbofinputbytes; i++)
                maskp[i] = 0;

            // get the histogram of green channel
            double[] pHist = new double[256];
            for (int ii = 0; ii < 256; ii++)
                pHist[ii] = 0;
            for (int jj = 0; jj < IO_bitmap.Height; jj++)
            {
                for (int ii = 0; ii < IO_bitmap.Width; ii++)
                {
                    int index = inp[bytesbpp * ii + 1 + jj * inputstride];
                    pHist[index] += 1.0;
                }
            }

            double img_size_d = (double)(IO_bitmap.Width * IO_bitmap.Height);

            // destroy the undesired low-level band
            for (int ii = 0; ii < Otsu_Shift; ii++)
            {
                img_size_d -= (int)pHist[ii];
                pHist[ii] = 0;
            }

            // normalize the histogram
            for (int ii = 0; ii < 256; ii++)
                pHist[ii] /= img_size_d;

            // produce the threshold value for green channel
            double ut = 0;
            for (int ii = 0; ii < 256; ii++)
                ut += ii * pHist[ii];
            int max_k = 0;
            int max_sigma_k = 0;
            for (int k = 0; k < 256; ++k)
            {
                double wk = 0;
                for (int ii = 0; ii <= k; ++ii)
                    wk += pHist[ii];
                double uk = 0;
                for (int ii = 0; ii <= k; ++ii)
                    uk += ii * pHist[ii];
                double sigma_k = 0;
                if (wk != 0 && wk != -1)
                    sigma_k = ((ut * wk - uk) * (ut * wk - uk)) / (wk * (1 - wk));
                if (sigma_k > max_sigma_k)
                {
                    max_k = k;
                    max_sigma_k = (int)sigma_k;
                }
            }

            // clear the background pixels (having smaller green component, than the Otsu threshold)
            for (int jj = 0; jj < IO_bitmap.Height; jj++)
            {
                for (int ii = 0; ii < IO_bitmap.Width; ii++)
                {
                    if (inp[bytesbpp * ii + 1 + jj * inputstride]< max_k)
                    {
                        inp[bytesbpp * ii + jj * inputstride] = (byte)((double)inp[bytesbpp * ii + jj * inputstride] / Suppress_Bkg);
                        inp[bytesbpp * ii + 1 + jj * inputstride] = (byte)((double)inp[bytesbpp * ii + 1 + jj * inputstride] / Suppress_Bkg);
                        inp[bytesbpp * ii + 2 + jj * inputstride] = (byte)((double)inp[bytesbpp * ii + 2 + jj * inputstride] / Suppress_Bkg);
                    }
                    else
                    {
                        maskp[bytesbpp * ii + jj * inputstride] = 255;
                        maskp[bytesbpp * ii + 1 + jj * inputstride] = 255;
                        maskp[bytesbpp * ii + 2 + jj * inputstride] = 255;
                    }
                }
            }
            // copy the processed pixels back to the input bitmap
            System.Runtime.InteropServices.Marshal.Copy(inp, 0, ptr1, nbofinputbytes);
            IO_bitmap.UnlockBits(input_data);
            // copy the mask pixels to the input plant mask
            BitmapData plant_mask_data = Plant_Mask.LockBits(rect1, ImageLockMode.ReadWrite, Plant_Mask.PixelFormat);
            IntPtr ptr_mask = plant_mask_data.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(maskp, 0, ptr_mask, nbofinputbytes);
            Plant_Mask.UnlockBits(plant_mask_data);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void SaveBitmap(Bitmap Input_Bitmap, string FileName, ImageFormat format)
        {
            if (Input_Bitmap == null)
                return;

            // (Due to bitmap saving behaviour, deep copy must be created for saving.)
            Bitmap tmp_bmp = new Bitmap(Input_Bitmap);

            Image image = new Bitmap(tmp_bmp);
            image.Save(FileName, format);

            tmp_bmp.Dispose();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        // ### format conversion ###

        public bool Convert_32bpp_To_RGB_24bpp(Bitmap Bitmap_32bpp, Bitmap Bitmap_24bpp)
        {
            // check the parameters
            if (Bitmap_32bpp == null || Bitmap_24bpp == null ||
                Bitmap_32bpp.PixelFormat != PixelFormat.Format32bppArgb || Bitmap_24bpp.PixelFormat != PixelFormat.Format24bppRgb ||
                Bitmap_32bpp.Width != Bitmap_24bpp.Width || Bitmap_32bpp.Height != Bitmap_24bpp.Height)
                return false;

            // get the input pixels
            Rectangle rect = new Rectangle(0, 0, Bitmap_32bpp.Width, Bitmap_32bpp.Height);
            BitmapData input_data = Bitmap_32bpp.LockBits(rect, ImageLockMode.ReadOnly, Bitmap_32bpp.PixelFormat);
            IntPtr ptr1 = input_data.Scan0;
            int bytesbpp = 4;    // bytes_per_pixel (it is fixed)
            int inputstride = (input_data.Width * bytesbpp + 3) / 4 * 4;
            int nbofinputbytes = inputstride * input_data.Height;
            byte[] inp = new byte[nbofinputbytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr1, inp, 0, nbofinputbytes);

            // get the output buffer
            BitmapData output_data = Bitmap_24bpp.LockBits(rect, ImageLockMode.ReadWrite, Bitmap_24bpp.PixelFormat);
            IntPtr ptr2 = output_data.Scan0;
            int outputstride = (output_data.Width * 3 + 3) / 4 * 4;
            int nbofoutputbytes = outputstride * output_data.Height;
            byte[] outp = new byte[nbofoutputbytes];

            for (int jj = 0; jj < Bitmap_32bpp.Height; jj++)
            {
                for (int ii = 0; ii < Bitmap_32bpp.Width; ii++)
                {
                    outp[3 * ii + jj * outputstride] = inp[4 * ii + 0 + jj * inputstride];
                    outp[3 * ii + 1 + jj * outputstride] = inp[4 * ii + 1 + jj * inputstride];
                    outp[3 * ii + 2 + jj * outputstride] = inp[4 * ii + 2 + jj * inputstride];
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(outp, 0, ptr2, nbofoutputbytes);
            Bitmap_32bpp.UnlockBits(input_data);
            Bitmap_24bpp.UnlockBits(output_data);

            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        // ### sharpen ###

        public Bitmap ImageSharpen(Bitmap bitmap)
        {
            // Only 24bpp RGB bitmap is accepted.
            if (bitmap == null || bitmap.PixelFormat != PixelFormat.Format24bppRgb)
                return null;

            int w = bitmap.Width;
            int h = bitmap.Height;
            BitmapData image_data = bitmap.LockBits(
                new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);
            int bytes = image_data.Stride * image_data.Height;
            byte[] buffer = new byte[bytes];
            byte[] result = new byte[bytes];
            Marshal.Copy(image_data.Scan0, buffer, 0, bytes);
            bitmap.UnlockBits(image_data);
            for (int i = 2; i < w - 2; i++)
            {
                for (int j = 2; j < h - 2; j++)
                {
                    int p = i * 3 + j * image_data.Stride;
                    for (int k = 0; k < 3; k++)
                    {
                        double val = (double)(4 * buffer[p]);
                        val -= (double)buffer[p - image_data.Stride];
                        val -= (double)buffer[p - 3];
                        val -= (double)buffer[p + 3];
                        val -= (double)buffer[p + image_data.Stride];

                        val = val > 0 ? val : 0;
                        result[p + k] = (byte)((val + buffer[p + k]) > 255 ? 255 : (val + buffer[p + k]));
                    }
                }
            }
            Bitmap res_img = new Bitmap(w, h, PixelFormat.Format24bppRgb);
            BitmapData res_data = res_img.LockBits(
                new Rectangle(0, 0, w, h),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);
            Marshal.Copy(result, 0, res_data.Scan0, bytes);
            res_img.UnlockBits(res_data);
            return res_img;
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

    }

}
