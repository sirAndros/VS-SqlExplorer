using System;
using System.Collections.ObjectModel;
using System.IO;
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
            result.Items = GetFileItems(result);
            return result;
        }

        private static ObservableCollection<SqlObjectNode> GetFileItems(SqlFile result)
        {
            var fileItems = new ObservableCollection<SqlObjectNode>();
            string fileContent = File.ReadAllText(result.Path);
            foreach (Match match in _regex.Matches(fileContent))
            {
                if (match.Success)
                {
                    var itemType = GetItemType(match.Groups[TypeGroup].Value);
                    var item = BuildFileItem(result, itemType,
                                    match.Groups[SchemeGroup].Value,
                                    match.Groups[NameGroup].Value);
                    fileItems.Add(item);
                }
            }
            return fileItems;
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
