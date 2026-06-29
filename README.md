# Assignment 3 - Mobile development 4
Assignment 3 for the Mobile Development 4 at VFS

## Description
The goal of this assignment was to create a Unity Mobile application that implements a few technical requirements as:

- **UI Framework:** An user interface built with Unity UI elements, with a responsive layout that can scale across different phone aspect ratios.
- **Networking & APIs:** The project integrates external and cloud-based services to dynamically handle user data.
- **Data persistance:** The user data is stored remotely.
- **Performance optimization:** The application is structured to initialize the required services efficiently and maintain stable performance during main user interactions.

With these requirements in main, the application created serves as a game where different users can connect and play one game per day, saving their scores and looking it up in a leaderboard in the app. With each game play, the user can unlock different badges of the characters of the _Rick and Morty_ show.

## Main Features
- Email and password Login and registration
- Google Sign-In support
- User session management
- Automatic profile creation in Firestore
- User profile data storage
- Username, profile photo, and badge management
- Real-time user data updates with Firestore listeners
- Player score submission
- Leaderboard ranking in the Game UI

## Login and Register page
When the application is first run the user needs to Login into the application, this can be done by their credentials (Email and password) or by Google. In case the user doesn't have credentials and doesn't want to do Login via Google there is also an option to register a new profile.

## Player profile
Each player has a profile page in the application in which they can see some of the essential information they have, such as their profile picture, username, and badges collected. The player will also be able to change their username and/or password in case it may be necessary.

## Minigames
Each day there will be a new _Game of the day_ in which all the players can participate trying to get the highest score in between all of them. At the moment there are 3 implemented games:

- **Catch the correct element:** In this game the user controls a basket that will have to move in order to collect the most amount of mangos posible, each mango gives the player one point. As well as fruit, bombs also fall from the top of the screen, and if the player gets 3 bombs, the game will be over and the score will be finalized.
- **Simon says:** The second game is a replica of the common game _Simon says_, in which the player is looking at 4 different color buttons. In each round a sequence will be shown, and the player must repeat it by memory, increasing one button each round, until the player gets one of the sequences incorrectly, then the game will stop.
- **Reaction:** In this game targets will appear in random positions of the screen for a few seconds each time, when they appear the player has to touch them to gain points. Every time one of the targets vanishes, one live will be reducted, and the game will stop when the player reaches 0 lives.

## Leaderboard
With each day, a new minigame will be available for the players, and when a player plays one game their final score will be updated in firestore and shown in the leaderscore in the main screen of the application.

## Settings page
In the settings page, the user can see the authors of the application, as well as logout of the application. As a debbug tool for the moment, and as an easy way to grade the assignment, there is also a button in this section to change the day and the minigame.

## Test account
- Email: diana@mail.com
- Password: Diana123*
  
## GitHub
Link to the GitHub repository [here](https://github.com/Enb4rr/UnityMobileDev_A3.git)

## Authors
   - PG29 Yeison
   - PG29 Diana
   - PG29 Felipe
   - PG29 Julian R
