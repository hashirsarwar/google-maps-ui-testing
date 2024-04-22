using GoogleMaps.Tests.Data;
using OpenQA.Selenium;

namespace GoogleMaps.Tests.UI.WebDriverCreators;

public interface IWebDriverCreator {
    /// <summary>
    /// Creates a web driver and applies given options to the driver.
    /// </summary>
    /// <param name="mockedGeolocation">Mocked geolocation coordinates</param>
    /// <returns>WebDriver object</returns>
    WebDriver GetWebDriver(Coordinates mockedGeolocation);
}