using OWOP_cs.OWOP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OWOP_cs
{
    public partial class Form1 : Form
    {
        string serverUri = "wss://owop.scar17off.repl.co/";

        List<Client> clients = new List<Client>();

        public Form1()
        {
            InitializeComponent();
        }

        private void UpdateLabel(string text)
        {
            if (label2.InvokeRequired)
            {
                label2.Invoke(new Action(() => label2.Text = text));
            }
            else
            {
                label2.Text = text;
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await Task.Run(async () =>
            {
                Client client = new Client(new { world = "main" });

                client.On("join", () =>
                {
                    UpdateLabel(clients.Count.ToString());
                });

                clients.Add(client);

                await client.Connect(serverUri);
            });
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            await Task.Run(async () =>
            {
                foreach (var client in clients.ToList())
                {
                    clients.Remove(client);
                    UpdateLabel(clients.Count.ToString());
                    await client.Disconnect();
                }
            });
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                Parallel.ForEach(clients.ToList(), async client =>
                {
                    for (int x = 0; x < 16; x++)
                    {
                        for (int y = 0; y < 16; y++)
                        {
                            await client.MoveAsync(x, y);
                        }
                    }
                });
            });
        } 

        private async void button4_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                Parallel.ForEach(clients.ToList(), async client =>
                {
                    for (int x = 0; x < 16; x++)
                    {
                        for (int y = 0; y < 16; y++)
                        {
                            await client.SetPixelAsync(x, y, new int[] { 255, 0, 0 }, false);
                        }
                    }
                });
            });
        }
    }
}