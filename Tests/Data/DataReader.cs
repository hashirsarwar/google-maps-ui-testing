namespace GoogleMaps.Tests.Data;

public static class DataReader {

    private static readonly string DataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Tests", "Data", "JSON");

    public static IEnumerable<Search> GetAddressSearches() {
        return Utils.ReadDataFromJson<IEnumerable<Search>>(DataPath, "AddressSearches.json");
    }

    public static IEnumerable<Search> GetLandmarkSearches() {
        return Utils.ReadDataFromJson<IEnumerable<Search>>(DataPath, "LandmarkSearches.json");
    }

    public static IEnumerable<string> GetInvalidSearches() {
        return Utils.ReadDataFromJson<IEnumerable<string>>(DataPath, "InvalidSearches.json");
    }

    public static IEnumerable<string> GetPartiallyValidSearches() {
        return Utils.ReadDataFromJson<IEnumerable<string>>(DataPath, "PartiallyValidSearches.json");
    }

    public static IEnumerable<Search> GetSearchesWithTypos() {
        return Utils.ReadDataFromJson<IEnumerable<Search>>(DataPath, "SearchesWithTypos.json");
    }

    public static IEnumerable<string> GetAmbiguousSearches() {
        return Utils.ReadDataFromJson<IEnumerable<string>>(DataPath, "AmbiguousSearches.json");
    }

    public static IEnumerable<SearchWithTranslation> GetAddressSearchesWithTranslation() {
        return Utils.ReadDataFromJson<IEnumerable<SearchWithTranslation>>(DataPath, "AddressSearchesWithTranslation.json");
    }

    public static IEnumerable<SearchWithTranslation> GetLandmarkSearchesWithTranslation() {
        return Utils.ReadDataFromJson<IEnumerable<SearchWithTranslation>>(DataPath, "LandmarkSearchesWithTranslation.json");
    }
}