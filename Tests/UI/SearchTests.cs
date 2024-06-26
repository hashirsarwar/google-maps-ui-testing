﻿using GoogleMaps.Tests.Data;
using GoogleMaps.Tests.UI.WebDriverCreators;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Collections.ObjectModel;
using System.Drawing;

namespace GoogleMaps.Tests.UI;

[TestFixture(typeof(ChromeDriverCreator))]
[TestFixture(typeof(EdgeDriverCreator))]
public class SearchTests<TWebDriverCreator> where TWebDriverCreator : IWebDriverCreator, new() {

    /// <summary>
    /// ID for map scale element that appears in the bottom right of Google Maps.
    /// This element indicates how much the map is zoomed in.
    /// </summary>
    private const string ScaleElementId = "scale";

    /// <summary>
    /// The percentage of similarity between a search query and search result that is
    /// required to consider the search result accurate.
    /// </summary>
    private const int SearchSimilarityThreshold = 90;

    /// <summary>
    /// Timeout value for waiting for elements to be visible or present.
    /// </summary>
    private const int ElementTimeoutInSeconds = 10;

    /// <summary>
    /// The search results on Google Maps are location-dependent. This mocks the web driver's
    /// geolocation to a constant value to ensure consistent search results. The coordinates are
    /// set to Pakistan.
    /// </summary>
    private readonly Coordinates MockedGeolocation = new(32.064608f, 72.697882f, 3.00f);

    /// <summary>
    /// Size of the test browser window. This is important to keep the map coordinate values
    /// consistent for different browsers, display resolutions, and test cases.
    /// </summary>
    private readonly Size WindowSize = new(1024, 768);

    private WebDriver _webDriver;
    private string _initialMapScale;

    [SetUp]
    public void SetUp() {
        // Get a web driver with a mocked geolocation to ensure consistent search results.
        TWebDriverCreator webDriverCreator = new();
        _webDriver = webDriverCreator.GetWebDriver(MockedGeolocation);

        // Set a constant window size to ensure consistent location coordinates.
        _webDriver.Manage().Window.Size = WindowSize;

        // Open Google Maps at the initial location, fully zoomed out.
        _webDriver.Navigate().GoToUrl($"https://www.google.com/maps/@{MockedGeolocation.Latitude},{MockedGeolocation.Longitude},{MockedGeolocation.ZoomLevel}z");

        // Wait until the map scale in the bottom-right is loaded.
        WebDriverWait wait = new(_webDriver, TimeSpan.FromSeconds(ElementTimeoutInSeconds));
        wait.Until(driver => driver.FindElement(By.Id(ScaleElementId)).Displayed);

        // Save initial map scale.
        _initialMapScale = _webDriver.FindElement(By.Id(ScaleElementId)).Text;
    }

    [TearDown]
    public void TearDown() {
        // Save screenshot on test failure.
        string? failedMethod = TestContext.CurrentContext.Test.MethodName;
        if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed && failedMethod != null) {
            try {
                string fileName = $"failure_{failedMethod}_{DateTime.Now:yyyyMMddHHmmss}.png";
                Screenshot screenshot = ((ITakesScreenshot)_webDriver).GetScreenshot();
                Utils.SaveScreenshot(screenshot, fileName);
            } catch (Exception e) {
                TestContext.WriteLine($"Unable to save the screenshot for the test failure.\n{e.Message}");
            }
        }

        _webDriver.Quit();
        _webDriver.Dispose();
    }

    #region Test Methods

    /// <summary>
    /// This method searches an address and verifies that a valid search result is displayed.
    /// Assertions are made to check zoom level, coordinates, action buttons presence, and address
    /// text similarity.
    /// </summary>
    /// <param name="search">Search object representing a query and associated information</param>
    [TestCaseSource(typeof(DataReader), nameof(DataReader.GetAddressSearches))]
    public void Search_Address_DisplaysCorrectResult(Search search) {
        SearchGoogleMaps(search.Text);
        WaitForSearchResultsToLoad();

        VerifySingleSearchResult(expectedCoordinates: search.Coordinates);

        // Verify the address text.
        IWebElement addressElement = _webDriver.FindElement(By.CssSelector("[data-tooltip='Copy address'][aria-label^='Address']"));
        Assert.That(Utils.GetStringSimilarity(addressElement.Text, search.Text), Is.GreaterThan(SearchSimilarityThreshold));
    }

    /// <summary>
    /// This method searches a landmark name and verifies that valid result for the landmark is
    /// displayed. It checks zoom level, coordinates, action buttons, and landmark title similarity.
    /// </summary>
    /// <param name="search">Search object representing a query and associated information</param>
    [TestCaseSource(typeof(DataReader), nameof(DataReader.GetLandmarkSearches))]
    public void Search_Landmark_DisplaysCorrectResult(Search search) {
        SearchGoogleMaps(search.Text);
        WaitForSearchResultsToLoad();

        VerifySingleSearchResult(expectedCoordinates: search.Coordinates);

        // Verify the landmark title.
        IWebElement locationTitleElement = _webDriver.FindElement(By.CssSelector(".DUwDvf"));
        Assert.That(Utils.GetStringSimilarity(locationTitleElement.Text, search.Text), Is.GreaterThan(SearchSimilarityThreshold));
    }

    /// <summary>
    /// This method tests searching with invalid text. It verifies that the map doesn't zoom in,
    /// coordinates remain unchanged, error messages are displayed, and no results are shown.
    /// </summary>
    /// <param name="searchText">Invalid search query</param>
    [TestCaseSource(typeof(DataReader), nameof(DataReader.GetInvalidSearches))]
    public void Search_Invalid_DisplaysNoResult(string searchText) {
        SearchGoogleMaps(searchText);
        WaitForSearchResultsToLoad();

        // Verify that no result is displayed.
        ReadOnlyCollection<IWebElement> matches = _webDriver.FindElements(By.CssSelector($"[aria-label=\"Results for {searchText}\"] div.fontBodyMedium.UaQhfb"));
        Assert.That(matches, Is.Empty);

        // Verify that the map is not zoomed in.
        VerifyZoom(shouldZoomIn: false);

        // Verify that coordinates aren't updated.
        VerifyCoordinates(expectedCoordinates: MockedGeolocation);

        // Verify the error messages.
        IWebElement errorMessages = _webDriver.FindElement(By.CssSelector(".miFGmb > div:nth-child(1) > div:nth-child(1)"));
        Assert.That(Utils.AreNormalizedStringsEqual(errorMessages.Text, $"Google Maps can't find {searchText}\nMake sure your search is spelled correctly. Try adding a city, state, or zip code.\nTry Google Search instead\nShould this place be on Google Maps?\nAdd a missing place"));
    }

    /// <summary>
    /// This method tests searching with partially valid text that may consist of parts of two valid
    /// addresses. It verifies that multiple matches are displayed that partially match the search
    /// query. It also verifies the relevant info messages.
    /// </summary>
    /// <param name="searchText">Partially valid search query</param>
    [TestCaseSource(typeof(DataReader), nameof(DataReader.GetPartiallyValidSearches))]
    public void Search_PartiallyValid_DisplaysMatchesAndInfo(string searchText) {
        SearchGoogleMaps(searchText);
        WaitForSearchResultsToLoad();

        VerifyMultipleSearchResults(searchText);

        // Verify the "Partial matches" info message.
        IWebElement partialMatchInfo = _webDriver.FindElement(By.CssSelector(".L5xkq"));
        Assert.That(Utils.AreNormalizedStringsEqual(partialMatchInfo.Text, $"Partial matches\n{searchText}"));
        IWebElement otherOptionsInfo = _webDriver.FindElement(By.CssSelector(".EwXvme > div:nth-child(1)"));
        Assert.That(Utils.AreNormalizedStringsEqual(otherOptionsInfo.Text, "Don't see what you're looking for?\nTry Google Search instead\nShould this place be on Google Maps?\nAdd a missing place"));
    }

    /// <summary>
    /// This method tests ambiguous search terms that could have multiple interpretations. It verifies
    /// that multiple search results are displayed.
    /// </summary>
    /// <param name="searchText">Ambiguous search query</param>
    [TestCaseSource(typeof(DataReader), nameof(DataReader.GetAmbiguousSearches))]
    public void Search_Ambiguous_DisplayMatches(string searchText) {
        SearchGoogleMaps(searchText);
        WaitForSearchResultsToLoad();

        VerifyMultipleSearchResults(searchText);
    }

    /// <summary>
    /// This method tests searching with typos in the query text. It verifies that the typing error
    /// is identified and corrected by the system. Assertions are made to check for the typo info,
    /// corrected address display, and similarity between the corrected address and search query.
    /// </summary>
    /// <param name="search">Search object representing a query and associated information</param>
    [TestCaseSource(typeof(DataReader), nameof(DataReader.GetSearchesWithTypos))]
    public void Search_WithTypo_DisplaysCorrectResult(Search search) {
        SearchGoogleMaps(search.Text);
        WaitForSearchResultsToLoad();

        VerifySingleSearchResult(expectedCoordinates: search.Coordinates);

        // Verify that system detected the typo and showed the message.
        string typoMessage = _webDriver.FindElement(By.CssSelector(".tBgtgb")).Text;
        string correctAddress = _webDriver.FindElement(By.CssSelector(".IpVgWd")).Text;
        Assert.That(typoMessage, Is.EqualTo($"Showing results for {correctAddress}. Search instead for {search.Text}."));

        // Verify that correct address detected matches the search query (address with typo).
        Assert.That(Utils.GetStringSimilarity(correctAddress, search.Text), Is.GreaterThan(SearchSimilarityThreshold));
    }

    /// <summary>
    /// This method tests searching using an address in a language different from the default language.
    /// It verifies that the address text is translated correctly to the default language and then a valid
    /// search result is displayed.
    /// </summary>
    /// <param name="search">
    /// Search object representing a query with translation and associated information.
    /// </param>
    [TestCaseSource(typeof(DataReader), nameof(DataReader.GetAddressSearchesWithTranslation))]
    public void Search_AddressInOtherLanguage_DisplaysCorrectResult(SearchWithTranslation search) {
        SearchGoogleMaps(search.Text);
        WaitForSearchResultsToLoad();

        VerifySingleSearchResult(expectedCoordinates: search.Coordinates);

        // Verify the translated address text.
        IWebElement addressElement = _webDriver.FindElement(By.CssSelector("[data-tooltip='Copy address'][aria-label^='Address']"));
        Assert.That(Utils.GetStringSimilarity(addressElement.Text, search.Translation), Is.GreaterThan(SearchSimilarityThreshold));
    }

    /// <summary>
    /// This method tests searching using an landmark in a language different from the default language.
    /// It verifies that the landmark heading is translated correctly to the default language and text in
    /// language of the query is also displayed as a sub-heading.
    /// </summary>
    /// <param name="search">
    /// Search object representing a query with translation and associated information.
    /// </param>
    [TestCaseSource(typeof(DataReader), nameof(DataReader.GetLandmarkSearchesWithTranslation))]
    public void Search_LandmarkInOtherLanguage_DisplaysCorrectResult(SearchWithTranslation search) {
        SearchGoogleMaps(search.Text);
        WaitForSearchResultsToLoad();

        VerifySingleSearchResult(expectedCoordinates: search.Coordinates);

        // Verify the translated landmark title.
        IWebElement locationTitleElement = _webDriver.FindElement(By.CssSelector(".DUwDvf"));
        Assert.That(Utils.GetStringSimilarity(locationTitleElement.Text, search.Translation), Is.GreaterThan(SearchSimilarityThreshold));

        // Verify the subtitle in the search language.
        IWebElement locationSubtitleElement = _webDriver.FindElement(By.CssSelector(".bwoZTb"));
        Assert.That(Utils.GetStringSimilarity(locationSubtitleElement.Text, search.Text), Is.GreaterThan(SearchSimilarityThreshold));
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Verifies that search result is a single, valid and unique location by
    /// checking the following:
    /// <list type="bullet">
    /// <item>The map is zoomed in to a specific location.</item>
    /// <item>The location being pointed is correct.</item>
    /// <item>The actions buttons (like Directions, Save, Nearby) are displayed.</item>
    /// </list>
    /// </summary>
    /// <param name="expectedCoordinates">
    /// Expected coordinates string obtained from the search data.
    /// </param>
    private void VerifySingleSearchResult(string expectedCoordinates) {
        VerifyZoom(shouldZoomIn: true);
        VerifyCoordinates(Utils.ParseCoordinates(expectedCoordinates));
        VerifyActionButtons();
    }

    /// <summary>
    /// Verifies that search results are correctly displayed by checking the
    /// following:
    /// <list type="bullet">
    /// <item>Multiple results are populated.</item>
    /// <item>The results loosely match the search text.</item>
    /// </list>
    /// </summary>
    /// <param name="searchText">The search text to be verified</param>
    private void VerifyMultipleSearchResults(string searchText) {
        // Verify that multiple results are populated.
        ReadOnlyCollection<IWebElement> matches = _webDriver.FindElements(By.CssSelector($"[aria-label=\"Results for {searchText}\"] div.fontBodyMedium.UaQhfb"));
        Assert.That(matches, Has.Count.GreaterThan(1));

        // Verify that the results loosely match the search text.
        foreach (IWebElement element in matches) {
            Assert.That(Utils.GetStringSimilarity(element.Text, searchText), Is.GreaterThan(60));
        }
    }

    /// <summary>
    /// Verifies whether the map is zoomed in or not by comparing map scale before
    /// and after a search query is performed.
    /// </summary>
    /// <param name="shouldZoomIn">
    /// Specifies whether the map should zoom in after the search query
    /// </param>
    private void VerifyZoom(bool shouldZoomIn)
    {
        string currentZoomScale = _webDriver.FindElement(By.Id(ScaleElementId)).Text;
        Assert.That(currentZoomScale, shouldZoomIn ? Is.Not.EqualTo(_initialMapScale) : Is.EqualTo(_initialMapScale));
    }

    /// <summary>
    /// Verifies that the marker is pointing to the correct location by comparing
    /// coordinates in the URL with expected coordinates. 
    /// </summary>
    /// <param name="expectedCoordinates">
    /// Expected coordinates object obtained from the search data.
    /// </param>
    private void VerifyCoordinates(Coordinates expectedCoordinates) {
        Coordinates coordinatesFromUrl = Utils.GetCoordinatesFromUrl(_webDriver.Url);
        float coordinateTolerance = 0.001f;
        Assert.That(coordinatesFromUrl.Latitude, Is.EqualTo(expectedCoordinates.Latitude).Within(coordinateTolerance));
        Assert.That(coordinatesFromUrl.Longitude, Is.EqualTo(expectedCoordinates.Longitude).Within(coordinateTolerance));
        Assert.That(coordinatesFromUrl.ZoomLevel, Is.EqualTo(expectedCoordinates.ZoomLevel).Within(coordinateTolerance));
    }

    /// <summary>
    /// Verifies that action buttons including Directions, Save, Nearby, and Send to phone
    /// are visible for a search result.
    /// </summary>
    private void VerifyActionButtons() {
        ReadOnlyCollection<IWebElement> directionsButton = _webDriver.FindElements(By.CssSelector("button[data-value=\"Directions\"]"));
        Assert.That(directionsButton, Is.Not.Empty);
        ReadOnlyCollection<IWebElement> saveButton = _webDriver.FindElements(By.CssSelector("button[data-value=\"Save\"]"));
        Assert.That(saveButton, Is.Not.Empty);
        ReadOnlyCollection<IWebElement> nearbyButton = _webDriver.FindElements(By.CssSelector("button[data-value=\"Nearby\"]"));
        Assert.That(nearbyButton, Is.Not.Empty);
        ReadOnlyCollection<IWebElement> sendToPhoneButton = _webDriver.FindElements(By.CssSelector("button[data-value=\"Send to phone\"]"));
        Assert.That(sendToPhoneButton, Is.Not.Empty);
    }

    /// <summary>
    /// Searches for a location on Google Maps.
    /// </summary>
    /// <param name="searchText">The text to search for</param>
    private void SearchGoogleMaps(string searchText) {
        IWebElement searchBox = _webDriver.FindElement(By.Id("searchboxinput"));
        searchBox.Clear();
        searchBox.SendKeys(searchText);
        IWebElement searchButton = _webDriver.FindElement(By.Id("searchbox-searchbutton"));
        searchButton.Click();
    }

    /// <summary>
    /// Waits for search results to load on Google Maps.
    /// </summary>
    private void WaitForSearchResultsToLoad() {
        // Wait for the result container to load and render.
        string resultsContainerSelector = "div.bJzME:nth-child(2)";
        WebDriverWait wait = new WebDriverWait(_webDriver, TimeSpan.FromSeconds(ElementTimeoutInSeconds));
        wait.Until(driver => driver.FindElement(By.CssSelector(resultsContainerSelector)).Displayed);

        // Google Maps generates the map canvas and location marker dynamically using JavaScript.
        // Therefore, we can't find it using CSS selector or Xpath to add explicit wait. Adding
        // some extra delay to ensure the map is centred and the marker is updated.
        Thread.Sleep(2500);
    }

    #endregion
}