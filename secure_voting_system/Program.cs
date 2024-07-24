// https://imansadati.com
// Thank you for attention ...
// -------------------------------------
// username: admin, password: admin. to login as admin.

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public bool IsAdmin { get; set; }
}

public class Candidate
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Votes { get; set; }
}

public class Vote
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CandidateId { get; set; }
    public DateTime Timestamp { get; set; }
}

public class VoteResult
{
    public string CandidateName { get; set; }
    public int VoteCount { get; set; }
}

public interface IAuthService
{
    User Authenticate(string username, string password);
    void RegisterRegularUser(User user);
    bool IsUserAdmin(User user);
}

public interface ICandidateService
{
    void AddCandidateByAdmin(Candidate candidate);
}

public interface IVoteService
{
    bool Vote(int userId, int candidateId);
}

public interface IResultsService
{
    List<VoteResult> GetVoteResults();
}

public static class FileStorage
{
    private static readonly string UsersFilePath = "users.json";
    private static readonly string VotesFilePath = "votes.json";
    private static readonly string CandidatesFilePath = "candidates.json";

    public static List<User> Users { get; private set; }
    public static List<Candidate> Candidates { get; private set; }
    public static List<Vote> Votes { get; private set; }

    static FileStorage()
    {
        Users = LoadFromFile<User>(UsersFilePath) ?? new List<User>
        {
            new User{Id=1,Username = "admin", PasswordHash= "adminhash", IsAdmin = true }
        };

        Candidates = LoadFromFile<Candidate>(CandidatesFilePath);

        Votes = LoadFromFile<Vote>(VotesFilePath) ?? new List<Vote>();
    }

    private static List<T> LoadFromFile<T>(string filePath)
    {
        if (File.Exists(filePath))
        {
            var jsonData = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<T>>(jsonData);
        }
        return null;
    }

    private static void SaveToFile<T>(string filePath, List<T> data)
    {
        var jsonData = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, jsonData);
    }

    public static void SaveUsers() => SaveToFile(UsersFilePath, Users);
    public static void SaveCandidates() => SaveToFile(CandidatesFilePath, Candidates);
    public static void SaveVotes() => SaveToFile(VotesFilePath, Votes);
}

public class AuthService : IAuthService
{
    public User Authenticate(string username, string password)
    {
        var passwordHash = HashPassword(password);
        return FileStorage.Users.SingleOrDefault(u => u.Username == username && u.PasswordHash == passwordHash);
    }

    public bool IsUserAdmin(User user)
    {
        return user.IsAdmin;
    }

    public static string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }

    public void RegisterRegularUser(User user)
    {
        FileStorage.Users.Add(user);
    }
}

public class CandidateService : ICandidateService
{
    public void AddCandidateByAdmin(Candidate candidate)
    {
        FileStorage.Candidates.Add(candidate);
    }
}

public class VoteService : IVoteService
{
    public bool Vote(int userId, int candidateId)
    {
        // Check if user has already voted
        if (FileStorage.Votes.Any(v => v.UserId == userId))
        {
            return false;
        }

        // Register the vote
        FileStorage.Votes.Add(new Vote
        {
            Id = FileStorage.Votes.Count + 1,
            UserId = userId,
            CandidateId = candidateId,
            Timestamp = DateTime.Now
        });

        // Update candidate votes
        var candidate = FileStorage.Candidates.SingleOrDefault(c => c.Id == candidateId);
        if (candidate != null)
        {
            candidate.Votes++;
        }

        FileStorage.SaveVotes();
        FileStorage.SaveCandidates();
        FileStorage.SaveUsers();

        return true;
    }
}

public class ResultsService : IResultsService
{
    public List<VoteResult> GetVoteResults()
    {
        return FileStorage.Candidates
            .Select(c => new VoteResult
            {
                CandidateName = c.Name,
                VoteCount = c.Votes
            })
            .ToList();
    }
}

// For more security we add controller for services.
// This implement strategy allowed us to users can't edit votes and other information by add limite on objects.

public class AuthController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    public User Login(string username, string password)
    {
        return _authService.Authenticate(username, password);
    }

    public void RegisterRegularUser(User user)
    {
        _authService.RegisterRegularUser(user);
    }

    public bool IsAdmin(User user)
    {
        return _authService.IsUserAdmin(user);
    }
}

public class CandidateController
{
    private readonly ICandidateService _candidateService;

    public CandidateController(ICandidateService candidateService)
    {
        _candidateService = candidateService;
    }

    public void AddCandidateByAdmin(Candidate candidate)
    {
        _candidateService.AddCandidateByAdmin(candidate);
    }
}

public class VoteController
{
    private readonly IVoteService _voteService;

    public VoteController(IVoteService voteService)
    {
        _voteService = voteService;
    }

    public bool Vote(int userId, int candidateId)
    {
        return _voteService.Vote(userId, candidateId);
    }
}

public class ResultsController
{
    private readonly IResultsService _resultsService;

    public ResultsController(IResultsService resultsService)
    {
        _resultsService = resultsService;
    }

    public List<VoteResult> GetResults()
    {
        return _resultsService.GetVoteResults();
    }
}

class Program
{
    static void Main(string[] args)
    {
        ShowMainMenu();
    }

    private static void ShowMainMenu()
    {
        while (true)
        {
            Console.WriteLine("\n** Voting Secure Online System **");
            Console.WriteLine("\n1. Login: ");
            Console.WriteLine("2. Register as regular: ");
            Console.WriteLine("3. Exit");
            Console.Write("Enter your choice: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Login();
                    break;
                case "2":
                    RegisterRegularUser();
                    break;
                case "3":
                    return;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }
    public static void Login()
    {
        IAuthService authService = new AuthService();
        IVoteService voteService = new VoteService();

        var authController = new AuthController(authService);
        var voteController = new VoteController(voteService);

        Console.Write("Enter username: ");
        var username = Console.ReadLine();
        Console.Write("Enter password: ");
        var password = Console.ReadLine();

        var user = authController.Login(username, password);

        if (user != null)
        {
            Console.WriteLine("------------------------------");
            Console.WriteLine("Login successful!");

            if (authController.IsAdmin(user))
            {
                Console.WriteLine("You are logged in as Admin.");
                Console.WriteLine("\n1. Display results");
                Console.WriteLine("2. Add candidate");
                Console.Write("Enter your choice: ");
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        DisplayResults();
                        break;
                    case "2":
                        AddCandidateByAdmin();
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("You are logged in as a Regular User.");
                DisplayCandidates();

                // Cast a vote
                Console.Write("Enter candidate ID to vote for: ");
                if (int.TryParse(Console.ReadLine(), out int candidateId))
                {
                    if (voteController.Vote(user.Id, candidateId))
                    {
                        Console.WriteLine("Vote cast successfully!");
                        DisplayResults();
                    }
                    else
                    {
                        Console.WriteLine("You have already voted or the vote failed.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid candidate ID.");
                }
            }
        }
        else
        {
            Console.WriteLine("Login failed. Please check your username and password.");
        }
    }

    public static void DisplayCandidates()
    {
        // Display candidates
        Console.WriteLine("------------------------------");
        Console.WriteLine("Available candidates:");
        foreach (var candidate in FileStorage.Candidates)
        {
            Console.WriteLine($"{candidate.Id}: {candidate.Name}");
        }
    }

    public static void DisplayResults()
    {
        IResultsService resultsService = new ResultsService();
        var resultsController = new ResultsController(resultsService);

        Console.WriteLine("------------------------------");
        Console.WriteLine("Vote Results:");
        var results = resultsController.GetResults();
        foreach (var result in results)
        {
            Console.WriteLine($"{result.CandidateName}: {result.VoteCount} votes");
        }
    }

    public static void AddCandidateByAdmin()
    {
        ICandidateService candidateService = new CandidateService();
        var candidateController = new CandidateController(candidateService);

        Console.WriteLine("Enter candidate name: ");
        var name = Console.ReadLine();

        var existingCandidate = FileStorage.Candidates.FirstOrDefault(c => c.Name == name);

        if (existingCandidate == null)
        {
            var candidate = new Candidate
            {
                Id = new Random().Next(1, 1000),
                Name = name,
                Votes = 0
            };
            candidateController.AddCandidateByAdmin(candidate);

            Console.WriteLine("Candidate added successful.");
            DisplayCandidates();
            FileStorage.SaveCandidates();
        }
        else
        {
            Console.WriteLine("This candidate already exist.");
        }
    }

    public static void RegisterRegularUser()
    {
        IAuthService authService = new AuthService();
        var authController = new AuthController(authService);

        Console.Write("Enter username: ");
        var username = Console.ReadLine();
        Console.Write("Enter password: ");
        var password = Console.ReadLine();

        var existingUser = FileStorage.Users.FirstOrDefault(u => u.Username == username);
        if (existingUser == null)
        {
            var passwordHash = AuthService.HashPassword(password);
            var user = new User
            {
                Id = new Random().Next(1, 1000),
                Username = username,
                PasswordHash = passwordHash,
                IsAdmin = false
            };
            authController.RegisterRegularUser(user);
            FileStorage.SaveUsers();
            Console.WriteLine("Registration successful. You can now login.");
        }
        else
        {
            Console.WriteLine("This user is username is already exist.");
        }
    }
}