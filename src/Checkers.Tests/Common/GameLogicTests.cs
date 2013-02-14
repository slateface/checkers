using System;
using System.Linq;

using Checkers.Common;

using NUnit.Framework;

namespace Checkers.Tests.Common
{
	[TestFixture]
	public class GameLogicTests
	{
		[Test]
		[TestCase(5, 2, 5, 3, Result = false, Description = "F4 is not a valid square")]
		[TestCase(5, 2, 4, 2, Result = false, Description = "E3 is not a valid destination")]
		[TestCase(4, 2, 5, 3, Result = false, Description = "E3 is not a valid source")]
		[TestCase(5, 2, 4, 4, Result = false, Description = "E3 is not a valid destination")]
		[TestCase(7, 2, 8, 3, Result = false, Description = "I is not a valid column")]
		[TestCase(5, 2, 7, 4, Result = false, Description = "F3 to D5 is too far")]
		public bool Assert_that_invalid_first_moves_return_false(int fromX, int fromY, int toX, int toY)
		{
			var gameLogic = new GameLogic();

			var move = new Move
				           {
					           Player = Player.PlayerOne,
					           From = new Square {X = fromX, Y = fromY},
					           To = new Square {X = toX, Y = toY},
				           };

			return gameLogic.SendMove(move);
		}

		[Test]
		public void Assert_that_it_is_player_ones_turn()
		{
			var gameLogic = new GameLogic();

			var player = Player.PlayerTwo;

			var move = new Move
				           {
					           Player = player,
					           From = new Square {X = 5, Y = 2},
					           To = new Square {X = 4, Y = 3},
				           };

			var result = gameLogic.SendMove(move);

			Assert.That(result, Is.False, "At the start of a game, player two may not make the first move.");
		}

		[Test]
		public void Assert_that_player_one_goes_first_but_not_second()
		{
			var gameLogic = new GameLogic();

			var move = new Move
				           {
					           Player = Player.PlayerOne,
					           From = new Square(5,2),
					           To = new Square(4,3),
				           };

			var result = gameLogic.SendMove(move);
			Assert.That(result, Is.True, "At the start of a game, player one must make the first move.");

			result = gameLogic.SendMove(move);
			Assert.That(result, Is.False, "At the start of a game, player one may not make the second move.");
		}

		[Test]
		public void Assert_that_player_two_goes_second()
		{
			var gameLogic = new GameLogic();

			var p1Move = new Move
				           {
					           Player = Player.PlayerOne,
					           From = new Square {X = 5, Y = 2},
					           To = new Square {X = 4, Y = 3},
				           };

			var result = gameLogic.SendMove(p1Move);
			Assert.That(result, Is.True, "At the start of a game, player one must make the first move.");

			var p2Move = new Move
				             {
					             Player = Player.PlayerTwo,
					             From = new Square(2, 5),
					             To = new Square(3, 4),
				             };

			result = gameLogic.SendMove(p2Move);
			Assert.That(result, Is.True, "At the start of a game, player two must make the second move.");
		}

		[Test]
		public void Assert_there_are_only_8_valid_opening_moves()
		{
			var expectedValidMoves = new[]
				                         {
					                         "1, 2, 0, 3",
					                         "1, 2, 2, 3",
					                         "3, 2, 2, 3",
					                         "3, 2, 4, 3",
					                         "5, 2, 4, 3",
					                         "5, 2, 6, 3",
					                         "7, 2, 6, 3",
				                         };

			for (var fromX = 0; fromX < 8; fromX++)
			{
				for (var fromY = 0; fromY < 8; fromY++)
				{
					for (var toX = 0; toX < 8; toX++)
					{
						for (var toY = 0; toY < 8; toY++)
						{
							var game = new GameLogic();
							var result = game.SendMove(new Move
								                           {
									                           Player = Player.PlayerOne,
									                           From = new Square {X = fromX, Y = fromY},
									                           To = new Square {X = toX, Y = toY},
								                           });
							var actual = string.Format("{0}, {1}, {2}, {3}", fromX, fromY, toX, toY);
							var isExpected = expectedValidMoves.Contains(actual);

							Assert.That(result, Is.EqualTo(isExpected), actual);
						}
					}
				}
			}
		}

		[Test]
		public void Assert_that_checker_is_moved_on_the_board()
		{
			var gameLogic = new GameLogic();

			var move = new Move
			{
				Player = Player.PlayerOne,
				From = new Square(5, 2),
				To = new Square(4, 3),
			};

			var checkerPreMove = gameLogic.Board[5][2];
			Assume.That(checkerPreMove, Is.Not.Null, "Before the move, the from square is empty");

			var result = gameLogic.SendMove(move);
			Assert.That(result, Is.True, "Simple first move failed");

			var checkerPostMove = gameLogic.Board[4][3];
			Assert.That(checkerPostMove, Is.Not.Null, "After the move, the destination square remains empty");
			Assert.That(checkerPostMove, Is.SameAs(checkerPreMove), "After the move, the checker in the new square is not the same checker");
		}

		[Test]
		public void Assert_that_jumping_last_checker_results_in_game_over()
		{
			var p1Checker = new Checker(Player.PlayerOne);
			var move = new Move
				           {
					           Player = Player.PlayerOne,
				           };

			var gameLogic = new GameLogic {Board = GetEmptyBoard()};
			gameLogic.Board[2][5] = p1Checker;

			var isGameOver = gameLogic.IsGameOver(move);
			Assert.That(isGameOver, Is.True);
		}

		[Test]
		public void Assert_that_logjam_forces_game_over()
		{
			var p1Checker = new Checker(Player.PlayerOne);
			var p2Checker1 = new Checker(Player.PlayerTwo);
			var p2Checker2 = new Checker(Player.PlayerTwo);

			var move = new Move
				           {
					           Player = Player.PlayerTwo,
				           };

			var gameLogic = new GameLogic {Board = GetEmptyBoard()};
			gameLogic.Board[0][7] = p1Checker;
			gameLogic.Board[1][6] = p2Checker1;
			gameLogic.Board[2][5] = p2Checker2;

			var isGameOver = gameLogic.IsGameOver(move);
			Assert.That(isGameOver, Is.True);
		}

		[Test]
		public void Assert_that_unkinged_p1_checker_cannot_jump_backwards()
		{
			var p1Checker = new Checker(Player.PlayerOne);
			var p2Checker = new Checker(Player.PlayerTwo);

			var move = new Move
			{
				Player = Player.PlayerOne,
				From = new Square(3, 2),
				To = new Square(2, 1),
			};

			var gameLogic = new GameLogic { Board = GetEmptyBoard() };
			gameLogic.Board[3][2] = p1Checker;
			gameLogic.Board[0][7] = p2Checker;

			var validation = gameLogic.IsValidMove(move);
			Assert.That(validation.IsValid, Is.False, "Cannot jump backwards!");
		}

		[Test]
		public void Assert_that_unkinged_p2_checker_cannot_jump_backwards()
		{
			var p1Checker = new Checker(Player.PlayerOne);
			var p2Checker = new Checker(Player.PlayerTwo);

			var move = new Move
			{
				Player = Player.PlayerTwo,
				From = new Square(2, 5),
				To = new Square(3, 6),
			};

			var gameLogic = new GameLogic { Board = GetEmptyBoard() };
			gameLogic.Board[2][5] = p1Checker;
			gameLogic.Board[7][0] = p2Checker;

			var validation = gameLogic.IsValidMove(move);
			Assert.That(validation.IsValid, Is.False, "Cannot jump backwards!");
		}

		[Test]
		public void Assert_that_kinged_p1_checker_can_jump_backwards()
		{
			var p1Checker = new Checker(Player.PlayerOne) { IsKinged = true, };
			var p2Checker = new Checker(Player.PlayerTwo);

			var move = new Move
			{
				Player = Player.PlayerOne,
				From = new Square(3, 2),
				To = new Square(2, 1),
			};

			var gameLogic = new GameLogic { Board = GetEmptyBoard() };
			gameLogic.Board[3][2] = p1Checker;
			gameLogic.Board[0][7] = p2Checker;

			var validation = gameLogic.IsValidMove(move);
			Assert.That(validation.IsValid, Is.True, "Kings can jump backwards!");
		}

		[Test]
		public void Assert_that_kinged_p2_checker_can_jump_backwards()
		{
			var p1Checker = new Checker(Player.PlayerOne);
			var p2Checker = new Checker(Player.PlayerTwo) { IsKinged = true, };

			var move = new Move
			{
				Player = Player.PlayerTwo,
				From = new Square(2, 5),
				To = new Square(3, 6),
			};

			var gameLogic = new GameLogic { Board = GetEmptyBoard() };
			gameLogic.Board[7][0] = p1Checker;
			gameLogic.Board[2][5] = p2Checker;

			var validation = gameLogic.IsValidMove(move);
			Assert.That(validation.IsValid, Is.True, "Kings can jump backwards!");
		}

		[Test]
		public void Assert_that_p1_can_jump_p2_baseline_test()
		{
			var p1Checker = new Checker(Player.PlayerOne);
			var p2Checker = new Checker(Player.PlayerTwo);

			var move = new Move
			{
				Player = Player.PlayerOne,
				From = new Square(5, 2),
				To = new Square(3, 4),
			};

			var gameLogic = new GameLogic { Board = GetEmptyBoard() };
			gameLogic.Board[5][2] = p1Checker;
			gameLogic.Board[4][3] = p2Checker;

			var validation = gameLogic.IsValidMove(move);
			Assert.That(validation.IsValid, Is.True, "P1 is jumping P2");
		}

		[Test]
		public void Assert_that_p2_can_jump_p1_baseline_test()
		{
			var p1Checker = new Checker(Player.PlayerOne);
			var p2Checker = new Checker(Player.PlayerTwo);

			var move = new Move
			{
				Player = Player.PlayerTwo,
				From = new Square(3, 4),
				To = new Square(5, 2),
			};

			var gameLogic = new GameLogic { Board = GetEmptyBoard() };
			gameLogic.Board[4][3] = p1Checker;
			gameLogic.Board[3][4] = p2Checker;

			var validation = gameLogic.IsValidMove(move);
			Assert.That(validation.IsValid, Is.True, "P2 is jumping P1");
		}

		[Test]
		public void Assert_that_p1_king_can_jump_p2_baseline_test()
		{
			var p1Checker = new Checker(Player.PlayerOne) { IsKinged = true,};
			var p2Checker = new Checker(Player.PlayerTwo);

			var move = new Move
			{
				Player = Player.PlayerOne,
				From = new Square(3, 4),
				To = new Square(5, 2),
			};

			var gameLogic = new GameLogic { Board = GetEmptyBoard() };
			gameLogic.Board[4][3] = p2Checker;
			gameLogic.Board[3][4] = p1Checker;

			var validation = gameLogic.IsValidMove(move);
			Assert.That(validation.IsValid, Is.True, "P1 is jumping P2");
		}

		[Test]
		public void Assert_that_p2_king_can_jump_p1_baseline_test()
		{
			var p1Checker = new Checker(Player.PlayerOne);
			var p2Checker = new Checker(Player.PlayerTwo) { IsKinged = true, };

			var move = new Move
			{
				Player = Player.PlayerTwo,
				From = new Square(5, 2),
				To = new Square(3, 4),
			};

			var gameLogic = new GameLogic { Board = GetEmptyBoard() };
			gameLogic.Board[5][2] = p2Checker;
			gameLogic.Board[4][3] = p1Checker;

			var validation = gameLogic.IsValidMove(move);
			Assert.That(validation.IsValid, Is.True, "P2 is jumping P1");
		}

		[Test]
		public void Assert_that_jump_removes_checker()
		{
			var p1Checker = new Checker(Player.PlayerOne);
			var p2Checker = new Checker(Player.PlayerTwo);

			var move = new Move
			{
				Player = Player.PlayerOne,
				From = new Square(5, 2),
				To = new Square(3, 4),
			};

			var gameLogic = new GameLogic { Board = GetEmptyBoard() };
			gameLogic.Board[5][2] = p1Checker;
			gameLogic.Board[4][3] = p2Checker;

			var isSuccessful = gameLogic.SendMove(move);
			Assert.That(isSuccessful, Is.True, "P1 is jumping P2");

			var oldP2Checker = gameLogic.Board[4][3];
			Assert.That(oldP2Checker, Is.Null, "Player Two's checker was not removed.");
		}

		[Test]
		public void Assert_that_a_checker_cannot_jump_empty_square()
		{
			var p1Checker = new Checker(Player.PlayerOne);
			var p2Checker = new Checker(Player.PlayerTwo);

			var move = new Move
			{
				Player = Player.PlayerOne,
				From = new Square(5, 2),
				To = new Square(3, 4),
			};

			var gameLogic = new GameLogic { Board = GetEmptyBoard() };
			gameLogic.Board[5][2] = p1Checker;
			gameLogic.Board[0][7] = p2Checker;

			var validation = gameLogic.IsValidMove(move);
			Assert.That(validation.IsValid, Is.False, "P1 attempted to jump over an empty square");
		}

		[Test]
		public void Assert_that_a_player_cannot_jump_self()
		{
			var p1Checker1 = new Checker(Player.PlayerOne);
			var p1Checker2 = new Checker(Player.PlayerOne);
			var p2Checker = new Checker(Player.PlayerTwo);

			var move = new Move
			{
				Player = Player.PlayerOne,
				From = new Square(5, 2),
				To = new Square(3, 4),
			};

			var gameLogic = new GameLogic { Board = GetEmptyBoard() };
			gameLogic.Board[5][2] = p1Checker1;
			gameLogic.Board[4][3] = p1Checker2;
			gameLogic.Board[0][7] = p2Checker;

			var validation = gameLogic.IsValidMove(move);
			Assert.That(validation.IsValid, Is.False, "P1 attempted to jump over himself");
		}

		[Test]
		public void Assert_that_checker_cannot_jump_2_squares_at_once()
		{
			var p1Checker = new Checker(Player.PlayerOne);
			var p2Checker = new Checker(Player.PlayerTwo);

			var move = new Move
			{
				Player = Player.PlayerOne,
				From = new Square(5, 2),
				To = new Square(2, 5),
			};

			var gameLogic = new GameLogic { Board = GetEmptyBoard() };
			gameLogic.Board[5][2] = p1Checker;
			gameLogic.Board[4][3] = p2Checker;

			var validation = gameLogic.IsValidMove(move);
			Assert.That(validation.IsValid, Is.False, "P1 is jumping too far");
		}

		[Test]
		public void Assert_that_p1_is_kinged_upon_reaching_bottom_row()
		{
			var p1Checker = new Checker(Player.PlayerOne);
			var p2Checker = new Checker(Player.PlayerTwo);

			var move = new Move
			{
				Player = Player.PlayerOne,
				From = new Square(1, 6),
				To = new Square(0, 7),
			};

			var gameLogic = new GameLogic { Board = GetEmptyBoard() };
			gameLogic.Board[1][6] = p1Checker;
			gameLogic.Board[3][2] = p2Checker;

			var success = gameLogic.SendMove(move);
			Assert.That(success, Is.True, "Send move succeeded");
			Assert.That(p1Checker.IsKinged, Is.True, "YOU DIDN'T KING ME!");
		}

		[Test]
		public void Assert_that_p2_is_kinged_upon_reaching_top_row()
		{
			var p1Checker = new Checker(Player.PlayerOne);
			var p2Checker = new Checker(Player.PlayerTwo);

			var move = new Move
			{
				Player = Player.PlayerTwo,
				From = new Square(6, 1),
				To = new Square(7, 0),
			};

			var gameLogic = new GameLogic { Board = GetEmptyBoard(), IsPlayerOnesTurn = false,};
			gameLogic.Board[3][2] = p1Checker;
			gameLogic.Board[6][1] = p2Checker;

			var success = gameLogic.SendMove(move);
			Assert.That(success, Is.True, "Send move succeeded");
			Assert.That(p2Checker.IsKinged, Is.True, "YOU DIDN'T KING ME!");
		}

		[Test]
		public void Assert_that_a_player_must_jump_when_possible()
		{
			// If a player attempts to simply move when a jump is possible, then the move is not valid
			var p1Checker = new Checker(Player.PlayerOne);
			var p2Checker = new Checker(Player.PlayerTwo);

			var move = new Move
			{
				Player = Player.PlayerOne,
				From = new Square(5, 2),
				To = new Square(4, 3),
			};

			var gameLogic = new GameLogic { Board = GetEmptyBoard() };
			gameLogic.Board[5][2] = p1Checker;
			gameLogic.Board[3][4] = p2Checker;

			var isSuccessful = gameLogic.SendMove(move);
			Assert.That(isSuccessful, Is.True, "P1 is allowed to commit suicide");

			var boardState = gameLogic.GetBoardState();
			Assert.That(boardState.IsPlayerOnesTurn, Is.False, "Should be P2's turn after P1's suicide");
		}

		[Test]
		public void Assert_that_a_player_must_jump_when_another_checker_is_able_to_jump()
		{
			// If a player attempts to simply move when another checker has a jump, then the move is not valid
			var p1Checker = new Checker(Player.PlayerOne);
			var p1Jumper = new Checker(Player.PlayerOne);
			var p2Checker = new Checker(Player.PlayerTwo);

			var move = new Move
			{
				Player = Player.PlayerOne,
				From = new Square(7, 2),
				To = new Square(6, 3),
			};

			var gameLogic = new GameLogic { Board = GetEmptyBoard() };
			gameLogic.Board[7][2] = p1Checker;
			gameLogic.Board[5][2] = p1Jumper;
			gameLogic.Board[4][3] = p2Checker;

			var isSuccessful = gameLogic.SendMove(move);
			Assert.That(isSuccessful, Is.False, "P1 cannot move a checker when another cheker has a jump available");
		}
		
		[Test]
		public void Assert_that_when_a_double_jump_is_possible_that_the_players_turn_does_not_end()
		{
			// If a player attempts to simply move when another checker has a jump, then the move is not valid
			var p1Checker = new Checker(Player.PlayerOne);
			var p2Jumpee1 = new Checker(Player.PlayerTwo);
			var p2Jumpee2 = new Checker(Player.PlayerTwo);
			var p2LastMove = new Checker(Player.PlayerTwo);

			var move = new Move
			{
				Player = Player.PlayerOne,
				From = new Square(5, 2),
				To = new Square(3, 4),
			};

			var gameLogic = new GameLogic { Board = GetEmptyBoard() };
			gameLogic.Board[5][2] = p1Checker;
			gameLogic.Board[4][3] = p2Jumpee1;
			gameLogic.Board[4][5] = p2Jumpee2;
			gameLogic.Board[0][1] = p2LastMove;

			var isSuccessful = gameLogic.SendMove(move);
			Assert.That(isSuccessful, Is.True, "P1 has a double jump opportunity");

			var boardState = gameLogic.GetBoardState();
			Assert.That(boardState.IsPlayerOnesTurn, Is.True, "It should still be P1's turn");

			//Make double jump
			move = new Move
			{
				Player = Player.PlayerOne,
				From = new Square(3, 4),
				To = new Square(5, 6),
			};
			isSuccessful = gameLogic.SendMove(move);
			Assert.That(isSuccessful, Is.True, "P1 should have been alowed to make a double jump");

			boardState = gameLogic.GetBoardState();
			Assert.That(boardState.IsPlayerOnesTurn, Is.False, "It should be P2's turn after the double jump");
		}

		[Test]
		public void Assert_that_suicidal_move_yields_turn_to_other_player_to_make_jump()
		{
			// If a player attempts to simply move when a jump is possible, then the move is not valid
			var p1Checker = new Checker(Player.PlayerOne);
			var p2Checker = new Checker(Player.PlayerTwo);

			var move = new Move
			{
				Player = Player.PlayerOne,
				From = new Square(5, 2),
				To = new Square(6, 3),
			};

			var gameLogic = new GameLogic { Board = GetEmptyBoard() };
			gameLogic.Board[5][2] = p1Checker;
			gameLogic.Board[4][3] = p2Checker;

			var isSuccessful = gameLogic.SendMove(move);
			Assert.That(isSuccessful, Is.False, "P1 cannot avoid a jump when available");

		}

		private static Checker[][] GetEmptyBoard()
		{
			return new Checker[8][].Select(row => new Checker[8]).ToArray();
		}
	}
}