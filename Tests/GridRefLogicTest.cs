using Xunit;
using SpeciesRecordAddGridRefs;

namespace Tests;

public class GridRefLogicTest
{
    [Fact]
    public void SimpleFirstTest()
    {
        var (sixFigureRef, fourFigureRef) = GeoLogic.MakeGridReferences(52.570740, 0.458449);

        Assert.Equal("TL667998", sixFigureRef);
        Assert.Equal("TL6699", fourFigureRef);
    }
}