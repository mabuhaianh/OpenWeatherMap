using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;

namespace OpenWeatherMapAutomation.Pages {
	public class Homepage : PageBase {

		public Homepage(IWebDriver driver) : base(driver) {
		}

		private IWebElement weatherInYourCitySearchInput {
			get {
				return GetElement(By.CssSelector(".search-container > input"));
			}
		}

		private String searchDropDownMenuCssSelector = ".search-dropdown-menu";

		private List<IWebElement> searchItems {
			get {
				return GetElements(By.XPath("//ul[@class = 'search-dropdown-menu']/li"));
			}
		}

		private String notFoundSubOpenCssSelector = ".sub.not-found.notFoundOpen";

		public void InputSearchKey(String searchKey) {

			Log("Input searchKey: " + searchKey);

			SetTextInput(weatherInYourCitySearchInput, searchKey);

			weatherInYourCitySearchInput.SendKeys(Keys.Enter);
		}

		public String VerifySearchResult(Int32 row, String keyword, String expectedOutcome, String matchingCount) {

			String errorMessage = "";

			if(expectedOutcome.Equals("found")) {

				Int32 count = Int32.Parse(matchingCount);

				if (!IsElementPresentAndVisible(By.CssSelector(searchDropDownMenuCssSelector), 2)) {

					errorMessage = "Row '" + row + "', Keyword '"+ keyword + "': Search Drop Down Menu does not show \n";
				}

				if (count != searchItems.Count) {

					errorMessage = "Row '" + row + "', Keyword '" + keyword + "': Matching items is incorrect. Current: " + searchItems.Count + ", Expectation: "+ count + ". \n";
				}

			} else {

				if(!IsElementPresentAndVisible(By.CssSelector(notFoundSubOpenCssSelector), 2)) {

					errorMessage = "Row '" + row + "', Keyword '" + keyword + "': The message 'Not found. To make search more precise put the city's name, comma, 2-letter country code (ISO3166).' does not show \n";
				}
			}

			if(String.IsNullOrEmpty(errorMessage)) {

				Log("Keyword '" + keyword + "' on row '" + row + "' with Expected Outcome '" + expectedOutcome + "' and Matching Items '" + matchingCount + "' ===> Passed");

			} else {

				Log("Keyword '" + keyword + "' on row '" + row + "' with Expected Outcome '" + expectedOutcome + "' and Matching Items '" + matchingCount + "' ===> Failed");
			}

			return errorMessage;
		}
	}
}
