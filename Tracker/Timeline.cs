using System;
using System.Collections.Generic;


namespace Tracker {
    /// <summary>
    /// Exception for invalid event.
    /// </summary>
    public class InvalidEvent : Exception {
        public InvalidEvent() : base() { }
        public InvalidEvent(string message) : base(message) { }
        public InvalidEvent(string message, Exception innerException) : base(message, innerException) { }
    }


    /// <summary>
    /// A single event in the campaign timeline, along with the changes it brings about in the campaign state.
    /// </summary>
    [Serializable]
    public class Event {
        public Timestamp timestamp;
        public DateTime created;
        public String desc;
        public StateChange change;
    }


    /// <summary>
    /// A combination of a reference to a base state and the diff to that state.
    /// </summary>
    [Serializable]
    public class SkipDiff {
        public readonly int base_index;
        public StateChange change;

        public SkipDiff(int base_index, StateChange change) {
            this.base_index = base_index;
            this.change = change;
        }
    }


    /// <summary>
    /// A timeline of all of the events in the campaign so far.
    /// </summary>
    [Serializable]
    public class Timeline {
        public Timestamp timestamp;
        public State state;
        public List<Event> events;

        protected List<SkipDiff> skip_diffs;

        /// <summary>
        /// Get the index of the state at a specified timestamp.
        /// </summary>
        /// <param name="t">The timestamp for which to get an index</param>
        /// <returns>The index of the specified timestamp</returns>
        protected int _getStateIndex(Timestamp t) {
            int left = 0, right = this.events.Count;
            while (left < right) {
                if (t < this.events[left].timestamp) {
                    return left;
                }
                if (t >= this.events[right - 1].timestamp) {
                    return right;
                }
                int mid = (left + right) / 2;
                if (t < this.events[mid].timestamp) {
                    right = mid;
                }
                else {
                    left = mid;
                }
            }
            return left;
        }

        /// <summary>
        /// Recursively adjust provided state to the specified state index by reversing each skip diff in the chain.
        /// </summary>
        /// <param name="state">The state to adjust</param>
        /// <param name="idx">The index of the desired final state</param>
        protected void _adjustState(State state, int idx) {
            if (idx >= this.skip_diffs.Count) { return; }
            this._adjustState(state, this.skip_diffs[idx].base_index);
            if (!(this.skip_diffs[idx].change is null)) {
                this.skip_diffs[idx].change.revertFromState(state);
            }
            else {
                this.events[idx].change.revertFromState(state);
            }
        }

        /// <summary>
        /// Get campaign state at a specified timestamp.
        /// </summary>
        /// <param name="t">The timestamp of the state to retrieve</param>
        /// <returns>The state of the campaign at the specified timestamp</returns>
        public State getState(Timestamp t) {
            if ((this.events.Count <= 0) || (t < this.events[0].timestamp)) {
                return new State();
            }
            State state = this.state.copy();
            if (t < this.timestamp) {
                int idx = this._getStateIndex(t);
                this._adjustState(state, idx);
            }
            return state;
        }

        /// <summary>
        /// Add an event to the campaign timeline.
        /// 
        /// Add/remove/update skip diffs as necessary, and update the current campaign state if possible.
        /// Each valid state from the beginning of the campaign will have a skip diff. Once an invalid state is encountered,
        /// its skip diff and all subsequent skip diffs will be removed, marking those states invalid. If the addition of the
        /// new event causes a previously-invalid state to become valid again (e.g. by adding a resource it removes), the
        /// campaign state and all relevant skip diffs will be updated such that all valid states have skip diffs.
        /// In any case, the campaign timestamp will be set to that of the latest valid event, and the saved state will be
        /// updated to reflect the state as of that timestamp.
        /// </summary>
        /// <param name="e">The event to add</param>
        /// <exception cref="InvalidEvent">The event is malformed or shares a timestamp with an existing event</exception>
        /// <exception cref="InvalidState">The event's change cannot be applied to the state at the event's timestamp</exception>
        public void addEvent(Event e) {
            if (e.timestamp == this.timestamp) {
                throw new InvalidEvent("Already an event with this timestamp");
            }
            int idx = this._getStateIndex(e.timestamp);
            if ((idx > 0) && (e.timestamp == this.events[idx - 1].timestamp)) {
                throw new InvalidEvent("Already an event with this timestamp");
            }
            if ((this.events.Count > this.skip_diffs.Count) && (e.timestamp >= this.events[this.skip_diffs.Count].timestamp)) {
                // e falls after first invalid event; insert without updating state
                this.events.Insert(idx, e);
                return;
            }

            State state = this.state.copy();
            int validStates;
            if (e.timestamp > this.timestamp) {
                // appending an event to the end of the valid events
                e.change.applyToState(state);
                this.events.Insert(idx, e);
                //mark some skip diffs backwards for updating to idx
                // try to apply outstanding events that might've been made valid by this one
                for (validStates = idx + 1; validStates < this.events.Count; validStates++) {
                    State newState = state.copy();
                    try {
                        this.events[validStates].change.applyToState(newState);
                    }
                    catch (InvalidState) {
                        break;
                    }
                    state = newState;
                    //mark some skip diffs backwards for updating to validStates
                }
                //update marked skip diffs
                this.state = state;
                this.timestamp = this.events[validStates - 1].timestamp;
                return;
            }
            // inserting an event between valid events
            this._adjustState(state, idx);
            e.change.applyToState(state);
            this.events.Insert(idx, e);
            for (validStates = idx + 1; validStates < this.events.Count; validStates++) {
                State newState = state.copy();
                try {
                    this.events[validStates].change.applyToState(newState);
                }
                catch (InvalidState) {
                    break;
                }
                state = newState;
            }
            this.state = state;
            this.timestamp = this.events[validStates - 1].timestamp;
            if (this.skip_diffs.Count > validStates) {
                this.skip_diffs.RemoveRange(validStates, this.skip_diffs.Count - validStates);
            }
            List<StateChange> diffs = new List<StateChange>();
            StateChange diff = new StateChange();
            for (int i = validStates - 1; i >= this.skip_diffs.Count; i--) {
                //diff += this.events[i].change
                diffs.Add(diff);
            }
            for (int i = 0; i < validStates; i++) {
                if (i >= this.skip_diffs.Count) {
                    this.skip_diffs.Add(new SkipDiff(validStates, diffs[validStates - i - 1]));
                }
                else if ((this.skip_diffs[i].base_index > idx) && (this.skip_diffs[i].change != null)) {
                    //this.skip_diffs[i].change -= this.events[this.skip_diffs[i].base_index].change;
                    if (i <= idx) {
                        //this.skip_diffs[i].change += e.change;
                    }
                    else {
                        //this.skip_diffs[i].change += this.events[i].change;
                    }
                }
            }
        }

        //edit event (update position if necessary; update current timestamp/state; recompute skip diffs)
    }
}
