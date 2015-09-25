using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpAvi;
using SharpAvi.Output;
using SharpAvi.Codecs;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace CaptureScreenPoc
{
    public partial class Form1 : Form
    {
        
        AviWriter writer;
        private IAviVideoStream stream;

        public Form1()
        {
            InitializeComponent();
        }

        private void LoadSettings()
        {
            try
            {
                writer = new AviWriter("test.avi")
                    {
                        FramesPerSecond = 25,
                        // Emitting AVI v1 index in addition to OpenDML index (AVI v2)
                        // improves compatibility with some software, including 
                        // standard Windows programs like Media Player and File Explorer
                        EmitIndex1 = true
                    };


                var encoder = new Mpeg4VideoEncoderVcm((int)numericWidth.Value, (int)numericHeight.Value, 25, 0, 100, KnownFourCCs.Codecs.Xvid);
//                stream = writer.AddMotionJpegVideoStream(640, 480, quality: 100);
                //stream = writer.AddMpeg4VideoStream(640, 480, 30, quality: 70, codec: KnownFourCCs.Codecs.X264, forceSingleThreadedAccess: true);
                stream = writer.AddEncodingVideoStream(encoder, true, (int)numericWidth.Value, (int)numericHeight.Value);

                // set standard VGA resolution
                //            stream.Width = 640;
                //            stream.Height = 480;
                // class SharpAvi.KnownFourCCs.Codecs contains FOURCCs for several well-known codecs
                // Uncompressed is the default value, just set it for clarity

                //stream.Codec = KnownFourCCs.Codecs.MotionJpeg;
                // Uncompressed format requires to also specify bits per pixel
                //stream.BitsPerPixel = BitsPerPixel.Bpp32;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }



        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoadSettings();
            timer1.Interval = (int) (writer.FramesPerSecond);
            timer1.Start();
        }

        private void CaptureMoni()
        {

            try
            {

                //var frameData = new byte[stream.Width*stream.Height*4];
                Bitmap bmp = new Bitmap(stream.Width, stream.Height);
                Graphics gr = Graphics.FromImage(bmp);
                
                gr.CopyFromScreen(0, 0, 0, 0, new Size(stream.Width, stream.Height));

                var frameData = BitmapToByteArray(bmp);

                
                // write data to a frame
                stream.WriteFrame(true,
                    // is key frame? (many codecs use concept of key frames, for others - all frames are keys)
                    frameData, // array with frame data
                    0, // starting index in the array
                    frameData.Length // length of the data
                    );

            }
            catch (Exception e)
            {
                
            }
        }

        public static byte[] BitmapToByteArray(Bitmap bitmap)
        {

            BitmapData bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            int numbytes = bmpdata.Stride * bitmap.Height;
            byte[] bytedata = new byte[numbytes];
            IntPtr ptr = bmpdata.Scan0;

            Marshal.Copy(ptr, bytedata, 0, numbytes);

            bitmap.UnlockBits(bmpdata);

            return bytedata;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            writer.Close();
            timer1.Stop();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            CaptureMoni();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            writer.Close();
            timer1.Stop();
        }

        
    }
}
