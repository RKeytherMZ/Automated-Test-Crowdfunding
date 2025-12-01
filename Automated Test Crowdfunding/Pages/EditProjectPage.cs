using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers; // Para usar ExpectedConditions
using System;

namespace Automated_Test_Crowdfunding.Pages
{
    public class EditProjectPage : CreateProjectPage
    {
        public EditProjectPage(IWebDriver driver) : base(driver) { }

        public bool IsEditMode()
        {
            return _driver.FindElement(By.CssSelector("#create-project-form button"))
                          .Text.Contains("Actualizar");
        }
    }
}
