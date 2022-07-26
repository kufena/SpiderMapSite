namespace SharedTypes;


public record SpeciesRecord(DateTime RecordSeenDate, DateTime RecordAddedTime, Species Type, Recorder Recorder, LatLong Position);