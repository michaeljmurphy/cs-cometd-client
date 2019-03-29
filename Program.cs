using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

using Genesys.Bayeux.Client;

namespace cometd_cs
{

    class Program
    {
        private static ManualResetEvent manualResetEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            var uri = args[1];
            var channel = args[2];

            BayeuxClient bc = SubToStream(uri, channel).Result;
            manualResetEvent.Set();
            WaitForEvent(bc).Wait();
        }

        private static async Task<BayeuxClient> SubToStream(String uri, String channel)
        {
            var httpClient = new HttpClient();
            var bayeuxClient = new BayeuxClient(
            new HttpLongPollingTransportOptions()
            {
                HttpClient = httpClient,
                Uri = uri
            });


            bayeuxClient.ConnectionStateChanged += (e, args) =>
            Console.WriteLine($"Bayeux connection state changed to {args.ConnectionState}");

            bayeuxClient.AddSubscriptions(channel);

            await bayeuxClient.Start();

            return bayeuxClient;
        }

        private static async Task WaitForEvent(BayeuxClient bc) 
        {
            await new TaskFactory().StartNew(() => 
            {
                while(true)
                {
                    manualResetEvent.WaitOne();
                    bc.EventReceived += (e, args) =>
                        Console.WriteLine($"Event received on channel {args.Channel} with data\n{args.Data}");
                    manualResetEvent.Reset();
                }
            });
        }
    }
}
