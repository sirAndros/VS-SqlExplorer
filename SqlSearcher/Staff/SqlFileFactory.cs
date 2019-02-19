using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using EnvDTE;

namespace SqlSearcher
{
    static class SqlFileFactory
    {
        enum FileItemType
        {
            Table,
            Proc,
            Func,
            View,
        }

        private const string TypeGroup = "TYPE";
        private const string SchemeGroup = "SCHEME";
        private const string NameGroup = "NAME";
        private static string _regexPattern =
            @"(?:(?<"+TypeGroup+@">" + GetFileItemTypesOrString() + @")\w*\s*"
            + @"(?:\[|\"")?(?<"+SchemeGroup+@">[\w\ ]+?)(?:\]|\"")?"
            + @"\.(?:\[|\"")?(?<"+NameGroup+@">[\w\ ]+?)(?:\]|\"")?[\s\r\n\(])"
            ;//+ @"|(?:sp_rename\s+N?\'\[?(?<SCHEMA>[\w\ ]+?)\]?\.(?:\[?[\w\ ]+?\]?)\'\,\s+N?\'\[?([\w\ ]+?)\]?\'\,\s+\'\w+\')";

        private static readonly Regex _regex = new Regex(_regexPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

        public static SqlFile FromProjectItem(ProjectItem projItem)
        {
            var result = new SqlFile(projItem);
            result.Name = projItem.Name;
            result.Path = projItem.FileNames[0];
            result.Items = GetFileItems(result, projItem);
            return result;
        }

        private static ObservableCollection<SqlObjectNode> GetFileItems(SqlFile result, ProjectItem projItem)
        {
            string fileContent = GetFileContent(projItem);
            var fileItems = new Dictionary<string, SqlObjectNode>();
            foreach (Match match in _regex.Matches(fileContent))
            {
                if (match.Success)
                {
                    var itemType = GetItemType(match.Groups[TypeGroup].Value);
                    string scheme = match.Groups[SchemeGroup].Value;
                    string name = match.Groups[NameGroup].Value;
                    string key = scheme + "." + name;
                    SqlObjectNode node;
                    if (!fileItems.TryGetValue(key, out node))
                    {
                        node = BuildFileItem(result, itemType, scheme, name);
                        fileItems.Add(key, node);
                    }
                    node.AddIndex(match.Index);
                }
            }
            return new ObservableCollection<SqlObjectNode>(fileItems.Values);
        }

        private static string GetFileContent(ProjectItem projItem)
        {
            bool wasOpened = !projItem.IsOpen;
            Window win = projItem.Open();

            var doc = win.Document.Object("TextDocument") as TextDocument;
            var editPoint = doc.CreateEditPoint(doc.StartPoint);
            string fileContent = editPoint.GetText(doc.EndPoint);

            if (wasOpened)
                win.Close();

            return fileContent;
        }

        private static FileItemType GetItemType(string value)
        {
            return (FileItemType)Enum.Parse(typeof(FileItemType), value, true);
        }

        private static string GetFileItemTypesOrString()
        {
            return String.Join("|", Enum.GetNames(typeof(FileItemType)));
        }

        private static SqlObjectNode BuildFileItem(SqlFile file, FileItemType type, string scheme, string name)
        {
            switch (type)
            {
                case FileItemType.Table:
                case FileItemType.View:
                    return new TableNode(file, name, scheme);

                case FileItemType.Proc:
                case FileItemType.Func:
                    return new ProcedureNode(file, name, scheme);

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), "Unknown node type");
            }
        }
    }
}
