using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminGroupDialogRenderer : eAdminModuleRenderer
    {
        int _groupId = -1;
        int _action = 0;
        eGroup _group = null;
        List<eGroup> _groupList = new List<eGroup>();

        public int GroupId
        {
            get
            {
                return _groupId;
            }

            set
            {
                _groupId = value;
            }
        }

        public int Action
        {
            get
            {
                return _action;
            }

            set
            {
                _action = value;
            }
        }

        private eAdminGroupDialogRenderer(ePref pref, int groupId, int action) : base(pref)
        {
            Pref = pref;
            GroupId = groupId;
            Action = action;
        }

        public static eAdminGroupDialogRenderer CreateAdminGroupDialogRenderer(ePref pref, int groupId, int action)
        {
            return new eAdminGroupDialogRenderer(pref, groupId, action);
        }

        protected override bool Init()
        {
            string strError = String.Empty;
            _groupList = eGroup.GetGroupList(Pref);

            if (GroupId > -1)
                _group = _groupList.Find(delegate (eGroup group) { return group.GroupId == GroupId; });

            if (_group == null)
                _group = new eGroup(-1, String.Empty, String.Empty, false, 0);

            return base.Init();
        }

        protected override bool Build()
        {
            _pgContainer.ID = "GroupModalContent";
            _pgContainer.Attributes.Add("class", "adminModalContent");

            // Libellé d'entête précisant le contexte
            string sectionLabel = _action == 0 ? eResApp.GetRes(Pref, 7571) : eResApp.GetRes(Pref, 7572).Replace("<ITEM>", _group.GroupName); // Nouveau groupe / Edition du groupe <ITEM>
            Panel dialogContents = GetModuleSection("groupEdit", sectionLabel, 0);
            Panel targetContainer = null;
            if (dialogContents.Controls.Count > 0 && dialogContents.Controls[dialogContents.Controls.Count - 1] is Panel)
                targetContainer = (Panel)dialogContents.Controls[dialogContents.Controls.Count - 1];
            if (targetContainer == null)
                targetContainer = _pgContainer;
            else
                _pgContainer.Controls.Add(dialogContents);

            // GroupID à éditer
            HtmlInputHidden hiddenId = new HtmlInputHidden();
            hiddenId.ID = "groupId";
            hiddenId.Value = (_action == 0 ? "0" : GroupId.ToString()); // si AJOUT : pas de GroupID
            targetContainer.Controls.Add(hiddenId);

            // GroupID du groupe parent actuel - Mémorisé pour avertir l'utilisateur en cas de changement
            HtmlInputHidden hiddenCurrentParentGroupId = new HtmlInputHidden();
            hiddenCurrentParentGroupId.ID = "currentParentGroupId";
            hiddenCurrentParentGroupId.Value = "0"; // sera réaffecté plus bas
            targetContainer.Controls.Add(hiddenCurrentParentGroupId);

            // Action demandée (0 = AJOUT, 1 = MODIFICATION, 2 = SUPPRESSION)
            HtmlInputHidden hiddenAction = new HtmlInputHidden();
            hiddenAction.ID = "action";
            hiddenAction.Value = Action.ToString();
            targetContainer.Controls.Add(hiddenAction);

            // Nom du groupe
            HtmlInputText input = new HtmlInputText();
            input.ID = "inputGroupName";
            input.Attributes.Add("class", "selectionField");
            input.MaxLength = 255;
            input.Value = _group.GroupName.Trim();
            input.Attributes.Add("title", eResApp.GetRes(Pref, 7573)); // Nom du groupe d'utilisateur affiché dans les listes
            if (_action == 0)
            {
                if (input.Value.Length > 0)
                    input.Value = String.Concat(input.Value, eResApp.GetRes(Pref, 7574)); // " (Sous-groupe)"
                else
                    input.Value = eResApp.GetRes(Pref, 7571); // Nouveau groupe
            }
            targetContainer.Controls.Add(GetLabelField(eResApp.GetRes(Pref, 7575), input)); // Groupe

            // Groupe parent
            DropDownList ddl = new DropDownList();
            ddl.ID = "ddlParentGroup";



            ddl.CssClass = "selectionField";
            ddl.Items.Add(new ListItem(eResApp.GetRes(Pref, 7576), "0"));  // <Racine>
            ddl.ToolTip = eResApp.GetRes(Pref, 7577); // Groupe de rattachement
            foreach (eGroup group in _groupList)
                // en mode MODIFICATION, on n'ajoute pas le groupe en cours parmi les propositions de déplacement vers un autre groupe parent
                if (_action == 0 || group != _group)
                {
                    // Proposition : préfixer le nom du groupe d'un indicateur visuel permettant de matérialiser sa hiérarchie (ex : |...|...R&D)
                    // Non retenu pour le moment, car cela empêche de sélectionner une entrée dans la combobox en saisissant les premières lettres au clavier
                    string formattedGroupLevelName = group.GroupName;
                    /*
                    string textPadding = "|...";
                    int groupSubLevels = (int)Math.Floor((decimal)group.GroupLevel.ToString().Length / 4);
                    for (int i = 0; i < groupSubLevels; i++)
                        formattedGroupLevelName = String.Concat(textPadding, formattedGroupLevelName);
                    */
                    ddl.Items.Add(new ListItem(formattedGroupLevelName, group.GroupId.ToString()));
                }
            // MODIFICATION : on recherche le parent du groupe en cours d'édition en fonction de son GroupLevel
            if (_action == 1)
            {
                eGroup parentGroup = _groupList.Find(existingGroup => existingGroup.GroupLevel == _group.GroupLevel.Remove(_group.GroupLevel.Length - 4));
                if (parentGroup != null)
                {
                    ddl.SelectedIndex = ddl.Items.IndexOf(ddl.Items.FindByValue(parentGroup.GroupId.ToString()));
                    ddl.Attributes.Add("oldval", parentGroup.GroupId.ToString());
                }
            }
            // AJOUT : on utilise le GroupID passé en paramètre, correspondant au groupe sélectionné lors du clic sur +
            else
                ddl.SelectedIndex = ddl.Items.IndexOf(ddl.Items.FindByValue(_groupId.ToString()));
            ddl.Attributes.Add("active", "1");
            targetContainer.Controls.Add(GetLabelField(eResApp.GetRes(Pref, 7578), ddl)); // Groupe parent
            // On mémorise le GroupID du parent actuel, pour afficher un message d'avertissement côté JavaScript en cas de réaffectation
            hiddenCurrentParentGroupId.Value = ddl.SelectedItem.Value;

            // Groupe public
            Boolean bChecked = _group.GroupPublic;
            Boolean bDisabled = false;
            targetContainer.Controls.Add(GetCheckBox("chkGroupPublic", eResApp.GetRes(Pref, 7579), String.Empty, bChecked, bDisabled, null)); // Groupe public

            return base.Build();
        }
    }
}