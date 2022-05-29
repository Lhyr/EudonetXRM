using Com.Eudonet.Xrm.IRISBlack.Model;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    public class ExceptionFactory
    {
        bool isLocalOrEudo = false;

        private ExceptionFactory(bool bLocal)
        {
            isLocalOrEudo = bLocal;
        }

        public static ExceptionFactory InitExceptionFactory(bool bLocal)
        {
            return new ExceptionFactory(bLocal);
        }

        public ExceptionModel GetExceptionModel(EudoException ex)
        {
            ExceptionModel exModel = new ExceptionModel() { UserMessage = ex.UserMessage, Code = ex.ErrorCode };

            if (isLocalOrEudo)
            {
                exModel.DebugMessage = ex.DebugMessage;
                exModel.Message = ex.Message;
                exModel.StackTrace = ex.StackTrace;
            }

            return exModel;
        }


        public ExceptionModel GetExceptionModel(Exception ex)
        {
            ExceptionModel exModel = new ExceptionModel() { UserMessage = "An unexpected Error has occured" };

            if (isLocalOrEudo)
            {
                exModel.DebugMessage = ex.Message;
                exModel.StackTrace = ex.StackTrace;
            }

            return exModel;
        }

    }
}