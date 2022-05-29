using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Grpc.Client
{
    public class GrpcClientOptions
    {

        private static GrpcChannelOptions oChannelOptions { get; } = new GrpcChannelOptions();
        private static HttpClientHandler HttpHandler {get; set;} = new HttpClientHandler();
        private static WinHttpHandler WinHandler {get; set;} = new WinHttpHandler();
        private static GrpcWebHandler GrpcHandler {get; set;}

        /// <summary>
        /// Constructeur pour un handler quand le .net ne supporte pas http/2
        /// </summary>
        private GrpcClientOptions()
        {

        }

        /// <summary>
        /// Initialiseur statique de la classe GrpcClientOptions
        /// un handler quand le .net ne supporte pas http/2
        /// </summary>
        /// <returns></returns>
        public static GrpcClientOptions initGrpcClientOptions() {
            return new GrpcClientOptions();
        }


        /// <summary>
        /// Permet d'ajouter au Handler une connexion non sécurisée.
        /// Derrière on appelle AddWebHandler, pour être sur de le rajouter.
        /// Donc inutile de le rappeler.
        /// </summary>
        public GrpcClientOptions AddHandlerDangerousCertificat()
        {
            HttpHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            return this;
        }

        /// <summary>
        /// Permet d'ajouter au Handler une connexion non sécurisée.
        /// Derrière on appelle AddWebHandler, pour être sur de le rajouter.
        /// Donc inutile de le rappeler.
        /// </summary>
        public GrpcClientOptions AddGrpcWebHandler()
        {
            GrpcHandler = new GrpcWebHandler(HttpHandler);

            return this;
        }

        /// <summary>
        /// Ajoute la version du protocole au Handler.
        /// </summary>
        /// <param name="major"></param>
        /// <param name="minor"></param>
        public GrpcClientOptions AddHandlerVersionHTTP(int major, int minor)
        {
            GrpcHandler.HttpVersion = new Version(major, minor);

            return this;
        }

        /// <summary>
        /// Ajoute un webHandler aux options grpc
        /// </summary>
        public GrpcClientOptions AddWebHandler()
        {
            oChannelOptions.HttpHandler = GrpcHandler;

            return this;
        }

        /// <summary>
        /// Un autre handler pour ajouter le support HTTP/2 sur .NET Framework,
        /// </summary>
        public GrpcClientOptions AddWinHttpHandler()
        {
            oChannelOptions.HttpHandler = WinHandler;

            return this;
        }

        /// <summary>
        /// Pour récupérer les options du grpc channel
        /// </summary>
        /// <returns></returns>
        public GrpcChannelOptions GetGrpcChannelOptions()
        {
            return oChannelOptions;
        }

    }
}