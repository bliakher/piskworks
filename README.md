# piskworks

Multiplayer game of 3D Noughts and Crosses.

## Rules of the game

Game is played in a cube N x N x N. You can choose dimension from 3 to 6.
Players take turns by placing their symbols in the cube-board.
Your goal is to make a straight line of N symbols in any direction (with all types of diagonals included).

## How to play

### Connecting

You will need two players, both connected on the same local network.
One will be the Host player and the other the Join player.

Host player chooses the dimension of the cube-board and he also plays first.
His symbol is a Cross.

Join player will connect to the host player's game. 
To do that you need to enter the IP address of the Host player. 
The IP address must be in IPv6 format.
Join player is playing second and his symbol is a Nought.

### Playing

After successfully connecting with your opponent, the playing screen will be displayed.
In the top part of the screen there is the cube-board sliced by levels,
where the leftmost square is the bottom level of the cube-board. 
Use these to place the symbols.

Under the squares is the 3D visualization of the cube-board. 
You can turn the cube around the Z-axis by dragging left or right on the cube.

If one of the players wins, the game stops and the winning fields are highlighted.
You can start a new game by returning to the menu. 
You will need to choose your role and connect again.



