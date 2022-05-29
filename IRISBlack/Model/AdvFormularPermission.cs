using Com.Eudonet.Internal;

namespace Com.Eudonet.Xrm.IRISBlack
{
    public class AdvFormularPermission
    {
        private PermissionMode _permMode;
        private int _permLevel;
        private int _permId;
        private string _permUser;
        private string _permUserDisplay;

        /// <summary>Mode de permission de la permission</summary>
        public PermissionMode PermMode
        {
            get { return _permMode; }
            set { _permMode = value; }
        }
        /// <summary>Niveau de la permission</summary>
        public int PermLevel
        {
            get { return _permLevel; }
            set { _permLevel = value; }
        }
        /// <summary>Identifiant de la permission dans la table Permission</summary>
        public int PermId
        {
            get { return _permId; }
            set { _permId = value; }
        }

        /// <summary>Utilisateur(s) et/ou Groupes concerné(s) par la permission</summary>
        public string PermUser
        {
            get { return _permUser; }
            set { _permUser = value; }
        }

        /// <summary>Libellé Utilisateur(s) et/ou Groupes concerné(s) par la permission</summary>
        public string PermUserDisplay
        {
            get { return _permUserDisplay; }
            set { _permUserDisplay = value; }
        }

        /// <summary>
        /// Constructeur de l'objet permission (transmission de toutes les caractéristiques par le constructeur)
        /// </summary>
        /// <param name="epermission">parametre représentant une permission Eudonet</param>
        public AdvFormularPermission(ePermission epermission)
        {
            _permMode = (PermissionMode)epermission.PermMode;
            _permLevel = epermission.PermLevel;
            _permId = epermission.PermId;
            _permUser = epermission.PermUser;
            _permUserDisplay = epermission.PermUserLabel;
        }
    }
}