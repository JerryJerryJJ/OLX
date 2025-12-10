# DOKUMENTACJA projektu OLX

## Ogólny opis

**OLX** to prosty serwis ogłoszeniowy zbudowany w technologii **ASP.NET Core MVC** na platformie **.NET 9**.  
Projekt wykorzystuje:

- **ASP.NET Identity** do obsługi kont użytkowników, logowania, rejestracji, ról i banowania,
- **Entity Framework Core** do komunikacji z bazą danych SQL Server,
- **SeedService** do inicjalnego tworzenia ról oraz konta administratora.

System umożliwia dodawanie ogłoszeń, komentowanie, zarządzanie ogłoszeniami, dodawanie produktów do koszyka oraz wykonywanie „zakupów” wraz z generowaniem transakcji (Paragonów).  
Aplikacja posiada panel użytkownika oraz prosty panel administratora (banowanie użytkowników, usuwanie komentarzy i ogłoszeń).

---

## Wymagania

- .NET 9 SDK  
- SQL Server Express / SQL Server / LocalDB  

Opcjonalnie narzędzia:

- `dotnet-ef` — do migracji lokalnych

---

## Struktura projektu (najważniejsze pliki i katalogi)

### **Program.cs**  
Konfiguracja aplikacji, middleware, routing, rejestracja Identity i EF Core, wywołanie SeedService.

### **appsettings.json**  
Konfiguracja aplikacji, m.in. connection string do SQL Server.

### **Data/OLX2Context.cs**  
Kontekst bazy danych aplikacji (DbContext) z definicją encji:

- `Posts`
- `Kosz`
- `Comments`
- `Paragons`  

Dodatkowo dziedziczy po `IdentityDbContext<Users>`.

### **Services/SeedService.cs**  
Serwis odpowiedzialny za seedowanie danych początkowych:

- tworzenie ról (`Admin`, `User`)  
- tworzenie konta administratora

### **Models** — modele domenowe aplikacji

- `Users.cs` — rozszerzenie IdentityUser (FullName, Banned)  
- `Post.cs` — model ogłoszenia (tytuł, opis, cena, zdjęcie, właściciel, status)  
- `Comment.cs` — komentarze pod ogłoszeniami  
- `Kosz.cs` — elementy koszyka użytkownika  
- `Paragon.cs` — encja zakupu / transakcji  

### **Controllers**

#### AccountController.cs
Obsługa użytkownika:

- rejestracja  
- logowanie  
- wylogowanie  
- zmiana hasła (VerifyEmail → ChangePassword)

#### HomeController.cs
Główna logika serwisu:

- wyświetlanie ogłoszeń  
- szczegóły ogłoszenia  
- filtrowanie wyszukiwania  
- dodawanie, edytowanie, usuwanie ogłoszeń  
- dodawanie komentarzy  
- koszyk (+ usuwanie, zakup)  
- generowanie transakcji (Paragonów)  
- panel użytkownika  
- panel administratora (banowanie użytkowników, usuwanie treści)

### **Views/**  
Widoki Razor (layout, strony kontrolerów, partiale do filtrowania postów itd.)

---

## Główne funkcjonalności

### Rejestracja i logowanie użytkowników
Realizowane przez **ASP.NET Core Identity**.  
Model `Users` rozszerza IdentityUser o pola:

- `FullName`  
- `Banned` (użytkownik może być zablokowany przez admina)

### Przeglądanie ogłoszeń
Użytkownicy mogą:

- przeglądać wszystkie ogłoszenia  
- wyświetlać szczegóły  
- filtrować po tytule, cenie, dacie, wyszukiwaniu  
- ukrywać ogłoszenia nieaktualne

### Dodawanie, edytowanie i usuwanie ogłoszeń
Każdy użytkownik może tworzyć własne posty.  
Obsługa zdjęć odbywa się poprzez przechowywanie pliku w formie `byte[]`.

### Komentarze
Użytkownicy mogą dodawać komentarze pod ogłoszeniami innych.  
Admin może usuwać komentarze.

### Koszyk
Użytkownik może:

- dodawać ogłoszenia do koszyka (`Kosz`)  
- usuwać z koszyka  
- zobaczyć wszystkie elementy koszyka

### Zakupy / Paragony
Gdy użytkownik kliknie **Kup**, system:

1. pobiera wszystkie pozycje koszyka użytkownika,  
2. generuje Paragony (`Paragon`) powiązane jednym `TransId`,  
3. oznacza ogłoszenia jako nieaktualne,  
4. czyści koszyk.  

Dostępny jest widok **Transakcje**, gdzie pogrupowane Paragony prezentują historię zakupów.

### Panel użytkownika
Widok zawierający:

- własne ogłoszenia  
- koszyk  
- komentarze

### Panel administratora
Admin może:

- wyświetlać listę wszystkich użytkowników  
- banować / odbanowywać (`Banned`)  
- usuwać komentarze  
- usuwać ogłoszenia dowolnych użytkowników

### Seedowanie bazy
SeedService automatycznie tworzy:

- role: `Admin` i `User`  
- konto administratora: `admin@admin.com / Admin@123`

---

## Technologie i biblioteki

- .NET 9  
- ASP.NET Core MVC  
- ASP.NET Core Identity  
- Entity Framework Core (SQL Server)

---

# HomeController - Funkcje i opis działania

## Konstruktor

**HomeController(ILogger<HomeController> logger, OLX2Context context, UserManager<Users> userManager)**  
Inicjalizuje kontroler i wstrzykuje zależności: logger, kontekst bazy danych i menedżera użytkowników.

---

## Metody kontrolera

### **Index()**
- Wyświetla stronę główną serwisu.
- Nie wymaga autoryzacji.

### **Posty()**
- Wyświetla wszystkie ogłoszenia (`Posts`) wraz z komentarzami (`Comments`) i koszykiem (`Kosz`).
- Wymaga zalogowania użytkownika.
- Zwraca model `PostsWithComments` do widoku.

### **Transakcje()**
- Wyświetla historię transakcji (`Paragons`) pogrupowaną po `TransId`.
- Tworzy widok `TransactionViewModel`.

### **Details(int id)**
- Wyświetla szczegóły jednego ogłoszenia wraz z komentarzami i informacją o koszyku.
- Wymaga zalogowania.
- Jeśli ogłoszenie nie istnieje, zwraca `NotFound()`.

### **Oops()**
- Widok informujący o zablokowanym użytkowniku.
- Dostępny tylko dla ról `User`.

### **Privacy()**
- Standardowy widok polityki prywatności.
- Dostępny tylko dla zalogowanych użytkowników.

### **Admin()**
- Panel administratora pokazujący wszystkich użytkowników.
- Dostępny tylko dla ról `Admin`.

### **Ban(string id)**
- Przełącza status `Banned` wybranego użytkownika.
- Wymaga roli `Admin`.
- Aktualizuje bazę danych i przekierowuje z powrotem do panelu administratora.

### **UserPanel()**
- Panel użytkownika z jego ogłoszeniami, koszykiem i komentarzami.
- Wymaga roli `User`.

### **Kosz()**
- Wyświetla wszystkie pozycje koszyka (`Kosz`) wraz z użytkownikami i ogłoszeniami.
- Widok dla przeglądania koszyka.

### **CommentForm(Comment model)**
- Dodaje nowy komentarz do ogłoszenia.
- Sprawdza, czy użytkownik nie jest zbanowany.  
- Po zapisaniu w bazie, przekierowuje z powrotem do poprzedniej strony.

### **Create(int? id)**
- Tworzy nowe ogłoszenie lub edytuje istniejące (jeśli `id` jest podane).  
- Sprawdza, czy użytkownik nie jest zbanowany lub adminem.
- Zwraca formularz do widoku.

### **DeleteKosz(int id)**
- Usuwa pozycję z koszyka o podanym `id`.
- Przekierowuje z powrotem do poprzedniej strony.

### **Delete(int id)**
- Usuwa ogłoszenie o podanym `id` wraz z powiązanymi elementami w koszyku.
- Przekierowuje do panelu użytkownika.

### **Delete2(int id)**
- Usuwa ogłoszenie oraz powiązane kosze.
- Przekierowuje do listy wszystkich postów (`Posty()`).

### **DeleteComment(int id)**
- Usuwa komentarz o podanym `id`.
- Przekierowuje z powrotem do poprzedniej strony.

### **PostForm(Post model, IFormFile file)**
- Obsługuje formularz tworzenia/edycji ogłoszenia.
- Dodaje lub aktualizuje ogłoszenie w bazie.
- Obsługuje zdjęcia (przechowywane w `byte[]`).
- Sprawdza, czy użytkownik nie jest zbanowany.

### **FilterPosts(string search, string sortBy, string sortOrder, string hideNotActual)**
- Filtrowanie i sortowanie ogłoszeń:
  - wg tytułu, ceny lub daty  
  - opcjonalnie ukrywa ogłoszenia nieaktualne
- Zwraca częściowy widok `_PostsPartial` z filtrowanymi wynikami.

### **KoszForm(Kosz model)**
- Dodaje wybrane ogłoszenie do koszyka użytkownika.
- Sprawdza, czy użytkownik nie jest zbanowany.
- Zapisuje zmiany i wraca do poprzedniej strony.

### **Kup()**
- Realizuje zakup wszystkich elementów w koszyku użytkownika.
- Tworzy Paragony (`Paragon`) powiązane jednym `TransId`.
- Oznacza ogłoszenia jako nieaktualne.
- Usuwa elementy z koszyka.
- Zapisuje zmiany w bazie i przekierowuje do widoku koszyka.

### **Error()**
- Wyświetla widok błędu z identyfikatorem żądania.
- Standardowa metoda obsługi błędów w MVC.

---
