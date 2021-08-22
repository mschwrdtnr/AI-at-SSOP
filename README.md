# AI-at-SSOP
Dieses Repository beinhaltet alle Dokumentationen, Präsentationen und Arbeitsdateien für ein durchgeführtes Forschungs- und Entwicklungsseminar an der HTW Dresden. Das Thema der zwei Semester war das Testen von Machine Learning innerhalb einer Simulation einer sich selbst organisierenden Produktion zur Verbesserung der Produktionsperformance.

## Links zu anderen Projekten
- Link zum Repository der SSOP: https://github.com/Krockema/ng-erp-4.0
- Branch der gemachten Implementierungen: https://github.com/MaxWeickert/ng-erp-4.0/tree/feat/ai-prediction-product-MS/
- Dokumentation des Machine-Learning Projektes: https://github.com/MaxWeickert/ng-erp-4.0/tree/feat/ai-prediction-product-MS/Master40.MachineLearning
- Link zum ML API Container: https://github.com/mschwrdtnr/tensorflow-rest-prediction

## Gut zu Wissen über das Projekt und die SSOP
### Erster Start
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
- In der Test-Datei: "SimulationTemplate.cs" können die Simulationsparameter wie gewünscht angepasst werden.
- Simulation starten: Rechtsklick auf Test SimulationTemplate --> Run

**Kennzahlen in Datenbank anschauen**
- MS SQL Server Management Studio öffnen
- Auf lokale Datenbank verbinden: `(localdb)\mssqllocaldb`
- Tabellen anschauen

### Anderes