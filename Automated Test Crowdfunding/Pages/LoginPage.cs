using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers; // Para usar ExpectedConditions
using System; // Para usar TimeSpan


namespace Automated_Test_Crowdfunding.Pages
{
    public class LoginPage
    {
        private readonly IWebDriver _driver;
        private readonly string _url = @"file:///C:/Users/vnt/OneDrive%20-%20Instituto%20Tecnol%C3%B3gico%20de%20Las%20Am%C3%A9ricas%20(ITLA)/c%23%20proyect/StudentsProyectsCRUD/CrowdFunding.Frontend/index.html";

        // Constructor, GoTo() y Login() se mantienen igual.

        public LoginPage(IWebDriver driver)
        {
            _driver = driver;
        }

        // ----------- Selectores -----------

        private IWebElement UserInput => _driver.FindElement(By.Id("username"));
        private IWebElement PasswordInput => _driver.FindElement(By.Id("password"));
        private IWebElement LoginForm => _driver.FindElement(By.Id("login-form"));

        // Mantener este selector por si se usa en otro lugar, aunque no lo usaremos directamente en el nuevo método.
        private IWebElement HomeSection => _driver.FindElement(By.Id("home-section"));


        // ----------- Métodos -----------

        public void GoTo()
        {
            _driver.Navigate().GoToUrl(_url);
        }

        public void Login(string username, string password)
        {
            UserInput.Clear();
            UserInput.SendKeys(username);

            PasswordInput.Clear();
            PasswordInput.SendKeys(password);

            LoginForm.Click();
        }

        // 🟢 MÉTODO CORREGIDO: Usando WebDriverWait para sincronización.
        public bool IsLoginSuccessful()
        {
            // Define el localizador del elemento que confirma el éxito
            // Ya sabemos que es By.Id("home-section")
            By successLocator = By.Id("home-section");

            // Define el tiempo máximo de espera (ej. 10 segundos)
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            try
            {
                // Espera activamente hasta que el elemento sea visible.
                // Si la espera es exitosa, devuelve true.
                wait.Until(ExpectedConditions.ElementIsVisible(successLocator));
                return true;
            }
            catch (WebDriverTimeoutException)
            {
                // Si se agota el tiempo de espera, la aserción falla.
                return false;
            }
            catch (Exception)
            {
                // Manejar cualquier otra excepción (ej. NoSuchElementException)
                return false;
            }
        }
    }
}