using System;
using OpenQA.Selenium;
using OpenWeatherMapAutomation.Helpers;

namespace OpenWeatherMapAutomation.Pages {

    public class Login : PageBase {

        public Login(IWebDriver driver) : base(driver) {
        }



	//
	// LoginToMembrain
	////////////////////////////////////////////////////////////////////////////////
	
        public void LoginToWhetherMap(String email, String password) {

			WaitForPageToFullyLoad();
			ExcelHelpers.PopulateInCollection(ProjectPath + "\\Data\\CountryInfo.xlsx");
			String data = ExcelHelpers.ReadData(1, "City");
			ClickElement(GetElement(By.CssSelector(".user-li")));
			SetTextInput(WaitForElement(By.CssSelector("#user_email")), email);
			SetTextInput(WaitForElement(By.CssSelector("#user_password")), password);

			ClickElement(GetElement(By.Name("commit")));
			//WaitForPendingServerRequestsToComplete();
        }
    }
}
