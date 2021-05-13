using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Text;

namespace OpenWeatherMapAutomation {

	public class TestBaseConfig {

		public String BaseURL;
		public String Username;
		public String Password;
		public Boolean TestFileUpload = false;
		public Boolean LogInternals = false;
		public Boolean Headless = false;
	}



	//
	// SharedBase
	///////////////////////////////////////////////////////////////////////////////////

	public class SharedBase {

		public const Double WAIT_TIMEOUT = 10;

		public const Double DEFAULT_FULL_WAIT = 10;
		public const Double DEFAULT_SINGLE_WAIT = 0.1;

		public static IWebDriver WebDriver = null;
		public static WebDriverWait WebDriverWait = null;
		public static WebDriverWait OldWait = null;
		public static TestBaseConfig Config = new TestBaseConfig();
		public static String ProjectPath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
		public static StringBuilder LogString = new StringBuilder();
		public static String RunId = DateTime.Now.ToString("u").Replace("Z", "").Replace(":", ".");

		protected SharedBase() {

			LoadConfigFile();
		}



		//
		// LoadConfigFile
		///////////////////////////////////////////////////////////////////////////////////

		public void LoadConfigFile() {

			XmlDocument dom = new XmlDocument();
			dom.XmlResolver = null;
			dom.Load(ProjectPath + "\\Config.xml");

			XmlNode configNode = dom["config"];
			Config.BaseURL = configNode["baseUrl"].InnerText;
			Config.Username = configNode["username"].InnerText;
			Config.Password = configNode["password"].InnerText;
		}



		//
		// Log
		///////////////////////////////////////////////////////////////////////////////////

		public static void Log(String s, Int32 level = 1) {

			String toLog = DateTime.Now.ToLongTimeString() + " - " + "".PadLeft(level * 3, ' ') + s + "\n";

			Console.Write(toLog);

			LogString.Append(toLog);

			File.AppendAllText(ProjectPath + "\\Logs\\Automation Log " + RunId + ".txt", toLog);
		}



		//
		// MouseMoveToLocation
		///////////////////////////////////////////////////////////////////////////////////

		protected static void MouseMoveToLocation(int x, int y) {

			Thread.Sleep(10);

			Actions actions = new Actions(WebDriver);
			actions.MoveByOffset(x, y);
			actions.Perform();

			Thread.Sleep(10);
		}



		//
		// PressEnter
		///////////////////////////////////////////////////////////////////////////////////

		public static void PressEnter() {

			Thread.Sleep(10);

			Actions actions = new Actions(WebDriver);
			actions.SendKeys(Keys.Enter).Perform();

			Thread.Sleep(10);
		}



		//
		// BlurActiveElement
		///////////////////////////////////////////////////////////////////////////////////

		public static void BlurActiveElement() {

			IJavaScriptExecutor js = (IJavaScriptExecutor)WebDriver;
			js.ExecuteScript("try { document.activeElement.blur(); } catch (e) {}");
		}



		//
		// GetElementId
		///////////////////////////////////////////////////////////////////////////////////

		public static String GetElementId(IWebElement element) {

			if (element == null) { return "No Element"; }
			return element.GetAttribute("id");
		}



		//
		// PressTab
		///////////////////////////////////////////////////////////////////////////////////

		public static void PressTab() {

			Thread.Sleep(10);

			Actions actions = new Actions(WebDriver);
			actions.SendKeys(Keys.Tab).Perform();

			Thread.Sleep(10);
		}



		//
		// PressESC
		///////////////////////////////////////////////////////////////////////////////////

		protected static void PressESC() {

			Thread.Sleep(10);

			Actions actions = new Actions(WebDriver);
			actions.SendKeys(Keys.Escape).Perform();

			Thread.Sleep(10);
		}



		//
		// IsElementPresentAndVisible
		///////////////////////////////////////////////////////////////////////////////////

		protected static Boolean IsElementPresentAndVisible(By selector, Double timeout = 0.1) {

			List<IWebElement> elements;

			while (timeout > 0) {

				elements = WebDriver.FindElements(selector).ToList();

				if (elements.Count > 0 && ElementIsViable(elements[0])) {

					return true;
				}

				Thread.Sleep(10);

				timeout -= 0.01;
			}

			return false;
		}



		//
		// PreventPreviewPopUp
		///////////////////////////////////////////////////////////////////////////////////

		public static void PreventPreviewPopUp() {

			MoveToElement(GetElement(By.CssSelector(".logo")));
		}



		//
		// WaitForElementVisible
		///////////////////////////////////////////////////////////////////////////////////

		public static void WaitForElementVisible(IWebElement element, Double timeout = 5) {

			Double d = 0;

			while (!element.Displayed) {

				Thread.Sleep(10);
				d += 0.01;

				if (d > timeout) {

					throw new Exception("Element " + element + " not visible within timeout window");
				}
			}
		}



		//
		// WaitForPageToFullyLoad
		///////////////////////////////////////////////////////////////////////////////////

		public static void WaitForPageToFullyLoad() {

			PerformanceCounter performanceCounter = new PerformanceCounter(true);

			Thread.Sleep(50); // we do a small sleep to ensure the JS engine actually has enough time to send out ServerRequests if it has to

			//WaitForPendingServerRequestsToComplete();
			WaitForLoadingComplete();

			if (Config.LogInternals) {

				Log("WaitForPageToFullyLoad completed in " + performanceCounter.StopCounterAndGetTimeInMS() + " ms", 2);
			}
		}



		//
		// WaitForFullOpacity
		///////////////////////////////////////////////////////////////////////////////////

		public static void WaitForFullOpacity(IWebElement element, Double timeout = 10) {

			Double t = 0;

			while (!element.GetCssValue("opacity").Equals("1")) {

				Thread.Sleep(10);
				t += 0.01;

				if (t > timeout) {

					break;
				}
			}

			if (element.GetCssValue("opacity").Equals("1")) {

				return;
			}

			throw new Exception("Opacity was not 1 within the timeout window");
		}



		//
		// FormatException
		///////////////////////////////////////////////////////////////////////////////////

		public static String FormatException(Exception ex) {

			return ex.Message + " - " + ex.StackTrace;
		}



		//
		// GoToPageAndWaitForLoadComplete
		///////////////////////////////////////////////////////////////////////////////////

		public static void GoToPageAndWaitForLoadComplete(String url) {

			WebDriver.Navigate().GoToUrl(Config.BaseURL + url);
			WaitForPageToFullyLoad();
		}



		//
		// WaitForPendingServerRequestsToComplete
		///////////////////////////////////////////////////////////////////////////////////

		public static void WaitForPendingServerRequestsToComplete() {

			PerformanceCounter performanceCounter = new PerformanceCounter(true);

			IJavaScriptExecutor js = (IJavaScriptExecutor)WebDriver;
			Int32 max = 1000; // 10 seconds

			while (Convert.ToInt32(js.ExecuteScript("return ServerRequestsGetActiveAndPendingCount()")) != 0) {

				Thread.Sleep(10);
				max--;

				if (max == 0) { throw new Exception("Pending Requests never zero"); }
			}

			if (Config.LogInternals) {

				Log("WaitForPendingServerRequestsToComplete completed in " + performanceCounter.StopCounterAndGetTimeInMS() + " ms", 2);
			}
		}


		//
		// MoveToElement
		///////////////////////////////////////////////////////////////////////////////////

		public static void MoveToElement(IWebElement element) {

			Actions actions = new Actions(WebDriver);
			actions.MoveToElement(element).Perform();

		}

		//
		// DragAndropElement
		///////////////////////////////////////////////////////////////////////////////////

		public static void DragAndDropElementToOffset(IWebElement source, Int32 offsetX, Int32 offsetY) {

			Actions actions = new Actions(WebDriver);
			actions.DragAndDropToOffset(source, offsetX, offsetY).Perform();

		}


		//
		// GetElement
		///////////////////////////////////////////////////////////////////////////////////

		public static IWebElement GetElement(By selector, IWebElement ancestor = null) {

			PerformanceCounter pc = new PerformanceCounter(true);

			try {

				IWebElement element;

				if (ancestor == null) {

					element = WebDriver.FindElement(selector);

				} else {

					element = ancestor.FindElement(selector);
				}

				if (ElementIsViable(element)) {

					if (Config.LogInternals) {

						Log("GetElement: '" + selector.ToString() + "' returned element in " + (pc.StopCounterAndGetTimeInSeconds() * 1000) + " ms", 2);
					}

					return element;

				} else {

					if (Config.LogInternals) {

						Log("GetElement: '" + selector.ToString() + "' returned null in " + (pc.StopCounterAndGetTimeInSeconds() * 1000) + " ms", 2);
					}

					return null;
				}

			} catch (Exception ex) {

				if (Config.LogInternals) {

					Log("GetElement: '" + selector.ToString() + "' returned null in " + (pc.StopCounterAndGetTimeInSeconds() * 1000) + " ms", 2);
				}

				return null;
			}
		}


		//
		// GetElements
		///////////////////////////////////////////////////////////////////////////////////

		public static List<IWebElement> GetElements(By selector, IWebElement ancestor = null) {

			PerformanceCounter pc = new PerformanceCounter(true);

			List<IWebElement> elements;

			if (ancestor == null) {

				elements = WebDriver.FindElements(selector).ToList();

			} else {

				elements = ancestor.FindElements(selector).ToList();
			}

			List<IWebElement> viableElements = new List<IWebElement>();

			foreach (IWebElement element in elements) {

				if (ElementIsViable(element)) {

					viableElements.Add(element);
				}
			}

			if (Config.LogInternals) {

				Log("GetElements: '" + selector.ToString() + "' returned " + viableElements.Count + " elements in " + (pc.StopCounterAndGetTimeInSeconds() * 1000) + " ms", 2);
			}

			return viableElements;
		}


		//
		// ClickElement
		// NOTE: use in place of element.Click, thus we can make this function a smarter
		// more robust variant that for example waits a little bit for element visibility
		// / interactivity and all that
		///////////////////////////////////////////////////////////////////////////////////

		public static void ClickElement(IWebElement element) {

			if (element == null) { throw new Exception("ClickElement was called with a null element"); }

			WaitForElementVisible(element);

			MoveToElement(element);

			try {

				element.Click();

			} catch (Exception ex) {

				if (ex.Message.Contains("element click intercepted") && ex.Message.Contains("previewElementContent")) {

					PreventPreviewPopUp();
					element.Click();
				}
			}
		}



		//
		// Wait
		///////////////////////////////////////////////////////////////////////////////////

		public static void Wait(Double time) {

			Thread.Sleep((Int32)(time * 1000));
		}




		//
		// WaitForElement
		///////////////////////////////////////////////////////////////////////////////////

		public static IWebElement WaitForElement(By selector, IWebElement ancestor = null) {

			PerformanceCounter pc = new PerformanceCounter(true);

			while (true && pc.GetTimePassed() < DEFAULT_FULL_WAIT) {

				IWebElement element = GetElement(selector, ancestor);

				if (element != null) {

					return element;
				}

				Wait(DEFAULT_SINGLE_WAIT);
			}

			throw new Exception("WaitForElement Failed: " + selector.ToString());
		}



		//
		// GetParentElement
		///////////////////////////////////////////////////////////////////////////////////

		public static IWebElement GetParentElement(IWebElement e) {

			return e.FindElement(By.XPath(".."));
		}



		//
		// ElementIsViable
		// does various checks to make sure the element actually is potentially
		// visible, even if it is not currently on screen, as we often get elements
		// that are in the DOM but hidden
		///////////////////////////////////////////////////////////////////////////////////

		public static Boolean ElementIsViable(IWebElement element) {

			try {

				if (element == null) {

					return false;
				}

				if (!element.Displayed) {

					if (element.GetCssValue("display") == "none") {

						return false;
					}

					IWebElement parentElement = GetParentElement(element);

					while (parentElement != null) {

						if (parentElement.GetCssValue("display") == "none") {

							return false;
						}

						if (parentElement.TagName.Equals("body")) {

							break;

						} else {

							parentElement = GetParentElement(parentElement);
						}
					}
				}

				return true;

			} catch {

				return false;
			}
		}


		//
		// ScrollToElement
		///////////////////////////////////////////////////////////////////////////////////

		protected static void ScrollToElement(IWebElement element, Int32 verticalOffset = 0) {

			IJavaScriptExecutor js = (IJavaScriptExecutor)WebDriver;
			js.ExecuteScript("arguments[0].scrollIntoView(false);", element);

			if (verticalOffset != 0) {

				js.ExecuteScript("E('MainFrame').scrollTop += " + verticalOffset + ";", element);
			}
		}


		//
		// WaitForMemBrainIsLoadingComplete
		///////////////////////////////////////////////////////////////////////////////////

		public static void WaitForLoadingComplete(Double timeout = WAIT_TIMEOUT) {

			IWebElement loader = GetElement(By.CssSelector(".own-loader"));

			if (loader != null) {

				while (timeout > 0) {

					Thread.Sleep(10);
					timeout -= 0.01;
				}

				throw new Exception("Wait For loader timed out");
			}
		}

		//
		// WaitForBrowser
		///////////////////////////////////////////////////////////////////////////////////

		public static void WaitForBrowser(Double d) {

			Thread.Sleep((Int32)(d * 1000));
		}


		//
		// SetTextInput
		///////////////////////////////////////////////////////////////////////////////////

		public static void SetTextInput(IWebElement element, String s) {

			MoveToElement(element);
			element.Clear();
			element.SendKeys(s);
			BlurActiveElement();
		}

	}
}
