# piskworks {#mainpage}

Multiplayer game of 3D Noughts and Crosses.

[TOC]

## General

Project is built using MonoGame Framework - Cross-Platform Application.

## Program architecture

### Game

In the MonoGame framework the central element of the application is the `Microsoft.Xna.Framework.Game`.
This class is used to initialize the game and load content. 
To run the game, `Update` and `Draw` methods are called by the framework. 
All of my updates and draws are called cascading from there.
On top of that `piskworks.Game` holds the current `piskworks.GameScreen` which is displayed to the user,
`piskworks.Player` representing the player and `piskworks.GameBoard`.

### User interface

User interacts with the game using a graphical user interface.
Different screens displayed to the user are all descendants of the `piskworks.GameScreen` abstract class.
This class holds a common color palette used in all screens.
Screens also have a link to the `piskworks.GameScreen` so they can manipulate with other elements of the game
(such as `piskworks.Player`) in reaction to user input.

I am partly using the Model-View-Control pattern regarding the user interface.
`piskworks.GameBoard` acts as a Model, it holds all information about the game play.
`piskworks.GameScreen` is a Controller - it takes inputs from the user and makes changes in the model.
But it also manages displaying the screen.
In case of `piskworks.PlayScreen`, 
there is `piskworks.Vizualizer3D` class that acts as the View displaying a 3D visualization of the game board based on the Model.

### Network communication

Game is intended for 2 players on a common network, where one has a role of a server and the other is a client that connects to the server.
This is represented by the `piskworks.HostPlayer` and `piskworks.GuestPlayer`.
Both players have a `piskworks.IComunicator` that manages the communication with the other player.
`piskworks.IComunicator` interface is very simple - it has a way to send a message, look if messages are available and receive them.
This allowed me to use a mock communicator that didn't perform any network connections for testing purposes.


## Implementation

### 3D graphics

When playing the game, a 3D visualization of the game board is displayed. 
`piskworks.Vizualizer3D` is responsible for that. 
The 3D model for the outline of the cube is generated programatically.
Models for cross and nought where drawn in Blender and exported as `.obj` files.
I use `piskworks.Model3DLoader` to load models from the source files.

### Network communication

In the game user chooses to be the host or the guest player. After that he waits to be connected with the other player.
This process is different between host and guest.

`piskworks.HostPlayer` acts as a server. To connect it creates a `TcpListener` and listens on a specified port. 
After establishing connection the created `TcpClient` is passed to `piskworks.ComunicatorTcp` that performs actual communication.
Listener is then closed.

`piskworks.GuestPlayer` acts as a client. To connect, user must enter the IP address of the host in IPv6.
After receiving the address, the guest creates a `TcpClient` and tries to connect to the host repeatedly, until it succeeds.

`piskworks.ComunicatorTcp` implements the `piskworks.IComunicator` interface and it is used by both host and guest players.
`piskworks.ComunicatorTcp` has 2 queues - send queue and receive queue.
`piskworks.IComunicator` methods interact only with these queues - ei. when sending a message it is enqueued in the send queue.
On the background there is a parallel thread running, that monitors the network stream and the queues and performs the network communication.

Because of this the queues of `piskworks.ComunicatorTcp` have to be made thread safe. 
This is achieved in the `piskworks.ThreadSafeQueue`. On enqueuing and dequeuing the queue is locked. 
In dequeuing checking the count and taking the element is done atomically in one lock.

I also had to solve an issue with the `TcpListener`. While waiting to connect, the user has an option to cancel connecting. 
If that happens with the host, it is necessary to stop the listener and cancel the async task of connecting the client.
To do that I made the task cancelable and passed a callback, that would cancel the cancellation token source, 
to the screen displayed while waiting for connection. If the connection is cancelled, the the callback is called.
This cancels the task, which emits an exception. On catching the exception, the listener is closed. 
It is necessary to close the listener, so that the port is freed and the user can trie to connect again.





