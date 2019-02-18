using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Controls;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.ComponentModel.Design;

namespace SqlSearcher
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("013c31b0-4113-4450-9eff-02667510dfbe")]
    public class SqlExplorer : ToolWindowPane
    {
        private readonly SqlExplorerControl _control;
        private NameFilterParams _lastFilter;
        private readonly object _lock = new object();


        public bool ShowTables { get; set; } = true;
        public bool ShowProcedures { get; set; } = true;



        public SqlExplorer() : base(null)
        {
            Caption = "SqlExplorer";

            _control = new SqlExplorerControl(this);

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            Content = _control;
            ToolBar = new CommandID(SqlExplorerToolbarCommands.CommandSet, SqlExplorerToolbarCommands.ToolbarId);
        }



        public void RefreshFilter()
        {
            var filterParameters = new NameFilterParams(_lastFilter?.FilterString, _lastFilter?.MatchCase ?? false, ShowTables, ShowProcedures);
            Filter(filterParameters);
        }

        public void Refresh()
        {
            _control.Reload();
        }

        private uint Filter(NameFilterParams filterParameters)
        {
            lock (_lock)
            {
                _control.Filter(filterParameters);
                _lastFilter = filterParameters;
            }
            return 0;
        }


        public override void ProvideSearchSettings(IVsUIDataSource pSearchSettings)
        {
            // Поиск по мере ввода
            Utilities.SetValue(pSearchSettings,
                SearchSettingsDataSource.SearchStartTypeProperty.Name,
                (uint)VSSEARCHSTARTTYPE.SST_DELAYED);

            //Utilities.SetValue(pSearchSettings,
            //    SearchSettingsDataSource.SearchStartMinCharsProperty.Name,
            //     2u);

            Utilities.SetValue(pSearchSettings,
                SearchSettingsDataSource.SearchPopupCloseDelayProperty.Name,
                 0u);

            // прогрессбар
            Utilities.SetValue(pSearchSettings,
                SearchSettingsDataSource.SearchProgressTypeProperty.Name,
                 (uint)VSSEARCHPROGRESSTYPE.SPT_INDETERMINATE);

            //todo ForwardEnterKeyOnSearchStartProperty VSUI_TYPE_BOOL

            Utilities.SetValue(pSearchSettings,
                SearchSettingsDataSource.ControlMaxWidthProperty.Name,
                600u);
        }





        public override bool SearchEnabled
        {
            get { return true; }
        }

        private IVsEnumWindowSearchOptions _optionsEnum;
        public override IVsEnumWindowSearchOptions SearchOptionsEnum
        {
            get
            {
                if (_optionsEnum == null)
                {
                    List<IVsWindowSearchOption> list = new List<IVsWindowSearchOption>();

                    list.Add(MatchCaseOption);

                    _optionsEnum = new WindowSearchOptionEnumerator(list) as IVsEnumWindowSearchOptions;
                }
                return _optionsEnum;
            }
        }

        private WindowSearchBooleanOption _matchCaseOption;
        public WindowSearchBooleanOption MatchCaseOption
        {
            get
            {
                if (_matchCaseOption == null)
                {
                    _matchCaseOption = new WindowSearchBooleanOption("Match case", "Match case", false);
                }
                return _matchCaseOption;
            }
        }



        public override IVsSearchTask CreateSearch(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchCallback pSearchCallback)
        {
            if (pSearchQuery == null || pSearchCallback == null)
                return null;
            return new TestSearchTask(dwCookie, pSearchQuery, pSearchCallback, this);
        }

        public override void ClearSearch()
        {
            _control.ResetFilter();
        }

        internal class TestSearchTask : VsSearchTask
        {
            private SqlExplorer _toolWindow;

            public TestSearchTask(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchCallback pSearchCallback, SqlExplorer toolwindow)
                : base(dwCookie, pSearchQuery, pSearchCallback)
            {
                _toolWindow = toolwindow;
            }

            protected override void OnStartSearch()
            {
                // Use the original content of the text box as the target of the search.   
                //var separator = new string[] { Environment.NewLine };
                //var control = (SqlExplorerControl)m_toolWindow.Content;
                //string[] contentArr = control.Files.Split(separator, StringSplitOptions.None);

                //// Get the search option.   
                //bool matchCase = m_toolWindow.MatchCaseOption.Value;   

                //// Set variables that are used in the finally block.  
                //StringBuilder sb = new StringBuilder("");
                //uint resultCount = 0;
                //this.ErrorCode = VSConstants.S_OK;

                //try
                //{
                //    string searchString = this.SearchQuery.SearchString;

                //    // Determine the results.   
                //    uint progress = 0;
                //    foreach (string line in contentArr)
                //    {
                //        if (matchCase == true)
                //        {
                //            if (line.Contains(searchString))
                //            {
                //                sb.AppendLine(line);
                //                resultCount++;
                //            }
                //        }
                //        else
                //        {
                //            if (line.ToLower().Contains(searchString.ToLower()))
                //            {
                //                sb.AppendLine(line);
                //                resultCount++;
                //            }
                //        }

                //        SearchCallback.ReportProgress(this, progress++, (uint)contentArr.GetLength(0));   
                //    }
                //}
                //catch (Exception e)
                //{
                //    this.ErrorCode = VSConstants.E_FAIL;
                //}
                //finally
                //{


                //    this.SearchResults = resultCount;
                //}

                ErrorCode = VSConstants.S_OK;
                var filterParameters = new NameFilterParams(SearchQuery.SearchString, _toolWindow.MatchCaseOption.Value, _toolWindow.ShowTables, _toolWindow.ShowProcedures);
                ThreadHelper.Generic.Invoke(() =>
                {
                    uint resultCount = 0;
                    try
                    {
                        resultCount = _toolWindow.Filter(filterParameters);
                    }
                    catch (Exception)
                    {
                        ErrorCode = VSConstants.E_FAIL;
                    }
                    SearchResults = resultCount;
                    SearchCallback.ReportComplete(this, resultCount);
                });
                
                // Call the implementation of this method in the base class.   
                // This sets the task status to complete and reports task completion.   
                base.OnStartSearch();
            }

            protected override void OnStopSearch()
            {
                SearchResults = 0;
            }
        }
    }
}
