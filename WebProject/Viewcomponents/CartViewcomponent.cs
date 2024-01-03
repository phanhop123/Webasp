using Microsoft.AspNetCore.Mvc;
using WebProject.Helpers;
using WebProject.ViewModels;

namespace WebProject.Viewcomponents
{
    public class CartViewcomponent : ViewComponent

    {
        public IViewComponentResult Invoke()
        {
           var cont =  HttpContext.Session.Get<List<CartVM>>(MySetting.CART_KEY) ?? new List<CartVM>();

            return View("CartPanel", new CartMV
            {
                Quantity = cont.Sum(p => p.SoLuong),
                Total = cont.Sum(p => p.ThanhTien)
            });
        }
    }
}
