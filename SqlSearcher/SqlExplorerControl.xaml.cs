using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Collections.Generic;
using System;
using System.IO;

namespace SqlSearcher
{
    public partial class SqlExplorerControl : UserControl
    {
        private readonly SqlExplorer _window;

        public ObservableCollection<SqlFile> Files { get; } = new ObservableCollection<SqlFile>();

        public SqlExplorerControl(SqlExplorer explorerWindow)
        {
            InitializeComponent();
            InitItems();
            DataContext = this;
            _window = explorerWindow;
        }

        public void Reload()
        {
            InitItems();
        }

        internal void Filter(FilterParams filter)
        {
            _sqlFilesTree.Items.Filter = i => ((SqlFile)i).Filter(filter);
        }

        internal void ResetFilter()
        {
            var filter = new FilterParams(_window.ShowTables, _window.ShowProcedures);
            Filter(filter);
        }

        private void InitItems()
        {
            Files.Clear();

            var projItems = GetSqlProjectItems();
            foreach (var item in projItems)
            {
                var file = SqlFileFactory.FromProjectItem(item);
                if (file.Items?.Count > 0)
                    Files.Add(file);
            }

            //GenerateStub();
        }

        private void GenerateStub()
        {
            for (int i = 0; i < 20; i++)
            {
                var sqlFile = new SqlFile()
                {
                    Name = "file" + i
                };
                var fileItems = new ObservableCollection<SqlObjectNode>();
                for (int j = 0; j < 1000; j++)
                {
                    fileItems.Add(new TableNode(sqlFile, "tbl" + j));
                    fileItems.Add(new ProcedureNode(sqlFile, "proc" + j));
                }
                sqlFile.Items = fileItems;
                Files.Add(sqlFile);
            }
        }

        private static List<ProjectItem> GetSqlProjectItems()
        {
            var result = new List<ProjectItem>();
            var dte2 = SqlExplorerCommand.Instance.ServiceProvider.GetService(typeof(DTE)) as DTE2;
            if (dte2?.Solution?.Projects != null)
            {
                foreach (var item in dte2.Solution.Projects)
                {
                    Project project = item as Project;
                    if (project == null)
                        continue;
                    result.AddRange(GetSqlProjectItems(project));
                }
            }
            return result;
        }

        private static IEnumerable<ProjectItem> GetSqlProjectItems(Project project)
        {
            if (project.ProjectItems == null)
                return Enumerable.Empty<ProjectItem>();

            return GetSqlProjectItems(project.ProjectItems);
        }

        private static IEnumerable<ProjectItem> GetSqlProjectItems(ProjectItems projectItems)
        {
            var result = new List<ProjectItem>();
            foreach (var item in projectItems)
            {
                var projectItem = item as ProjectItem;
                if (projectItem == null)
                    continue;
                result.AddRange(GetSqlProjectItems(projectItem));
            }
            return result;
        }

        private static IEnumerable<ProjectItem> GetSqlProjectItems(ProjectItem projectItem)
        {
            var subProject = projectItem.SubProject;
            if (subProject == null)
            {
                if (projectItem.ProjectItems?.Count > 0)
                    return GetSqlProjectItems(projectItem.ProjectItems);
                if (Path.GetExtension(projectItem.Name).ToLower() == ".sql")
                    return new[] { projectItem };
                else
                    return Enumerable.Empty<ProjectItem>();
            }
            else
            {
                return GetSqlProjectItems(subProject);
            }
        }

        private string ReadSolutionPath()
        {
            string pbstrSolutionDirectory, pbstrSolutionFile, pbstrUserOptsFile;
            var solution = (IVsSolution)SqlExplorerCommand.Instance.ServiceProvider.GetService(typeof(SVsSolution));
            solution.GetSolutionInfo(out pbstrSolutionDirectory, out pbstrSolutionFile, out pbstrUserOptsFile);

            return pbstrSolutionDirectory;
        }
    }
}