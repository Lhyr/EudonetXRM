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

        // On reprend les informations de l'admin sur l'userid 0
        RqParam rqModif = new RqParam(new StringBuilder()
            .Append("INSERT INTO [FINDERPREF] ([tab], [userid], [findercol]) ")
            .Append("SELECT distinct [DescId], 0, [SearchDescId] ")
            .Append("FROM [DESC] LEFT JOIN [PREF] on[Tab] = [DESC].[DescId] and [UserId] = 0 ")
            .Append("WHERE [DescId] like '%00' and[Type] = 0 ")
            .Append("   and not exists (SELECT 1 FROM [FINDERPREF] where [Tab] = [DESC].[DescId] and [UserId] = 0)").ToString());

        eDal.ExecuteNonQuery(rqModif, out error);
        if (!String.IsNullOrEmpty(error))
            throw new Exception(error);

        // On reprend uniquement les pref qui ne sont pas deja présente dans finderpref
        StringBuilder sb = new StringBuilder();
        sb.Append("SELECT [UserId], [UserLogin], [DescId], ")
            .Append("   (SELECT top 1 [SearchDescId] FROM [PREF] where [Tab] = [DESC].[DescId] and [UserId] = 0  order by prefid desc) as adm, ")
            .Append("   (SELECT top 1 [SearchDescId] FROM [PREF] where [Tab] = [DESC].[DescId] and [UserId] = [USER].[UserId]  order by prefid desc) as uti, ")
            .Append("   (SELECT top 1 [Value] FROM [PREFADV] where Parameter = 'SEARCHCOLWIDTH' and [Tab] = [DescId] and [UserId] = [USER].[UserId] order by id ) as width  ")
            .Append("FROM [USER] CROSS JOIN [DESC]")
            .Append("WHERE [DescId] like '%00' and [Type] = ").Append(EdnType.FILE_MAIN.GetHashCode()).Append(" ")
            .Append(" and not exists (SELECT 1 FROM [FINDERPREF] where [UserId] = [USER].[UserId] and [tab] = [DESC].[DescId])");

        DataTableReaderTuned dtr = null;
        try
        {
            dtr = eDal.Execute(new RqParam(sb.ToString()), out error);
            if (!String.IsNullOrEmpty(error))
                throw new Exception(error);

            if (dtr == null)
                return;

            List<int> lst;
            ISet<int> globalListCol;
            ColWidthOptions colOpts;
            StringBuilder sbNewColWidth = new StringBuilder();

            RqParam rqInsert = new RqParam("INSERT INTO [FINDERPREF] ([userid], [tab], [findercol], [findercolwidth]) SELECT @userid, @tab, @col, @width");
            while (dtr.Read())
            {
                int tab = dtr.GetEudoNumeric("DescId");

                rqInsert.AddInputParameter("@userid", System.Data.SqlDbType.Int, dtr.GetEudoNumeric("UserId"));
                rqInsert.AddInputParameter("@tab", System.Data.SqlDbType.Int, tab);

                globalListCol = new HashSet<int>();
                // Ajout des colonnes definies en administration
                lst = dtr.GetString("adm").ConvertToListInt(";");
                if (lst != null && lst.Count > 0)
                    globalListCol.UnionWith(lst);
                // Ajout des colonnes definies par l'utilisateur
                lst = dtr.GetString("uti").ConvertToListInt(";");
                if (lst != null && lst.Count > 0)
                    globalListCol.UnionWith(lst);

                if (globalListCol.Count > 0)
                    rqInsert.AddInputParameter("@col", System.Data.SqlDbType.VarChar, eLibTools.Join(";", globalListCol));
                else
                    rqInsert.AddInputParameter("@col", System.Data.SqlDbType.VarChar, null);

                colOpts = new ColWidthOptions();
                // Répartition des width
                string[] array = dtr.GetString("width").Split(";");
                for (int i = 0; i < array.Length; i++)
                {
                    string[] kv = array[i].Split(":");
                    if (kv.Length != 2)
                        continue;

                    int d = eLibTools.GetNum(kv[0]);
                    if (d == 0)
                        continue;

                    colOpts.Options.Add(new ColWidthOption() { DescId = d, Width = eLibTools.GetNum(kv[1]) });
                }

                if (colOpts.Options.Count > 0)
                    rqInsert.AddInputParameter("@width", System.Data.SqlDbType.VarChar, SerializerTools.JsonSerialize(colOpts));
                else
                    rqInsert.AddInputParameter("@width", System.Data.SqlDbType.VarChar, null);

                eDal.ExecuteNonQuery(rqInsert, out error);
                if (!String.IsNullOrEmpty(error))
                    throw new Exception(error);
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