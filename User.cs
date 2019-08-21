using System;

namespace AuthService
{
	public class User
	{
		public string Id { get; set; }
		public DateTime PasswordChanged { get; set; }
		public string Login { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Locale { get; set; }
		public string TimeZone { get; set; }
	}
}
