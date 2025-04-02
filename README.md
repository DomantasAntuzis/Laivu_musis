# Laivų Mūšis

## Projekto Paleidimas
1. Atsisiųskite projekto failus
2. Atidarykite komandinę eilutę (Command Prompt)
3. Naviguokite į projekto katalogą:
   ```
   cd C:\...\Laivu_musis
   ```
4. Sukompiliuokite projektą:
   ```
   dotnet build
   ```
5. Paleiskite žaidimą:
   ```
   dotnet run
   ```

## Konfigūracijos Failai

### Laivų Išdėstymas
Kiekvienas žaidėjas turi sukurti savo konfigūracijos failą:
- Pirmasis žaidėjas: `player1_cfg.txt`
- Antrasis žaidėjas: `player2_cfg.txt`

Konfigūracijos failo formatas:
```
A1 5 H
B3 4 V
C5 3 H
...
```
Kur:
- Pirmas skaičius - koordinatės (pvz., A1)
- Antras skaičius - laivo dydis (2-5)
- Trečias simbolis - orientacija (H - horizontaliai, V - vertikaliai)

### Taisyklių Konfigūracija
Žaidimo taisyklės konfigūruojamos `game_rules_config.txt` faile:
```
Board size: 10
Ships:
5: 1
4: 2
3: 3
2: 5
Bombs:
Radar: 2
Explosive: 2
```

Galite keisti:
- Lentos dydį (nuo 8 iki 20)
- Laivų kiekį
- Specialių bombų kiekį

## Žaidimo Taisyklės

### Pradžia
1. Žaidime dalyvauja 2 žaidėjai
2. Kiekvienas žaidėjas turi savo lentą (dydis nustatomas game_rules_config.txt faile)
3. Žaidėjai turi išdėstyti savo laivus pagal taisykles:
   - 1 laivas (5 langeliai)
   - 2 laivai (4 langeliai)
   - 7 laivai (3 langeliai)
   - 5 laivai (2 langeliai)

### Laivų Išdėstymo Taisyklės
- Laivai negali liestis (turi būti bent vieno langelio atstumas)
- Laivai negali išeiti už lentos ribų
- Laivai gali būti išdėstyti horizontaliai arba vertikaliai

### Specialios Bombos
Kiekvienas žaidėjas turi:
- 2 "Radaro" bombas
- 2 "Sprogstamąsias" bombas

#### Radaro Bomba
- Atskleidžia 3x3 plotą
- Nežymi pataikymų ar nepataikymų
- Po naudojimo eilė pereina kitam žaidėjui

#### Sprogstamoji Bomba
- Sunaikina 3x3 plotą
- Žymi pataikymus (X) ir nepataikymus (O)
- Jei pataiko į laivą, žaidėjas gauna dar vieną ėjimą
- Jei nepataiko, eilė pereina kitam žaidėjui

### Žaidimo Eiga
1. Žaidėjai paeiliui atlieka ėjimus
2. Ėjimas atliekamas įvedant koordinates (pvz., "A1", "B5")
3. Speciali bomba naudojama įvedant:
   - "RADAR A1" - radaro bombai
   - "EXPLOSIVE B5" - sprogstamajai bombai

### Žaidimo Rezultatai
- Pataikymas žymimas "X"
- Nepataikymas žymimas "O"
- Sunaikintas laivas žymimas "X" visuose jo langeliuose
- Žaidimas baigiasi, kai vieno žaidėjo visi laivai sunaikinti

### Failų Struktūra
Žaidimas sukuria šiuos failus:
- `player1_board.txt` - Pirmojo žaidėjo lenta
- `player2_board.txt` - Antrojo žaidėjo lenta
- `game_log.txt` - Žaidimo istorija
- `game_state.txt` - Žaidimo būsena

### Žaidimo Pabaiga
Žaidimas baigiasi, kai:
- Vieno žaidėjo visi laivai sunaikinti
- Laimėtojas paskelbiamas konsolėje
- Galutinė žaidimo būsena išsaugoma failuose

## Simbolių Reikšmės
- `.` - tuščias langelis
- `X` - pataikymas į laivą
- `O` - nepataikymas
- Skaičiai (2-5) - laivų dydžiai