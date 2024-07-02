using Core.FileTree;
using Core.Models;

namespace Comparator.Models
{
    public class SelectionResult(LabelFile leftFile, LabelFile rightFile, LabelDpi dpi, LabelSize size)
    {
        public LabelFile LeftFile => leftFile;
        public LabelFile RightFile => rightFile;
        public LabelDpi Dpi => dpi;
        public LabelSize Size => size;
    }
}
