using System;
using System.Collections.Generic;


namespace Tracker {
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
                    // we shouldn't get here, but a missing tag shouldn't prevent a revert
                    continue;
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
                    // we shouldn't get here, but a missing tag shouldn't prevent a removal
                    continue;
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
