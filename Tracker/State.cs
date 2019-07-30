using System;
using System.Collections.Generic;


namespace Tracker {
    /// <summary>
    /// Exception for invalid state or change which invalidates state.
    /// </summary>
    public class InvalidState : Exception {
        public InvalidState() : base() { }
        public InvalidState(string message) : base(message) { }
        public InvalidState(string message, Exception innerException) : base(message, innerException) { }
    }


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
            //TODO: apply change; raise InvalidState on error
        }

        public void revertChange(StateChange change) {
            //TODO: apply change in reverse; raise InvalidState on error
        }
    }


    /// <summary>
    /// Diff between two campaign states.
    /// </summary>
    [Serializable]
    public class StateChange {
    }
}
