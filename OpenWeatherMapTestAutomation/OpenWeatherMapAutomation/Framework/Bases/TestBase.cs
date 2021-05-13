using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using System.Diagnostics;
using System.Collections.ObjectModel;
using OpenWeatherMapAutomation.Pages;

namespace OpenWeatherMapAutomation {

    [TestFixture("GoogleChrome")]
    //[TestFixture("Firefox")]
    //[Parallelizable(ParallelScope.Self)]
    //[TestFixture]

    public class TestBase : SharedBase {

        public static Boolean IsInitalized = false;
        public static Boolean IsLoggedIn = false;        

        protected PerformanceCounter mPerformanceCounter = new PerformanceCounter();        

		public Int32 SucceededTests = 0;
		public Int32 FailedTests = 0;

        public enum TestrailStatus {

            Passed = 1,
            Blocked = 2,
            Untested = 3,
            Retest = 4,
            Failed = 5
        }
        
        public TestBase(String browser) {

            if (!IsInitalized) {

				Process[] processCollection = Process.GetProcesses();  
    
				foreach (Process p in processCollection) {  

					if (p.ProcessName == "chromedriver") {

						p.Kill();
					}
				}   

                InitDriver(browser);
                IsInitalized = true;
            }
        }



	//
	// BeforeTest
	///////////////////////////////////////////////////////////////////////////////////

		[SetUp]
        public void BeforeTest() {

			Log("Starting Test '" + TestContext.CurrentContext.Test.MethodName + "'", 0);
			Log("", 0);
        }



	//
	// AfterTest
	///////////////////////////////////////////////////////////////////////////////////

		[TearDown]
        public void AfterTest() {

			String outcome = TestContext.CurrentContext.Result.Outcome.ToString();
			String line = "".PadLeft(outcome.Length, '-');

			Log(line);
			Log(outcome.ToUpper());
			Log(line);

			Log("", 0);
			Log("Completed Test '" + TestContext.CurrentContext.Test.MethodName + "'", 0);
			Log("-------------------------------------------------------------", 0);
        }



	//
	// InitDriver
	///////////////////////////////////////////////////////////////////////////////////
	
        public void InitDriver(String browser) {

            if (browser.Equals("GoogleChrome")) {

				ChromeDriverService chromeDriverService = ChromeDriverService.CreateDefaultService(ProjectPath + "\\Drivers");
				chromeDriverService.HideCommandPromptWindow = true;
				chromeDriverService.SuppressInitialDiagnosticInformation = true; 

                ChromeOptions options = new ChromeOptions();
				options.AddArguments("start-maximized");
				options.AddArgument("--host-resolver-rules=MAP www.google-analytics.com 127.0.0.1");
				options.AddArgument("--silent");
				options.UnhandledPromptBehavior = UnhandledPromptBehavior.Accept;

				if (Config.Headless) {

					options.AddArguments("--headless", "window-size=1920,1080", "--no-sandbox"); // Enable for headless option
				}

                WebDriver = new ChromeDriver(chromeDriverService, options);

            } else if (browser.Equals("Firefox")) {

                FirefoxOptions options = new FirefoxOptions();
                //options.AddArguments("--headless", "window-size=1280,1024", "--no-sandbox"); // Enable for headless option

                WebDriver = new FirefoxDriver(ProjectPath + "\\Drivers");
				WebDriver.Manage().Window.Maximize();
            }
        }



	//
	// GetScreenshotSavePath
	///////////////////////////////////////////////////////////////////////////////////
	
        protected String GetScreenshotSavePath() {

            String nameSpace = this.GetType().Namespace;
            String path = ProjectPath + "\\Screenshot\\" + nameSpace + "\\";

            if (!Directory.Exists(path)) {

                Directory.CreateDirectory(path);
            }

            return path;
        }



	//
	// TakeScreenshot
	///////////////////////////////////////////////////////////////////////////////////
	
        protected String TakeScreenshot(String imageName) {

            String path = GetScreenshotSavePath() + imageName + ".png";
            Screenshot ss = ((ITakesScreenshot)WebDriver).GetScreenshot();
            String screenshot = ss.AsBase64EncodedString;
            byte[] screenshotAsByteArray = ss.AsByteArray;
            ss.SaveAsFile(path, ScreenshotImageFormat.Png);
            
			return path;
        }



	//
	// OneTimeSetUp
	///////////////////////////////////////////////////////////////////////////////////
	
        [OneTimeSetUp]
        public void OneTimeSetUp() {

            if (!IsLoggedIn) {

                WebDriver.Navigate().GoToUrl(Config.BaseURL);

				WaitForPageToFullyLoad();
				//Login login = new Login(WebDriver);
				//login.LoginToWhetherMap(Config.Username, Config.Password);

				//            IsLoggedIn = true;
			}

            mPerformanceCounter.StartCounter();
        }



	//
	// OneTimeTearDown
	///////////////////////////////////////////////////////////////////////////////////

        [OneTimeTearDown]
        public void OneTimeTearDown() {

			if (FailedTests == 0 && SucceededTests > 0) {

				if (WebDriver != null) {

					WebDriver.Close();
				}

			}
        }
	}
}
