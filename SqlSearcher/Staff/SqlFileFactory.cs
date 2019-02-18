using EnvDTE;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSearcher
{
    static class SqlFileFactory
    {
        public static SqlFile FromProjectItem(ProjectItem projItem)
        {
            var result = new SqlFile(projItem);
            result.Name = projItem.Name;
            result.Path = projItem.FileNames[0];
            var fileItems = new ObservableCollection<SqlObjectNode>();
            for (int j = 0; j < 10; j++)
            {
                fileItems.Add(new TableNode(result, "tbl" + j));
                fileItems.Add(new ProcedureNode(result, "proc" + j));
            }
            result.Items = fileItems;
            return result;
        }
    }
}
