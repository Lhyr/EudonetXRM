using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Description résumée de eChkDbl
    /// </summary>
    public class eChkDblMgr : eEudoManager
    {

        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void ProcessManager()
        {
            _xmlResult = new XmlDocument();
            XmlNode maintNode = _xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null);
            _xmlResult.AppendChild(maintNode);
            XmlNode xmlRoot = _xmlResult.CreateElement("root");
            _xmlResult.AppendChild(xmlRoot);

            String sMainSearch = "", sFirstNameSearch = "", sPartSearch = "";
            Int32? nTab = 0;
            Int32 nMainFld = 0;
            Boolean bPM = false;

            if (_requestTools.AllKeys.Contains("Tab"))
            {
                nTab = _requestTools.GetRequestFormKeyI("Tab");
                nMainFld = nTab.Value + 1;
                bPM = nTab.Value == TableType.PM.GetHashCode();
            }

            if (_requestTools.AllKeys.Contains(nMainFld.ToString()))
                sMainSearch = _requestTools.GetRequestFormKeyS(nMainFld.ToString());

            if (nTab == TableType.PP.GetHashCode())
            {
                if (_requestTools.AllKeys.Contains("202"))
                    sFirstNameSearch = _requestTools.GetRequestFormKeyS("202");
                if (_requestTools.AllKeys.Contains("203"))
                    sPartSearch = _requestTools.GetRequestFormKeyS("203");

            }


            if (String.IsNullOrEmpty(sMainSearch) || nTab == 0)
                LaunchError(eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 602), eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "")));

            eDataFillerGeneric dtf = new eDataFillerGeneric(_pref, nTab.Value, EudoQuery.ViewQuery.CUSTOM);

            dtf.EudoqueryComplementaryOptions = delegate(EudoQuery.EudoQuery eq)
            {
                String sAdditionalCol = "";

                if (bPM)
                    sAdditionalCol = String.Concat(";395;397");

                eq.SetListCol = String.Concat(nMainFld.ToString(), sAdditionalCol);

                List<WhereCustom> liWc = new List<WhereCustom>();
                liWc.Add(new WhereCustom(nMainFld.ToString(), Operator.OP_EQUAL, sMainSearch));
                if (nTab == TableType.PP.GetHashCode())
                {
                    liWc.Add(new WhereCustom("202", String.IsNullOrEmpty(sFirstNameSearch) ? Operator.OP_IS_EMPTY : Operator.OP_EQUAL, sFirstNameSearch, InterOperator.OP_AND));
                    liWc.Add(new WhereCustom("203", String.IsNullOrEmpty(sPartSearch) ? Operator.OP_IS_EMPTY : Operator.OP_EQUAL, sPartSearch, InterOperator.OP_AND));
                }
                eq.AddCustomFilter(new WhereCustom(liWc, InterOperator.OP_AND));
            };

            dtf.Generate();

            XmlNode xmltab = _xmlResult.CreateElement("tab");
            xmlRoot.AppendChild(xmltab);
            xmltab.InnerText = dtf.ViewMainTable.DescId.ToString();

            XmlNode xmlIcn = _xmlResult.CreateElement("iconstyle");
            xmlRoot.AppendChild(xmlIcn);
            xmlIcn.InnerText = String.Concat("background:url(themes/",
                _pref.ThemePaths.GetImageWebPath("/images/iFileIcon/" + dtf.ViewMainTable.GetIcon.Replace(".jpg", ".png"))
                , ") 0 0 no-repeat !important  ");

            String sResDateCreate = "", sResUserCreate = "";
            if (bPM)
            {
                eRes res = new eRes(_pref, "395");
                sResDateCreate = res.GetRes(395, eResApp.GetRes(_pref, 113));
                sResUserCreate = eResApp.GetRes(_pref, 60);
            }

            foreach (eRecord rec in dtf.ListRecords)
            {
                eFieldRecord fldMain = rec.GetFieldByAlias(String.Concat(nTab.Value, "_", nMainFld));
                eFieldRecord fldDate, fldUser;
                String sLabel = "", sDate = "", sUser = "";

                if (fldMain == null)
                {
                    ErrorContainer = eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, String.Concat("Une Erreur est survenue durant la recherche de doublon sur la table ", nTab.Value, " : Le champ principal est introuvable"));
                    break;
                }

                if (bPM)
                {
                    fldDate = rec.GetFieldByAlias("300_395");
                    if (fldDate != null)
                        sDate = fldDate.DisplayValue;

                    fldUser = rec.GetFieldByAlias("300_397");
                    if (fldUser != null)
                        sUser = fldUser.DisplayValue;

                    sLabel = String.Concat(fldMain.DisplayValue, " ",
                            sResDateCreate, " ", sDate, " ",
                            sResUserCreate, " ", sUser);

                }
                else {
                    sLabel = fldMain.DisplayValue;
                }

                XmlNode xmlRec = _xmlResult.CreateElement("rec");
                xmlRoot.AppendChild(xmlRec);

                XmlAttribute xmlId = _xmlResult.CreateAttribute("id");
                xmlId.InnerText = rec.MainFileid.ToString();
                xmlRec.Attributes.Append(xmlId);

                XmlAttribute xmlLabel = _xmlResult.CreateAttribute("label");
                xmlLabel.InnerText = sLabel;
                xmlRec.Attributes.Append(xmlLabel);


            }
            LaunchError();
            RenderResult(RequestContentType.XML, delegate() { return _xmlResult.OuterXml; });

        }

    }
}