using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Com.Eudonet.Internal;
using EudoQuery;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Permet de charger le mapping depuis config_adv
    /// </summary>
    public class eSaml2Settings : ISaml2Settings
    {
        // On invoque init dans le cas ou le client ne le fait pas
        bool _initialized = false;
        private ePrefSQL _pref;

        /// <summary>
        /// Config de la base
        /// </summary>
        public eSaml2DatabaseConfig DbConfig;

        /// <summary>
        /// Objet permettant de récupérer les infos du mapping dans la base
        /// </summary>
        /// <param name="pref">préférence sql de la base</param>
        public eSaml2Settings(ePrefSQL pref)
        {
            _pref = pref;
        }

        /// <summary>
        /// Récupération du paramétrage de Saml2
        /// </summary>
        /// <exception cref="ArgumentException">Pas de mapping définit</exception>
        /// <exception cref="eSaml2JsonFormatException">Format json du mapping n'est pas valide</exception>
        public void Init()
        {
            if (_initialized)
                return;

            _initialized = true;

            // Recupère le mapping dans config adv
            string authSettings = eLibTools.GetConfigAdvValues(_pref, new[] { eLibConst.CONFIGADV.AUTHENTICATION_SETTINGS })[eLibConst.CONFIGADV.AUTHENTICATION_SETTINGS];
            if (string.IsNullOrWhiteSpace(authSettings))
                throw new ArgumentException("No mapping found from configadv");

            try
            {
                DbConfig = eSaml2DatabaseConfig.GetConfig(authSettings);               
            }
            catch (Exception ex)
            {
                throw new eSaml2JsonFormatException($"Invalid json mapping format : {Environment.NewLine} {authSettings}", ex);
            }
        }

        /// <summary>
        /// Conversion des attributs forunis par l'IDP en champs mappés dans l'application
        /// </summary>
        /// <remarks>Si les attributs ont tous le même nom, le premier de la liste sera pris en compte, les autres sont ignoré</remarks>
        /// <param name="saml2Attributes">attributes de l'idp pour identifier l'utilisateur</param>
        /// <exception cref="InvalidOperationException">La methode est appélé avant <see cref="Init"/></exception>
        /// <return> List des champs mappé en base avec les valeurs récupéré dans les attributs</return>
        public IEnumerable<eSaml2Field> Convert(IEnumerable<eSaml2Attribute> saml2Attributes)
        {
            if (!_initialized)
                throw new InvalidOperationException("Impossible de convertir sans initialisation");

            List<eSaml2Field> fields = new List<eSaml2Field>();
            InternalConvert(saml2Attributes, fields);
            AddDescInfosTo(fields);

            return fields;
        }

        /// <summary>
        /// On valide le mapping avec les attributs fournis par l'IDP puis on le transforme en liste des fields
        /// </summary>
        /// <param name="saml2Attributes">liste attributs IDP</param>
        /// <param name="fields">List des fields associés avec les valeurs des attributes</param>
        /// <exception cref="ArgumentException">Pas de mapping </exception>
        /// <exception cref="eSaml2MappingMissingException"> un des champ mappé n'est pas transmis par l'idp</exception>
        private void InternalConvert(IEnumerable<eSaml2Attribute> saml2Attributes, List<eSaml2Field> fields)
        {
            if (DbConfig.MappingAttributes == null || DbConfig.MappingAttributes.Count == 0)
                throw new ArgumentException("Empty mapping");

            ILookup<string, eSaml2Attribute> lookup = saml2Attributes.ToLookup(e => e.AttributeName);
            foreach (var map in DbConfig.MappingAttributes)
            {
                if (string.IsNullOrWhiteSpace(map.Saml2Attribute))
                    continue;

                if (lookup.Contains(map.Saml2Attribute))
                {
                    // On prend le premier de la même famille (au cas l'IDP envoi le même attribut plusieurs fois)
                    var saml2Attr = lookup[map.Saml2Attribute].First();
                    fields.Add(new eSaml2Field() { DescId = int.Parse(map.DescId), ColumnValue = saml2Attr.AttributeValue, IsKey = map.IsKey });
                }
                else
                {
                    if (map.IsKey)
                        throw new eSaml2MappingMissingException($"Attribute {map.Saml2Attribute} non trouvé !");//TODORES
                }
            }
        }

        /// <summary>
        ///Enrichit les champs avec les infos desc
        /// </summary>
        /// <param name="fields">Champs à enrichir par les infos desc</param>
        /// <exception cref="ArgumentException">Pas de champ mappés </exception>
        /// <exception cref="eSaml2SqlException">Operation de lecture de la base impossible</exception>
        private void AddDescInfosTo(List<eSaml2Field> fields)
        {
            if (fields == null || fields.Count == 0)
                throw new ArgumentException("Empty map field list");

            //  Récupérer les infos des rubriques mappées dans desc
            RqParam rq = new RqParam($"select [Descid], [Field], [Unicode], [Format] from [DESC] where [descid] in ({ eLibTools.Join(",", fields, f => f.DescId.ToString())} )");
            using (eudoDAL dal = eLibTools.GetEudoDAL(_pref))
            {
                dal.OpenDatabase();

                string error;
                var dtr = dal.Execute(rq, out error);

                if (!string.IsNullOrWhiteSpace(error))
                    throw new eSaml2SqlException($"Error : {error} {Environment.NewLine} {rq.GetSqlCommandText()}");

                if (dtr == null)
                    throw new eSaml2SqlException($"Error : dtr null {Environment.NewLine} {rq.GetSqlCommandText()}");

                ILookup<int, eSaml2Field> lookup = fields.ToLookup(e => e.DescId);
                while (dtr.Read())
                {
                    var fld = lookup[dtr.GetEudoNumeric("Descid")].First();
                    fld.ColumnName = dtr.GetString("Field");
                    fld.ColumnValue = TransformValue(dtr.GetInt16("Format"), dtr.GetBoolean("Unicode"), fld.ColumnValue, out fld.SqlDataType);
                }
            }
        }

        /// <summary>
        /// On transforme la valeur en entrée en valeur sql avec la prise en compte du unicode
        /// Retourne le type sql
        /// </summary>
        /// <param name="format">format de la rubrique</param>
        /// <param name="isUnicode">unicode ou pas</param>
        /// <param name="originVal">valeur d'origine</param>
        /// <param name="sqlDbType">type sql</param>
        /// <exception cref="ArgumentNullException">Format de la rubrique n'est pas connu</exception>
        /// <returns>La valeur finale à transmettre au sql</returns>
        private object TransformValue(short format, bool isUnicode, object originVal, out SqlDbType sqlDbType)
        {
            object finalVal;
            SqlDbType? sqlTyp = eLibTools.GetParamFieldValue(eLibTools.GetEnumFromCode<FieldFormat>(format), isUnicode, null, originVal, out finalVal);

            if (sqlTyp == null)
                throw new ArgumentNullException($"Format de champ : {format} inconnu");//TODORES

            if (sqlTyp == SqlDbType.Bit)
                finalVal = (bool)finalVal ? "1" : "0";

            sqlDbType = sqlTyp.Value;

            return finalVal;
        }
    }
}