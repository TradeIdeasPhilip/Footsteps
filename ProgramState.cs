using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Footsteps
{
    public interface ProgramStateProvider
    {
        ProgramState ProgramState { get; }
        event Action ProgramStateChanged;
    }

    public enum ProgramState { Won, Running, Lost }
}
