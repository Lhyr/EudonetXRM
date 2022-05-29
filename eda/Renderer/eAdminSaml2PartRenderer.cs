using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System.Web.UI;
using System.Text;
using System.IO;
using ITfoxtec.Identity.Saml2.Util;
using System.Security.Cryptography.X509Certificates;
using System.Configuration;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using System.Dynamic;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Merge;
using Newtonsoft.Json;
using ITfoxtec.Identity.Saml2.Schemas;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Classe gérant le rendu de la partie config de saml2
    /// </summary>
    public class eAdminSaml2PartRenderer : eAdminModuleRenderer
    {
        /// <summary>
        /// ProtocolSupported
        /// </summary>
        public IEnumerable<string> ProtocolSupported = new string[] { ProtocolBindings.HttpPost.ToString(), ProtocolBindings.HttpRedirect.ToString() };

        private eAuditLogging logger;
        private eSaml2DatabaseConfig config;
        private string jsonValue;
        private X509Certificate2 certificate;
        private string certificateError;

        private HelpRes helpRes;

        /// <summary>
        /// Renderer de la partie config de saml
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="config"></param>
        /// <param name="jsonValue"></param>
        public eAdminSaml2PartRenderer(ePref pref, eSaml2DatabaseConfig config, string jsonValue) : base(pref)
        {
            this.config = config;

            this.config.ServiceName = string.IsNullOrEmpty(this.config.ServiceName) ? $"Eudonet CRM - {pref.EudoBaseName}" : this.config.ServiceName;
            this.logger = new eAuditLogging(HttpContext.Current?.Session.SessionID, pref.User.UserLogin, pref.GetBaseName, config.AuditLogging ? 0 : 2);
            this.jsonValue = jsonValue;
            try
            {
                this.certificate = eSaml2DatabaseConfig.LoadCertificateFromDB(this.config.SigningCertificate);
            }
            catch (Exception ex)
            {
                logger.Warn($"Certificats introuvables <br />{ex.Message}<br /> {ex.StackTrace}", "INTERNAL");
                this.certificate = null;
            }



            helpRes = new HelpRes();

            helpRes["AutoCreateUser"] = new SamlItemRes() { Label = eResApp.GetRes(Pref, 1683), Title = eResApp.GetRes(Pref, 1683) };

            helpRes["SSOURL"] = new SamlItemRes() { Label = eResApp.GetRes(_ePref, 3114), Title = eResApp.GetRes(_ePref, 3114) };

            helpRes["IdentityProviderMetadata"] = new SamlItemRes() { Label = eResApp.GetRes(_ePref, 1943), Title = "Identity Provider Metadata (IdP) Metadata" };

            helpRes["ServiceProviderName"] = new SamlItemRes() { Label = eResApp.GetRes(_ePref, 1944), Title = "Service Provider Name" };
            helpRes["ServiceProviderIssuer"] = new SamlItemRes() { Label = eResApp.GetRes(_ePref, 1945), Title = "Service Provider (SP) Issuer" };
            helpRes["AllowedAudienceUri"] = new SamlItemRes() { Label = "AllowedAudienceUri", Title = "Allowed Audience Uri" };
            helpRes["AssertionConsumerUrl"] = new SamlItemRes() { Label = eResApp.GetRes(_ePref, 1946), Title = "Assertion Consumer Url" };
           
            helpRes["SigningCertificate"] = new SamlItemRes() { Label = eResApp.GetRes(_ePref, 8858), Title = eResApp.GetRes(_ePref, 8860) };
            //helpRes["EncryptionCertificate"] = new SamlItemRes() { Label = eResApp.GetRes(_ePref, 8857), Title = eResApp.GetRes(_ePref, 8859) };

            helpRes["IdentityProviderIssuer"] = new SamlItemRes() { Label = eResApp.GetRes(_ePref, 1947), Title = "Identity Provider (IdP) Issuer" };
            helpRes["SignOnDestination"] = new SamlItemRes() { Label = eResApp.GetRes(_ePref, 1948), Title = "Sign On Destination" };

            //helpRes["ForceAuthen"] = new SamlItemRes() { Label = eResApp.GetRes(_ePref, 1949), Title = eResApp.GetRes(_ePref, 1935) };

            helpRes["SignOnDestinationBinding"] = new SamlItemRes() { Label = eResApp.GetRes(_ePref, 1950), Title = "Sign On Binding" };
            helpRes["ConsumerBinding"] = new SamlItemRes() { Label = eResApp.GetRes(_ePref, 1951), Title = "Consumer Binding" };

            helpRes["NameIDFormat"] = new SamlItemRes() { Label = "NameID Format", Title = "NameID Format" };
            helpRes["MappingAttributes"] = new SamlItemRes() { Label = eResApp.GetRes(_ePref, 1952), Title = eResApp.GetRes(_ePref, 1936) };

            helpRes["SignRequest"] = new SamlItemRes() { Label = eResApp.GetRes(_ePref, 1953), Title = eResApp.GetRes(_ePref, 1937) };
            //helpRes["WantAssertionsSigned"] = new SamlItemRes() { Label = eResApp.GetRes(_ePref, 1954), Title = eResApp.GetRes(_ePref, 1938) };

            helpRes["SignAlgorithm"] = new SamlItemRes() { Label = eResApp.GetRes(_ePref, 1955), Title = eResApp.GetRes(_ePref, 1939) };

            helpRes["ServiceProviderMetadata"] = new SamlItemRes() { Label = eResApp.GetRes(_ePref, 1956), Title = eResApp.GetRes(_ePref, 1940) };

            //helpRes["CertificateValidationMode"] = new SamlItemRes() { Label = eResApp.GetRes(_ePref, 1961), Title = eResApp.GetRes(_ePref, 1963) };
            //helpRes["RevocationMode"] = new SamlItemRes() { Label = eResApp.GetRes(_ePref, 1962), Title = eResApp.GetRes(_ePref, 1966) };

            helpRes["AuditLogging"] = new SamlItemRes() { Label = "Trace", Title = "Trace" };
            helpRes["TraceText"] = new SamlItemRes() { Label = "", Title = "TraceText" };
        }

        /// <summary>
        /// On gènère la partie saml2
        /// </summary>
        /// <returns></returns>
        public override bool Generate()
        {
            _pgContainer = new Panel();
            _pgContainer.ID = "samlcontainer";


            // Fait un rendu d'input pour sauvegrder l'etat de la config
            RenderHiddenState(jsonValue, _pgContainer);

            Panel target = new Panel();
            target.ID = "samldisplay";
            target.CssClass = "stepContent";
            _pgContainer.Controls.Add(target);

            // Activation du journalisation de la connexion
            RenderRowSeparator(target);


            // Definition des url du service 
            RenderRowSeparator(target);

            #region URL SSO
            var def = new { b = Pref.DatabaseUid, a = (int)eLibConst.AuthenticationMode.SAML2, c = _ePref.LoginId };
            //Si connexion via EudoAdmin, le loginid n'est pas renseigné.            
            string baseUrl = eModelTools.GetBaseUrlXRM() + "sso/" + ExternalUrlTools.GetCryptEncode(JsonConvert.SerializeObject(def));
            if (_ePref.LoginId == 0)
                baseUrl = eResApp.GetRes(_ePref, 2466);


            string sAction = "nsAdminField.CopyTextToClipBoard('" + baseUrl + "')";
            RenderRowLabel("SSOURL", "SSOURL", baseUrl, target, "icon-files-o", sAction);

            #endregion

            RenderRowSeparator(target);
            RenderTabsOption("IdentityProviderMetadata", "IdentityProviderMetadata", target);
            RenderRowSeparator(target);


            // Definition des url du service 
            RenderRowSeparator(target);
            RenderRowLabel("ServiceProviderName", "ServiceProviderName", config.ServiceName, target);
            RenderRowLabel("ServiceProviderIssuer", "ServiceProviderIssuer", config.ServiceProviderIssuer, target);
            // RenderRowLabel("AllowedAudienceUri", "AllowedAudienceUri", config.AllowedAudienceUri, target);
            RenderRowLabel("AssertionConsumerUrl", "AssertionConsumerUrl", config.ServiceProviderIssuer + "/login", target);

            // Definition des urls de l'authorité avec activation d'authentification 
            RenderRowSeparator(target);
            RenderRowLabel("IdentityProviderIssuer", "IdentityProviderIssuer", config.IdentityProviderIssuer, target);
            RenderRowLabel("SignOnDestination", "SignOnDestination", config.SignOnDestination, target);

            RenderRowText("SigningCertificate", "SigningCertificate", config.SigningCertificate, target);
            // Définition les methodes du binding 
            RenderRowSeparator(target);

            string sSignOnDestinationBinding = null;
            if (config != null && config.SignOnDestinationBinding != null)
            {
                if (config.SignOnDestinationBinding.Contains("HTTP-POST"))
                    sSignOnDestinationBinding = ProtocolBindings.HttpPost.ToString();
                else
                    sSignOnDestinationBinding = ProtocolBindings.HttpRedirect.ToString();
            }
            else
                sSignOnDestinationBinding = ProtocolBindings.HttpPost.ToString();
            RenderRowOptions("SignOnDestinationBinding", "SignOnDestinationBinding", sSignOnDestinationBinding , ProtocolSupported, target);
            
            RenderRowLabel("ConsumerBinding", "ConsumerBinding", string.Concat("HTTP-POST", ", ", "HTTP-Redirect"), target);

            RenderRowSeparator(target);

            RenderRowBool("AutoCreateUser", "AutoCreateUser", config.AutoCreateUser, target);

            // Définition du mapping pour identification
            RenderRowSeparator(target);
            RenderRowObjects("MappingAttributes", "MappingAttributes", config.MappingAttributes, 3, target);


            RenderRowSeparator(target);
            if (this.certificate != null && this.certificate.HasPrivateKey)
            {
                RenderRowBool("SignRequest", "SignRequest", config.SignRequest, target);
            }
            // Si le certificat est en erreur on affiche le message dans l'interface admin
            else if (!string.IsNullOrWhiteSpace(this.certificateError))
            {
                RenderRowLabel("SignRequest", "SignRequest", this.certificateError, target);
            }

            RenderRowOptions("SignAlgorithm", "SignAlgorithm", config.SignAlgorithm, eSignAlgorithm.Supported, target);

            RenderRowSeparator(target, 2);
            RenderBtn("ServiceProviderMetadata", "ServiceProviderMetadata", target);

            // Option pour superadmin
            if (_ePref.User.UserLevel >= (int)UserLevel.LEV_USR_SUPERADMIN)
            {
                RenderRowSeparator(target, 2);
                RenderRowBool("AuditLogging", "AuditLogging", config.AuditLogging, target);

                RenderRowSeparator(target);
                RenderRowMultiText("TraceText", "TraceText", logger.ReadTrace(), target);
            }
            RenderRowSeparator(target, 4);

            return true;
        }

        /// <summary>
        /// Export la clé public du certificat
        /// </summary>
        /// <returns>format pem sinon vide s'il y a une erreur</returns>
        private string ExportPublicKey()
        {
            try
            {
                using (TextWriter writer = new StringWriter())
                {
                    using (RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)certificate.PrivateKey)
                    {
                        AsymmetricCipherKeyPair keypair = DotNetUtilities.GetRsaKeyPair(rsa);
                        PemWriter pem = new PemWriter(writer);
                        pem.WriteObject(keypair.Public);
                        pem.Writer.Flush();
                        return writer.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                certificateError = string.Concat(ex.Message, "\n", ex.StackTrace);
                return string.Empty;
            }
        }

        private void RenderTabsOption(string id, string label, Panel target)
        {
            HtmlGenericControl tabs = new HtmlGenericControl("div");
            tabs.Attributes.Add("class", "field");

            string tabHtml = $@"               
                    <label class='labelField optionField' title='{helpRes[id].Title.Replace("'", "&#39;")}'>{helpRes[id].Label}</label>
                    <div id='saml-tab-btns' >
                        <div>
                          <input name='MetadataToUpload' id='MetadataToUpload' onchange='nsAdmin.Saml.uploadMetadata(this);' style='position: absolute; left: -2000em;' type='file' /> 
                          <label val='idp-metadata' title='{eResApp.GetRes(_ePref, 1931).Replace("'", "&#39;")}' for='MetadataToUpload'><span class='icon-upload'> </span>{eResApp.GetRes(_ePref, 1929)}</label>
                          <label val='reset-metadata' title='{eResApp.GetRes(_ePref, 1933).Replace("'", "&#39;")}' onclick ='nsAdmin.Saml.resetConfig();'><span class='icon-trash'> </span></label>
                          </div>
                    </div>
                ";
            tabs.InnerHtml = tabHtml;
            target.Controls.Add(tabs);
        }

        private void RenderBtn(string id, string label, Panel target)
        {
            HtmlGenericControl tabs = new HtmlGenericControl("div");
            tabs.Attributes.Add("class", "field");

            string tabHtml = $@"               
                    <label class='labelField optionField' title='{helpRes[id].Title.Replace("'", "&#39;")}'>{helpRes[id].Label}</label>
                    <div id='saml-tab-btns' >
                        <div>
                              <label val='sp-metadata' title='{eResApp.GetRes(_ePref, 1932).Replace("'", "&#39;")}'><a id='sp-metadata-link' style='color:#fff;' target='_blank' href = './sp/sp-metadata.xml' ><span class='icon-download'> </span>{eResApp.GetRes(_ePref, 1930)}</a></label>                              
                        </div>
                    </div>
                ";
            tabs.InnerHtml = tabHtml;
            target.Controls.Add(tabs);
        }

        /// <summary>
        /// Faire un rendu de l'etat de la config dans un input caché
        /// </summary>
        /// <param name="jsonValue"></param>
        /// <param name="container"></param>
        private void RenderHiddenState(string jsonValue, Panel container)
        {
            Panel hidden = new Panel();
            hidden.Attributes.Add("style", "display:none;");
            container.Controls.Add(hidden);

            // Génération d'un composant caché pour etre peuplé avec le js
            AddTextboxOptionField(hidden
                  , id: "samlsettings"
                  , label: ""
                  , tooltip: ""
                  , propCat: eAdminUpdateProperty.CATEGORY.CONFIGADV
                  , propKeyCode: (int)eLibConst.CONFIGADV.AUTHENTICATION_SETTINGS
                  , propKeyType: eLibConst.CONFIGADV.AUTHENTICATION_SETTINGS.GetType()
                  , currentValue: jsonValue
                  , adminFieldType: AdminFieldType.ADM_TYPE_CHAR,
                  onChange: "return false;"
                 );
        }


        /// <summary>
        /// Creation du séparateur horizontal
        /// </summary>
        /// <param name="container"></param>
        /// <param name="times"> le nombre de fois le séparateur sera répité</param>
        private static void RenderRowSeparator(Panel container, int times = 1)
        {
            if (times < 1)
                return;

            for (int i = 0; i < times; i++)
            {
                Panel ctn = new Panel();
                ctn.CssClass = "field";
                container.Controls.Add(ctn);
                HtmlGenericControl span = new HtmlGenericControl("label");
                span.Attributes.Add("class", "labelField optionField");
                ctn.Controls.Add(span);
            }
        }

        /// <summary>
        /// Faire un rendu avec label : textarea
        /// </summary>
        /// <param name="id"></param>
        /// <param name="label"></param>
        /// <param name="value"></param>
        /// <param name="container"></param>
        private void RenderRowMultiText(string id, string label, string value, Panel container)
        {
            Panel ctn = new Panel();
            ctn.CssClass = "field";
            container.Controls.Add(ctn);

            HtmlGenericControl span = new HtmlGenericControl("label");
            span.InnerText = helpRes[id].Label;
            span.Attributes.Add("title", helpRes[id].Title);
            span.Attributes.Add("class", "labelField optionField");
            span.Style.Add(HtmlTextWriterStyle.VerticalAlign, "top");
            ctn.Controls.Add(span);

            HtmlTextArea text = new HtmlTextArea();
            text.Value = string.IsNullOrEmpty(value) ? "" : value;
            // text.Attributes.Add("onchange", "nsAdmin.Saml.setValue('" + id + "', this.value);");
            text.Attributes.Add("cols", "500");
            text.Attributes.Add("readonly", "true");
            text.Attributes.Add("style", "width:500px;height:300px;overflow:auto;resize:none;white-space:pre;");

            ctn.Controls.Add(text);
        }

        /// <summary>
        /// Faire un rendu avec label : textarea
        /// </summary>
        /// <param name="id"></param>
        /// <param name="label"></param>
        /// <param name="value"></param>
        /// <param name="container"></param>
        private void RenderTable(string id, string label, string value, Panel container)
        {
            Panel ctn = new Panel();
            ctn.CssClass = "field";
            container.Controls.Add(ctn);

            HtmlGenericControl span = new HtmlGenericControl("label");
            span.InnerText = helpRes[id].Label;
            span.Attributes.Add("title", helpRes[id].Title);
            span.Attributes.Add("class", "labelField optionField");
            span.Style.Add(HtmlTextWriterStyle.VerticalAlign, "top");
            ctn.Controls.Add(span);

            HtmlGenericControl table = new HtmlGenericControl("div");
            table.InnerHtml = value;

            table.Attributes.Add("style", "width:475px;height:400px;overflow:auto;resize:none;white-space:pre;");

            ctn.Controls.Add(table);
        }

        /// <summary>
        /// Faire un rendu avec label : objet
        /// </summary>
        /// <param name="id"></param>
        /// <param name="label"></param>
        /// <param name="fields"></param>
        /// <param name="maxField"></param>
        /// <param name="target"></param>
        private void RenderRowObjects(string id, string label, List<eSaml2Map> fields, int maxField, Panel target)
        {
            if (fields.Count < maxField)
                while (fields.Count < maxField)
                    fields.Add(new eSaml2Map() { Saml2Attribute = string.Empty, DescId = "0" });

            for (int i = 0; i < fields.Count; i++)
            {
                RenderRowHeaderOptions(id, i, "Saml2Attribute", "DescId", label, fields[i],
                    new Dictionary<int, string>()
                    {    {0, "------" },
                         {(int)UserField.LOGIN ,  "User.UserLogin" },
                         {(int)UserField.EMAIL ,  "User.UserMail" },
                         {(int)UserField.NAME ,  "User.UserName" },
                         {(int)UserField.FUNCTION ,  "User.Function" },
                         {(int)UserField.LEVEL , "User.UserLevel" },
                         {(int)UserField.GroupId , "User.UserGroupId" },
                         {(int)UserField.UserLoginExternal , "User.UserExternalLogin" }
                    }, target);

                if (i == 0)
                    label = null;
            }
        }

        /// <summary>
        /// Faire un rendu de l'objet
        /// </summary>
        /// <param name="id"></param>
        /// <param name="index"></param>
        /// <param name="attrId"></param>
        /// <param name="descId"></param>
        /// <param name="label"></param>
        /// <param name="f"></param>
        /// <param name="values"></param>
        /// <param name="container"></param>
        private void RenderRowHeaderOptions(string id, int index, string attrId, string descId, string label, eSaml2Map f, Dictionary<int, string> values, Panel container)
        {

            Panel ctn = new Panel();
            ctn.CssClass = "field";
            container.Controls.Add(ctn);

            HtmlGenericControl span = new HtmlGenericControl("label");
            if (!string.IsNullOrEmpty(label))
            {
                span.Attributes.Add("title", helpRes[id].Title);
                span.InnerText = helpRes[id].Label;
            }

            span.Attributes.Add("class", "labelField optionField");
            ctn.Controls.Add(span);

            HtmlGenericControl select = new HtmlGenericControl("select");
            select.ID = descId + "_" + index;
            select.Attributes.Add("onchange", "nsAdmin.Saml.map(" + index + ");");
            select.Attributes.Add("style", "height:22px; margin-right: 5px; margin-top: 3px; width: 196px;");

            foreach (var item in values)
            {
                HtmlGenericControl option = new HtmlGenericControl("option");
                option.Attributes.Add("value", item.Key.ToString());
                option.InnerHtml = item.Value;
                if (f.DescId.Equals(item.Key.ToString()))
                    option.Attributes.Add("selected", "true");

                select.Controls.Add(option);
            }

            ctn.Controls.Add(select);

            HtmlInputText text = new HtmlInputText();
            text.ID = attrId + "_" + index;
            text.Value = f.Saml2Attribute;
            text.Attributes.Add("onchange", "nsAdmin.Saml.map(" + index + ");");
            text.Style.Add(HtmlTextWriterStyle.Width, "200px");
            ctn.Controls.Add(text);

            eCheckBoxCtrl checkbox = new eCheckBoxCtrl(f.IsKey, false);
            checkbox.Attributes.Add("style", "margin-left: 5px;");
            checkbox.ID = "IsKey_" + index;
            checkbox.AddText("IsKey");
            checkbox.AddClick("nsAdmin.Saml.map(" + index + ");");
            ctn.Controls.Add(checkbox);

        }

        /// <summary>
        /// Faire un rendu label : select
        /// </summary>
        /// <param name="id"></param>
        /// <param name="label"></param>
        /// <param name="value"></param>
        /// <param name="values"></param>
        /// <param name="container"></param>
        private void RenderRowOptions(string id, string label, string value, IEnumerable<string> values, Panel container)
        {
            Panel ctn = new Panel();
            ctn.CssClass = "field";
            container.Controls.Add(ctn);

            HtmlGenericControl span = new HtmlGenericControl("label");
            span.Attributes.Add("title", helpRes[id].Title);
            span.InnerText = helpRes[id].Label;
            span.Attributes.Add("class", "labelField optionField");
            ctn.Controls.Add(span);

            HtmlGenericControl select = new HtmlGenericControl("select");
            select.Attributes.Add("style", "height:22px; margin-right: 5px; margin-top: 3px; width: 50%;");
            select.Attributes.Add("onchange", "nsAdmin.Saml.setValue('" + id + "', this.options[this.selectedIndex].value);");
            foreach (string op in values)
            {
                HtmlGenericControl option = new HtmlGenericControl("option");
                option.Attributes.Add("value", op);
                option.InnerHtml = op;
                if (op.Equals(value))
                    option.Attributes.Add("selected", "true");

                select.Controls.Add(option);
            }

            ctn.Controls.Add(select);
        }

        /// <summary>
        /// Faire un rendu label : input
        /// </summary>
        /// <param name="id"></param>
        /// <param name="label"></param>
        /// <param name="value"></param>
        /// <param name="container"></param>
        private void RenderRowText(string id, string label, string value, Panel container)
        {
            Panel ctn = new Panel();
            ctn.CssClass = "field";
            container.Controls.Add(ctn);

            HtmlGenericControl span = new HtmlGenericControl("label");
            span.Attributes.Add("title", helpRes[id].Title);
            span.InnerText = helpRes[id].Label;
            span.Attributes.Add("class", "labelField optionField");
            ctn.Controls.Add(span);

            HtmlInputText text = new HtmlInputText();
            text.Value = string.IsNullOrEmpty(value) ? "" : value;
            text.Attributes.Add("onchange", "nsAdmin.Saml.setValue('" + id + "', this.value);");
            text.Style.Add(HtmlTextWriterStyle.Width, "475px");
            ctn.Controls.Add(text);

        }

        /// <summary>
        /// Faire un rendu label : Label
        /// </summary>
        /// <param name="id"></param>
        /// <param name="label"></param>
        /// <param name="value"></param>
        /// <param name="container"></param>
        /// <param name="sIcon">Icon a gauche dy l</param>
        /// <param name="sAction">Action au click</param>
        private void RenderRowLabel(string id, string label, string value, Panel container, string sIcon = "", string sAction="")
        {
            Panel ctn = new Panel();
            ctn.CssClass = "field";
            container.Controls.Add(ctn);

            HtmlGenericControl span = new HtmlGenericControl("label");
            span.Attributes.Add("title", helpRes[id].Title);
            span.InnerText = helpRes[id].Label;
            span.Attributes.Add("class", "labelField optionField");
            ctn.Controls.Add(span);

            HtmlGenericControl lab = new HtmlGenericControl("label");
            lab.InnerHtml = string.IsNullOrEmpty(value) ? "" : value;
            lab.Attributes.Add("style", "width:400px;font-style: italic;user-select:all;");
            ctn.Controls.Add(lab);

            if (!string.IsNullOrEmpty(sIcon))
            {

                sIcon += " icon-left-spacing";
                HtmlGenericControl lbl = new HtmlGenericControl("span");

                 
                lbl.Attributes.Add("class", sIcon);            
                ctn.Controls.Add(lbl);

                if (!string.IsNullOrEmpty(sAction))
                {
                    lbl.Attributes.Add("onclick", sAction);
                }
            }
        }

        /// <summary>
        /// Faire un rendu label : Label
        /// </summary>
        /// <param name="id"></param>
        /// <param name="label"></param>
        /// <param name="value"></param>
        /// <param name="container"></param>
        private void RenderRowLabelUrl(string id, string label, string value, Panel container)
        {
            value = string.Concat("<a href='", value, "' target='_blank'>", value, "</a>");
            RenderRowLabel(id, label, value, container);
        }

        /// <summary>
        /// Faire un rendu case à coché : label
        /// </summary>
        /// <param name="id"></param>
        /// <param name="label"></param>
        /// <param name="value"></param>
        /// <param name="container"></param>
        private void RenderRowBool(string id, string label, bool value, Panel container)
        {
            Panel ctn = new Panel();
            ctn.CssClass = "field";
            container.Controls.Add(ctn);


            HtmlGenericControl span = new HtmlGenericControl("label");
            span.Attributes.Add("class", "labelField optionField");
            ctn.Controls.Add(span);

            eCheckBoxCtrl checkbox = new eCheckBoxCtrl(value, false);
            checkbox.ID = "cbo_" + id;
            checkbox.AddText(helpRes[id].Label);
            checkbox.ToolTip = helpRes[id].Title;
            checkbox.AddClick("nsAdmin.Saml.setValue('" + id + "', getAttributeValue(this, 'chk') == '1');");
            ctn.Controls.Add(checkbox);

        }

        /// <summary>
        /// Conteneur grlobal
        /// </summary>
        public override Panel PgContainer
        {
            get
            {
                return base.PgContainer;
            }
        }

        /// <summary>
        /// Fait un rendu html du control PgContainer
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            PgContainer.RenderControl(new HtmlTextWriter(new StringWriter(sb)));
            return sb.ToString();
        }

        /// <summary>
        /// Structure pour enmbarquer les infos sur saml
        /// </summary>
        private struct SamlItemRes
        {
            public string Label;
            public string Title;
        }

        /// <summary>
        ///  Si la clé n'est dispo on retourne l'id
        /// </summary>
        private class HelpRes : Dictionary<string, SamlItemRes>
        {
            /// <summary>
            /// Si la clé n'est dipo on retourne un truc par défaut pour que l'appli continue a fonctionner
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public new SamlItemRes this[string key]
            {
                get
                {
                    if (base.ContainsKey(key))
                        return base[key];

                    return new SamlItemRes() { Label = key, Title = $"{key}-KeyNotFound" };
                }
                set
                {
                    base[key] = value;
                }
            }
        }
    }
}
