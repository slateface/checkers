using System;
using System.Collections.Generic;
using System.Linq;

namespace Checkers.Common
{
	public class GameLogic
	{
		public GameLogic()
		{
			StartGame();
		}

		public bool IsPlayerOnesTurn { get; set; }

		public Checker[][] Board { get; set; }

		public void StartGame()
		{
			Board = new[]
				        {
					        new Checker[8],
					        new Checker[8],
					        new Checker[8],
					        new Checker[8],
					        new Checker[8],
					        new Checker[8],
					        new Checker[8],
					        new Checker[8],
				        };

			for (var y = 0; y < 3; y++)
			{
				for (var x = 0; x < 8; x++)
				{
					if (new Square {X = x, Y = y}.IsValidSquare())
					{
						Board[x][y] = new Checker(Player.PlayerOne);
					}
				}
			}

			for (var y = 5; y < 8; y++)
			{
				for (var x = 0; x < 8; x++)
				{
					if (new Square {X = x, Y = y}.IsValidSquare())
					{
						Board[x][y] = new Checker(Player.PlayerTwo);
					}
				}
			}

			IsPlayerOnesTurn = true;
		}

		public BoardState GetBoardState()
		{
			var boardState = Board.Select(row => row.Select(checker =>
				                                                {
					                                                if (checker == null) return "X";
					                                                var code = checker.Player == Player.PlayerOne ? "b" : "r";
					                                                return checker.IsKinged ? code.ToUpperInvariant() : code;
				                                                }).ToArray()).ToArray();
			return new BoardState
				       {
					       IsPlayerOnesTurn = IsPlayerOnesTurn,
					       Squares = boardState,
				       };
		}

		public MoveResult MakeMove(Move move)
		{
			return new MoveResult
				       {
					       IsSuccessful = SendMove(move),
					       IsGameOver = IsGameOver(move),
					       CanJumpAgain = false,
					       Message = "I love lamp!",
					       BoardState = GetBoardState(),
				       };
		}

		public bool SendMove(Move move)
		{
			//TODO change result from bool to complex state
			//TODO Make computer able to play
			if (!IsPlayersTurn(move.Player)) return false;

			var moveValidation = IsValidMove(move);
			if (!moveValidation.IsValid) return false;

			// Move the checker
			var checker = Board[move.From.X][move.From.Y];
			Board[move.To.X][move.To.Y] = checker;
			Board[move.From.X][move.From.Y] = null;

			if (moveValidation.JumpedSquare != null)
				Board[moveValidation.JumpedSquare.X][moveValidation.JumpedSquare.Y] = null;

			if (!checker.IsKinged)
			{
				if ((move.To.Y == 0 && move.Player == Player.PlayerTwo) ||
				    (move.To.Y == 7 && move.Player == Player.PlayerOne))
					checker.IsKinged = true;
			}

			if (moveValidation.JumpedChecker == null || !PlayerHasJumpAvailable(move.Player))
			{
				IsPlayerOnesTurn = !IsPlayerOnesTurn;
			}

			return true;
		}

		public MoveValidation IsValidMove(Move move)
		{
			var result = new MoveValidation();

			if (!move.From.IsValidSquare() || !move.To.IsValidSquare()) return result;
			if (!PlayerHasCheckerOnSquare(move.Player, move.From)) return result;
			if (!IsSquareEmpty(move.To)) return result;

			// Check if an actual move was sent
			var deltaX = move.From.X - move.To.X;
			var deltaY = move.From.Y - move.To.Y;
			if (deltaX == 0 || deltaY == 0) return result;
			if (Math.Abs(deltaX) != Math.Abs(deltaY)) return result;

			if (Math.Abs(deltaX) == 2)
			{
				// Attempted a jump
				var jumpedSquare = new Square((move.From.X + move.To.X)/2, (move.From.Y + move.To.Y)/2);
				var jumpedChecker = Board[jumpedSquare.X][jumpedSquare.Y];
				if (jumpedChecker == null) return result;
				if (jumpedChecker.Player == move.Player) return result;

				result.JumpedChecker = jumpedChecker;
				result.JumpedSquare = jumpedSquare;
			}
			else if (Math.Abs(deltaX) != 1) return result;
			else if (PlayerHasJumpAvailable(move.Player)) return result;

			var checker = Board[move.From.X][move.From.Y];
			if (!checker.IsKinged)
			{
				if (move.Player == Player.PlayerOne && deltaY > 0) return result;
				if (move.Player == Player.PlayerTwo && deltaY < 0) return result;
			}

			result.IsValid = true;
			return result;
		}

		private bool PlayerHasJumpAvailable(Player player)
		{
			return Board.SelectMany((row, x) => row.Select((checker, y) =>
			                                               new {checker, From = new Square(x, y), Jumps = new Square(x, y).AllJumps()}))
			            .Where(x => x.checker != null && x.checker.Player == player)
			            .SelectMany(x => x.Jumps.Select(j => new Move {Player = x.checker.Player, From = x.From, To = j}))
			            .Any(m => IsValidMove(m).IsValid);
		}

		public bool IsGameOver(Move move)
		{
			var otherPlayerCheckers = Board.Select((row, x) => row.Select((checker, y) => new {checkerWithLocation = checker, Square = new Square(x, y)})
			                                                      .Where(o => o.checkerWithLocation != null && o.checkerWithLocation.Player != move.Player))
			                               .SelectMany(x => x)
			                               .ToArray();

			if (!otherPlayerCheckers.Any())
			{
				return true;
			}

			var hasAtLeastOneValidMove = otherPlayerCheckers.SelectMany(x => new[]
				                                                                 {
					                                                                 x.Square.MoveNW(),
					                                                                 x.Square.MoveNE(),
					                                                                 x.Square.MoveSW(),
					                                                                 x.Square.MoveSE(),
					                                                                 x.Square.JumpNW(),
					                                                                 x.Square.JumpNE(),
					                                                                 x.Square.JumpSW(),
					                                                                 x.Square.JumpSE(),
				                                                                 }
				                                                                 .Select(to => new Move {Player = x.checkerWithLocation.Player, From = x.Square, To = to}))
			                                                .Any(m => IsValidMove(m).IsValid);
			if (!hasAtLeastOneValidMove) return true;

			return false;
		}

		private bool IsPlayersTurn(Player player)
		{
			return (player == Player.PlayerOne) == IsPlayerOnesTurn;
		}

		private bool IsSquareEmpty(Square square)
		{
			return Board[square.X][square.Y] == null;
		}

		private bool PlayerHasCheckerOnSquare(Player player, Square square)
		{
			var checker = Board[square.X][square.Y];
			return checker != null && (player != null ? checker.Player == player : checker.Player != null);
		}
	}

	public class Move
	{
		public Player Player { get; set; }
		public Square From { get; set; }
		public Square To { get; set; }
	}

	public class BoardState
	{
		public bool IsPlayerOnesTurn { get; set; }
		public string[][] Squares { get; set; }
	}

	public class MoveResult
	{
		public bool IsSuccessful { get; set; }
		public bool IsGameOver { get; set; }
		public bool CanJumpAgain { get; set; }
		public string Message { get; set; }
		public BoardState BoardState { get; set; }
	}

	public class MoveValidation
	{
		public bool IsValid { get; set; }
		public Checker JumpedChecker { get; set; }
		public Square JumpedSquare { get; set; }
		public string ValidationMessage { get; set; }
	}

	public class Square
	{
		public Square() {}

		public Square(int x, int y)
		{
			X = x;
			Y = y;
		}

		public int X { get; set; }
		public int Y { get; set; }

		public bool IsValidSquare()
		{
			return ((X%2) == 0) != ((Y%2) == 0)
			       && X <= 7 && X >= 0 && Y <= 7 && Y >= 0;
		}
	}

	public class Player
	{
		public static readonly Player PlayerOne = new Player();
		public static readonly Player PlayerTwo = new Player();
		private Player() {}
	}

	public class Checker
	{
		public Checker(Player player)
		{
			Player = player;
			ID = Guid.NewGuid();
		}

		public Guid ID { get; private set; }
		public Player Player { get; private set; }
		public bool IsKinged { get; set; }
	}

	public static class ExtensionsForSquare
	{
// ReSharper disable InconsistentNaming
		public static Square MoveNW(this Square square)
		{
			return new Square(square.X - 1, square.Y - 1);
		}

		public static Square JumpNW(this Square square)
		{
			return new Square(square.X - 2, square.Y - 2);
		}

		public static Square MoveNE(this Square square)
		{
			return new Square(square.X + 1, square.Y - 1);
		}

		public static Square JumpNE(this Square square)
		{
			return new Square(square.X + 2, square.Y - 2);
		}

		public static Square MoveSW(this Square square)
		{
			return new Square(square.X - 1, square.Y + 1);
		}

		public static Square JumpSW(this Square square)
		{
			return new Square(square.X - 2, square.Y + 2);
		}

		public static Square MoveSE(this Square square)
		{
			return new Square(square.X + 1, square.Y + 1);
		}

		public static Square JumpSE(this Square square)
		{
			return new Square(square.X + 2, square.Y + 2);
		}

		public static IEnumerable<Square> AllJumps(this Square square)
		{
			yield return square.JumpNE();
			yield return square.JumpSE();
			yield return square.JumpSW();
			yield return square.JumpNW();
		}
	}
}