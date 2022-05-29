using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <className>eOrganigrammeRenderer</className>
    /// <summary>classe gérant le rendu des organigrammes</summary>
    /// <authors>GCH</authors>
    /// <date>2014-05-05</date>
    public class eOrganigrammeRenderer
    {
        /// <summary>Objet représentant l'organigramme</summary>
        eOrganigramme _org = null;
        /// <summary>descid de la table ou l'on souhaite voir l'organigramme</summary>
        Int32 _nTab = -1;
        ePref _pref;

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pref">preferences de l'utilisateur</param>
        /// <param name="nTabId">Table ou se trouve l'organigramme à réaliser</param>
        /// <param name="nFileId">Id de la fiche de départ</param>
        /// <param name="IsTopLevel">Indique si l'on souhaite que la fiche indiquée soit la plus haute affichée (c'est à dire sans les parents de cette dernière)</param>
        public eOrganigrammeRenderer(ePref pref, Int32 nTabId, Int32 nFileId, Boolean IsTopLevel = false)
        {
            _pref = pref;
            _nTab = nTabId;
            _org = new eOrganigramme(pref, _nTab, nFileId);
        }

        /// <summary>
        /// Initialisation de l'objet métier
        /// </summary>
        /// <param name="sError">erreur détaillée</param>
        /// <returns>à faux si un problème s'est produit</returns>
        public Boolean Load(out String sUserError, out String sError)
        {
            sUserError = String.Empty;
            sError = String.Empty;
            if (!_org.Load(out sError))
            {
                sUserError = eResApp.GetRes(_pref, 8178);
                sError = String.Concat("_org.Load : ", sError);
                return false;
            }
            if (!_org.Build(out sError))
            {
                sUserError = eResApp.GetRes(_pref, 8179);
                sError = String.Concat("_org.Build : ", sError);
                return false;
            }
            return true;
        }
        /// <summary>
        /// Construit le rendu de chaque branche de la liste en paramètre
        /// </summary>
        /// <param name="control">Control auquel on souhaite ajouter le rendu de la liste</param>
        /// <param name="orgItem">liste d'item à organiser (pour le niveau le plus haut, norlement un seul niveau doit être présent</param>
        private void DiplayAllBranch(Control control, List<eOrganigrammeItem> orgItem)
        {
            foreach (eOrganigrammeItem currentOrg in orgItem)
            {
                HtmlGenericControl li = new HtmlGenericControl("li");
                control.Controls.Add(li);
                HtmlGenericControl elem = new HtmlGenericControl("div");
                li.Controls.Add(elem);
                elem.Attributes.Add("onclick", String.Concat("rf(", _nTab, ",", currentOrg.Id, ");"));
                elem.Attributes.Add("class", "elemChart");
                HtmlGenericControl subElem;
                foreach (KeyValuePair<String, String> currentKVP in currentOrg.DisplayValue)
                {
                    subElem = new HtmlGenericControl("span");
                    subElem.Controls.Add(new LiteralControl(currentKVP.Value));
                    elem.Controls.Add(subElem);
                }
                String imgWebPath = currentOrg.ImgWebPath;
                if (imgWebPath.Length > 0)
                {
                    HtmlGenericControl img = new HtmlGenericControl("img");
                    li.Controls.Add(img);
                    img.Attributes.Add("src", String.Concat("./", imgWebPath));
                }
                if (currentOrg.SubItem.Count > 0)
                {
                    HtmlGenericControl ul = new HtmlGenericControl("ul");
                    li.Controls.Add(ul);
                    DiplayAllBranch(ul, currentOrg.SubItem);
                }
            }
        }

        /// <summary>
        /// Retourne le rendu de l'organigramme
        /// </summary>
        /// <returns></returns>
        public Control GetDialogContent()
        {
            HtmlGenericControl divMain = new HtmlGenericControl("div");

            HtmlGenericControl ul = new HtmlGenericControl("ul");
            divMain.Controls.Add(ul);
            ul.ID = "org";
            ul.Style.Add("display", "none");
            List<eOrganigrammeItem> listOrg = new List<eOrganigrammeItem>();
            listOrg.Add(_org.GetMainItem());
            DiplayAllBranch(ul, listOrg);
            HtmlGenericControl div = new HtmlGenericControl("div");
            divMain.Controls.Add(div);
            div.ID = "chart";
            div.Attributes.Add("class", "orgChart");

            return divMain;


            /*
             
    <li>
       Food
       <ul>
         <li id="beer">Beer</li>
         <li>Vegetables
           <a href="http://wesnolte.com" target="_blank">Click me</a>
           <ul>
             <li>Pumpkin</li>
             <li>
                <a href="http://tquila.com" target="_blank">Aubergine</a>
                <p>A link and paragraph is all we need.</p>
             </li>
           </ul>
         </li>
         <li class="fruit">Fruit
           <ul>
             <li>Apple
               <ul>
                 <li>Granny Smith</li>
               </ul>
             </li>
             <li>Berries
               <ul>
                 <li>Blueberry</li>
                 <li><img src="images/raspberry.jpg" alt="Raspberry"></li>
                 <li>Cucumber</li>
               </ul>
             </li>
           </ul>
         </li>
         <li>Bread</li>
         <li class="collapsed">Chocolate
           <ul>
             <li>Topdeck</li>
             <li>Reese's Cups</li>
           </ul>
         </li>
       </ul>
     </li>
   
             
             */


        }


    }
}