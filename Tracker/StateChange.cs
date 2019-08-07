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
        /// <exception cref="InvalidState">The change cannot be applied to the store</exception>
        public abstract void applyToStore(StateAspectStore store);

        /// <summary>
        /// Revert this change from the specified store.
        /// </summary>
        /// <param name="store">The store from which to revert the change</param>
        /// <exception cref="InvalidState">The change cannot be reverted from the store</exception>
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
    /// Change which updates a <see cref="StateAspect"/>.
    /// </summary>
    [Serializable]
    public class StateAspectUpdate : StateAspectChange {
        public StateAspectUpdate(Guid aspect_id) : base(aspect_id) {
            //TODO
        }

        public override void applyToStore(StateAspectStore store) {
            //TODO
        }

        public override void revertFromStore(StateAspectStore store) {
            //TODO
        }
    }


    /// <summary>
    /// Diff between two campaign states.
    /// </summary>
    [Serializable]
    public class StateChange {
        public Dictionary<string, Dictionary<Guid, StateAspectAdd>> additions;
        public Dictionary<string, Dictionary<Guid, StateAspectUpdate>> updates;
        public Dictionary<string, Dictionary<Guid, StateAspectRemove>> removals;

        public StateChange() {
            this.additions = new Dictionary<string, Dictionary<Guid, StateAspectAdd>>();
            this.updates = new Dictionary<string, Dictionary<Guid, StateAspectUpdate>>();
            this.removals = new Dictionary<string, Dictionary<Guid, StateAspectRemove>>();
        }

        /// <summary>
        /// Add a <see cref="StateAspectChange"/> to this change.
        /// </summary>
        /// <param name="aspect">The aspect type (see <see cref="StateAspect"/>)</param>
        /// <param name="state_change">The change being added</param>
        /// <exception cref="InvalidAspect">The aspect is already being modified by this change</exception>
        public void addChange(string aspect, StateAspectChange state_change) {
            if ((this.additions.ContainsKey(aspect)) && (this.additions[aspect].ContainsKey(state_change.aspect_id))) {
                throw new InvalidAspect("Change already adds aspect");
            }
            if ((this.updates.ContainsKey(aspect)) && (this.updates[aspect].ContainsKey(state_change.aspect_id))) {
                throw new InvalidAspect("Change already updates aspect");
            }
            if ((this.removals.ContainsKey(aspect)) && (this.removals[aspect].ContainsKey(state_change.aspect_id))) {
                throw new InvalidAspect("Change already removes aspect");
            }

            StateAspectAdd aspectAdd = state_change as StateAspectAdd;
            if (aspectAdd != null) {
                if (!this.additions.ContainsKey(aspect)) {
                    this.additions[aspect] = new Dictionary<Guid, StateAspectAdd>();
                }
                this.additions[aspect][state_change.aspect_id] = aspectAdd;
                return;
            }

            StateAspectUpdate aspectUpdate = state_change as StateAspectUpdate;
            if (aspectUpdate != null) {
                if (!this.updates.ContainsKey(aspect)) {
                    this.updates[aspect] = new Dictionary<Guid, StateAspectUpdate>();
                }
                this.updates[aspect][state_change.aspect_id] = aspectUpdate;
                return;
            }

            StateAspectRemove aspectRemove = state_change as StateAspectRemove;
            if (aspectRemove != null) {
                if (!this.removals.ContainsKey(aspect)) {
                    this.removals[aspect] = new Dictionary<Guid, StateAspectRemove>();
                }
                this.removals[aspect][state_change.aspect_id] = aspectRemove;
                return;
            }

            throw new InvalidAspect("Invalid aspect change type");
        }

        /// <summary>
        /// Remove a <see cref="StateAspectChange"/> from this change.
        /// </summary>
        /// <param name="aspect">The aspect type (see <see cref="StateAspect"/>)</param>
        /// <param name="aspect_id">Aspect ID of the change being removed</param>
        /// <exception cref="InvalidAspect">The aspect isn't being modified by this change</exception>
        public void removeChange(string aspect, Guid aspect_id) {
            if ((this.additions.ContainsKey(aspect)) && (this.additions[aspect].ContainsKey(aspect_id))) {
                this.additions[aspect].Remove(aspect_id);
                if (this.additions[aspect].Count <= 0) {
                    this.additions.Remove(aspect);
                }
                return;
            }
            if ((this.updates.ContainsKey(aspect)) && (this.updates[aspect].ContainsKey(aspect_id))) {
                this.updates[aspect].Remove(aspect_id);
                if (this.updates[aspect].Count <= 0) {
                    this.updates.Remove(aspect);
                }
                return;
            }
            if ((this.removals.ContainsKey(aspect)) && (this.removals[aspect].ContainsKey(aspect_id))) {
                this.removals[aspect].Remove(aspect_id);
                if (this.removals[aspect].Count <= 0) {
                    this.removals.Remove(aspect);
                }
                return;
            }

            if (
                (this.additions.ContainsKey(aspect)) ||
                (this.updates.ContainsKey(aspect)) ||
                (this.removals.ContainsKey(aspect))
            ) {
                throw new InvalidAspect("Not a valid aspect");
            }
            throw new InvalidAspect("Not a valid aspect type");
        }

        /// <summary>
        /// Apply this change to the specified state.
        /// </summary>
        /// <param name="state">The state to which to apply the change</param>
        /// <exception cref="InvalidState">The change cannot be applied to the state</exception>
        public void applyToState(State state) {
            // first apply all the additions
            foreach (KeyValuePair<string, Dictionary<Guid, StateAspectAdd>> changeDict in this.additions) {
                StateAspectStore store = state.getStore(changeDict.Key);
                foreach (KeyValuePair<Guid, StateAspectAdd> stateChange in changeDict.Value) {
                    stateChange.Value.applyToStore(store);
                }
            }
            // then apply all the updates
            foreach (KeyValuePair<string, Dictionary<Guid, StateAspectUpdate>> changeDict in this.updates) {
                StateAspectStore store = state.getStore(changeDict.Key);
                foreach (KeyValuePair<Guid, StateAspectUpdate> stateChange in changeDict.Value) {
                    stateChange.Value.applyToStore(store);
                }
            }
            // then apply all the removals
            foreach (KeyValuePair<string, Dictionary<Guid, StateAspectRemove>> changeDict in this.removals) {
                StateAspectStore store = state.getStore(changeDict.Key);
                foreach (KeyValuePair<Guid, StateAspectRemove> stateChange in changeDict.Value) {
                    stateChange.Value.applyToStore(store);
                }
            }
            // finally do any necessary cross-store validation
        }

        /// <summary>
        /// Revert this change from the specified state.
        /// </summary>
        /// <param name="state">The state from which to revert the change</param>
        /// <exception cref="InvalidState">The change cannot be reverted from the state</exception>
        public void revertFromState(State state) {
            // first revert all the removals
            foreach (KeyValuePair<string, Dictionary<Guid, StateAspectRemove>> changeDict in this.removals) {
                StateAspectStore store = state.getStore(changeDict.Key);
                foreach (KeyValuePair<Guid, StateAspectRemove> stateChange in changeDict.Value) {
                    stateChange.Value.revertFromStore(store);
                }
            }
            // then revert all the updates
            foreach (KeyValuePair<string, Dictionary<Guid, StateAspectUpdate>> changeDict in this.updates) {
                StateAspectStore store = state.getStore(changeDict.Key);
                foreach (KeyValuePair<Guid, StateAspectUpdate> stateChange in changeDict.Value) {
                    stateChange.Value.revertFromStore(store);
                }
            }
            // then revert all the additions
            foreach (KeyValuePair<string, Dictionary<Guid, StateAspectAdd>> changeDict in this.additions) {
                StateAspectStore store = state.getStore(changeDict.Key);
                foreach (KeyValuePair<Guid, StateAspectAdd> stateChange in changeDict.Value) {
                    stateChange.Value.revertFromStore(store);
                }
            }
            // finally do any necessary cross-store validation
        }
    }
}
