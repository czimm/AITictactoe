# AITicTacToe
JSON Tictactoe Game for AI Testing

Check Game State

Request: http://localhost:8000/getgamestate

Response: {"PlayerTurn":2,"PlayerWin":0,"CurrentMoves":[{"x":2,"y":3,"player":1}]}

Play a Game Move

Request: http://localhost:8000/gamemove

Request Body: {"x":3,"y":3,"player":2}
