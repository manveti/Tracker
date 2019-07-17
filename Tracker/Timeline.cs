using System;
using System.Collections.Generic;


namespace Tracker {
    /// <summary>
    /// A single event in the campaign timeline, along with the changes it brings about in the campaign state.
    /// </summary>
    public class Event {
        public Timestamp timestamp;
        public DateTime created;
        public String desc;
        public StateChange change;
    }


    /// <summary>
    /// A timeline of all of the events in the campaign so far.
    /// </summary>
    public class Timeline {
        //current (latest) timestamp
        //current (latest) state
        //list of events (sorted)
        //skip diffs

        //get state at timestamp
        //add event (insert at proper position; update current timestamp/state; recompute skip diffs)
    }
}
