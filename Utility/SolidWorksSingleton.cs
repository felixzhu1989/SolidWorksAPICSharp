using System;
using SolidWorks.Interop.sldworks;

namespace Utility
{
    public class SolidWorksSingleton
    {
        private static SldWorks swApp;
        /// <summary>
        /// 连接SolidWorks
        /// </summary>
        /// <returns></returns>
        public static SldWorks GetApplication()
        {
            if (swApp == null)
            {
                swApp = Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application")) as SldWorks;
                swApp.Visible = true;
                return swApp;
            }
            return swApp;
        }
    }
}
