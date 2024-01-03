using Microsoft.AspNetCore.Mvc;
using WebProject.Data;
using WebProject.ViewModels;
using WebProject.Helpers;
namespace WebProject.Controllers
{
    public class CartController : Controller
    {
        private readonly Hshop2023Context db;

        public CartController(Hshop2023Context context) { db = context; }
        public IActionResult Index()
        {
            return View(Cart);


        }
        public List<CartVM> Cart => HttpContext.Session.Get<List<CartVM>>(MySetting.CART_KEY) ?? new List<CartVM>();
        public IActionResult AddToCart(int id, int quantity = 1)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHh == id);
            if (item == null)
            {
                var hangHoa = db.HangHoas.SingleOrDefault(p => p.MaHh == id);
                if(hangHoa == null)
                {
                    TempData["Message"] = $"No Id {id}";
                    return Redirect("/404");
                }
                item = new CartVM
                {
                    MaHh = hangHoa.MaHh,
                    TenHh = hangHoa.TenHh,
                    DonGia = hangHoa.DonGia ?? 0,
                    Hinh = hangHoa.Hinh ?? string.Empty,
                    SoLuong = quantity


                };
                gioHang.Add(item);
            }
            else
            {
                item.SoLuong += quantity;
            }
            HttpContext.Session.Set(MySetting.CART_KEY, gioHang);

            return RedirectToAction("Index");
        }
        public IActionResult RemoveToCart(int id, int quantity = 1)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHh == id);
            if (item == null)
            {
                var hangHoa = db.HangHoas.SingleOrDefault(p => p.MaHh == id);
                if (hangHoa == null)
                {
                    TempData["Message"] = $"No Id {id}";
                    return Redirect("/404");
                }
                item = new CartVM
                {
                  
                    SoLuong = quantity


                };
                gioHang.Add(item);
            }
            else
            {
                item.SoLuong -= quantity;
            }
            HttpContext.Session.Set(MySetting.CART_KEY, gioHang);

            return RedirectToAction("Index");
        }

        
        public IActionResult RemoveCart(int id)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHh == id);
            if (item != null)
            {
                gioHang.Remove(item);
                HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
            }

            return RedirectToAction("Index");
        }

    }
}
