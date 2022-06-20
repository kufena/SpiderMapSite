namespace SharedTypes;

public record Species
{
    public Species(string GenusName, string SpeciesName, string Comment)
    {
        this.GenusName = GenusName;
        this.SpeciesName = SpeciesName;
        this.Comment = Comment;
    }

    public string GenusName { init; get; }
    public string SpeciesName { init; get; }
    public string Comment { init; get; }
}
