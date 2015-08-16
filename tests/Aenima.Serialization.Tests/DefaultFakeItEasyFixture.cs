using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoFakeItEasy;

namespace Aenima.Serialization.Tests
{
    /// <summary> 
    ///     Provides anonymous object creation services by using FakeItEasy.
    /// </summary>
    /// <seealso cref="T:Ploeh.AutoFixture.Fixture"/>
    public class DefaultFakeItEasyFixture : Fixture
    {
        private const int DefaultRecursionDepth = 10;
        private const int DefaultRepeatCount = 5;

        public DefaultFakeItEasyFixture()
            : this(DefaultRecursionDepth, DefaultRepeatCount) { }

        public DefaultFakeItEasyFixture(int repeatCount)
            : this(DefaultRecursionDepth, repeatCount) { }

        public DefaultFakeItEasyFixture(int recursionDepth, int repeatCount)
        {
            Customize(new AutoFakeItEasyCustomization());

            RepeatCount = repeatCount;

            if (recursionDepth > 0)
            {
                Behaviors.Clear();
                Behaviors.Add(new OmitOnRecursionBehavior(recursionDepth));
            }
        }
    }
}