using System.Web.Mvc;

using Checkers.Models;

namespace Checkers.Controllers
{
	public class CheckersController : Controller
	{
		public ActionResult Index()
		{
			return View();
		}

		public ActionResult Player1()
		{
			return View("Play", new GameModel {IsPlayerOne = true});
		}

		public ActionResult Player2()
		{
			return View("Play", new GameModel {IsPlayerOne = false});
		}

		public ActionResult Watch()
		{
			return View();
		}
	}
}