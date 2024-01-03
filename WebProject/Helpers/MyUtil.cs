using System.Text;

namespace WebProject.Helpers
{
	public class MyUtil
	{
		public static string UploadHinh(IFormFile Hinh, string folder)
		{
			try
			{
				var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Hinh", folder , Hinh.FileName);
				using (var myfile = new FileStream(fullPath, FileMode.CreateNew))
				{
					Hinh.CopyTo(myfile);
				}
				return Hinh.FileName;
			} catch (Exception ex)
			{
				return string.Empty;
			}
		}
		public static string GenerateRamdomKey(int  length = 5 ) 
		{
			var patter = @"qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM!@#$%^&*";
			var sb = new StringBuilder();
			var rd = new Random();
			for ( int i = 0; i < length; i++ )
			{
				sb.Append(patter[rd.Next(0, patter.Length)]);
			}
			return sb.ToString();
		}
	}
}
