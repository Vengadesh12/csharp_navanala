using MyBackend.Domain.Common;
using Xunit;

namespace MyBackend.UnitTests.Domain
{
    public class TestEntity : BaseEntity
    {
    }

    public class BaseEntityTests
    {
        [Fact]
        public void NewEntity_ShouldBeActiveByDefault()
        {
            var entity = new TestEntity();

            Assert.Equal(1, entity.DeletedFlag);
            Assert.True(entity.IsActive);
        }

        [Fact]
        public void SoftDelete_ShouldSetDeletedFlagToZero()
        {
            var entity = new TestEntity();

            entity.SoftDelete();

            Assert.Equal(0, entity.DeletedFlag);
            Assert.False(entity.IsActive);
            Assert.NotNull(entity.UpdatedAt);
        }

        [Fact]
        public void Restore_ShouldSetDeletedFlagToOne()
        {
            var entity = new TestEntity();
            entity.SoftDelete();

            entity.Restore();

            Assert.Equal(1, entity.DeletedFlag);
            Assert.True(entity.IsActive);
        }
    }
}
