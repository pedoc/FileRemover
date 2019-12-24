using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace FileRemover
{
    public class Rule
    {
        public bool IsEnable { get; set; } = true;
        public string Directory { get; set; }
        public string Filter { get; set; }
        public MatchMode MatchMode { get; set; } = MatchMode.Regex;
        public bool ContainSubDirectory { get; set; }
        public DeleteMode DeleteMode { get; set; }
        public bool DeleteDirectoryIfEmpty { get; set; }

        public uint? SecondsAgo { get; set; } = 0;

        public override string ToString()
        {
            return $"IsEnable={IsEnable} Directory={Directory} Filter={Filter} MatchMode={MatchMode} ContainSubDirectory={ContainSubDirectory} DeleteMode={DeleteMode} SecondsAgo={SecondsAgo}";
        }

        private Regex _regex;
        private readonly object _locker = new object();

        public bool Match(FileInfo fileInfo)
        {
            if (fileInfo == null || !fileInfo.Exists) return false;
            switch (MatchMode)
            {
                default:
                case MatchMode.Normal:
                    throw new NotImplementedException();
                    break;
                case MatchMode.Regex:
                    if (_regex == null)
                    {
                        lock (_locker)
                        {
                            if (_regex == null)
                            {
                                var regex = new Regex(Filter, RegexOptions.Compiled, TimeSpan.FromSeconds(3));
                                _regex = regex;
                            }
                        }
                    }
                    return _regex.IsMatch(fileInfo.FullName);
                    break;
            }
        }
    }
}
