using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace OpenWeatherMapAutomation {

	public abstract class PageBase : SharedBase {

		public PageBase(IWebDriver driver) {

			WebDriver = driver;
			WebDriverWait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
		}

		public static IWebElement ClickToEditPen {
			get {

				return GetElement(By.CssSelector("#clickToEditPen"));
			}
		}
	}
}
