using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EudoExtendedClasses;
using System.Text.RegularExpressions;
using System.IO;
using Com.Eudonet.Internal;

namespace Com.Eudonet.Xrm.Import
{
    /// <summary>
    /// Objet qui regroupe toutes les infos sur l'import cible etendue
    /// </summary>
    public class eImportParams
    {
        /// <summary>Action du Manager</summary>
        public eImportAction Action { get; private set; }

        /// <summary>indique si l'import est lancé depuis un fichier</summary>
        public bool FromFile { get; private set; }

        /// <summary>DescId de la table cible</summary>
        public int TabMainDid { get; private set; }
        /// <summary>DescId de la table depuis laquel est lancé l'import</summary>
        public int TabFromDid { get; private set; }
        /// <summary>id de la fiche de la table depuis laquel est lancé l'import</summary>
        public int EvtId { get; private set; }

        /// <summary>Nom des colonnes sur la premiere ligne</summary>
        public bool IgnoreFirstLine { get; private set; }
        /// <summary>Délimiteur de colonnes</summary>
        public char Delimiter { get; private set; }
        /// <summary>Identificateur de text</summary>
        public char Qualifier { get; private set; }


        /// <summary>
        /// Champ relation parent (si liaison non principale)
        /// </summary>
        public int RelationParentDescId { get; internal set; } = 0;

        private string[] aDescId;
        private string[] aLabelList;
        private int[] aId;
        private string[] aKey;

        /// <summary>
        /// Informations sur l'import demandé
        /// </summary>
        /// <param name="requestTools">valeurs de paramètres postées</param>
        public eImportParams(eRequestTools requestTools)
        {
            //Récupération des paramètres
            TabMainDid = requestTools.GetRequestFormKeyI("tab") ?? 0;
            TabFromDid = requestTools.GetRequestFormKeyI("tabfrom") ?? 0;
            EvtId = requestTools.GetRequestFormKeyI("evtid") ?? 0;

            Action = (eImportAction)(requestTools.GetRequestFormKeyI("action") ?? eImportAction.DO_NOTHING.GetHashCode());

            FromFile = requestTools.GetRequestFormKeyB("fromfile") ?? false;

            // Il n'est pas possible d'être multi caractères
            Delimiter = Char.MinValue;
            string deli = requestTools.GetRequestFormKeyS("separator") ?? String.Empty;
            if (deli == "<tab>" || deli == "&lt;tab&gt;")
                Delimiter = (char)9;
            else if (deli.Length > 0)
                Delimiter = deli[0];

            // Il n'est pas possible d'être multi caractères
            Qualifier = Char.MinValue;
            string quali = requestTools.GetRequestFormKeyS("identificator") ?? String.Empty;
            if (quali.Length > 0)
                Qualifier = quali[0];

            // la première ligne est le header
            IgnoreFirstLine = requestTools.GetRequestFormKeyB("colheader") ?? false;

            string sDescIdList = requestTools.GetRequestFormKeyS("descidlist") ?? String.Empty;
            string sLabelList = requestTools.GetRequestFormKeyS("labellist") ?? String.Empty;
            string sIdList = requestTools.GetRequestFormKeyS("idlist") ?? String.Empty;
            string sKeyList = requestTools.GetRequestFormKeyS("keylist") ?? String.Empty;

            aDescId = sDescIdList.Split(';');
            aLabelList = sLabelList.Split(";|;");
            aId = sIdList.ConvertToListInt(";").ToArray();
            aKey = sKeyList.Split(';');
        }

        /// <summary>
        /// Rattache les informations collectées sur les colonnes trouvées sur le flux CSV
        /// </summary>
        /// <param name="contentColInf">Objet de colonnes à enrichir</param>
        public void SetAllInfos(eImportContentColumn contentColInf)
        {
            int paramsIndex = GetIndex(contentColInf.Index);

            if (paramsIndex == -1
                || paramsIndex >= aDescId.Length
                || paramsIndex >= aLabelList.Length
                || paramsIndex >= aKey.Length)
                return;

            // DescId
            contentColInf.DescId = eLibTools.GetNum(aDescId[paramsIndex]);
            // Label
            contentColInf.EudoColLabel = aLabelList[paramsIndex].Trim();
            // Clé
            contentColInf.IsKey = aKey[paramsIndex] == "1";
        }

        /// <summary>
        /// Retourne l'index des informations de l'import par rapport à l'index de la colonne de la source
        /// </summary>
        /// <param name="contentColIndex">index de la colonne de la source</param>
        /// <returns></returns>
        private int GetIndex(int contentColIndex)
        {
            int index = 0;

            for (int i = 0; i < aId.Length; i++)
            {
                if (aId[i] == contentColIndex)
                    return index;
                index++;
            }


            return -1;
        }
    }
}