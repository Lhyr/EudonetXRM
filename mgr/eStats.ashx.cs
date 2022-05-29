using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Objet permettant d'afficher les appels couteux
    /// </summary>
    public class eStats : eEudoManagerReadOnly
    {
        /// <summary>
        /// Une table affaichant le appels avec la durée
        /// </summary>
        protected override void ProcessManager()
        {
            StringBuilder sb = new StringBuilder();

            bool bLocalOrEudo = eLibTools.IsLocalOrEudoMachine(_context);
            if (bLocalOrEudo)
            {
                int CallbackInterval = _requestTools.GetRequestQSKeyI("interval") ?? 5; //secondes

                // Activation
                bool? isActive = _requestTools.GetRequestQSKeyB("active");
                if (isActive != null)
                    eRequestReportManager.Instance.SwitchState(isActive.Value);

                // Limitation de nombre de ligne
                int? maxItem = _requestTools.GetRequestQSKeyI("max");
                if (maxItem != null)
                    eRequestReportManager.Instance.MaxItems = maxItem.Value;

                sb.Append("<html><head>");

                // Styles de la table
                sb.Append("<style type='text/css'>");
                sb.Append(@"#stats { font-family: 'Trebuchet MS', Arial, Helvetica, sans-serif; border - collapse: collapse;  width: 100%;}
                            #stats th { background-color: #ddd;  padding: 8px; }
                            #stats td { border: 1px solid #ddd;  padding: 8px; }
                            #stats tr:nth-child(even){background-color: #f2f2f2;}");
                sb.Append("</style>");

                // Function callback
                sb.Append("<script type='text/javascript'>");
                sb.Append(@"function init(waitInSeconds){ if(waitInSeconds <= 0) waitInSeconds = 5; setTimeout( function(){ window.location.reload(true); }, waitInSeconds * 1000); } ");
                sb.Append("</script>");

                sb.Append("</head><body>");

                // Callback de js par second
                if (eRequestReportManager.Instance.IsStateActive())
                {
                    sb.Append("<script type='text/javascript'>");
                    sb.Append(@" init(").Append(CallbackInterval).Append(");");
                    sb.Append("</script>");
                }

                // Table
                sb.Append("<table id='stats'>");

                // Entête
                sb.Append("<tr>")
                       .Append("<th>Duration (ms)</th>")
                       .Append("<th>Base</th>")
                       .Append("<th>User</th>")
                       .Append("<th>Action</th>")
                       .Append("<th>Grid ID</th>")
                       .Append("<th>Widget ID</th>")
                       .Append("<th>Report ID</th>")
                   .Append("</tr>");

                // Données
                foreach (var call in eRequestReportManager.Instance.GetCalls())
                    sb.Append("<tr>")
                        .Append("<td>").Append(call.ElapsedTime).Append("</td>")
                        .Append("<td>").Append(call.BaseName).Append("</td>")
                        .Append("<td>").Append(call.User).Append("</td>")
                        .Append("<td>").Append(call.Action).Append("</td>")
                        .Append("<td>").Append(call.Grid).Append("</td>")
                        .Append("<td>").Append(call.Widget).Append("</td>")
                        .Append("<td>").Append(call.Report).Append("</td>")
                    .Append("</tr>");

                sb.Append("</table>");
                sb.Append("</body></html>");
            }

            RenderResult(RequestContentType.HTML, delegate () { return sb.ToString(); });
        }
    }
}