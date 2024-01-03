using Microsoft.AspNetCore.Mvc;
using WebProject.Data;
using WebProject.ViewModels;

namespace WebProject.Viewcomponents
{
    public class MenuLoaiViewcomponent : ViewComponent
    {
        private readonly Hshop2023Context db;

        public MenuLoaiViewcomponent(Hshop2023Context context) => db = context;

        public IViewComponentResult Invoke()
        {
            var data = db.Loais.Select(lo => new MenuLoai {
                MaLoai = lo.MaLoai,
                TenLoai = lo.TenLoai,
                SoLuong = lo.HangHoas.Count
            });
            return View(data);
        }
    }
}
