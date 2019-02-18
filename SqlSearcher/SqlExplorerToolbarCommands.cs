using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSearcher
{
    internal sealed class SqlExplorerToolbarCommands
    {
        public static readonly Guid CommandSet = new Guid("3f993825-e6dc-4ee0-b14d-39df1144596f");
        public const int ToolbarId = 0x1000;
        public const int ShowTablesCommandId = 0x0150;
        public const int ShowProcCommandId = 0x0151;
        public const int RefreshCommandId = 0x0200;

        private static SqlExplorer _window;
        private static MenuCommand _showTablesCommand;
        private static MenuCommand _showProcCommand;

        public static void Init()
        {
            OleMenuCommandService commandService = SqlExplorerCommand.Instance.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                _showTablesCommand = new MenuCommand(SwitchShowTables, new CommandID(CommandSet, ShowTablesCommandId));
                _showProcCommand = new MenuCommand(SwitchShowProc, new CommandID(CommandSet, ShowProcCommandId));
                commandService.AddCommand(_showTablesCommand);
                commandService.AddCommand(_showProcCommand);

                var refreshCommand = new MenuCommand(Refresh, new CommandID(CommandSet, RefreshCommandId));
                commandService.AddCommand(refreshCommand);
            }
        }

        public static void SetWindow(SqlExplorer window)
        {
            _window = window;
            _showTablesCommand.Checked = window.ShowTables;
            _showProcCommand.Checked = window.ShowProcedures;
        }

        private static void Refresh(object sender, EventArgs e)
        {
            if (_window != null)
            {
                _window.Refresh();
            }
        }

        private static void SwitchShowProc(object sender, EventArgs e)
        {
            if (_window != null)
            {
                _window.ShowProcedures = !_window.ShowProcedures;
                _showProcCommand.Checked = _window.ShowProcedures;
                _window.RefreshFilter();
            }
        }

        private static void SwitchShowTables(object sender, EventArgs e)
        {
            if (_window != null)
            {
                _window.ShowTables = !_window.ShowTables;
                _showTablesCommand.Checked = _window.ShowTables;
                _window.RefreshFilter();
            }
        }
    }
}
