# FileScanner
vytvořeno podle zadání předaném na osobní zchuzce, skládá se ze dvou částí API a webové stránky. 
FileScanner - API část
FileScanner - FE část
Snažil jsem se o co nejuchopitelnější a nejkomplexnější řešení, zároveň jsem se pokoušel řešení udržet lehce pochopitelné pro člověka který to bude procházet
Níže jsou popsané obě části, včetně zmíněných limitů kterých jsem si vědom

## FileScanner
Rest API část
Poožívá swagger pro lehké odskoušení a testování api
API obsahuje následující tři volání 

### api/Scan
nejduležitější volání, jediné které se používá ve webové aplikaci
slouží k předání informací o změnách ve vybrané složce


### api/scan/admin/view-master-library a api/Scan/admin/view-snapshot-by-name
Momentálně jde v podstatě o servisní volání, které jsem používal při testování pro lepší přehlednost. 
Zároveň pokud by nešlo o hypotetický scénář, tak jsou klíčem k rozšíření funkčnost, například k předávání informací nějakému dalšímu systému


## FileScannerUI
webová část aplikace
udělaná velice jednoduše, předpokládal jsem že má jít spíše o nějaký interní jednoduchý nástroj který má být primárně přehledný a rychlí na pochopení.
Oproti zadání jsem si dovolil přidat dvě vylepšení a to možnosti scanovat i podsložky nacházející se ve vybrané složce a rozhodnutí se jestli chce operátor scanovat i skryté soubory


## Další důležité informace

### Logování
případné chyby které vzniknou při volání se zobrazují uživately jak na straně webové aplikace tak se ukládají do složky logs na straně API

### Limity mého řešení
1. webová aplikace určitě nevyhraje soutěž krásy. V ideálním světě bych ji udělal určitě hezčí, na druhou stranu i přes ten minimalistický vzhled by měla být přehledná a dělat vše co se od ní požaduje
2. Duplicitnost - vzhledem k mému rozhodnutí do scanování zahrnout i podsložky dochází k situaci, že když člověk nascanuje nějakou složku a pak pustí scan na její podsložce, program ji bere jako samostatnou entitu a vytvoří si pro ni vlastní verzovací soubor. Tomu by se dalo zabránit kdybych rekurzivně řešil dědičnost u jednotlivých objektů a kaskádově je mezi sebou provolával, to by ale mohlo dost spomalit výkon. Vzhledem k tomu, že jsem chtěl aby výpočty netrvali dlouho rozhodl jsem se jít současným způsobem řešení

### Ukládání informací
informace se ukládají do json souborů. Existuje jeden hlavní soubor nazvaný MasterLibrary, který drží informace o všech doposud nascanovaných složkách. 
Ve složce Library se pak nachází jednotlivé json soubory které drží informace o jednotlivých nascanovaných entitách

