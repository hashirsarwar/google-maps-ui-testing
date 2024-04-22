using GoogleMaps.Tests.Data;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace GoogleMaps.Tests.UI.WebDriverCreators;

public class ChromeDriverCreator : IWebDriverCreator {
    public WebDriver GetWebDriver(Coordinates mockedGeolocation) {
        ChromeDriver chromeDriver = new();

        // Set geolocation for the chrome driver.
        chromeDriver.GetDevToolsSession();
        Dictionary<string, object> coordinates = new() {
            {"latitude",mockedGeolocation.Latitude },
            {"longitude",mockedGeolocation.Longitude },
            {"accuracy",1 }
        };
        chromeDriver.ExecuteCdpCommand("Emulation.setGeolocationOverride", coordinates);

        return chromeDriver;
    }
}