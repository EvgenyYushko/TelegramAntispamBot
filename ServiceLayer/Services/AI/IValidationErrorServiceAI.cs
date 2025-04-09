using System.Threading.Tasks;

namespace ServiceLayer.Services.AI
{
	public interface IValidationErrorServiceAI
	{
		Task<string> ExplainInvalidCronExpression(string invalidCroneExpression);
	}
}
