# Cal Poly CSC 570: AI in Games Spring 2022 Project
**Instructor**: Dr. Rodrigo Canaan, Cal Poly Computer Science & Software Engineering Department (rcanaan@calpoly.edu)

**Group members**: Kenny Lau, Kaanan Kharwa, Daniel Tisdale (klau24@calpoly.edu, kkharwa@calpoly.edu, dctisdal@calpoly.edu)

# Overview

The collectible online card game Hearthstone features a rich testbed and poses unique demands for generating artificial intelligence agents. The game is a turn-based card game between two opponents, using constructed decks of thirty cards along with a selected hero with a unique power. Players use their limited mana crystals to cast spells or summon minions to attack their opponent, with the goal to reduce the opponent’s health to zero. In this project, we will explore, compare, and evaluate various popular AI agents used to play the game.

### Project Structure ###

* **SabberStoneCoreAI** *(.NET Core)*

  A test project to run A.I. simulations with predefinied decks and strategys.
		
* **SabberStoneCore** *(.NET Core)*

  Core simulator engine, all the functions needed for the simulator are in here. Check out the Wiki [Link](https://github.com/HearthSim/SabberStone/wiki) for informations about the core and how to use it.

### Usage ###
For the best results, we highly recommend running this repository on a Windows machine. When we tried to setup the SabberStone environment on MacOS machines, we ran into compilation issues that we were unable to solve. Our best guess is that while it is possible to run .NET applications on MacOS, the SabberStone repository is outdated or incompatible with the operating system.

Instructions to setup the repository:
* Clone this repository to your machine
* Open the project with [Visual Studio](https://visualstudio.microsoft.com/) (will not work if opened with Visual Studio Code!). Check and make sure you have Visual Studio Build Tools 2017, 2019 installed
* Go to the following directory: [`HearthstoneAICompetition/core-extensions/SabberStoneBasicAI/src`](https://github.com/klau24/HearthstoneAICompetition/tree/master/core-extensions/SabberStoneBasicAI/src)
	- In this directory there are a few key files and folders. [`Program.cs`](https://github.com/klau24/HearthstoneAICompetition/blob/master/core-extensions/SabberStoneBasicAI/src/Program.cs) is the main file used to run and compete each AI agent against each other. The [`AIAgents`](https://github.com/klau24/HearthstoneAICompetition/tree/master/core-extensions/SabberStoneBasicAI/src/AIAgents) folder contains the implementation for each agent.
* Build the project
* Run the project
	- A terminal window should pop up that looks something like this:

	![image](https://user-images.githubusercontent.com/49251143/172262614-6af0b7fc-2392-4517-9b7d-f166c93d7da1.png)
	- With the current parameters of `games_per_matchup=50` and `nr_of_threads=10`, the program will play around 9,000 games which took ~4 hours on our machines. These parameters can be changed on line `71` and `72` in [`Program.cs`](https://github.com/klau24/HearthstoneAICompetition/blob/master/core-extensions/SabberStoneBasicAI/src/Program.cs)

### Results ###
The results from the report can be recreated using the parameters mentioned above (`games_per_matchup=50` and `nr_of_threads=10`). The agent we created, Weighted 2 Step Look Ahead, competed against the following agents: Random, Greedy, MCTS, and Naive 2 Step Look Ahead. The results are computationally expensive to reproduce, so below is the raw output we recieved after the program ran:

![image](https://user-images.githubusercontent.com/49251143/172273240-bb56dd6b-7672-4914-aee2-e4d1d78d1088.png)

Insert some more detailed data about the results.

# Acknowledgements
This project will have been impossible without the SabberStone Hearthstone simulator engine. Check out the [Github](https://github.com/HearthSim/SabberStone) for more information. This repository is a fork of the HearthstoneAICompetition from [Adockhorn](https://github.com/ADockhorn/HearthstoneAICompetition). Compared to Sabberstone, our repository is a minimal environment with emphasis on the AI components.
