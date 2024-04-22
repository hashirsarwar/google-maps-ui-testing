using GoogleMaps.Tests.Data;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;

namespace GoogleMaps.Tests.UI.WebDriverCreators;

public class EdgeDriverCreator : IWebDriverCreator {
    public WebDriver GetWebDriver(Coordinates mockedGeolocation) {
        EdgeDriver edgeDriver = new();

        // Set geolocation for the edge driver.
        edgeDriver.GetDevToolsSession();
        Dictionary<string, object> coordinates = new() {
            {"latitude",mockedGeolocation.Latitude },
            {"longitude",mockedGeolocation.Longitude },
            {"accuracy",1 }
        };
        edgeDriver.ExecuteCdpCommand("Emulation.setGeolocationOverride", coordinates);
        
        return edgeDriver;
    }
}