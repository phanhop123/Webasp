using System.ComponentModel.DataAnnotations;

namespace WebProject.ViewModels
{
    public class RegisterVM
    {
        [Display(Name = "Name Login")]
        [Required(ErrorMessage = "*")]
        [MaxLength(20 , ErrorMessage = " 20 ki tu ")]

        public string MaKh { get; set; }
        [Required(ErrorMessage = "*")]
        [Display(Name = "Password Login")]
        public string MatKhau { get; set; }
        [Required(ErrorMessage = "*")]
        [MaxLength(50, ErrorMessage = " 50 ki tu ")]
        public string HoTen { get; set; }

        public bool GioiTinh { get; set; } = true;

        public DateTime? NgaySinh { get; set; }
        [MaxLength(60, ErrorMessage = " 60 ki tu ")]
        public string? DiaChi { get; set; }
        [MaxLength(24, ErrorMessage = " 24 ki tu ")]
        public string DienThoai { get; set; }
        [EmailAddress(ErrorMessage = "chua dung dinh dang email ")]
        public string Email { get; set; } = null!;

        public string? Hinh { get; set; }
    }
}
