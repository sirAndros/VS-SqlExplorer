using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSearcher
{
    class FilterParams
    {
        public bool ShowTables { get; set; }
        public bool ShowProcedures { get; set; }



        public FilterParams(bool showTables, bool showProcedures)
        {
            ShowTables = showTables;
            ShowProcedures = showProcedures;
        }
    }

    class NameFilterParams : FilterParams
    {
        public string FilterString { get; set; }
        public bool MatchCase { get; set; }


        public NameFilterParams(string filterString, bool matchCase, bool showTables, bool showProcedures)
            : base(showTables, showProcedures)
        {
            FilterString = filterString;
            MatchCase = matchCase;
        }

        public NameFilterParams(string filterString, bool matchCase = false)
            : this(filterString, matchCase, true, true)
        {
        }
    }
}
