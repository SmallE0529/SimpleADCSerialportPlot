using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Windows.Forms.DataVisualization.Charting;
using System.Threading;

namespace cc3200ADC
{
    public partial class Form1 : Form
    {
        private SerialPort ports;
        private const int SampleRate = 8;
        private Int16[] InputDataArray = new Int16[SampleRate];
        private static int cnt = 0;
        private Thread cpuThread;

        public Form1()
        {
            InitializeComponent();
            GetAvailablePorts();
            label1.Text = "COM";
            chart1.Series[0].ChartType = SeriesChartType.Line;
            chart1.Series[0].Color = Color.Red;
            chart1.Series[0].IsValueShownAsLabel = false;

        }

        void GetAvailablePorts()
        {
            string[] ports = SerialPort.GetPortNames();
            comboBox1.Items.AddRange(ports);
        }

        private void RescaleChart()
        {
            const int N = 200;

            var points = chart1.Series[0].Points;
            if (points.Count > N)
            {
                int xmax = points.Count - 1;
                int xmin = points.Count - N;
                chart1.ChartAreas[0].Axes[0].Minimum = xmin;
                chart1.ChartAreas[0].Axes[0].Maximum = xmax;
                double ymin = points.FindMinByValue("Y", xmin).YValues[0];
                double ymax = points.FindMaxByValue("Y", xmin).YValues[0];
                chart1.ChartAreas[0].Axes[1].Minimum = ymin;
                chart1.ChartAreas[0].Axes[1].Maximum = ymax;
            }

        }

        private void UpdateCpuChart(Int16[] data)
        {
            foreach (Int16 item in data)
            {
                chart1.Series[0].Points.AddY(item);

            }
            RescaleChart();
        }

        private void getPerformanceCounters()
        {
            while (true)
            {
                if (chart1.IsHandleCreated)
                {
                    //InputData = ports.ReadLine();
                    InputDataArray[cnt] = Int16.Parse(ports.ReadLine());
                    cnt++;
                    if (cnt >= SampleRate)
                    {
                        cnt = 0;
                        Int16[] InputDataArray2 = (Int16[])InputDataArray.Clone();
                        //this.Invoke(new Action(() => UpdateCpuChart(InputDataArray2)));
                        this.Invoke((MethodInvoker)delegate { UpdateCpuChart(InputDataArray2); });
                    }
                }
                else
                {
                    //......
                }
                //Thread.Sleep(16);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (comboBox1.Text == "")
            {
                MessageBox.Show("Please Select Serial Port");
            }
            else
            {
                ports = new SerialPort(comboBox1.Text, 
                                       115200, 
                                       Parity.None, 
                                       8, 
                                       StopBits.One);
                try
                {
                    ports.Open();
                    label1.Text = "";
                    cpuThread = new Thread(new ThreadStart(this.getPerformanceCounters));
                    cpuThread.IsBackground = true;
                    cpuThread.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                ports.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
