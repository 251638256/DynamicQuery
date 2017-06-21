using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicQuery
{
    public enum QueryOperator
    {
        EQUAL,
        CONTAINS,
        LESSOREQUAL,
        LESS,
        GREATEROREQUAL,
        GERATER,
        IN
    }
 
    

    public class QueryCondition
    {
        public string Key
        {
            get;
            set;
        }

        public QueryOperator Operator
        {
            get;
            set;
        }

        public object Value
        {
            get;
            set;
        }

        public string ValueType
        {
            get;
            set;
        }
    }
}
