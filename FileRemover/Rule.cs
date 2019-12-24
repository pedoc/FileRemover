using System;
using System.Collections.Generic;
using System.Text;

namespace FileRemover
{
    public class Rule
    {
        public string Directory { get; set; }
        public string Filter { get; set; }
        public MatchMode MatchMode { get; set; }
        public bool ContainSubDirectory { get; set; }
    }
}
