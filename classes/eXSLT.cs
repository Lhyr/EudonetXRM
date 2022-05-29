using System;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace Com.Eudonet.Xrm
{
    /// <className>eXSLT</className>
    /// <summary>Classe de génération du menu utilisateur</summary>    
    /// <authors>SPH/JBE</authors>
    /// <date>2011-09-30</date>
    static public class eXSLT
    {

        static string _xslUserMenu = AppDomain.CurrentDomain.BaseDirectory + @"themes\default\xsl\menuxsl.xsl";
        static string _xslNavBar = AppDomain.CurrentDomain.BaseDirectory + @"themes\default\xsl\navbarxsl.xsl";
        static string _xslMainList = AppDomain.CurrentDomain.BaseDirectory + @"themes\default\xsl\mainlistxsl.xsl";


        /// <summary>
        /// Retourne l'objet XML avec la navbar en HTML
        /// </summary>
        /// <returns></returns>
        public static string NavBarHTML(XmlDocument _xTransform)
        {

            StringBuilder _sbNavBar = new StringBuilder();
            XslCompiledTransform xslt = new XslCompiledTransform();
            xslt.Load(_xslNavBar);

            XmlWriter _writer = XmlWriter.Create(_sbNavBar, xslt.OutputSettings);
            xslt.Transform(_xTransform, null, _writer);

            return _sbNavBar.ToString();
        }

        /// <summary>
        /// Retourne l'objet XML avec le USER MENU en HTML
        /// </summary>
        /// <returns></returns>
        public static string UserMenuHTML(XmlDocument _xTransform)
        {
            StringBuilder _sbUserMenu = new StringBuilder();
            XslCompiledTransform xslt = new XslCompiledTransform();
            xslt.Load(_xslUserMenu);

            XmlWriter _writer = XmlWriter.Create(_sbUserMenu, xslt.OutputSettings);
            xslt.Transform(_xTransform, null, _writer);

            return _sbUserMenu.ToString();
        }


        /// <summary>
        /// Retourne l'objet XML de la liste principale en HTML
        /// </summary>
        /// <returns></returns>
        public static string MainListHTML(XmlDocument _xTransform)
        {
            StringBuilder _sbMainList = new StringBuilder();
            XslCompiledTransform xslt = new XslCompiledTransform();
            xslt.Load(_xslMainList);

 
          
            XmlWriter _writer = XmlWriter.Create(_sbMainList, xslt.OutputSettings);
            xslt.Transform(_xTransform, null, _writer);

            return _sbMainList.ToString();
        }


    }
}