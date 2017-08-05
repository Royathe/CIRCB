using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIRCBot
{
    public static class Params
    {
        private static List<ParamInt> paramList = new List<ParamInt>();

        public static ParamInt[] All
        {
            get
            {
                return paramList.ToArray();
            }
        }

        public static ParamInt[] ParamAttName(string attName)
        {
            return paramList.Where(x => x.ParamAttName == attName).ToArray();
        }

        public static void Add(ParamInt paramInt)
        {
            paramList.Add(paramInt);
        }

        /// <summary>
        /// Empty the repository.
        /// </summary>
        public static void Reset()
        {
            paramList = new List<ParamInt>();
        }
    }
}
