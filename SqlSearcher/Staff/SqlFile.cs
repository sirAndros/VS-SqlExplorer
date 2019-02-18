using EnvDTE;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SqlSearcher
{
    public class SqlFile : INotifyPropertyChanged
    {
        public string Path { get; set; }
        public string Name { get; set; }

        private bool _isFiltered = false;
        private bool _isExpanded = false;
        public bool IsFiltered
        {
            get { return _isFiltered; }
            set
            {
                if (_isFiltered != value)
                {
                    _isFiltered = value;
                    //OnPropertyChanged(nameof(IsFiltered));
                    OnPropertyChanged(nameof(IsExpanded));
                }
            }
        }
        public bool IsExpanded
        {
            get { return _isExpanded || _isFiltered; }
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    //OnPropertyChanged(nameof(IsExpanded));
                }
            }
        }


        public ObservableCollection<SqlObjectNode> Items { get; set; } = new ObservableCollection<SqlObjectNode>();

        private readonly ProjectItem _projectItem;


        public SqlFile(ProjectItem projectItem)
        {
            _projectItem = projectItem;
        }

        public SqlFile()
        {
        }


        public void GotoItem(SqlObjectNode item)
        {
            var window = _projectItem?.Open();
            window.Visible = true;
            window.Activate();
            var findResult = _projectItem.DTE.Find.FindReplace(vsFindAction.vsFindActionFind, item.Name, ResultsLocation: vsFindResultsLocation.vsFindResultsNone);
            //if (findResult != vsFindResult.vsFindResultFound)
            //    throw new Exception("Not found");
        }

        internal bool Filter(FilterParams filter)
        {
            var nameFilter = filter as NameFilterParams;
            IsFiltered = !String.IsNullOrEmpty(nameFilter?.FilterString);

            ICollectionView view = CollectionViewSource.GetDefaultView(Items);
            view.Filter = obj => ((SqlObjectNode)obj).Filter(filter);
            return Items.Any(i => i.Filter(filter)); 
        }

        public override string ToString()
        {
            return Name;
        }



        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
