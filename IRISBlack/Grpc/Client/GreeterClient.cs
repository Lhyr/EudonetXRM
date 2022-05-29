using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Eudo_Grpc;

namespace Com.Eudonet.Xrm.IRISBlack.Grpc.Client
{
    public class GreeterClient
    {
        private static string URL { get; set; }
        private static GrpcChannelOptions GrpcOptions { get; set; }

        /// <summary>
        /// Constructeur pour Greeter Client GRPC
        /// </summary>
        /// <param name="url"></param>
        /// <param name="options"></param>
        private GreeterClient(string url, GrpcChannelOptions options = null)
        {
            URL = url;
            GrpcOptions = options;
        }

        /// <summary>
        /// Initialiseur static pour le Greeter Client GRPC
        /// </summary>
        /// <param name="url"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static GreeterClient InitGreeterClient(string url, GrpcChannelOptions options = null)
        {
            return new GreeterClient(url, options);
        }

        /// <summary>
        /// On récupère la réponse du serveur.
        /// </summary>
        /// <returns></returns>
        public async Task<HelloReply> GetServerResponse()
        {
            HelloReply reply = null;

            try
            {
                using (var channel = GrpcChannel.ForAddress(URL, GrpcOptions))
                {
                    var client = new Greeter.GreeterClient(channel);
                    reply = await client.SayHelloAsync(new HelloRequest { Name = "GreeterClient" });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
            }

            return reply;
        }
    }
}