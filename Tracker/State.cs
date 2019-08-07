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
    /// Exception for an invalid state aspect.
    /// </summary>
    public class InvalidAspect : Exception {
        public InvalidAspect() : base() { }
        public InvalidAspect(string message) : base(message) { }
        public InvalidAspect(string message, Exception innerException) : base(message, innerException) { }
    }


    /// <summary>
    /// Base class for aspects of campaign state.
    /// </summary>
    [Serializable]
    public abstract class StateAspect {
        public const string NOTE = "note";

        public HashSet<string> tags;

        public StateAspect() {
            this.tags = new HashSet<string>();
        }

        public StateAspect(StateAspect aspect) {
            this.tags = new HashSet<string>(aspect.tags);
        }

        public abstract StateAspect copy();
    }


    /// <summary>
    /// Simple text aspect for campaign notes.
    /// </summary>
    [Serializable]
    public class NoteAspect : StateAspect {
        public string content;

        public NoteAspect(string content) : base() {
            this.content = content;
        }

        public NoteAspect(NoteAspect aspect) : base(aspect) {
            this.content = aspect.content;
        }

        public override StateAspect copy() {
            return new NoteAspect(this);
        }
    }


    /// <summary>
    /// Storage for <see cref="StateAspect"/>s.
    /// </summary>
    [Serializable]
    public class StateAspectStore {
        public Dictionary<Guid, StateAspect> aspects;
        public Dictionary<string, HashSet<Guid>> tag_index;
        public Dictionary<Guid, StateAspect> removed_aspects;

        public StateAspectStore() {
            this.aspects = new Dictionary<Guid, StateAspect>();
            this.tag_index = new Dictionary<string, HashSet<Guid>>();
            this.removed_aspects = new Dictionary<Guid, StateAspect>();
        }

        public StateAspectStore copy() {
            StateAspectStore store = new StateAspectStore();
            foreach (KeyValuePair<Guid, StateAspect> aspect in this.aspects) {
                store.aspects[aspect.Key] = aspect.Value.copy();
            }
            foreach (KeyValuePair<string,HashSet<Guid>> tag in this.tag_index) {
                store.tag_index[tag.Key] = new HashSet<Guid>(tag.Value);
            }
            foreach (KeyValuePair<Guid, StateAspect> aspect in this.removed_aspects) {
                store.removed_aspects[aspect.Key] = aspect.Value.copy();
            }
            return store;
        }
    }


    /// <summary>
    /// Snapshot of campaign state.
    /// </summary>
    [Serializable]
    public class State {
        public StateAspectStore notes;
        //quests
        //characters
        //inventory

        public State() {
            this.notes = new StateAspectStore();
        }

        public State copy() {
            State state = new State();
            state.notes = this.notes.copy();
            return state;
        }

        /// <summary>
        /// Get the store for the specified aspect.
        /// </summary>
        /// <param name="aspect">The aspect store to get (one of <see cref="StateAspect"/>)</param>
        /// <returns>The aspect store</returns>
        /// <exception cref="InvalidAspect">The specified aspect was not valid</exception>
        public StateAspectStore getStore(string aspect) {
            switch (aspect) {
            case StateAspect.NOTE:
                return this.notes;
            }
            throw new InvalidAspect("Not a valid aspect type");
        }
    }
}
