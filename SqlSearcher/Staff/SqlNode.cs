using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace SqlSearcher
{
    public class SqlObjectNode
    {
        public string Name { get; set; }
        public string Scheme { get; set; }

        public ICommand FindInDocument { get; set; }


        public SqlFile File { get; private set; }


        public SqlObjectNode(SqlFile file, string scheme, string name)
        {
            File = file;
            Name = name;
            Scheme = scheme ?? "dbo";
            FindInDocument = new Command(() => File.GotoItem(this));
        }


        private void OnItemMouseDoubleClick(object sender, MouseButtonEventArgs args)
        {
            var tvi = sender as TreeViewItem;
            if (tvi?.IsSelected != true)
                return;

            File.GotoItem(this);
        }

        public override string ToString()
        {
            return $"{Scheme}.{Name}";
        }

        internal virtual bool Filter(FilterParams filter)
        {
            var nameFilter = filter as NameFilterParams;
            if (String.IsNullOrEmpty(nameFilter?.FilterString))
                return true;

            var fullName = ToString();
            return nameFilter.MatchCase
                ? fullName.Contains(nameFilter.FilterString)
                : fullName.ToLowerInvariant().Contains(nameFilter.FilterString.ToLowerInvariant());
        }
    }

    class ProcedureNode : SqlObjectNode
    {
        public ProcedureNode(SqlFile file, string name, string scheme = null)
            : base(file, scheme, name)
        {
        }

        internal override bool Filter(FilterParams filter)
        {
            return base.Filter(filter) && filter.ShowProcedures;
        }
    }

    class TableNode : SqlObjectNode
    {
        public TableNode(SqlFile file, string name, string scheme = null)
            : base(file, scheme, name)
        {
        }

        internal override bool Filter(FilterParams filter)
        {
            return base.Filter(filter) && filter.ShowTables;
        }
    }
}
