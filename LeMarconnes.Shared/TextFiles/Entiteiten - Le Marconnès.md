# Entiteiten \- Le Marconnès

Eventueel nog toevoegen:

- Borg

Nakijken:

- Is btw standaard goed genoeg?  
- Check-in/uit

# 

# Inhoudsopgave {#inhoudsopgave}

[**Inhoudsopgave	2**](#inhoudsopgave)

[**1\. Inleiding	3**](#1.-inleiding)

[1.1 Aanleiding & Doelstelling	3](#1.1-aanleiding-&-doelstelling)

[1.2 Projectcontext & Stakeholders	3](#1.2-projectcontext-&-stakeholders)

[1.3 Scope van de Analyse	3](#1.3-scope-van-de-analyse)

[**2\. De Bron van Waarheid (Business Context)	4**](#2.-de-bron-van-waarheid-\(business-context\))

[2.1 Algemene Bedrijfsgegevens & Seizoenen	4](#2.1-algemene-bedrijfsgegevens-&-seizoenen)

[2.2 Hotel: Structuur & Capaciteit	5](#2.2-hotel:-structuur-&-capaciteit)

[2.3 Gîte: Het Hybride Verhuurmodel	5](#2.3-gîte:-het-hybride-verhuurmodel)

[2.4 Camping: Componenten & Variabelen	6](#2.4-camping:-componenten-&-variabelen)

[2.5 Restaurant & Faciliteiten	7](#2.5-restaurant-&-faciliteiten)

[**3\. Financiële Logica & Tarieven	8**](#3.-financiële-logica-&-tarieven)

[3.1 Prijsmodellen per Accommodatietype	8](#3.1-prijsmodellen-per-accommodatietype)

[3.2 Belastingen & Toeslagen (Toeristenbelasting Logica)	9](#3.2-belastingen-&-toeslagen-\(toeristenbelasting-logica\))

[3.3 Platformen & Commissies (Channel Management)	9](#3.3-platformen-&-commissies-\(channel-management\))

[3.4 Betalingsstromen & "Op Rekening" Logica (Oplossing voor US\_G021)	10](#3.4-betalingsstromen-&-"op-rekening"-logica-\(oplossing-voor-us_g021\))

[**4\. Identificatie van de Entiteiten (De Lijst)	11**](#4.-identificatie-van-de-entiteiten-\(de-lijst\))

[4.1 Actoren (Personen, Accounts & Rollen)	11](#4.1-actoren-\(personen,-accounts-&-rollen\))

[4.2 Inventaris & Producten (De Assets):	12](#4.2-inventaris-&-producten-\(de-assets\):)

[4.3 Prijzen & Configuratie	12](#4.3-prijzen-&-configuratie)

[4.4 Proces & Transacties (Dynamische Data)	13](#4.4-proces-&-transacties-\(dynamische-data\))

[**5\. Complexe Vraagstukken & Oplossingen	14**](#5.-complexe-vraagstukken-&-oplossingen)

[5.1 Het Gîte Dilemma: Synchronisatie van Beschikbaarheid	14](#5.1-het-gîte-dilemma:-synchronisatie-van-beschikbaarheid)

[5.2 Gast vs. Account: Omgaan met Externe Boekers	14](#5.2-gast-vs.-account:-omgaan-met-externe-boekers)

[5.3 Historische Data: Prijswijzigingen over jaren heen	15](#5.3-historische-data:-prijswijzigingen-over-jaren-heen)

[**6\. Data-Scope & Beperkingen	16**](#6.-data-scope-&-beperkingen)

[6.1 Personeel & HR	16](#6.1-personeel-&-hr)

[6.2 Voorraadbeheer & Inkoop	16](#6.2-voorraadbeheer-&-inkoop)

[6.3 Technische API Afhandeling	16](#6.3-technische-api-afhandeling)

# 1\. Inleiding {#1.-inleiding}

## 1.1 Aanleiding & Doelstelling {#1.1-aanleiding-&-doelstelling}

Le Marconnès is een groeiend recreatiebedrijf in de Franse Auvergne, bestaande uit vier bedrijfstakken: een hotel, een gîte (vakantieappartement), een camping en een restaurant. De huidige bedrijfsvoering, geleid door eigenaren Elvire en Ed, loopt tegen limieten aan wat betreft administratief overzicht en efficiëntie. Er is behoefte aan centralisatie van gegevens om inzicht te krijgen in reserveringen, financiën en gastgegevens.

Het doel van dit document is het vastleggen van de volledige informatiebehoefte en het identificeren van de functionele entiteiten die nodig zijn voor het nieuwe digitale beheersysteem. Dit document fungeert als de "Bron van Waarheid" waarop het technische database-ontwerp gebaseerd zal worden.

## 1.2 Projectcontext & Stakeholders {#1.2-projectcontext-&-stakeholders}

De eigenaren zijn digitaal minder vaardig (geen ICT'ers) en vereisen een systeem dat complexiteit aan de achterkant oplost, zodat de voorkant gebruiksvriendelijk blijft.

* **De Opdracht:** Het ontwerpen van een database die zowel de starre structuur van het hotel, de hybride verhuur van de gîte, als de flexibele componenten van de camping **ondersteunt.**  
* **De Uitdaging:** Het systeem moet rekening houden met boekingen via verschillende kanalen (Eigen website, Booking.com, Airbnb), variërende belastingregels en historische prijsfluctuaties.

## 1.3 Scope van de Analyse {#1.3-scope-van-de-analyse}

Dit document beperkt zich tot de functionele data-analyse en het identificeren van de informatiebehoefte. De focus ligt op de vertaling van de bedrijfsprocessen van Le Marconnès (Hotel, Gîte, Camping, Restaurant) naar conceptuele entiteiten en bedrijfsregels.

**De scope omvat:**

* Het inventariseren van alle relevante 'zelfstandige naamwoorden' (entiteiten) uit de casusbeschrijving en interviews.  
* Het vastleggen van de relaties tussen deze entiteiten op basis van bedrijfslogica (bijv. "Een reservering heeft één gast").  
* Het definiëren van de specifieke attributen die nodig zijn om aan de rapportage- en facturatie-eisen te voldoen.

# 

# 2\. De Bron van Waarheid (Business Context) {#2.-de-bron-van-waarheid-(business-context)}

Dit hoofdstuk centraliseert alle bedrijfsgegevens, capaciteiten en operationele regels van Le Marconnès. Deze informatie is geaggregeerd uit de casusbeschrijvingen en interviews met de eigenaren en dient als de basis voor de datamodellering.

## 2.1 Algemene Bedrijfsgegevens & Seizoenen {#2.1-algemene-bedrijfsgegevens-&-seizoenen}

Le Marconnès is gevestigd in een 16e-eeuwse boerderij in **St. Arcons de Barges (Auvergne, Frankrijk)** en combineert vier bedrijfsactiviteiten op één locatie: **Hotel**, **Gîte**, **Camping** en **Restaurant**.

**Operationele Kaders**

| Onderwerp | Detail | Toelichting |
| :---- | :---- | :---- |
| **Locatie** | St. Arcons de Barges (Auvergne, Frankrijk) | Relevant voor **fiscale regelgeving** (Taxe de Séjour). |
| **Bedrijfsactiviteiten** | Hotel, Gîte, Camping, Restaurant | Vier gecombineerde activiteiten op één locatie. |
| **Seizoen** | 1 maart t/m 31 oktober | Geldt voor Hotel & Bar (aanname: gehele terrein). |
| **Beheer** | Elvire & Ed | Eigenaren voeren operationeel beheer en hebben volledige rechten (overschrijven). |

## 

## 2.2 Hotel: Structuur & Capaciteit {#2.2-hotel:-structuur-&-capaciteit}

Het hotelgedeelte bestaat uit vaste, private accommodatie-eenheden met eigen sanitair.

| Aantal Kamers | Type Kamer | Capaciteit (p.p.) | Totaal Slaapplaatsen |
| :---- | :---- | :---- | :---- |
| 2 | Tweepersoons | 2 | 4 |
| 3 | Vierpersoons | 4 | 12 |
| 1 | Vijfpersoons | 5 | 5 |
| **6** | **Totaal** |  | **21** |
| **Fiscale Status** | Exclusief toeristenbelasting | Toeristenbelasting wordt **apart berekend**. |  |

## 2.3 Gîte: Het Hybride Verhuurmodel {#2.3-gîte:-het-hybride-verhuurmodel}

De Gîte (appartement) beschikt over 4 slaapkamers, 2 badkamers en gemeenschappelijke leefruimtes. De operationele capaciteit is vastgesteld op 9 slaapplaatsen. Vanwege de verhuur via verschillende kanalen (Booking.com & Airbnb), hanteren we twee modellen die de beschikbaarheid wederzijds beïnvloeden.

| Kenmerk | Model A: Gehele Accommodatie | Model B: Losse Slaapplaatsen |
| :---- | :---- | :---- |
| **Operationele Capaciteit** | 9 slaapplaatsen | 9 slaapplaatsen |
| **Concept** | Privé verhuur van het volledige appartement. | Gedeelde verhuur (**Hostel-model**) per bed. |
| **Kanaal** | Primair Booking.com / Eigen site. | Primair Airbnb. |
| **Tariefstructuur** | Vaste eenheidsprijs (€ 200,-). | Prijs per persoon (€ 27,50). |
| **Beschikbaarheid** | Blokkeert **alle 9 slaapplaatsen** én Model B. | Blokkeert Model A; andere bedden blijven beschikbaar. |
| **Fiscale Status** | Inclusief toeristenbelasting | Inclusief toeristenbelasting |

## 2.4 Camping: Componenten & Variabelen {#2.4-camping:-componenten-&-variabelen}

De camping werkt met een stapelbaar tariefmodel. Een reservering is geen vast bedrag, maar een som van losse componenten.

| Categorie | Component | Type Kosten |
| :---- | :---- | :---- |
| **Basis** | Kampeerplaats | Vast bedrag per nacht. |
| **Gasten** | Volwassene | Variabel per persoon. |
|  | Kind (7-12 jaar) | Variabel per kind. |
|  | Kind (0-7 jaar) | Variabel per kind. |
| **Opties** | Elektriciteit | Optionele toeslag. |
|  | Hond | Optionele toeslag. |
| **Informatiebehoefte** | Exacte gezinssamenstelling | Vereist bij boeking. |
| **Fiscale Status** | Exclusief toeristenbelasting | Toeristenbelasting wordt **apart berekend**. |

## 

## 2.5 Restaurant & Faciliteiten {#2.5-restaurant-&-faciliteiten}

Het restaurant en de faciliteiten ondersteunen de verblijfsgasten, maar functioneren ook deels zelfstandig.

**Restaurant**

| Onderwerp | Detail |
| :---- | :---- |
| **Capaciteit** | Maximaal **30 couverts** per avond. |
| **Diensten** | Ontbijt, Lunch(pakket), Diner. |
| **Betaling** | **Direct afrekenen** OF **op rekening van accommodatie**. |

**Faciliteiten**

| Faciliteit | Status | Aanname/Toelichting |
| :---- | :---- | :---- |
| **Zwembad** | Gratis | Voor Hotel- en Gîte-gasten (Aanname: ook Camping). |
| **Sauna** | Betaald |  |
| **Wifi** | Gratis |  |
| **Wandelkaarten** | Gratis |  |

# 3\. Financiële Logica & Tarieven {#3.-financiële-logica-&-tarieven}

Dit hoofdstuk definieert de bedrijfsregels omtrent prijsvorming, belastingen en geldstromen. Gezien de wens van de eigenaren om historisch inzicht te krijgen in de omzet, worden alle tarieven en belastingen **tijdsgebonden** (historisch) opgeslagen.

## 3.1 Prijsmodellen per Accommodatietype {#3.1-prijsmodellen-per-accommodatietype}

Om de uiteenlopende verhuurvormen in één database te vangen, onderscheiden we drie fundamentele prijsmodellen.

| Model | Toepassing | Logica | Prijsformule (Concept) |
| :---- | :---- | :---- | :---- |
| **1\. Eenheidsprijs** | Hotel, Gîte (Model A) | Prijs per nacht voor de fysieke eenheid. Bij Hotel afhankelijk van bezettingsgraad (1-5 pers), maar gekoppeld aan de kamer. | Basisprijs\_Bij\_Bezetting \* Aantal\_Nachten |
| **2\. Capaciteitsprijs** | Gîte (Model B) | Prijs per persoon per nacht (Hostel-model). | (Prijs\_p.p. \* Aantal\_Personen) \* Aantal\_Nachten |
| **3\. Gestapeld Tarief** | Camping | Som van basisplaats \+ losse componenten per nacht. | (Basisplaats \+ (Volw \* Prijs) \+ (Kind \* Prijs) \+ Opties) \* Nachten |

***Bedrijfsregel:***   
Tarieven zijn altijd gekoppeld aan een **geldigheidsperiode** (DatumVan \- DatumTot). Een boeking in 2026 mag niet automatisch de prijzen van 2025 gebruiken.

## 

## 3.2 Belastingen & Toeslagen (Toeristenbelasting Logica) {#3.2-belastingen-&-toeslagen-(toeristenbelasting-logica)}

Er zijn tegenstrijdige bronnen over het exacte tarief (€ 0,25 vs € 0,50) en de berekeningswijze (inclusief/exclusief). Om risico's uit te sluiten, wordt de belastingmodule **flexibel** opgezet.

Gezien de tegenstrijdige broninformatie is verificatie bij de opdrachtgever vereist. In dit ontwerp gaan we uit van een configureerbaar tarief om beide scenario's te ondersteunen.

**Berekeningsregels per Type**

| Situatie | Type Accommodatie | Rekenmethode |
| :---- | :---- | :---- |
| Inclusief | Gîte (Booking/Airbnb) | De opgeslagen prijs is de bruto-prijs. De belasting wordt op de factuur administratief uitgesplitst (reverse calculation). |
| Exclusief | Hotel & Camping | De belasting wordt bovenop de kale huursom berekend. |

**Formule:** Aantal\_Belastingplichtigen \* Tarief\_Per\_Nacht \* Aantal\_Nachten.  
**Flexibiliteit:**   
Het tarief is instelbaar per seizoen en per accommodatietype, zodat wijzigingen in wetgeving direct doorgevoerd kunnen worden zonder code-aanpassingen.

## 3.3 Platformen & Commissies (Channel Management) {#3.3-platformen-&-commissies-(channel-management)}

Omdat netto-inkomsten variëren per verkoopkanaal, wordt elk tarief gekoppeld aan een Platform. Dit stelt de eigenaren in staat om te rapporteren op bruto-omzet versus netto-winst.

* **Identificatie:** Een reservering bevat altijd een Bron (Eigen Site, Booking.com, Airbnb, Telefonisch/Balie).  
* **Commissie:** Elk platform heeft een commissiepercentage of vast bedrag. Dit wordt berekend over de kale huursom (excl. toeristenbelasting).  
* **Doel:** De eigenaren moeten in hun rapportage kunnen zien: *"Wat is de omzet vóór commissie en wat is de netto winst?"*

## 

## 3.4 Betalingsstromen & "Op Rekening" Logica (Oplossing voor US\_G021) {#3.4-betalingsstromen-&-"op-rekening"-logica-(oplossing-voor-us_g021)}

Om te voldoen aan de eis om restaurantkosten te koppelen aan een verblijf, hanteert het systeem het concept van een "Master Rekening".

**De "Gezamenlijke Rekening" Logica:**

1. **De 'Master' Rekening:** Elke Verblijfsreservering (Hotel/Gîte/Camping) creëert automatisch een financiële container: de Rekening.  
2. **Koppeling:** Een restaurantbestelling (of bar-transactie) kan twee statussen hebben:  
   * *Direct Betaald:* Transactie wordt direct afgesloten (Passant).  
   * *Op Rekening:* De bestelling wordt gekoppeld aan het ReserveringID van de gast.  
3. **Facturatie:** Bij het uitchecken genereert het systeem één factuur die alle regels bevat:  
   * Regel 1: Verblijfskosten (Accommodatie).  
   * Regel 2: Toeristenbelasting.  
   * Regel 3: Gekoppelde Restaurant/Bar bestellingen.

# 

# 4\. Identificatie van de Entiteiten (De Lijst) {#4.-identificatie-van-de-entiteiten-(de-lijst)}

In dit hoofdstuk vertalen we de bedrijfscontext en financiële logica naar concrete informatie-objecten. De onderstaande entiteiten vormen de functionele blauwdruk voor de tabellen in het uiteindelijke database-ontwerp.

## 4.1 Actoren (Personen, Accounts & Rollen) {#4.1-actoren-(personen,-accounts-&-rollen)}

We maken een strikt onderscheid tussen de persoon (de identiteit voor het nachtregister) en het account (de toegang tot het systeem).

| Entiteit | Omschrijving & Doel | Rationale voor Ontwerp |
| :---- | :---- | :---- |
| **GEBRUIKER** | Supertype voor inloggegevens (Email, WachtwoordHash, Rol). | Centraliseert authenticatie. Essentieel voor security en logging. |
| **GAST** | Volledige NAW-gegevens (Straat, Huisnummer, Postcode, Woonplaats, Land), Telefoonnummer, IBAN (optioneel) en Geboortedatum. | **Losgekoppeld van Account:** Noodzakelijk omdat externe boekers (Booking.com) geen account hebben (want ze hebben deze al op booking.com en airbnb), maar wel geregistreerd moeten worden (Nachtregister).Adresgegevens zijn verplicht voor de facturatie en het wettelijke nachtregister (Frankrijk). IBAN wordt opgeslagen voor eventuele restituties of automatische incasso, maar is niet verplicht. |

**Rollen binnen Gebruiker (Realisatie met attributen)**  
Deze rollen bepalen de rechten binnen de applicatie, maar zijn geen aparte entiteiten.

| Medewerker | Personeel dat operationele taken uitvoert. | Gekoppeld aan GEBRUIKER voor toegang en audit trails. |
| :---- | :---- | :---- |
| **Eigenaar** | Specifieke beheerdersrol. | Heeft unieke rechten om prijzen en blokkades te overschrijven (business rule). |
| **Gast-met-account** | Een gast die via de eigen website heeft geboekt en een login heeft. | Biedt self-service functionaliteit (inzien reservering, factuur en betaalstatus). Koppelt een GEBRUIKER-record aan een GAST-record. |

## 4.2 Inventaris & Producten (De Assets): {#4.2-inventaris-&-producten-(de-assets):}

De fysieke en boekbare objecten van Le Marconnès.

| Entiteit | Omschrijving & Doel | Rationale voor Ontwerp |
| :---- | :---- | :---- |
| **ACCOMMODATIE\_TYPE** | Categorie-definitie (Hotelkamer, Gîte-Geheel, Slaapplek, Kampeerplaats). | Nodig om de verschillende prijslogica's (per stuk, per persoon, gestapeld) technisch te scheiden. |
| **VERHUUR\_EENHEID** | De fysieke, boekbare asset (bijv. "Kamer 1", "Plek A", "De Gîte"). | Bevat de *vaste* eigenschappen zoals naam en maximale capaciteit. |
| **SLAAPPLEK** | Specifieke sub-entiteit van de Gîte. | **Essentieel:** Maakt het Airbnb-model (verhuur per bed) mogelijk zonder de bovenliggende Gîte-eenheid dubbel te boeken. |
| **PRODUCT** | Verkoopbare items (Horeca: Cola, Pizza) en Services (Linnenpakket). | Bevat de naam, basisprijs en BTW-categorie voor facturatie op de bon. |

## 4.3 Prijzen & Configuratie {#4.3-prijzen-&-configuratie}

De bedrijfsregels en instellingen die bepalen hoeveel een transactie kost.

| Entiteit | Omschrijving & Doel | Rationale voor Ontwerp |
| :---- | :---- | :---- |
| **PLATFORM** | Verkoopkanaal (Eigen Site, Booking.com, Airbnb). | Definieert het commissiepercentage per boeking voor netto-winst rapportages. |
| **TARIEF** | De historische prijstabel (Prijs X in Jaar Y). | **Cruciaal:** Koppelt een bedrag aan een Platform, Type, Categorie en Geldigheidsperiode. |
| **TARIEF\_CATEGORIE** | Het soort kosten (Logies, Hond, Elektra, Toeristenbelasting). | Nodig voor de gespecificeerde factuur en de stapelbare campingprijzen. |

## 4.4 Proces & Transacties (Dynamische Data) {#4.4-proces-&-transacties-(dynamische-data)}

De daadwerkelijke boekingen, verkopen en geldstromen.

| Entiteit | Omschrijving & Doel | Rationale voor Ontwerp |
| :---- | :---- | :---- |
| **RESERVERING** | Kernkoppeling tussen Gast, Eenheid en Periode. | Bevat status (Gereserveerd, Ingecheckt, Geannuleerd) en Platform-bron. |
| **RESERVERING\_DETAIL** | Specificatie van de inhoud (Aantal volw/kind/hond). | Essentieel voor Camping-prijsberekening en Hotel-bezetting checks. |
| **BESTELLING** | Horeca-transactie (Restaurant/Bar). | Kan gekoppeld zijn aan een RESERVERING (op rekening) of NULL zijn (direct betaald). |
| **BESTEL\_REGEL** | Koppelt **PRODUCT** aan **BESTELLING** met aantal. | Detailniveau van de bon (bijv. 2x Cola, 1x Pizza). |
| **FACTUUR** | Het financiële einddocument (Momentopname). | Voldoet aan wettelijke eisen, bevat BTW-specificatie. |
| **BETALING** | Registratie van geldstromen. | Om aanbetalingen en deelbetalingen bij te houden (Saldo \= Factuur \- Betalingen). |
| **LOGBOEK** | Registratie van mutaties (Wie, Wat, Wanneer). | Harde eis voor veiligheid en auditing (S-NF-SYS-003). |

# 

# 5\. Complexe Vraagstukken & Oplossingen {#5.-complexe-vraagstukken-&-oplossingen}

In dit hoofdstuk beschrijven we de bedrijfslogica die de statische entiteiten (Hoofdstuk 4\) met elkaar verbindt. Deze regels zorgen ervoor dat het systeem om kan gaan met de unieke en complexe scenario's van Le Marconnès.

## 5.1 Het Gîte Dilemma: Synchronisatie van Beschikbaarheid {#5.1-het-gîte-dilemma:-synchronisatie-van-beschikbaarheid}

Het systeem moet garanderen dat de Gîte nooit dubbel geboekt wordt via de verschillende kanalen, ondanks de twee verhuurmodellen (Booking.com vs. Airbnb).

**Oplossing: Hiërarchische Blokkades**  
De database modelleert de Gîte als een hiërarchie: 1 Parent-eenheid (Het Appartement) en 9 Child-eenheden (De Slaapplekken).

| Scenario | Actie van Gast | Systeemreactie (Logica) |
| :---- | :---- | :---- |
| **A. Hele Gîte** | Boekt de **Parent**\-eenheid. | 1\. Blokkeert de Parent-eenheid.  2\. Blokkeert automatisch alle 9 Child-eenheden (Slaapplekken). |
| **B. Slaapplek** | Boekt 1 of meer **Child**\-eenheden. | 1\. Blokkeert de specifieke Child-eenheid(en). 2\. Blokkeert automatisch de Parent-eenheid (want niet meer als geheel beschikbaar). 3\. Laat de overige Child-eenheden beschikbaar. |

## 5.2 Gast vs. Account: Omgaan met Externe Boekers {#5.2-gast-vs.-account:-omgaan-met-externe-boekers}

De eis "Een gast moet een account hebben" (S-NF-RES-001) conflicteert met de realiteit dat gasten van Booking.com of Airbnb niet inloggen op de Marconnès-website.

* **De Oplossing in Data:**  
  * We splitsen identiteit (GAST) van toegang (GEBRUIKER).  
  * *Directe Boekers:* Hebben een GAST-record én een GEBRUIKER-record (account).  
  * *Externe Boekers:* Het systeem maakt automatisch een GAST-record aan op basis van de import-data (voor het nachtregister en de factuur), maar maakt **geen** GEBRUIKER-record aan. Hierdoor blijft de database zuiver en veilig (geen geforceerde wachtwoorden).

## 5.3 Historische Data: Prijswijzigingen over jaren heen {#5.3-historische-data:-prijswijzigingen-over-jaren-heen}

De eigenaren willen inzicht in de financiële prestaties over de jaren heen. Een vaste prijskolom bij een accommodatie volstaat niet.

* **De Oplossing in Data:**  
  * Prijzen worden niet opgeslagen bij de VERHUUR\_EENHEID of het PRODUCT, maar in een aparte TARIEF tabel.  
  * Elk tarief heeft een GeldigVan en GeldigTot datum.  
  * Bij het maken van een RESERVERING of FACTUUR haalt het systeem het tarief op dat geldig was op het moment van de transactie (of de verblijfsdatum, afhankelijk van de bedrijfsregel). Dit garandeert dat rapportages over 2024 accuraat blijven, ook als de prijzen in 2025 stijgen.

# 

# 6\. Data-Scope & Beperkingen {#6.-data-scope-&-beperkingen}

Om de focus te leggen op de kernfunctionaliteit (het reserverings- en gastenbeheer) en de haalbaarheid te waarborgen, zijn de volgende datadomeinen bewust buiten beschouwing gelaten in dit entiteitenmodel.

## 6.1 Personeel & HR {#6.1-personeel-&-hr}

De entiteit **MEDEWERKER** bevat enkel gegevens die noodzakelijk zijn voor **authenticatie** (inloggen) en **autorisatie** (rechten/logging).

* **Buiten scope:** Salarisadministratie, BSN-nummers, contractbeheer en dienstroosters.  
* **Rationale:** De casus vermeldt het gebruik van externe HR-software (met JSON-export). Wij modelleren het doelsysteem (OLTP) voor de reserveringen, niet de salarisadministratie.

## 6.2 Voorraadbeheer & Inkoop {#6.2-voorraadbeheer-&-inkoop}

Het model ondersteunt de verkoopkant van het restaurant via de entiteiten **PRODUCT** en **BESTELLING** ten behoeve van de facturatie.

* **Buiten scope:** Voorraadbeheer van ingrediënten (stock), inkoopprijzen van grondstoffen en recepturen.  
* **Rationale:** Het doel van het systeem is omzetregistratie en gastgemak (alles op één factuur), niet het logistieke beheer van de keuken.

## 6.3 Technische API Afhandeling {#6.3-technische-api-afhandeling}

Het datamodel faciliteert de opslag van boekingen uit externe bronnen via de entiteiten **PLATFORM** en **RESERVERING** (met bronvermelding).

* **Buiten scope:** De technische API-logica, synchronisatie-algoritmes en "webhooks" die de data daadwerkelijk ophalen bij Booking.com of Airbnb.  
* **Rationale:** Dit document beschrijft de **datastructuur** (de opslag), niet de applicatielogica of connectiviteit.

