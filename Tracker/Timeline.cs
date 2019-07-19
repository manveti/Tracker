using System;
using System.Collections.Generic;


namespace Tracker {
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
        public readonly StateChange change;

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

        protected SortedList<Timestamp, SkipDiff> skip_diffs;

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
            this._adjustState(state, this.skip_diffs.Values[idx].base_index);
            state.revertChange(this.skip_diffs.Values[idx].change);
        }

        /// <summary>
        /// Get campaign state at a specified timestamp.
        /// </summary>
        /// <param name="t">The timestamp of the state to retrieve</param>
        /// <returns>The state of the campaign at the specified timestamp</returns>
        public State getState(Timestamp t) {
            State state = this.state.copy();
            int idx = this._getStateIndex(t);
            this._adjustState(state, idx);
            return state;
        }

        //add event (insert at proper position; update current timestamp/state; recompute skip diffs)
        //edit event (update position if necessary; update current timestamp/state; recompute skip diffs)
    }
}
