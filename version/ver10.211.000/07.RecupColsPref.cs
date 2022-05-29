using Com.Eudonet.Internal;
using Com.Eudonet.Xrm;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Text;
using Com.Eudonet.Core.Model;
/// <className>ExtendedString</className>
/// <summary>Regroupement des méthodes étendues de la classe String</summary>
/// <purpose></purpose>
/// <authors>HLA</authors>
/// <date>2017-12-07</date>
public static class ExtendedString
{

    /// <summary>
    /// Raccourcis de la méthode Split système avec en option StringSplitOptions.None
    /// </summary>
    /// <param name="value">chaine à découper</param>
    /// <param name="separator">separateur de découpage</param>
    /// <returns>tableau de valeurs</returns>
    public static string[] Split(this string value, string separator)
    {
        return value.Split(new string[] { separator }, StringSplitOptions.None);
    }

    /// <summary>
    /// Split la chaine et convertit le contenu en tableau d'entier
    /// </summary>
    /// <param name="value"></param>
    /// <param name="separator"></param>
    /// <returns></returns>
    public static int[] SplitToInt(this string value, string separator)
    {
        List<int> lst = value.ConvertToListInt(separator);
        return lst.ToArray();


    }


    /// <summary>
    /// Convertit une chaine de caractère avec séparateur en une liste d'un autre type.
    /// </summary>
    /// <typeparam name="TOutput">type des éléments de la liste cible.</typeparam>
    /// <param name="value">chaine de caractère</param>
    /// <param name="separator">séparateur de valeur dans la chaine de caractère de départ</param>
    /// <param name="converter">convertit chaque élément du type string en un autre type</param>
    /// <returns>Liste du type cible qui contient les éléments convertis de la chaine source.</returns>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="converter" /> est null.</exception>
    public static List<TOutput> ConvertToList<TOutput>(this string value, string separator, Converter<string, TOutput?> converter) where TOutput : struct
    {
        //return new List<TOutput>(Array.ConvertAll(value.Split(separator), converter));

        List<TOutput> lst = new List<TOutput>();

        if (string.IsNullOrEmpty(value))
            return lst;

        string[] array = value.Split(separator);

        if (converter == null)
            throw new ArgumentNullException("converter");

        foreach (string val in array)
        {
            TOutput? newVal = converter(val);
            if (newVal != null)
                lst.Add(newVal.Value);
        }

        return lst;
    }

    /// <summary>
    /// Convertit une chaine de caractère avec séparateur en une liste de int.
    /// </summary>
    /// <param name="value">chaine de caractère</param>
    /// <param name="separator">séparateur de valeur dans la chaine de caractère de départ</param>
    /// <returns>Liste du type int qui contient les éléments convertis de la chaine source.</returns>
    public static List<int> ConvertToListInt(this string value, string separator)
    {
        return ConvertToList(value, separator, new Converter<string, int?>(
                delegate (string s)
                {
                    int result = 0;
                    if (int.TryParse(s, out result))
                        return result;
                    return null;
                }));
    }


}

public class VersionFile
{
    public class FinderPrefKey
    {
        public int UserId { get; set; }
        public int Tab { get; set; }
        public override int GetHashCode()
        {
            return (UserId.GetHashCode() << 5) + UserId.GetHashCode() ^ Tab.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            FinderPrefKey tmp = obj as FinderPrefKey;
            if (tmp == null)
                return false;

            return UserId == tmp.UserId && Tab == tmp.Tab;
        }
        public override string ToString()
        {
            return String.Concat("UserId : ", UserId, " / Tab : ", Tab);
        }
    }

    public static void Upgrade(Object sender)
    {


        eUpgrader upgraderSender = (eUpgrader)sender;
        //upgraderSender.AddReferenceAssembly(typeof(System.Data.DataTableReader));
        eudoDAL eDal = upgraderSender.EDal;

        String error = String.Empty;




        // On reprend uniquement les pref qui ne sont pas deja présente dans finderpref
        StringBuilder sbGetOldPref = new StringBuilder();
        sbGetOldPref.Append("SELECT P.USERID, P.Tab, P.ListSelCol, ListSelColWidth,ListSelOrder,ListSelSort,")
            .Append(" (select top 1  isnull(aa.ProspectEnabled,0) from [desc] aa where descid = p.tab) TARGET ")
            .Append(" FROM PREF P ")
            .Append(" WHERE P.Tab IN (SELECT Tab FROM PREF where AdrJoin = 1) ");


        RqParam rqSelect = new RqParam(sbGetOldPref.ToString());
        rqSelect.AddInputParameter("@type", System.Data.SqlDbType.Int, ColPrefType.INVITADD);

        DataTableReaderTuned dtr = null;
        try
        {
            dtr = eDal.Execute(rqSelect, out error);
            if (!String.IsNullOrEmpty(error))
                throw new Exception(error);

            if (dtr == null)
                return;



            List<int> lst;
            ISet<int> globalListCol;

            ColWidthOptions colOpts;
            StringBuilder sbNewColWidth = new StringBuilder();

            StringBuilder sbRs = new StringBuilder();
            sbRs.Append("IF NOT EXISTS (SELECT 1 FROM COLSPREF WHERE TAB=@TAB AND colspreftype = @type AND userid=@userid) ")
                .Append(" BEGIN ")
                .Append(" INSERT INTO[COLSPREF]([userid], [tab], [col], [colwidth],  [colspreftype]) SELECT @userid, @tab, @col, @width, @type ")
                .Append(" END ");





            RqParam rqInsert = new RqParam(sbRs.ToString());



            while (dtr.Read())
            {


                bool bIsTarget = dtr.GetEudoNumeric("TARGET") == 1;


                int tab = dtr.GetEudoNumeric("tab");

                rqInsert.AddInputParameter("@userid", System.Data.SqlDbType.Int, dtr.GetEudoNumeric("UserId"));
                rqInsert.AddInputParameter("@tab", System.Data.SqlDbType.Int, tab);


                #region Add

                if (bIsTarget)
                    rqInsert.AddInputParameter("@type", System.Data.SqlDbType.Int, ColPrefType.TARGETADD);
                else
                    rqInsert.AddInputParameter("@type", System.Data.SqlDbType.Int, ColPrefType.INVITADD);

                globalListCol = new HashSet<int>();

                // Ajout des colonnes definies par l'utilisateur
                lst = dtr.GetString("ListSelCol").ConvertToListInt(";");
                if (lst != null && lst.Count > 0)
                    globalListCol.UnionWith(lst);



                if (!lst.Contains(201))
                    lst.Insert(0, 201);

                colOpts = new ColWidthOptions();

                // Répartition des width
                string[] array = dtr.GetString("ListSelColWidth").Split(";");

                if (lst.Count > 1 && lst.Count == array.Length)
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        colOpts.Options.Add(new ColWidthOption() { DescId = lst[i], Width = eLibTools.GetNum(array[i]) });
                    }
                }

                //sur les ++ on accepte que les champs de PP/PM/ADDRESS
                List<int> lstAdd = new List<int>();
                ColWidthOptions colOptsAdd = new ColWidthOptions();
                foreach (int i in lst)
                {

                    int nTab = i - i % 100;

                    if (nTab == 400 || nTab == 300 || nTab == 200)
                    {
                        lstAdd.Add(i);

                        ColWidthOption a = colOpts.Options.Find(p => p.DescId == i);
                        if (a != null)
                            colOptsAdd.Options.Add(new ColWidthOption() { DescId = a.DescId, Width = a.Width });

                    }

                }

                lstAdd.Remove(201);

                if (lstAdd.Count > 0)
                    rqInsert.AddInputParameter("@col", System.Data.SqlDbType.VarChar, eLibTools.Join(";", lstAdd));
                else
                    rqInsert.AddInputParameter("@col", System.Data.SqlDbType.VarChar, null);


                if (colOptsAdd.Options.Count > 0)
                    rqInsert.AddInputParameter("@width", System.Data.SqlDbType.VarChar, SerializerTools.JsonSerialize(colOptsAdd));
                else
                    rqInsert.AddInputParameter("@width", System.Data.SqlDbType.VarChar, null);

                eDal.ExecuteNonQuery(rqInsert, out error);
                if (!String.IsNullOrEmpty(error))
                    throw new Exception(error);




                #endregion

                #region del




                if (bIsTarget)
                    rqInsert.AddInputParameter("@type", System.Data.SqlDbType.Int, ColPrefType.TARGETDELETE);
                else
                    rqInsert.AddInputParameter("@type", System.Data.SqlDbType.Int, ColPrefType.INVITDELETE);

                //sur les ++ on accepte que les champs de PP/PM/ADDRESS
                List<int> lsDel = new List<int>();
                ColWidthOptions colOptsDel = new ColWidthOptions();
                foreach (int i in lst)
                {

                    int nTab = i - i % 100;

                    if (nTab == 400 || nTab == 300 || nTab == 200 || (nTab == tab && bIsTarget))
                    {
                        lsDel.Add(i);
                        ColWidthOption a = colOpts.Options.Find(p => p.DescId == i);
                        if (a != null)
                            colOptsDel.Options.Add(new ColWidthOption() { DescId = a.DescId, Width = a.Width });

                    }

                }

                lsDel.Remove(201);
                if (lsDel.Count > 0)
                    rqInsert.AddInputParameter("@col", System.Data.SqlDbType.VarChar, eLibTools.Join(";", lsDel));
                else
                    rqInsert.AddInputParameter("@col", System.Data.SqlDbType.VarChar, null);


                if (colOptsAdd.Options.Count > 0)
                    rqInsert.AddInputParameter("@width", System.Data.SqlDbType.VarChar, SerializerTools.JsonSerialize(colOptsDel));
                else
                    rqInsert.AddInputParameter("@width", System.Data.SqlDbType.VarChar, null);

                eDal.ExecuteNonQuery(rqInsert, out error);
                if (!String.IsNullOrEmpty(error))
                    throw new Exception(error);



                #endregion




            }
        }
        catch
        {
            throw;
        }
        finally
        {
            if (dtr != null)
                dtr.Dispose();
        }
    }
}