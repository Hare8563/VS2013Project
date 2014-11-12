using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using LabImg;


namespace kMeansTester
{
    public partial class Form1 : Form
    {
        float[,] depthData;
        IplImage img;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            img = new IplImage(Environment.CurrentDirectory+@"\RectifyRight.bmp");
            CommonUtility.FillPicBox(img.ToBitmap(), pictureBox1);
            depthData = HTUtility.loadCSV(Environment.CurrentDirectory + @"\top.csv");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CvMat m = Cv.CreateMat(depthData.Length, 3, MatrixType.F32C1);
            CvMat cluster = Cv.CreateMat(m.Rows, 1, MatrixType.S32C1);
            int K = int.Parse(textBox1.Text);
            int max_iter = int.Parse(textBox2.Text);
            double epsilon = double.Parse(textBox3.Text);
            for (int y = 0; y < depthData.GetLength(0); y++)
            {
                for (int x = 0; x < depthData.GetLength(1); x++)
                {
                    float depth = depthData[y, x];
                    int index = y * depthData.GetLength(1) + x;

                    m[index, 0] = x;
                    m[index, 1] = y;
                    m[index, 2] = depth;
                }
            }
            
            Cv.KMeans2(m, K, cluster, new CvTermCriteria(max_iter, epsilon));

            Bitmap bmp = new Bitmap(img.Width, img.Height);
            int[] hist = new int[K];
            hist.Initialize();
            for (int y = 0; y < depthData.GetLength(0); y++)
            {
                for (int x = 0; x < depthData.GetLength(1) - 1; x++)
                {
                    float depth = depthData[y, x];
                    int index = y * depthData.GetLength(1) + x;
                    hist[(int)(cluster[index].Val0)]++;

                    //if (cluster[index].Val0 < 14.0 && cluster[index].Val0 >= 5)
                    //{
                    //    bmp.SetPixel(x, y, Color.White);
                    //}
                    //else
                    //{
                    //    bmp.SetPixel(x, y, Color.Black);
                    //}
                   
                    Color color = ColorScaleBCGYR((cluster[index].Val0 / (double)K));
                    bmp.SetPixel(x, y, color);
                }
            }

            CommonUtility.FillPicBox(bmp, pictureBox2);
            plotChart(hist);

        }

        private void plotChart(int[] data)
        {
            string legend = "グラフ";
            chart1.Series.Clear();

            chart1.Series.Add(legend);

            chart1.Series[legend].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
            chart1.Series[legend].LegendText = legend;

            int[] xValue = new int[data.Length];
            for (int i = 0; i < xValue.Length; i++)
            {
                System.Windows.Forms.DataVisualization.Charting.DataPoint dp = new System.Windows.Forms.DataVisualization.Charting.DataPoint();
                dp.SetValueXY(xValue[i], data[i]);
                dp.IsValueShownAsLabel = true;
                chart1.Series[legend].Points.Add(dp);
            }

        }

        
        Color ColorScaleBCGYR(double in_value)
        {
            // 0.0～1.0 の範囲の値をサーモグラフィみたいな色にする
            // 0.0                    1.0
            // 青    水    緑    黄    赤
            // 最小値以下 = 青
            // 最大値以上 = 赤
            int ret;
            int a = 255;    // alpha値
            int r, g, b;    // RGB値
            double value = in_value;
            double tmp_val = Math.Cos(4 * Math.PI * value);
            int col_val = (int)((-tmp_val / 2 + 0.5) * 255);
            if (value >= (4.0 / 4.0)) { r = 255; g = 0; b = 0; }   // 赤
            else if (value >= (3.0 / 4.0)) { r = 255; g = col_val; b = 0; }   // 黄～赤
            else if (value >= (2.0 / 4.0)) { r = col_val; g = 255; b = 0; }   // 緑～黄
            else if (value >= (1.0 / 4.0)) { r = 0; g = 255; b = col_val; }   // 水～緑
            else if (value >= (0.0 / 4.0)) { r = 0; g = col_val; b = 255; }   // 青～水
            else { r = 0; g = 0; b = 255; }   // 青
            Color color = Color.FromArgb(r, g, b);
            return color;

            //ret = (a & 0x000000FF) << 24
            //    | (r & 0x000000FF) << 16
            //    | (g & 0x000000FF) << 8
            //    | (b & 0x000000FF);
            //return ret;
        }


    }
}
