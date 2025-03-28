using System.Globalization;

namespace Infrastructure.Helpers
{
	public static class FunctionsHelper
	{
		public static bool DecimalParse(string str, out decimal ret)
		{
			str = str.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
			str = str.Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
			str = str.Replace(CultureInfo.CurrentCulture.NumberFormat.CurrencyGroupSeparator, "");
			var parseResult = decimal.TryParse(str, out ret);
			if (!parseResult)
			{
				ret = 0;
			}

			return parseResult;
		}
	}
}