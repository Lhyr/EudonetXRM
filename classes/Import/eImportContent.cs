using System;
using System.Collections.Generic;
using System.IO;

namespace Com.Eudonet.Xrm.Import
{
    /// <summary>
    /// Manager de donnees
    /// </summary>
    /// <purpose>Classe de manipulation des données découpées du flux CSV</purpose>
    public class eImportContent
    {
        private ICollection<eImportContentColumn> _columns = new List<eImportContentColumn>();
        private IDictionary<int, eImportContentLine> _lines = new Dictionary<int, eImportContentLine>();

        /// <summary>Taille du contenu des données à importer</summary>
        public Int32 ContentSize { get; private set; }
        /// <summary>Nombres de colonnes trouvées</summary>
        public Int32 NbColumns { get { return _columns.Count; } }
        /// <summary>Nombres de lignes trouvées</summary>
        public Int32 NbLines { get { return _lines.Count; } }

        /// <summary>Liste des colonnes</summary>
        public IEnumerable<eImportContentColumn> Columns { get { return _columns; } }

        /// <summary>
        /// Parse et charge les lignes dans des objets utilisable par la suite
        /// </summary>
        /// <param name="content">Contenu des données à importer</param>
        /// <param name="impParams">Paramètres de l'import</param>
        public void Load(String content, eImportParams impParams)
        {
            int idx;

            ContentSize = content.Length;

            ResetIndex();

            // Parcours des colonnes
            using (var reader = new StringReader(content))
            {
                idx = 0;
                eImportContentColumn contentColInf;
                var columns = CsvParser.ParseHead(reader, impParams.Delimiter, impParams.Qualifier, impParams.IgnoreFirstLine);

                foreach (String colName in columns)
                {
                    contentColInf = new eImportContentColumn() { Index = idx++, FileColLabel = colName };

                    // Récuperation du descid, clé, label
                    impParams.SetAllInfos(contentColInf);

                    _columns.Add(contentColInf);
                }
            }

            // Parcours des lignes
            using (var reader = new StringReader(content))
            {
                idx = 0;
                var lines = CsvParser.ParseTail(reader, impParams.Delimiter, impParams.Qualifier, impParams.IgnoreFirstLine);

                foreach (IList<String> line in lines)
                {
                    _lines.Add(idx, new eImportContentLine() { Index = idx, Values = line });
                    idx++;
                }
            }
        }

        /// <summary>
        /// Détruit les collections en interne pour libèrer l'espace mémoire rapidement
        /// </summary>
        public void Destroy()
        {
            _columns.Clear();
            _lines.Clear();
        }

        #region gestion d'iteraion sur les lignes

        private int _indexCurrentLine;

        /// <summary>
        /// Repositionne l'index courant sur la premiere ligne
        /// </summary>
        private void ResetIndex()
        {
            _indexCurrentLine = -1;
        }

        /// <summary>
        /// Ligne en cours
        /// </summary>
        public eImportContentLine LinesIteratorCurrent
        {
            get { return _lines[_indexCurrentLine]; }
        }

        /// <summary>
        /// Passe à la ligne suivante
        /// </summary>
        /// <returns></returns>
        public bool LinesIteratorMoveNext()
        {
            _indexCurrentLine++;
            return _indexCurrentLine < _lines.Count;
        }

        /// <summary>
        /// Reviens sur la premiere ligne
        /// </summary>
        public void LinesIteratorReset()
        {
            ResetIndex();
        }

        #endregion
    }
}