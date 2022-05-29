using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    /// <summary>
    /// Methodes globales pour les factories
    /// </summary>
    public static class GlobalMethodsFactory
    {
        public static TDerived ToDerived<TBase, TDerived>(TBase tBase)
            where TDerived : TBase, new()
        {
            TDerived tDerived = new TDerived();
            foreach (PropertyInfo propBase in typeof(TBase).GetProperties())
            {
                PropertyInfo propDerived = typeof(TDerived).GetProperty(propBase.Name);
                propDerived.SetValue(tDerived, propBase.GetValue(tBase, null), null);
            }
            return tDerived;
        }
    }
}