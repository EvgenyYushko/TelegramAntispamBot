using System;

namespace DomainLayer.Models.Authorization
{
	public class ExternalLoginEntity
	{
		public Guid Id { get; set; }

		public Guid UserId { get; set; }

		public string Provider { get; set; }

		public string ProviderKey { get; set; }

		public DateTime DateAdd { get; set; }

		public UserEntity User { get; set; }
	}
}