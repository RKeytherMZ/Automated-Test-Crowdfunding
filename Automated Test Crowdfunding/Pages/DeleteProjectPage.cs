using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers; // Para usar ExpectedConditions
using System;

namespace Automated_Test_Crowdfunding.Pages
{
    public class DeleteProjectPage
    {
        private readonly IWebDriver _driver;

        public DeleteProjectPage(IWebDriver driver)
        {
            _driver = driver;
        }

        public void ConfirmDelete()
        {
            var alert = _driver.SwitchTo().Alert();
            alert.Accept();
            Thread.Sleep(1500);
        }
    }
}
