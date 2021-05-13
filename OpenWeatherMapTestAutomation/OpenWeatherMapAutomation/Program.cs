using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenWeatherMapAutomation.TestSuiteA;

namespace OpenWeatherMapAutomation {

	class Program {

		public const String COLOR_GREEN = "rgb(65, 200, 100)";
		public const String COLOR_RED = "rgb(235, 65, 65)";
		public const String COLOR_BLUE = "rgb(84, 146, 201)";
		public const String COLOR_ORANGE = "rgb(255, 201, 84)";
		public const String COLOR_MAIN_BACKGROUND = "rgb(248, 249, 250)";
		public const String COLOR_LIGHT_GREY_ON_WHITE = "rgb(243, 244, 245)";
		public const String COLOR_MEDIUM_GREY_ON_WHITE = "rgb(233, 233, 233)";
		public const String COLOR_MEDIUM_GREY_ON_GREY = "rgb(210, 210, 210)";
		public const String COLOR_WHITE = "rgb(255, 255, 255)";
		public static List<String> SuccessfullyCompletedTests = new List<String>();

		delegate void TestDelegate();

		public enum TestResult {

			SUCCESS,
			FAIL,
			SKIPPED,
			NOT_RUN
		}

		class NamedTestDelegate {

			public String Name = "";
			public TestDelegate Delegate = null;
			public Double CompletedTime = -1;
			public TestResult TestResult = TestResult.NOT_RUN;
			public List<String> DependsOn = null;
			public Exception Exception = null;
			public String SkippedReason = "";
			public String Screenshot = "";
		}

		class TestSuite {

			public List<NamedTestDelegate> NamedTestDelegates = new List<NamedTestDelegate>();
			public String Name;

			public void AddTest(String name, TestDelegate testDelegate, List<String> dependsOn = null) {

				NamedTestDelegate namedTestDelegate = new NamedTestDelegate();
				namedTestDelegate.Name = name;
				namedTestDelegate.Delegate = testDelegate;
				namedTestDelegate.DependsOn = dependsOn;

				NamedTestDelegates.Add(namedTestDelegate);
			}
		}

		static String browser = "GoogleChrome";
		static List<TestSuite> ExecutedTestSuites = new List<TestSuite>();



	//
	// TakeScreenshot
	///////////////////////////////////////////////////////////////////////////////////
	
        protected static String TakeScreenshot(String imageName) {

            String path = TestBase.ProjectPath + "\\Logs\\" + imageName;
            OpenQA.Selenium.Screenshot ss = ((OpenQA.Selenium.ITakesScreenshot)TestBase.WebDriver).GetScreenshot();
            String screenshot = ss.AsBase64EncodedString;
            byte[] screenshotAsByteArray = ss.AsByteArray;
            ss.SaveAsFile(path, OpenQA.Selenium.ScreenshotImageFormat.Png);
            
			return path;
        }



	//
	// Main
	////////////////////////////////////////////////////////////////////////////////

		static void Main(string[] args) {

			SharedBase.Config.Headless = false;

			List<String> suitesToRun = new List<String>();
			suitesToRun.Add("search-Weather-In-Your-City");

			//
			// Define Suites
			//

			List<TestSuite> testSuites = new List<TestSuite>();

			//
			// Clean Up Test Suite
			//

			SearchWeatherInYourCity searchWeatherInYourCity = new SearchWeatherInYourCity(browser);

			TestSuite testSuite = new TestSuite();
			testSuite.Name = "Search-Weather-In-Your-City";

			testSuite.AddTest("Search Weather In Your Country", new TestDelegate(searchWeatherInYourCity.SearchWeatherInYourCountry));
			testSuites.Add(testSuite);

			//
			// Run Suites
			//

			Boolean isFirstTestSuite = true;

			foreach (TestSuite ts in testSuites) {

				if (suitesToRun.Contains(ts.Name)) {

					if (isFirstTestSuite) {

						isFirstTestSuite = false;

					} else {

						SharedBase.Log("");
						SharedBase.Log("");
						SharedBase.Log("");
					}

					RunTestSuite(ts);

				} else {

					SharedBase.Log("Skipped Test Suite: '" + ts.Name + "'", 0);
				}
			}

			//
			// Write Results
			//

			StringBuilder sb = new StringBuilder();
			sb.Append("<html>");
			sb.Append("<head>");
			sb.Append("<title>Test Run Results</title>");
			sb.Append("</head>");
			sb.Append("<body style=\"font-family: Arial; font-size: 14px; padding: 32px;\">");

			Int32 successful = 0;
			Int32 failed = 0;
			Int32 skipped = 0;

			foreach (TestSuite executedTestSuite in ExecutedTestSuites) {

				foreach (NamedTestDelegate test in executedTestSuite.NamedTestDelegates) {

					if (test.TestResult == TestResult.SUCCESS) {

						successful += 1;

					} else if (test.TestResult == TestResult.FAIL) {

						failed += 1;

					} else if (test.TestResult == TestResult.SKIPPED) {

						skipped += 1;
					}
				}
			}

			sb.Append("<h1>Test Run Report, Successful: " + successful + ", Skipped: " + skipped + ", Failed: " + failed + "</h1>");

			foreach (TestSuite executedTestSuite in ExecutedTestSuites) {

				sb.Append("<h2>" + executedTestSuite.Name + "</h2>");

				foreach (NamedTestDelegate test in executedTestSuite.NamedTestDelegates) {

					String color = COLOR_LIGHT_GREY_ON_WHITE;

					if (test.TestResult == TestResult.SUCCESS) {

						color = COLOR_GREEN;

					} else if (test.TestResult == TestResult.FAIL) {

						color = COLOR_RED;

					} else if (test.TestResult == TestResult.SKIPPED) {

						color = COLOR_ORANGE;
					}

					sb.Append("<div style=\"width: 16px; height: 16px; display: inline-block; vertical-align: middle; background-color: " + color + "; border-radius: 50%; margin-right: 8px;\"></div>");
					sb.Append("<div style=\"display: inline-block; vertical-align: middle;\">" + test.Name + "</div>");
					sb.Append("<br />");

					if (test.Exception != null) {

						sb.Append("<div style=\"padding-left: 24px; color: #888888; padding-top: 8px; padding-bottom: 8px;\">");
						sb.Append(SharedBase.FormatException(test.Exception).Replace("\n", "<br />"));
						sb.Append("</div>");
					}

					if (test.Screenshot != "") {

						sb.Append("<div style=\"padding-left: 24px; padding-top: 8px; padding-bottom: 8px;\">");
						sb.Append("<a href=\"" + test.Screenshot + "\"><img src=\"" + test.Screenshot + "\" width=\"192\" height=\"108\" style=\"border: 0px;\" /></a>");
						sb.Append("</div>");
					}
				}
			}

			sb.Append("</body>");
			sb.Append("</html>");

			System.IO.File.AppendAllText(TestBase.ProjectPath + "\\Logs\\Test Run Results " + TestBase.RunId + ".html", sb.ToString());
		}



	//
	// RunTestSuite
	////////////////////////////////////////////////////////////////////////////////

		static void RunTestSuite(TestSuite testSuite) {

			ExecutedTestSuites.Add(testSuite);

			SharedBase.Log("===============================================================", 0);
			SharedBase.Log("RUNNING TEST SUITE: '" + testSuite.Name + "', " + testSuite.NamedTestDelegates.Count + " tests", 0);
			SharedBase.Log("===============================================================", 0);
			SharedBase.Log("");

			TestBase testBase = new TestBase(browser); // just using this because TestBase is abstract and I don't have the time now to fix that
			testBase.OneTimeSetUp();

			Int32 testIndex = 0;

			foreach (NamedTestDelegate testDelegate in testSuite.NamedTestDelegates) {

				testIndex += 1;

				if (testDelegate.DependsOn != null) {

					foreach (String dependsOn in testDelegate.DependsOn) {

						if (!SuccessfullyCompletedTests.Contains(dependsOn)) {

							if (testIndex == 0) {
							
								SharedBase.Log("---------------------------------------------------------------", 0);
							}

							SharedBase.Log("");
							SharedBase.Log("Skipped: '" + testDelegate.Name + "', it depends on '" + dependsOn + "' which has either not run at all, or failed", 0);
							SharedBase.Log("");
							SharedBase.Log("---------------------------------------------------------------", 0);

							testDelegate.TestResult = TestResult.SKIPPED;
							testDelegate.SkippedReason = "Skipped: '" + testDelegate.Name + "', it depends on '" + dependsOn + "' which has either not run at all, or failed";

							break;
						}
					}

					if (testDelegate.TestResult == TestResult.SKIPPED) {

						continue;
					}
				}

				PerformanceCounter pc = new PerformanceCounter(true);

				try {

					if (testIndex == 0) {

						SharedBase.Log("---------------------------------------------------------------", 0);
					}

					SharedBase.Log("");
					SharedBase.Log("Running: '" + testDelegate.Name + "' (" + testIndex + " of " + testSuite.NamedTestDelegates.Count + ")", 0);
					SharedBase.Log("");

					testDelegate.Delegate.Invoke();

					testDelegate.TestResult = TestResult.SUCCESS;
					testDelegate.CompletedTime = pc.StopCounterAndGetTimeInSeconds();

					SuccessfullyCompletedTests.Add(testDelegate.Name);

					SharedBase.Log("");
					SharedBase.Log("Completed: '" + testDelegate.Name + "' (Completed in " + testDelegate.CompletedTime + "s)", 0);
					SharedBase.Log("");
					SharedBase.Log("---------------------------------------------------------------", 0);

				} catch (Exception ex) {

					testDelegate.TestResult = TestResult.FAIL;
					testDelegate.CompletedTime = pc.StopCounterAndGetTimeInSeconds();
					testDelegate.Exception = ex;

					String screenshotName = "Failed_" + DateTime.UtcNow.Ticks + ".png";
					TakeScreenshot(screenshotName);
					testDelegate.Screenshot = screenshotName;

					SharedBase.Log("");
					SharedBase.Log("FAILED: '" + testDelegate.Name + "', Exception: '" + ex.Message + "' (Failed in " + testDelegate.CompletedTime + "s)", 0);
					SharedBase.Log("");
					SharedBase.Log("---------------------------------------------------------------", 0);
				}
			}

			testBase.OneTimeTearDown();
		}
	}
}
