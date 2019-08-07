using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tracker;
using System;
using System.Collections.Generic;


namespace Tracker.Tests {
    [TestClass]
    public class StateAspectAddTests {
        [TestMethod]
        public void test_add() {
            StateAspectStore store = new StateAspectStore();
            NoteAspect note = new NoteAspect("This is a test note");
            note.tags.Add("tag1");
            note.tags.Add("tag2");
            StateAspectAdd noteAdd = new StateAspectAdd(note);

            // apply add and verify it worked
            noteAdd.applyToStore(store);
            Assert.IsTrue(store.aspects.ContainsKey(noteAdd.aspect_id));
            Assert.AreEqual(((NoteAspect)(store.aspects[noteAdd.aspect_id])).content, note.content);
            Assert.AreEqual(store.tag_index.Count, note.tags.Count);
            foreach (string tag in note.tags) {
                Assert.IsTrue(store.tag_index.ContainsKey(tag));
                Assert.AreEqual(store.tag_index[tag].Count, 1);
                Assert.IsTrue(store.tag_index[tag].Contains(noteAdd.aspect_id));
            }
            Assert.AreEqual(store.removed_aspects.Count, 0);

            // verify applying add a second time fails
            Assert.ThrowsException<InvalidState>(() => { noteAdd.applyToStore(store); });

            // revert add and verify revert worked
            noteAdd.revertFromStore(store);
            Assert.AreEqual(store.aspects.Count, 0);
            Assert.AreEqual(store.tag_index.Count, 0);
            Assert.AreEqual(store.removed_aspects.Count, 0);

            // verify reverting add a second time fails
            Assert.ThrowsException<InvalidState>(() => { noteAdd.revertFromStore(store); });
        }

        [TestMethod]
        public void test_add_multiple() {
            StateAspectStore store = new StateAspectStore();
            NoteAspect note1 = new NoteAspect("This is a test note"), note2 = new NoteAspect("This is another test note");
            note1.tags.Add("common_tag");
            note1.tags.Add("tag1");
            note2.tags.Add("common_tag");
            note2.tags.Add("tag2");
            StateAspectAdd note1Add = new StateAspectAdd(note1), note2Add = new StateAspectAdd(note2);

            // apply both changes to store and verify both are in store
            note1Add.applyToStore(store);
            note2Add.applyToStore(store);
            Assert.IsTrue(store.aspects.ContainsKey(note1Add.aspect_id));
            Assert.IsTrue(store.aspects.ContainsKey(note2Add.aspect_id));
            Assert.AreEqual(((NoteAspect)(store.aspects[note1Add.aspect_id])).content, note1.content);
            Assert.AreEqual(((NoteAspect)(store.aspects[note2Add.aspect_id])).content, note2.content);
            Assert.AreEqual(store.tag_index.Count, 3);
            foreach (KeyValuePair<string, HashSet<Guid>> tag_index in store.tag_index) {
                Assert.AreEqual(tag_index.Value.Contains(note1Add.aspect_id), note1.tags.Contains(tag_index.Key));
                Assert.AreEqual(tag_index.Value.Contains(note2Add.aspect_id), note2.tags.Contains(tag_index.Key));
            }
            Assert.AreEqual(store.removed_aspects.Count, 0);

            // revert one change and verify other change still present
            note1Add.revertFromStore(store);
            Assert.AreEqual(store.aspects.Count, 1);
            Assert.IsFalse(store.aspects.ContainsKey(note1Add.aspect_id));
            Assert.IsTrue(store.aspects.ContainsKey(note2Add.aspect_id));
            Assert.AreEqual(((NoteAspect)(store.aspects[note2Add.aspect_id])).content, note2.content);
            Assert.AreEqual(store.tag_index.Count, note2.tags.Count);
            foreach (string tag in note2.tags) {
                Assert.IsTrue(store.tag_index.ContainsKey(tag));
                Assert.AreEqual(store.tag_index[tag].Count, 1);
                Assert.IsTrue(store.tag_index[tag].Contains(note2Add.aspect_id));
            }
            Assert.AreEqual(store.removed_aspects.Count, 0);

            // verify reverting same change a second time fails
            Assert.ThrowsException<InvalidState>(() => { note1Add.revertFromStore(store); });

            // revert second change and verify revert worked
            note2Add.revertFromStore(store);
            Assert.AreEqual(store.aspects.Count, 0);
            Assert.AreEqual(store.tag_index.Count, 0);
            Assert.AreEqual(store.removed_aspects.Count, 0);
        }

        [TestMethod]
        public void test_tag_index_changes() {
            StateAspectStore store, base_store = new StateAspectStore();
            NoteAspect note = new NoteAspect("This is a test note");
            note.tags.Add("tag1");
            note.tags.Add("tag2");
            StateAspectAdd noteAdd = new StateAspectAdd(note);
            noteAdd.applyToStore(base_store);
            Guid otherGuid = Guid.NewGuid();

            // add a tag and verify revert handles it correctly
            store = base_store.copy();
            store.tag_index["tag1"].Add(otherGuid);
            noteAdd.revertFromStore(store);
            Assert.AreEqual(store.aspects.Count, 0);
            Assert.AreEqual(store.tag_index.Count, 1);
            Assert.IsTrue(store.tag_index.ContainsKey("tag1"));
            Assert.AreEqual(store.tag_index["tag1"].Count, 1);
            Assert.IsTrue(store.tag_index["tag1"].Contains(otherGuid));
            Assert.AreEqual(store.removed_aspects.Count, 0);

            // remove added aspect from tag index and verify revert works anyway
            store = base_store.copy();
            store.tag_index["tag1"].Add(otherGuid);
            store.tag_index["tag1"].Remove(noteAdd.aspect_id);
            noteAdd.revertFromStore(store);
            Assert.AreEqual(store.aspects.Count, 0);
            Assert.AreEqual(store.tag_index.Count, 1);
            Assert.IsTrue(store.tag_index.ContainsKey("tag1"));
            Assert.AreEqual(store.tag_index["tag1"].Count, 1);
            Assert.IsTrue(store.tag_index["tag1"].Contains(otherGuid));
            Assert.AreEqual(store.removed_aspects.Count, 0);

            // remove a necessary tag and verify revert works anyway
            store = base_store.copy();
            store.tag_index.Remove("tag1");
            noteAdd.revertFromStore(store);
            Assert.AreEqual(store.aspects.Count, 0);
            Assert.AreEqual(store.tag_index.Count, 0);
            Assert.AreEqual(store.removed_aspects.Count, 0);
        }
    }


    [TestClass]
    public class StateAspectRemoveTests {
        [TestMethod]
        public void test_remove() {
            StateAspectStore store = new StateAspectStore();
            NoteAspect note = new NoteAspect("This is a test note");
            note.tags.Add("tag1");
            note.tags.Add("tag2");
            StateAspectAdd noteAdd = new StateAspectAdd(note);
            noteAdd.applyToStore(store);
            StateAspectRemove noteRemove = new StateAspectRemove(noteAdd.aspect_id);

            // apply remove and verify it worked
            noteRemove.applyToStore(store);
            Assert.AreEqual(store.aspects.Count, 0);
            Assert.AreEqual(store.tag_index.Count, 0);
            Assert.AreEqual(store.removed_aspects.Count, 1);
            Assert.IsTrue(store.removed_aspects.ContainsKey(noteAdd.aspect_id));
            Assert.AreEqual(((NoteAspect)(store.removed_aspects[noteAdd.aspect_id])).content, note.content);

            // verify applying remove a second time fails
            Assert.ThrowsException<InvalidState>(() => { noteRemove.applyToStore(store); });

            // revert remove and verify revert worked
            noteRemove.revertFromStore(store);
            Assert.AreEqual(store.aspects.Count, 1);
            Assert.AreEqual(store.tag_index.Count, note.tags.Count);
            foreach (string tag in note.tags) {
                Assert.IsTrue(store.tag_index.ContainsKey(tag));
                Assert.AreEqual(store.tag_index[tag].Count, 1);
                Assert.IsTrue(store.tag_index[tag].Contains(noteAdd.aspect_id));
            }
            Assert.AreEqual(store.removed_aspects.Count, 0);

            // verify reverting remove a second time fails
            Assert.ThrowsException<InvalidState>(() => { noteRemove.revertFromStore(store); });
        }

        [TestMethod]
        public void test_remove_multiple() {
            StateAspectStore store = new StateAspectStore();
            NoteAspect note1 = new NoteAspect("This is a test note"), note2 = new NoteAspect("This is another test note");
            note1.tags.Add("common_tag");
            note1.tags.Add("tag1");
            note2.tags.Add("common_tag");
            note2.tags.Add("tag2");
            StateAspectAdd note1Add = new StateAspectAdd(note1), note2Add = new StateAspectAdd(note2);
            note1Add.applyToStore(store);
            note2Add.applyToStore(store);
            StateAspectRemove note1Remove = new StateAspectRemove(note1Add.aspect_id);
            StateAspectRemove note2Remove = new StateAspectRemove(note2Add.aspect_id);

            // apply first change and verify second note still present
            note1Remove.applyToStore(store);
            Assert.AreEqual(store.aspects.Count, 1);
            Assert.IsFalse(store.aspects.ContainsKey(note1Add.aspect_id));
            Assert.IsTrue(store.aspects.ContainsKey(note2Add.aspect_id));
            Assert.AreEqual(((NoteAspect)(store.aspects[note2Add.aspect_id])).content, note2.content);
            Assert.AreEqual(store.tag_index.Count, note2.tags.Count);
            foreach (string tag in note2.tags) {
                Assert.IsTrue(store.tag_index.ContainsKey(tag));
                Assert.AreEqual(store.tag_index[tag].Count, 1);
                Assert.IsTrue(store.tag_index[tag].Contains(note2Add.aspect_id));
            }
            Assert.AreEqual(store.removed_aspects.Count, 1);
            Assert.IsTrue(store.removed_aspects.ContainsKey(note1Add.aspect_id));
            Assert.IsFalse(store.removed_aspects.ContainsKey(note2Add.aspect_id));
            Assert.AreEqual(((NoteAspect)(store.removed_aspects[note1Add.aspect_id])).content, note1.content);

            // revert first change and verify both notes present
            note1Remove.revertFromStore(store);
            Assert.IsTrue(store.aspects.ContainsKey(note1Add.aspect_id));
            Assert.IsTrue(store.aspects.ContainsKey(note2Add.aspect_id));
            Assert.AreEqual(((NoteAspect)(store.aspects[note1Add.aspect_id])).content, note1.content);
            Assert.AreEqual(((NoteAspect)(store.aspects[note2Add.aspect_id])).content, note2.content);
            Assert.AreEqual(store.tag_index.Count, 3);
            foreach (KeyValuePair<string, HashSet<Guid>> tag_index in store.tag_index) {
                Assert.AreEqual(tag_index.Value.Contains(note1Add.aspect_id), note1.tags.Contains(tag_index.Key));
                Assert.AreEqual(tag_index.Value.Contains(note2Add.aspect_id), note2.tags.Contains(tag_index.Key));
            }
            Assert.AreEqual(store.removed_aspects.Count, 0);

            // verify reverting same change a second time fails
            Assert.ThrowsException<InvalidState>(() => { note1Remove.revertFromStore(store); });

            // apply both changes and verify both notes removed
            note1Remove.applyToStore(store);
            note2Remove.applyToStore(store);
            Assert.AreEqual(store.aspects.Count, 0);
            Assert.AreEqual(store.tag_index.Count, 0);
            Assert.AreEqual(store.removed_aspects.Count, 2);
            Assert.IsTrue(store.removed_aspects.ContainsKey(note1Add.aspect_id));
            Assert.IsTrue(store.removed_aspects.ContainsKey(note2Add.aspect_id));
            Assert.AreEqual(((NoteAspect)(store.removed_aspects[note1Add.aspect_id])).content, note1.content);
            Assert.AreEqual(((NoteAspect)(store.removed_aspects[note2Add.aspect_id])).content, note2.content);
        }

        [TestMethod]
        public void test_tag_index_changes() {
            StateAspectStore store, base_store = new StateAspectStore();
            NoteAspect note = new NoteAspect("This is a test note");
            note.tags.Add("tag1");
            note.tags.Add("tag2");
            StateAspectAdd noteAdd = new StateAspectAdd(note);
            noteAdd.applyToStore(base_store);
            StateAspectRemove noteRemove = new StateAspectRemove(noteAdd.aspect_id);
            Guid otherGuid = Guid.NewGuid();

            // add a tag and verify remove handles it correctly
            store = base_store.copy();
            store.tag_index["tag1"].Add(otherGuid);
            noteRemove.applyToStore(store);
            Assert.AreEqual(store.aspects.Count, 0);
            Assert.AreEqual(store.tag_index.Count, 1);
            Assert.IsTrue(store.tag_index.ContainsKey("tag1"));
            Assert.AreEqual(store.tag_index["tag1"].Count, 1);
            Assert.IsTrue(store.tag_index["tag1"].Contains(otherGuid));
            Assert.AreEqual(store.removed_aspects.Count, 1);
            Assert.IsTrue(store.removed_aspects.ContainsKey(noteAdd.aspect_id));

            // remove added aspect from tag index and verify remove works anyway
            store = base_store.copy();
            store.tag_index["tag1"].Add(otherGuid);
            store.tag_index["tag1"].Remove(noteAdd.aspect_id);
            noteRemove.applyToStore(store);
            Assert.AreEqual(store.aspects.Count, 0);
            Assert.AreEqual(store.tag_index.Count, 1);
            Assert.IsTrue(store.tag_index.ContainsKey("tag1"));
            Assert.AreEqual(store.tag_index["tag1"].Count, 1);
            Assert.IsTrue(store.tag_index["tag1"].Contains(otherGuid));
            Assert.AreEqual(store.removed_aspects.Count, 1);
            Assert.IsTrue(store.removed_aspects.ContainsKey(noteAdd.aspect_id));

            // remove a necessary tag and verify remove works anyway
            store = base_store.copy();
            store.tag_index.Remove("tag1");
            noteRemove.applyToStore(store);
            Assert.AreEqual(store.aspects.Count, 0);
            Assert.AreEqual(store.tag_index.Count, 0);
            Assert.AreEqual(store.removed_aspects.Count, 1);
            Assert.IsTrue(store.removed_aspects.ContainsKey(noteAdd.aspect_id));
        }
    }


    [TestClass]
    public class StateAspectUpdateTests {
        //TODO
    }


    [TestClass]
    public class StateChangeTests {
        [TestMethod]
        public void test_add_remove_changes() {
            NoteAspect note1 = new NoteAspect("This is a test note"), note2 = new NoteAspect("This is another test note");
            StateAspectAdd note1Add = new StateAspectAdd(note1), note2Add = new StateAspectAdd(note2);
            Guid updateGuid = Guid.NewGuid(), removeGuid = Guid.NewGuid();
            StateAspectRemove noteRemove = new StateAspectRemove(removeGuid);
            //TODO: update
            StateChange stateChange = new StateChange();

            Assert.AreEqual(stateChange.additions.Count, 0);
            Assert.AreEqual(stateChange.updates.Count, 0);
            Assert.AreEqual(stateChange.removals.Count, 0);

            // add an addition
            stateChange.addChange(StateAspect.NOTE, note1Add);
            Assert.AreEqual(stateChange.additions.Count, 1);
            Assert.IsTrue(stateChange.additions.ContainsKey(StateAspect.NOTE));
            Assert.AreEqual(stateChange.additions[StateAspect.NOTE].Count, 1);
            Assert.IsTrue(stateChange.additions[StateAspect.NOTE].ContainsKey(note1Add.aspect_id));
            Assert.AreEqual(stateChange.additions[StateAspect.NOTE][note1Add.aspect_id], note1Add);

            // add a second addition
            stateChange.addChange(StateAspect.NOTE, note2Add);
            Assert.AreEqual(stateChange.additions.Count, 1);
            Assert.IsTrue(stateChange.additions.ContainsKey(StateAspect.NOTE));
            Assert.AreEqual(stateChange.additions[StateAspect.NOTE].Count, 2);
            Assert.IsTrue(stateChange.additions[StateAspect.NOTE].ContainsKey(note2Add.aspect_id));
            Assert.AreEqual(stateChange.additions[StateAspect.NOTE][note2Add.aspect_id], note2Add);

            //TODO: add an update

            // add a removal
            stateChange.addChange(StateAspect.NOTE, noteRemove);
            Assert.AreEqual(stateChange.removals.Count, 1);
            Assert.IsTrue(stateChange.removals.ContainsKey(StateAspect.NOTE));
            Assert.AreEqual(stateChange.removals[StateAspect.NOTE].Count, 1);
            Assert.IsTrue(stateChange.removals[StateAspect.NOTE].ContainsKey(noteRemove.aspect_id));
            Assert.AreEqual(stateChange.removals[StateAspect.NOTE][noteRemove.aspect_id], noteRemove);

            // remove the first addition
            stateChange.removeChange(StateAspect.NOTE, note1Add.aspect_id);
            Assert.AreEqual(stateChange.additions.Count, 1);
            Assert.IsTrue(stateChange.additions.ContainsKey(StateAspect.NOTE));
            Assert.AreEqual(stateChange.additions[StateAspect.NOTE].Count, 1);
            Assert.IsTrue(stateChange.additions[StateAspect.NOTE].ContainsKey(note2Add.aspect_id));
            //TODO: Assert.AreEqual(stateChange.updates.Count, 1);
            Assert.AreEqual(stateChange.removals.Count, 1);

            // remove the second addition
            stateChange.removeChange(StateAspect.NOTE, note2Add.aspect_id);
            Assert.AreEqual(stateChange.additions.Count, 0);
            //TODO: Assert.AreEqual(stateChange.updates.Count, 1);
            Assert.AreEqual(stateChange.removals.Count, 1);

            //TODO: remove the update

            // remove the removal
            stateChange.removeChange(StateAspect.NOTE, noteRemove.aspect_id);
            Assert.AreEqual(stateChange.additions.Count, 0);
            Assert.AreEqual(stateChange.updates.Count, 0);
            Assert.AreEqual(stateChange.removals.Count, 0);
        }

        [TestMethod]
        public void test_add_existing_aspect() {
            NoteAspect note = new NoteAspect("This is a test note");
            StateAspectAdd noteAdd = new StateAspectAdd(note);
            StateAspectRemove noteRemove = new StateAspectRemove(noteAdd.aspect_id);
            //TODO: update
            StateChange stateChange = new StateChange();

            // test pre-existing add
            stateChange.addChange(StateAspect.NOTE, noteAdd);
            Assert.ThrowsException<InvalidAspect>(() => { stateChange.addChange(StateAspect.NOTE, noteAdd); });
            //TODO: update
            Assert.ThrowsException<InvalidAspect>(() => { stateChange.addChange(StateAspect.NOTE, noteRemove); });
            // clean up and verify we're still in a good state
            stateChange.removeChange(StateAspect.NOTE, noteAdd.aspect_id);
            Assert.AreEqual(stateChange.additions.Count, 0);
            Assert.AreEqual(stateChange.updates.Count, 0);
            Assert.AreEqual(stateChange.removals.Count, 0);

            //TODO: test pre-existing update

            // test pre-existing removal
            stateChange.addChange(StateAspect.NOTE, noteRemove);
            Assert.ThrowsException<InvalidAspect>(() => { stateChange.addChange(StateAspect.NOTE, noteAdd); });
            //TODO: update
            Assert.ThrowsException<InvalidAspect>(() => { stateChange.addChange(StateAspect.NOTE, noteRemove); });
            // clean up and verify we're still in a good state
            stateChange.removeChange(StateAspect.NOTE, noteRemove.aspect_id);
            Assert.AreEqual(stateChange.additions.Count, 0);
            Assert.AreEqual(stateChange.updates.Count, 0);
            Assert.AreEqual(stateChange.removals.Count, 0);
        }

        [TestMethod]
        public void test_remove_invalid() {
            Guid validGuid = Guid.NewGuid(), invalidGuid = Guid.NewGuid();
            StateAspectRemove noteRemove = new StateAspectRemove(validGuid);
            StateChange stateChange = new StateChange();
            stateChange.addChange(StateAspect.NOTE, noteRemove);

            Assert.ThrowsException<InvalidAspect>(() => { stateChange.removeChange("invalid aspect", validGuid); });
            Assert.ThrowsException<InvalidAspect>(() => { stateChange.removeChange(StateAspect.NOTE, invalidGuid); });
            stateChange.removeChange(StateAspect.NOTE, validGuid);
            Assert.AreEqual(stateChange.additions.Count, 0);
            Assert.AreEqual(stateChange.updates.Count, 0);
            Assert.AreEqual(stateChange.removals.Count, 0);
        }

        //TODO: apply/revert
    }
}
