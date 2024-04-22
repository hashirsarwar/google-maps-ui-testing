using GoogleMaps.Tests.Data;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace GoogleMaps.Tests.UI.WebDriverCreators;

public class ChromeDriverCreator : IWebDriverCreator {
    public WebDriver GetWebDriver(Coordinates geolocation) {
        ChromeDriver chromeDriver = new();

        // Set geolocation to chrome driver.
        chromeDriver.GetDevToolsSession();
        Dictionary<string, object> coordinates = new() {
            {"latitude",geolocation.Latitude },
            {"longitude",geolocation.Longitude },
            {"accuracy",1 }
        };
        chromeDriver.ExecuteCdpCommand("Emulation.setGeolocationOverride", coordinates);

        return chromeDriver;
    }
}