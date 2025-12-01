using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;

namespace Automated_Test_Crowdfunding.Drivers
{
    public static class DriverFactory
    {
        public static IWebDriver CreateDriver(bool headless = false)
        {
            var options = new ChromeOptions();

            // Iniciar maximizado
            options.AddArgument("--start-maximized");

            // Evitar errores de automatización
            options.AddArgument("--disable-infobars");
            options.AddArgument("--disable-notifications");
            options.AddArgument("--disable-popup-blocking");

            // Modo headless opcional
            if (headless)
            {
                options.AddArgument("--headless=new");
                options.AddArgument("--window-size=1920,1080");
            }

            IWebDriver driver = new ChromeDriver(options);

            // Timeout global (20 segundos)
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(20);
            driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(20);

            return driver;
        }
    }
}
