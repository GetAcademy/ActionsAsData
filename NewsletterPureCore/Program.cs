using System.Text.Json;

var service = new NewsletterService();

Console.Write("E-post: ");
string? email = Console.ReadLine();

// INPUT / IO
var subscriptions = Infrastructure.LoadSubscriptions();
Subscription? existing = subscriptions.FirstOrDefault(s => s.Email == email);

var result = NewsletterService.Subscribe(email, existing);

Console.WriteLine(result);


// =====================
// Application/service
// =====================

public class NewsletterService
{
    private const string _databaseFile = "subscriptions.json";
    private const string _emailOutboxFolder = "email-outbox";

    public static string Subscribe(string emailInput, Subscription? existing)
    {
        var emailResult = EmailAddress.Create(emailInput);
        if (!emailResult.IsSuccess) return emailResult.Error!;
        var email = emailResult.Value!;

        if (existing is null)
        {
            var subscription = Subscription.CreateUnconfirmed(email);
            subscriptions.Add(subscription);

            var emailToSend = EmailToSend.CreateConfirmation(email);

            // OUTPUT / IO
            SaveSubscriptions(subscriptions);
            SendEmail(emailToSend);

            return "Subscription created.";
        }

        if (!existing.IsConfirmed)
        {
            var emailToSend = EmailToSend.CreateConfirmation(email);

            // OUTPUT / IO
            SendEmail(emailToSend);

            return "Confirmation email resent.";
        }

        return "Email address is already confirmed.";
    }
}

class Infrastructure
{
    public static List<Subscription> LoadSubscriptions()
    {
        if (!File.Exists(_databaseFile))
        {
            return new List<Subscription>();
        }

        var json = File.ReadAllText(_databaseFile);

        return JsonSerializer.Deserialize<List<Subscription>>(json)
            ?? new List<Subscription>();
    }

    private static void SaveSubscriptions(List<Subscription> subscriptions)
    {
        var json = JsonSerializer.Serialize(
            subscriptions,
            new JsonSerializerOptions { WriteIndented = true });

        File.WriteAllText(_databaseFile, json);
    }

    private static void SendEmail(EmailToSend email)
    {
        Directory.CreateDirectory(_emailOutboxFolder);

        var fileName = $"{DateTime.UtcNow:yyyyMMdd-HHmmssfff}-{Guid.NewGuid()}.json";
        var path = Path.Combine(_emailOutboxFolder, fileName);

        var json = JsonSerializer.Serialize(
            email,
            new JsonSerializerOptions { WriteIndented = true });

        File.WriteAllText(path, json);
    }
}


// =====================
// Domain-ish models
// =====================

public record EmailAddress
{
    public string Value { get; }

    private EmailAddress(string value)
    {
        Value = value;
    }

    public static Result<EmailAddress> Create(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Result<EmailAddress>.Fail("Email is required.");
        }

        var normalized = input.Trim().ToLower();

        if (!normalized.Contains("@"))
        {
            return Result<EmailAddress>.Fail("Invalid email address.");
        }

        return Result<EmailAddress>.Ok(new EmailAddress(normalized));
    }
}

public class Subscription
{
    public Guid Id { get; set; }
    public string Email { get; set; } = "";
    public bool IsConfirmed { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public static Subscription CreateUnconfirmed(EmailAddress email)
    {
        return new Subscription
        {
            Id = Guid.NewGuid(),
            Email = email.Value,
            IsConfirmed = false,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}

public record EmailToSend(
    string To,
    string Subject,
    string Body,
    DateTime CreatedAtUtc)
{
    public static EmailToSend CreateConfirmation(EmailAddress email)
    {
        return new EmailToSend(
            To: email.Value,
            Subject: "Bekreft abonnement",
            Body: "Klikk her for å bekrefte abonnementet.",
            CreatedAtUtc: DateTime.UtcNow);
    }
}


// =====================
// Result<T>
// =====================

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Ok(T value)
    {
        return new Result<T>(true, value, null);
    }

    public static Result<T> Fail(string error)
    {
        return new Result<T>(false, default, error);
    }
}