# game_of_life_unity
Meine Implementation von [Conway's Game of Life](https://de.wikipedia.org/wiki/Conways_Spiel_des_Lebens#Die_Spielregeln) in Unity mit 


### Start mit zufälligem Rauschen:
![](https://github.com/FDoerr/game_of_life_unity/blob/main/docs/start_from_noise.gif)


### Platzierung von gespeicherter Zellen-Konfiguration:
![](https://github.com/FDoerr/game_of_life_unity/blob/main/docs/place_configurations.gif)


## Notizen:
Es ist einige Zeit her dass ich dieses Programm geschrieben habe.

Sachen die mir beim drüberschauen aufgefallen sind:

- Zu viel Funktionalität in der MainLoop Klasse
     
- "GetNumberOfAliveNeighbours" & "GetNumberOfAliveNeighboursWrapAround"
   - enthalten viele Code Wiederholungen
   - sollten in eine eigene Klasse
     
- Methoden um Zellen-Konfiguration zu speichern sollten in eigene Klasse
   - Dies würde auch einfach ermöglichen mehrere Konfigurationen zu speichern und zwischen ihnen zu wechseln
 
- UI-Funktionalität sollte in eigene Klasse
 

## Build
Dieses Repository dient nur als Showcase und ist nicht buildable.

Ein Windows Build ist verfügbar unter [Releases](https://github.com/FDoerr/game_of_life_unity/releases).
