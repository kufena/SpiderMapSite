namespace SharedTypes;


public record SpeciesRecord(DateOnly RecordSeenDate, DateTime RecordAddedTime, Species Type, Recorder Recorder, LatLong Position);