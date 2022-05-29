using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Xrm.eda;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe de représentaion du mapping MOBILE mais aussi ADDIN
    /// </summary>
    public class eMobileMapping
    {

        /// <summary>
        /// type d'utilisation du mapping. Comme le mapping MOBILE a été utilisé pour l'addin, on est obligé de faire la distinction
        /// au vu des usages qui en sont fait...
        /// </summary>
        public eLibConst.MOBILEMAPPINGTYPE Tp { get; set; }

        /// <summary>
        /// Clé des champs stockés dans la table MOBILE
        /// </summary>
        public eLibConst.MOBILE Key { get; private set; }
        /// <summary>
        /// Utilisateur concerné par le mapping (par défaut : 0 = mapping pour tous les utilisateurs)
        /// </summary>
        public int UserId { get; private set; }
        /// <summary>
        /// Liste des TabId des fichiers dont les rubriques peuvent être proposées comme mappables
        /// </summary>
        public List<int> Tab { get; private set; }
        /// <summary>
        /// Référence au numéro de ressource de EUDORES.APP à utiliser comme libellé du champ à mapper
        /// </summary>
        public int Field { get; private set; }
        /// <summary>
        /// DescID de la rubrique correspondant au champ mappé
        /// </summary>
        public int DescId { get; private set; }
        /// <summary>
        /// Indique si le champ doit être géré en lecture seule ou non sur l'application mobile
        /// </summary>
        public bool ReadOnly { get; private set; }
        /// <summary>
        /// Indique si le mapping champ peut être personnalisé ou non
        /// </summary>
        public bool Customized { get; private set; }

        public eMobileMapping(eLibConst.MOBILE key, int userId, List<int> tab, int field, int descId, bool readOnly, bool customized)
        {
            Key = key;
            UserId = userId;
            Tab = tab;
            Field = field;
            DescId = descId;
            ReadOnly = readOnly;
            Customized = customized;
        }

        public Int32 MobileMappingExists(eudoDAL eDal, bool checkAllColumns)
        {
            String error = String.Empty;
            int mobileId = -1;

            StringBuilder sb = new StringBuilder("SELECT [MobileId] FROM [MOBILE] WHERE [Key] = @key ");

            if (checkAllColumns)
            {
                if (UserId != -1)
                    sb.Append("AND [UserId] = @userid ");
                if (Tab != null)
                    sb.Append("AND [Tab] = @tab ");
                if (Field != -1)
                    sb.Append("AND [Field] = @field ");
                if (DescId != -1)
                    sb.Append("AND [DescId] = @descid ");
            }

            RqParam rp = new RqParam(sb.ToString());
            SetRqParameters(rp);

            DataTableReaderTuned dtr = eDal.Execute(rp, out error);

            try
            {
                if (error.Length != 0)
                    throw new Exception(String.Concat("eMobileMapping.MobileMappingExists => Requête invalide : ", error));

                if (dtr == null || !dtr.Read())
                    return 0;

                if (String.IsNullOrEmpty(error))
                {
                    mobileId = dtr.GetInt32("MobileId");
                }
            }
            finally
            {
                if (dtr != null)
                    dtr.Dispose();
            }

            return mobileId;
        }

        internal bool ReinitMobileMappingTab(eudoDAL dal, string strSection, int nTab)
        {
            List<eLibConst.MOBILE> lstIsPlanningField = new List<eLibConst.MOBILE>()
            {
                eLibConst.MOBILE.CALENDAR_DATE_BEGIN,
                eLibConst.MOBILE.CALENDAR_DATE_END,
                eLibConst.MOBILE.CALENDAR_ACTION,
                eLibConst.MOBILE.CALENDAR_LOCATION,
                eLibConst.MOBILE.CALENDAR_MEMO,
                eLibConst.MOBILE.CALENDAR_COLOR
            };



            List<eLibConst.MOBILE> lstIsEvt = new List<eLibConst.MOBILE>()
            {
                eLibConst.MOBILE.EVENT_DATE,
                eLibConst.MOBILE.EVENT_NAME
            };


            List<eLibConst.MOBILE> lstIsInvit = new List<eLibConst.MOBILE>()
            {
                eLibConst.MOBILE.EVENT_INVITATION_CONFIRMATION
            };

            List<int> lstKey = new List<int>();
            switch (strSection)
            {
                case "planning":
                    lstKey = lstIsPlanningField.Select(w => (int)w).ToList();
                    break;
                default:
                    //pour l'heurem uniquement planning
                    return true;

            }

            string sSQL =
                    $@"UPDATE [MOBILE]
	                    SET DescId = -1
                    WHERE 
                     [key] in (SELECT ID FROM @LSTDESCID)
                    AND [DescId] - [DESCID] % 100 <> {nTab}
                    ";
            RqParam rqParam = new RqParam(sSQL);
            rqParam.AddInputParamListId("@LSTDESCID", lstKey);

            dal.ExecuteNonQuery(rqParam, out string serror);
            return dal.InnerException == null;
        }

        private void SetRqParameters(RqParam rp)
        {
            List<string> tabList = new List<string>();
            foreach (int tab in this.Tab)
            {
                if (!tabList.Contains(tab.ToString()))
                    tabList.Add(tab.ToString());
            }
            rp.AddInputParameter("@userid", SqlDbType.Int, this.UserId);
            rp.AddInputParameter("@tab", SqlDbType.NVarChar, String.Join(";", tabList.ToArray()));
            rp.AddInputParameter("@field", SqlDbType.Int, this.Field);
            rp.AddInputParameter("@descid", SqlDbType.Int, this.DescId);
            rp.AddInputParameter("@readonly", SqlDbType.Bit, this.ReadOnly);
            rp.AddInputParameter("@customized", SqlDbType.Bit, this.Customized);
            rp.AddInputParameter("@key", SqlDbType.Int, (int)this.Key);
        }

        /// <summary>Met à jour ou ajoute l'entrée dans la table MOBILE</summary>
        /// <param name="eDal">EudoDAL</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public Int32 SaveMobileMapping(eudoDAL eDal)
        {
            String error = String.Empty;

            int mobileId = MobileMappingExists(eDal, false);

            StringBuilder sb = new StringBuilder();
            if (mobileId > 0)
            {
                #region Mise à jour
                sb.Append("UPDATE [MOBILE] SET ");
                if (UserId != -1)
                    sb.Append("[UserId] = @userid, ");
                if (Tab != null)
                    sb.Append("[Tab] = @tab, ");
                if (Field != -1)
                    sb.Append("[Field] = @field, ");
                //if (DescId != -1) // On autorise à vider le mapping des champs
                sb.Append("[DescId] = @descid, ");
                sb.Append("[ReadOnly] = @readonly, ");
                sb.Append("[Customized] = @customized ");
                sb.Append("WHERE [Key] = @key");
                #endregion
            }
            else
            {
                #region Insertion
                sb.Append("INSERT INTO [MOBILE] ([UserId], [Tab], [Field], [DescId], [ReadOnly], [Customized], [Key]) VALUES ");
                sb.Append("(@userid, @tab, @field, @descid, @readonly, @customized, @key); SELECT @mobileid = scope_identity()");
                #endregion
            }

            RqParam rp = new RqParam(sb.ToString());
            SetRqParameters(rp);
            rp.AddOutputParameter("@mobileid", SqlDbType.Int, 0);

            int result = eDal.ExecuteNonQuery(rp, out error);
            if (result > 0 && String.IsNullOrEmpty(error))
            {
                if (mobileId <= 0)
                    mobileId = eLibTools.GetNum(rp.GetParamValue("@mobileid").ToString());
            }
            else
            {
                throw new Exception(String.Concat("eMobileMapping.SaveMobileMapping => Requête invalide : ", error));
            }

            return mobileId;
        }

        /// <summary>Supprime l'entrée dans la table MOBILE</summary>
        /// <param name="eDal">EudoDAL</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public int DeleteMapping(eudoDAL eDal)
        {
            String error = String.Empty;
            int result = 0;

            int mobileId = MobileMappingExists(eDal, true);
            if (mobileId > 0)
            {
                #region Suppression
                StringBuilder sb = new StringBuilder();
                sb.Append("DELETE FROM [MOBILE] ");
                sb.Append("WHERE MobileId = @mobileid");
                #endregion

                RqParam rp = new RqParam(sb.ToString());
                rp.AddInputParameter("@mobileid", SqlDbType.Int, mobileId);
                result = eDal.ExecuteNonQuery(rp, out error);

                if (result == 0 || !String.IsNullOrEmpty(error))
                    throw new Exception(String.Concat("eMobileMapping.DeleteMapping => Requête invalide : ", error));
            }

            return result;
        }

        /// <summary>Récupère la liste des lignes de la table MOBILE</summary>
        /// <param name="pref">Objet ePref</param>
        /// <param name="error">Erreur</param>
        /// <returns></returns>
        public static Dictionary<eLibConst.MOBILE, eMobileMapping> LoadMapping(ePref pref, out String error)
        {
            Dictionary<eLibConst.MOBILE, eMobileMapping> list = new Dictionary<eLibConst.MOBILE, eMobileMapping>();

            eudoDAL edal = null;
            error = String.Empty;
            DataTableReaderTuned dtrFields = null;

            try
            {
                edal = eLibTools.GetEudoDAL(pref);
                edal.OpenDatabase();

                #region Construction de la requête pour récupérer les champs du mapping

                String labelField = pref.Lang;

                String _select =
                String.Concat("SELECT [MobileId], [UserId], [Tab], [Field], [DescId], [ReadOnly], [Customized], [Key] ",
                "FROM [MOBILE] ");
                RqParam rq = new RqParam(_select);
                #endregion

                // Execution de la requete
                dtrFields = edal.Execute(rq, out error);
                if (dtrFields != null)
                {
                    while (dtrFields.Read())
                    {
                        eLibConst.MOBILE key = (eLibConst.MOBILE)dtrFields.GetInt32("Key");

                        List<int> tabList = new List<int>();
                        foreach (string tab in dtrFields.GetString("Tab").Split(';'))
                            tabList.Add(eLibTools.GetNum(tab));

                        eMobileMapping value =
                            new eMobileMapping(
                                key,
                                dtrFields.GetEudoNumeric("UserId"),
                                tabList,
                                dtrFields.GetInt32("Field"),
                                dtrFields.GetEudoNumeric("DescId"),
                                dtrFields.GetBoolean("ReadOnly"),
                                dtrFields.GetBoolean("Customized")
                            );

                        if (!list.ContainsKey(key))
                            list.Add(key, value);
                        else
                            list[key] = value;
                    }
                }
            }
            catch (Exception e)
            {
                error = e.Message;
            }
            finally
            {
                edal.CloseDatabase();
                if (dtrFields != null)
                    dtrFields.Dispose();

            }

            return list;
        }


        /// <summary>
        /// Renvoie la liste des formats de rubriques autorisés à être mappés sur le champ passé en paramètre
        /// et du type de mapping
        /// </summary>
        /// <param name="key">Clé de type eLibConst.MOBILE</param>
        /// <param name="tp"></param>
        /// <returns></returns>
        public static List<FieldFormat> GetAllowedFieldFormats(eLibConst.MOBILE key, eLibConst.MOBILEMAPPINGTYPE tp)
        {
            switch (key)
            {
                case eLibConst.MOBILE.NAME:
                case eLibConst.MOBILE.FIRSTNAME:
                case eLibConst.MOBILE.PARTICLE:
                case eLibConst.MOBILE.CIVILITY:
                case eLibConst.MOBILE.TITLE:
                case eLibConst.MOBILE.COMPANY:
                case eLibConst.MOBILE.STREET_1:
                case eLibConst.MOBILE.STREET_2:
                case eLibConst.MOBILE.STREET_3:
                case eLibConst.MOBILE.POSTALCODE:
                case eLibConst.MOBILE.CITY:
                case eLibConst.MOBILE.COUNTRY:
                case eLibConst.MOBILE.CALENDAR_ACTION:
                case eLibConst.MOBILE.CALENDAR_LOCATION:
                case eLibConst.MOBILE.EVENT_NAME:
                    return new List<FieldFormat>() {
                        FieldFormat.TYP_CHAR,
                        FieldFormat.TYP_NUMERIC
                    };



                case eLibConst.MOBILE.FREE_1:
                case eLibConst.MOBILE.FREE_2:
                case eLibConst.MOBILE.FREE_3:

                    if (tp == eLibConst.MOBILEMAPPINGTYPE.MOBILE)
                    {
                        return new List<FieldFormat>() {
                        FieldFormat.TYP_CHAR,
                        FieldFormat.TYP_NUMERIC,
                        FieldFormat.TYP_COUNT,
                        FieldFormat.TYP_EMAIL,
                        FieldFormat.TYP_BIT,
                        FieldFormat.TYP_GEOGRAPHY_V2,
                        FieldFormat.TYP_MONEY,
                        FieldFormat.TYP_PHONE,
                        FieldFormat.TYP_WEB,
                        FieldFormat.TYP_SOCIALNETWORK
                    };
                    }
                    else if (key == eLibConst.MOBILE.FREE_3)
                    {
                        return new List<FieldFormat>() {

                        FieldFormat.TYP_EMAIL
                        };
                    }
                    else
                        return new List<FieldFormat>();



                case eLibConst.MOBILE.EMAIL:
                    return new List<FieldFormat>() {
                        FieldFormat.TYP_EMAIL
                    };

                // TODO iso v7: restreindre au type Stockage en base ?
                case eLibConst.MOBILE.PHOTO:
                    return new List<FieldFormat>() {
                        FieldFormat.TYP_IMAGE
                    };

                case eLibConst.MOBILE.TEL_FIXED:
                case eLibConst.MOBILE.TEL_MOBILE:
                case eLibConst.MOBILE.TEL_COMPANY:
                    return new List<FieldFormat>() {
                        FieldFormat.TYP_PHONE
                    };

                case eLibConst.MOBILE.CALENDAR_DATE_BEGIN:
                case eLibConst.MOBILE.CALENDAR_DATE_END:
                case eLibConst.MOBILE.EVENT_DATE:
                    return new List<FieldFormat>() {
                        FieldFormat.TYP_DATE
                    };

                case eLibConst.MOBILE.CALENDAR_MEMO:
                    return new List<FieldFormat>() {
                        FieldFormat.TYP_MEMO
                    };

                case eLibConst.MOBILE.CALENDAR_COLOR:
                    return new List<FieldFormat>() {
                        FieldFormat.TYP_CHAR // TOCHECK : à corriger dans le futur si les couleurs sont stockées dans un type de champ spécifique
                    };

                case eLibConst.MOBILE.EVENT_INVITATION_CONFIRMATION:
                    return new List<FieldFormat>() {
                        FieldFormat.TYP_BIT
                    };
            }

            return new List<FieldFormat>();
        }

        /// <summary>
        /// Renvoie la liste des formats de rubriques autorisés à être mappés sur le champ passé en paramètre
        /// </summary>
        /// <param name="key">Clé de type eLibConst.MOBILE</param>
        /// <returns></returns>
        public static List<FieldFormat> GetAllowedFieldFormats(eLibConst.MOBILE key)
        {
            return GetAllowedFieldFormats(key, eLibConst.MOBILEMAPPINGTYPE.MOBILE);
        }

        /// <summary>
        /// Renvoie le ResID du libellé standard à afficher en admin pour le champ MOBILE en question
        /// </summary>
        /// <param name="key">Clé de type eLibConst.MOBILE</param>
        /// <returns>Le libellé correspondant à cette clé</returns>
        public static int GetMobileFieldResId(eLibConst.MOBILE key)
        {
            return GetMobileFieldResId(key, eLibConst.MOBILEMAPPINGTYPE.MOBILE);
        }


        /// <summary>
        ///  Renvoie le ResID du libellé standard à afficher en admin pour le champ MOBILE en question, en fonction du mapping (mobile/addin)
        /// </summary>
        /// <param name="key">Clé de type eLibConst.MOBILE</param>
        /// <param name="tp">Type d'utilisation du mapping</param>
        /// <returns></returns>
        public static int GetMobileFieldResId(eLibConst.MOBILE key, eLibConst.MOBILEMAPPINGTYPE tp)
        {
            switch (key)
            {
                case eLibConst.MOBILE.NAME: return 1514; // Nom
                case eLibConst.MOBILE.FIRSTNAME: return 1589; // Prénom
                case eLibConst.MOBILE.PARTICLE: return 1590; // Particule
                case eLibConst.MOBILE.CIVILITY: return 1591; // Civilité
                case eLibConst.MOBILE.TITLE:
                    return 607; // Fonction
                case eLibConst.MOBILE.EMAIL:
                    if (tp == eLibConst.MOBILEMAPPINGTYPE.MOBILE)
                        return 656; // E-mail
                    else
                        return 656;
                case eLibConst.MOBILE.PHOTO: return 6166; // Photo
                case eLibConst.MOBILE.COMPANY: return 1592; // Société
                case eLibConst.MOBILE.STREET_1: return 1593; // Rue 1
                case eLibConst.MOBILE.STREET_2: return 1594; // Rue 2
                case eLibConst.MOBILE.STREET_3: return 7834; // Rue 3
                case eLibConst.MOBILE.POSTALCODE: return 6106; // Code postal
                case eLibConst.MOBILE.CITY: return 1183; // Ville
                case eLibConst.MOBILE.COUNTRY: return 6104; // Pays
                case eLibConst.MOBILE.TEL_FIXED: return 7835; // Téléphone fixe
                case eLibConst.MOBILE.TEL_MOBILE: return 7836; // Téléphone mobile
                case eLibConst.MOBILE.TEL_COMPANY: return 7837; // Téléphone société
                case eLibConst.MOBILE.CALENDAR_DATE_BEGIN: return 1091; // Date de début
                case eLibConst.MOBILE.CALENDAR_DATE_END: return 1090; // Date de fin
                case eLibConst.MOBILE.CALENDAR_ACTION: return 380; // Action - TOCHECKRES
                case eLibConst.MOBILE.CALENDAR_LOCATION: return 1598; // Lieu
                case eLibConst.MOBILE.CALENDAR_MEMO: return 235; // Mémo - TOCHECKRES
                case eLibConst.MOBILE.CALENDAR_COLOR: return 1505; // Couleur du fond - TOCHECKRES
                case eLibConst.MOBILE.EVENT_DATE: return 7838; // Date de début de l'évènement - TOCHECKRES
                case eLibConst.MOBILE.EVENT_NAME: return 7839; // Nom de l'évènement - TOCHECKRES
                case eLibConst.MOBILE.EVENT_INVITATION_CONFIRMATION: return 7840; // Confirmation de présence à l'évènement - TOCHECKRES
                case eLibConst.MOBILE.FREE_1:
                    if (tp == eLibConst.MOBILEMAPPINGTYPE.MOBILE)
                        return 1595; // Libre 1
                    else
                        return 830; // Table Email

                case eLibConst.MOBILE.FREE_2:
                    if (tp == eLibConst.MOBILEMAPPINGTYPE.MOBILE)
                        return 1596; // Libre 2
                    else
                        return 4142; // Table Planning

                case eLibConst.MOBILE.FREE_3:
                    if (tp == eLibConst.MOBILEMAPPINGTYPE.MOBILE)
                        return 1597; // Libre 3
                    else
                        return 2293; // "Champ email de recherche"
            }

            return 0;
        }

        /// <summary>
        /// Renvoie le libellé à afficher en admin pour le champ MOBILE en question, en fonction du ResId indiqué dans le mapping
        /// </summary>
        /// <returns>Le libellé correspondant à cette clé</returns>
        public int GetMobileFieldResId()
        {
            // MOBILE.Field, en base, correspond au ResId de la ressource à utiliser pour décrire le champ en admin
            // Si cela correspond à un ResId valide, on renvoie donc cette informatio
            if (Field > 0)
            {
                //Toutes cette classe s'appuie sur la version "originale  v5" du mapping qui n'était pas très souple
                //elle "hérite" donc de ce manque de souplesse et de choix d'archi qui ne sont désormais plus appropriée
                // dans l'idéal, il aurait donc fallut refactoriser/revoir cette classe ainsi que les classe l'utilisant.
                // dans l'attente, des moyens de contournemens sont utilisés pour utiliser l'existant.
                // en l'occurence, comme ce vieux mapping a été réutilisé pour un autre usage, la façon de gérer les ressources
                // n'est pas du tout adapté
                if (Tp == eLibConst.MOBILEMAPPINGTYPE.MOBILE)
                    return Field;
                else
                {
                    //"Libre 3" devient "Champ de recherche email" pour l'utilsation ADDIN du mapping
                    if (Field == 1597)
                        return 2325;

                    return Field;
                }
            }
            // Sinon, on renvoie un ResId prédéfini
            else
                return eMobileMapping.GetMobileFieldResId(this.Key, Tp);
        }

        /// <summary>
        /// Indique si le mapping du champ peut être personnalisé
        /// </summary>
        /// <param name="key">Clé correspondant au champ</param>
        /// <returns>true si autorisé, false sinon</returns>
        public static bool CanBeCustomized(ePref pref, eLibConst.MOBILE key)
        {
            // Assouplissement par rapport à la v7 :
            // si l'admin est un super-administrateur, on affiche tous les champs comme mappables afin de lui permettre de corriger
            // d'éventuels mappings corrompus, ce qui était impossible en v7 sans passer par SQL.
            if (pref.User.UserLevel >= UserLevel.LEV_USR_SUPERADMIN.GetHashCode())
                return true;
            else
            {
                bool bCanBeCustomized = true;

                switch (key)
                {
                    case eLibConst.MOBILE.NAME:
                    case eLibConst.MOBILE.FIRSTNAME:
                    case eLibConst.MOBILE.PARTICLE:
                    case eLibConst.MOBILE.COMPANY:
                    case eLibConst.MOBILE.CALENDAR_DATE_BEGIN:
                    case eLibConst.MOBILE.CALENDAR_DATE_END:
                    case eLibConst.MOBILE.TEL_COMPANY:
                        bCanBeCustomized = false;
                        break;
                }

                return bCanBeCustomized;
            }
        }

        /// <summary>
        /// Indique si le mapping du champ peut être personnalisé
        /// </summary>
        /// <param name="pref">Objet Pref (pour vérifier le niveau de l'utilisateur)</param>
        /// <returns>true si autorisé, false sinon</returns>
        public bool CanBeCustomized(ePref pref)
        {
            return CanBeCustomized(pref, Key);
        }
    }
}
