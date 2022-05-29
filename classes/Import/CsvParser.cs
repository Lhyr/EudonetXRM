using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Com.Eudonet.Xrm.Import
{
    /// <summary>
    /// Outil de parse d'un contenu CSV
    /// </summary>
    public static class CsvParser
    {
        private static IEnumerable<T> EnumerateTail<T>(IEnumerator<T> en)
        {
            while (en.MoveNext()) yield return en.Current;
        }

        /// <summary>
        /// Parse le contenu pour récupérer les colonnes
        /// </summary>
        /// <param name="reader">Reader contenant les donnes brutes</param>
        /// <param name="delimiter">Délimiteur de colonnes</param>
        /// <param name="qualifier">Identifieur de text</param>
        /// <param name="withHead">Indique si la premiere ligne contient les nom de colonnes</param>
        /// <returns>colonnes</returns>
        public static IList<string> ParseHead(
            TextReader reader, char delimiter, char qualifier = Char.MinValue, bool withHead = true)
        {
            var source = Parse(reader, delimiter, qualifier);

            if (source == null)
                throw new ArgumentNullException("source is null");

            if (withHead)
            {
                var en = source.GetEnumerator();
                en.MoveNext();
                return en.Current;
            }

            var firstLine = source.FirstOrDefault();

            // Cas du sans ligne, on retourne une liste de colonnes vide
            if (firstLine == null)
                return new List<string>();

            int i = 0;
            IList<string> cols = new List<string>();
            foreach (string val in firstLine)
                cols.Add(String.Concat("Col #", ++i));
            return cols;
        }

        /// <summary>
        /// Parse le contenu pour récupérer les lignes
        /// </summary>
        /// <param name="reader">Reader contenant les donnes brutes</param>
        /// <param name="delimiter">Délimiteur de colonnes</param>
        /// <param name="qualifier">Identifieur de text</param>
        /// <param name="withHead">Indique si la premiere ligne contient les nom de colonnes</param>
        /// <returns>lignes</returns>
        public static IEnumerable<IList<string>> ParseTail(
            TextReader reader, char delimiter, char qualifier = Char.MinValue, bool withHead = true)
        {
            var source = Parse(reader, delimiter, qualifier);

            if (source == null)
                throw new ArgumentNullException("source is null");

            var en = source.GetEnumerator();

            // On saute la première lignes
            if (withHead)
                en.MoveNext();

            return EnumerateTail(en);
        }

        private static IEnumerable<IList<string>> Parse(TextReader reader, char delimiter, char qualifier = Char.MinValue)
        {
            bool inQuote = false;
            IList<string> record = new List<string>();
            StringBuilder sb = new StringBuilder();

            while (reader.Peek() != -1)
            {
                var readChar = (char)reader.Read();

                if (readChar == '\n' || (readChar == '\r' && (char)reader.Peek() == '\n'))
                {
                    // If it's a \r\n combo consume the \n part and throw it away.
                    if (readChar == '\r')
                        reader.Read();

                    if (inQuote)
                    {
                        if (readChar == '\r')
                            sb.Append('\r');
                        sb.Append('\n');
                    }
                    else
                    {
                        if (record.Count > 0 || sb.Length > 0)
                        {
                            record.Add(sb.ToString());
                            sb.Clear();
                        }

                        if (record.Count > 0)
                            yield return record;

                        record = new List<string>(record.Count);
                    }
                }
                else if (sb.Length == 0 && !inQuote)
                {
                    if (readChar == qualifier)
                        inQuote = true;
                    else if (readChar == delimiter)
                    {
                        record.Add(sb.ToString());
                        sb.Clear();
                    }
                    else if (char.IsWhiteSpace(readChar))
                    {
                        // Ignore leading whitespace
                    }
                    else
                        sb.Append(readChar);
                }
                else if (readChar == delimiter)
                {
                    if (inQuote)
                        sb.Append(delimiter);
                    else
                    {
                        record.Add(sb.ToString());
                        sb.Clear();
                    }
                }
                else if (readChar == qualifier)
                {
                    if (inQuote)
                    {
                        if ((char)reader.Peek() == qualifier)
                        {
                            reader.Read();
                            sb.Append(qualifier);
                        }
                        else
                            inQuote = false;
                    }
                    else
                        sb.Append(readChar);
                }
                else
                    sb.Append(readChar);
            }

            if (record.Count > 0 || sb.Length > 0)
                record.Add(sb.ToString());

            if (record.Count > 0)
                yield return record;
        }
    }
}