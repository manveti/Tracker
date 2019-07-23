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
                state.revertChange(this.skip_diffs[idx].change);
            }
            else {
                state.revertChange(this.events[idx].change);
            }
        }

        /// <summary>
        /// Get campaign state at a specified timestamp.
        /// </summary>
        /// <param name="t">The timestamp of the state to retrieve</param>
        /// <returns>The state of the campaign at the specified timestamp</returns>
        public State getState(Timestamp t) {
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
        public void addEvent(Event e) {
            //if in bad state and e.timestamp >= first bad event's timestamp: insert without any updating; return
            //if e.timestamp > this.timestamp: (append case)
            //  try to apply e; reject on failure
            //  insert e
            //  mark some skip diffs backwards for updating to insert index
            //  while in bad state:
            //    try to apply next event; break on failure
            //    update this.timestamp to event.timestamp
            //    mark some skip diffs backwards for updating to event index
            //  update marked skip diffs
            //else: (insert case)
            //  get state and insert index at e.timestamp
            //  try to apply e to state; reject on failure
            //  for each following event:
            //    try to apply next event; on failure: truncate this and remaining skip diffs; break
            //  update this.timestamp to timestamp of last good event
            //  insert e; insert skip diff for e based on state
            //  for each valid event:
            //    if skip diff after last good: update skip diff to state
            //    elif skip diff base_index >= insert index:
            //      if event before e: add e.change to skip diff
            //      increment skip diff base_index
        }

        //edit event (update position if necessary; update current timestamp/state; recompute skip diffs)
    }
}
