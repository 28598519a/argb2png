using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.IO;
using System.Drawing;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace argb2png
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen; //居中顯示
        }

        
        private void btn_deal_pic_Click(object sender, RoutedEventArgs e)
        {
            App.count = 0;

            CommonOpenFileDialog fbd = new CommonOpenFileDialog();
            fbd.InitialDirectory = App.selectPath;
            fbd.IsFolderPicker = true;
            if (fbd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                App.selectPath = fbd.FileName;
            }
            else
            {
                return;
            }

            try
            {
                List<string> fileList = Directory.GetFiles(App.selectPath, "*.argb", cb_alpha_child_dir.IsChecked == true ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
                foreach (string file in fileList)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    string filefolder = Path.GetDirectoryName(file);
                    string filepath = Path.Combine(filefolder, fileName);

                    Bitmap img1 = Cutjpg(filepath);
                    Bitmap img2 = Cutpng(filepath);

                    ImageDeal.DealImage(img1, img2, $"{filepath}.png");
                    img1.Dispose();
                    img2.Dispose();

                    File.Delete(file);
                    lb_count.Content = $"( {App.count++} / {fileList.Count} )";
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"失敗：{ex.Message}");
            }
            lb_count.Content = String.Empty;
            System.Windows.MessageBox.Show($"完成! 共轉換{App.count}個");
        }

        /// <summary>
        /// 從argb切出png
        /// </summary>
        /// <param name="fileName">不含副檔名的完整路徑</param>
        /// <returns>Bitmap</returns>
        private Bitmap Cutpng(string fileName)
        {
            //string MaskFile = fileName + "+msk.png";
            Bitmap bmp;

            byte[] TMP = File.ReadAllBytes($"{fileName}.argb");

            byte[] pngStartSequence = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
            byte[] pngEndSequence = new byte[] { 0xAE, 0x42, 0x60, 0x82 };

            int start = IndexOf(TMP, pngStartSequence, 0);
            if (start == -1)
            {
                System.Windows.MessageBox.Show("Could not find PNG header");
                return null;
            }

            int end = IndexOf(TMP, pngEndSequence, start + pngStartSequence.Length);
            if (end == -1)
            {
                System.Windows.MessageBox.Show("Could not find PNG footer");
                return null;
            }

            int pngLength = end - start + 4;
            byte[] PNG = new byte[pngLength];

            Array.Copy(TMP, start, PNG, 0, pngLength);
            //File.WriteAllBytes(MaskFile, PNG);
            
            using (var ms = new MemoryStream(PNG))
            {
                bmp = new Bitmap(ms);
            }
            return bmp;
        }

        /// <summary>
        /// 從argb切出jpg
        /// </summary>
        /// <param name="fileName">不含副檔名的完整路徑</param>
        /// <returns>Bitmap</returns>
        private Bitmap Cutjpg(string fileName)
        {
            //string RGBFile = fileName + "+rgb.jpg";
            Bitmap bmp;

            byte[] TMP = File.ReadAllBytes($"{fileName}.argb");

            byte[] jpgStartSequence = new byte[] { 0xFF, 0xD8 };
            byte[] jpgEndSequence = new byte[] { 0xFF, 0xD9 };

            int start = IndexOf(TMP, jpgStartSequence, 0);
            if (start == -1)
            {
                System.Windows.MessageBox.Show("Could not find JPG header");
                return null;
            }

            int end = IndexOf(TMP, jpgEndSequence, start + jpgStartSequence.Length);
            if (end == -1)
            {
                System.Windows.MessageBox.Show("Could not find JPG footer");
                return null;
            }

            int jpgLength = end - start + 2;
            byte[] JPG = new byte[jpgLength];

            Array.Copy(TMP, start, JPG, 0, jpgLength);
            //File.WriteAllBytes(RGBFile, JPG);

            using (var ms = new MemoryStream(JPG))
            {
                bmp = new Bitmap(Image.FromStream(ms));
            }
            return bmp;
        }

        private static int IndexOf(byte[] array, byte[] sequence, int startIndex)
        {
            if (sequence.Length == 0)
                return -1;

            int found = 0;
            for (int i = startIndex; i < array.Length; i++)
            {
                if (array[i] == sequence[found])
                {
                    if (++found == sequence.Length)
                    {
                        return i - found + 1;
                    }
                }
                else
                {
                    found = 0;
                }
            }

            return -1;
        }
    }
}