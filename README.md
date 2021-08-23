# Machine Learning at SSOP
Dieses Repository beinhaltet alle Dokumentationen, Präsentationen und Arbeitsdateien für ein durchgeführtes Forschungs- und Entwicklungsseminar an der HTW Dresden. Das Thema der zwei Semester war das Testen von Machine Learning innerhalb einer Simulation einer sich selbst organisierenden Produktion zur Verbesserung der Produktionsperformance.

## Links zu anderen Projekten
- Link zum Repository der SSOP: https://github.com/Krockema/ng-erp-4.0
- Branch der gemachten Implementierungen: https://github.com/MaxWeickert/ng-erp-4.0/tree/feat/ai-prediction-product-MS/
- Dokumentation des Machine-Learning Projektes: https://github.com/MaxWeickert/ng-erp-4.0/tree/feat/ai-prediction-product-MS/Master40.MachineLearning
- Link zum ML API Container: https://github.com/mschwrdtnr/tensorflow-rest-prediction

## Erster Start
**Nötige Programme installieren**
- Visual Studio Installer
- Microsoft Visual Studio Community Edition mit den folgenden Paketen
  - ASP.NET and web development
  - .NET desktop development
  - .NET 5.0 Runtime
  - .NET Core 2.1 Runtime
  - .NET Core 3.1 Runtime
  - .NET Framework 4.0 bis .NET Framework 4.8
  - .NET SDK
  - GitHub extension for Visual Studio
  - NuGet package manager
  - C# and Visual Basic
  - MSBuild
- Microsoft SQL Server Management Studio

**Projekt klonen**
```
git clone https://github.com/MaxWeickert/ng-erp-4.0.git
```

**Dependencies herunterladen und Projekt erstellen**
- Master40 --> Rechtsklick `libman.json` --> "Restore Client-Side Libraries"
- "Build Solution" auf oberster Ebene des Projektes klicken

**Simulationsumgebung erstellen**
- Test Explorer öffnen
- Datenbank zurücksetzen oder erstellen: Rechtsklick auf  Test "ResetResultsDB" --> Run 
- Produktion erstellen: Rechtsklick auf Test "SetInput" --> Run

**Simulationsparameter anpassen und Simulation starten**
- In der Test-Datei: "SimulationTemplate.cs" können die Simulationsparameter wie gewünscht angepasst werden. Hier kann ebenfalls der Algorithmus für die Vorhersage gewählt werden.
- Simulation starten: Rechtsklick auf Test _SimulationTemplate_ --> Run

**Kennzahlen in Datenbank anschauen**
- MS SQL Server Management Studio öffnen
- Auf lokale Datenbank verbinden: `(localdb)\mssqllocaldb`
- Tabellen anschauen

## Trainingsdaten generieren
1. Anzahl von approaches in der Simulationsschleife in _SimulationTemplate_ wählen
2. Kennzahlen, die für die Trainingsdaten nicht benötigt werden kann ein `[IGNORE]` in `SimulationKpisReshaped.cs` vorangestellt werden
3. Simulation starten: Rechtsklick auf Test _SimulationTemplate_ --> Run
4. Die Kennzahlen werden unter folgendem Verzeichnis abgelegt: `Master40.XUnitTest\GeneratedData\`

> Es sollte darauf geachtet werden, dass immer ein anderer Seed verwendet wird, da die Kennzahlen sonst gleich werden.
 
> Ein sinnvoller guter Wert für die ThroughPutTime beim Simulationsstart kann wie folgt berechnet werden: `Durchschnittliche Durchlaufzeit der Produkte * minDeliveryTime` 

> Wenn Trainingsdaten für für neue Produkte erstellt werden sollen, dann kann in `SetInput` die Anzahl der Produkte bestimmt werden. Auch hier sollte ein neuer Seed verwendet werden

## Testen einer guten Ankunftsrate
1. arrivalRate in _SimulationTemplate_ setzen
2. Simulation starten: Rechtsklick auf Test _SimulationTemplate_ --> Run
3. Workload der Maschinen in SQL Datenbank mit dem letzten SQL-Snippet abfragen
4. Wenn Workload zu hoch ist, dann sollte arrivalRate verringert werden (BSP: Wenn Workload einer Maschine bei 90%, dann arrivalRate um 10% verringern)
5. Alle Schritte wiederholen bis keine Maschine einen Workload von 0.85 anzeigt


## Aktuelle Probleme und Fehler
> Bezieht sich auf den verwendeten Branch und kann bereits im master der ssop gefixt sein.

- Die Variablen "TotalWork" und "TotalSetup" werden falsch berechnet. Eigentlich sollten sie Werte zwischen 0 und 1 haben.
- Das Originalprojekt wurde mittlerweile von `Master40` zu `MATE` umgenannt. Diese Doku sollte wenn möglich bei einem merge auch angepasst werden.
- ...
