using System.Windows.Forms;

namespace DataEditorX.Controls
{
    public interface IMainForm
    {
        void CdbMenuClear();
        void AddCdbMenu(ToolStripItem item);
        void Open(string file);
    }
}
