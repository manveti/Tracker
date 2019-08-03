using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tracker;
using System;


namespace Tracker.Tests {
    [TestClass]
    public class StateAspectChangeTests {
        [TestMethod]
        public void test_StateAspectAdd() {
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
        public void test_StateAspectAdd_tag_index_changes() {
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

            // remove added aspect from tag index and verify revert fails
            store = base_store.copy();
            store.tag_index["tag1"].Add(otherGuid);
            store.tag_index["tag1"].Remove(noteAdd.aspect_id);
            Assert.ThrowsException<InvalidState>(() => { noteAdd.revertFromStore(store); });

            // remove a necessary tag and verify revert fails
            store = base_store.copy();
            store.tag_index["tag1"].Add(otherGuid);
            store.tag_index.Remove("tag1");
            Assert.ThrowsException<InvalidState>(() => { noteAdd.revertFromStore(store); });
        }
    }
}