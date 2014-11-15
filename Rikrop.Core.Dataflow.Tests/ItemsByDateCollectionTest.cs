using System;
using NUnit.Framework;

namespace Rikrop.Core.Dataflow.Tests
{
    [TestFixture]
    public class ItemsByDateCollectionTest
    {
        [Test]
        public void AddingExpiredItemAllowsToGetIt()
        {
            const int key = 1;

            var scheduler = new ItemsByDateCollection<int>();

            var now = DateTime.Now;
            scheduler.Add(key, now);

            var expiredKeys = scheduler.TakeItemsTill(now);
            Assert.That(() => expiredKeys, Is.EquivalentTo(new[] { key }));
        }

        [Test]
        public void AddingItemAllowToRescheduleIt()
        {
            const int key = 1;

            var scheduler = new ItemsByDateCollection<int>();

            var now = DateTime.Now;
            scheduler.Add(key, now);

            DateTime nextExpirationDate;
            Assert.True(scheduler.TryGetNearestDate(out nextExpirationDate));
            Assert.AreEqual(now, nextExpirationDate);

            var newDate = now.AddSeconds(1);
            scheduler.Add(key, newDate);

            DateTime newNextExpirationDate;
            Assert.True(scheduler.TryGetNearestDate(out newNextExpirationDate));
            Assert.AreEqual(newDate, newNextExpirationDate);

            var expiredKeys = scheduler.TakeItemsTill(now.AddSeconds(2));
            Assert.That(() => expiredKeys, Is.EquivalentTo(new[] { key }));
        }

        [Test]
        public void StopScheduleItemRemovesIt()
        {
            const int key = 1;

            var scheduler = new ItemsByDateCollection<int>();

            var now = DateTime.Now;
            scheduler.Add(key, now);

            scheduler.Remove(key);
            DateTime nextExpirationDate;
            Assert.False(scheduler.TryGetNearestDate(out nextExpirationDate));
        }

        [Test]
        public void AddingEarlierKeyChangeScheduleDate()
        {
            const int key = 1;
            const int key2 = 2;

            var scheduler = new ItemsByDateCollection<int>();

            var now = DateTime.Now;
            scheduler.Add(key, now);

            DateTime nextExpirationDate;
            Assert.True(scheduler.TryGetNearestDate(out nextExpirationDate));
            Assert.AreEqual(now, nextExpirationDate);

            var earlierDate = now.AddDays(-1);
            scheduler.Add(key2, earlierDate);

            DateTime newNextExpirationDate;
            Assert.True(scheduler.TryGetNearestDate(out newNextExpirationDate));
            Assert.AreEqual(earlierDate, newNextExpirationDate);

            var expiredKeys = scheduler.TakeItemsTill(now.AddSeconds(1));
            Assert.That(() => expiredKeys, Is.EquivalentTo(new[] { key, key2 }));
        }

        [Test]
        public void AddingKeyToExpiredGroupStaysBothExpired()
        {
            const int key = 1;
            const int key2 = 2;

            var scheduler = new ItemsByDateCollection<int>();

            var now = DateTime.Now;
            scheduler.Add(key, now);
            scheduler.Add(key2, now);

            DateTime nextExpirationDate;
            Assert.True(scheduler.TryGetNearestDate(out nextExpirationDate));
            Assert.AreEqual(now, nextExpirationDate);

            var expiredKeys = scheduler.TakeItemsTill(now);
            Assert.That(() => expiredKeys, Is.EquivalentTo(new[] { key, key2 }));
        }

        [Test]
        public void TakeExpiredKeysShouldRemoveItemsFromSchedule()
        {
            const int key = 1;

            var scheduler = new ItemsByDateCollection<int>();

            var now = DateTime.Now;
            scheduler.Add(key, now);

            Assert.AreEqual(1, scheduler.Count);

            var expiredKeys = scheduler.TakeItemsTill(now);

            Assert.AreEqual(0, scheduler.Count);
            Assert.That(() => expiredKeys, Is.EquivalentTo(new[] { key }));
        }
    }
}