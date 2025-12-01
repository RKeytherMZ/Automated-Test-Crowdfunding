using OpenQA.Selenium;

namespace Automated_Test_Crowdfunding.Pages
{
    public class LoginPage
    {
        private readonly IWebDriver _driver;
        private readonly string _url = @"file:///C:/Users/vnt/OneDrive%20-%20Instituto%20Tecnol%C3%B3gico%20de%20Las%20Am%C3%A9ricas%20(ITLA)/c%23%20proyect/StudentsProyectsCRUD/CrowdFunding.Frontend/index.html";
        

        public LoginPage(IWebDriver driver)
        {
            _driver = driver;
        }

        // ----------- Selectores -----------

        private IWebElement UserInput => _driver.FindElement(By.Id("username"));
        private IWebElement PasswordInput => _driver.FindElement(By.Id("password"));
        private IWebElement LoginForm => _driver.FindElement(By.Id("login-form"));
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

        public bool IsLoginSuccessful()
        {
            try
            {
                return HomeSection.Displayed;
            }
            catch
            {
                return false;
            }
        }
    }
}
