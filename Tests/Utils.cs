using FuzzySharp;
using GoogleMaps.Tests.Data;
using Newtonsoft.Json;
using OpenQA.Selenium;
using System.Text.RegularExpressions;

namespace GoogleMaps.Tests;

public static partial class Utils {

    #region Parsing and Conversions

    /// <summary>
    /// Converts a distance string to its equivalent value in meters.
    /// </summary>
    /// <param name="distance">The distance string (e.g., "23 km" or "1000 m").</param>
    /// <returns>Converted distance value in meters</returns>
    /// <exception cref="ArgumentException">Thrown for invalid distance string formats</exception>
    public static int ConvertDistanceToMeters(string distance) {
        string[] splitDistance = distance.Split(" ");
        if (splitDistance[1] == "km") {
            return int.Parse(splitDistance[0]) * 1000;
        } else if (splitDistance[1] == "m") {
            return int.Parse(splitDistance[0]);
        } else {
            throw new ArgumentException($"Invalid distance string format {distance}");
        }
    }

    /// <summary>
    /// Extracts latitude, longitude, and zoom level from a Google Maps URL.
    /// </summary>
    /// <param name="url">Google Maps URL</param>
    /// <returns>An extracted coordinates object</returns>
    public static Coordinates GetCoordinatesFromUrl(string url) {
        string coordStr = new Regex(@"/@(.*?)z").Match(url).Groups[1].Value;
        return ParseCoordinates(coordStr);
    }

    /// <summary>
    /// Parses comma-separated latitude, longitude, and zoom level coordinates into a struct.
    /// </summary>
    /// <param name="coordinates">A comma-separated coordinates string</param>
    /// <returns>A Coordinates object</returns>
    public static Coordinates ParseCoordinates(string coordinates) {
        string[] coordArr = coordinates.Split(',');
        return new Coordinates(float.Parse(coordArr[0]), float.Parse(coordArr[1]), float.Parse(coordArr[2]));
    }

    #endregion

    #region String Comparisons

    /// <summary>
    /// Calculates the similarity between two strings using fuzzy string comparison.
    /// </summary>
    /// <returns>String similarity percentage (0-100)</returns>
    public static int GetStringSimilarity(string str1, string str2) {
        // Lowercase the entire string and remove some special characters.
        string regex = @"[.,\r\n-]+";
        str1 = Regex.Replace(str1.ToLower(), regex, " ");
        str2 = Regex.Replace(str2.ToLower(), regex, " ");

        // Return fuzzy string comparison.
        return Fuzz.PartialTokenSetRatio(str1, str2);
    }

    /// <summary>
    /// Determines whether two strings are equal after normalizing line endings for different operating systems.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the strings are equal after normalizing line endings; otherwise, <c>false</c>.
    /// </returns>
    public static bool AreNormalizedStringsEqual(string str1, string str2) {
        // Replace '\r\n' with '\n' and '\r' with '\n'.
        string regex = @"\r\n?|\n|\r";
        string normalizedStr1 = Regex.Replace(str1, regex, "\n");
        string normalizedStr2 = Regex.Replace(str2, regex, "\n");

        return normalizedStr1.Equals(normalizedStr2);
    }

    #endregion

    #region File Manipulations

    /// <summary>
    /// Saves a screenshot in Tests/UI/Screenshots folder.
    /// </summary>
    /// <param name="screenshot">The screenshot object</param>
    /// <param name="fileName">The filename for the screenshot to save (including extension)</param>
    public static void SaveScreenshot(Screenshot screenshot, string fileName) {
        string screenshotPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Tests", "UI", "Screenshots");
        Directory.CreateDirectory(screenshotPath);
        string filePath = Path.Combine(screenshotPath, fileName);
        screenshot.SaveAsFile(filePath);
    }

    /// <summary>
    /// Reads and deserializes data from a JSON file.
    /// </summary>
    /// <typeparam name="TData">The type to deserialize into</typeparam>
    /// <param name="filePath">Directory path of the JSON file</param>
    /// <param name="fileName">Name of the JSON file (including extension)</param>
    /// <returns>The deserialized data of type <typeparamref name="TData"/></returns>
    public static TData ReadDataFromJson<TData>(string filePath, string fileName) {
        string fullPath = Path.Combine(filePath, fileName);
        string jsonData = File.ReadAllText(fullPath);
        TData? data = JsonConvert.DeserializeObject<TData>(jsonData);
        if (data == null) {
            Assert.Fail($"Unable to parse data from {fileName}.");
        }
        return data!;
    }

    #endregion
}
