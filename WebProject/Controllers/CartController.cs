using Microsoft.AspNetCore.Mvc;
using WebProject.Data;
using WebProject.ViewModels;
using WebProject.Helpers;
using Microsoft.AspNetCore.Authorization;
using ECommerceMVC.Services;
namespace WebProject.Controllers
{
    public class CartController : Controller
    {
		private readonly PaypalClient _paypalClient;
		private readonly Hshop2023Context db;
		private readonly IVnPayService _vnPayservice;
		public CartController(Hshop2023Context context, PaypalClient paypalClient , IVnPayService vnPayservice) 
		{
			_paypalClient = paypalClient;
			db = context;
			_vnPayservice = vnPayservice;
		}
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
		[Authorize]
		[HttpGet]
		public IActionResult Checkout()
		{
			if (Cart.Count == 0)
			{
				return Redirect("/");
			}

			ViewBag.PaypalClientdId = _paypalClient.ClientId;
			return View(Cart);
		}

		[Authorize]
		[HttpPost]
		public IActionResult Checkout(CheckoutVM model, string payment = "COD")
		{
			if (ModelState.IsValid)
			{
				if (payment == "Thanh toán VNPay")
				{
					var vnPayModel = new VnPaymentRequestModel
					{
						Amount = Cart.Sum(p => p.ThanhTien),
						CreatedDate = DateTime.Now,
						Description = $"{model.HoTen} {model.DienThoai}",
						FullName = model.HoTen,
						OrderId = new Random().Next(1000, 100000)
					};
					return Redirect(_vnPayservice.CreatePaymentUrl(HttpContext, vnPayModel));
				}
				var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID).Value;
				var khachHang = new KhachHang();
				if (model.GiongKhachHang)
				{
					khachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == customerId);
				}

				var hoadon = new HoaDon
				{
					MaKh = customerId,
					HoTen = model.HoTen ?? khachHang.HoTen,
					DiaChi = model.DiaChi ?? khachHang.DiaChi,
					Sdt = model.DienThoai ?? khachHang.DienThoai,
					NgayDat = DateTime.Now,
					CachThanhToan = "COD",
					CachVanChuyen = "GRAB",
					MaTrangThai = 0,
					GhiChu = model.GhiChu
				};

				db.Database.BeginTransaction();
				try
				{
					db.Database.CommitTransaction();
					db.Add(hoadon);
					db.SaveChanges();

					var cthds = new List<ChiTietHd>();
					foreach (var item in Cart)
					{
						cthds.Add(new ChiTietHd
						{
							MaHd = hoadon.MaHd,
							SoLuong = item.SoLuong,
							DonGia = item.DonGia,
							MaHh = item.MaHh,
							GiamGia = 0
						});
					}
					db.AddRange(cthds);
					db.SaveChanges();

					HttpContext.Session.Set<List<CartVM>>(MySetting.CART_KEY, new List<CartVM>());

					return View("Success");
				}
				catch
				{
					db.Database.RollbackTransaction();
				}
			}

			return View(Cart);
		}
		public IActionResult PaymentSuccess()
		{
			return View("Success");
		}


		#region Paypal payment
		[Authorize]
		[HttpPost("/Cart/create-paypal-order")]
		public async Task<IActionResult> CreatePaypalOrder(CancellationToken cancellationToken)
		{
			// Thông tin đơn hàng gửi qua Paypal
			var tongTien = Cart.Sum(p => p.ThanhTien).ToString();
			var donViTienTe = "USD";
			var maDonHangThamChieu = "DH" + DateTime.Now.Ticks.ToString();

			try
			{
				var response = await _paypalClient.CreateOrder(tongTien, donViTienTe, maDonHangThamChieu);

				return Ok(response);
			}
			catch (Exception ex)
			{
				var error = new { ex.GetBaseException().Message };
				return BadRequest(error);
			}
		}

		[Authorize]
		[HttpPost("/Cart/capture-paypal-order")]
		public async Task<IActionResult> CapturePaypalOrder(string orderID, CancellationToken cancellationToken, CheckoutVM model)
		{
			
				if (ModelState.IsValid)
				{
					var response = await _paypalClient.CaptureOrder(orderID);
					var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID).Value;
					var khachHang = new KhachHang();
					if (model.GiongKhachHang)
					{
						khachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == customerId);
					}

					var hoadon = new HoaDon
					{
						MaKh = customerId,
						HoTen = model.HoTen ?? khachHang.HoTen,
						DiaChi = model.DiaChi ?? khachHang.DiaChi ??  "Default Address" ,
						Sdt = model.DienThoai ?? khachHang.DienThoai,
						NgayDat = DateTime.Now,
						CachThanhToan = "Paypal",
						CachVanChuyen = "GRAB",
						MaTrangThai = 0,
						GhiChu = model.GhiChu
					};

					db.Database.BeginTransaction();
					try
					{
						db.Database.CommitTransaction();
						db.Add(hoadon);
						db.SaveChanges();

						var cthds = new List<ChiTietHd>();
						foreach (var item in Cart)
						{
							cthds.Add(new ChiTietHd
							{
								MaHd = hoadon.MaHd,
								SoLuong = item.SoLuong,
								DonGia = item.DonGia,
								MaHh = item.MaHh,
								GiamGia = 0
							});
						}
						db.AddRange(cthds);
						db.SaveChanges();

						HttpContext.Session.Set<List<CartVM>>(MySetting.CART_KEY, new List<CartVM>());

						return Ok(response);
					}
					catch
					{
						db.Database.RollbackTransaction();
					}
				}
			
			return View(Cart);
		}


		#endregion
		[Authorize]
		public IActionResult PaymentCallBack(CheckoutVM model)
		{
			var response = _vnPayservice.PaymentExecute(Request.Query);

			if (response == null || response.VnPayResponseCode != "00")
			{
				TempData["Message"] = $"Lỗi thanh toán VN Pay: {response.VnPayResponseCode}";
				return RedirectToAction("PaymentFail");
			}

			var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID).Value;
			var hoadon = new HoaDon
			{
				MaKh = customerId,
				HoTen = model.HoTen,
				DiaChi = model.DiaChi ?? "Default Address" ,
				Sdt = model.DienThoai,
				NgayDat = DateTime.Now,
				CachThanhToan = "VNpay",
				CachVanChuyen = "GRAB",
				MaTrangThai = 0,
				GhiChu = model.GhiChu
			};

			db.Database.BeginTransaction();
			try
			{
				db.Database.CommitTransaction();
				db.Add(hoadon);
				db.SaveChanges();

				var cthds = new List<ChiTietHd>();
				foreach (var item in Cart)
				{
					cthds.Add(new ChiTietHd
					{
						MaHd = hoadon.MaHd,
						SoLuong = item.SoLuong,
						DonGia = item.DonGia,
						MaHh = item.MaHh,
						GiamGia = 0
					});
				}
				db.AddRange(cthds);
				db.SaveChanges();

				HttpContext.Session.Set<List<CartVM>>(MySetting.CART_KEY, new List<CartVM>());


				TempData["Message"] = $"Thanh toán VNPay thành công";
				
			}
			catch
			{
				db.Database.RollbackTransaction();
			}
			return RedirectToAction("PaymentSuccess");

		}
	}

}
