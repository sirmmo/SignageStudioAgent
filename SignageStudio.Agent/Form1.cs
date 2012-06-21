using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ServiceProcess;
using ServiceTools;

namespace SignageStudio.Agent
{
    public partial class Form1 : Form
    {
        private Communicator c;
        public Form1()
        {
            InitializeComponent();

            c = new Communicator();

            if (ExistsData())
                this.Hide();
            else
                this.Show();

            c.Connected += new EventHandler(c_Connected);
            c.ConnectionError += new EventHandler(c_ConnectionError);
            c.Quit += new EventHandler(c_Quit);

            c.Start();

            
        }

        void c_Quit(object sender, EventArgs e)
        {
            c.Disconnect();
            Application.Exit();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        void c_ConnectionError(object sender, EventArgs e)
        {
            notifyIcon1.ShowBalloonTip(1000, "Connection Error!", "The agent is connected", ToolTipIcon.Error);
            this.Show();
        }

        void c_Connected(object sender, EventArgs e)
        {
            notifyIcon1.ShowBalloonTip(1000, "Connected!", "The agent is connected", ToolTipIcon.Info);
        }

        private bool ExistsData()
        {
            return false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            c.Username = textBox1.Text;
            c.Password = textBox2.Text;
            c.Domain = textBox3.Text;
            c.Refresh();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            c.Disconnect();
            Application.Exit();
        }



        public static void StartService(string serviceName, int timeoutMilliseconds)
        {
            ServiceController service = new ServiceController(serviceName);
            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
            catch
            {
                Console.WriteLine("cazz");

            }
        }
    }
}
