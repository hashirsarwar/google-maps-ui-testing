using GoogleMaps.Tests.Data;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;

namespace GoogleMaps.Tests.UI.WebDriverCreators;

public class EdgeDriverCreator : IWebDriverCreator {
    public WebDriver GetWebDriver(Coordinates geolocation) {
        EdgeDriver edgeDriver = new();

        // Set geolocation to edge driver.
        edgeDriver.GetDevToolsSession();
        Dictionary<string, object> coordinates = new() {
            {"latitude",geolocation.Latitude },
            {"longitude",geolocation.Longitude },
            {"accuracy",1 }
        };
        edgeDriver.ExecuteCdpCommand("Emulation.setGeolocationOverride", coordinates);
        
        return edgeDriver;
    }
}