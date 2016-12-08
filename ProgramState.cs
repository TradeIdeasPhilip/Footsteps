using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Footsteps
{
    /// <summary>
    /// We use this interface to connect StatusPanel objects to WorldView objects. 
    /// </summary>
    public interface ProgramStateProvider
    {
        ProgramState ProgramState { get; }
        event Action ProgramStateChanged;
    }

    /// <summary>
    /// The order of these states is important.  If you need to combine more than
    /// one state, you use the largest of the values.  For example, if your program sometimes
    /// wins and sometimes loses, the summary is that your program lost.
    /// </summary>
    public enum ProgramState { Won, Running, Lost }
}
