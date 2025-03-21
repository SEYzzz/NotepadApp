using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notepad
{
    public class Note
    {
        public string Text { get; set; } = string.Empty;
        public string RtfText { get; set; } = string.Empty;
        public string Title { get; set; } = "Новая заметка";
        public string FilePath { get; set; } = string.Empty;
        public bool IsModified { get; set; } = false;
    }
}
