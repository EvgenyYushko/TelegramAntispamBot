using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Enumerations;

namespace Infrastructure.Models
{
	public class UserAccount
	{
		public UserAccount(Guid id, string userName, string passwordHash, string email, List<Role> roles)
		{
			Id = id;
			UserName = userName;
			PasswordHash = passwordHash;
			Email = email;
			Roles = roles;
		}

		public Guid Id { get; private set; }
		public string UserName { get; private set; }
		public string PasswordHash { get; private set; }
		public string Email { get; private set; }
		public List<Role> Roles { get; private set; }

		public static UserAccount Create(string userName, string passwordHash, string email, string role)
		{
			var roleEnumValue = Enum.GetValues<Role>().Where(r => role == r.ToString()).FirstOrDefault();
			return new UserAccount(Guid.NewGuid(), userName, passwordHash, email, new List<Role>() { roleEnumValue });
		}

		public override string ToString()
		{
			return $"Id={Id} UserName={UserName} PasswordHash={PasswordHash} Email={Email}";
		}
	}
}
