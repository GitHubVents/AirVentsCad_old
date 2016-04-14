using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Windows.Forms;
using VentsCadServiceLibrary;

namespace VentsCadService
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            button2.Enabled = false;
        }

        ServiceHost host { get; set; }
        Uri baseAddress { get; set; } = new Uri("http://192.168.14.86:8000/hello");

        private void button1_Click(object sender, EventArgs e)
        {
            host = new ServiceHost(typeof(VentsService), baseAddress);

            var smb = new ServiceMetadataBehavior
            {
                HttpGetEnabled = true,
                MetadataExporter = { PolicyVersion = PolicyVersion.Policy15 }
            };
            host.Description.Behaviors.Add(smb);

            host.Open();
            Status.Text = $"The service is ready at {baseAddress}";
            button1.Enabled = false;
            button2.Enabled = true;            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            host.Close();
            Status.Text = $"The service is stopped";
            button1.Enabled = true;
            button2.Enabled = false;
        }
    }
}
