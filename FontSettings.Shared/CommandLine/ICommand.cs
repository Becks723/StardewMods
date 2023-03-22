using System;
using System.Collections.Generic;
using System.Text;

namespace FontSettings.Shared.CommandLine
{
    internal interface ICommand
    {
        ICommandRunner GetRunner();
    }
}
