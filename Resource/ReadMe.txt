Date: 11/25/2019
Author: Bao Chau Pham, Antonio Arceo
Project: Tank Wars game


** Game Discription ***
	This TankWars project is a twist on the classic arcade version of Tank Wars designed by Professor Daniel Kopta of the University of Utah. This game was built using C# and the .NET framework utilizing .Net's Windows Forms.
The project includes a GameControll class which handles all the game logic, a Form class which holds the games visual content along with it's menu bar, a DrawingPanel class where all the visuals are drawn too, and a World class which
holds all the object in the current game world/state. This project also has several classes that represent objects in the game including: tanks, projectiles, powerups, turrets, beams, walls, movement controls, and constant values in the game.
The project uses a server-to-client network connection using a networking library dll created in a previous school project. The server communicates to the client by producing data as plain string text which the client converts to objects of the game 
using JSON deserialization.

*** Getting started ***

If you would like to run this project you can clone this repository and run the projects View.exe executable file. Because this project was created using Microsoft's Visual Studio it will only run on a windows machine.
You would need to 1. Close the repository, 2. Open the TankWars solution, and 3. Run/Build the whole project.


*** Game Breakdown ***

	The application is designed as a typical tankWars arcade shooter game playing from a above view. You are able to specify the IP address you would like to connect to by typing it into the menu bar along with your player name.
You will then load up in the world with a random color assigned to your tank and info about your player being displayed as a health bar above your tank and your players name plus player score below the tank.
Tanks can shoot projectiles on a timed cooldown and special beam attacks only when a randomly generated powerup has been picked up by the player.
Tanks each have 3 health points, getting hit by a projectile will decrease this by one and in turn will change the color and size of the players health bar. Health bars change with the classic green, yellow, and red color scheme.
Projectiles cannot go through walls, but special beam attacks can.
If a player is hit with a special beam attack, then that tanks’ health immediately drops to zero.
Special beam attacks produce rays of color changing energy.
Eliminated tanks produce an explosion animation for a brief amount of time before respawning after a short delay penalty.

	The Help menu can be accessed to find more information on tank controls and to learn a bit about the game.

*** Addition Features Beyond Assignment Specs **

Special beam attacks produce rays of color changing energy created by layering rectangle on top of each other and cycling through colors for the added effect.
The health bar changes color and size representing the players current health points.
If a player dies, a explosion animation plays in game while the player is waiting to respawn.
If there is an error when trying to connect to the server the player is given the option to try to reconnect with the same IP(requirement) or to type in a new IP address and player name (additional feature). 
If a player is in the middle of a game and the server crashes/shuts down and the player decides to try to reconnect, the world is cleared, and a new game will start.



*** Design Decisions ***

Object Data Structures
	We decided that because several objects in the world had unique IDs we would be adding those to a dictionary as the key of that specific object that we deserialize from the server.

Drawing Transformation
	We knew that the transformations required for drawing would be a big challenge to understand so we made sure to looks over the class provided lab-11 project in order to research how it's done.
We originally ran into issues trying to draw the walls that are initially sent form the server, but after some tinkering with the methods we were able to accomplish this.

Mouse Event Handle
	We then ran into troubles with the capturing the mouse movement across the screen. We tried following several examples we found on the Microsoft documentations pages as well as Stackoverflow example
but we could not get our tanks turret to follow in the proper way. We finally realized that we were getting the wrong coordinates from the mouseMove event and after some review we realized what had happened.

Object Creation
	When creating deserializing objects from the server using JSON, we add them to separate object dictionaries and use those dictionaries to iterate through when drawing objects using the OnPaint method.
We found this to be the best way on ensuring that we draw all objects with appropriate color schemes by using the Key in the dictionaries to only draw each object once per frame.

Tank color
	To ensure that we are able to draw up to 8 tanks with a unique color we create a separate dictionary that holds the unique tankID as the key and the value is a counter which is incremented 
as the tanks get added. We then use a modulus operator to assign the index of our colorArray by assigning it to (tankID.value % 8) which will result in the first 8 tank iterating through
one color of the array, once we reach the 9th tank the value for the tankID-colorCounter dictionary will reset back to 0 and the cycle will start again.
This seemed to be the best way to ensure that we do not assign the same color to any tank before reaching the 9th player.

Explosion effect
	With the explosion animations we at first thought about using a GIF animation as this was being used by several groups in class but we found it to be over-complicated.
We had a simple idea of drawing pictures of explosion for a couple frames in order to show a small animation that just dealt with transparent images that we could easly find online.
This ended up working better than we expected and it looks pretty cool.

Movement
	In regards to the movement of the tanks we at first were recording the keyDown presses of the user and once a player would lift up a key we would set the movement to "none". This seemed to 
work well for only the certain occasion that the player would press one key at a time. Once you started to play like a normal person and hold down one key while transitioning to next it seemed
to lack that smoothness of switching directions without pause. After deciding that hard setting the movements directly from the keyDown presses, we explored using an array of movement values 
that would keep track of the most recently pressed direction and assign that to the movement controls. After tinkering with it for some time we managed to have the array delete any key the 
player was not pressing while still sending only the most recent key presses. This allowed for smooth movements in the game.

Beam
	When deciding the way we would represent the beam we thought that a simple design would be better since we were already drawing rectangles in the game anyways. We at first just had beams
that matched the color of the tanks which seemed ok but looked very basic. We landed on the idea of having the beam look like some sort of ultra-light beam with "rainbow energy" concentrated
in the middle of the beam. We were able to achieve this by layering a color iterating rectangle on top of a solid white rectangle which gives a very nice visual effect.

Projectiles
	The projectiles were a implemented with more ease than we thought would require. Originally we had difficulty with the speed at which projectiles were being drawn, they seemed to be
moving a lot faster than we wanted and they would suddenly stop for no apparent reson. After some help from the TAs we were able to see that we were drawing the projectiles just fine
but we were using the original vector that the server was sending to draw. This resulted in the projectiles moving very fast through the game and we could only see it for a second or two.
We then realized that we were not normalizing the projectile vectors before drawing them which made all the difference. 
We were able to sync the color of the projectile with the unique color of each tank and we use the direction and location of the tank turret to mark the origin and direction of the projectile.



 * * * * * * * * * * * * * * * * * * * * *  Server Section (PS9) * * * * * * * * * * * * * * * * * * * * * * * * * * 

 ** Server Discription ** 

The server client is a stand-alone program that implements the physics, logic and server-side operations of the server-to-client network connection. 
The physics engine tracks the movements, locations, creation, and deletion of each of the objects in the game.

** Movement **

Tanks
The server receives control object request form the client and parses string into separate strings that represent each object. These separate strings are then deserialized into control objects 
and are added into a controlMessage dictionary. This dictionary assures that we are updating the control request to each client and we only allow one request to be processed at the time that we
iterate through the dictionary and handle the movement request.
If the tank tries to move into the areas, the movement will be caught in a wallCollison method and will be unable to go any future than the edge of any wall. 

Projectiles
The projectiles are moved by a method that takes the current projectile, out of a dictionary, and uses the projectiles original orientation and location to update the location by the specified 
units in the same vector direction. 
If the projectile tries to move into the areas, the movement will be caught in a wallCollison method and the projectiles died flag will be set to true. The projectile will then be removed from
the world and will no longer be drawn.
 

** Locations **

Tanks
The tanks original locations are randomly decided using a location that ranges inside the world view. This location uses the wallCollision method in order to determine in that tank’s location 
will collide with a wall set in the map. If so, the tank will then get a new random location until it will not interfere with the walls
Projectiles
The projectiles original location is determined by the tank’s location. When the projectile is drawn it uses the direction of the turret and of the orientation to determine the direction that 
its location will be drawn to in future frames.

Walls
The walls location is read from the XML file in the resources folder. There is a reader method that saves the location of each endpoint of the wall. After reading both end points of the wall 
and creates a new vector2D. The reader method then creates a new wall using the newly required vectors and adds them to a dictionary of walls that is iterated through before the server sends 
them to the client. 

Powerups
The PowerUp original locations are randomly decided using a location that ranges inside the world view. This location uses the wallCollision method in order to determine in that PowerUp’s 
location will collide with a wall set in the map, just like the tanks. If so, the PowerUp will then get a new random location until it will not interfere with the walls.

Beams
The beams original location is determined by the tank’s location. When the beam is drawn it uses the direction of the turret and of the orientation to determine the direction and able to be drawn.

** Creation **

Tanks
Tanks are created when a new client is connected to the sever. Each client gets a unique ID and this ID is used as the key in the tanks dictionary. The tanks values are all set to the default 
of each.

Beams
When the client sends a control command with the right-clicked mouse button it is processed by the FirstUpdate method which uses JSON to deserialized the control string and this is then sent
to the client in order for the client to draw it.

Powerups
Powerups are created with a random location that must not intersect with a wall. 
The powerups are also created with a ~30 sec delay that starts to count at the time when the game starts. There is a limit of 2 powerups that can be created at a time.

Projectiles
When the client sends a control command with the left-clicked mouse button it is processed by the FirstUpdate method which uses JSON to deserialized the control string and this is then sent 
to the client in order for the client to draw it.


** Deletion **

Projectiles
When a projectile hits a wall, hits a tank, or it is outside of the bound of the world the projectile is considered to be dead. The projectiles died flag is then set to true and it is removed 
from the dictionary of projectiles in the world.

Beams
A string is sent to the client if the tank/client that request a beam has collected a powerup. This beam is only created in one frame

Tanks
A tank is deleted if it has either died or if the client has disconnected from the server.

Powerups
Powerups are deleted if they are either picked up from a tank or if the time limit has been reached before anyone has picked it up.

*** DataBase **

The server sends information to the DataBase controller class that handles the manipulation of the Games, GamesPlayed, and Players database tables that are held in a SQL database. 
The database controller reads data from each tank when the user of the program types something into the server console. All the current players info is saved into tables using SQL commands. 
This includes the score, accuracy, and name of each tank.

** WebServer **

The web server uses a connection that is set up in the webServerController to handle HTTP request that are send from the browser. The server is able to get data that is stored in the 3 game 
database tables that are mentioned above using SQL commands.
This data is then sent to the WebView class and is drawn into tables using HTML. 
The browser is able to have different tables that represent either the overall games that are saved in the database or the specific games for a player are queried and shown. These are specified
depending on the URL that is typed in the browser.


