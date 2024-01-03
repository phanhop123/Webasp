using AutoMapper;
using WebProject.ViewModels;
using WebProject.Data;
namespace WebProject.Helpers
{
	public class AutoMapperProfile : Profile
	{
		public AutoMapperProfile() 
		{
			CreateMap<RegisterVM, KhachHang>();
		}
	}
}
