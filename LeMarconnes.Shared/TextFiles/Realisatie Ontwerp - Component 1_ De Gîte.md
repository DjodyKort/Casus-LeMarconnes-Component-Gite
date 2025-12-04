#  Realisatie Ontwerp \- Component 1: De Gîte

# Inhoudsopgave

[**1\. Inleiding & Scope	4**](#1.-inleiding-&-scope)

[1.1 Doel	4](#1.1-doel)

[1.2 Relatie tot Master Architectuur	4](#1.2-relatie-tot-master-architectuur)

[1.3 Afbakening \- Sprint 01 (In Scope / Out of Scope)	5](#1.3-afbakening---sprint-01-\(in-scope-/-out-of-scope\))

[**2\. Requirements Analyse	6**](#2.-requirements-analyse)

[2.1 Geselecteerde User Stories & Requirements	6](#2.1-geselecteerde-user-stories-&-requirements)

[2.2 Business Regels	8](#2.2-business-regels)

[**3\. Datamodel Implementatie	9**](#3.-datamodel-implementatie)

[3.1 Entiteiten	9](#3.1-entiteiten)

[3.2 ERD	10](#3.2-erd)

[3.3 Implementatie Hiërarchie (Parent-Child)	10](#3.3-implementatie-hiërarchie-\(parent-child\))

[**4\. Data Dictionary (Sprint 1\)	11**](#4.-data-dictionary-\(sprint-1\))

[4.1 Stamgegevens (Basis)	11](#4.1-stamgegevens-\(basis\))

[4.2 Configuratie & Types (Lookup)	14](#4.2-configuratie-&-types-\(lookup\))

[4.3 Inventaris & Prijzen	16](#4.3-inventaris-&-prijzen)

[4.4 Transacties	18](#4.4-transacties)

[**5\. Technische Logica & SQL Functies	20**](#5.-technische-logica-&-sql-functies)

[5.1 Beschikbaarheids-algoritme (Check Parent \+ Children)	20](#5.1-beschikbaarheids-algoritme-\(check-parent-+-children\))

[5.2 Prijsbepaling (Hoe wordt €200 vs €27,50 opgehaald?)	21](#5.2-prijsbepaling-\(hoe-wordt-€200-vs-€27,50-opgehaald?\))

[**6\. Dataseeding & Teststrategie	22**](#6.-dataseeding-&-teststrategie)

[6.1 Initiële Data	22](#6.1-initiële-data)

[6.2 Test Scenarios	23](#6.2-test-scenarios)

[**Bijlagen	24**](#bijlagen)

# 1\. Inleiding & Scope {#1.-inleiding-&-scope}

## 1.1 Doel {#1.1-doel}

Dit document dient als het **Technisch Ontwerp** en de **Sprint Backlog** voor de eerste realisatiefase (Sprint 1\) van het digitaliseringsproject voor Le Marconnès.  
Het doel is om de specifieke datastructuur en bedrijfslogica te definiëren die nodig is om het complexere verhuurmodel van **De Gîte** te realiseren. Dit document slaat de brug tussen de abstracte bedrijfsarchitectuur en de daadwerkelijke SQL-implementatie die deze week wordt opgeleverd. Het instrueert de ontwikkelaars over welke tabellen, relaties en constraints nu prioriteit hebben.

## 1.2 Relatie tot Master Architectuur {#1.2-relatie-tot-master-architectuur}

Dit ontwerp staat niet op zichzelf, maar is een afgeleide van de overkoepelende projectdocumentatie:

1. **Document 1 (Entiteiten Analyse):** Beschrijft de volledige bedrijfscontext.  
2. **Document 2 (Normalisatie & Architectuur):** Bevat de blauwdruk voor de uiteindelijke, allesomvattende database.

Om de toekomstbestendigheid te garanderen, bouwen we in deze sprint geen "losse Gîte-database", maar implementeren we een **deelverzameling** van de Master Architectuur. 

We gebruiken de generieke tabellen (zoals **VERHUUR\_EENHEID**), maar vullen deze vooralsnog enkel met data en logica voor de Gîte. Hierdoor kunnen in latere sprints de Camping en het Hotel naadloos worden toegevoegd zonder de database te herstructureren.

## 

## 1.3 Afbakening \- Sprint 01 (In Scope / Out of Scope) {#1.3-afbakening---sprint-01-(in-scope-/-out-of-scope)}

Om een werkend Minimum Viable Product (MVP) voor de Gîte op te leveren, hanteren we de volgende strikte scope-indeling.

### In Scope (Wordt nu gerealiseerd)

* **Het Hybride Verhuurmodel:** Ondersteuning voor het boeken van de Gîte als geheel (via Booking.com) én als losse slaapplekken (via Airbnb).

* **Conflictbeheer:** Technische validatie om dubbele boekingen tussen het 'Geheel' en 'Slaapplekken' te voorkomen (Parent-Child logica).

* **Gast Beheer:** Registratie van gastgegevens (voor Factuur/Nachtregister) en Accounts (voor inloggen).

* **Tarieven:** De specifieke prijsstructuur van de Gîte (€ 200,- vs € 27,50) inclusief de logica voor toeristenbelasting (inclusief in de prijs). Hierbij wordt tevens de datum-afhankelijkheid (GeldigVan/Tot) geïmplementeerd om historische en toekomstige prijswijzigingen te ondersteunen.

* **Platformen:** Registratie van de bron van de boeking (Eigen Site, Booking.com, Airbnb).

### Out of Scope (Voor latere sprints)

* **Horeca & Restaurant:** Het bestellen van producten en het "op rekening zetten" van consumpties.

* **Camping-specifieke logica:** Het stapelen van kostencomponenten (zoals elektriciteit, hond-tarieven) en camping-specifieke velden.

* **Hotel-specifieke logica:** Capaciteitschecks voor hotelkamers (2-persoons vs 4-persoons).

* **Betalingsverwerking:** De daadwerkelijke financiële transactie (PSP-koppeling).

# 

# 2\. Requirements Analyse {#2.-requirements-analyse}

In dit hoofdstuk specificeren we de functionele eisen die de basis vormen voor de datastructuur van deze sprint. Uit de volledige backlog zijn de volgende items geselecteerd omdat ze direct betrekking hebben op de verhuur van de Gîte.

## 2.1 Geselecteerde User Stories & Requirements {#2.1-geselecteerde-user-stories-&-requirements}

### Accommodatie & Verhuurmodel

De kern van deze sprint is het faciliteren van de twee unieke boekingsmodellen (Geheel vs. Slaapplek).

| Code | Requirement / User Story | Impact op Sprint 1 (Gîte) |
| :---- | :---- | :---- |
| **G-F-RES-020** | De gast kan het hele appartement (gîte) boeken. | Vereist een boekbare entiteit **VERHUUR\_EENHEID** voor de hoofdeenheid (Parent). |
| **G-F-RES-021** | De gast kan een slaapplaats boeken in de gîte. | Vereist 9 individueel boekbare entiteiten voor de slaapplekken (Children). |
| **G-F-RES-016** | De gast kan kiezen welke accommodatie hij/zij wil (camping, gite, hotel). | De database moet bij beschikbaarheidschecks filteren op Type \= 'Gîte'. |

### Het Boekingsproces

De basisstappen die nodig zijn om een geldige reservering in de database te krijgen.

| Code | Requirement / User Story | Impact op Sprint 1 (Gîte) |
| :---- | :---- | :---- |
| **G-F-RES-001** | De gast kan online een reservering plaatsen. | De basisfunctionaliteit: **INSERT** in de tabel **RESERVERING**. |
| **G-F-RES-006****G-F-RES-007** | De gast kan een begindatum en einddatum kiezen. | Opslag van **StartDatum** en **EindDatum**. Validatie dat einddatum na startdatum ligt. |
| **G-F-RES-008** | De gast kan aangeven met hoeveel personen hij/zij komt. | Relevant om te bepalen of de groep binnen de **capaciteit (9)** past of hoeveel slaapplekken er nodig zijn. |

### Informatie & Financiën

De regels omtrent prijzen en beschikbaarheid die specifiek voor de Gîte gelden.

| Code | Requirement / User Story | Impact op Sprint 1 (Gîte) |
| :---- | :---- | :---- |
| **G-NF-RES-003** | De gast kan voor het boeken de beschikbaarheid zien. | **Complexiteit:** Beschikbaarheid van 'Gîte Geheel' is afhankelijk van de status van de 'Slaapplekken' (Parent-Child logica). |
| **G-NF-RES-004** | De gast kan tijdens het maken van een reservatie de kosten zien. | Ophalen van tarieven uit de TARIEF tabel op basis van het gekozen model (€200 of €27,50). |
| **G-NF-RES-005** | De gast ziet hoeveel toeristentaks hij moet betalen. | Voor de Gîte is de taks **inclusief** in de prijs. De database moet dit ondersteunen via **TaxStatus** \= 'Incl'. |

### Systeem & Accounts

De technische randvoorwaarden voor gebruikersbeheer.

| Code | Requirement / User Story | Impact op Sprint 1 (Gîte) |
| :---- | :---- | :---- |
| **S-NF-RES-001** | Om een reservering te boeken moet een gast een account hebben. | We splitsen **GAST** (persoonsgegevens) en **GEBRUIKER** (login). Zo faciliteren we zowel directe boekers (met account) als externe boekers (zonder account). |
| **S-NF-SYS-003** | Van elke transactie en wijziging moeten logs bijgehouden worden. | Implementatie van de **LOGBOEK** tabel of mechanisme. |

## 

## 2.2 Business Regels {#2.2-business-regels}

Bovenstaande requirements vertalen zich naar de volgende harde bedrijfsregels die in de database moeten worden afgedwongen of ondersteund:

1. **Hiërarchische Beschikbaarheid (Parent-Child):**  
   * Als de **Parent** (Gîte Geheel) geboekt is, zijn **alle Children** (Slaapplekken) onbeschikbaar.  
   * Als één of meerdere **Children** (Slaapplekken) geboekt zijn, is de **Parent** (Gîte Geheel) onbeschikbaar.  
   * Meerdere Children kunnen tegelijkertijd geboekt worden door verschillende gasten (Hostel-model) zolang de totale capaciteit (9) niet overschreden wordt..  
2. **Prijsmodellen (2025):**  
   * **Model A (Geheel):** Vaste eenheidsprijs van **€ 200,-** per nacht.  
   * **Model B (Slaapplek):** Prijs per persoon/bed van **€ 27,50** per nacht.  
   * *Platform Logica:*   
     Model A wordt primair geassocieerd met Booking.com/Eigen Site.  
     Model B met Airbnb. De database moet echter beide prijzen ondersteunen ongeacht het platform.  
3. **Fiscale Status:**  
   * De prijzen voor de Gîte zijn **inclusief toeristenbelasting**. Dit wijkt af van de regels voor Hotel en Camping (die exclusief zijn). De database moet deze status (TaxStatus \= Incl) per tarief kunnen opslaan.

# 

# 3\. Datamodel Implementatie {#3.-datamodel-implementatie}

In dit hoofdstuk presenteren we de technische vertaling van de requirements naar een datamodel. Zoals beschreven in de scope, is dit model een subset van de Master Architectuur, waarbij entiteiten die niet relevant zijn voor de Gîte (zoals Horeca) zijn weggelaten.

## 3.1 Entiteiten {#3.1-entiteiten}

Onderstaande tabel beschrijft de rol van elke entiteit binnen de context van deze sprint.

| Entiteit | Rol in Sprint 1 |
| :---- | :---- |
| **GAST** | Opslag van de NAW-gegevens van de hoofdboeker (vereist voor factuur/nachtregister). |
| **GEBRUIKER** | Verzorgt de inlogtoegang voor gasten (website) en beheerders. Gekoppeld aan GAST. |
| **ACCOMMODATIE\_TYPE** | Bevat in deze fase 2 records: *'Gîte-Geheel'* en *'Gîte-Slaapplek'*. |
| **VERHUUR\_EENHEID** | Bevat de fysieke inventaris: 1x Parent ('De Gîte') en 9x Child ('Slaapplek'). |
| **PLATFORM** | Registreert de bron (Eigen Site, Booking.com, Airbnb) om de juiste prijs/commissie te bepalen. |
| **TARIEF\_CATEGORIE** | Bevat categorieën voor prijsopbouw, zoals *'Logies'* en *'Toeristenbelasting'*. |
| **TARIEF** | Bevat de actuele prijzen voor 2025 (€ 200,- en € 27,50), gekoppeld aan Type en Platform. |
| **RESERVERING** | De centrale koppeling tussen Gast, Eenheid en Periode. |
| **RESERVERING\_DETAIL** | Opslag van specifieke aantallen (personen/kinderen) voor de belastingberekening. |
| **LOGBOEK** | Audit trail die alle mutaties (aanmaken, wijzigen, annuleren) vastlegt (S-NF-SYS-003). |

## 3.2 ERD {#3.2-erd}

Onderstaand diagram (afbeelding 1\) toont de relaties tussen de actieve entiteiten. De Horeca- en Product-tabellen uit het master-ontwerp zijn hierin verborgen om de focus op de Gîte-realisatie te leggen.

Afbeelding 1  
![][image1]

## 3.3 Implementatie Hiërarchie (Parent-Child) {#3.3-implementatie-hiërarchie-(parent-child)}

Om de Requirements **G-F-RES-020** (Hele Gîte) en **G-F-RES-021** (Slaapplek) gelijktijdig te ondersteunen zonder dubbele boekingen, wordt de tabel **VERHUUR\_EENHEID** recursief gebruikt.  
Dit wordt gerealiseerd door de kolom **ParentEenheidID**.

**Voorbeeldvulling van de database:**

| EenheidID | Naam | Type | Capaciteit | ParentEenheidID |
| :---- | :---- | :---- | :---- | :---- |
| **1** | **Gîte Le Marconnès** | Gîte-Geheel | **9** | **NULL** |
| 2 | Slaapplek 1 (Kamer 1A) | Gîte-Slaapplek | 1 | 1 |
| 3 | Slaapplek 2 (Kamer 1B) | Gîte-Slaapplek | 1 | 1 |
| ... | ... | ... | ... | ... |
| 10 | Slaapplek 9 (Kamer 4B) | Gîte-Slaapplek | 1 | 1 |

**Werking:**   
Een boeking op ID 1 blokkeert logisch gezien alle kinderen (ID 2-10). Een boeking op ID 2 blokkeert de ouder (ID 1), maar laat ID 3-10 vrij. Deze logica wordt in de applicatielaag (SQL Stored Procedure of Backend Code) afgehandeld.

# 

# 4\. Data Dictionary (Sprint 1\) {#4.-data-dictionary-(sprint-1)}

## 4.1 Stamgegevens (Basis) {#4.1-stamgegevens-(basis)}

### **Entiteit \- GAST**

Bevat de stamgegevens van de gasten. Deze entiteit is losgekoppeld van het gebruikersaccount om ook gasten zonder account (bijv. via Booking.com) te kunnen registreren.

| Attribuut | Beschrijving | Gegevenstype | Sleutel | Aanvullende Regels |
| :---- | :---- | :---- | :---- | :---- |
| **GastID** | Unieke identificatie voor elke gast. | INT | PK | IDENTITY(1,1) |
| **Naam** | Volledige naam van de gast. | VARCHAR(100) |  | NOT NULL |
| **Email** | Het e-mailadres van de gast (gebruikt voor communicatie). | VARCHAR(150) |  | NOT NULL |
| **Tel** | Telefoonnummer van de gast. | VARCHAR(20) |  |  |
| **Straat** | Straatnaam van het factuuradres. | VARCHAR(100) |  | NOT NULL (Factuurvereiste) |
| **Huisnr** | Huisnummer en toevoeging. | VARCHAR(20) |  | NOT NULL(Factuurvereiste) |
| **Postcode** | Postcode van het factuuradres. | VARCHAR(20) |  | NOT NULL(Factuurvereiste) |
| **Plaats** | Woonplaats van de gast. | VARCHAR(100) |  | NOT NULL(Factuurvereiste) |
| **Land** | Land van herkomst (relevant voor Tax/Nachtregister). | VARCHAR(50) |  | NOT NULL (Default: 'Nederland') |
| **IBAN** | Bankrekeningnummer (optioneel, voor restitutie). | VARCHAR(34) |  | NULLABLE |

### 

### **Entiteit \- GEBRUIKER**

Bevat inloggegevens en rollen voor personen die toegang hebben tot het systeem (Gasten, Personeel, Eigenaar).

| Attribuut | Beschrijving | Gegevenstype | Sleutel | Aanvullende Regels |
| :---- | :---- | :---- | :---- | :---- |
| **GebruikerID** | Unieke identificatie voor het account. | INT | PK | IDENTITY(1,1) |
| **GastID** | Koppeling naar de persoonsgegevens in de tabel GAST. | INT | FK, UK | UNIQUE (1:1 of 1:0 relatie) |
| **Email** | Het e-mailadres dat als gebruikersnaam dient. | VARCHAR(150) |  | NOT NULL, UNIQUE |
| **WachtwoordHash** | Versleutelde versie van het wachtwoord. | NVARCHAR(255) |  | NOT NULL |
| **Rol** | De rechten van de gebruiker (bijv. 'Gast', 'Medewerker', 'Eigenaar'). | VARCHAR(20) |  | DEFAULT 'Gast' |

### 

### **Entiteit \- LOGBOEK**

| Attribuut | Beschrijving | Gegevenstype | Sleutel | Aanvullende Regels |
| :---- | :---- | :---- | :---- | :---- |
| **LogID** | Unieke identificatie van de logregel. | INT | PK | IDENTITY(1,1) |
| **GebruikerID** | Wie heeft de actie uitgevoerd? | INT | FK | NULLABLE (Systeemacties kunnen NULL zijn) |
| **Tijdstip** | Exacte datum en tijd van de actie. | DATETIME |  | DEFAULT GETDATE() |
| **Actie** | Type actie (bv. 'INSERT', 'UPDATE\_PRIJS', 'LOGIN'). | VARCHAR(50) |  | NOT NULL |
| **TabelNaam** | Op welke entiteit vond de actie plaats? | VARCHAR(50) |  |  |
| **RecordID** | Het ID van het aangepaste record (bv. het ReserveringID). | INT |  |  |
| **OudeWaarde** | De waarde vòòr de wijziging (voor herstel). | VARCHAR(MAX) |  | NULLABLE |
| **NieuweWaarde** | De waarde na de wijziging. | VARCHAR(MAX) |  | NULLABLE |

## 

## 4.2 Configuratie & Types (Lookup) {#4.2-configuratie-&-types-(lookup)}

### **Entiteit \- ACCOMMODATIE\_TYPE**

Lookup-tabel voor de soorten accommodaties (Hotel, Gîte, Camping).

| Attribuut | Beschrijving | Gegevenstype | Sleutel | Aanvullende Regels |
| :---- | :---- | :---- | :---- | :---- |
| **TypeID** | Unieke identificatie voor het type. | INT | PK | IDENTITY(1,1) |
| **Naam** | Naam van het type (bv. 'Hotelkamer', 'Standplaats', 'Appartement'). | VARCHAR(50) |  | NOT NULL, UNIQUE |

### **Entiteit \- PLATFORM**

Bevat de externe en interne kanalen waarlangs geboekt kan worden, inclusief standaard commissiepercentages

| Attribuut | Beschrijving | Gegevenstype | Sleutel | Aanvullende Regels |
| :---- | :---- | :---- | :---- | :---- |
| **PlatformID** | Unieke identificatie voor het platform. | INT | PK | IDENTITY(1,1) |
| **Naam** | Naam van het platform (bv. 'Booking.com', 'Airbnb', 'Eigen Website'). | VARCHAR(100) |  | NOT NULL |
| **CommissiePercentage** | Het standaard percentage dat het platform inhoudt. | DECIMAL(5,2) |  | DEFAULT 0.00 |

### **Entiteit \- TARIEF\_CATEGORIE**

Tarief categorie wat eigenlijk op basis van de tarieven waarmee Le Marconnès mee werkt en waarmee gerekend wordt.

| Attribuut | Beschrijving | Gegevenstype | Sleutel | Aanvullende Regels |
| :---- | :---- | :---- | :---- | :---- |
| **CategorieID** | Unieke identificatie voor de tarief categorie | INT | PK | IDENTITY(1,1) |
| **Naam** | Naam van de categorie.(bv. ‘Volwassene’, ‘Kind’, ‘Hond’, ‘Elektra’, ‘Toeristenbelasting’) | VARCHAR(100) |  | NOT NULL |

### 

## 

## 4.3 Inventaris & Prijzen {#4.3-inventaris-&-prijzen}

### **Entiteit \- VERHUUR\_EENHEID**

Bevat de fysieke objecten die verhuurd worden. Ondersteunt een hiërarchische structuur voor de Gîte (gehele woning vs. losse slaapplekken).

| Attribuut | Beschrijving | Gegevenstype | Sleutel | Aanvullende Regels |
| :---- | :---- | :---- | :---- | :---- |
| **EenheidID** | Unieke identificatie voor de verhuureenheid. | INT | PK | IDENTITY(1,1) |
| **Naam** | De naam of het nummer van de eenheid (bv. "Kamer 1", "Gîte Totaal"). | VARCHAR(100) |  | NOT NULL |
| **TypeID** | Verwijzing naar het type accommodatie. | INT | FK |  |
| **MaxCapaciteit** | Het maximaal aantal personen voor deze eenheid. | INT |  | CHECK \> 0 |
| **ParentEenheidID** | Verwijzing naar een hoofdeenheid indien dit een sub-eenheid is (Recursief). | INT | FK | NULLABLE |

### 

### **Entiteit \- TARIEF**

De prijzentabel. Hierin worden prijzen vastgelegd per combinatie van accommodatietype, categorie en platform, eventueel tijdsgebonden (seizoenen).

| Attribuut | Beschrijving | Gegevenstype | Sleutel | Aanvullende Regels |
| :---- | :---- | :---- | :---- | :---- |
| **TariefID** | Unieke identificatie voor het tarief. | INT | PK | IDENTITY(1,1) |
| **TypeID** | Accommodatietype waarvoor dit geldt. | INT | FK |  |
| **CategorieID** | Categorie waarvoor dit geldt (bv. 'Logies', 'Hond'). | INT | FK |  |
| **PlatformID** | Specifiek platformtarief (optioneel). | INT | FK | NULLABLE |
| **Prijs** | Het basisbedrag. | MONEY |  | NOT NULL |
| **TaxStatus** | Geeft aan of prijs incl. of excl. toeristenbelasting is. | BIT |  | 0 \= Excl, 1 \= Incl |
| **TaxTarief** | Het bedrag aan toeristenbelasting indien van toepassing. | MONEY |  | DEFAULT 0 |
| **GeldigVan** | Startdatum geldigheid tarief. | DATE |  |  |
| **GeldigTot** | Einddatum geldigheid tarief. | DATE |  | NULL \= Onbeperkt |

### 

## 4.4 Transacties {#4.4-transacties}

### **Entiteit \- RESERVERING**

De kopgegevens van een boeking. Koppelt een gast aan een eenheid voor een bepaalde periode.

| Attribuut | Beschrijving | Gegevenstype | Sleutel | Aanvullende Regels |
| :---- | :---- | :---- | :---- | :---- |
| **ReserveringID** | Unieke identificatie voor de reservering. | INT | PK | IDENTITY(1,1) |
| **GastID** | De gast die de boeking heeft geplaatst. | INT | FK |  |
| **EenheidID** | De specifieke eenheid die is geboekt. | INT | FK |  |
| **PlatformID** | Het kanaal waarlangs de boeking binnenkwam. | INT | FK |  |
| **Startdatum** | De datum van aankomst. | DATE |  | NOT NULL |
| **Einddatum** | De datum van vertrek. | DATE |  | CHECK (Einddatum \> Startdatum) |
| **Status** | Status van de boeking (bv. 'Gereserveerd', 'Ingecheckt', 'Geannuleerd'). | NVARCHAR(20) |  | DEFAULT 'Gereserveerd' |

### 

### **Entiteit \- RESERVERING\_DETAIL**

Specificatie van de kostenposten binnen een reservering (logies, personen, huisdieren, elektra). Dit maakt de prijsopbouw flexibel per type verblijf

| Attribuut | Beschrijving | Gegevenstype | Sleutel | Aanvullende Regels |
| :---- | :---- | :---- | :---- | :---- |
| **DetailID** | Unieke identificatie voor de detailregel. | INT | PK | IDENTITY(1,1) |
| **ReserveringID** | De bijbehorende reservering. | INT | FK |  |
| **CategorieID** | Het type kostenpost (bv. 'Volwassene', 'Hond'). | INT | FK |  |
| **Aantal** | De hoeveelheid (aantal personen/nachten/items). | INT |  | DEFAULT 1 |
| **PrijsOpMoment** | De prijs per eenheid die gold tijdens het boeken (historische vastlegging). | MONEY |  | NOT NULL |

# 

# 5\. Technische Logica & SQL Functies {#5.-technische-logica-&-sql-functies}

In dit hoofdstuk beschrijven we de logica die nodig is om de datastructuur correct te gebruiken. Omdat de database-constraints (Foreign Keys) niet voorkomen dat een 'Parent' en een 'Child' tegelijk geboekt worden, moet deze validatie in de applicatielaag of via Stored Procedures worden afgedwongen.

## 5.1 Beschikbaarheids-algoritme (Check Parent \+ Children) {#5.1-beschikbaarheids-algoritme-(check-parent-+-children)}

Het systeem moet bij elke boekingspoging controleren of de gekozen periode vrij is. Vanwege de hiërarchische structuur van de Gîte zijn er twee verschillende algoritmes nodig.

**Scenario A: Gast wil 'Hele Gîte' (Parent) boeken**  
**Doel:** Voorkomen dat de Gîte geboekt wordt als er ook maar één los slaapplekje bezet is (via Airbnb).

* **Input:** GewensteStartDatum, GewensteEindDatum, GiteParentID (bijv. 1).  
* **Logica:** Er is een conflict (beschikbaarheid \= FALSE) als:  
  * Er een reservering bestaat voor EenheidID \= GiteParentID (De Gîte is al geheel bezet).  
  * **OF** Er een reservering bestaat waar EenheidID een **Child** is van GiteParentID (Er is al een los bed bezet).  
  * **Check:** In beide gevallen binnen de gewenste datumrange.

**Scenario B: Gast wil 'Losse Slaapplek' (Child) boeken**  
**Doel:** Voorkomen dat een bed geboekt wordt als de Gîte al in zijn geheel verhuurd is (via Booking.com).

* **Input:** GewensteStartDatum, GewensteEindDatum, SlaapplekChildID (bijv. 2).  
* **Logica:** Er is een conflict (beschikbaarheid \= FALSE) als:  
  * Er een reservering bestaat voor EenheidID \= SlaapplekChildID (Dit specifieke bed is al bezet).  
  * **OF** Er een reservering bestaat voor de **Parent** van dit ID (De hele Gîte is al bezet).  
  * **Note:** Het is **geen** conflict als andere 'Children' (broertjes/zusjes) bezet zijn. Dat is juist de bedoeling van het hostel-model.

## 

## 5.2 Prijsbepaling (Hoe wordt €200 vs €27,50 opgehaald?) {#5.2-prijsbepaling-(hoe-wordt-€200-vs-€27,50-opgehaald?)}

Het systeem moet de juiste prijs ophalen uit de TARIEF tabel. Omdat prijzen historisch worden opgeslagen, mag niet zomaar de "laatste" prijs worden gepakt.

**Stappenplan ophalen prijs:**

1. Bepaal het **TypeID** van de gekozen eenheid (via de tabel **VERHUUR\_EENHEID** \-\> **ACCOMMODATIE\_TYPE**).  
2. Bepaal het **PlatformID** (Komt de gast via de site, Booking of Airbnb?).  
3. **Query:** Selecteer het record uit **TARIEF** waarbij:  
   * **TypeID** overeenkomt.  
   * **PlatformID** overeenkomt.  
   * **CategorieID** overeenkomt met 'Logies'.  
   * De datum van boeking (systeemdatum) valt tussen GeldigVan en GeldigTot (of GeldigTot IS NULL).

**Validatie Toeristenbelasting:**

* Het systeem checkt het veld **TaxStatus** in het opgehaalde tarief.  
* Voor de Gîte zal dit Incl teruggeven.  
* *Actie:* Op de factuur/scherm wordt geen apart bedrag bijgerekend, maar wordt vermeld "Inclusief **Toeristenbelasting**".

# 

# 6\. Dataseeding & Teststrategie {#6.-dataseeding-&-teststrategie}

Omdat de logica van de Gîte afhankelijk is van een specifieke inrichting van de database (Parents en Children), moet de database na creatie worden gevuld met stamgegevens (Seeding). Zonder deze data functioneert de beschikbaarheidscheck niet.

## 6.1 Initiële Data {#6.1-initiële-data}

Onderstaande SQL-structuur is vereist voor de 'Go-Live' van Sprint 1\.

### **Stap 1: Lookup data aanmaken**

| \-- Categorieën en TypesINSERT INTO ACCOMMODATIE\_TYPE (TypeID, Naam) VALUES (1, 'Gîte-Geheel'), (2, 'Gîte-Slaapplek');INSERT INTO PLATFORM (PlatformID, Naam) VALUES (1, 'Eigen Site'), (2, 'Booking.com'), (3, 'Airbnb');INSERT INTO TARIEF\_CATEGORIE (CategorieID, Naam) VALUES (1, 'Logies'), (2, 'Toeristenbelasting'); |
| :---- |

### **Stap 2: De Gîte Hiërarchie opbouwen (Cruciaal)**

| \-- 1\. De Parent (Het Appartement)INSERT INTO VERHUUR\_EENHEID (EenheidID, Naam, TypeID, MaxCapaciteit, ParentEenheidID) VALUES (1, 'Gîte Le Marconnès (Totaal)', 1, 9, NULL);\-- 2\. De Children (De Slaapplekken)\-- Let op: ParentEenheidID \= 1 (Verwijst naar de Gîte hierboven)INSERT INTO VERHUUR\_EENHEID (Naam, TypeID, MaxCapaciteit, ParentEenheidID) VALUES ('Slaapplek 1', 2, 1, 1);INSERT INTO VERHUUR\_EENHEID (Naam, TypeID, MaxCapaciteit, ParentEenheidID) VALUES ('Slaapplek 2', 2, 1, 1);\-- ... (Herhaal tot Slaapplek 9\) |
| :---- |

### **Stap 3: Tarieven instellen (2025)**

| \-- Prijs Model A: €200 (Geheel) op Booking.comINSERT INTO TARIEF (TypeID, PlatformID, CategorieID, Prijs, TaxStatus, GeldigVan) VALUES (1, 2, 1, 200.00, 'Incl', '2025-01-01');\-- Prijs Model B: €27,50 (Slaapplek) op AirbnbINSERT INTO TARIEF (TypeID, PlatformID, CategorieID, Prijs, TaxStatus, GeldigVan) VALUES (2, 3, 1, 27.50, 'Incl', '2025-01-01'); |
| :---- |

## 6.2 Test Scenarios {#6.2-test-scenarios}

Om te valideren of de in H5 beschreven logica werkt, dienen de volgende tests uitgevoerd te worden na implementatie:

### **Test A: Het "Exclusiviteit" Scenario**

**Actie:** Maak een reservering voor 'Gîte Le Marconnès (Totaal)' (EenheidID 1).  
**Vervolgactie:** Probeer een reservering te maken voor 'Slaapplek 1'.  
**Verwacht Resultaat:**   
Systeem weigert de tweede boeking (Conflict: Parent is bezet).

### **Test B: Het "Hostel" Scenario**

**Actie:** Maak een reservering voor 'Slaapplek 1'.  
**Vervolgactie 1:** Probeer een reservering te maken voor 'Gîte Le Marconnès (Totaal)'.  
**Verwacht Resultaat:**   
	Geweigerd (Conflict: Een Child is bezet).  
**Vervolgactie 2:** Probeer een reservering te maken voor 'Slaapplek 2'.  
**Verwacht Resultaat:**   
	Geaccepteerd (Geen conflict: Ander Child is vrij).

# Bijlagen {#bijlagen}

* [Normalisatie OLTP - Le Marconnès](https://docs.google.com/document/d/1mqOBPnGz6LEanjo7b0kP2hMc3p83aUXyt1ZLNn0WT4g/edit?tab=t.0#heading=h.7vayxlia4t2a)  
* [Entiteiten - Le Marconnès](https://docs.google.com/document/d/12Yz2rbDUZeA0azK6cwIm2jXnBff75Kv7PLTluI86tX0/edit?tab=t.0#heading=h.mbcc8054ssu8)

[image1]: <data:image/png;base64,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>