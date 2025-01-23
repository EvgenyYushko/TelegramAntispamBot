namespace DomainLayer.Models.Authorization
{
	public class AuthorizationOptions
	{
		public RolePermissions[] RolePermissions { get; set; } = new RolePermissions[]{};
	}

	public class RolePermissions
	{
		public string Role { get; set; } = string.Empty;
		public string[] Permission { get; set; }
	}
}
