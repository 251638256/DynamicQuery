using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicQuery
{
    public class QueryDescriptor
    {
        public IList<OrderByClause>  OrderByMuilt { get; set; }
        public OrderByClause OrderBy { get; set; }
        public IList<QueryCondition> Conditions { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
    }

    public class OrderByClause
    {
        public string Key { get; set; }
        public OrderSequence Order { get; set; }
    }

    public enum OrderSequence
    {
        ASC,
        DESC        
    }
}
