# Normalisatie OLTP \- Le Marconnès

**Auteurs:** 

# Inhoudsopgave

[**1\. Inleiding	3**](#1.-inleiding)

[1.1 Doelstelling van dit document	3](#1.1-doelstelling-van-dit-document)

[1.2 Relatie tot de Functionele Analyse (Doc 1\)	3](#1.2-relatie-tot-de-functionele-analyse-\(doc-1\))

[1.3 Aanpak en Methodiek	3](#1.3-aanpak-en-methodiek)

[**2\. Data Inventarisatie (De Input)	4**](#2.-data-inventarisatie-\(de-input\))

[2.1 Bron A: Het Reserveringsformulier	4](#2.1-bron-a:-het-reserveringsformulier)

[2.2 Bron B: Analyse van Casusteksten & E-mails	5](#2.2-bron-b:-analyse-van-casusteksten-&-e-mails)

[2.2.1 Specificatietekst	5](#2.2.1-specificatietekst)

[2.2.2 Email	5](#2.2.2-email)

[2.2.3 Extra tekst	6](#2.2.3-extra-tekst)

[2.2.4 Analyse en Extractie	6](#2.2.4-analyse-en-extractie)

[2.3 Geaggregeerde Data-elementen Lijst	8](#2.3-geaggregeerde-data-elementen-lijst)

[**3\. Het Normalisatieproces	9**](#3.-het-normalisatieproces)

[3.1 Nulde Normaalvorm (0NF) \- De Unnormalized Form	9](#3.1-nulde-normaalvorm-\(0nf\)---de-unnormalized-form)

[0a \- Bepaal naam begin-ENTITEIT: Identificeer de hoofdentiteit	9](#0a---bepaal-naam-begin-entiteit:-identificeer-de-hoofdentiteit)

[0b \- Neem alle gegevens van het formulier over: Verzamel alle gegevens.	9](#0b---neem-alle-gegevens-van-het-formulier-over:-verzamel-alle-gegevens.)

[0c \- Verwijder de proces-, statische-, herleidbare gegevens	10](#0c---verwijder-de-proces-,-statische-,-herleidbare-gegevens)

[0d \- Bepaal de primary key: Identificeer de unieke sleutel voor de entiteit.	10](#0d---bepaal-de-primary-key:-identificeer-de-unieke-sleutel-voor-de-entiteit.)

[0e \- Eindresultaat 0NF (LGS):	10](#0e---eindresultaat-0nf-\(lgs\):)

[3.2 Eerste Normaalvorm (1NF) \- Verwijderen van Herhalende Groepen	11](#3.2-eerste-normaalvorm-\(1nf\)---verwijderen-van-herhalende-groepen)

[1a \- Verwijder herhalende groep (HG)	11](#1a---verwijder-herhalende-groep-\(hg\))

[1b \- Eindresultaat (LGS)	11](#1b---eindresultaat-\(lgs\))

[3.3 Tweede Normaalvorm (2NF) \- Partiële Afhankelijkheden	12](#3.3-tweede-normaalvorm-\(2nf\)---partiële-afhankelijkheden)

[2a \- Verwijder attributen die afhankelijk zijn van een deel van de primaire sleutel	12](#2a---verwijder-attributen-die-afhankelijk-zijn-van-een-deel-van-de-primaire-sleutel)

[2b \- Eindresultaat (LGS)	12](#2b---eindresultaat-\(lgs\))

[3.4 Derde Normaalvorm (3NF) \- Transitieve Afhankelijkheden	14](#3.4-derde-normaalvorm-\(3nf\)---transitieve-afhankelijkheden)

[3a \- Verwijder attributen die afhankelijk zijn van niet-sleutelvelden	14](#3a---verwijder-attributen-die-afhankelijk-zijn-van-niet-sleutelvelden)

[3b \- Eindresultaat (LGS)	14](#3b---eindresultaat-\(lgs\))

[**4\. Relaties & Kardinaliteit	15**](#4.-relaties-&-kardinaliteit)

[**5\. Het Definitieve Datamodel	19**](#5.-het-definitieve-datamodel)

[4.1 Logisch Gegevensschema (LGS)	19](#4.1-logisch-gegevensschema-\(lgs\))

[4.2 Entity Relationship Diagram (ERD)	20](#4.2-entity-relationship-diagram-\(erd\))

[**5\. Data Dictionary (DD)	21**](#5.-data-dictionary-\(dd\))

[5.1 Tabellen en Attributen	21](#heading=h.fmau0yxy0pe0)

[5.2 Datatypes en Constraints	21](#heading=h.1h40skcw5axa)

[**6\. Verantwoording & Reflectie	22**](#6.-verantwoording-&-reflectie)

[6.1 Dekking van de Requirements	22](#6.1-dekking-van-de-requirements)

[6.2 Beperkingen en Toekomstbestendigheid	22](#6.2-beperkingen-en-toekomstbestendigheid)

# 1\. Inleiding {#1.-inleiding}

## 1.1 Doelstelling van dit document {#1.1-doelstelling-van-dit-document}

Voor de digitalisering van recreatiebedrijf Le Marconnès is een robuuste en toekomstbestendige opslag van gegevens noodzakelijk. Waar de huidige situatie wordt gekenmerkt door losse formulieren, e-mailverkeer en impliciete kennis van de eigenaren, vereist de nieuwe applicatie een gestructureerde database die de data-integriteit waarborgt.

Het doel van dit document is het realiseren van een technisch gevalideerd database-ontwerp. Door middel van een formeel normalisatieproces (van 0NF naar 3NF) wordt de ruwe informatiebehoefte omgezet in een relationeel model dat vrij is van redundantie en anomalieën, zoals oorspronkelijk gedefinieerd in het relationele model (Codd, 1970).

Het eindresultaat van dit document zal bestaand uit een Logisch Gegevensschema (LGS), een Entity Relationship Diagram (ERD) en een Data Dictionary (DD). Deze dienen als de directe blauwdruk voor de realisatie van de SQL-database.

## 1.2 Relatie tot de Functionele Analyse (Doc 1\) {#1.2-relatie-tot-de-functionele-analyse-(doc-1)}

Dit document bouwt voort op de inzichten uit **Document 1: Entiteiten Analyse.**

* **Document 1** beschreef de bedrijfslogica en de informatiebehoefte: welke gegevens zijn nodig om het hotel, de gîte en de camping te runnen? Het identificeerde de functionele entiteiten en complexe vraagstukken zoals het hybride verhuurmodel van de Gîte.

* **Document 2 (dit document)** vertaalt deze behoefte naar technische specificaties. Het normalisatieproces in dit document fungeert als het mathematische bewijs dat de geïdentificeerde datastructuur technisch correct en efficiënt is.

Waar Document 1 de vraag "Wat?" beantwoordt, beantwoordt dit document de vraag "Hoe?". Eventuele discrepanties die tijdens het normaliseren aan het licht komen, worden teruggekoppeld om de bedrijfsregels aan te scherpen.

## 1.3 Aanpak en Methodiek {#1.3-aanpak-en-methodiek}

Om tot een correct ontwerp te komen, wordt de volgende methodiek gehanteerd, gebaseerd op de standaardcyclus voor databaseontwikkeling (Connolly & Begg, 2014):

1. **Data Inventarisatie:** Alle relevante data-elementen worden verzameld uit de primaire bronnen (het reserveringsformulier, casusteksten en e-mails van de eigenaren).

2. **Normalisatie:** De verzamelde data wordt stapsgewijs genormaliseerd:

   * **0NF (Unnormalized Form):** De volledige dataset in één ongeordende tabel.  
   * **1NF:** Het elimineren van herhalende groepen om atomiciteit te garanderen.  
   * **2NF:** Het verwijderen van partiële afhankelijkheden (essentieel voor historische prijzen en samengestelde sleutels) Een relatie staat in 2NF als het in 1NF staat en elk niet-sleutel attribuut volledig functioneel afhankelijk is van de primaire sleutel (Codd, 1970; Connolly & Begg, 2014).  
   * **3NF:** Het verwijderen van transitieve afhankelijkheden (het creëren van lookup-tabellen).  
3. **Modellering:** Het genormaliseerde model wordt visueel uitgewerkt in een ERD en gedetailleerd beschreven in een Data Dictionary, inclusief datatypes en constraints.

# 2\. Data Inventarisatie (De Input) {#2.-data-inventarisatie-(de-input)}

## 2.1 Bron A: Het Reserveringsformulier {#2.1-bron-a:-het-reserveringsformulier}

**Formulier**  
![][image1]

**Geïdentificeerde Attributen**:

* Waarvoor (Keuze Camping/Hotel/Gîte)  
* Naam (Verplicht)  
* E-mailadres  
* Telefoon  
* Startdatum (Te reserveren datum)  
* Aantal volwassenen  
* Kinderen 0 \-7  
* Kinderen 7-12  
* Opmerkingen

## 2.2 Bron B: Analyse van Casusteksten & E-mails {#2.2-bron-b:-analyse-van-casusteksten-&-e-mails}

De teksten bevatten cruciale bedrijfsregels die leiden tot extra data-elementen die niet op het formulier staan, maar wel opgeslagen moeten worden.

### **2.2.1 Specificatietekst** {#2.2.1-specificatietekst}

Elke reservering hoort bij één gast en kan betrekking hebben op één accommodatie (gîte, hotelkamer of kampeerplaats).

Een gast kan uiteraard meerdere reserveringen hebben, zowel op hetzelfde moment als over een tijdspanne.

Een gast betaalt elke reservering afzonderlijk, ook als er meerdere reserveringen door dezelfde gast gedaan zijn.

Restaurantbezoeken kunnen bij een reservering gezet worden of afzonderlijk afgerekend worden. **“Zet maar op de rekening.”**

### **2.2.2 Email** {#2.2.2-email}

Goedemorgen Miel,

Goed dat je weer even onze herinnering hebt opgeschoond.  
Om antwoord te geven op je vragen:  
We promoten en verhuren de Gite via **Booking**.com en **Airbnb**.

De prijs op beide sites is onderhevig aan commissie.  
Op **Booking** verhuren we de Gite als apartement voor € **200**,- per nacht incl tax-sejour.  
Dan gaat het over de hele Gite (9 slaapplaatsen)  
Op **Airbnb** kan dat ook maar tevens verhuren we daar per slaapplaats a **€ 27,50** per nacht incl tax sejour.

Het hotel heeft in totaal **6 kamers**: **2-2persoons, 3-4 persoons en 1-5 persoons dus 21** slaapplaatsen alle **excl. tax sejour**.  
In het restaurant doen wij maximaal 30 couverts per diner.

Totaal aantal slaapplaatsen Gite en Hotel 30\. Die zijn vrijwel nooit allemaal verhuurd.  
Dat we meer open zetten dan we hebben is een keuze van ons om ook grotere gezelschappen te bereiken.

Hopelijk is dit voldoende info.  
We zijn benieuwd naar de resultaten.

Met vriendelijke groet,  
Le Marconnes  
Elvire en Ed.

### **2.2.3 Extra tekst** {#2.2.3-extra-tekst}

Reserveren gaat op dit moment via bovenstaand form.  
Deze informatie gaat dan via mail naar de eigenaars. Er is geen manier om de beschikbaarheid te controleren en de rest van de communicatie moet dus via mail of telefoon.

Personeel moet in staat zijn om **beschikbaarheid te controleren en beheren**. Er moet een functionaliteit zijn om bepaalde **plaatsen** of **kamers** te **blokkeren** (**onderhoud**, **schoonmaak**) en te **reserveren** (kennissen, familie, mondelinge overeenkomsten). Personeel kan deze blokkades aanbrengen, maar **eigenaars moeten alles kunnen overrulen**. Veranderingen die een personeelslid heeft gemaakt kunnen aangepast worden door eigenaars, maar andersom niet.  
Er is op dit moment geen mogelijkheid om te reserveren voor het restaurant. 

Dat moet veranderen. Al de beperkingen moeten snel en makkelijk worden aangepast. Het maximale aantal bezoekers van het restaurant varieert qua gerecht en hoe de zin van de uitbater is.  Meestal is het een vast aantal, soms meer, soms minder. De eigenaars willen een makkelijk aanpasbaar systeem.

### **2.2.5 Overige Details (Logging \- Slide 13\)**

"De applicatie houdt een volledig log bij van alle acties (wijzigingen, aanmeldingen, reserveringen)."

### 

### **2.2.4 Analyse en Extractie** {#2.2.4-analyse-en-extractie}

Bij deze de gevonden attributen binnen de tekst. Die nodig zijn binnen dit project

| *"We promoten en verhuren de Gite via Booking.com en Airbnb."* Platform (Bron van de boeking). Commissie (Percentage of bedrag per platform). |
| :---- |
| ***"Op Booking... € 200,- per nacht incl tax... Op Airbnb... € 27,50 per nacht."*** **PrijsPerNacht** (Variabel per bron/type). **ToeristenbelastingStatus** (Inclusief of Exclusief). **VerhuurType** (Gehele eenheid of losse slaapplaats). |
| ***"Het hotel heeft... 21 slaapplaatsen alle excl. tax sejour."*** **ToeristenbelastingTarief** (Het bedrag dat apart gerekend moet worden). |
| ***"Zet maar op de rekening."*** **RestaurantBestellingID** (Gekoppeld aan reservering). **BetaalStatus** (Op rekening / Direct voldaan). |
| ***Personeel moet... beschikbaarheid controleren... blokkeren (onderhoud, schoonmaak)."*** **KamerStatus** (Vrij, Geboekt, Geblokkeerd, Onderhoud). **GebruikerRol** (Eigenaar die kan overrulen vs. Personeel). |
| ***"Camping componenten (uit eerdere analyse Doc 1)"*** **Elektriciteit** (Ja/Nee), Hond (Ja/Nee). |
| **Overige Details (Logging) LogDatum** (Tijdstip van de handeling) **LogActie** (Het type handeling: Aanmaken, Wijzigen, Verwijderen, Inloggen) **LogGebruiker** (Wie de actie heeft uitgevoerd) **LogTabel** (Op welke entiteit de actie betrekking had) **OudeWaarde** (De data vòòr de wijziging, voor herstel) **NieuweWaarde** (De data na de wijziging) |

## 

## 2.3 Geaggregeerde Data-elementen Lijst {#2.3-geaggregeerde-data-elementen-lijst}

1. **Gastgegevens:**  
   * Gast Naam  
   * Gast Email  
   * Gast Telefoon  
   * Gast Straat  
   * Gast Huisnummer  
   * Gast Postcode  
   * Gast Woonplaats  
   * Gast Land  
   * Gast IBAN  
   * Gebruiker Wachtwoord (voor account)  
   * Gebruiker Rol (Gast/Medewerker/Eigenaar)

2. **Verblijfsgegevens:**  
   * Reservering Startdatum  
   * Reservering Einddatum (of Aantal Nachten)  
   * Aantal Volwassenen  
   * Aantal Kinderen (0-7)  
   * Aantal Kinderen (7-12)  
   * Aantal Honden  
   * Opmerkingen  
   * Platform (Website/Booking/Airbnb)  
   * Reservering Status (Gereserveerd/Ingecheckt/Geannuleerd)

3. **Accommodatie & Prijzen:**  
   * Accommodatie Keuze (Hotel/Gite/Camping)  
   * Verhuur Eenheid Naam (bijv. "Kamer 1", "Plek A")  
   * Verhuur Type (Slaapplek vs Geheel)  
   * Prijs Per Nacht  
   * Toeristenbelasting Tarief  
   * Toeristenbelasting Status (Incl/Excl)  
   * Commissie Bedrag  
   * Camping Optie Elektriciteit  
   * Camping Optie Hond

4. **Horeca & Financieel:**  
   * Restaurant Reservering Datum/Tijd  
   * Aantal Couverts  
   * Besteld Product (Gerecht/Drank)  
   * Product Prijs  
   * Factuur Totaalbedrag  
   * Betaalwijze (Direct / Op Rekening)

# 

# 3\. Het Normalisatieproces {#3.-het-normalisatieproces}

## 3.1 Nulde Normaalvorm (0NF) \- De Unnormalized Form {#3.1-nulde-normaalvorm-(0nf)---de-unnormalized-form}

#### 0a \- Bepaal naam **begin-ENTITEIT**: Identificeer de hoofdentiteit {#0a---bepaal-naam-begin-entiteit:-identificeer-de-hoofdentiteit}

We definiëren één allesomvattende entiteit die een volledige klantinteractie (boeking \+ verblijf \+ betaling) representeert: **VERBLIJFSTRANSACTIE**.

#### 0b \- Neem alle gegevens van het formulier over: **Verzamel alle gegevens**. {#0b---neem-alle-gegevens-van-het-formulier-over:-verzamel-alle-gegevens.}

We combineren de invoervelden van de gast (formulier) met de vaste gegevens uit de casustekst (zoals belastingregels en platformcommissies).

* **Gast & Account (Bron: Formulier \+ Tekst)**  
  * Naam  
  * Email  
  * Straat  
  * Huisnummer  
  * Postcode  
  * Woonplaats  
  * Land  
  * IBAN  
  * Telefoonnumer  
  * Wachtwoord  
  * Rol  
* **Verblijfsgegevens (Bron: Formulier \+ Bedrijfsregels)**  
  * Startdatum  
  * Einddatum / Aantal Nachten  
  * Aantal Volwassenen  
  * Aantal Kinderen (0-7)  
  * Aantal Kinderen (7-12)  
  * Aantal Honden  
  * Opmerkingen  
  * Reservering Status  (Gereserveerd/Ingecheckt/Geannuleerd)  
* **Accommodatie & Eenheid (Bron: Formulier \+ Tekst)**  
  * Type (Hotel/Gîte/Camping)  
  * Verhuur Type (Slaapplek vs Geheel)  
  * Eenheid Naam (bijv. “Kamer 1”, “Plek A”)  
* **Financiële Componenten (Bron: E-mails)**  
  * Platform  
  * Platform Commisie  
  * Prijs Per Nacht (Basis)  
  * Toeristenbelasting Tarief  
  * Toeristenbelasting Status

* **Camping \- Basis (Bron: Formulier \+ Tekst)**  
  * Prijs Plaats  
  * Prijs Volwassenen  
  * Prijs Hond  
  * Prijs Elektra  
* **Camping \- Opties  (Bron: Formulier \+ Tekst)**  
  * Camping Optie Elektriciteit (bool)  
  * Camping Optie Hond (bool)  
* **Horeca (Bron: Tekst "Op rekening")**  
  * Reservering Datum/Tijd  
  * Aantal Couverts  
  * Bestelde producten  
  * Product Prijs  
  * Factuur Totaalbedrag  
  * Betaalwijze (Direct / Op Rekening)  
* **Logging (Systeem entiteit NIET NORMALISEREN pas 3NF)**  
  * Log Datum  
  * Log Actie  
  * Log Gebruiker  
  * Log Tabel  
  * Oude Waarde   
  * Nieuwe Waarde 

#### 0c \- Verwijder de **proces**\-, **statische**\-, **herleidbare** gegevens {#0c---verwijder-de-proces-,-statische-,-herleidbare-gegevens}

* **Factuur Totaalbedrag:**   
  Dit is \[Aantal nachten \* Prijs \+ Horeca\]  
* **Commisie Bedrag:**  
  Dit is \[Prijs \* PlatformPercentage\]  
* **Boolean Hond:**   
  Afleidbaar uit 'Aantal Honden \> 0'.

#### 0d \- Bepaal de **primary key**: Identificeer de unieke sleutel voor de entiteit. {#0d---bepaal-de-primary-key:-identificeer-de-unieke-sleutel-voor-de-entiteit.}

GastEmail \+ Startdatum \+ AccommodatieEenheid.  
**Aanname:** Een gast op 1 dag niet 2x dezelfde kamer boeken.

#### 0e \- Eindresultaat 0NF (LGS): {#0e---eindresultaat-0nf-(lgs):}

**VERBLIJFSTRANSACTIE** (  
GastEmail, Startdatum, EenheidNaam, GastNaam, GastTel, GastStraat, GastHuisnr, GastPostcode, GastWoonplaats, GastLand, GastIBAN, GastWachtwoord, GastRol, Einddatum, AantalVolw, AantalKind, AantalHond, Opmerkingen, ReserveringStatus, PlatformNaam, PlatformCommissiePerc, AccommodatieType, VerhuurType, PrijsPerNacht, TaxTarief, TaxStatus, PrijsPlaats, PrijsVolw, PrijsKind, PrijsHond, PrijsElektra, CampingStroomOptie, CampingHondOptie, HorecaDatum, HorecaProduct, HorecaAantal, HorecaPrijs,   
)

## 3.2 Eerste Normaalvorm (1NF) \- Verwijderen van Herhalende Groepen {#3.2-eerste-normaalvorm-(1nf)---verwijderen-van-herhalende-groepen}

#### 1a \- Verwijder herhalende groep (HG) {#1a---verwijder-herhalende-groep-(hg)}

**Herhaal onderstaande stappen totdat geen HG meer voorkomt**

* Maak een nieuw entiteittype aan  
* Bepaal een unieke naam  
* Kopieer primarykey-attributen van origineel  
* Verplaats alle herhalende velden  
* Bepaal de unieke key voor nieuw entiteittype

Een verblijfstransactie bevat groepen gegevens die vaker kunnen voorkomen binnen één verblijf (1:N relatie):

1. **Horeca:** Een gast kan meerdere keren iets bestellen tijdens het verblijf.  
2. **Accommodatie Details (Tariefregels):** Vooral bij de Camping bestaat de prijs uit meerdere regels (Plaats \+ Persoon \+ Hond).

We splitsen deze groepen af. We moeten de samengestelde sleutel van de ouder (GastEmail \+ Startdatum \+ EenheidNaam) meenemen naar de kind-tabellen om de koppeling te behouden.

#### 1b \- Eindresultaat (LGS) {#1b---eindresultaat-(lgs)}

**RESERVERING** (  
GastEmail, Startdatum, EenheidNaam, GastNaam, GastTel, GastStraat, GastHuisnr, GastPostcode, GastWoonplaats, GastLand, GastIBAN, GastWachtwoord, GastRol, Einddatum, AantalVolw, AantalKind, AantalHond, Opmerkingen, ReserveringStatus, PlatformNaam, PlatformCommissiePerc, AccommodatieType, VerhuurType, PrijsPerNacht, TaxTarief, TaxStatus  
)  
**RESERVERING\_DETAIL** (  
GastEmail, Startdatum, EenheidNaam, DetailType, Aantal, PrijsPerEenheid  
)  
**HORECA\_BESTELLING** (  
GastEmail, Startdatum, EenheidNaam, BestellingDatum, ProductNaam, Aantal, ProductPrijs  
)

De natuurlijke sleutels beginnen complex te worden en ook wel lang dus gaan we deze omtoveren naar ID’s

## 

## 3.3 Tweede Normaalvorm (2NF) \- Partiële Afhankelijkheden {#3.3-tweede-normaalvorm-(2nf)---partiële-afhankelijkheden}

#### 2a \- Verwijder attributen die afhankelijk zijn van een deel van de primaire sleutel {#2a---verwijder-attributen-die-afhankelijk-zijn-van-een-deel-van-de-primaire-sleutel}

**Verwijder alle attributen die slechts functioneel afhankelijk zijn van een deel van de sleutel**

* Maak een nieuw entiteittype aan met unieke naam  
* Kopieer het afhankelijke deel van de primary key  
* Verplaats alle herhalende velden  
* Bepaal unieke primary key voor nieuw entiteittype

We zoeken naar attributen die niet afhankelijk zijn van de gehele sleutel, maar slechts van een deel. Tegelijkertijd introduceren we **Surrogaatsleutels (ID's)** om de complexe sleutels uit 1NF op te lossen.

**De keys**

* **Gastgegevens:** GastNaam, Tel, Wachtwoord hangen af van de GastEmail, niet van de boeking. Ik wil deze eigenlijk splitten naar **GAST & GEBRUIKER**   
  (Vanwege Doc 1: Splitsing van account/gast)

* **Platformgegevens:** CommissiePerc hangt af van PlatformNaam. Deze wil ik splitsen naar een **PLATFORM** entiteit voor extra scheiding en netheid.

* **Horeca Prodcuten:** ProductPrijs hangt af van ProductNaam. Deze wil ik splitsen naar **PRODUCT**. Zodat alles goed bij elkaar staat en er een duidelijke scheiding is.

#### 2b \- Eindresultaat (LGS) {#2b---eindresultaat-(lgs)}

**GAST** (  
GastID, Naam, Email, Tel, Straat, Huisnr, Postcode, Plaats, Land, IBAN  
)

**GEBRUIKER** (  
GebruikerID, GastID, Email, WachtwoordHash, Rol  
)  
**Foreign keys:**  
	(GastID → GAST(GastID))

**PLATFORM** (  
PlatformID, Naam, CommissiePercentage  
)

**PRODUCT** (  
ProductID, Naam, HuidigePrijs, BTW\_Categorie  
)

**RESERVERING** (  
ReserveringID, GastID, PlatformID, EenheidNaam, Startdatum, Einddatum, Status, AccommodatieType, VerhuurType, PrijsPerNacht, TaxTarief, TaxStatus  
)  
**Foreign keys:**  
	(GastID → GAST(GastID))  
	(PlatformID → PLATFORM(PlatformID))

**RESERVERING\_DETAIL** (  
DetailID, ReserveringID, DetailType, Aantal  
)  
**Foreign keys:**  
	(ReserveringID → RESERVERING(ReserveringID))

**HORECA\_BESTELLING** (  
BestellingID, ReserveringID, Datum, ProductID, Aantal, PrijsOpMoment  
)  
**Foreign keys:**  
	ReserveringsID → RESERVERING(ReserveringID)  
	ProductID → PRODUCT(ProductID)

## 

## 3.4 Derde Normaalvorm (3NF) \- Transitieve Afhankelijkheden {#3.4-derde-normaalvorm-(3nf)---transitieve-afhankelijkheden}

#### 3a \- Verwijder attributen die afhankelijk zijn van niet-sleutelvelden {#3a---verwijder-attributen-die-afhankelijk-zijn-van-niet-sleutelvelden}

**Herhaal de onderstaande stappen totdat er geen functionele afhankelijkheid meer te vinden is.**

* Maak een nieuwe entiteit aan.  
* Verplaats de afhankelijke attributen naar de nieuwe entiteit.  
* Bepaal een primaire sleutel voor de nieuwe entiteit.  
* Bepaal primary key nieuw entiteittype

We verwijderen attributen die afhankelijk zijn van een niet-sleutel attribuut. Hier integreren we de "Bedrijfslogica" uit Document 1\.

#### 3b \- Eindresultaat (LGS) {#3b---eindresultaat-(lgs)}

**GAST** (  
GastID, Naam, Email, Tel, Straat, Huisnr, Postcode, Plaats, Land, IBAN  
)

**GEBRUIKER** (  
GebruikerID, GastID, Email, WachtwoordHash, Rol  
)  
**Foreign keys:**  
	(GastID → GAST(GastID))

**PLATFORM** (  
PlatformID, Naam, CommissiePercentage  
)

**PRODUCT** (  
ProductID, Naam, HuidigePrijs, BTW\_Categorie  
)

**ACCOMMODATIE\_TYPE** (  
TypeID, Naam (Hotelkamer, Gîte, Campingplek)  
)

**TARIEF\_CATEGORIE** (  
CategorieID, Naam (Waarden: Logies, Volwassene, Kind, Hond, Elektra, Toeristenbelasting)  
**)**

**VERHUUR\_EENHEID (**  
EenheidID, Naam (Kamer 1, Gîte), TypeID, MaxCapaciteit, ParentEenheidID  
**)**  
**Foreign keys:**  
	(TypeID → ACCOMMODATIE\_TYPE(TypeID))  
	(ParentEenheidID → VERHUUR\_EENHEID(EenheidID))

**TARIEF (**  
TariefID, TypeID, CategorieID, PlatformID, Prijs, TaxStatus, TaxTarief, GeldigVan, GeldigTot  
**)**  
**Foreign keys:**  
(TypeID → ACCOMMODATIE\_TYPE (TypeID))  
(CategorieID → TARIEF\_CATEGORIE(CategorieID))  
(PlatformID → PLATFORM(PlatformID))  
	

**RESERVERING** (  
ReserveringID, GastID, EenheidID, PlatformID, Startdatum, Einddatum, Status  
)  
**Foreign keys:**  
	(GastID → GAST(GastID))  
	(EenheidID → VERHUUR\_EENHEID(EenheidID))  
	(PlatformID → PLATFORM(PlatformID))  
	

**RESERVERING\_DETAIL** (  
DetailID, ReserveringID, CategorieID, Aantal, PrijsOpMoment  
)  
**Foreign keys:**  
	**(**ReserveringID → RESERVERING(ReserveringID))  
	(CategorieID → TARIEF\_CATEGORIE(CategorieID))

**HORECA\_BESTELLING** (  
BestellingID, ReserveringID, Datum, ProductID, Aantal, PrijsOpMoment  
)  
**Foreign keys:**  
	ReserveringsID → RESERVERING(ReserveringID)  
	ProductID → PRODUCT(ProductID)

**LOGBOEK** (  
**LogID**, GebruikerID, Tijdstip, Actie, TabelNaam, RecordID, OudeWaarde, NieuweWaarde  
)  
**Foreign keys:**  
	GebruikerID → GEBRUIKER(GebruikerID)

# 4\. Relaties & Kardinaliteit {#4.-relaties-&-kardinaliteit}

| Relatie: GAST *doet* RESERVERING Werkwoorden: Hoort bij / Heeft / Plaatsen  Tekstuele basis / Bron: "Elke reservering hoort bij één gast... Een gast kan uiteraard meerdere reserveringen hebben, zowel op hetzelfde moment als over een tijdspanne." Betrokken entiteiten: GAST, RESERVERING Kardinaliteit: 1:N (GAST : RESERVERING) Beschrijving: Een unieke GAST kan nul, één of meerdere RESERVERINGEN op zijn naam hebben staan. Een enkele RESERVERING is altijd gekoppeld aan exact één GAST (de hoofdboeker). Modellering: RESERVERING.GastID (FK) verwijst naar GAST.GastID (PK) |
| :---- |
| **Relatie:** RESERVERING *betreft* VERHUUR\_EENHEID **Werkwoorden:** Betrekking hebben op / Reserveren  **Tekstuele basis / Bron:** "...en kan betrekking hebben op één accommodatie (gîte, hotelkamer of kampeerplaats)." **Betrokken entiteiten:** VERHUUR\_EENHEID, RESERVERING **Kardinaliteit:** 1:N (VERHUUR\_EENHEID : RESERVERING) **Beschrijving:** Een VERHUUR\_EENHEID (bijv. 'Kamer 1' of 'Plek A') kan over de tijd heen meerdere keren gereserveerd worden. Een specifieke RESERVERING betreft altijd exact één fysieke VERHUUR\_EENHEID. **Modellering:** RESERVERING.EenheidID (FK) verwijst naar VERHUUR\_EENHEID.EenheidID (PK) |
| **Relatie**: RESERVERING is afkomstig van PLATFORM **Werkwoorden**: Promoten / Verhuren via **Tekstuele basis / Bron**: "We promoten en verhuren de Gite via Booking.com en Airbnb. De prijs op beide sites is onderhevig aan commissie." **Betrokken entiteiten**: PLATFORM, RESERVERING **Kardinaliteit:** 1:N (PLATFORM : RESERVERING) **Beschrijving:** Een PLATFORM (bron) faciliteert meerdere RESERVERINGEN. Elke RESERVERING is afkomstig van exact één PLATFORM (of 'Eigen Website'). Dit bepaalt de commissie. **Modellering:** RESERVERING.PlatformID (FK) verwijst naar PLATFORM.PlatformID (PK) |
| **Relatie**: VERHUUR\_EENHEID is onderdeel van VERHUUR\_EENHEID (Recursief) **Werkwoorden**: Gaat over / Bevat / Verhuren per **Tekstuele basis / Bron:** "Dan gaat het over de hele Gite (9 slaapplaatsen)... tevens verhuren we daar per slaapplaats." **Betrokken entiteiten:** VERHUUR\_EENHEID, VERHUUR\_EENHEID **Kardinaliteit:** 1:N (Recursief op VERHUUR\_EENHEID) **Beschrijving:** Een VERHUUR\_EENHEID (Parent, bijv. 'De Gîte') kan bestaan uit meerdere sub-eenheden (Children, bijv. 'Slaapplek 1'). Een sub-eenheid behoort tot maximaal één hoofdeenheid. Dit faciliteert het hybride verhuurmodel. **Modellering:** VERHUUR\_EENHEID.ParentEenheidID (FK) verwijst naar VERHUUR\_EENHEID.EenheidID (PK) |
| **Relatie**: RESERVERING bevat RESERVERING\_DETAILS **Werkwoorden:** Aangeven / Bestaan uit **Tekstuele basis / Bron:** Formulier invoervelden (Volwassenen, Kinderen) en Camping-tarieven (Hond, Elektra). "Aantal personen: Aantal volwassenen, Kinderen 0-7, Kinderen 7-12". **Betrokken entiteiten:** RESERVERING, RESERVERING\_DETAIL **Kardinaliteit:** 1:N (RESERVERING : RESERVERING\_DETAIL) **Beschrijving:** Een RESERVERING bestaat uit één of meerdere detailregels die de samenstelling van het gezelschap en de opties beschrijven (bijv. 2 volwassenen, 1 hond). Een detailregel hoort bij één specifieke reservering. **Modellering:** RESERVERING\_DETAIL.ReserveringID (FK) verwijst naar RESERVERING.ReserveringID (PK) |
| **Relatie**: HORECA\_BESTELLING staat op rekening van RESERVERING **Werkwoorden:** Op de rekening zetten / Afrekenen **Tekstuele basis / Bron:** "Restaurantbezoeken kunnen bij een reservering gezet worden of afzonderlijk afgerekend worden. 'Zet maar op de rekening.'" **Betrokken entiteiten:** RESERVERING, HORECA\_BESTELLING **Kardinaliteit:** 1:N (RESERVERING : HORECA\_BESTELLING) \- Optioneel **Beschrijving:** Een RESERVERING kan meerdere HORECA\_BESTELLINGEN gekoppeld hebben ("op de rekening"). Een HORECA\_BESTELLING kan gekoppeld zijn aan maximaal één RESERVERING (of aan géén, indien direct betaald door een passant). **Modellering:** HORECA\_BESTELLING.ReserveringID (FK, Nullable) verwijst naar RESERVERING.ReserveringID (PK) |
| **Relatie:** GEBRUIKER is gekoppeld aan GAST **Werkwoorden:** Inloggen / Hebben **Tekstuele basis / Bron:** "Om een reservering te boeken moet een gast een account hebben" (S-NF-RES-001) versus praktijk externe boekingen (Document 1 analyse). **Betrokken entiteiten:** GAST, GEBRUIKER **Kardinaliteit:** 1:1 (of 1:0/1) **Beschrijving:** Een GEBRUIKER (account) hoort bij exact één GAST (persoonsgegevens). Een GAST kan een gebruikersaccount hebben (bij directe boeking), maar dit is niet verplicht (bijv. Booking.com gasten). **Modellering:** GEBRUIKER.GastID (FK, Unique) verwijst naar GAST.GastID (PK) |
| **Werkwoorden:** Uitvoeren / Wijzigen / Vastleggen **Tekstuele basis / Bron:** "De applicatie houdt een volledig log bij van alle acties (wijzigingen, aanmeldingen, reserveringen)."(**Bron:** Overige Details & S-NF-SYS-003). **Betrokken entiteiten:** GEBRUIKER, LOGBOEK **Kardinaliteit:** 1:N (GEBRUIKER : LOGBOEK) **Beschrijving:** Een enkele GEBRUIKER kan in de loop van de tijd vele acties uitvoeren (zoals inloggen, boeken, annuleren). Elke individuele actie resulteert in één LOGBOEK-regel. Een LOGBOEK-regel verwijst naar de specifieke gebruiker die de actie heeft geïnitieerd (voor de audit trail). **Modellering:** LOGBOEK.GebruikerID (FK) verwijst naar GEBRUIKER.GebruikerID (PK). |

# 

# 5\. Het Definitieve Datamodel {#5.-het-definitieve-datamodel}

## 4.1 Logisch Gegevensschema (LGS) {#4.1-logisch-gegevensschema-(lgs)}

**GAST** (  
GastID, Email, Naam, Email, Tel, Straat, Huisnr, Postcode, Plaats, Land, IBAN  
)  
**Foreign keys:**  
	N.v.t.

**GEBRUIKER** (  
GebruikerID, GastID, Email, WachtwoordHash, Rol  
)  
**Foreign keys:**  
	(GastID → GAST(GastID))

**PLATFORM** (  
PlatformID, Naam, CommissiePercentage  
)  
**Foreign keys:**  
	N.v.t.

**PRODUCT** (  
ProductID, Naam, HuidigePrijs, BTW\_Categorie  
)  
**Foreign keys:**  
	N.v.t.

**ACCOMMODATIE\_TYPE** (  
TypeID, Naam (Hotelkamer, Gîte, Campingplek)  
)  
**Foreign keys:**  
	N.v.t.

**TARIEF\_CATEGORIE** (  
CategorieID, Naam (Waarden: Logies, Volwassene, Kind, Hond, Elektra, Toeristenbelasting)  
**)**  
**Foreign keys:**  
	N.v.t.

**VERHUUR\_EENHEID (**  
EenheidID, Naam (Kamer 1, Gîte), TypeID, MaxCapaciteit, ParentEenheidID  
**)**  
**Foreign keys:**  
	(TypeID → ACCOMMODATIE\_TYPE(TypeID))  
	(ParentEenheidID → VERHUUR\_EENHEID(EenheidID))

**TARIEF (**  
TariefID, TypeID, CategorieID, PlatformID, Prijs, TaxStatus, TaxTarief, GeldigVan, GeldigTot  
**)**  
**Foreign keys:**  
(TypeID → ACCOMMODATIE\_TYPE (TypeID))  
(CategorieID → TARIEF\_CATEGORIE(CategorieID))  
(PlatformID → PLATFORM(PlatformID))  
	

**RESERVERING** (  
ReserveringID, GastID, EenheidID, PlatformID, Startdatum, Einddatum, Status  
)  
**Foreign keys:**  
	(GastID → GAST(GastID))  
	(EenheidID → VERHUUR\_EENHEID(EenheidID))  
	(PlatformID → PLATFORM(PlatformID))  
	

**RESERVERING\_DETAIL** (  
DetailID, ReserveringID, CategorieID, Aantal, PrijsOpMoment  
)  
**Foreign keys:**  
	**(**ReserveringID → RESERVERING(ReserveringID))  
	(CategorieID → TARIEF\_CATEGORIE(CategorieID))

**HORECA\_BESTELLING** (  
BestellingID, ReserveringID, Datum, ProductID, Aantal, PrijsOpMoment  
)  
**Foreign keys:**  
	ReserveringsID → RESERVERING(ReserveringID)  
	ProductID → PRODUCT(ProductID)

**LOGBOEK** (  
**LogID**, GebruikerID, Tijdstip, Actie, TabelNaam, RecordID, OudeWaarde, NieuweWaarde  
)  
**Foreign keys:**  
	GebruikerID → GEBRUIKER(GebruikerID)

## 

## 4.2 Entity Relationship Diagram (ERD) {#4.2-entity-relationship-diagram-(erd)}

Het genormaliseerde model wordt visueel uitgewerkt in een ERD (Afbeelding 1), om de entiteiten en hun onderlinge relaties grafisch weer te geven volgens de notatie van (Puja, Puscic, Jaksic 2019).

Afbeelding 1  
![][image2]

# 5\. Data Dictionary (DD) {#5.-data-dictionary-(dd)}

Dit document beschrijft de datastructuur van het genormaliseerde OLTP-systeem voor Le Marconnès. Elke tabel wordt gedetailleerd met zijn attributen, beschrijvingen, gegevenstypen, sleutels en aanvullende bedrijfsregels.

### **Entiteit \- GAST**

Bevat de stamgegevens van de gasten. Deze entiteit is losgekoppeld van het gebruikersaccount om ook gasten zonder account (bijv. via Booking.com) te kunnen registreren.

| Attribuut | Beschrijving | Gegevenstype | Sleutel | Aanvullende Regels |
| :---- | :---- | :---- | :---- | :---- |
| **GastID** | Unieke identificatie voor elke gast. | INT | PK | IDENTITY(1,1) |
| **Naam** | Volledige naam van de gast. | VARCHAR(100) |  | NOT NULL |
| **Email** | Het e-mailadres van de gast (gebruikt voor communicatie). | NVARCHAR(150) |  | NOT NULL |
| **Tel** | Telefoonnummer van de gast. | VARCHAR(20) |  |  |
| **Straat** | Straatnaam van het factuuradres. | NVARCHAR(100) |  | NOT NULL (Factuurvereiste) |
| **Huisnr** | Huisnummer en toevoeging. | VARCHAR(20) |  | NOT NULL(Factuurvereiste) |
| **Postcode** | Postcode van het factuuradres. | VARCHAR(20) |  | NOT NULL(Factuurvereiste) |
| **Plaats** | Woonplaats van de gast. | NVARCHAR(100) |  | NOT NULL(Factuurvereiste) |
| **Land** | Land van herkomst (relevant voor Tax/Nachtregister). | NVARCHAR(50) |  | NOT NULL (Default: 'Nederland') |
| **IBAN** | Bankrekeningnummer (optioneel, voor restitutie). | VARCHAR(34) |  | NULLABLE |

### 

### **Entiteit \- GEBRUIKER**

Bevat inloggegevens en rollen voor personen die toegang hebben tot het systeem (Gasten, Personeel, Eigenaar).

| Attribuut | Beschrijving | Gegevenstype | Sleutel | Aanvullende Regels |
| :---- | :---- | :---- | :---- | :---- |
| **GebruikerID** | Unieke identificatie voor het account. | INT | PK | IDENTITY(1,1) |
| **GastID** | Koppeling naar de persoonsgegevens in de tabel GAST. | INT | FK, UK | UNIQUE (1:1 of 1:0 relatie) |
| **Email** | Het e-mailadres dat als gebruikersnaam dient. | NVARCHAR(150) |  | NOT NULL, UNIQUE |
| **WachtwoordHash** | Versleutelde versie van het wachtwoord. | NVARCHAR(255) |  | NOT NULL |
| **Rol** | De rechten van de gebruiker (bijv. 'Gast', 'Medewerker', 'Eigenaar'). | VARCHAR(20) |  | DEFAULT 'Gast' |

### 

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

### 

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

### **Entiteit \- TARIEF\_CATEGORIE**

Tarief categorie wat eigenlijk op basis van de tarieven waarmee Le Marconnès mee werkt en waarmee gerekend wordt.

| Attribuut | Beschrijving | Gegevenstype | Sleutel | Aanvullende Regels |
| :---- | :---- | :---- | :---- | :---- |
| **CategorieID** | Unieke identificatie voor de tarief categorie | INT | PK | IDENTITY(1,1) |
| **Naam** | Naam van de categorie.(bv. ‘Volwassene’, ‘Kind’, ‘Hond’, ‘Elektra’, ‘Toeristenbelasting’) | VARCHAR(100) |  | NOT NULL |

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

### **Entiteit \- HORECA\_BESTELLING**

Registratie van bestellingen in het restaurant die op rekening van een kamer/plaats worden gezet.

| Attribuut | Beschrijving | Gegevenstype | Sleutel | Aanvullende Regels |
| :---- | :---- | :---- | :---- | :---- |
| **BestellingID** | Unieke identificatie van de bestelling. | INT | PK | IDENTITY(1,1) |
| **ReserveringID** | De reservering waarop de kosten geboekt worden. | INT | FK | NULLABLE (Indien direct betaald) |
| **ProductID** | Het bestelde product. | INT | FK |  |
| **Datum** | Datum en tijd van bestelling. | DATETIME |  | DEFAULT GETDATE() |
| **Aantal** | Aantal stuks. | INT |  | CHECK \> 0 |
| **PrijsOpMoment** | De prijs van het product op moment van bestellen. | MONEY |  | NOT NULL |

### 

### **Entiteit \- PRODUCT**

Stamgegevens van de producten die in het restaurant verkocht worden)

| Attribuut | Beschrijving | Gegevenstype | Sleutel | Aanvullende Regels |
| :---- | :---- | :---- | :---- | :---- |
| **ProductID** | Unieke identificatie van het product. | INT | PK | IDENTITY(1,1) |
| **Naam** | Naam van het gerecht of drankje. | VARCHAR(100) |  | NOT NULL |
| **HuidigePrijs** | De actuele verkoopprijs. | MONEY |  |  |
| **BTW\_Categorie** | BTW tariefgroep (Laag/Hoog). | VARCHAR(10) |  |  |

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
| **OudeWaarde** | De waarde vòòr de wijziging (voor herstel). | NVARCHAR(MAX) |  | NULLABLE |
| **NieuweWaarde** | De waarde na de wijziging. | NVARCHAR(MAX) |  | NULLABLE |

# 

# 6\. Verantwoording & Reflectie {#6.-verantwoording-&-reflectie}

## 6.1 Dekking van de Requirements {#6.1-dekking-van-de-requirements}

Het in dit document gepresenteerde datamodel is ontworpen om naadloos aan te sluiten op de eisen zoals vastgelegd in de Requirements Analyse. Het traceren van requirements naar technische specificaties waarborgt dat aan de zakelijke doelstellingen wordt voldaan (Wiegers & Beatty, 2013).

**1\. Flexibele Accommodatiestructuur & Gîte**

* **Requirement:** **G-F-RES-020** (Hele Gîte boeken) **& G-F-RES-021** (Slaapplek boeken).  
* **Realisatie:** Door gebruik te maken van de entiteit VERHUUR\_EENHEID met een recursieve relatie (ParentEenheidID), ondersteunt de database beide verhuurmodellen binnen één structuur. Het systeem kan hierdoor zowel de exclusiviteit van het appartement als de gedeelde capaciteit van losse bedden beheren.  
* **Requirement:** **G-F-RES-016** (Keuze uit Camping, Hotel, Gîte).  
* **Realisatie:** De entiteit **ACCOMMODATIE\_TYPE** maakt het model generiek, waardoor alle bedrijfstakken in dezelfde RESERVERING-tabel passen.

**2\. Complexe Prijsstelling & Componenten**

* **Requirement:** **G-F-RES-008** (Aantal personen/kinderen) & **G-F-RES-017** (Extra opties).  
* **Realisatie:** De splitsing tussen **RESERVERING** (kop) en **RESERVERING\_DETAIL** (regels) maakt het mogelijk om – specifiek voor de camping – kosten op te bouwen uit losse componenten (Plaats \+ Volwassene \+ Elektra).  
* **Requirement: G-NF-RES-005** (Toeristenbelasting inzicht) & **G-NF-RES-004** (Kosten inzicht).  
* **Realisatie:** De entiteit **TARIEF** slaat prijzen en belastingregels historisch op (GeldigVan/Tot). Hierdoor kan het systeem altijd de correcte prijs en specifieke belasting (inclusief bij Gîte, exclusief bij Hotel) tonen en herberekenen.

**3\. Financiële Integratie & Horeca**

* **Requirement: G-F-RES-012** (Restaurant reserveren) & **US\_G021** ("Zet maar op de rekening").  
* **Realisatie:** De tabel **HORECA\_BESTELLING** bevat een nullable Foreign Key naar **RESERVERING**. Dit maakt het mogelijk om consumpties direct te koppelen aan een verblijfsfactuur, of (indien NULL) als directe verkoop aan een passant te registreren.  
* **Requirement:** **E-F-BEH-002** (Financieel overzicht).  
* **Realisatie:** Door de entiteit **PLATFORM** te koppelen, kunnen de eigenaren onderscheid maken tussen bruto-omzet en netto-inkomsten (na aftrek commissie Booking.com/Airbnb).

**4\.  Security & Toegangsbeheer**

* **Requirement: S-NF-RES-001** (Account vereiste) & **S-NF-SYS-003** (Logging).  
* **Realisatie:** Er is gekozen voor een strikte scheiding tussen persoonsgegevens (**GAST**) en inloggegevens (**GEBRUIKER**). Dit lost het conflict op waarbij externe gasten (Booking.com) wel geregistreerd moeten worden, maar geen account hebben. Tevens faciliteert de entiteit **LOGBOEK** de harde eis om alle mutaties traceerbaar te maken voor de eigenaren.

## 6.2 Beperkingen en Toekomstbestendigheid {#6.2-beperkingen-en-toekomstbestendigheid}

Hoewel het model robuust is en voldoet aan de eisen, zijn er aandachtspunten voor de realisatiefase (software-implementatie):

1. **Validatie van Beschikbaarheid (Business Logic):**  
   De database structuur faciliteert de Gîte-hiërarchie, maar dwingt de blokkade niet automatisch af via een constraint. De applicatielaag moet geprogrammeerd worden om te checken: "Als Parent geboekt is, blokkeer Children" en vice versa. Dit is een bewuste keuze om de database performant te houden.

2. **Historische Stamgegevens:**  
   Er is gekozen om **PrijsOpMoment** op te slaan in de transactietabellen (snapshotting). Dit garandeert financiële consistentie. Echter, als een gast verhuist (adreswijziging in GAST), verandert dit adres ook op historische facturen in de weergave. Indien dit juridisch onwenselijk is, dient in de toekomst een aparte **FACTUUR\_ADRES** tabel toegevoegd te worden. Voor de huidige scope wordt dit als acceptabel beschouwd.

3. **Schaalbaarheid Restaurant:**  
   Het huidige model focust op verkoop (**HORECA\_BESTELLING**) en omzet. Diepgaand voorraadbeheer van ingrediënten (inkoop/recepturen) valt buiten de huidige scope, maar het model is voorbereid om in de toekomst een INKOOP-module te koppelen aan de entiteit **PRODUCT**.

# Literatuurlijst

Puja, I. (2019). Overview and Comparison of Several elational Database Modelling Metodologies and Notations*.* https://ieeexplore.ieee.org/abstract/document/8756667

Codd, E. F. (1970). A relational model of data for large shared data banks. *Communications of*  
*the ACM, 13*(6), 377–387. [https://doi.org/10.1145/362384.362685](https://doi.org/10.1145/362384.362685)

Connolly, T. M., & Begg, C. E. (2014). *Database systems: A practical approach to design,*  
*implementation, and management* (6th ed.). Pearson.

Wiegers, K., & Beatty, J. (2013). *Software requirements* (3rd ed.). Microsoft Press.

# Bijlage

* [Casus Blok 2 (2).pptx](https://docs.google.com/presentation/d/1C7de5lcteU7YOSKrZ9IE3XdT5FXuQH8z/edit?usp=sharing&ouid=115156608576203878334&rtpof=true&sd=true)  
* [Documentatie - Requirements.xlsx](https://docs.google.com/spreadsheets/d/1SpREeF2PAKsFrGvmPEoCK45mChN81q2H/edit?usp=drive_link&ouid=115156608576203878334&rtpof=true&sd=true)

[image1]: <data:image/png;base64,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>

[image2]: <data:image/png;base64,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>