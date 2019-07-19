using System;
using System.Collections.Generic;


namespace Tracker {
    /// <summary>
    /// Snapshot of campaign state.
    /// </summary>
    [Serializable]
    public class State {
        public State copy() {
            //TODO: create a deep copy of this
            return new State();
        }

        public void applyChange(StateChange change) {
            //TODO: apply change
        }

        public void revertChange(StateChange change) {
            //TODO: apply change in reverse
        }
    }


    /// <summary>
    /// Diff between two campaign states.
    /// </summary>
    [Serializable]
    public class StateChange {
    }
}
