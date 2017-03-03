# Tera framework
A set of tools designed in C# that make interoperation with the game's client and servers easy.

* [DataCenterReader](https://github.com/Mirrawrs/Tera/tree/master/DataCenterReader): read information from the data center.
* [EnMasseDataProvider](https://github.com/Mirrawrs/Tera/tree/master/EnMasseDataProvider): communicate with EnMasse services and authenticate.
* [Entities](https://github.com/Mirrawrs/Tera/tree/master/Entities): primitive and complex game-related types.
* [GameClientAnalyzer](https://github.com/Mirrawrs/Tera/tree/master/GameClientAnalyzer): scan the game client for version-dependent data.
* [Interfaces](https://github.com/Mirrawrs/Tera/tree/master/Interfaces): to keep implementations separated from contracts.
* [SampleBot](https://github.com/Mirrawrs/Tera/tree/master/SampleBot): a simple bot working out of the box, good as a learning reference.
* [TeraClient](https://github.com/Mirrawrs/Tera/tree/master/TeraClient): the core elements to connect to a TERA server.

## Why?
There are public projects that can tackle one or more of the tasks above, I didn't find one that did **all of them** cohesively with a consistent and simple code style and ample documentation. The game's protocol isn't overly complicated and I wanted to create a toolset that is easy to use and understand. Also, this solution makes extensive use of the [Lotus](https://github.com/Mirrawrs/Lotus) framework and helped it to develop.