using OpenWeatherMapAutomation.Pages;
using NUnit.Framework;
using System;
using OpenWeatherMapAutomation.Helpers;


namespace OpenWeatherMapAutomation.TestSuiteA {

    class SearchWeatherInYourCity : TestBase {

        public SearchWeatherInYourCity(String browser) : base(browser) {
        }

	//
	// Test Case: Search Weather In Your Country
	////////////////////////////////////////////////////////////////////////////////
	
        [TestCase, Order(1)]
        public void SearchWeatherInYourCountry() {

			String errorMessage = "";

			ExcelHelpers.PopulateInCollection(ProjectPath + "\\Data\\KeywordAndOutcome.xlsx");

			Homepage homepage = new Homepage(WebDriver);

			String keyword = "";

			for (int i = 1; i <= ExcelHelpers.GetRows(); i++) {

				keyword = ExcelHelpers.ReadData(i, "Keyword");

				homepage.InputSearchKey(keyword);

				errorMessage += homepage.VerifySearchResult(i, keyword, ExcelHelpers.ReadData(i, "Expected Outcome"), ExcelHelpers.ReadData(i, "Matching Count"));
			}

			if(!String.IsNullOrEmpty(errorMessage)) {

				throw new Exception(errorMessage);
			}
		}
	}
}