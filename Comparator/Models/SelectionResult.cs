using Core.FileTree;
using Core.Models;

namespace Comparator.Models
{
    public class SelectionResult(IFile leftFile, IFile rightFile, LabelDpi dpi, LabelSize size)
    {
        public IFile LeftFile => leftFile;
        public IFile RightFile => rightFile;
        public LabelDpi Dpi => dpi;
        public LabelSize Size => size;
    }
}
