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
    /// Base class for aspects of campaign state.
    /// </summary>
    [Serializable]
    public class StateAspect {
        public HashSet<string> tags;

        public StateAspect() {
            this.tags = new HashSet<string>();
        }

        public StateAspect(StateAspect aspect) {
            this.tags = new HashSet<string>(aspect.tags);
        }

        public StateAspect copy() {
            return new StateAspect(this);
        }
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
    }


    /// <summary>
    /// Base class for a change to a <see cref="StateAspectStore"/>.
    /// </summary>
    [Serializable]
    public abstract class StateAspectChange {
        public readonly Guid aspect_id;

        public StateAspectChange(Guid aspect_id) {
            this.aspect_id = aspect_id;
        }

        /// <summary>
        /// Apply this change to the specified store.
        /// </summary>
        /// <param name="store">The store to which to apply the change</param>
        public abstract void applyToStore(StateAspectStore store);

        /// <summary>
        /// Revert this change from the specified store.
        /// </summary>
        /// <param name="store">The store from which to revert the change</param>
        public abstract void revertFromStore(StateAspectStore store);
    }


    /// <summary>
    /// Change which adds a <see cref="StateAspect"/>.
    /// </summary>
    [Serializable]
    public class StateAspectAdd : StateAspectChange {
        public readonly StateAspect aspect;

        public StateAspectAdd(StateAspect aspect) : base(Guid.NewGuid()) {
            this.aspect = aspect;
        }

        public override void applyToStore(StateAspectStore store) {
            if ((store.aspects.ContainsKey(this.aspect_id)) || (store.removed_aspects.ContainsKey(this.aspect_id))) {
                throw new InvalidState("Cannot add aspect that already exists.");
            }
            store.aspects[this.aspect_id] = this.aspect.copy();
            foreach (string tag in aspect.tags) {
                if (!store.tag_index.ContainsKey(tag)) {
                    store.tag_index[tag] = new HashSet<Guid>();
                }
                store.tag_index[tag].Add(this.aspect_id);
            }
        }

        public override void revertFromStore(StateAspectStore store) {
            if (!store.aspects.ContainsKey(this.aspect_id)) {
                throw new InvalidState("Cannot revert adding aspect that doesn't exist.");
            }
            foreach (string tag in store.aspects[this.aspect_id].tags) {
                if ((!store.tag_index.ContainsKey(tag)) || (!store.tag_index[tag].Contains(this.aspect_id))) {
                    throw new InvalidState("Cannot revert adding tag that doesn't exist.");
                }
                store.tag_index[tag].Remove(this.aspect_id);
                if (store.tag_index[tag].Count <= 0) {
                    store.tag_index.Remove(tag);
                }
            }
            store.aspects.Remove(this.aspect_id);
        }
    }


    /// <summary>
    /// Change which removes a <see cref="StateAspect"/>.
    /// </summary>
    [Serializable]
    public class StateAspectRemove : StateAspectChange {
        public StateAspectRemove(Guid aspect_id) : base(aspect_id) { }

        public override void applyToStore(StateAspectStore store) {
            if ((!store.aspects.ContainsKey(this.aspect_id)) || (store.removed_aspects.ContainsKey(this.aspect_id))) {
                throw new InvalidState("Cannot remove aspect that doesn't exist.");
            }
            foreach (string tag in store.aspects[this.aspect_id].tags) {
                if ((!store.tag_index.ContainsKey(tag)) || (!store.tag_index[tag].Contains(this.aspect_id))) {
                    throw new InvalidState("Cannot revert adding tag that doesn't exist.");
                }
                store.tag_index[tag].Remove(this.aspect_id);
                if (store.tag_index[tag].Count <= 0) {
                    store.tag_index.Remove(tag);
                }
            }
            store.removed_aspects[this.aspect_id] = store.aspects[this.aspect_id];
            store.aspects.Remove(this.aspect_id);
        }

        public override void revertFromStore(StateAspectStore store) {
            if ((store.aspects.ContainsKey(this.aspect_id)) || (!store.removed_aspects.ContainsKey(this.aspect_id))) {
                throw new InvalidState("Cannot revert removing aspect that hasn't been removed.");
            }
            store.aspects[this.aspect_id] = store.removed_aspects[this.aspect_id];
            store.removed_aspects.Remove(this.aspect_id);
            foreach (string tag in store.aspects[this.aspect_id].tags) {
                if (!store.tag_index.ContainsKey(tag)) {
                    store.tag_index[tag] = new HashSet<Guid>();
                }
                store.tag_index[tag].Add(this.aspect_id);
            }
        }
    }


    /// <summary>
    /// Diff between two campaign states.
    /// </summary>
    [Serializable]
    public class StateChange {
        public void applyToState(State state) {
            //TODO: apply change; raise InvalidState on error
        }

        public void revertFromState(State state) {
            //TODO: apply change in reverse; raise InvalidState on error
        }
    }
}
