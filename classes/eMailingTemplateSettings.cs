using System;
using System.Collections.Generic;
using System.Data;
using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;
using System.Linq;

namespace Com.Eudonet.Xrm
{
    /*************************************************************/
    /*   Classe de gestion des template mailing     */
    /* DESCRIPTION  : Classe de gestion des template mailing            */
    /* CREATION     : KJE le 03/06/2021                        */
    /************************************************************/
    /// <summary>
    /// Classe de gestion des template mailing
    /// </summary>
    public class eMailingTemplateSettings
    {
        /// <summary>
        /// Liste des templates client
        /// </summary>
        public List<eMailingTemplate> templates;
        /// <summary>
        /// Liste des secteurs d'activités
        /// </summary>
        public List<ActivityArea> activityArea;
        /// <summary>
        /// Liste des thèmes des template
        /// </summary>
        public List<Themathic> thematic;

        /// <summary>
        /// Class pour les res
        /// </summary>
        public class TemplateRes
        {
            /// <summary>
            /// Liste des res
            /// </summary>
            public IList<Res> res;
            /// <summary>
            /// Recupérer le res à partir de le langId
            /// </summary>
            /// <param name="langId"></param>
            /// <returns></returns>
            public string GetRes(int langId)
            {
                if (res.Any(r => r.langId == langId && !string.IsNullOrEmpty(r.label)))
                    return res.Where(r => r.langId == langId).FirstOrDefault().label;
                else
                    return res.Where(r => r.langId == langId).FirstOrDefault().label;
            }
        }

        /// <summary>
        /// Class Activité
        /// </summary>
        public class ActivityArea : TemplateRes
        {
            /// <summary>
            /// Code Activité
            /// </summary>
            public string code;
        }

        /// <summary>
        /// Class Theme
        /// 9a peut être modifié dans des futures US
        /// </summary>
        public class Themathic: TemplateRes
        {
            /// <summary>
            /// Code du thème
            /// </summary>
            public string code;
        }

        /// <summary>
        /// Class Mailing Template
        /// </summary>
        public class eMailingTemplate: TemplateRes
        {
            /// <summary>
            /// Id du template
            /// </summary>
            public int id;
            /// <summary>
            /// Image en background du template
            /// </summary>
            public string imageName;
            /// <summary>
            /// Image zoom
            /// </summary>
            public string imageZoom;
            /// <summary>
            /// Dossier du template
            /// </summary>
            public string folder;
            /// <summary>
            /// Liste des activités du template
            /// </summary>
            public List<string> activityArea;
            /// <summary>
            /// Liste des thèmes du thèmes
            /// </summary>
            public List<string> thematic;
        }

        /// <summary>
        /// Res
        /// </summary>
        public struct Res
        {
            /// <summary>
            /// Id de la langue
            /// </summary>
            public int langId;
            /// <summary>
            /// res
            /// </summary>
            public string label;
        }
    }
}