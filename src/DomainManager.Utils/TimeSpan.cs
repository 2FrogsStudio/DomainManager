namespace DomainManager;

public static class TimespanExtensions {
    public static string ToHumanReadableString(this TimeSpan t) {
        if (t.TotalSeconds <= 1) {
            return $@"{t:s\.ff} seconds";
        }

        if (t.TotalMinutes <= 1) {
            return $@"{t:%s} seconds";
        }

        if (t.TotalHours <= 1) {
            return $@"{t:%m} minutes";
        }

        return t.TotalDays <= 1 ? $@"{t:%h} hours" : $@"{t:%d} days";
    }
}