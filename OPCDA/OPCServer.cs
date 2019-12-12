using Opc;
using Opc.Da;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPCDA
{
    public class OPCServer
    {
        private Opc.Da.Server _server;
        private URL _serverURL;
        public delegate void DataChangedEventHandler(object subscriptionHandle, object requestHandle, ItemValueResult[] values);

        public OPCServer(string serverName)
        {
            if (String.IsNullOrEmpty(serverName))
                throw new ArgumentException("Server name is invalid.");

            _serverURL = new URL(serverName);
            _serverURL.Scheme = "opcda";
            _server = new Opc.Da.Server(new OpcCom.Factory(), _serverURL);
            _server.Connect();
        }

        public void AddSubscription(string groupName, List<string> tagList, DataChangedEventHandler onDataChange)
        {

            if (!_server.IsConnected)
            {
                Console.WriteLine("Connection to OPC server is not established");
                return;
            }

            // Create group
            Opc.Da.Subscription group;
            Opc.Da.SubscriptionState groupState = new Opc.Da.SubscriptionState();
            groupState.Name = groupName;
            groupState.Active = true;
            groupState.UpdateRate = 1000;

            // Short circuit if group already exists
            SubscriptionCollection existingCollection = _server.Subscriptions;
            
            foreach(Subscription collection in existingCollection)
            {
                if (collection.Name == groupName)
                {
                    Console.WriteLine(String.Format("Subscription group {0} already exists", groupName));
                    return;
                }
            }

            group = (Opc.Da.Subscription)_server.CreateSubscription(groupState);

            // Create list of items to monitor
            Item[] opcItems = new Item[tagList.Count];
            int j = 0;
            foreach (string tag in tagList)
            {
                opcItems[j] = new Item();
                opcItems[j].ItemName = tag;
                j++;
            }

            // Attach items and event to group
            group.AddItems(opcItems);
            group.DataChanged += new Opc.Da.DataChangedEventHandler(onDataChange);
        }
    }
}
