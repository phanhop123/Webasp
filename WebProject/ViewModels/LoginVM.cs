using System.ComponentModel.DataAnnotations;

namespace WebProject.ViewModels
{
	public class LoginVM
	{
		[Display(Name ="UserName")]
		[Required(ErrorMessage ="*")]
		[MaxLength(20)]
		public string UserName { get; set; }

		[Display(Name = "Password")]
		[Required(ErrorMessage = "*")]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		

	}
}
