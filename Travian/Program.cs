using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Travian
{
    class Program
    {
        public static IWebDriver webDriver = new ChromeDriver();

        static void Main(string[] args)
        {
            OpenLogin();
            LoginHandler();
            //GetMinimumLevelResourcesAndUpgrade();
            ResourceUpgradeLoop();
        }

        public  static void ResourceUpgradeLoop()
        {
            while(true)
            {
                if (!IsResourceBeingUpgraded())
                {
                    GetMinimumLevelResourcesAndUpgrade();
                }
                else
                {                  
                    var sleepTime = GetResourceUpgradeTime();
                    Console.WriteLine("GOING DOWN: "+sleepTime);
                    Thread.Sleep(sleepTime*1000+1000);
                }
            }
        }

        public static void GetMinimumLevelResourcesAndUpgrade()
        {
            var resources = WaitForElement("//map/area[contains(@href, 'build')]");
            var min = resources[0];

            foreach (var resource in resources)
            {
                if (int.Parse(Regex.Match(min.GetAttribute("alt"), @"\d+").Value) >
                       int.Parse(Regex.Match(resource.GetAttribute("alt"), @"\d+").Value))
                {
                    min = resource;
                }
            }
            min.Click();

            var resourceUpgradeMenu = webDriver.FindElement(By.XPath("//div[@class='button-content']"));
            var closeButton = webDriver.FindElement(By.XPath("//a[@id='closeContentButton']"));
            if(!resourceUpgradeMenu.Text.Contains("Construct with master builder"))
            {
                resourceUpgradeMenu.Click();
            }
            else
            {
                closeButton.Click();
            }
        }

        public static void OpenLogin()
        {
            webDriver.Navigate().GoToUrl("https://www.travian.com/international");
            webDriver.Manage().Window.Maximize();

            IWebElement loginElement = webDriver.FindElement(By.XPath("//a[@title='Login']"));
            loginElement.Click();
        }

        public static void LoginHandler()
        {
            if (webDriver.FindElements(By.XPath("//div[@class='registrationWrapper front']")).Count > 0)
            { // enter pass
                Login();
            }
            else
            { // select server - > enter pass
                var serverSelector = webDriver.FindElements(By.XPath("//span[@class='title']"));

                foreach (var server in serverSelector)
                {
                    if (server.Text == "COM2")
                    {
                        server.Click();
                        break;
                    }
                }
                Thread.Sleep(3000);
                Login();
            }
        }

        public static void Login()
        {
            WaitForElement("//input[@name='usernameOrEmail']");
            IWebElement inputUserName = webDriver.FindElement(By.XPath("//input[@name='usernameOrEmail']"));
            IWebElement inputPassword = webDriver.FindElement(By.XPath("//input[@name='password']"));
            IWebElement loginButton = webDriver.FindElement(By.XPath("//button[@class='button default'][@type='submit']"));

            inputUserName.SendKeys("yoss");
            inputPassword.SendKeys("yosr6da");
            loginButton.Click();
        }

        #region Helpers
        public static ReadOnlyCollection<IWebElement> WaitForElement(string findByString)
        {
            while (webDriver.FindElements(By.XPath(findByString)).Count == 0)
            {
                Thread.Sleep(500);
            }
            return webDriver.FindElements(By.XPath(findByString));
        }

        public static bool IsResourceBeingUpgraded()
        {
            
            if (webDriver.FindElements(By.XPath("//div[@class='buildDuration']/span[@class='timer']")).Count > 0)
                return true;
            return false;
        }

        public static int GetResourceUpgradeTime()
        {
            var timer = webDriver.FindElement(By.XPath("//div[@class='buildDuration']/span[@class='timer']"));
            return int.Parse(timer.GetAttribute("value"));
        }
        #endregion Helpers
    }
}
