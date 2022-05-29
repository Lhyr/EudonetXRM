using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    public class eBkmDiscCommRenderer : eRenderer
    {
        eBkmDiscComm bkmComm;

        public eBkmDiscCommRenderer(ePref ePref, eBkmDiscComm bkm)
        {
            Pref = ePref;
            bkmComm = bkm;
        }


        protected override bool Build()
        {
            _pgContainer = eBkmDiscCommRenderer.GetComm(Pref, bkmComm.Record);
            return true;
        }

        public static Panel GetComm(ePref pref, eRecord rec, DiscCustomParam discCustomFields = null)
        {

            Panel pnComm, pnPen, pnDel, pnUserDate, pnNotes;
            String sUserDate, sNotes;
            eFieldRecord fldUser, fldDate, fldNotes;
            HtmlImage imgAvatar;
            Int32 iUserId = 0;

            if (discCustomFields == null)
            {
                String sListCols = pref.GetPrefDefault(rec.ViewTab, new String[] { "DISCCUSTOMFIELDS" })["DISCCUSTOMFIELDS"];
                discCustomFields = new DiscCustomParam(sListCols, rec.ViewTab);
            }

            if (discCustomFields.Error.Length > 0)
            {
                throw new Exception(String.Concat("Signet de type discussion : ", discCustomFields.Error));
            }

            fldUser = rec.GetFieldByAlias(String.Concat(rec.ViewTab, "_", discCustomFields.UserDescId));
            fldDate = rec.GetFieldByAlias(String.Concat(rec.ViewTab, "_", discCustomFields.DateDescId));
            fldNotes = rec.GetFieldByAlias(String.Concat(rec.ViewTab, "_", discCustomFields.NotesDescId));

            if (fldDate == null || fldUser == null || fldNotes == null)
            {
                throw new Exception("Signet de type discussion : Champs Créé par, Créé le ou Notes manquant(s)");
            }

            Int32.TryParse(fldUser.Value, out iUserId);

            imgAvatar = new HtmlImage();
            imgAvatar.Src = eImageTools.GetAvatar(pref, true, iUserId);
            imgAvatar.Attributes.Add("class", "memoAvatar");

            sUserDate = String.Concat(fldUser.DisplayValue, " ", fldDate.DisplayValue, " :");
            sNotes = eLibTools.RemoveHTML(fldNotes.Value).Replace("\n", "<br/>");

            pnComm = new Panel();
            pnComm.CssClass = "discComm";

            pnPen = new Panel();
            pnDel = new Panel();
            if (fldNotes.RightIsUpdatable && iUserId == pref.UserId)
            {
                pnPen.CssClass = "icon-edn-pen discPen";
                pnComm.Controls.Add(pnPen);
                pnPen.Attributes.Add("onclick", "edit(this);");

                if (rec.RightIsDeletable)
                {
                    pnDel.CssClass = "icon-delete discDel";
                    pnComm.Controls.Add(pnDel);
                    pnDel.Attributes.Add("onclick", "del(this);");
                }

            }

            pnUserDate = new Panel();
            pnUserDate.Controls.Add(imgAvatar);
            pnUserDate.Controls.Add(new LiteralControl(sUserDate));
            pnComm.Controls.Add(pnUserDate);
            pnUserDate.CssClass = "discUsrDate";

            pnNotes = new Panel();
            pnNotes.ID = eTools.GetFieldValueCellId(rec, fldNotes);
            pnNotes.Controls.Add(new LiteralControl(sNotes));
            pnComm.Controls.Add(pnNotes);
            pnNotes.CssClass = "discNotes";

            if (fldNotes.RightIsUpdatable)
            {
                pnNotes.Attributes.Add("ename", String.Concat("bkm_", rec.ViewTab));
                pnNotes.Attributes.Add("html", fldNotes.FldInfo.IsHtml ? "1" : "0");
                pnNotes.Attributes.Add("efld", "1");
                pnPen.Attributes.Add("eacttg", pnNotes.ID);
            }

            pnComm.Attributes.Add("fid", rec.MainFileid.ToString());


            return pnComm;
        }

    }
}