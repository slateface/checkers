using System.Web;

using Checkers.Common;

using SignalR.Hubs;

namespace Checkers.Host
{
	public class Game : Hub
	{
		private static GameLogic CurrentGame
		{
			get { return HttpContext.Current.Application.Get("game") as GameLogic; }
			set { HttpContext.Current.Application.Set("game", value); }
		}

		public void SignIn(bool isPlayerOne, string name)
		{
			Context.Cookies.Add(new HttpCookie("Name", name));
			Caller.IsPlayerOne = isPlayerOne;
			Caller.Name = name;
		}

		public bool MustSignIn()
		{
			return Caller.Name == null || Context.Cookies["Name"] == null;
		}

		public void Send(string name, string message)
		{
			// Call the addMessage method on all clients
			Clients.addMessage(name, message);
		}

		public MoveResult Move(string source, string destination)
		{
			if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(destination) || source.Length != 2 || destination.Length != 2)
			{
				return new MoveResult
					       {
						       Message = "Source and Destination must be two digit numbers",
						       BoardState = CurrentGame.GetBoardState(),
					       };
			}

			int fromX, fromY, toX, toY;
			if (!int.TryParse(source.Substring(0, 1), out fromX)
			    || !int.TryParse(source.Substring(1, 1), out fromY)
			    || !int.TryParse(destination.Substring(0, 1), out toX)
			    || !int.TryParse(destination.Substring(1, 1), out toY))
			{
				return new MoveResult
					       {
						       Message = "Source and Destination must be two digit numbers",
						       BoardState = CurrentGame.GetBoardState(),
					       };
			}

			var currentGame = CurrentGame;

			var move = new Move
				           {
					           Player = Caller.IsPlayerOne ? Player.PlayerOne : Player.PlayerTwo,
					           From = new Square(fromX, fromY),
					           To = new Square(toX, toY),
				           };

			var moveResult = currentGame.MakeMove(move);
			moveResult.Message = moveResult.IsSuccessful ? string.Format("{0} played {1}-{2}", Caller.Name, source, destination) : "You cannot make that move.";

			Clients.playHappened(moveResult);
			return moveResult;
		}

		public BoardState NewGame()
		{
			CurrentGame = new GameLogic();
			return CurrentGame.GetBoardState();
		}

		public BoardState GetBoardState()
		{
			var gameLogic = CurrentGame;
			return gameLogic != null ? gameLogic.GetBoardState() : null;
		}
	}
}