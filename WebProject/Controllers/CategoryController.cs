using Microsoft.AspNetCore.Mvc;
using WebProject.Data;

namespace WebProject.Controllers
{
	public class CategoryController : Controller
	{

        private readonly Hshop2023Context db;
        public CategoryController(Hshop2023Context context) { db = context; }
        public ActionResult Index()
		{
			var items = db.Loais;
			return View(items);
		}
		public ActionResult Add()
		{
			return View();
		}
		
	}
}
