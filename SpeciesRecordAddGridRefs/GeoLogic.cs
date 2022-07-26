namespace SpeciesRecordAddGridRefs;

using GeoUK;
using GeoUK.Projections;
using GeoUK.Coordinates;
using GeoUK.Ellipsoids;

public static class GeoLogic
{

    public static (string GridRefSixFig, string GridRefFourFig) MakeGridReferences(double latitude, double longitude)
    {
        string GridRefSixFig = "";
        string GridRefFourFig = "";


        var latLon = new LatitudeLongitude(latitude, longitude);
        var cartesianPair = Convert.ToCartesian(new Wgs84(), latLon);

        var osGBPair = Transform.Etrs89ToOsgb36(cartesianPair);
        var eastingNorthingCoordinates = Convert.ToEastingNorthing(new Airy1830(), new BritishNationalGrid(), osGBPair);
        var osGBAgain = new Osgb36(eastingNorthingCoordinates);
        GridRefSixFig = osGBAgain.MapReference;

        if ((GridRefSixFig.Length == 8))
        {
            GridRefFourFig = GridRefSixFig.Substring(0, 4) + GridRefSixFig.Substring(5, 2);
        }

        return (GridRefSixFig, GridRefFourFig);
    }
    
}