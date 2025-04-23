using Gamanet.C4.Client.Panels.DemoPanel.WPF.Windows.Interfaces;
using Gamanet.C4.Client.Panels.DemoPanel.WPF.Windows.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamanet.C4.Client.Panels.DemoPanel.WPF.Windows.Model
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    internal class _DemoPanelContext : IDemoPanelContext
    {
        public PersonRepository PersonRepo { get; }

        public _DemoPanelContext()
        {
            this.PersonRepo = new PersonRepository();

        }
    }
}
