 

namespace Com.Eudonet.Xrm.mgr
{
    /// <summary>
    /// handler de test
    /// </summary>
    public class eTestHandler  : eExternalManager, System.Web.SessionState.IReadOnlySessionState
    {


        protected override void LoadSession()
        {
            
        }

        protected override bool ValidateExternalLoad()
        {
            return true;
        }


        

        protected override void ProcessManager()
        {
            _context.Response.Write(System.DateTime.Now.Ticks  );
        }
    }
}