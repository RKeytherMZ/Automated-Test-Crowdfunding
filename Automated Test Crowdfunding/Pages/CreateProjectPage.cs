using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers; // Para usar ExpectedConditions
using System;

namespace Automated_Test_Crowdfunding.Pages
{
    public class CreateProjectPage
    {
        protected readonly IWebDriver _driver;

        public CreateProjectPage(IWebDriver driver)
        {
            _driver = driver;
        }

        private IWebElement Title => _driver.FindElement(By.Id("title"));
        private IWebElement Description => _driver.FindElement(By.Id("description"));
        private IWebElement Goal => _driver.FindElement(By.Id("fundingGoal"));
        private IWebElement Start => _driver.FindElement(By.Id("startDate"));
        private IWebElement End => _driver.FindElement(By.Id("endDate"));
        private IWebElement Students => _driver.FindElement(By.Id("studentIds"));
        private IWebElement Status => _driver.FindElement(By.Id("status"));
        private IWebElement Submit => _driver.FindElement(By.CssSelector("#create-project-form button"));

        public string GetAndAcceptAlertText()
        {
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));

            try
            {
                // 🟢 SOLUCIÓN: Esperar hasta que la alerta esté presente (ExpectedConditions.AlertIsPresent()).
                wait.Until(ExpectedConditions.AlertIsPresent());

                // Ahora que la alerta existe, cambiamos el foco del driver a ella.
                IAlert alert = _driver.SwitchTo().Alert();

                string alertText = alert.Text;

                // Aceptamos/cerramos la alerta para que la ejecución continúe.
                alert.Accept();

                return alertText;
            }
            catch (WebDriverTimeoutException)
            {
                // Si la alerta no aparece en 5 segundos, esto es un fallo de la prueba negativa.
                throw new Exception("Error de sincronización: La alerta de error (Bad Request) no apareció después de enviar el formulario.");
            }
        }

        public void FillForm(string title, string desc, string goal, string start, string end, string students, string status)
        {
            Title.Clear(); Title.SendKeys(title);
            Description.Clear(); Description.SendKeys(desc);
            Goal.Clear(); Goal.SendKeys(goal);
            Start.Clear(); Start.SendKeys(start);
            End.Clear(); End.SendKeys(end);
            Students.Clear(); Students.SendKeys(students);
            Status.SendKeys(status);
        }

        public void SubmitForm()
        {
            Submit.Click();
            
        }
    }
}
