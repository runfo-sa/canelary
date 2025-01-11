using Dragablz;
using System.Windows;

namespace Main.Models;

public class InterTabClient : IInterTabClient
{
    public INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source)
    {
        var view = new Views.TabWindow();
        return new NewTabHost<Window>(view, view.Tabs);
    }

    public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window)
    {
        return TabEmptiedResponse.DoNothing;
    }
}