# RailwayWars
An engine for running a tournament for bots playing *RailwayWars* game.

# Game rules

The goal of the game is to build a railway connecting cities which performs better than other's players railways.
A game is played in rounds on a hexagonal board. Some cells of the board are occupied by cities. Some cells are
occupied by water and are unavailable to players. Other cells are initially free and can be bought by players
as a part of their railway.

Each free cell has its minimal price. When player wants to buy given cell he makes an offer with the price he is
willing to pay. If multiple players want to buy the same cell an auction takes place and the player who offered to pay
the most wins. If two players offer the same highest price then auction is cancelled and the cell remains free. 

Every turn each player earns some money from cities and makes some profit from every completed connection between two
cities.

The game ends when no free cells are available or no cell was bought for fixed amount of rounds.
Player gets points for:

- 1 point for every owned cell.
- `n * n * n / m` points for every connected pair of cities where `n` is the length of the shortest possible connection and `m` is
the length of player's shortest connection.

Player with most points wins.

# Game play

A tournament is divided into matches. Each match is divided into rounds and is played by all players. On the beginning
of each round every player receives information about the current state of the board. Next, the game server waits fixed
amount of time for buy offers from every player. Player can make multiple offers in one round. When round ends
the server holds all auctions based on buy offers received in that round, updates the state of the board and the next
round begins. Auctions are ordered from the highest to the lowest offer. If player doesn't have declared amount of
money when an auction takes place then it is assumed that he made an offer with the amount of money he has.

# Credits

The game viewer icon made by [Lukas Software](http://www.awicons.com/) downloaded from http://findicons.com/.
