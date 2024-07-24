// https://imansadati.com
// Thank you for attention ...


public interface IUserService
{
    void Register(User user);
    User Login(string username, string password);
}

public interface IBookService
{
    void AddBook(Book book);
    void RemoveBook(int bookId);
    void UpdateBook(Book book);
    List<Book> ListBooks();
    List<Book> SearchBooks(string query);
}

public interface IMemberService
{
    void AddMember(Member member);
    void RemoveMember(int memberId);
    void UpdateMember(Member member);
    List<Member> ListMembers();
}

public interface IBorrowService
{
    void BorrowBook(int bookId, int memberId);
    void ReturnBook(int bookId, int memberId);
}

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}

public class Member
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public string ISBN { get; set; }
    public bool IsAvailable { get; set; } = true;
}

public class UserService : IUserService
{
    private List<User> users = new List<User>();

    public User Login(string username, string password)
    {
        return users.FirstOrDefault(u => u.Username == username && u.Password == password);
    }

    public void Register(User user)
    {
        users.Add(user);
    }
}

public class MemberService : IMemberService
{
    private List<Member> members = new List<Member>();
    public void AddMember(Member member)
    {
        members.Add(member);
    }

    public List<Member> ListMembers() => members;

    public void RemoveMember(int memberId)
    {
        members.RemoveAll(m => m.Id == memberId);
    }

    public void UpdateMember(Member member)
    {
        var existingMember = members.FirstOrDefault(m => m.Id == member.Id);
        if (existingMember != null)
        {
            existingMember.Name = member.Name;
            existingMember.Email = member.Email;
        }
    }
}

public class BookService : IBookService
{
    private List<Book> books = new List<Book>();
    public void AddBook(Book book)
    {
        books.Add(book);
    }

    public List<Book> ListBooks() => books;

    public void RemoveBook(int bookId)
    {
        books.FirstOrDefault(b => b.Id == bookId);
    }

    public List<Book> SearchBooks(string query)
    {
        return books.Where(b => b.Title.Contains(query) || b.Author.Contains(query) || b.ISBN.Contains(query)).ToList();
    }

    public void UpdateBook(Book book)
    {
        var existingBook = books.FirstOrDefault(b => b.Id == book.Id);
        if (existingBook != null)
        {
            existingBook.Title = book.Title;
            existingBook.Author = book.Author;
            existingBook.ISBN = book.ISBN;
            existingBook.IsAvailable = book.IsAvailable;
        }
    }
}

public class BorrowService : IBorrowService
{
    private List<Book> books = new List<Book>();
    private List<Member> members = new List<Member>();

    public BorrowService(List<Book> books, List<Member> members)
    {
        this.books = books;
        this.members = members;
    }

    public void BorrowBook(int bookId, int memberId)
    {
        var book = books.FirstOrDefault(b => b.Id == bookId);
        var member = members.FirstOrDefault(m => m.Id == memberId);

        if (book != null && member != null && book.IsAvailable)
        {
            book.IsAvailable = false;
            Console.WriteLine($"Book '{book.Title}' borrowed by {member.Name}");
        }
        else
        {
            Console.WriteLine("Borrowing failed. Book may not be available or member not found.");
        }
    }

    public void ReturnBook(int bookId, int memberId)
    {
        var book = books.FirstOrDefault(b => b.Id == bookId);
        var member = members.FirstOrDefault(m => m.Id == memberId);

        if (book != null && member != null && !book.IsAvailable)
        {
            book.IsAvailable = true;
            Console.WriteLine($"Book '{book.Title}' returned by {member.Name}");
        }
        else
        {
            Console.WriteLine("Returning failed. Book may not be borrowed or member not found.");
        }
    }
}

class Program
{
    private static IUserService userService = new UserService();
    private static IBookService bookService = new BookService();
    private static IBorrowService borrowService;
    private static IMemberService memberService = new MemberService();
    private static User currentUser;

    static void Main(string[] args)
    {
        borrowService = new BorrowService(((BookService)bookService).ListBooks(), ((MemberService)memberService).ListMembers());
        ShowMainMenu();
    }

    private static void ShowMainMenu()
    {
        while (true)
        {
            Console.WriteLine("\nLibrary Management System");
            Console.WriteLine("1. Register");
            Console.WriteLine("2. Login");
            Console.WriteLine("3. Exit");
            Console.Write("Enter your choice: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Register();
                    break;
                case "2":
                    Login();
                    break;
                case "3":
                    return;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }

    static void Register()
    {
        Console.Write("Enter username: ");
        var username = Console.ReadLine();
        Console.Write("Enter password: ");
        var password = Console.ReadLine();

        var user = new User
        {
            Id = new Random().Next(1, 1000),
            Username = username,
            Password = password
        };
        userService.Register(user);
        Console.WriteLine("Registration successful. You can now login.");
    }

    static void Login()
    {
        Console.Write("Enter username: ");
        var username = Console.ReadLine();
        Console.Write("Enter password: ");
        var password = Console.ReadLine();

        currentUser = userService.Login(username, password);
        if (currentUser != null)
        {
            ShowUserMenu();
        }
        else
        {
            Console.WriteLine("Invalid username or password. Please try again.");
        }
    }

    static void ShowUserMenu()
    {
        while (true)
        {
            Console.WriteLine("\nUser Menu:");
            Console.WriteLine("1. Add Book");
            Console.WriteLine("2. Remove Book");
            Console.WriteLine("3. Update Book");
            Console.WriteLine("4. List Books");
            Console.WriteLine("5. Search Books");
            Console.WriteLine("6. Add Member");
            Console.WriteLine("7. Remove Member");
            Console.WriteLine("8. Update Member");
            Console.WriteLine("9. List Members");
            Console.WriteLine("10. Borrow Book");
            Console.WriteLine("11. Return Book");
            Console.WriteLine("12. Logout");
            Console.Write("Enter your choice: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    AddBook();
                    break;
                case "2":
                    RemoveBook();
                    break;
                case "3":
                    UpdateBook();
                    break;
                case "4":
                    ListBooks();
                    break;
                case "5":
                    SearchBooks();
                    break;
                case "6":
                    AddMember();
                    break;
                case "7":
                    RemoveMember();
                    break;
                case "8":
                    UpdateMember();
                    break;
                case "9":
                    ListMembers();
                    break;
                case "10":
                    BorrowBook();
                    break;
                case "11":
                    ReturnBook();
                    break;
                case "12":
                    return;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }

    static void AddBook()
    {
        Console.Write("Enter book title: ");
        var title = Console.ReadLine();
        Console.Write("Enter book author: ");
        var author = Console.ReadLine();
        Console.Write("Enter book ISBN: ");
        var isbn = Console.ReadLine();

        var book = new Book
        {
            Id = new Random().Next(1, 1000),
            Title = title,
            Author = author,
            ISBN = isbn,
        };
        bookService.AddBook(book);
        Console.WriteLine("Book added successfully.");
    }

    static void RemoveBook()
    {
        Console.Write("Enter book ID to remove: ");
        var id = int.Parse(Console.ReadLine());
        bookService.RemoveBook(id);
        Console.WriteLine("Book removed successfully.");
    }

    static void UpdateBook()
    {
        Console.Write("Enter book ID to update: ");
        var id = int.Parse(Console.ReadLine());
        Console.Write("Enter new title: ");
        var title = Console.ReadLine();
        Console.Write("Enter new author: ");
        var author = Console.ReadLine();
        Console.Write("Enter new ISBN: ");
        var isbn = Console.ReadLine();
        Console.Write("Is the book available? (yes/no): ");
        var isAvailable = Console.ReadLine().ToLower() == "yes";

        var book = new Book
        {
            Id = id,
            Title = title,
            Author = author,
            ISBN = isbn,
            IsAvailable = isAvailable
        };
        bookService.UpdateBook(book);
        Console.WriteLine("Book updated successfully.");
    }

    static void ListBooks()
    {
        var books = bookService.ListBooks();
        foreach (var book in books)
        {
            Console.WriteLine($"ID: {book.Id}, Title: {book.Title}, Author: {book.Author}, ISBN: {book.ISBN}, Available: {book.IsAvailable}");
        }
    }

    static void SearchBooks()
    {
        Console.Write("Enter search query (title/author/ISBN): ");
        var query = Console.ReadLine();
        var books = bookService.SearchBooks(query);
        foreach (var book in books)
        {
            Console.WriteLine($"ID: {book.Id}, Title: {book.Title}, Author: {book.Author}, ISBN: {book.ISBN}, Available: {book.IsAvailable}");
        }
    }

    static void ReturnBook()
    {
        Console.Write("Enter member ID: ");
        var memberId = int.Parse(Console.ReadLine());
        Console.Write("Enter book ID: ");
        var bookId = int.Parse(Console.ReadLine());

        borrowService.ReturnBook(memberId, bookId);
    }

    static void BorrowBook()
    {
        Console.Write("Enter member ID: ");
        var memberId = int.Parse(Console.ReadLine());
        Console.Write("Enter book ID: ");
        var bookId = int.Parse(Console.ReadLine());

        borrowService.BorrowBook(memberId, bookId);
    }

    static void ListMembers()
    {
        var members = memberService.ListMembers();
        foreach (var member in members)
        {
            Console.WriteLine($"ID: {member.Id}, Name: {member.Name}, Email: {member.Email}");
        }
    }

    static void UpdateMember()
    {
        Console.Write("Enter member ID to update: ");
        var id = int.Parse(Console.ReadLine());
        Console.Write("Enter new name: ");
        var name = Console.ReadLine();
        Console.Write("Enter new email: ");
        var email = Console.ReadLine();

        var member = new Member
        {
            Id = id,
            Name = name,
            Email = email
        };

        memberService.UpdateMember(member);
        Console.WriteLine("Member updated successfully.");
    }

    static void RemoveMember()
    {
        Console.Write("Enter member ID to remove: ");
        var id = int.Parse(Console.ReadLine());
        memberService.RemoveMember(id);
        Console.WriteLine("Member removed successfully.");
    }

    static void AddMember()
    {
        Console.Write("Enter member name: ");
        var name = Console.ReadLine();
        Console.Write("Enter member email: ");
        var email = Console.ReadLine();

        var member = new Member
        {
            Id = new Random().Next(1, 1000),
            Name = name,
            Email = email
        };

        memberService.AddMember(member);
        Console.WriteLine("Member added successfully.");
    }
}