using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GuiLabs.Undo;

namespace JunimoStudio.Actions
{
    /// <summary>Represents an <see cref="IAction"/> with a name.</summary>
    internal interface INamedAction : IAction
    {
        /// <summary>Gets or sets name of the action.</summary>
        string Name { get; set; }
    }
}
