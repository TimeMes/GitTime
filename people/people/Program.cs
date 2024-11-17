using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

MongoClient client = new MongoClient("mongodb://localhost:27017");
var database = client.GetDatabase("bigproject");
IMongoCollection<People> peopleCollection = database.GetCollection<People>("people");

StartPhrase();
Reader.backToMenu += () => Switcher();
Switcher();

void StartPhrase()
{
    Console.WriteLine("Write one of the list");
    Console.WriteLine("New people - new \nAll People List - all \nSearch People - search ");
}

void Switcher()
{
    switch (Reader.ReadLine<string>().ToLower())
    {
        case "search":
            Search();
            Console.WriteLine("Backed to Menu");
            break;
        case "search+":
            SearchPlus();
            Console.WriteLine("Backed to Menu");
            break;
        case "new":
            NewPeople();
            Console.WriteLine("Backed to Menu");
            break;
        case "delete":
            PeopleListDelete(new BsonDocument());
            Console.WriteLine("Backed to Menu");
            break;
        case "all":
            AllPeople();
            Console.WriteLine("Backed to Menu");
            break;
        case "clear" or "":
            Console.Clear();
            StartPhrase();
            break;
        default:
            Console.Clear();
            Console.WriteLine("Its not correct try again");
            StartPhrase();
            break;
    }
    Switcher();
}

void SearchPlus()
{
    BsonDocument bsons = new BsonDocument();
    // bsons = ReaderPlus.CompareInt();
    //var filter = new BsonDocument("Age", bsons);
    var filt = Builders<People>.Filter.StringIn(human => human.Age.ToString());
    Console.WriteLine(filt.ToBsonDocument());
    PeopleListWrite(filt.ToBsonDocument());
}
void AllPeople()
{
    PeopleListWrite(new BsonDocument());
}

async void NewPeople()
{
    People human = new People();
    Console.Write("Name ");
    human.Name = Reader.ReadLine<string>(str => str != "");
    Console.Write("Age ");
    human.Age = Reader.ReadLine<int>(age => age > 0 && age < 100);
    Console.Write("Balance ");
    human.Balance = Reader.ReadLine<int>();
    Console.WriteLine($"Save: {human.Name} {human.Age} years old with {human.Balance}$");
    await peopleCollection.InsertOneAsync(human);
}

void Search()
{
    Console.WriteLine("Search by Name/Age/Balance");
    string[] keys = { "Name", "Age", "Balance" };
    string key = Reader.ReadLine<string>(line => keys.Any(key => key == line));
    Console.Write($"Searching by {key}:");
    BsonValue value = 0;
    value = key switch
    {
        "Name" => Reader.ReadLine<string>(),
        "Age" or "Balance" => Reader.ReadLine<int>(i => i > 0),
        _ => value
    };
    var filter = new BsonDocument(key, value);
    PeopleListWrite(filter);
}

void PeopleListWrite(BsonDocument filter)
{
    var people = PeopleList(filter).Result;
    if (people.Count == 0)
    {
        Console.WriteLine("Sorry, searching end without result");
        return;
    }
    foreach (People p in people)
    {
        Console.WriteLine($"{p.Name} is {p.Age} years old and has {p.Balance}$");
    }

    Console.WriteLine("Press Enter for Continue || E for edit || D for delete");
    switch (Reader.ReadLine<string>().ToLower())
    {
        case "e":
            PeopleListEdit(filter);
            break;
        case "d":
            PeopleListDelete(filter);
            break;
    }
}

void PeopleListEdit(BsonDocument filter)
{
    var people = PeopleList(filter).Result;
    foreach (People p in people)
    {
        Console.WriteLine($"Edit {p.Name} {p.Age} years old {p.Balance}$");
        Console.WriteLine("Y for yes || X for end editing || Enter for skip");
        switch (Reader.ReadLine<string>().ToLower())
        {
            case "y":
                Console.WriteLine("Edit Name||Age||Balance");
                BsonDocument BsonHuman = p.ToBsonDocument();
                switch (Reader.ReadLine<string>().ToLower())
                {
                    case "name":
                        Console.Write("New name for " + p.Name + ": ");
                        p.Name = Reader.ReadLine<string>();
                        break;
                    case "age":
                        Console.Write("New age for " + p.Name + ": ");
                        p.Age = Reader.ReadLine<int>();
                        break;
                    case "balance":
                        Console.Write("New balance for " + p.Name + ": ");
                        p.Balance = Reader.ReadLine<int>();
                        break;
                }
                peopleCollection.ReplaceOne(BsonHuman, p);
                break;
            case "x":
                return;
            case "":
                Console.WriteLine("Skiped");
                break;
            default:
                Console.WriteLine("Unknown option.. skiped");
                break;
        }
    }
}
void PeopleListDelete(BsonDocument filter)
{
    var people = PeopleList(filter).Result;
    foreach (People p in people)
    {
        Console.WriteLine($"Delete {p.Name} {p.Age} years old {p.Balance}$");
        Console.WriteLine("Y for yes || X for end deleting || Enter for skip");
        switch (Reader.ReadLine<string>().ToLower())
        {
            case "y":
                peopleCollection.DeleteOne(p.ToBsonDocument());
                break;
            case "x":
                return;
            case "":
                Console.WriteLine("Skiped");
                break;
            default:
                Console.WriteLine("Unknown option.. skiped");
                break;
        }
    }
}
async Task<List<People>> PeopleList(BsonDocument filter)
{
    var cursor = await peopleCollection.FindAsync(filter);
    Task<List<People>> peopleTask = Task.Run(() => cursor.ToList());
    return peopleTask.Result;
}

[BsonIgnoreExtraElements]
class People
{
    public string Name = "Unknown";
    public int Age = 0;
    public int Balance = 0;
}

