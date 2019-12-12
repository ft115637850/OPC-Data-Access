using Opc;
using Opc.Da;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OPCDA
{
    public partial class Form1 : Form
    {
        Opc.Da.Subscription group;
        public Form1()
        {
            InitializeComponent();
            // Create a server object and connect to the Server
            Opc.URL url = new Opc.URL("opcda://localhost/OI.MBTCP.1");
            Opc.Da.Server server = null;
            OpcCom.Factory fact = new OpcCom.Factory();
            server = new Opc.Da.Server(fact, null);
            server.Connect(url, new Opc.ConnectData(new System.Net.NetworkCredential()));

            // Create a group
            Opc.Da.SubscriptionState groupState = new Opc.Da.SubscriptionState();
            groupState.Name = "Group";
            groupState.Active = true; // false when read/write
            groupState.UpdateRate = 200;
            group = (Opc.Da.Subscription)server.CreateSubscription(groupState);

            // add items to the group
            Opc.Da.Item[] items = new Opc.Da.Item[3];
            items[0] = new Opc.Da.Item();
            items[0].ItemName = "New_TCPIP_PORT_000.New_ModbusPLC_000.Item_0";
            items[1] = new Opc.Da.Item();
            items[1].ItemName = "New_TCPIP_PORT_000.New_ModbusPLC_000.Item_1";
            items[2] = new Opc.Da.Item();
            items[2].ItemName = "New_TCPIP_PORT_000.New_ModbusPLC_000.Item_2";
            items = group.AddItems(items);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // and now read the items again
            Opc.IRequest req;
            group.Read(group.Items, 123, new Opc.Da.ReadCompleteEventHandler(ReadCompleteCallback), out req);
        }

        private void WriteCompleteCallback(object clientHandle, Opc.IdentifiedResult[] results)
        {
            this.label3.Text = "Write completed";
            this.label1.Text = "";
            foreach (Opc.IdentifiedResult writeResult in results)
            {
                Console.WriteLine("\t{0} write result: {1}", writeResult.ItemName, writeResult.ResultID);
                this.label1.Text = this.label1.Text + $" | {writeResult.ItemName} write result: {writeResult.ResultID}";
            }
        }

        private void ReadCompleteCallback(object clientHandle, Opc.Da.ItemValueResult[] results)
        {
            Console.WriteLine("Read completed");
            this.label2.Text = "Read completed";
            this.label1.Text = "";
            foreach (Opc.Da.ItemValueResult readResult in results)
            {
                Console.WriteLine("\t{0}\tval:{1}", readResult.ItemName, readResult.Value);
                this.label1.Text = this.label1.Text + $" | {readResult.ItemName} val:{readResult.Value}";
            }
            Console.WriteLine();
        }

        private void OnTransactionCompleted(object group, object hReq, Opc.Da.ItemValueResult[] items)
        {
            for (int i = 0; i < items.GetLength(0); i++)
            {
                Console.WriteLine("Item DataChange - ItemId: {0}", items[i].ItemName);
                this.label1.Text = $"Item DataChange - ItemId: {items[i].ItemName}";
                Console.WriteLine(" Value: {0,-20}", items[i].Value);
                this.label2.Text = $"Value: {items[i].Value}";
                Console.WriteLine(" TimeStamp: {0:00}:{1:00}:{2:00}.{3:000}",

                items[i].Timestamp.Hour,

                items[i].Timestamp.Minute,

                items[i].Timestamp.Second,

                items[i].Timestamp.Millisecond);
                this.label3.Text = $"TimeStamp: {items[i].Timestamp.Hour}:{items[i].Timestamp.Minute}:{items[i].Timestamp.Second}.{items[i].Timestamp.Millisecond}";

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // write items
            Opc.Da.ItemValue[] writeValues = new Opc.Da.ItemValue[3];
            writeValues[0] = new Opc.Da.ItemValue();
            writeValues[1] = new Opc.Da.ItemValue();
            writeValues[2] = new Opc.Da.ItemValue();
            writeValues[0].ServerHandle = group.Items[0].ServerHandle;
            writeValues[0].Value = 0;
            writeValues[1].ServerHandle = group.Items[1].ServerHandle;
            writeValues[1].Value = 0;
            writeValues[2].ServerHandle = group.Items[2].ServerHandle;
            writeValues[2].Value = 0;
            Opc.IRequest req;
            group.Write(writeValues, 321, new Opc.Da.WriteCompleteEventHandler(WriteCompleteCallback), out req);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            group.DataChanged += new Opc.Da.DataChangedEventHandler(OnTransactionCompleted);
        }
    }
}
