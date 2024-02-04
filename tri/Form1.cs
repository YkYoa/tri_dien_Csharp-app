using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace tri
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            String[] Baudrate = { "115200", "200000" };
            cboBaud.Items.AddRange(Baudrate);
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }
        private string hex2binary(string hexvalue)
        {
            string binaryval = "";
            binaryval = Convert.ToString(Convert.ToInt32(hexvalue, 16), 2);
            return binaryval;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cboPort.DataSource = SerialPort.GetPortNames();
            cboBaud.Text = "115200";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.PortName = cboPort.Text;
                serialPort1.BaudRate = int.Parse(cboBaud.Text);
                serialPort1.Open();
                button1.Enabled = false;
                button2.Enabled = true;
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.Close();
                button1.Enabled = true;
                button2.Enabled = false;
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }
        private int lastReceivedIndex = -1;
        private string lastReceivedName = "";
        private DateTime lastReceivedTime = DateTime.MinValue;
        private Dictionary<string, int> deviceCounts = new Dictionary<string, int>();
        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = serialPort1.ReadLine();
            Invoke(new MethodInvoker(() =>
                {
                    repBox.Text = data;
                    listBox1.Items.Add(data);
                    string[] array = data.Split(new string[] { "/", ":" }, StringSplitOptions.RemoveEmptyEntries);
                    listBox4.Items.Add($" {data} Received at {DateTime.Now.ToString("HH:mm:ss")}");
                    if (array.Length >= 34)
                    {
                        listBox4.Items.Clear();
                        repBox.Clear();
                        idbox1.Text = array[0];
                        nodebox.Text = array[1];
                        int currentIndex = int.Parse(array[0]);
                        string currentName = array[1];

                        // Update last received data
                        lastReceivedIndex = currentIndex;
                        lastReceivedName = currentName;
                        lastReceivedTime = DateTime.Now;

                        // Update dictionary with device count
                        string deviceKey = $"{currentName}_{currentIndex}";
                        int deviceCount = 1;
                        if (deviceCounts.ContainsKey(deviceKey))
                        {
                            deviceCount = ++deviceCounts[deviceKey];
                        }
                        else
                        {
                            deviceCounts[deviceKey] = deviceCount;
                        }

                        // Update listbox2 with current name, ID, timestamp, and device count
                        string displayText = $"{currentName} ({currentIndex}) - Count: {deviceCount} - Last Received: {lastReceivedTime.ToString("HH:mm:ss")}";
                        listBox2.Invoke((MethodInvoker)(() => listBox2.Items.Add(displayText)));

                        // Check if device didn't appear three times
                        if (deviceCount % 2 == 0)
                        {
                            // Display a message in listbox3
                            string listbox3Text = $"Device {currentName} ({currentIndex}) still online at {lastReceivedTime.ToString("HH:mm:ss")} ";
                            listBox3.Invoke((MethodInvoker)(() => listBox3.Items.Add(listbox3Text)));
                        }



                        // Process each measurement
                        for (int i = 0; i < 8; i++)
                        {
                            int index = 2 + i * 4;
                            if (array.Length >= index + 4)
                            {
                                string hex = array[index];
                                string bin = hex2binary(hex);

                                // Split binary string into separate numbers
                                string[] myStringArray = bin.PadLeft(12, '0').Select(x => x.ToString()).ToArray();

                                // Assign values to labels in the form
                                Label emcLabel = Controls.Find("emc_" + (i + 1), true).FirstOrDefault() as Label;
                                Label[] sLabels = new Label[12];
                                for (int j = 0; j < 12; j++)
                                {
                                    sLabels[j] = Controls.Find("s" + (j + 1) + "_" + (i + 1), true).FirstOrDefault() as Label;
                                }
                                Label as1Label = Controls.Find("as1_" + (i + 1), true).FirstOrDefault() as Label;
                                Label as2Label = Controls.Find("as2_" + (i + 1), true).FirstOrDefault() as Label;
                                Label as3Label = Controls.Find("as3_" + (i + 1), true).FirstOrDefault() as Label;

                                emcLabel.Invoke((MethodInvoker)(() => emcLabel.Text = myStringArray[0]));
                                for (int j = 0; j < 11; j++)
                                {
                                    int labelIndex = j; // Create a local variable to capture the value of j
                                    sLabels[j].Invoke((MethodInvoker)(() => sLabels[labelIndex].Text = myStringArray[labelIndex + 1]));
                                }
                                as1Label.Invoke((MethodInvoker)(() => as1Label.Text = array[index + 1]));
                                as2Label.Invoke((MethodInvoker)(() => as2Label.Text = array[index + 2]));
                                as3Label.Invoke((MethodInvoker)(() => as3Label.Text = array[index + 3]));
                            }
                        }
                    }
                })); 
        }

        private void button3_Click(object sender, EventArgs e)
        {
            serialPort1.WriteLine(serialBox.Text);
        }
    }
}
