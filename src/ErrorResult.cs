using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorCatcher
{
    public class ErrorResult
    {
        public ErrorResult(string fileName)
        {
            FileName = fileName;
        }

        public string FileName { get; set; }
        public int Errors { get; set; }
        public int Warnings { get; set; }
        public int Info { get; set; }
    }
}
