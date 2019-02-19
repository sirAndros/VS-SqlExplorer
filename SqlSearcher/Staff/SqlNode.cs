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
        private readonly List<int> _positionsInFile = new List<int>();
        private int _currentIndex = 0;

        public string Name { get; set; }
        public string Scheme { get; set; }

        public ICommand FindInDocument { get; set; }


        public SqlFile File { get; private set; }


        public SqlObjectNode(SqlFile file, string scheme, string name)
        {
            File = file;
            Name = name;
            Scheme = scheme ?? "dbo";
            FindInDocument = new Command(() =>
            {
                if (_positionsInFile.Count == 0)
                    return;
                if (_currentIndex >= _positionsInFile.Count)
                    _currentIndex = 0;
                int position = _positionsInFile[_currentIndex++];
                File.GotoPosition(position);
            });
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

        internal void AddIndex(int index)
        {
            if (!_positionsInFile.Contains(index))
                _positionsInFile.Add(index);
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
