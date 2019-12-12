using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Opc;
using Opc.Da;
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

namespace OPCDA
{
    public partial class MainForm : Form
    {
        private readonly string config = "data_config.json";
        private Dictionary<string, Dictionary<string, ListViewItem.ListViewSubItem[]>> subscribeItems;
        public MainForm()
        {
            InitializeComponent();
            if (File.Exists(config))
            {
                try
                {
                    this.initialize_Subscription();
                }
                catch
                {
                    MessageBox.Show("Config File format error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Config File is missing", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void initialize_Subscription()
        {
            subscribeItems = new Dictionary<string, Dictionary<string, ListViewItem.ListViewSubItem[]>>();
            StreamReader file = new StreamReader(config);
            JsonTextReader reader = new JsonTextReader(file);
            JToken obj = (JToken)JToken.ReadFrom(reader);
            foreach (JObject grp in obj)
            {
                var subscribeGroup = new Dictionary<string, ListViewItem.ListViewSubItem[]>();
                subscribeItems.Add(grp.Value<string>("name"), subscribeGroup);
                var lstGroup = new ListViewGroup(grp.Value<string>("name"));
                this.listView1.Groups.Add(lstGroup);
                var items = JArray.Parse(grp["items"].ToString());
                foreach (var item in items)
                {
                    var i = new ListViewItem(item.Value<string>());
                    var iValue = new ListViewItem.ListViewSubItem(i, "");
                    var iQuality = new ListViewItem.ListViewSubItem(i, "");
                    var iTimeStamp = new ListViewItem.ListViewSubItem(i, "");
                    i.Group = lstGroup;
                    i.SubItems.Add(iValue);
                    i.SubItems.Add(iQuality);
                    i.SubItems.Add(iTimeStamp);
                    subscribeGroup.Add(item.Value<string>(), new ListViewItem.ListViewSubItem[] { iValue, iQuality, iTimeStamp });
                    this.listView1.Items.Add(i);
                }
            }
        }

        private void connectBtn_Click(object sender, EventArgs e)
        {
            this.connectBtn.Enabled = false;
            try
            {
                var server = new OPCServer(this.serverTxtB.Text.Trim());
                foreach(var group in subscribeItems)
                {
                    var tagLst = group.Value.Keys.ToList<string>();
                    var subscriber = new DataChangedSubscription
                    {
                        GroupName = group.Key,
                        SubscribeItems = this.subscribeItems
                    };
                    server.AddSubscription(group.Key, tagLst, subscriber.OPCSubscription_DataChanged);
                }

                this.connectBtn.Text = "Connected";
            } catch (Exception ex)
            {
                this.connectBtn.Text = "Connect";
                this.connectBtn.Enabled = true;
                MessageBox.Show("Connection failed " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        class DataChangedSubscription
        {
            public string GroupName { get; set; }
            public Dictionary<string, Dictionary<string, ListViewItem.ListViewSubItem[]>> SubscribeItems { get; set; }
            public void OPCSubscription_DataChanged(object subscriptionHandle, object requestHandle, ItemValueResult[] tags)
            {
                // Display result whenever a tag value is updated
                foreach (var tag in tags)
                {
                    SubscribeItems[GroupName][tag.ItemName][0].Text = tag.Value.ToString();
                    SubscribeItems[GroupName][tag.ItemName][1].Text = tag.Quality.ToString();
                    SubscribeItems[GroupName][tag.ItemName][2].Text = tag.Timestamp.ToString();
                }
            }
        }

        /*
        private void onBrowse_Click(object sender, EventArgs e)
        {
            Opc.URL url = new Opc.URL($"opcda://{this.textBox1.Text}/{this.textBox2.Text}");
            Opc.Da.Server server = null;
            OpcCom.Factory fact = new OpcCom.Factory();
            server = new Opc.Da.Server(fact, null);
            server.Connect(url, new Opc.ConnectData(new System.Net.NetworkCredential()));
            ItemIdentifier itemId = null;
            BrowseFilters filters = new BrowseFilters() { BrowseFilter = browseFilter.branch };
            BrowsePosition position = new BrowsePosition(itemId, filters);
            BrowseElement[] elements = server.Browse(itemId, filters, out position);
            foreach (var el in elements)
            {
                this.listBox1.Items.Add(el.ItemName);
            }
        }
        */
    }
}
