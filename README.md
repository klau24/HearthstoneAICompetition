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

### License

[![AGPLv3](https://www.gnu.org/graphics/agplv3-88x31.png)](http://choosealicense.com/licenses/agpl-3.0/)

SabberStone is licensed under the terms of the
[Affero GPLv3](https://www.gnu.org/licenses/agpl-3.0.en.html) or any later version.

# Acknowledgements
This project will have been impossible without the SabberStone Hearthstone simulator engine. Check out the [Github](https://github.com/HearthSim/SabberStone) for more information. This repository is a fork of the HearthstoneAICompetition from [Adockhorn](https://github.com/ADockhorn/HearthstoneAICompetition). Compared to Sabberstone, our repository is a minimal environment with emphasis on the AI components.
