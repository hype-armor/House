using System;
using System.Collections.Generic;

namespace PluginContracts
{
    public interface IPlugin
    {
        string Name { get; }
        string Go(string input);
        List<string> Actions { get; }
    }
}
