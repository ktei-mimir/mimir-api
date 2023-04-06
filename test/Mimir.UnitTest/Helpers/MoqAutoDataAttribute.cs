using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;

namespace Mimir.UnitTest.Helpers;

public class MoqAutoDataAttribute : AutoDataAttribute
{
    public MoqAutoDataAttribute()
        : base(() =>
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization
                { ConfigureMembers = true, GenerateDelegates = true });
            fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        })
    {
    }
}