var checkers = {
	game: null,
	columns: ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'H'],
	rows: ['1', '2', '3', '4', '5', '6', '7', '8'],
	player: null,
	isDragging: false,

	resetBoard: function () {
		// Set black checkers on rows 1-3
		$('.blackTile[id$="1"],.blackTile[id$="2"],.blackTile[id$="3"]').
			html('<div class="blackChecker">&nbsp;</div>');
		// Set red checkers on rows 6-8
		$('.blackTile[id$="6"],.blackTile[id$="7"],.blackTile[id$="8"]').
			html('<div class="redChecker">&nbsp;</div>');
	},
	updateBoard: function (boardState) {
		for (var i = 0; i < boardState.Squares.length; i++) {
			var column = boardState.Squares[i];
			for (var j = 0; j < column.length; j++) {
				var checker = column[j];
				var html = null;

				switch(checker) {
					case 'b':
						html = '<div class="blackChecker">&nbsp;</div>';
						break;
					case 'B':
						html = '<div class="blackKing">&nbsp;</div>';
						break;
					case 'r':
						html = '<div class="redChecker">&nbsp;</div>';
						break;
					case 'R':
						html = '<div class="redKing">&nbsp;</div>';
						break;
				}
				
				$('.blackTile[id="' + checkers.columns[i] + checkers.rows[j] + '"]').html(html);
			}
		}
		
		$('#source').val(null);
		$('#destination').val(null);

		var checkerDivs = $('.blackChecker, .redChecker');

		checkerDivs
			.mousedown(function() {
				var checkerDiv = $(this);
				var td = checkerDiv.parent();
				var from = td.attr('data-coord');
				$('#source').val(from);

				$(window).mousemove(function() {
					isDragging = true;
					$(window).unbind("mousemove");
				});
			})
			.mouseup(function() {
				var wasDragging = isDragging;
				isDragging = false;
				$(window).unbind("mousemove");
				if (!wasDragging)
					return;
				
				var checkerDiv = $(this);

				// Offset relative to document
				var offset = checkerDiv.offset();
				var center = { x: offset.left + (checkerDiv.width() / 2), y: offset.top + (checkerDiv.height() / 2) };

				var destination = $('#board td.blackTile').map(function () {
					var tile = $(this);
					var l = tile.offset().left;
					var t = tile.offset().top;
					var r = l + tile.width();
					var b = t + tile.height();

					return (l <= center.x && t <= center.y && r >= center.x && b >= center.y)
						? tile
						: null;
				});

				if (destination != null && destination.length) {
					var to = destination[0].attr('data-coord');
					$('#destination').val(to);
					checkers.sendMove();
				} else {
					checkers.updateBoard(boardState);
				}
			})
			.draggable({ containment: "div#board", scroll: false });
	},
	sendMove: function () {
		$('#playError').html('');
		checkers.game.move($('#source').val(), $('#destination').val());
	},
	init: function () {
		// Proxy created on the fly
		checkers.game = $.connection.game;

		// Declare a function on the chat hub so the server can invoke it
		checkers.game.showPlay = function (source, destination) {
			var checker = $('#' + source).html();
			$('#' + source).html('');
			$('#' + destination).html(checker);
		};

		for (var i = 0; i < checkers.columns.length; i++) {
			var column = checkers.columns[i];
			for (var j = 0; j < checkers.rows.length; j++) {
				var row = checkers.rows[j];

				var cssClass;
				if (i % 2 != j % 2) {
					cssClass = 'blackTile';
				} else {
					cssClass = 'whiteTile';
				}

				$('#' + column + row).addClass(cssClass);
			}
		}
	}
};